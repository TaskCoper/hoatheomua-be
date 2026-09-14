# PostGen — Contract AI proxy

Cập nhật 2026-09-14 theo contract người dùng cung cấp. Mốc 3 đã hiện thực typed `HttpClient` tại `HoaTheoMua.Service/AiService/`. Nội dung và ảnh dùng chung endpoint chat completions; luồng ảnh sẽ triển khai ở mốc 4.

## Endpoint và cấu hình

`POST https://proxy.hoatheomua.com/v1/chat/completions`, `Content-Type: application/json`. Hiện không cần API key.

| Option / biến môi trường | Mặc định | Ý nghĩa |
| --- | --- | --- |
| `AiOption__ChatCompletionsUrl` | `https://proxy.hoatheomua.com/v1/chat/completions` | URL đầy đủ |
| `AiOption__Model` | `chatgpt/gpt-5.6` | Model gửi trong request |
| `AiOption__ApiKey` | trống | Khi có giá trị, client hỗ trợ gửi `Authorization: Bearer`; cơ chế auth thực tế cần đối chiếu khi proxy bật key |
| `AiOption__TimeoutSeconds` | `120` | Timeout mỗi lời gọi, giá trị hợp lệ 1–180 giây |

Không log API key, prompt hoặc dữ liệu nguồn. Không tự follow redirect. Buffer response tối đa 1 MiB ở HttpClient đã đăng ký trong API. Không gửi `response_format`, tool hoặc tham số riêng chưa được proxy xác nhận.

Runtime đọc `Config(postgen.enabled)` nếu có: JSON boolean `false` chặn trước khi gọi AI. Ba Config prompt (content và isActive) đọc bằng Group/Key, không lọc UserId. Model hiện lấy từ AiOption; chưa hiện thực override bằng Config.

## Sinh nội dung — đã triển khai

```json
{
  "model": "chatgpt/gpt-5.6",
  "messages": [
    {"role": "system", "content": [
      {"type": "text", "text": "<nguyên value.content của postgen.system-prompt.facebook>"}
    ]},
    {"role": "user", "content": [
      {"type": "text", "text": "{\"platform\":\"facebook\",\"source\":{\"title\":\"Bó hoa Cảm Ơn\",\"content\":\"10 cành hồng...\",\"hashtag\":\"#HoaTheoMua\"},\"previousContent\":null,\"feedback\":null,\"constraints\":{\"language\":\"vi\"}}"}
    ]}
  ]
}
```

Prompt được gửi nguyên văn, không ghép prompt Post cũ hoặc `description` chuẩn nền tảng. Không gửi constraints.maxLength: các giới hạn biên tập nằm trong prompt. isActive đọc ngay từ Config prompt trước khi gọi. Chưa xác minh giới hạn xuất bản chính thức. Lời gọi nội dung chỉ text, không gửi ảnh bài gốc.

Proxy trả:

```json
{
  "id": "chatcmpl-example",
  "model": "chatgpt/gpt-5.6",
  "choices": [{
    "index": 0,
    "message": {"role": "assistant", "content": "{\"title\":\"Bó hoa Cảm Ơn\",\"content\":\"Nội dung bài đăng\",\"hashtag\":\"#HoaTheoMua\"}"},
    "finish_reason": "stop"
  }],
  "usage": {"prompt_tokens": 0, "completion_tokens": 0, "total_tokens": 0}
}
```

Đọc đúng một choice có `finish_reason=stop`, rồi parse JSON trong `message.content`. Chấp nhận một code fence JSON bao ngoài; không tìm/trích JSON tùy ý giữa lời giải thích. Bắt buộc `title`, `content`, `hashtag` là chuỗi; content không được rỗng. Title tối đa 500 ký tự, hashtag tối đa 1000 để phù hợp schema. Câu hỏi lại, Markdown ảnh, JSON hỏng hoặc kết quả bị cắt đều là lỗi, không tạo version.

