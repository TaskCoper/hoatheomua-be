# Tích hợp Xanh SM Express

## Luồng gọi API

```text
Frontend (JWT customer)
  -> XanhSmController
  -> FluentValidation
  -> XanhSmService
  -> XanhSmTokenProvider (lấy/cache OAuth token)
  -> Xanh SM Partner API
  -> ApiResponseFactory.Base(...)
  -> Frontend
```

`XanhSmController` chỉ nhận request, gọi service và bọc kết quả theo format chung của Hoa Theo Mùa. Controller có `[Authorize(Policy = JwtExtensions.CustomerPolicy)]`, nên token đăng nhập của khách hàng là bắt buộc. Token này không bao giờ được gửi cho Xanh SM.

## Endpoints cho frontend

| API | Upstream | Nội dung `value` khi thành công |
| --- | --- | --- |
| `POST /api/v1/xanh-sm/available-services` | `/list-available-services` | Mảng dịch vụ |
| `POST /api/v1/xanh-sm/estimates` | `/estimate/order` | Estimate, gồm `fee_id`, phí và thời hạn |
| `POST /api/v1/xanh-sm/orders` | `/create-order` | Đơn Xanh SM mới tạo |
| `POST /api/v1/xanh-sm/orders/cancel` | `/cancel-order` | `{ "message": "ok" }` |

Mọi response thành công dùng envelope hiện có:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "...",
  "timestampUtc": "...",
  "value": {}
}
```

## Trách nhiệm của từng file

- `Request.cs`: DTO từ FE sang Xanh SM. Mọi `[JsonPropertyName]` giữ nguyên tên snake_case của Xanh SM, ví dụ `fee_id` và `package_size`; tránh lỗi serialise camelCase mặc định của ASP.NET.
- `Response.cs`: DTO response từ Xanh SM. Chỉ khai báo các field FE cần; `System.Text.Json` tự bỏ qua field upstream chưa dùng nên không vỡ khi họ bổ sung field.
- `XanhSmOption.cs`: một nơi chứa URL, OAuth credential, đường dẫn endpoint và timeout. Secret được lấy qua biến môi trường, không commit vào source.
- `XanhSmTokenProvider.cs`: gửi OAuth `application/x-www-form-urlencoded` với `client_credentials`; cache token in-memory; `SemaphoreSlim` bảo đảm nhiều request đồng thời chỉ refresh token một lần.
- `Service.cs`: gắn `Authorization: Bearer <partner-token>` và `Accept-Language`, gọi upstream, retry đúng một lần khi 401, deserialize body, rồi map lỗi Xanh SM sang lỗi API an toàn.
- `Validations/`: kiểm tra tọa độ, phone format, pickup/drop-off, package size, COD matching, bulky/door-to-door và payment checkpoint trước khi gửi đối tác.
- `XanhSmController.cs`: expose đúng 4 endpoint FE gọi.

## Quy tắc quan trọng khi FE gọi

1. Gọi available-services để lấy `service.id` và package sizes hợp lệ.
2. Gọi estimate. Nếu chọn addon, addon nằm trong `service_ids[].addons`.
3. Lưu `fee_id`, `order_time`, points và addons. `order_time` create phải giống estimate.
4. Gọi create. Addon chuyển lên root `addons[]`; riêng COD vẫn là object `points[].addons` tại drop-off và `collect_cod` phải bằng `remain_cod`.
5. Dùng `data.id` trả từ create làm `order_id` để cancel.

## Cấu hình môi trường

Điền các giá trị sau vào Docker/GitHub environment secrets:

```text
XANH_SM_BASE_URL=https://api-sandbox.vn.gsm-api.net/v1/sandbox
XANH_SM_TOKEN_URL=https://api-partner.vn.gsm-api.net/v1/public/token
XANH_SM_CLIENT_ID=
XANH_SM_CLIENT_SECRET=
XANH_SM_SCOPE=sandbox.express.trips
XANH_SM_GRANT_TYPE=client_credentials
XANH_SM_ACCEPT_LANGUAGE=vi
XANH_SM_TOKEN_REFRESH_BUFFER_SECONDS=300
XANH_SM_TIMEOUT_SECONDS=30
```

Token endpoint dùng chính xác OAuth Client Credentials:

```text
POST https://api-partner.vn.gsm-api.net/v1/public/token
Content-Type: application/x-www-form-urlencoded

client_id=<CLIENT_ID>
client_secret=<CLIENT_SECRET>
grant_type=client_credentials
scope=sandbox.express.trips
```

Token có hiệu lực 30 ngày (`expires_in = 2592000`) và không có refresh token. `XanhSmTokenProvider` cache token đến trước hạn 300 giây, sau đó tự tạo token mới. Cấu hình code hiện tại đã đúng theo flow này.

Sandbox gọi API theo base URL `https://api-sandbox.vn.gsm-api.net/v1/sandbox`, nên endpoint dịch vụ cuối cùng là ví dụ `https://api-sandbox.vn.gsm-api.net/v1/sandbox/list-available-services`.

Trước khi gọi sandbox/production thật, IP public của server/container phải được GSM whitelist. Đây là điều kiện phía GSM, không thể xử lý bằng code.

## Lỗi

- Validation request: HTTP 422 từ middleware hiện hữu.
- Xanh SM trả lỗi request: HTTP 400/404/409/429, `messageCode` dạng `XANH_SM_<code>`.
- Token/credential/network/upstream 5xx: HTTP 502.
- Timeout: HTTP 504.

Access token, client secret và raw request body có số điện thoại không được ghi vào log.
