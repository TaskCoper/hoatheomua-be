#### Request
```CSharp
{
  "key": "quan123",
  "value": "quanquan",
  "isPublic": true
}
```
#### Response
```CSharp

{
  "value": {
    "id": "be6ef091-f3a7-4904-a913-75a34594ff59",
    "key": "quan123",
    "value": "quanquan",
    "userId": "528b7881-e6bb-4c83-ad31-bee1a8551525",
    "isPublic": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67L1:00000004",
  "timestampUtc": "2026-06-23T09:44:05.583757Z"
}

```
#### Flow diagram
![[../Store-Image/CreateConfig.png]]

#### Test case

**Test case 01: Tạo config thành công**
- **Điều kiện:** User đã đăng nhập, `GetRequiredUserId()` trả về `UserId` hợp lệ. `Key` chưa tồn tại trong bảng `Configs`. `Group` khớp với thuộc tính `group` trong `Value`.
- **Kết quả:** Hệ thống tạo config mới, gọi `SaveChangesAsync` đúng một lần và trả về `ConfigResponse` với đúng `Key`, `Group`, `UserId`.

**Test case 02: Key đã tồn tại**
- **Điều kiện:** Trong database đã có config cùng `Key` và `IsDeleted = false`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Key is exists`.

**Test case 03: Group không khớp với group trong Value**
- **Điều kiện:** `request.Group = "web"` nhưng trong `Value` có `"group": "other"`.
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `group doesn't match`.

**Test case 04: User chưa đăng nhập / không xác thực**
- **Điều kiện:** `GetRequiredUserId()` ném `UnauthorizedException` (token không có `UserId`).
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.





