#### Request
```json
{
  "title": "Công Nghệ Mới",
  "parentId": null
}
```
#### Response
```json
{
  "value": {
    "categoryId": "44444444-4444-4444-4444-444444444441",
    "title": "Công Nghệ Mới",
    "userId": "11111111-1111-1111-1111-111111111111",
    "parentId": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000004",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Service Flow (UpdateCate)
![[../Store-Image/UpdateCate.drawio (1).png]]
#### Test case
**Test case 01**: Cập nhật danh mục thành công  
**Điều kiện:** Category tồn tại, title mới không trùng, quyền Admin.  
**Kết quả:** Cập nhật title và parentId, trả về thông tin mới.

**Test case 02**: Title bị trùng  
**Điều kiện:** Title mới đã tồn tại trong cùng parent (trừ chính nó).  
**Kết quả:** Hệ thống ném ConflictException với message "CATEGORY_ALREADY_EXISTS".

**Test case 04**: Gán parent cho category đang có con  
**Điều kiện:** Category hiện tại đang là parent của các category con khác.  
**Kết quả:** Hệ thống ném BadRequestException với message "CATEGORY_PARENT_INVALID".

**Test case 05**: Category không tồn tại  
**Điều kiện:** CategoryId không có trong DB.  
**Kết quả:** Hệ thống ném NotFoundException với message "CATEGORY_NOT_FOUND".

**Test case 06**: Cập nhật title giống hệt title cũ (không thay đổi)  
**Điều kiện:** Title mới giống title cũ.  
**Kết quả:** Cập nhật thành công (không conflict với chính nó do exclude self trong check duplicate).

**Test case 07**: Chuyển category từ root thành child (gán parentId hợp lệ)  
**Điều kiện:** Category hiện tại là root (parentId = null), gán parentId = id của root khác.  
**Kết quả:** Cập nhật thành công nếu category không có children. Nếu có children thì throw BadRequestException "CATEGORY_PARENT_INVALID".

**Test case 08**: Chỉ cập nhật title, giữ nguyên parentId (không gửi parentId)  
**Điều kiện:** Request body chỉ có title, không bao gồm parentId.  
**Kết quả:** Cập nhật title thành công, parentId giữ nguyên giá trị cũ.

**Test case 09**: Category đã bị soft delete mà đi update
**Điều kiện:** CategoryId thuộc về category có IsDeleted = true.  
**Kết quả:** Ném NotFoundException với message "CATEGORY_NOT_FOUND".

**Test case 12:** Cập nhật danh mục với mã danh mục cha không tồn tại
**Điều kiện:** Tiến hành cập nhật một danh mục, trong đó truyền vào mã danh mục cha (`parentId`) là một `Id` ngẫu nhiên không hề tồn tại trong cơ sở dữ liệu.
**Kết quả:** Hệ thống lập tức ngăn chặn hành động và ném ra lỗi `NotFoundException` với thông điệp báo lỗi chính xác là `"PARENT_CATEGORY_NOT_FOUND"`.

**Test case 13:** Cập nhật danh mục cha thành một danh mục đang là cấp con
**Điều kiện:** Tiến hành cập nhật một danh mục cha, trong đó truyền vào mã danh mục cha mới (`parentId`) là `Id` của một danh mục đang đóng vai trò là danh mục con (chỉ được chọn danh mục gốc/root làm cha).
**Kết quả:** Hệ thống lập tức ngăn chặn hành động để tránh phân cấp quá sâu và ném ra lỗi `BadRequestException` với thông điệp báo lỗi chính xác là `"CATEGORY_PARENT_INVALID"`.