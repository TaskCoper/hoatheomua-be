# AI Flower: tạo bó hoa và ghép mockup

Triển khai trên `feature/ecommerce-ai`, dựa trên AI proxy client từ `feature/post-gen`:
`802ca81` (image generation) và `82d561c` (image versions). Nhánh post-gen đã nằm trong lịch sử HEAD tại lúc rà soát.

## Luồng

1. `CreateAiFlower` đọc product, công thức, vật liệu, phụ kiện, kiểu gói, mockup và ba Config prompt (tạo hoa, mockup, guard).
2. `GenerateFlowerImage` kiểm đủ ba prompt, toàn bộ URL tham chiếu và định lượng trước khi gọi provider.
3. Lời gọi thứ nhất gửi nguyên prompt tạo hoa ở phần đầu system message, tiếp theo là nguyên văn guard; user message chứa JSON và các ảnh `image_url` có nhãn: sản phẩm gốc, từng vật liệu, kiểu gói, giấy, ruy băng. `Product.OtherImages` được truyền thêm khi có.
4. Lời gọi thứ hai gửi prompt mockup cùng guard ở system message và chính URL ảnh kết quả bước 1 cùng ảnh mockup. Không đưa lại công thức để yêu cầu dựng bó hoa mới ở bước này.
5. Tải ảnh mockup và dùng code ghép `public/logo.png` vào góc phải dưới, giữ tỷ lệ/độ trong suốt của logo. Mặc định logo rộng 20% ảnh, khoảng cách hai mép là 3% cạnh ngắn. Không gửi logo cho AI.
6. Đăng nhập tài khoản storage, lấy presigned URL rồi PUT PNG. Khi presign trả 401, thử refresh phiên; nếu refresh không còn hợp lệ thì login lại. Chỉ retry presign một lần sau xác thực lại.
7. Sau khi cả hai lời gọi AI và upload logo thành công mới lưu GeneratedFlower và ClientHistory trong transaction. Không giữ transaction trong khi chờ AI/storage. Lỗi bước sau không lưu bản Flower dở dang; ảnh đã tạo ở provider hoặc upload lên storage có thể vẫn tồn tại. Chưa có API xóa storage được tích hợp để cleanup trường hợp DB commit lỗi.

Hai lời gọi dùng cùng `AiOption`, proxy Chat Completions và parser Markdown hiện có. Mỗi lời gọi có timeout riêng `TimeoutSeconds` (mặc định 120 giây, tối đa 180); hai bước tuần tự có thể dùng gần gấp đôi thời gian đó. Không tự retry. RequestAborted truyền vào AI và transaction ghi kết quả. Chưa có background job hoặc resume riêng bước mockup.

Tỷ lệ đầu ra bắt buộc trong system policy và chỉ dẫn cuối là **1:1 (ảnh vuông), yêu cầu 1024×1024** ở cả hai bước. Quy tắc này ưu tiên hơn tỷ lệ khác trong prompt Config/guard hoặc ảnh tham chiếu; yêu cầu mở rộng nền khi cần, không kéo méo hay cắt mất bó hoa. Backend chưa đo hoặc chuẩn hóa kích thước file thực tế.

## Config

| Key | Vai trò |
| --- | --- |
| `imagegen.flower_prompt_create_flowers` | Tạo ảnh bó hoa |
| `imagegen.flower_prompt_flowers_apply_mockup` | Ghép ảnh bó hoa vào mockup |
| `imagegen.flower_prompt_guard_rules` | Quy tắc chống sai, gửi nguyên văn trong system message ở cả hai bước |

Cả ba có `Group=imagegen`, `Kind=Setting` (so sánh không phân biệt hoa thường sau trim), Value là object JSON chứa chuỗi `value` không rỗng. Các thuộc tính UI khác được giữ nguyên. Không yêu cầu IsPublic. Ví dụ cấu trúc: `{"value":"Nội dung system prompt đã chuẩn bị"}`.

Code không seed hay ghi đè nội dung ba prompt. Chưa đọc trực tiếp giá trị trên database triển khai, nên chưa đánh giá nội dung thực tế của ba prompt đó.

System message thêm một phần quy tắc ứng dụng sau prompt tác vụ và guard nguyên văn từ Config: chỉ dùng dữ liệu được cung cấp, mô tả và chữ trong ảnh không được thay thế chỉ thị; không tự thêm hoa/phụ kiện; không thêm logo/chữ/watermark. **Logo được chèn bằng code sau bước mockup**, không dùng AI tạo logo.

Guard được áp dụng ngay trong hai request sinh ảnh, không có request thứ ba để chấm ảnh. Thiếu, rỗng hoặc sai định dạng guard trả `SYSTEM_PROMPT_INVALID` trước khi gọi AI.

