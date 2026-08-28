# Những vị trí gọi API Nhanh.vn

## 1. Khi khách hàng tạo đơn hàng

Endpoint: `POST /api/v1/orders`

Hàm xử lý: `Order.Service.CreateOrder`

Một lần tạo đơn gọi Nhanh.vn như sau:

1. Gọi `product/list` một lần để lấy giá, cân nặng và kích thước của các sản phẩm khách đặt.
2. Gọi `product/detail` nhiều lần để kiểm tra tồn kho:
   - Sản phẩm thường được kiểm tra trực tiếp.
   - Combo được kiểm tra theo từng vật liệu core.
   - Các mã trùng nhau được gom lại trước khi gọi.

Tổng số lượt gọi Nhanh.vn:

```text
1 lần product/list + N lần product/detail
```

`N` là số `ItemId` riêng biệt sau khi gom sản phẩm thường và vật liệu core của combo.

Ví dụ sau khi gom có 6 mã cần kiểm kho thì một lần tạo đơn gọi Nhanh.vn tổng cộng 7 lần. Đây là vị trí dễ chạm rate limit nhất.

## 2. Khi FE gọi trực tiếp API sản phẩm Nhanh.vn

### Lấy danh sách sản phẩm

- Endpoint hệ thống: `POST /api/v1/products/nhanh-vn/list`
- Hàm xử lý: `Product.Service.GetListProducts`
- API Nhanh.vn: `product/list`
- Số lượt: 1 lần cho mỗi request từ FE.

### Lấy chi tiết và tồn kho sản phẩm

- Endpoint hệ thống: `POST /api/v1/products/nhanh-vn/details`
- Hàm xử lý: `Product.Service.GetProductDetails`
- API Nhanh.vn: `product/detail`
- Số lượt: 1 lần cho mỗi request từ FE.

`GetProductDetails` còn được gọi bên trong vòng lặp kiểm kho của `CreateOrder`, vì vậy số lượt thực tế không chỉ đến từ FE.

## 3. Khi FE yêu cầu tính phí giao hàng

Endpoint: `POST /api/v1/ahamove/estimate`

Hàm xử lý: `AhamoveService.EstimateShippingFee`

Trước khi tính phí, hàm gọi Nhanh.vn `product/list` một lần để lấy giá, cân nặng và kích thước sản phẩm.

Số lượt gọi Nhanh.vn: 1 lần cho mỗi request.

## 4. Khi hoàn tất một đơn đã thanh toán

Hàm xử lý: `Webhook.Service.FulfillPaidOrder`

Hàm gọi Nhanh.vn `order/add` để tạo đơn hàng trên Nhanh.vn.

Số lượt gọi Nhanh.vn: 1 lần cho mỗi đơn được hoàn tất thành công.

## 5. Các endpoint khác gọi Nhanh.vn một lần

- `POST api/v1/nhanh-vn/orders-tags` gọi `order/tags` để lấy tag đơn hàng.
- `POST api/v1/nhanh-vn/orders-sources` gọi `order/source` để lấy nguồn đơn hàng.
- `POST /api/v1/nhanh-vn/depot` gọi `business/depot` để lấy thông tin kho.
- `POST /api/v1/orders/nhanh-vn` gọi `order/list` để lấy danh sách đơn hàng.
- `POST /api/v1/orders/shipping-location` gọi `shipping/location` để lấy dữ liệu địa chỉ giao hàng.

Mỗi endpoint trên gọi Nhanh.vn một lần cho mỗi request.
