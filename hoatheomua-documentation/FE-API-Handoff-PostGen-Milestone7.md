# Bàn giao FE — PostGen mốc 7: payload nội dung và ảnh đã chốt

Cập nhật 2026-09-14. Đã triển khai trên nhánh `feature/post-gen`, chưa deploy. Không cần migration.

## Endpoint

`GET /api/v1/posts/{basePostId}/platforms/{platform}/official`

JWT Staff/Admin, bài dùng chung. Platform là facebook/instagram/zalo (không phân biệt hoa thường). Không có request body, không dùng HeavyPolicy, không gọi AI/Config/publish và không ghi database. Bài hoặc nền tảng chưa có/đã xóa trả 404. Không tự tạo PlatformPost: tạo qua generate trước.

## Response

HTTP 200 với `ApiResponse<OfficialVersion>`. Ví dụ phần value:

```json
{
  "basePostId": "11111111-1111-4111-8111-111111111111",
  "platformPostId": "22222222-2222-4222-8222-222222222222",
  "platform": "facebook",
  "status": "Ready",
  "isReady": true,
  "notReadyReasons": [],
  "generationId": "33333333-3333-4333-8333-333333333333",
  "title": "Bó hoa gửi lời cảm ơn",
  "content": "Nội dung đã chốt.\n\nĐoạn tiếp theo.",
  "hashtag": "#HoaTheoMua #CamOn",
  "selectedAt": "2026-09-14T10:00:00Z",
  "images": [
    {
      "sourceBasePostImageId": "44444444-4444-4444-8444-444444444444",
      "ratioCode": "Square1x1",
      "selectedGeneratedPostImageId": "55555555-5555-4555-8555-555555555555",
      "imageUrl": "https://example.com/official.png",
      "pixelWidth": 1080,
      "pixelHeight": 1080,
      "sortOrder": 1
    }
  ]
}
```

- generationId/title/content/hashtag lấy đúng version đang được OfficialGenerationId trỏ tới, kể cả Manual hoặc bản cũ. Không fallback sang bài gốc hoặc version mới nhất. Các trường null khi chưa có version chốt hợp lệ.
- images chỉ gồm các ảnh được chọn, Succeeded, có URL, đúng bài và đúng cặp ảnh nguồn × tỷ lệ của ô; tăng dần theo sortOrder. Không gồm ô chưa chốt hoặc ảnh Failed. Shape mỗi ảnh giống detail.platforms[].images.
- Ảnh gốc đã soft delete không xóa ảnh kết quả đang chốt: snapshot ảnh Succeeded đã chọn vẫn được trả, giống GET detail. Muốn bỏ ảnh khỏi payload thì PUT tập ảnh nền tảng ở mốc 6.
- selectedAt là thời điểm chốt gần nhất lưu trên PlatformPost; có thể null với dữ liệu chưa chốt. GET không cập nhật nó.
- Không có versions/baseImages/prompt/metadata AI trong payload này. title/content/hashtag giữ riêng, không tự nối thành một chuỗi đăng bài.

## Sẵn sàng hay chưa

`isReady` được tính từ dữ liệu trả về: có content chốt không rỗng và ít nhất một ảnh chốt hợp lệ; nền tảng không Generating và không ở trạng thái publish. Các ô chưa chốt khác không chặn readiness nếu đã có ít nhất một ảnh hợp lệ, theo tiêu chí của kế hoạch hiện tại.

| Trường hợp | status trả về | isReady | notReadyReasons |
| --- | --- | --- | --- |
| Có content và ít nhất một ảnh | Ready | true | [] |
| Có content, chưa có ảnh hợp lệ | Generated | false | ["MissingImages"] |
| Có ảnh, chưa có content | Draft | false | ["MissingContent"] |
| Chưa có cả hai | Draft | false | ["MissingContent", "MissingImages"] |
| Đang Generating | Generating | false | Thêm "GenerationInProgress" |
| Scheduled/Publishing/Published/PublishFailed | Giữ tên trạng thái đó | false | Thêm "PublishingState" |

Thiếu dữ liệu vẫn trả 200 để FE hiển thị phần đã có. notReadyReasons là mã ổn định để FE dịch nhãn; nếu nhiều điều kiện cùng tồn tại, trả đủ lý do theo thứ tự MissingContent, MissingImages, GenerationInProgress hoặc PublishingState.

**status của API này mô tả payload tại lúc đọc**, không phải cập nhật trạng thái xử lý trong database. Ví dụ generate trước đó để trạng thái Generated nhưng đã tự chốt đủ text/ảnh thì official trả Ready. Lượt AI sau đó Failed vẫn có thể còn đủ bản chốt cũ dùng được: official trả Ready, còn detail tiếp tục thể hiện lỗi xử lý. Ngược lại, cờ Ready cũ không làm payload thiếu dữ liệu trở thành sẵn sàng.

isReady chưa kiểm quyền OAuth, giới hạn xuất bản thực tế từng mạng xã hội hoặc khả năng tải URL. Việc đăng thật/lập lịch thuộc giai đoạn publishing riêng. Backend vẫn lưu target pixel, chưa đo kích thước file ảnh.

## Cách FE dùng

Mở màn hình xem bản chốt hoặc chuẩn bị dữ liệu đăng → gọi official. Hiển thị content và images đúng thứ tự. Nếu isReady=false, hiển thị các lý do thiếu dữ liệu; người dùng có thể chọn/sửa nội dung qua API mốc 5 hoặc chọn ảnh qua API mốc 6, rồi tải lại official. GET official không làm phát sinh nội dung/ảnh mới.

Các truy vấn sử dụng một snapshot Repeatable Read của PostgreSQL, nên việc người khác sửa Manual hoặc đổi ảnh chốt giữa lúc đọc không làm trộn hai lần chốt trong cùng response. Response là snapshot tại lúc request; không phải bản khóa để đăng về sau, cũng không phải API đăng bài.

## Lỗi

| HTTP | Trường hợp |
| --- | --- |
| 401 | Chưa đăng nhập |
| 403 | Không thuộc Staff/Admin |
| 404 | Post not found hoặc Platform post not found |
| 422 | Platform không phải facebook/instagram/zalo, gồm chuỗi số |

Contract liên quan: [Post Core](FE-API-Handoff-PostCore.md), [mốc 5](FE-API-Handoff-PostGen-Milestone5.md), [mốc 6](FE-API-Handoff-PostGen-Milestone6.md).