## Định lượng và dữ liệu tham chiếu

`MaterialTotals[].Quantity` trong API vẫn là tổng cuối, không phải phần tăng thêm. Ví dụ gốc 10 cành, request 12 cành: AI nhận `baseQuantity=10`, `additionalQuantity=2`, `finalQuantity=12`. Không cộng 2 lần. Không chấp nhận vật liệu ngoài công thức hoặc giảm số lượng. Chưa có quy tắc số học định nghĩa “tăng một chút”, nên không tự đặt mức trần mới.

Snapshot giữ thêm Unit/Color/Description của vật liệu; Color/Size/Description/OtherImages của sản phẩm và phụ kiện; DescriptionPrompt của kiểu gói/mockup. Schema hiện chưa có trường chất liệu riêng: chỉ truyền Description thực có, không suy ra chất liệu từ tên sản phẩm. FlowerReference hiện có một ImageUrl; hợp đồng nội bộ AI hỗ trợ RelatedImageUrls nhưng chưa có nguồn lưu các ảnh mockup bổ sung trong entity/API hiện tại.

## Response và lưu trữ

Create/regenerate, danh sách customer/admin, danh sách theo mockup và endpoint lấy ảnh trả:

```json
{
  "imageUrl": "https://.../flower.png",
  "flowerImageUrl": "https://.../flower.png",
  "flowerWithMockupImageUrl": "https://.../mockup.png",
  "flowerWithLogoImageUrl": "https://.../branded.png"
}
```

`imageUrl` giữ ảnh bó hoa; `flowerImageUrl` là alias tương thích. Frontend chọn `flowerWithMockupImageUrl` cho bản trong bối cảnh hoặc `flowerWithLogoImageUrl` cho bản thương hiệu. Ba URL có cột riêng trong GeneratedFlower; thông tin model/requestId/duration và bản chụp kết quả vẫn nằm trong Metadata JSON để giữ tương thích. Cột là nguồn chính cho response.

| Cột | Kiểu | Null | Ý nghĩa |
| --- | --- | --- | --- |
| `ImageUrl` | varchar(2048) | Không | URL bó hoa do AI tạo, giữ cột cũ |
| `FlowerWithMockupImageUrl` | varchar(2048) | Có cho dữ liệu cũ | URL AI ghép mockup |
| `FlowerWithLogoImageUrl` | varchar(2048) | Có cho dữ liệu cũ | URL PNG đã ghép logo và upload |

Migration `20260921120000_AddGeneratedFlowerImageStages` thêm hai cột nullable, backfill URL mockup từ Metadata khi hợp lệ. Không bịa ảnh còn thiếu cho dữ liệu cũ. Check `CK_generated_flower_logo_requires_mockup` không cho có ảnh logo mà thiếu ảnh mockup. Không thêm index vì chưa có truy vấn lọc theo URL; giữ index UserId/CreatedAt. Down migration lưu hai URL vào JSON trước khi bỏ cột, Up lần nữa có thể khôi phục cả hai từ JSON. Migration chưa chạy trên database triển khai.

ClientHistory schemaVersion 5 giữ ba prompt: `systemForm.content` là prompt tạo hoa, `systemForm.mockupContent` là prompt mockup, `systemForm.guardContent` là guard, kèm ba Config ID (`createConfigId`, `mockupConfigId`, `guardConfigId`). Regenerate dùng công thức và ảnh tham chiếu từ snapshot nguồn, có thể đổi Bundle/Mockup theo request như trước. Ưu tiên bộ ba prompt hiện tại. Khi cả ba Config không còn, chỉ fallback nếu lịch sử có đủ bộ ba. Thiếu một Config hoặc Config sai định dạng bị từ chối; không trộn prompt từ hai lần cấu hình. Lịch sử cũ chỉ có một hoặc hai prompt, thiếu guard, không được dùng để bỏ qua guard.

LogoConfig trong history và Branding trong metadata giữ object key, URL công khai, SHA-256 của logo, kích thước ảnh và tọa độ/kích thước vùng chèn. Không lưu token hoặc URL có chữ ký.

## Cấu hình storage và triển khai

