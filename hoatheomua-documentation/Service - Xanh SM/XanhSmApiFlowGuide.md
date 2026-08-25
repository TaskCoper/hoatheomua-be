# Hướng dẫn sử dụng API Xanh SM

Tài liệu này mô tả đầy đủ luồng frontend sử dụng dịch vụ giao hàng Xanh SM qua backend Hoa Theo Mùa, đồng thời giải thích nhiệm vụ của từng cụm code trong feature `XanhSmService`.

> Frontend chỉ gọi API Hoa Theo Mùa. Frontend **không** được gọi trực tiếp GSM/Xanh SM và **không** biết `ClientSecret` hoặc access token của GSM.

## 1. Sơ đồ tổng quát

```text
Frontend
    |
    | JWT của khách hàng
    v
HoaTheoMua API: XanhSmController
    |
    | FluentValidation kiểm tra dữ liệu đầu vào
    v
XanhSmService
    |
    | cần token?
    v
XanhSmTokenProvider
    |-- Có token cache còn hạn --> dùng lại token
    `-- Chưa có/hết hạn --------> OAuth Client Credentials tới GSM
    |
    v
GSM / Xanh SM API
    |
    v
HoaTheoMua API response envelope
    |
    v
Frontend
```

Response thành công luôn được bọc theo format chung:

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "...",
  "timestampUtc": "2026-08-24T00:00:00Z",
  "value": {}
}
```

Toàn bộ dữ liệu Xanh SM hữu ích cho FE nằm trong `value`.

---

## 2. Chuẩn bị trước khi gọi API

### 2.1. Backend cần có JWT customer

Bốn endpoint Xanh SM dùng `[Authorize(Policy = JwtExtensions.CustomerPolicy)]`. FE gửi JWT Hoa Theo Mùa:

```http
Authorization: Bearer <JWT_CUA_KHACH_HANG>
Content-Type: application/json
```

JWT này chỉ xác thực khách hàng với Hoa Theo Mùa. Nó không được gửi tiếp sang GSM.

### 2.2. Cấu hình server

Các biến môi trường cần có tại server/container:

```text
XanhSmOption__BaseUrl=https://api-sandbox.vn.gsm-api.net/v1/sandbox
XanhSmOption__TokenUrl=https://api-partner.vn.gsm-api.net/v1/public/token
XanhSmOption__ClientId=<CLIENT_ID_GSM>
XanhSmOption__ClientSecret=<CLIENT_SECRET_GSM>
XanhSmOption__Scope=sandbox.express.trips
XanhSmOption__GrantType=client_credentials
XanhSmOption__AcceptLanguage=vi
XanhSmOption__TokenRefreshBufferSeconds=300
XanhSmOption__TimeoutSeconds=30
```

`ClientSecret` chỉ đặt trong environment secret/Docker secret/GitHub secret. Không gửi về FE, không commit vào Git và không in log.

### 2.3. Whitelist IP GSM

GSM chặn token request nếu IP outbound chưa whitelist. IP public đã kiểm tra của server hiện tại là:

```text
171.243.48.81
```

Thông tin gửi GSM:

```text
Environment: Sandbox
Client ID: sb_hsJWsSodxNoUlDZ3uEUOgf7KcFwApOHQ
Scope: sandbox.express.trips
Public IPv4: 171.243.48.81
```

Nếu chạy Docker, phải whitelist IP public của host/NAT gateway, không phải IP container `172.x.x.x`.

---

## 3. Flow 1 — Lấy dịch vụ khả dụng

### Mục đích

Tìm những dịch vụ Xanh SM đang nhận đơn ở khu vực pickup/dropoff. FE dùng response này để lấy:

- `service.id`: dùng tại estimate và create order.
- `service.service_type`: dùng tại create order.
- `service.express_package_sizes`: xác định package size hợp lệ.
- `service.travel_mode`: phân biệt Bike/Truck.
- `service.during_service`: biết dịch vụ có đang trong giờ hoạt động hay không.

### Endpoint

```http
POST /api/v1/xanh-sm/available-services
```

### Ví dụ request FE

```json
{
  "pickups": [
    {
      "longitude": 106.70098,
      "latitude": 10.77653
    }
  ],
  "dropoffs": [
    {
      "longitude": 106.70321,
      "latitude": 10.78011
    }
  ],
  "service_types": ["EXPRESS-ONDEMAND"],
  "order_time": 1770350401
}
```

