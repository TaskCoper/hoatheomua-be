#### Request
```Json
Query param: id của order
```
![[../Store-Image/Pasted image 20260624022823.png]]
#### Response
```json
{
  "value": "Pending",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000003",
  "timestampUtc": "2026-06-23T19:18:33.2517535Z"
}
```
#### Flow chart


![[Pasted image 20260625113547.png]]

#### Test case
**Test case 01: Lấy trạng thái order thành công**
- **Điều kiện:** User đã đăng nhập, user tồn tại và `IsVerify = true`. Order tồn tại và thuộc về user.
- **Kết quả:** Hệ thống trả về `order.Status`.
**Test case 02: Không tìm thấy UserId trong token**
- **Điều kiện:** Request không có claim `UserId`.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.
**Test case 03: UserId sai định dạng Guid** -> chưa xử lý
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống ném lỗi khi gọi `Guid.Parse(user)`.
**Test case 04: User không tồn tại hoặc chưa verify**
- **Điều kiện:** User không tồn tại hoặc `IsVerify = false`.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `User not found`.
**Test case 05: Order không tồn tại**
- **Điều kiện:** Không có order nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order ID {id} not found`.
**Test case 06: Order thuộc user khác**
- **Điều kiện:** Order tồn tại nhưng `UserId` khác user đang đăng nhập.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Order ID {id} not found`.