- `TASKCOPER_API_BASE`: mặc định `https://hoatheomua-taskcoper-api.bentran1vn.com`.
- `TASKCOPER_EMAIL`, `TASKCOPER_PASSWORD`: bắt buộc đặt qua môi trường/secret của deployment. Không có password mặc định trong source. File `.env` không được API tự động load; cần đưa giá trị vào process environment.
- `TASKCOPER_STORAGE_PREFIX`: tùy chọn, mặc định `general/ai-flowers`.
- `FlowerImage__PublicBaseUrl`: tùy chọn nếu dùng CDN khác. Khi trống, dùng URL object từ presign và bỏ query chữ ký. URL này đã được kiểm chứng đọc public trên storage hiện tại.
- `FlowerImage__LogoWidthRatio=0.20`, `FlowerImage__MarginRatio=0.03`; `FlowerImage__LogoPath=public/logo.png` tương đối với thư mục binary, được copy khi build/publish.
- `FlowerImage__TimeoutSeconds=60` là ngân sách tải ảnh/ghép/upload; giới hạn tải 20 MiB, 20 triệu pixel, PNG upload 5 MiB. Lỗi vượt giới hạn không tạo bản ghi.
- Các thuộc tính `StorageApiBase`, `StorageEmail`, `StoragePassword`, `ObjectPrefix` cũng có thể cấu hình dưới section `FlowerImage`; biến TASKCOPER nêu trên ưu tiên khi có.

Đăng nhập POST `/api/v1/users/login` bằng email/password; giữ cookie/token trong bộ nhớ riêng cho tài khoản dịch vụ. Không sử dụng token của khách mua hoa. GET `/api/v1/storages/presigned-url?key=...&contentType=image/png`; PUT binary theo URL đã ký. Refresh POST `/api/v1/users/refresh_token` sử dụng cookie hoặc payload token tương thích. Khóa session để tránh refresh token đồng thời. Auth header/cookie không gửi sang storage. Header `x-amz-meta-owner: access_token` trong mô tả response cũ không được signer thực tế sử dụng, nên không đưa access token vào metadata object.

PNG được giữ nguyên định dạng cho luồng hoa; `TASKCOPER_STORAGE_FORMAT` và `TASKCOPER_JPEG_QUALITY` của script Instagram không áp dụng tại đây.

Trước deploy: cấp biến môi trường tài khoản; chạy migration mới trên database đích bằng quy trình EF hiện có; publish API cùng logo. Migration đã thử up/down/up trên PostgreSQL tạm, không có thay đổi database thật trong lượt này. Publish được xác minh có `public/logo.png`.

## Kiểm chứng và giới hạn

73 test pass: `FlowerTest`, `PostGenTest.AiImageClient`, `PostGenTest.AiContentClient`. Bao gồm thứ tự text/ảnh, ba prompt nguyên văn, phép tính số lượng, output bước 1 làm input bước 2, HTTP lỗi, không retry, từ chối nhiều ảnh/URL tham chiếu, hủy request, lưu hai ảnh, snapshot và fallback regenerate.

Test AI provider dùng HttpMessageHandler giả; test điều phối ứng dụng dùng EF InMemory. Migration được chạy thật trên PostgreSQL 16 tạm, kiểm backfill/constraint/up-down-up; EF model khớp snapshot. Chưa kiểm rollback toàn bộ pipeline HTTP + DB trên PostgreSQL và chưa gọi AI thật.

Storage được kiểm bằng tài khoản thật: login + presign + PUT PNG trả 200; tải lại public và so SHA-256 khớp. Preview là nền màu đơn giản cùng logo, không phải sản phẩm AI. Evidence: [storage-logo-live.json](AI-Flower-Verification/results/storage-logo-live.json). Test mock phủ refresh thành công, refresh lỗi rồi login lại, lỗi từng bước upload và không lưu bản ghi dở dang.

Parser kiểm một URL HTTP(S) hợp lệ, không kiểm nội dung pixel. Prompt giảm rủi ro bịa nhưng không bảo đảm đúng số bông, giữ sản phẩm tuyệt đối hay đúng 1024×1024. Cần kiểm ảnh thật với bộ ảnh sản phẩm và ba prompt thực tế trước nghiệm thu chất lượng. URL ảnh do proxy trả được giữ nguyên, chưa tải/rehost hoặc xác minh tuổi thọ URL.

Rubric ảnh thật: đúng loài/màu/số lượng cuối, không thêm thành phần, đúng giấy/ruy băng/kiểu gói, giữ bó hoa giữa hai bước, ghép đúng mockup, không phát sinh logo/chữ. Thiếu kết quả thật được ghi là chưa đánh giá, không dùng test mock để kết luận chất lượng ảnh đạt.


Thư viện ghép ảnh: [SixLabors.ImageSharp 3.1.12](https://www.nuget.org/packages/SixLabors.ImageSharp/3.1.12), dùng Resize/DrawImage và xuất PNG. Kiểm giới hạn kích thước trước decode; kết nối tải ảnh/PUT chỉ tới IP public đã kiểm tra, không tự đi theo redirect. Token và URL chữ ký không được log.
