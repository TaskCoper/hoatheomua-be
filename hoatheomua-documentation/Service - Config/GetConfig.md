	#### Request
```CSharp
{  
"pageIndex": 1,  
"pageSize": 10  
}
```
#### Response
```CSharp
{
  "value": [
    {
      "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
      "key": "string",
      "value": "string",
      "userId": "528b7881-e6bb-4c83-ad31-bee1a8551525",
      "isPublic": false
    }
],
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Flow diagram
![[../Store-Image/GetConfig.drawio.png]]

#### Test case

**Test case 01: Lấy danh sách config kèm masking phù hợp theo Kind**
- **Điều kiện:** Database có config `Kind = Content` và config `Kind = Secret` (đều `IsDeleted = false`).
- **Kết quả:** Hệ thống trả về đủ các config đang hoạt động; config `Content` giữ nguyên `Value`, còn config `Secret` được che (`••••`) trong trường `value`.

**Test case 02: Database không có config nào**
- **Điều kiện:** Bảng `Configs` không có bản ghi nào.
- **Kết quả:** Hệ thống trả về danh sách rỗng `[]`.

**Test case 03: Chỉ lấy config chưa bị xóa mềm**
- **Điều kiện:** Config tồn tại nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống loại bỏ config đã xóa và trả về danh sách rỗng.

**Test case 04: Config Secret có Value là chuỗi rỗng**
- **Điều kiện:** Config `Kind = Secret` có `Value = ""`.
- **Kết quả:** Hệ thống không lỗi mà trả về một `JsonElement` kiểu Object rỗng cho `Value`.
