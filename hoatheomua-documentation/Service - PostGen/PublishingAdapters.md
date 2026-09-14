# Mốc 13 — Facebook và Instagram

Triển khai local **2026-09-15**. Có adapter gọi Graph API thật, nối vào API/job/outbox/worker mốc 12. Kiểm chứng phát triển dùng HTTP Meta/media giả lập, PostgreSQL và RabbitMQ thật. **Chưa deploy, chưa chạy smoke đăng lên account thật; hai flag publishing mặc định false.** Zalo, lịch đăng, AI auth và kho ảnh vẫn thuộc các mốc đang hoãn/chưa làm.

## Contract đã hiện thực

| Kênh | Phạm vi | Chuẩn bị | Đăng |
| --- | --- | --- | --- |
| Facebook Page | 1–10 ảnh; cap 10 là giới hạn sản phẩm | Một ảnh: kiểm URL/file. Nhiều ảnh: upload từng ảnh `published=false, temporary=true`, lưu photo ID theo order | Một ảnh: `/photos` với `url,caption,published=true`, lấy **post_id**. Nhiều ảnh: `/feed` với `message,attached_media,published=true`, lấy **id** |
| Instagram professional qua Facebook Login | Một ảnh hoặc carousel 2–10 ảnh vuông | Tạo từng `/media`, lưu container ID, poll `status_code`; carousel có parent riêng sau khi children FINISHED | `/media_publish` với parent/single `creation_id`, lấy **media ID**; đọc permalink sau đó |

API version lấy từ `PlatformAccountOptions:Meta:GraphApiVersion` và lưu trong snapshot. Dùng Facebook Page token đã mã hóa từ mốc 11; không đổi sang Instagram Login hoặc lưu token trong snapshot. Trước từng bước, kiểm lại account/token/scopes/liên kết Page qua luồng đọc của OAuthProvider; Instagram đọc `content_publishing_limit` và dùng `config.quota_total` thực tế, không hardcode 50/100.

Formatter giữ `postgen-text-v1`: nối title/content/hashtag không rỗng bằng hai xuống dòng, giữ nguyên Unicode và khoảng trắng trong từng phần. Adapter kiểm text đúng formatter. Instagram tối đa 2.200 Unicode scalar, 30 hashtag và 20 mention trên caption cuối. Không cắt text/hashtag hoặc bỏ ảnh. Facebook chưa có giới hạn text được xác minh nên không tự gán một con số suy đoán.

## Kiểm media

- Preview chỉ kiểm snapshot local; `mediaVerification=Unverified`. `canPublish=true` nghĩa là nhận được job, chưa bảo đảm provider chấp nhận.
- Worker đọc URL đã chốt, kiểm HTTP, MIME, chữ ký file và giới hạn bytes; JPEG đọc thêm kích thước từ header. Đây không phải bộ giải mã ảnh hoàn chỉnh; provider vẫn có thể từ chối file hỏng phía sau header.
- Facebook: JPEG/PNG/GIF/BMP/TIFF, tối đa 10 MiB. Instagram: JPEG, tối đa 8 MiB, width 320–1440, ratio 0,8–1,91. MIME không suy ra từ đuôi URL hoặc metadata mục tiêu trong DB.
- Carousel Instagram đợt này chỉ nhận file vuông để tránh crop mặc định của provider. Ảnh portrait/landscape hợp lệ có thể đăng đơn; muốn carousel cần chọn ảnh vuông đã chốt. Không tự crop/chuyển JPEG/rehost hoặc gọi AI.
- Chỉ HTTP(S), cổng mặc định, không userinfo/fragment/loopback/private IP. Media HTTP client không mang token Meta, không dùng proxy/cookie, không theo redirect; DNS được kiểm và kết nối đến IP đã kiểm để chống rebinding. Redirect hoặc URL hết hạn báo lỗi để sửa URL nguồn.
- Mốc 10 vẫn hoãn: không lưu asset/hash bytes. File tại cùng URL có thể thay đổi giữa lần kiểm và lúc Meta tải; snapshot cố định URL/order, không cam kết bytes bất biến.

## Tiến độ, lỗi và retry

Checkpoint JSON mở rộng tương thích gồm `preparedAt`, media `{order,providerId,width,height,ready}` và `containerId/readyToPublish`. Không thêm migration. Mỗi bước tạo media có tối đa một POST, kết quả được lưu trước bước tiếp theo. Child/parent IN_PROGRESS poll sau 60 giây. Bộ đếm steps/attempts của mốc 12 giới hạn toàn bộ chuỗi xử lý.

Media tạm đã chuẩn bị quá 23 giờ bị từ chối **trước lệnh đăng**, reset checkpoint và cho retry cùng snapshot. Đây là ngưỡng vận hành bảo thủ trước TTL khoảng 24 giờ. Không reset theo TTL trong luồng đối soát sau publish intent.

