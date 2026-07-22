#### Request
```Json
Query param: id của order
```
![[../Store-Image/Pasted image 20260624022835.png]]
#### Response
```json
{
  "value": {
    "orderId": "ddba561c-e7c1-4375-8ca6-49a2c30f91a1",
    "status": "Pending",
    "trackNumber": "",
    "ahahaMoveOrderId": null,
    "ahahaMoveTrackingUrl": null,
    "remark": "TEST",
    "orderTime": 1782234000,
    "qrCodeUrl": "https://qr.sepay.vn/img?acc=96247BENTRAN&bank=BIDV&amount=782000&des=HTM-ddba561c-e7c1-4375-8ca6-49a2c30f91a1&template=qronly",
    "transaction": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": null,
  "timestampUtc": "2026-06-23T19:17:47.1051207Z"
}
```
#### Flow chart

![[../Store-Image/Pasted image 20260625113425.png]]

#### Test case
**Test case 01: Lấy chi tiết order thành công**
- **Điều kiện:** User đã đăng nhập. Order tồn tại, thuộc về user hiện tại và `IsDeleted = false`.
- **Kết quả:** Hệ thống trả về `BaseResponse` chứa thông tin chi tiết order.
**Test case 02: Không tìm thấy UserId trong token**
- **Điều kiện:** Request không có claim `UserId`.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.
**Test case 03: UserId sai định dạng Guid** -> chưa xử lý
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống ném lỗi khi gọi `Guid.Parse(user)`.
**Test case 04: Order không tồn tại**
- **Điều kiện:** Không có order nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not found`.
**Test case 05: Order thuộc user khác**
- **Điều kiện:** Order tồn tại nhưng `UserId` của order khác user đang đăng nhập.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not found`.
**Test case 06: Order đã bị xóa mềm**
- **Điều kiện:** Order có `IsDeleted = true`.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order not found`.
**Test case 07: Tạo QR code URL chính xác**
- **Điều kiện:** Order tồn tại và có `TotalFee`.
- **Kết quả:** Hệ thống tạo `QrCodeUrl` với:
    - `bank = BIDV`
    - `acc = 96247BENTRAN`
    - `amount = orders.TotalFee`
    - `des = HTM-{orderId}`
**Test case 08: Order chưa có transaction**
- **Điều kiện:** Order tồn tại nhưng không có transaction.
- **Kết quả:** Response trả về `Transaction = null`.
**Test case 09: Order có nhiều transaction**
- **Điều kiện:** Order có nhiều transaction với `CreatedAt` khác nhau.
- **Kết quả:** Hệ thống lấy transaction mới nhất theo `CreatedAt` giảm dần.