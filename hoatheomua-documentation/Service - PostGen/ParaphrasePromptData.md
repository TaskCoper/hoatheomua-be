# Dữ liệu paraphrase content — Facebook, Instagram, Zalo

Phiên bản: 2.0 · Ngày soạn: 14/09/2026 · Ngôn ngữ: tiếng Việt.

## 1. Cấu trúc và phạm vi sử dụng

Bộ dữ liệu dành cho Hoa Theo Mùa, chuyển một bài content base thành ba bản viết phù hợp Facebook, Instagram và Zalo. Giữ nguyên thông tin sản phẩm, thay đổi cách mở bài, cách diễn đạt, bố cục và lời kêu gọi hành động theo nền tảng.

Thiết kế mới dùng **3 Config prompt độc lập**, mỗi bản chứa `isActive` và `content`, tất cả quản trị qua API Config với quyền Admin. Mỗi prompt đã đầy đủ vai trò, nguyên tắc, định dạng đầu ra và hướng dẫn riêng của một nền tảng. Khi gọi AI, đọc nguyên `value.content` của đúng nền tảng; không có Config platform standard riêng. Seed ba prompt đã triển khai trong migration `20260914065606_AddPostGenSystemPromptConfigs`; migration `20260914073414_ConsolidatePostGenPlatformConfigs` bổ sung isActive và xóa key standard cũ; luồng generate nội dung đã gọi proxy chat completions theo prompt riêng. Migration chỉ thêm key chưa có bản active, giữ nguyên dữ liệu Admin đã cấu hình.

| Dữ liệu | Nơi lưu | Vai trò |
| --- | --- | --- |
| System prompt Facebook | `Configs`, key `postgen.system-prompt.facebook`, `value.content` | Prompt hoàn chỉnh, chỉnh sửa độc lập |
| System prompt Instagram | `Configs`, key `postgen.system-prompt.instagram`, `value.content` | Prompt hoàn chỉnh, chỉnh sửa độc lập |
| System prompt Zalo | `Configs`, key `postgen.system-prompt.zalo`, `value.content` | Prompt hoàn chỉnh, chỉnh sửa độc lập |

Giả định biên tập: thương hiệu hoa tươi, giọng ấm áp, tinh tế, chân thành. Facebook và Instagram áp dụng cho bài feed/caption; Zalo áp dụng cho nội dung bài viết OA. Không dùng bộ này làm mẫu tin giao dịch, quảng cáo trả phí hoặc kịch bản video.

Các con số dưới đây là **ngưỡng nội bộ đề xuất cho bộ dữ liệu**, không phải tuyên bố giới hạn tối đa chính thức của các nền tảng. Bộ này tập trung vào text; số ảnh và tỷ lệ ảnh cần cấu hình riêng theo hình thức đăng.

| Quy ước | Facebook | Instagram | Zalo OA |
| --- | --- | --- | --- |
| Độ dài mục tiêu của phần `content` | 450–1.000 ký tự | 250–700 ký tự | 300–800 ký tự |
| Ngân sách tối đa toàn bài theo quy ước mục 4 | 1.800 ký tự | 1.800 ký tự | 1.200 ký tự |
| Độ dài tối đa `title` | 90 ký tự | 80 ký tự | 100 ký tự |
| Hashtag | 0–3 | 0–5, ưu tiên 3–5 khi có đủ chủ đề phù hợp | 0 |
| Emoji toàn bài | 0–3 | 0–4 | 0–1 |
| CTA | Một hành động chính nếu phù hợp bài gốc | Một hành động chính nếu phù hợp bài gốc | Một hành động chính, hướng dẫn rõ ràng |

Không kéo dài bài chỉ để đạt khoảng mục tiêu. Khi bài gốc có nhiều điều kiện quan trọng, được viết dài hơn khoảng mục tiêu nhưng vẫn phải nằm trong ngân sách tối đa.

## 2. Ba system prompt độc lập

Mỗi file dưới đây là **một body POST `/api/v1/configs` hoàn chỉnh**, gồm `group`, `key`, `kind` và `value.content`. Nội dung prompt được lưu đầy đủ trong từng file; không dùng biến tham chiếu tới prompt chung hay phần văn bản nằm ở Config khác.

