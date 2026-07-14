#### Request
```Json
{
  "latitude": 0,
  "longitude": 0,
  "ward": 0,
  "city": 0,
  "address": "string",
  "shortAddress": "string",
  "fullName": "string",
  "phone": "string",
  "remarks": "string",
  "orderTime": 0,
  "items": [
    {
      "itemId": 0,
      "number": 0
    }
  ]
}
```
#### Response
```json
{
  "value": "ddba561c-e7c1-4375-8ca6-49a2c30f91a1",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000001",
  "timestampUtc": "2026-06-23T19:16:43.2840128Z"
}
```
#### Flow chart

![[../Store-Image/Pasted image 20260625113408.png]]

#### Test case
**Test case 01: Tạo order thành công**
- **Điều kiện:** User đã đăng nhập, user tồn tại và `IsVerify = true`. NhanhVn trả về danh sách sản phẩm hợp lệ. Sản phẩm còn đủ tồn kho tại depot `129255`. Ahamove trả về phí ship hợp lệ.
- **Kết quả:** Hệ thống tạo `Order`, tạo `OrderDetails`, lưu database và trả về `newOrder.Id`.
**Test case 02: Không tìm thấy UserId trong token**
- **Điều kiện:** Request không có claim `UserId`.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.
**Test case 03: UserId sai định dạng Guid**
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống ném lỗi khi gọi `Guid.Parse(user)`.
**Test case 04: User chưa verify**
- **Điều kiện:** UserId hợp lệ nhưng `IsVerified = false`.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User is not verified`.
**Test case 05: NhanhVn API trả về lỗi HTTP**
- **Điều kiện:** Gọi API `/product/list` của NhanhVn thất bại.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVn API HTTP Error`.
**Test case 06: NhanhVn response null hoặc không có Data**
- **Điều kiện:** API NhanhVn trả về response null hoặc `Data = null`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVnResponse returned null or empty data`.
**Test case 07: Sản phẩm không có dữ liệu tồn kho**
- **Điều kiện:** Khi gọi `GetProductDetails`, response không có `Data.Inventory.Depots` hoặc danh sách depot rỗng.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Sản phẩm Id={itemId} không có dữ liệu tồn kho`.
**Test case 08: Sản phẩm không có tồn kho tại depot 129255**
- **Điều kiện:** Danh sách depot không có depot có `Id = 129255`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Sản phẩm Id={itemId} không có dữ liệu tồn kho tại depot 129255`.
**Test case 09: Số lượng mua vượt quá tồn kho**
- **Điều kiện:** `item.Number > available`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Không đủ hàng`.
**Test case 10: Ahamove API trả về lỗi HTTP**
- **Điều kiện:** Gọi API `/v3/orders/estimates` của Ahamove thất bại.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Ahamove API HTTP Error`.
**Test case 11: Ahamove response null hoặc rỗng**
- **Điều kiện:** Ahamove trả về `null` hoặc danh sách rỗng.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Ahamove returned null or empty data`.
**Test case 12: Tính tổng tiền order chính xác**
- **Điều kiện:** NhanhVn trả về giá sản phẩm, Ahamove trả về phí ship.
- **Kết quả:** Hệ thống tính:
    - `ShippingFee = ahamoveResult[0].Data.TotalPrice`
    - `ProductFree = tổng giá sản phẩm`
    - `TotalFee = ProductFree + ShippingFee`
**Test case 13: Order mới có status Pending**
- **Điều kiện:** Tạo order thành công.
- **Kết quả:** Order được lưu với `Status = "Pending"`.
**Test case 14: OrderDetails được tạo đúng**
- **Điều kiện:** Tạo order với nhiều item.
- **Kết quả:** Hệ thống tạo `OrderDetails` tương ứng với từng sản phẩm, gồm `ItemId`, `Name`, `Price`, `Number`, `Height`, `Weight`, `Length`, `Width`.

