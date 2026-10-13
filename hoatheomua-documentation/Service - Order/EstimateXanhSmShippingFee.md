# Estimate XanhSM Shipping Fee

Trả phí giao hàng XanhSM tạm tính cho người mua. Backend tự lấy địa chỉ lấy hàng từ `CompanyAddress`, chỉ chọn dịch vụ xe máy `EXPRESS-ONDEMAND`, Door-to-door và `partner_pay`.

## Endpoint

`POST /api/v1/xanhsm/estimate`

Yêu cầu access token của khách hàng.

## Request

```json
{
  "latitude": 10.801937,
  "longitude": 106.712372,
  "address": "208 Nguyễn Hữu Cảnh, Phường 22, Bình Thạnh, TP.HCM",
  "shortAddress": "Landmark 81",
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567",
  "remarks": "Gọi trước khi giao",
  "orderTime": 0,
  "items": [
    {
      "itemId": 1833667,
      "productId": "00000000-0000-0000-0000-000000000000",
      "number": 2
    }
  ]
}
```

- `orderTime = 0`: backend sử dụng thời điểm hiện tại.
- `orderTime > 0`: Unix timestamp theo giây, không được quá 7 ngày trong tương lai.
- `items`: bắt buộc để backend lấy cân nặng sản phẩm từ Nhanh.vn và tính giống luồng tạo order. `productId` chưa được dùng khi chỉ báo giá.
- Điểm lấy hàng không nhận từ request; backend lấy từ địa chỉ công ty đang hoạt động.

## Response 200

```json
{
  "value": {
    "shippingFee": 33000,
    "currencyCode": "VND",
    "distance": 5.33,
    "distanceUnit": "km",
    "duration": 702,
    "orderTime": 1787986800,
    "expiresAt": 1787987100
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000001",
  "timestampUtc": "2026-08-29T10:00:00Z"
}
```

`shippingFee` chỉ là phí tạm tính. Khi tạo order, backend sẽ estimate lại để lấy `fee_id` mới và lưu phí chính thức vào order.

- Báo phí và tạo order cùng áp dụng cấu hình `ahamove.free_ship` (nhóm `ahamove`, đơn vị km, đúng với nhãn cấu hình quản trị). Ví dụ `{"value":5}` miễn phí khi khoảng cách ≤ 5 km, bao gồm đúng 5 km; vượt 5 km thì `shippingFee = (km thực tế − 5) × 5.000đ`, làm tròn tới đồng (9 km → 20.000đ, 12,5 km → 37.500đ).
- Backend giữ nguyên khoảng cách `distanceUnit = km` và chia cho 1000 khi đơn vị là `m`. Không có cấu hình hoặc giá trị ≤ 0 thì tính phí theo báo giá hãng.
- Nếu khoảng cách thiếu, âm hoặc đơn vị không được hỗ trợ, giữ phí hãng trả về. Chính sách miễn phí chỉ thay đổi phí người mua trả, không thay đổi báo giá hay phí thanh toán cho hãng vận chuyển.
- `distance`: khoảng cách giao hàng dự kiến do GreenSM trả về.
- `distanceUnit`: đơn vị khoảng cách; GreenSM hiện trả `km`.
- `duration`: thời gian giao hàng dự kiến theo giây.
- Ba field hành trình có thể là `null` nếu GreenSM không trả `estimate_info.trip`.

## Validation

- `latitude`, `longitude`: bắt buộc và nằm trong phạm vi tọa độ hợp lệ.
- `address`, `shortAddress`, `fullName`, `phone`: bắt buộc.
- `phone`: số điện thoại Việt Nam hợp lệ.
- `remarks`: tối đa 512 ký tự.
- `orderTime`: `0` hoặc thời điểm hợp lệ trong vòng 7 ngày.
- `items`: ít nhất một phần tử; `itemId` và `number` phải lớn hơn `0`, không lặp `itemId`.

## Luồng xử lý

1. Lấy địa chỉ công ty đang hoạt động làm điểm `PICK UP`.
2. Lấy `Shipping.Weight` của từng sản phẩm từ Nhanh.vn, nhân số lượng, cộng tổng gram, đổi sang kg và làm tròn lên; tối thiểu `1 kg`.
3. Gắn `package_weight`, `package_size = LARGE`, `package_types = [flowers]` vào điểm `PICK UP`; điểm `DROP OFF` không mang ba field này.
4. Gọi `list-available-services` và chọn xe máy có addon `DOOR2DOOR`.
5. Giữ nguyên `LARGE/flowers` khi gọi Estimate. Sandbox service `1878` trả `express_package_sizes` và `express_package_types` bằng `null` trong `list-available-services` nhưng `estimate/order` vẫn chấp nhận hai giá trị này, nên backend không dùng metadata discovery để phủ quyết request.
6. Giới hạn tối đa `30 kg` cho xe máy tiêu chuẩn vì hệ thống không dùng Bulky addon.
7. Lấy addon ID động từ response rồi gọi `estimate/order`.
8. Chỉ trả phí và thời hạn báo giá cho frontend.