### Backend xử lý

```text
XanhSmController.GetAvailableServices
  -> AvailableServicesRequestValidator
  -> XanhSmService.GetAvailableServices
  -> XanhSmService.PostAsync
  -> XanhSmTokenProvider.GetAccessToken
  -> GSM POST /list-available-services
```

### Validation

- `pickups` phải có ít nhất một phần tử.
- `latitude` từ `-90` đến `90`.
- `longitude` từ `-180` đến `180`.
- `service_types[]` không được rỗng.
- `order_time` nếu có phải lớn hơn hoặc bằng `0`.

### Response FE cần dùng

```json
{
  "isSuccess": true,
  "value": [
    {
      "service": {
        "id": 2099,
        "display_name": "Van Express",
        "service_type": "EXPRESS-ONDEMAND",
        "travel_mode": "Truck",
        "express_package_sizes": ["VAN_SMALL", "VAN_MEDIUM", "VAN_LARGE"],
        "during_service": true
      }
    }
  ]
}
```

Sau đó FE cho khách chọn service và package size phù hợp trước khi estimate.

---

## 4. Flow 2 — Estimate phí giao hàng

### Mục đích

Lấy giá giao hàng trước khi tạo đơn. Response trả `fee_id`; đây là mã giá mà Xanh SM khóa cho request create order.

### Endpoint

```http
POST /api/v1/xanh-sm/estimates
```

### Ví dụ request không COD

```json
{
  "service_ids": [
    { "id": 2099 }
  ],
  "order_time": 1770350401,
  "payment_method": "cash",
  "points": [
    {
      "type": "PICK UP",
      "name": "Người gửi",
      "phone_number": "+84886901000",
      "latitude": 10.776889,
      "longitude": 106.700806,
      "short_address": "Vincom Đồng Khởi",
      "address": "72 Lê Thánh Tôn, Quận 1, TP. Hồ Chí Minh",
      "package_weight": 1,
      "package_size": "VAN_SMALL",
      "package_types": ["food"]
    },
    {
      "type": "DROP OFF",
      "name": "Người nhận",
      "phone_number": "+84886901001",
      "latitude": 10.801937,
      "longitude": 106.712372,
      "short_address": "Landmark 81",
      "address": "208 Nguyễn Hữu Cảnh, Bình Thạnh, TP. Hồ Chí Minh"
    }
  ]
}
```

### Ví dụ request có COD và Door-to-door

Ở **estimate**, addons đặt trong mỗi phần tử `service_ids[]`.

```json
{
  "service_ids": [
    {
      "id": 2099,
      "addons": [
        { "id": 4, "apply": true, "remain_cod": 100000, "value": 1 },
        { "id": 5, "apply": true }
      ]
    }
  ],
  "order_time": 1770350401,
  "payment_method": "cash",
  "points": [
    {
      "type": "PICK UP",
      "name": "Người gửi",
      "phone_number": "+84886901000",
      "latitude": 10.776889,
      "longitude": 106.700806,
      "short_address": "Vincom Đồng Khởi",
      "address": "72 Lê Thánh Tôn, Quận 1, TP. Hồ Chí Minh",
      "package_weight": 1,
      "package_size": "SMALL",
      "package_types": ["food"]
    },
    {
      "type": "DROP OFF",
      "name": "Người nhận",
      "phone_number": "+84886901001",
      "latitude": 10.801937,
      "longitude": 106.712372,
      "short_address": "Landmark 81",
      "address": "208 Nguyễn Hữu Cảnh, Bình Thạnh, TP. Hồ Chí Minh",
      "addons": {
        "collect_cod": 100000,
        "currency_code": "VND"
      }
    }
  ]
}
```

### Rule quan trọng

- Phải có ít nhất một `PICK UP` và một `DROP OFF`.
- `package_weight`, `package_size`, `package_types` chỉ bắt buộc ở `PICK UP`.
- `payment_method` chỉ là `cash` hoặc `partner_pay`.
- COD addon là ID `4` hoặc `11`:
  - `apply: true`
  - `remain_cod >= 0`
  - `value: 1`
  - `points[].addons.collect_cod` tại DROP OFF phải bằng `remain_cod`.
- Door-to-door addon ID `5` chỉ có một DROP OFF.
- Bulky addon ID `9` dùng `package_size: LARGE`, cân nặng `31–50 kg`.

