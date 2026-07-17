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
  "value": "Cập nhật thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67L2:00000008",
  "timestampUtc": "2026-06-23T10:01:27.146889Z"
}
```
#### Flow diagram

![[../Store-Image/UpdateConfig.drawio.png]]

#### Test case

**Test case 01: Cập nhật config thành công**
- **Điều kiện:** Config có `Id` hợp lệ, `IsDeleted = false`. `Group` khớp với `group` trong `Value` và `Kind` không thay đổi.
- **Kết quả:** Hệ thống cập nhật config, gọi `SaveChangesAsync` đúng một lần và trả về message `Cập nhật thành công`.

**Test case 02: Group không khớp với group trong Value**
- **Điều kiện:** `request.Group = "web"` nhưng trong `Value` có `"group": "other"`.
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `group doesn't match`.

**Test case 03: Key mới trùng với key của config khác**
- **Điều kiện:** Request đổi `Key` sang một giá trị đã được config khác sử dụng (`IsDeleted = false`).
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Key is exists`.

**Test case 04: Id config không tồn tại**
- **Điều kiện:** Không có config nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Id/Key Not Found`.

**Test case 05: Thay đổi Kind của config**
- **Điều kiện:** Config hiện tại có `Kind = Content`, request truyền `Kind = Setting`.
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `ConfigKindImmutable` (Kind không được phép đổi).

