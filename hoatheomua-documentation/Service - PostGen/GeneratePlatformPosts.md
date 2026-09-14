# GeneratePlatformPosts — mốc 3–4

`POST /api/v1/posts/{id:guid}/generate`, JWT Staff/Admin, heavy rate limit. HTTP đồng bộ, không retry tự động. Mốc 4 và mode content/images/all đã triển khai, kiểm thử và được đưa vào nhánh `feature/post-gen`; chưa deploy. Không cần migration mới.

## Chọn phần cần sinh bằng `mode`

Cùng endpoint `POST /api/v1/posts/{id}/generate`, `mode` là chuỗi `content`, `images` hoặc `all` (viết thường). Bỏ field thì mặc định `all` để tương thích request cũ; gửi null hoặc giá trị khác trả 422.

| Mode | Xử lý |
| --- | --- |
| `content` | Chỉ gọi AI sinh nội dung, tạo version và chốt nội dung mới. Giữ nguyên ảnh/slot và thứ tự đã chốt. Không gửi ratios/usePreset=true; nếu gửi trả 422. Không cần Config chỉ dẫn ảnh/preset. |
| `images` | Chỉ gọi AI sinh ảnh. Không thêm GeneratedPost, không đổi nội dung, OfficialGenerationId hoặc SelectedAt. Mỗi nền tảng phải có ratios hoặc usePreset=true; thiếu trả 422. |
| `all` (mặc định) | Sinh nội dung trước, rồi ảnh theo lựa chọn. Không có lựa chọn ảnh thì chỉ sinh nội dung như request cũ. |

Hai lần gọi tách rời để tránh sinh nội dung hai lần:

```json
{
  "mode": "content",
  "platforms": [{ "platform": "facebook" }]
}
```

```json
{
  "mode": "images",
  "platforms": [{
    "platform": "facebook",
    "ratios": [{ "ratioCode": "Story9x16", "basePostImageIds": [] }]
  }]
}
```

Images mode vẫn kiểm postgen.enabled và isActive trong Config nền tảng, cùng chỉ dẫn ảnh/preset khi dùng; không yêu cầu hoặc gửi value.content của prompt nội dung. Có thể sinh ảnh trước khi có nội dung: platform.content vẫn null, chưa sẵn sàng đăng. Thành công/lỗi ảnh chỉ cập nhật trạng thái xử lý, ảnh và lastError; nội dung chốt giữ nguyên kể cả khi mọi ảnh lỗi. Hủy request hoặc ghi DB lỗi vẫn giải phóng claim và giữ dữ liệu đã chốt trước.

Mode áp dụng cho toàn bộ request. Các mode cùng dùng khóa/claim của nền tảng để chặn yêu cầu sinh trùng; không gọi content và images đồng thời cho cùng nền tảng. Không tự retry POST.

## Request

```json
{
  "platforms": [
    {
      "platform": "facebook",
      "ratios": [{ "ratioCode": "Portrait4x5", "basePostImageIds": [] }]
    },
    { "platform": "instagram", "usePreset": true }
  ]
}
```

Ở mode all mặc định, chỉ có platform (không ratios/usePreset) vẫn sinh nội dung. Khi sinh ảnh: ratios có 1–5 tỷ lệ hợp lệ không trùng; IDs thiếu/null/[] là mọi ảnh nguồn còn hiệu lực. Các ID chỉ định phải thuộc bài, không trùng/Guid.Empty và không bị xóa. usePreset=true đọc Config chung postgen.ratio-preset và loại trừ ratios. first_n lấy theo sortOrder; all_images lấy tất cả. Thiếu hoặc sai Config khi sử dụng trả 400; vượt 12 cặp ảnh × tỷ lệ khác nhau trả 422 trước bất kỳ AI call hoặc claim nào.

Chỉ dẫn ảnh là JSON string của Config postgen.image_instruction, không ghép system prompt nền tảng. Request/response FE đầy đủ tại [bàn giao mốc 4](../FE-API-Handoff-PostGen-Milestone4.md).