Lưu `id` → `AiRequestId`, `model` → `AiModel`, thời gian gọi → `DurationMs`. `SystemPromptConfigId` và `PromptSnapshot` lưu nguồn prompt và nội dung đã gửi. Không suy ra chi phí từ `usage` bằng 0 của proxy.

## Nội dung có ảnh — contract cho mốc 4

Dùng cùng endpoint/model. `messages[].content` là **một danh sách có thứ tự**, không nhóm lại text và ảnh:

```json
[
  {"type": "text", "text": "Ảnh MỘT:"},
  {"type": "image_url", "image_url": {"url": "https://example.com/one.png"}},
  {"type": "text", "text": "Ảnh HAI:"},
  {"type": "image_url", "image_url": {"url": "https://example.com/two.png"}},
  {"type": "text", "text": "MỘT màu gì, HAI màu gì?"}
]
```

DTO hiện giữ thứ tự này và đã có test serialization. Chưa có method/service gọi sinh ảnh.

Response tạo ảnh do người dùng cung cấp có `message.content` dạng Markdown:

```text
![generated image](https://hcm.ss.bfcplatform.vn/taskcoper/general/example.png)
```

Mốc 4 sẽ trích URL ảnh HTTP/HTTPS từ nội dung này và kiểm kết quả trước khi lưu. Proxy không trả riêng `imageUrl`, width, height hoặc expiresAt. Không coi lời hỏi lại người dùng là kết quả tạo ảnh. Độ bền URL và kích thước ảnh cần kiểm trong mốc 4; việc dựng lại ảnh vẫn phải giữ sản phẩm/nhận diện và đúng tỷ lệ theo kế hoạch.

## Lỗi, timeout và giao dịch

| Trường hợp | Backend |
| --- | --- |
| Proxy HTTP 400/422 | 400 `AI service rejected the request` |
| Proxy HTTP 401/403/429/5xx hoặc lỗi kết nối/timeout | 502 `AI service is unavailable` |
| Envelope/JSON nội dung sai, thiếu trường, content rỗng, finish_reason khác stop | 502 `AI service returned an invalid response` |
| Caller hủy request | Hủy lời gọi; thu hồi trạng thái Generating bằng token cleanup riêng |

Không retry lời gọi nội dung. Tổng ngân sách một lượt sinh là 180 giây; từng lời gọi có timeout 120 giây mặc định. Sinh các nền tảng tuần tự, giữ kết quả trong bộ nhớ tới khi tất cả thành công. Lưu mọi GeneratedPost và cập nhật official trong một transaction; nếu một nền tảng lỗi, không ghi version mới cho bất kỳ nền tảng nào trong request. Bản official cũ vẫn giữ, các nền tảng đã claim chuyển Failed.

Claim dùng transaction ngắn khóa BasePost như PostCore update/delete, sau đó đặt các nền tảng đích Generating và commit **trước** HTTP. Request khác sinh cùng nền tảng hoặc sửa/xóa bài đang sinh nhận 409. Không giữ transaction mở suốt lời gọi AI.

Cleanup khi lỗi/cancel có ngân sách riêng 10 giây. Nếu process bị kill hoặc database mất kết nối khiến cleanup thất bại, có thể còn trạng thái Generating; hiện chưa có cơ chế tự phục hồi job. Reverse proxy cần cho phép request kéo dài theo ngân sách; chưa kiểm chứng thời gian chờ trên môi trường deployed.

## Kiểm chứng

- Mock HttpMessageHandler kiểm payload, prompt nguyên văn, optional auth, thứ tự text/ảnh, timeout/cancellation, HTTP lỗi, JSON sai và không retry.
- PostgreSQL kiểm claim trùng, chặn sửa/xóa trong lúc sinh, rollback khi ghi official lỗi và giữ version trước.
- Một lời gọi trực tiếp tới proxy bằng prompt Facebook và bài mẫu trả HTTP 200, `finish_reason=stop`, JSON đủ ba trường; provider ID `chatcmpl-98193ffb08d04da1aef8a3b1`. Đây là kiểm chứng contract text, chưa phải kiểm thử sinh ảnh hoặc toàn bộ chất lượng nội dung AI.
