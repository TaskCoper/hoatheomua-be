#### Request
```CSharp
{  
"id": "a6a27c96-6735-4da0-b809-020c68f8260f"  
}
```
#### Response
```CSharp
{
  "value": {
    "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
    "key": "string",
    "value": "string",
    "userId": "528b7881-e6bb-4c83-ad31-bee1a8551525",
    "isPublic": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67L0:00000003",
  "timestampUtc": "2026-06-23T09:39:59.772033Z"
}
```
#### Flow diagram


![[../Store-Image/GetConfigById.drawio.png]]

#### Test case

**Test case 01: Lấy chi tiết config thành công**
- **Điều kiện:** User đã đăng nhập, `GetRequiredUserId()` hợp lệ. Config có `Id` hợp lệ và `IsDeleted = false`, `Kind = Content`.
- **Kết quả:** Hệ thống trả về `ConfigResponse` với đúng `Key` và `Value` (không bị che).

**Test case 02: Id config không tồn tại**
- **Điều kiện:** Không có config nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Id Not Found`.

**Test case 03: User chưa đăng nhập / không xác thực**
- **Điều kiện:** `GetRequiredUserId()` ném `UnauthorizedException` (token không có `UserId`).
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.

**Test case 04: Config có Kind = Secret**
- **Điều kiện:** Config hợp lệ nhưng `Kind = Secret`.
- **Kết quả:** Hệ thống trả về `ConfigResponse` với trường `value` bị che (`••••`).