## Giao dịch và kết quả

1. Validate request, đọc prompt nền tảng đang bật.
2. Khóa BasePost như PostCore PUT/DELETE. Snapshot ảnh nguồn, đọc preset/chỉ dẫn và kiểm ngưỡng. Claim các nền tảng Generating, commit, nhả khóa trước HTTP.
3. Với mode content/all, sinh mọi nội dung tuần tự; một nội dung lỗi thì không sinh ảnh, không ghi version mới, giữ official trước và chuyển các nền tảng đã claim Failed.
4. Với mode images/all, gom cặp ảnh/tỷ lệ toàn request; mỗi cặp chỉ một lời gọi AI, tối đa 3 đồng thời. Các task không truy cập DbContext. Lỗi từng ảnh giữ thành attempt Failed; task khác tiếp tục.
5. Transaction lưu kết quả theo mode; chỉ tự chọn nội dung mới khi có GeneratedPost mới, tạo slot ảnh còn thiếu; chỉ điền ảnh thành công vào slot chưa chọn. Giữ ảnh và sortOrder đã chọn; cặp mới nối cuối. Ghi thất bại rollback cả nội dung, ảnh và slot mới.
6. GET detail trả nội dung/ảnh đã chốt; không có value.images hoặc platforms[].versions. Lịch sử vẫn lưu trong database.

Ngân sách AI 180 giây, timeout mỗi lời gọi mặc định 120 giây. Hết ngân sách ở pha ảnh vẫn lưu kết quả đã có, các cặp chưa hoàn thành thành Failed. Lưu DB được cấp tối đa 10 giây riêng. Caller hủy trước commit bỏ kết quả mới; cleanup claim tối đa 10 giây. Process crash vẫn chưa có job thu hồi claim.

| Kết quả từng nền tảng | HTTP / trạng thái |
| --- | --- |
| Nội dung/ảnh thành công | 200 Generated; Partial nếu còn slot cũ chưa chọn |
| Một số ảnh yêu cầu lỗi | 200 Partial, lastError, giữ nội dung hiện tại và ảnh thành công |
| Mọi ảnh yêu cầu lỗi | 200 Failed, lastError; mode all chốt nội dung mới; mode images giữ nội dung cũ, ảnh đã chốt trước giữ nguyên |
| Request/ratio sai hoặc vượt ngưỡng | 422 |
| Ảnh sai bài/đã xóa, Config thiếu/sai hoặc disabled | 400 |
| Bài không tồn tại/đã xóa | 404 |
| Nền tảng đang sinh hoặc trạng thái không cho phép | 409 |
| AI nội dung 400/422 | 400 |
| AI nội dung timeout/response sai/lỗi kết nối/5xx | 502 |
| Heavy rate limit | 429 kèm Retry-After |

`isSuccess=true` ở HTTP 200 không bảo đảm mọi ảnh thành công; FE đọc status/lastError của từng nền tảng. GET detail suy ra Partial từ trạng thái Generated/Ready và LastError hoặc slot trộn đã/chưa chọn.

## Giới hạn provider

Parse đúng một Markdown ảnh có URL HTTP/HTTPS và finish_reason=stop; từ chối câu hỏi lại, nhiều ảnh, URL chứa credentials, URL quá 2048 ký tự. Proxy chưa trả số đo file hoặc expiresAt: pixelWidth/pixelHeight là kích thước mục tiêu gửi AI, ImageExpiresAt để null. Không tải/rehost ảnh. Chưa kiểm độ bền URL/kích thước thực/giữ sản phẩm bằng provider thật trong mốc này.

## Kiểm chứng

Service test và PostgreSQL integration kiểm dedup, concurrency, source/preset, timeout/cancel, giữ ảnh cũ, partial/all failure và rollback cả ảnh/slot khi ghi official lỗi. HTTP probe đi qua controller, JWT, validation, rate limiter, AiService thật với fake HttpMessageHandler, và PostgreSQL riêng. Báo cáo tại [Implementation.md](Implementation.md).
