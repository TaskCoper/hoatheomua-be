#### Request
```json
{
  "title": "Công nghệ",
  "parentId": null
}
```
#### Response
```json
{
  "value": {
    "categoryId": "44444444-4444-4444-4444-444444444441",
    "title": "Công nghệ",
    "userId": "11111111-1111-1111-1111-111111111111",
    "parentId": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000001",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Service Flow (CreateCate)

![[../Store-Image/CreateCate.drawio (1).png]]
#### Test case
**Test case 01**: Tạo danh mục cha thành công  
**Điều kiện:** User đã đăng nhập với role Admin, title không trùng với danh mục đang active, parentId = null.  
**Kết quả:** Hệ thống tạo danh mục mới, trả về 201 Created kèm categoryId.

**Test case 02**: Tạo danh mục con thành công  
**Điều kiện:** parentId hợp lệ (danh mục cha là root, không có parent).  
**Kết quả:** Danh mục con được tạo, parentId được gán đúng.

**Test case 03**: Title bị trùng trong cùng parent  
**Điều kiện:** title đã tồn tại trong cùng parent và chưa bị xóa.  
**Kết quả:** Hệ thống ném ConflictException với message "CATEGORY_ALREADY_EXISTS".

**Test case 04**: Title rỗng  
**Điều kiện:** title = "" hoặc whitespace.  
**Kết quả:** Hệ thống ném BadRequestException với message "CATEGORY_TITLE_REQUIRED".

**Test case 05**: Parent không hợp lệ (parent là danh mục con)  
**Điều kiện:** parentId thuộc về danh mục đã có parent khác.  
**Kết quả:** Hệ thống ném BadRequestException với message "CATEGORY_PARENT_INVALID".

**Test case 06**: Parent không tồn tại  
**Điều kiện:** parentId không tồn tại trong DB.  
**Kết quả:** Hệ thống ném NotFoundException với message "CATEGORY_NOT_FOUND".

**Test case 07**: Title đã tồn tại nhưng ở parent khác  
**Điều kiện:** title giống với category khác nhưng khác parentId (hoặc một bên là root, một bên có parent).  
**Kết quả:** Tạo thành công vì chỉ kiểm tra duplicate trong cùng parent.

**Test case 08**: Title đã tồn tại trong cùng parent nhưng đã bị xóa mềm  
**Điều kiện:** title đã tồn tại trong cùng parent nhưng category cũ có IsDeleted = true.  
**Kết quả:** Tạo thành công vì chỉ check duplicate với category đang active (IsDeleted = false).

**Test case 10**: Title chỉ chứa khoảng trắng  
**Điều kiện:** title = " " (toàn whitespace).  
**Kết quả:** Hệ thống ném BadRequestException với message "CATEGORY_TITLE_REQUIRED".

==**Test case 11**: Title quá dài (vượt max column length)==  //chưa validate
**Điều kiện:** title > 256 ký tự (vượt độ dài cho phép của cột trong DB).  
**Kết quả:** Entity Framework throw DbUpdateException hoặc validation error.

