#### Request
```Json
Query param: id của order
```
#### Response
```json
{
  "value": "Hủy đơn thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000004",
  "timestampUtc": "2026-06-23T19:18:33.2517535Z"
}
```
#### Flow chart
![[../Store-Image/CancelOrder.png]]
#### Test case
**Test case 01: Hủy đơn thành công**
- **Điều kiện:** Order tồn tại và có `Status = "Pending"`.
- **Kết quả:** Hệ thống đổi `Status = "Cancelled"`, gán `UpdatedAt`, gọi `SaveChangesAsync` một lần và trả về message `Hủy đơn thành công`.
**Test case 02: Order không tồn tại**
- **Điều kiện:** Không có order nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Order not found`.
**Test case 03: Order tồn tại nhưng status khác "Pending"**
- **Điều kiện:** Order tồn tại nhưng `Status != "Pending"` (ví dụ `Cancelled`). Query lọc theo `Status == "Pending"` nên coi như không tìm thấy.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Order not found` và không gọi `SaveChangesAsync`.
