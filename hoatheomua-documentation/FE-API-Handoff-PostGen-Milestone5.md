# Bàn giao FE — PostGen mốc 5: version nội dung

Cập nhật 2026-09-14. Đã triển khai trên nhánh `feature/post-gen`, chưa deploy. Bốn API dùng JWT Staff/Admin, bài viết dùng chung giữa tài khoản. Chỉ POST regenerate có heavy rate limit; GET lịch sử và PUT sửa/chốt không gọi AI. Không đổi schema hoặc API generate/mode của mốc 4.

## Danh sách API

Tiền tố: `/api/v1/posts/{basePostId}/platforms/{platform}/content`. Platform dùng `facebook`, `instagram`, `zalo`.

| Method | Đường dẫn | Hành vi |
| --- | --- | --- |
| POST | `{prefix}/regenerate` | Sinh lại từ bài gốc hoặc một version, tạo version Ai và tự chốt |
| PUT | `{prefix}` | Sửa tay toàn bộ title/content/hashtag, lưu Manual và tự chốt |
| GET | `{prefix}/versions` | Đọc riêng lịch sử nội dung của nền tảng |
| PUT | `{prefix}/official` | Chọn một version đã có, không tạo version hoặc gọi AI |

Các API yêu cầu PlatformPost đã tồn tại. Chưa có thì dùng `generate` trước; chúng không tự tạo nền tảng. Bài gốc hoặc nền tảng đã xóa không truy cập được.

## Sinh lại nội dung

```json
{
  "sourceGenerationId": "11111111-1111-4111-8111-111111111111",
  "feedback": "Viết ngắn gọn và gần gũi hơn"
}
```

`sourceGenerationId` là ID version nội dung của đúng nền tảng, kể cả Manual hoặc bản cũ. Nếu bỏ/null: AI làm lại từ bài gốc, **không tự lấy nội dung đang chốt**. Muốn sửa tiếp nội dung đang hiển thị, gửi `platforms[].content.generationId` vào đây. Guid.Empty không hợp lệ.

Feedback tùy chọn, tối đa 2000 ký tự; mỗi lần chỉ gửi feedback của lần đó, không cộng dồn lịch sử. Backend gửi bài gốc, previousContent từ version được chỉ định và prompt Config hiện tại. Version mới ghi parentGeneratedPostId, feedback và snapshot prompt đã gửi.

Chỉ sinh nội dung; không sinh hoặc đổi ảnh. Thành công tự chốt version mới và chuyển Generated (response có thể Partial nếu còn ô ảnh chưa chốt). Lỗi AI không thêm version, giữ official cũ, chuyển Failed với lastError. HTTP call đồng bộ, timeout AI mặc định 120 giây, tổng budget 180 giây và tối đa 10 giây để lưu kết quả; không tự retry POST.

## Sửa tay

```json
{
  "title": "Bó hoa gửi lời cảm ơn",
  "content": "Nội dung do người dùng chỉnh sửa.",
  "hashtag": "#HoaTheoMua #CamOn"
}
```

Content bắt buộc, không chỉ khoảng trắng, tối đa 20000 ký tự. Title tùy chọn tối đa 500; hashtag tùy chọn tối đa 1000. PUT thay toàn bộ ba trường; bỏ title/hashtag hoặc gửi null sẽ xóa giá trị đó.

Mỗi lần sửa tạo version Manual có parent là nội dung đang chốt và tự chọn bản mới. Ngoại lệ để tránh nhiều version do lưu liên tiếp: nếu bản đang chốt đồng thời là bản mới nhất, Source=Manual và chưa có version con tham chiếu thì cập nhật ngay dòng đó; giữ generationId, parent, createdAt và versionNo. Title/content/hashtag và UserId người sửa được cập nhật. Nếu đã chọn bản cũ, lần sửa tạo nhánh mới và không ghi đè bản Manual ở nhánh khác.

Version Ai hoặc bản Manual đã được phiên bản con tham chiếu không bị sửa tại chỗ. Manual không chứa prompt snapshot, model, request ID hoặc tham chiếu Config/SystemPrompt. Sửa tay không cần đọc Config hoặc gọi AI. Giữ nguyên ảnh/slot và trạng thái xử lý hiện tại (kể cả Ready hoặc Failed); đây không phải thao tác publish.