| Nền tảng | Payload hoàn chỉnh |
| --- | --- |
| Facebook | [postgen.system-prompt.facebook.json](Data/postgen.system-prompt.facebook.json) |
| Instagram | [postgen.system-prompt.instagram.json](Data/postgen.system-prompt.instagram.json) |
| Zalo | [postgen.system-prompt.zalo.json](Data/postgen.system-prompt.zalo.json) |

Các quy tắc bảo toàn thông tin và đầu ra JSON được sao chép đầy đủ vào từng prompt khi soạn bộ dữ liệu. Sau khi lưu, ba bản ghi độc lập: sửa Facebook không đổi prompt Instagram/Zalo. Không có cơ chế kế thừa hoặc ghép prompt lúc chạy.

## 3. Cách sử dụng trong luồng paraphrase

1. Lấy một content base có `title`, `content`, `hashtag` và đúng platform đích.
2. Đọc duy nhất Config `postgen.system-prompt.{platform}`; chỉ chạy khi `value.isActive = true` và `value.content` không rỗng.
3. Dùng nguyên nội dung system prompt theo mẫu dưới đây; thiếu prompt thì báo lỗi cho nền tảng đó, không dùng prompt chung hoặc prompt của nền tảng khác. Truyền source, previousContent và feedback trong dữ liệu đầu vào riêng, không nối content base vào phần system.
4. Gọi AI riêng cho từng nền tảng; mỗi lần nhận một object gồm `title`, `content`, `hashtag`. Nếu có adapter AI Service, adapter bổ sung metadata như `requestId`, `model`, `usage` theo contract tích hợp.
5. Backend kiểm JSON đúng schema, content không rỗng, title/hashtag phù hợp độ dài cột trước khi lưu. Quy tắc biên tập và giới hạn nền tảng nằm trong prompt; hiện không có bộ kiểm tự động về tính đúng sự thật hoặc giới hạn xuất bản. Object có ba chuỗi rỗng bị từ chối.

```text
{Config(postgen.system-prompt.{platform}).Value.content}
```

Ngân sách độ dài được viết trực tiếp trong từng prompt. Không lưu thêm maxContentLength/maxHashtagCount trong Config thứ hai. Kiểm chứng giới hạn chính thức và cách tính độ dài khi xuất bản thuộc phase đăng bài.

Giữ riêng ba trường ở tầng dữ liệu. Việc ghép thành caption Facebook/Instagram hoặc ánh xạ title/body của bài Zalo OA thuộc tầng xuất bản theo loại nội dung và API được chọn.

### Ghi dữ liệu vào các API hiện có

- System prompt chưa có: POST `/api/v1/configs` với một trong ba payload ở mục 2. POST mặc định `IsPublic = false`.
- System prompt đã có: GET `/api/v1/configs/settings?groupName=postgen&key=postgen.system-prompt.{platform}`, lấy bản ghi khớp chính xác rồi PUT `/api/v1/configs/{id}` với body tương ứng, thêm `"isPublic": false`. Không dùng API `/system-prompts` cho PostGen mới.

Luồng generate nội dung đã triển khai; sinh ảnh và xuất bản chưa triển khai. Xem [cấu hình hiện tại](PlatformConfig.md) và [contract AI proxy](AiServiceContract.md). PUT cần giữ cả content và isActive.

## 4. Ví dụ một content base → ba bản paraphrase

Dữ liệu sau **hoàn toàn minh họa**, không dùng làm giá, tồn kho hoặc cam kết giao hàng thực tế của Hoa Theo Mùa. Các khoảng độ dài mục tiêu không bắt buộc với content base ngắn này.

### Content base

```json
{
  "title": "Bó hoa Cảm Ơn dành tặng người bạn trân quý",
  "content": "Hoa Theo Mùa giới thiệu bó hoa Cảm Ơn gồm 10 cành hồng màu hồng, giá 450.000đ/bó. Bó hoa phù hợp để gửi lời cảm ơn đến người bạn trân quý. Sản phẩm nhận đặt trước 1 ngày, giao tại TP.HCM. Giá chưa bao gồm phí giao hàng. Nhắn tin cho Hoa Theo Mùa để được tư vấn và đặt hoa.",
  "hashtag": "#HoaTheoMua #HoaCamOn #HoaHong"
}
```

