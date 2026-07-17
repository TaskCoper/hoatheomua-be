#### Request
```CSharp
{  
"id": "a6a27c96-6735-4da0-b809-020c68f8260f"  
}
```
#### Response
```CSharp

{
  "value": "Xóa thành công.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67L3:00000001",
  "timestampUtc": "2026-06-23T10:08:19.120086Z"
}

```
#### Flow diagram

![[../Store-Image/DeleteConfig.png]]

#### Test case

**Test case 01: Xóa config thành công**
- **Điều kiện:** Config có `Id` hợp lệ và `IsDeleted = false`.
- **Kết quả:** Hệ thống cập nhật `IsDeleted = true`, gọi `SaveChangesAsync` đúng một lần và trả về message `Xóa thành công.`

**Test case 02: Id config không tồn tại**
- **Điều kiện:** Không có config nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Id Not Found`.

**Test case 03: Config đã bị xóa mềm trước đó**
- **Điều kiện:** Config có tồn tại nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Id Not Found`.

**Test case 04: Id truyền vào là Guid.Empty**
- **Điều kiện:** Request truyền `Id = Guid.Empty` (không khớp config nào).
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Id Not Found`.

