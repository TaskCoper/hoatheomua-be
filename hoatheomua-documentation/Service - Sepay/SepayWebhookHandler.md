#### Request
```Json
{
  "gateway": "string",
  "transactionDate": "string",
  "accountNumber": "string",
  "subAccount": "string",
  "code": "string",
  "content": "string",
  "transferType": "string",
  "description": "string",
  "transferAmount": 0,
  "referenceCode": "string",
  "accumulated": 0,
  "id": 0
}
```
#### Response
```Json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000003",
  "timestampUtc": "2026-06-23T19:18:33.2517535Z"
}
```
#### Flow chart

![[../Store-Image/SepayWebhookHandler.png]]

#### Test case
**Test case 01: Xử lý webhook thành công với orderId trong Content**  
- **Điều kiện:** `request.Content` chứa mã `HTM-{orderId}` hợp lệ. Order tồn tại, có `Status = "Pending"`, `TransferAmount = order.TotalFee`. Ahamove API và NhanhVn API đều trả về thành công.  
- **Kết quả:** Hệ thống cập nhật order thành `Paid`, lưu Ahamove order id, Ahamove tracking URL, NhanhVn order id, NhanhVn tracking URL, tạo transaction và gọi `SaveChangesAsync`.  
**Test case 02: Xử lý webhook thành công với orderId trong Code**  
- **Điều kiện:** `request.Content` không có mã hợp lệ nhưng `request.Code` chứa `HTM-{orderId}` hợp lệ.  
- **Kết quả:** Hệ thống vẫn tìm được order và xử lý thanh toán thành công.  
**Test case 03: Xử lý webhook thành công với mã bị cắt ngắn**  
- **Điều kiện:** `request.Content` hoặc `request.Code` chứa prefix GUID bị cắt ngắn, độ dài từ `24` đến `31` ký tự hex. Database chỉ có một order match prefix đó.  
- **Kết quả:** Hệ thống tìm được order theo prefix và xử lý thanh toán.  
**Test case 04: Không tìm thấy mã thanh toán hợp lệ**  
- **Điều kiện:** `request.Content` và `request.Code` đều không có mã `HTM` hợp lệ.  
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Invalid description format`.  
**Test case 05: Prefix match nhiều order**  
- **Điều kiện:** Mã bị cắt ngắn match nhiều hơn 1 order trong database.  
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `Ambiguous payment code`.  
**Test case 06: Không tìm thấy order trong database**  
- **Điều kiện:** Extract được order id hoặc prefix nhưng database không có order tương ứng.  
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not found in description`.  
**Test case 07: Order không ở trạng thái Pending**  
- **Điều kiện:** Order tồn tại nhưng `Status != "Pending"`.  
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not on Pending`.  
**Test case 08: Số tiền chuyển khoản không khớp**  
- **Điều kiện:** `request.TransferAmount != order.TotalFee`.  
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `Transfer amount must be equal to transfer amount`.  
**Test case 09: Ahamove API trả về lỗi HTTP**  
- **Điều kiện:** Khi gọi API Ahamove `/v3/orders`, response không thành công.  
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Ahamove API HTTP Error ({StatusCode})`.  
**Test case 10: Ahamove response null**  
- **Điều kiện:** Ahamove API trả về HTTP success nhưng deserialize ra `createAhamoveOrderResult = null`.  
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Ahamove returned null or empty data`.  
**Test case 11: Cập nhật thông tin Ahamove thành công**  
- **Điều kiện:** Ahamove trả về `OrderId` và `SharedLink`.  
- **Kết quả:** Order được cập nhật:  
    - `AhahaMoveOrderId = createAhamoveOrderResult.OrderId`  
    - `AhahaMoveTrackingUrl = createAhamoveOrderResult.SharedLink`  
    - `UpdatedAt = DateTimeOffset.UtcNow`  
**Test case 12: NhanhVn API trả về lỗi HTTP**  
- **Điều kiện:** Khi gọi API NhanhVn `/order/add`, response không thành công.  
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVn API HTTP Error ({StatusCode})`.  
**Test case 13: NhanhVn response có Data null**  
- **Điều kiện:** NhanhVn API trả về HTTP success, `Code = 1` nhưng `Data = null` (bị coi là tạo đơn thất bại).  
- **Kết quả:** Hệ thống ném lỗi với message chứa `NhanhVn order creation failed`.  
**Test case 14: NhanhVn trả về Code = 0**  
- **Điều kiện:** Response từ NhanhVn có `Code = 0`.  
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVn order creation failed: {nhanhRawContent}`.  
**Test case 15: Cập nhật thông tin NhanhVn thành công**  
- **Điều kiện:** NhanhVn trả về `Data.Id` và `Data.TrackingUrl`.  
- **Kết quả:** Order được cập nhật:  
    - `NhanhOrderId = createOrderNhanhResult.Data.Id`  
    - `NhanhTrackingUrl = createOrderNhanhResult.Data.TrackingUrl`  
**Test case 16: Tạo transaction thành công**  
- **Điều kiện:** Webhook xử lý thành công.  
- **Kết quả:** Hệ thống tạo `Transaction` mới với các field:  
    - `SePayId`  
    - `Gateway`  
    - `TransactionDate`  
    - `AccountNumber`  
    - `SubAccount`  
    - `Code`  
    - `Content`  
    - `TransferType`  
    - `Description`  
    - `TransferAmount`  
    - `Accumulated`  
    - `ReferenceCode`  
    - `OrderId`  
    - `CreatedAt`  
  
**Test case 19: Mapping OrderDetails sang AhamoveItem đúng**  
- **Điều kiện:** Order có nhiều `OrderDetails`.  
- **Kết quả:** Hệ thống tạo danh sách `AhamoveItem` với:  
    - `Id = OrderDetail.Id.ToString()`  
    - `Name = OrderDetail.Name`  
    - `Price = OrderDetail.Price`  
    - `Num = OrderDetail.Number`  
**Test case 20: Mapping OrderDetails sang PackageDetail đúng**  
- **Điều kiện:** Order có nhiều `OrderDetails`.  
- **Kết quả:** Hệ thống tạo danh sách `PackageDetail` với:  
    - `Height`  
    - `Weight`  
    - `Length`  
    - `Width`  
    - `Description = "Mo ta"`  
**Test case 21: Mapping OrderDetails sang NhanhVn Product đúng**  
- **Điều kiện:** Order có nhiều `OrderDetails`.  
- **Kết quả:** Hệ thống tạo danh sách product gửi NhanhVn với:  
    - `Id = OrderDetail.ItemId`  
    - `Price = OrderDetail.Price`  
    - `Quantity = OrderDetail.Number`  
**Test case 22: Order không có OrderDetails**  
- **Điều kiện:** Order tồn tại nhưng `OrderDetails` rỗng.  
- **Kết quả:** Code hiện tại vẫn tạo request với danh sách item rỗng và tiếp tục gọi Ahamove/NhanhVn. Nên bổ sung validation nếu business không cho phép order rỗng.  
**Test case 24: Webhook bị gửi lại lần 2**  
- **Điều kiện:** Order đã được xử lý trước đó và có `Status = "Paid"`.  
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not on Pending`.