## Chọn nội dung chính thức

```json
{ "generationId": "11111111-1111-4111-8111-111111111111" }
```

ID bắt buộc, thuộc đúng nền tảng của bài này. Có thể chọn Ai, Manual hoặc bản cũ; giữ số version cũ. Chỉ đổi con trỏ official và thời điểm chốt/cập nhật; không thêm version hoặc đổi ảnh, không cần Config/AI. Trạng thái xử lý hiện tại giữ nguyên. Gọi lại cùng ID không thêm version.

## Response

Ba API POST/PUT trả HTTP 200 với `ApiResponse<PostCoreDetail>` như generate và GET detail. Mỗi nền tảng chỉ có object `content` chốt và `images` chốt; không có `value.images` hoặc `platforms[].versions`. FE cập nhật detail và tải lại lịch sử sau sửa/chốt/sinh lại nếu bảng lịch sử đang mở.

GET versions trả HTTP 200; dưới đây là **phần value** trong envelope:

```json
{
  "platformPostId": "22222222-2222-4222-8222-222222222222",
  "platform": "facebook",
  "officialGenerationId": "33333333-3333-4333-8333-333333333333",
  "versions": [
    {
      "generationId": "11111111-1111-4111-8111-111111111111",
      "parentGeneratedPostId": null,
      "versionNo": 1,
      "source": "Ai",
      "title": "Tiêu đề AI",
      "content": "Nội dung AI",
      "hashtag": "#HoaTheoMua",
      "createdAt": "2026-09-14T09:00:00Z",
      "feedback": null,
      "isOfficial": false
    },
    {
      "generationId": "33333333-3333-4333-8333-333333333333",
      "parentGeneratedPostId": "11111111-1111-4111-8111-111111111111",
      "versionNo": 2,
      "source": "Manual",
      "title": "Tiêu đề đã sửa",
      "content": "Nội dung đã sửa tay",
      "hashtag": null,
      "createdAt": "2026-09-14T09:01:00Z",
      "feedback": null,
      "isOfficial": true
    }
  ]
}
```

Lịch sử chưa phân trang, sắp từ cũ đến mới theo (createdAt, generationId). versionNo tính theo phạm vi nền tảng, không dùng làm ID. parentGeneratedPostId mô tả cây phân nhánh; null là xuất phát từ bài gốc. Nền tảng chưa có nội dung trả versions=[] và officialGenerationId=null. Không trả prompt/model/metadata nội bộ qua API lịch sử.

## Lỗi và lưu đồng thời

| Trường hợp | HTTP |
| --- | --- |
| Chưa đăng nhập / không đúng quyền | 401 / 403 |
| Không có hoặc đã xóa bài/nền tảng | 404 |
| Platform không hợp lệ; Guid.Empty; body hoặc độ dài sai | 422 |
| Source/official ID không thuộc đúng nền tảng hoặc không tồn tại | 400 |
| Đang Generating; hoặc ở Scheduled/Publishing/Published/PublishFailed | 409 khi sửa/chốt/sinh lại |
| Thiếu/sai/tắt prompt hoặc PostGen disabled | 400 ở regenerate |
| AI từ chối HTTP 400/422 | 400 |
| AI timeout/5xx/response sai | 502 |
| Heavy rate limit | 429, đọc Retry-After |

GET lịch sử vẫn đọc được trong khi đang sinh. Sau khi bắt đầu regenerate, backend nhả khóa database trước HTTP nhưng giữ trạng thái Generating; mọi thay đổi nội dung cùng nền tảng đều bị chặn cho tới khi xong. Sửa tay và chọn official dùng transaction/khóa bài gốc; các lần lưu được tuần tự hóa, lần sau áp dụng lên trạng thái mới nhất. Chưa có ETag hoặc kiểm tra bản form đã cũ: FE nên khóa nút trong lúc lưu và tải lại dữ liệu sau 409.

FE có thể cho chọn version rồi bấm “Dùng bản này” hoặc “Sinh tiếp từ bản này”. Các chức năng lịch sử/chọn version ảnh thuộc mốc 6, chưa triển khai.
