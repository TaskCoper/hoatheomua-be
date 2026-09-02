# Kế hoạch tích hợp Xanh SM

## Trạng thái

- XanhSM Express client, token cache và cấu hình Sandbox/Production đã được triển khai theo tài liệu chính thức.
- `CreateOrderRequest` đã hỗ trợ `ShippingGateway.XanhSm` và báo giá bằng `partner_pay`, Door-to-door.
- Payment webhook estimate lại để lấy `fee_id` còn hiệu lực, booking XanhSM và lưu `ShippingPartyOrderId`/`ShippingTrackingUrl`.
- Door-to-door addon ID được lấy động từ `list-available-services` theo `group_name = DOOR2DOOR` và được giữ nguyên qua Estimate/Book; hệ thống không có ID cấu hình fallback.
- Điểm lấy hàng gửi đủ `package_weight`, `package_size`, `package_types`. Cân nặng được đổi từ tổng gram Nhanh.vn sang kg và làm tròn lên. Không dùng package metadata từ `list-available-services` để reject request vì sandbox partner service có thể trả các field này bằng `null`; `estimate/order` là nguồn validation cuối cùng.
- `ShippingPartyOrderId` và `ShippingTrackingUrl` là field dùng chung cho mọi đơn vị vận chuyển.

## Thông tin cần lấy từ Xanh SM

1. Sandbox/base URL production, cơ chế cấp và refresh credential, partner/merchant ID.
2. Contract báo giá, tạo đơn, lấy chi tiết/tracking, hủy đơn và danh sách mã dịch vụ.
3. Quy tắc idempotency khi tạo đơn và retry; giới hạn rate limit, timeout, SLA.
4. Contract webhook: event ID, trạng thái, chữ ký, timestamp chống replay, retry policy và IP allowlist.
5. Vùng phục vụ, giới hạn kích thước/khối lượng, đặt lịch, nhiều điểm giao, COD và cách làm tròn phí.
6. Quy tắc lấy tracking URL, ảnh/POD, thông tin tài xế và chính sách lưu dữ liệu cá nhân.

Không chốt tên endpoint hoặc DTO trước khi có tài liệu trên.

## Kiến trúc đích

Tách logic vận chuyển khỏi `Order.Service` và `Webhook.Service`:

```text
Order / payment webhook
        |
        v
IShippingGatewayResolver
        |
        +-- AhamoveShippingGateway
        |
        +-- XanhSmShippingGateway
```

Contract nội bộ đề xuất:

- `EstimateFeeAsync(ShippingQuoteRequest)` trả phí, service code, quote ID và thời hạn hiệu lực nếu provider có hỗ trợ.
- `CreateOrderAsync(ShippingCreateRequest)` trả `ShippingPartyOrderId`, `ShippingTrackingUrl` và trạng thái ban đầu.
- `CancelOrderAsync(...)` và `GetOrderAsync(...)` phục vụ hủy/đối soát.
- Mỗi adapter tự map DTO, auth, error code và trạng thái provider sang model chung; business flow không tham chiếu DTO Ahamove/Xanh SM.

## Các giai đoạn triển khai

### 1. Chuẩn hóa Ahamove trước

- Trích logic báo giá/tạo đơn Ahamove hiện nằm trong hai service sang `AhamoveShippingGateway`.
- Thêm resolver theo `ShippingGateway` và test để chứng minh hành vi Ahamove không đổi.
- Chỉ khi bước này pass mới thêm adapter Xanh SM, tránh tạo thêm nhánh `if/switch` rải rác.

### 2. Xây Xanh SM client

- Tạo `XanhSmOption`, typed `HttpClient`, token cache và cấu hình timeout.
- Retry chỉ với lỗi tạm thời (`408`, `429`, `5xx`) và chỉ khi request có idempotency key; không retry mù thao tác tạo đơn.
- Chuẩn hóa lỗi thành nhóm: authentication, validation, out-of-service-area, quote-expired, rate-limit, provider-unavailable.
- Không log credential, số điện thoại đầy đủ hoặc payload webhook chứa dữ liệu cá nhân.

### 3. Báo giá và checkout

- `EstimateShippingFeeAsync` chọn adapter theo `request.ShippingGateway`.
- Kiểm tra vùng phục vụ và giới hạn kiện hàng trước khi tạo payment order.
- Nếu Xanh SM yêu cầu quote ID/service code, thêm field typed vào order hoặc bảng quote có `ExpiresAt`; không lưu dữ liệu cần query trong JSON.
- Sau khi sandbox báo giá ổn định, bỏ rule `SHIPPING_GATEWAY_NOT_SUPPORTED` và bật qua feature flag `xanhsm.enabled`.

### 4. Fulfillment sau thanh toán

- `FulfillPaidOrder` gọi adapter đã chọn và lưu hai field dùng chung.
- Dùng `Order.Id` làm idempotency key với provider nếu contract hỗ trợ.
- Tách trạng thái thanh toán và trạng thái giao hàng; tối thiểu cần `ShippingStatus`, `ShippingProviderStatus`, `ShippingLastSyncedAt`.
- Nên đưa thao tác tạo vận đơn sang job/outbox có retry. Webhook thanh toán không nên gọi provider rồi mới `SaveChanges`, vì timeout có thể tạo vận đơn thành công bên provider nhưng hệ thống retry và tạo trùng.

### 5. Webhook Xanh SM và đối soát

- Tạo endpoint riêng cho webhook vận chuyển; xác minh chữ ký/timestamp trước khi đọc business payload.
- Lưu hoặc khóa theo provider event ID để xử lý idempotent.
- Map trạng thái provider sang state machine nội bộ; không ghi trực tiếp chuỗi trạng thái bên ngoài vào `Order.Status`.
- Có scheduled reconciliation cho đơn đang giao bị mất webhook.

### 6. Kiểm thử và rollout

- Unit test mapping request/response, status, error và chữ ký webhook.
- Contract test bằng payload chính thức; test timeout, retry, `429`, event trùng/thứ tự ngược và quote hết hạn.
- Sandbox E2E: báo giá → checkout → thanh toán → tạo vận đơn → webhook trạng thái → hủy/hoàn tất.
- Canary theo feature flag, metric riêng theo provider: quote/create latency, success rate, retry count, webhook lag, đơn thiếu tracking.
- Rollback bằng cách tắt `xanhsm.enabled`; các đơn Xanh SM đã tạo vẫn phải tiếp tục nhận webhook/đối soát.

## Điều kiện hoàn thành

- Không còn logic provider-specific trong order/payment flow ngoài adapter và webhook adapter.
- Không thể tạo hai vận đơn khi payment webhook hoặc job bị retry.
- Phí checkout khớp quote Xanh SM theo quy tắc làm tròn đã ký kết.
- Tạo/hủy/theo dõi đơn và toàn bộ transition trạng thái pass sandbox E2E.
- Ahamove regression test pass và có dashboard/alert cho cả hai provider.