### Dữ liệu phải lưu trên FE

FE cần giữ nguyên các giá trị sau cho bước tạo đơn:

```text
fee_id
order_time
service.id
service.service_type
points
addons đã áp dụng
payment_method
```

Không được thay đổi `order_time`, `points` hoặc addons giữa estimate và create order; Xanh SM có thể từ chối `fee_id`.

### Response cần đọc

```json
{
  "isSuccess": true,
  "value": {
    "payment_method": "cash",
    "order_time": "1770350401",
    "service_estimates": [
      {
        "fee_unavailable": false,
        "service": {
          "id": 2099,
          "service_type": "EXPRESS-ONDEMAND"
        },
        "estimate_info": {
          "fee_id": "d98fc496-492f-41a3-be80-3c2590345df6",
          "expires_at": 1778494276,
          "estimate": {
            "total_fee": 177000,
            "total_pay": 177000,
            "currency_code": "VND"
          },
          "trip": {
            "distance_estimate": 5.33,
            "duration_estimate": 702
          }
        }
      }
    ]
  }
}
```

Chỉ cho phép người dùng bấm tạo đơn khi:

```text
fee_unavailable = false
fee_id tồn tại
expires_at còn hạn
```

---

## 5. Flow 3 — Tạo đơn giao hàng

### Mục đích

Tạo đơn Xanh SM từ giá đã estimate. Xanh SM trả `data.id`; đây là Xanh SM order ID để hủy hoặc theo dõi sau này.

### Endpoint

```http
POST /api/v1/xanh-sm/orders
```

### Khác biệt quan trọng với estimate

| Dữ liệu | Estimate | Create order |
| --- | --- | --- |
| Addons | `service_ids[].addons` | `addons` ở root request |
| `service_id` | `service_ids[].id` | `service_id` |
| `fee_id` | Chưa có | Bắt buộc |
| `service_type` | Nằm response service | Bắt buộc |
| COD point addon | DROP OFF object | DROP OFF object, giống estimate |

### Ví dụ request

```json
{
  "order_time": 1770350401,
  "service_id": 2099,
  "service_type": "EXPRESS-ONDEMAND",
  "fee_id": "d98fc496-492f-41a3-be80-3c2590345df6",
  "goods_value": 100000,
  "payment_method": "cash",
  "partner_trip_id": "HTM-ORDER-0001",
  "addons": [
    { "id": 4, "apply": true, "remain_cod": 100000, "value": 1 },
    { "id": 5, "apply": true }
  ],
  "points": [
    {
      "type": "PICK UP",
      "name": "Người gửi",
      "phone_number": "+84886901000",
      "latitude": 10.776889,
      "longitude": 106.700806,
      "short_address": "Vincom Đồng Khởi",
      "address": "72 Lê Thánh Tôn, Quận 1, TP. Hồ Chí Minh",
      "package_weight": 1,
      "package_size": "SMALL",
      "package_types": ["food"],
      "is_payment_check_point": true
    },
    {
      "type": "DROP OFF",
      "name": "Người nhận",
      "phone_number": "+84886901001",
      "latitude": 10.801937,
      "longitude": 106.712372,
      "short_address": "Landmark 81",
      "address": "208 Nguyễn Hữu Cảnh, Bình Thạnh, TP. Hồ Chí Minh",
      "is_payment_check_point": false,
      "addons": {
        "collect_cod": 100000,
        "currency_code": "VND"
      }
    }
  ]
}
```

### Người trả phí ship khi `payment_method = cash`

- Người gửi trả: `PICK UP.is_payment_check_point = true`, các point còn lại `false`.
- Người nhận trả: `DROP OFF.is_payment_check_point = true`, các point còn lại `false`.
- Hoặc bỏ toàn bộ field này để Xanh SM áp dụng default payer.

Nếu đã gửi field này, backend yêu cầu mọi point đều phải có field và chỉ một point là `true`.

### Response cần lưu

```json
{
  "isSuccess": true,
  "value": {
    "id": "01KRACXTPBPQJ8J5N3SSX1QA1H",
    "partner_trip_id": "HTM-ORDER-0001",
    "status": "FINDING",
    "order_time": "1770350401",
    "path": [],
    "geofence_id": ""
  }
}
```

FE/backend cần lưu `value.id` làm `xanhSmOrderId`. Đây không phải `partner_trip_id`.

---