### Kết quả Facebook

```json
{
  "title": "Gửi lời cảm ơn đến người bạn trân quý 🌷",
  "content": "Một bó hoa có thể thay bạn gửi lời cảm ơn chân thành. Bó Cảm Ơn của Hoa Theo Mùa gồm 10 cành hồng màu hồng, dành cho người bạn luôn trân quý.\n\nGiá 450.000đ/bó, chưa bao gồm phí giao hàng. Sản phẩm nhận đặt trước 1 ngày, giao tại TP.HCM.\n\nNhắn tin cho Hoa Theo Mùa để được tư vấn và đặt hoa.",
  "hashtag": "#HoaTheoMua #HoaCamOn #HoaHong"
}
```

### Kết quả Instagram

```json
{
  "title": "Một lời cảm ơn, gửi bằng hoa 🌷",
  "content": "10 cành hồng màu hồng trong bó Cảm Ơn của Hoa Theo Mùa — gửi chút trân quý đến người bạn muốn cảm ơn.\n\n450.000đ/bó, chưa bao gồm phí giao hàng. Đặt trước 1 ngày, giao tại TP.HCM.\n\nNhắn tin cho Hoa Theo Mùa để được tư vấn và đặt hoa.",
  "hashtag": "#HoaTheoMua #HoaCamOn #HoaHong"
}
```

### Kết quả Zalo

```json
{
  "title": "Bó hoa Cảm Ơn: 10 cành hồng màu hồng, giá 450.000đ/bó",
  "content": "Hoa Theo Mùa giới thiệu bó hoa Cảm Ơn, một lựa chọn để gửi lời cảm ơn đến người bạn trân quý.\n\n- Thành phần: 10 cành hồng màu hồng.\n- Giá: 450.000đ/bó, chưa bao gồm phí giao hàng.\n- Thời gian đặt: trước 1 ngày.\n- Khu vực giao: TP.HCM.\n\nNhắn tin cho Hoa Theo Mùa để được tư vấn và đặt hoa.",
  "hashtag": ""
}
```

## 5. Cơ sở tham khảo và lựa chọn biên tập

- Facebook: Meta nêu việc hạn chế nội dung spam có caption dài gây phân tán, hashtag quá nhiều hoặc caption không liên quan. Bộ dữ liệu chọn nội dung tập trung và tối đa 3 hashtag như một quy ước nội bộ; nguồn không quy định con số 3 hay ngân sách 1.800 ký tự. [Meta — Cracking Down on Spammy Content on Facebook](https://about.fb.com/news/2025/04/cracking-down-spammy-content-facebook/).
- Instagram: bộ này chủ động đặt ngưỡng nội bộ 5 hashtag và 1.800 ký tự. Tài liệu vận hành của Buffer hiện cũng giới hạn việc xuất bản Instagram qua Buffer ở 5 hashtag; đây là nguồn về công cụ Buffer, không thay thế contract API Meta hay bảo đảm mọi tài khoản có cùng hành vi. [Buffer — How hashtags work in Buffer](https://support.buffer.com/en-us/articles/how-hashtags-work-in-buffer-kusRDsVgD8).
- Zalo: tài liệu chính thức mô tả bài viết/video OA là hình thức cung cấp nội dung cho khách hàng. Bộ này chọn phạm vi bài viết OA, giọng thông tin rõ ràng và không dùng hashtag theo quyết định biên tập nội bộ; không suy ra giới hạn kỹ thuật của tin nhắn từ bài viết OA. [Zalo OA — Tính năng Nội dung](https://oa.zalo.me/home/function/interaction?type=noi-dung).

Các lựa chọn về giọng điệu, CTA, khoảng độ dài, số emoji và hashtag trong gói này là cấu hình khởi đầu để đội nội dung điều chỉnh sau khi đánh giá bài sinh thực tế; không phải cam kết về lượt tiếp cận hay chuyển đổi.