| Tình huống | Kết quả / thao tác |
| --- | --- |
| `MEDIA_*_{order}`, `INSTAGRAM_JPEG_REQUIRED_{order}`, `INSTAGRAM_IMAGE_DIMENSIONS_INVALID_{order}`, `INSTAGRAM_CAROUSEL_SQUARE_REQUIRED_{order}`, `FACEBOOK_IMAGE_FORMAT_INVALID_{order}` | Số cuối là order của ảnh. Sửa/chọn lại ảnh khi job đã Failed, preview lại, tạo job mới. Không thay snapshot cũ |
| `META_TOKEN_INVALID`, `META_PERMISSION_DENIED` | Đánh dấu account `ReauthRequired` chỉ nếu revision vẫn là phiên worker đã dùng; không ghi đè reconnect mới. Kết nối lại và xem `canRetry` |
| `META_RATE_LIMITED` | Tôn trọng Retry-After (1–3600 giây; mặc định 60), quota đầy chờ 3600. Job khác cùng account cũng được hoãn. Retry quá sớm trả HTTP 429 |
| `INSTAGRAM_QUOTA_UNVERIFIED` | Response không đủ usage/limit để quyết định; sửa quyền/contract rồi retry nếu cho phép |
| `META_INVALID_MEDIA_OR_PARAMETER` | Meta từ chối tham số/media; không retry tự động |
| GET status gặp lỗi tạm | Giữ checkpoint, poll có backoff và giới hạn; không tạo lại container |
| POST chuẩn bị mất phản hồi | Failed, có thể retry thủ công vì chưa đăng. Media tạm mất ID có thể bị mồ côi đến khi provider thu hồi |
| POST đăng timeout, 5xx, response sai schema/thiếu ID hoặc lỗi không nhận diện | NeedsReview; không tự gửi lại |
| POST đăng nhận lỗi Meta 4xx rõ ràng về quyền/token/quota/tham số | Failed với bằng chứng bị từ chối; chỉ retry khi `canRetry=true` |
| Đăng trả media ID, đọc permalink sau đó lỗi | Vẫn Succeeded, URL có thể null; không đăng lại |

Attempt lưu mã đã lọc `HTTP-status:code:subcode`, fbtrace_id hợp lệ và mã lỗi nội bộ; không lưu raw response/message/token. Nhóm lỗi chưa nhận diện giữ kết quả không chắc chắn khi đã gửi lệnh đăng.

## Giới hạn đối soát

- Facebook: Admin đưa candidate post ID; adapter GET theo ID, kiểm đúng Page, nội dung, is_published và **đúng photo IDs/thứ tự đã lưu** mới xác nhận Succeeded. Một ảnh trả photo ID nhưng thiếu post_id vẫn giữ photo ID cho bước này. Timeout trước khi nhận photo ID có thể không đủ chứng cứ để xác nhận.
- Instagram: reference container không có phép ánh xạ chắc chắn từ container ID sang published media ID. FINISHED/ERROR/EXPIRED sau timeout không được coi là chưa đăng. PUBLISHED thiếu media ID trả `INSTAGRAM_PUBLISHED_ID_UNRESOLVED`. Candidate ID hoặc caption giống nhau chưa đủ; `ConfirmPublished` không tự ép Succeeded trong tình huống này.
- Cả hai kênh không kết luận `ConfirmNotPublished` thành công chỉ vì không thấy bài trên feed. Khi không đủ chứng cứ, tiếp tục NeedsReview hoặc Admin `Close` với ghi chú; Close giữ kết quả bên ngoài chưa rõ và chặn đăng lại cùng PlatformPost.

## Cấu hình và vận hành

Giữ cấu hình Meta/token mã hóa của mốc 11 và PostgreSQL/RabbitMQ của mốc 12. Đăng ký thực tế qua `AddPostGenPublishingAdapters` trong Program.cs, dùng typed HttpClient không có retry HTTP tự động, không log URL/header/body nhạy cảm.

| Runtime .NET | Docker/GitHub Environment Secret | Mặc định |
| --- | --- | --- |
| `PublishingOptions__FacebookEnabled` | `POSTGEN_PUBLISH_FACEBOOK_ENABLED` | false |
| `PublishingOptions__InstagramEnabled` | `POSTGEN_PUBLISH_INSTAGRAM_ENABLED` | false |

`POSTGEN_META_ENABLED=true` chỉ bật kết nối, không ngầm bật đăng. Hai compose, env sample và workflow đã có mapping flag mới. Chưa thay secret môi trường hoặc bật kênh đang chạy. Nếu tắt flag khi job đang xử lý, request đã gửi có thể vẫn hoàn tất; giữ lịch sử và đối soát, không coi tắt flag là thu hồi bài.

Trước khi phát hành một kênh: chỉ định account và bài thử được phép đăng, kiểm version/app permissions, dùng URL media thử phù hợp, bật flag trong môi trường thử và chạy một lượt qua API. Xác minh external ID/bài hiển thị thực tế. Smoke chưa chạy trong đợt triển khai này; không tự đăng thử hoặc gọi AI thật.

## Nguồn và kiểm chứng

Contract provider đối chiếu ngày 2026-09-15: [Page Photos](https://developers.facebook.com/docs/graph-api/reference/page/photos/), [Page Feed](https://developers.facebook.com/docs/graph-api/reference/page/feed/), [IG User Media](https://developers.facebook.com/docs/instagram-platform/instagram-graph-api/reference/ig-user/media/), [IG Container](https://developers.facebook.com/docs/instagram-platform/instagram-graph-api/reference/ig-container/), [Media Publish](https://developers.facebook.com/docs/instagram-platform/instagram-graph-api/reference/ig-user/media_publish/), [Meta error handling](https://developers.facebook.com/docs/graph-api/guides/error-handling/). Các điểm reference mâu thuẫn và giới hạn chưa smoke được giữ tại [nghiên cứu](PublishingAdaptersResearch.md).

Toàn suite **1.094 PASS / 0 fail / 0 skip**, gồm **79 test mới** mốc 13; build Release PASS. Test adapter/worker nằm trong `HoaTheoMua.Service.Test/PostPublishingTest/Adapters.cs` và `AdapterWorker.cs`. Probe HTTP chạy `PublicationVerification --adapters`: 49 request, 157 checks PASS, adapter production + fake HTTP Meta/media + PostgreSQL/RabbitMQ thật; không có AI hoặc live Meta. Xem [evidence](PublicationVerification/results/api-integration-tester.json) và [hướng dẫn chạy lại](PublicationVerification/README.md).