## 6. Flow 4 — Hủy đơn

### Khi dùng

Chỉ dùng khi cần hủy đơn đã tạo. Xanh SM quyết định đơn còn hủy được hay không theo trạng thái thực tế.

### Endpoint

```http
POST /api/v1/xanh-sm/orders/cancel
```

### Request

```json
{
  "order_id": "01KRACXTPBPQJ8J5N3SSX1QA1H",
  "cancel_code": 9000,
  "cancel_message": "Khách hàng thay đổi nhu cầu"
}
```

### Validation

- `order_id` bắt buộc.
- `cancel_code` phải là `9000`.
- `cancel_message` không được rỗng hoặc chỉ chứa khoảng trắng.

### Response

```json
{
  "isSuccess": true,
  "value": {
    "message": "ok"
  }
}
```

Chỉ cập nhật trạng thái local sang đã hủy sau khi API này trả thành công.

---

## 7. Chức năng của từng cụm code

### 7.1. `HoaTheoMua.API/Controllers/XanhSmController.cs`

Đây là lớp nhận HTTP request từ FE.

| Hàm | Chức năng |
| --- | --- |
| `GetAvailableServices` | Nhận tọa độ, gọi service tìm dịch vụ. |
| `EstimateOrder` | Nhận tuyến đường/service/addon, gọi service báo giá. |
| `CreateOrder` | Nhận fee_id và dữ liệu đã estimate, gọi service tạo đơn. |
| `CancelOrder` | Nhận order id/lý do, gọi service hủy đơn. |

Controller không tự viết logic OAuth, validation hay HTTP upstream. Nó chỉ điều phối request và bọc response bằng `ApiResponseFactory.Base`.

### 7.2. `HoaTheoMua.Service/XanhSmService/Request.cs`

Chứa DTO frontend gửi lên. `[JsonPropertyName]` rất quan trọng:

```csharp
[JsonPropertyName("fee_id")]
public string FeeId { get; init; }
```

Property C# là `FeeId`, nhưng JSON gửi Xanh SM là `fee_id`. Nếu bỏ attribute này, ASP.NET có thể serialize thành `feeId` và Xanh SM từ chối request.

`CreateXanhSmOrderRequest` được đặt tên riêng để tránh Swagger trùng với `Order.Request.CreateOrderRequest` của module đặt hàng hiện tại. Tên C# này không làm thay đổi JSON FE gửi.

### 7.3. `HoaTheoMua.Service/XanhSmService/Response.cs`

Chứa DTO response từ Xanh SM. Backend chỉ khai báo những field FE cần như service id, fee id, total fee, order id. Field mới Xanh SM bổ sung sẽ được `System.Text.Json` tự bỏ qua nên không làm hỏng API.

### 7.4. `HoaTheoMua.Service/XanhSmService/XanhSmTokenProvider.cs`

Quản lý access token OAuth.

```text
GetAccessToken
  -> kiểm tra cấu hình Client ID/Secret/scope
  -> kiểm tra memory cache
  -> cache còn hạn: trả token ngay
  -> cache trống: lock RefreshLock
  -> request OAuth token endpoint
  -> cache token đến expires_in trừ buffer
```

`RefreshLock` ngăn 100 request đồng thời cùng gọi token endpoint khi token vừa hết hạn. Một request tạo token; request còn lại chờ rồi dùng token cache.

`Invalidate()` chỉ xóa cache token khi API nghiệp vụ GSM trả 401. Sau đó `Service` lấy token mới và retry đúng một lần.

### 7.5. `HoaTheoMua.Service/XanhSmService/Service.cs`

Đây là core proxy service.

| Hàm | Chức năng |
| --- | --- |
| `GetAvailableServices` | Gọi shared POST logic với path `/list-available-services`. |
| `EstimateOrder` | Gọi shared POST logic với path `/estimate/order`. |
| `CreateOrder` | Gọi shared POST logic với path `/create-order`. |
| `CancelOrder` | Gọi shared POST logic với path `/cancel-order`. |
| `PostAsync` | Lấy token, gửi request, retry một lần nếu 401, xử lý timeout/network. |
| `BuildPostRequest` | Tạo request mới với header `Authorization: Bearer` và `Accept-Language`. |
| `ReadResponse` | Đọc body, deserialize success response hoặc chuyển error response thành exception. |
| `ToPartnerException` | Đổi lỗi GSM thành HTTP status/message code ổn định cho FE. |

Lý do retry chỉ một lần: `HttpRequestMessage` không gửi lại được và retry vô hạn có thể tạo trùng delivery order.

### 7.6. `HoaTheoMua.Service/XanhSmService/Validations/`

`ValidationActionFilter` tự chạy FluentValidation trước khi controller gọi service. Nếu không hợp lệ, backend trả HTTP 422 và không gọi GSM.

| File | Chức năng |
| --- | --- |
| `AvailableServicesRequestValidator.cs` | Kiểm tra pickup, tọa độ, service type, order time. |
| `EstimateOrderRequestValidator.cs` | Kiểm tra service ids, payment method, point và addons của estimate. |
| `CreateOrderRequestValidator.cs` | Kiểm tra fee id, service, goods value, partner trip id, COD và người trả phí. |
| `CancelOrderRequestValidator.cs` | Kiểm tra order id, cancel code, cancel message. |
| `XanhSmValidationRules.cs` | Rule dùng chung cho route point, package, COD, bulky và door-to-door. |

### 7.7. `GlobalExceptionHandlerMiddleware`

Mọi `XanhSmException` được middleware chuyển thành error envelope. Những message này do backend tự tạo nên an toàn để FE đọc; backend không trả ClientSecret, access token hoặc raw body OAuth.

---

## 8. Mã lỗi frontend cần xử lý

### Lỗi OAuth/token

| `messageCode` | Ý nghĩa | Hành động |
| --- | --- | --- |
| `XANH_SM_CREDENTIAL_NOT_CONFIGURED` | Server thiếu Client ID/Secret/scope/Token URL. | Báo đội backend/devops. |
| `XANH_SM_TOKEN_BAD_REQUEST` | Sai grant type, Client ID hoặc scope. | Kiểm tra config GSM. |
| `XANH_SM_TOKEN_UNAUTHORIZED` | Client ID/Secret bị từ chối. | Kiểm tra credential sandbox. |
| `XANH_SM_TOKEN_FORBIDDEN` | GSM chặn token, thường IP chưa whitelist hoặc scope chưa cấp. | Gửi IP public cho GSM whitelist. |
| `XANH_SM_TOKEN_RATE_LIMITED` | GSM rate limit token request. | Chờ và thử lại sau. |
| `XANH_SM_TOKEN_PROVIDER_UNAVAILABLE` | GSM token endpoint 5xx. | Thử lại sau. |
| `XANH_SM_TOKEN_INVALID_RESPONSE` | GSM trả token response thiếu/sai. | Báo backend/GSM. |

### Lỗi API nghiệp vụ

| `messageCode` | Ý nghĩa |
| --- | --- |
| `XANH_SM_API_TOKEN_REJECTED` | Token bị GSM từ chối; backend đã thử refresh một lần. |
| `XANH_SM_API_TOKEN_REJECTED_AFTER_REFRESH` | Token mới vẫn bị từ chối; cần kiểm tra credential/scope. |
| `XANH_SM_API_FORBIDDEN` | Scope, sandbox hoặc IP không có quyền gọi endpoint. |
| `XANH_SM_<code>` | Code validation cụ thể do GSM trả, ví dụ `XANH_SM_4079`. |
| `XANH_SM_TIMEOUT` | GSM không phản hồi trước timeout cấu hình. |
| `XANH_SM_UNAVAILABLE` | Không kết nối được GSM. |

---

## 9. Checklist test thủ công sandbox

1. GSM đã whitelist IP `171.243.48.81`.
2. Server có `ClientSecret` sandbox thật.
3. Gọi `available-services` với pickup hợp lệ.
4. Chọn một service có `during_service = true`.
5. Gọi estimate; kiểm tra có `fee_id`, `expires_at`, `total_fee`.
6. Dùng **đúng** fee id/order time/points/addons từ estimate để create order.
7. Lưu `value.id` của create response.
8. Chỉ test cancel nếu đơn vẫn cho phép hủy.
9. Kiểm tra log không xuất hiện ClientSecret hoặc access token.

---

## 10. Lệnh kiểm tra chất lượng

```powershell
dotnet build HoaTheoMua.sln -c Release
dotnet test HoaTheoMua.sln -c Release
```

Test hiện có kiểm tra serialization snake_case, header Bearer, placement addons, mapping lỗi GSM 403, cache token và COD amount mismatch.
