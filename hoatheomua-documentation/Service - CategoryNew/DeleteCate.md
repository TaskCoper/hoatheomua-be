#### Request
```json
DELETE /api/v1/category-news/44444444-4444-4444-4444-444444444441
```
#### Response
```json
{
  "value": {
    "message": "Xóa danh mục thành công."
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Service Flow (DeleteCate)
![[../Store-Image/DeleteCate.drawio (1).png]]

#### Test case
**Test case 01:** Xóa danh mục cha, tự động xóa mềm các danh mục con đang hoạt động (Cascade soft delete)
**Điều kiện:** Hệ thống có 1 danh mục cha và 2 danh mục con đang hoạt động bình thường (`IsDeleted = false`). Tiến hành xóa danh mục cha.
**Kết quả:** Danh mục cha và cả 2 danh mục con đều được chuyển trạng thái thành đã xóa (`IsDeleted = true`), đồng thời thời gian cập nhật (`UpdatedAt`) của cả 3 danh mục đều được cập nhật giá trị mới.

**Test case 02:** Xóa danh mục con, chỉ xóa mềm chính nó
**Điều kiện:** Hệ thống có 1 danh mục cha và 1 danh mục con đang hoạt động. Tiến hành xóa danh mục con.
**Kết quả:** Chỉ có danh mục con bị chuyển trạng thái thành đã xóa (`IsDeleted = true`) và cập nhật `UpdatedAt`. Danh mục cha vẫn giữ nguyên trạng thái hoạt động (`IsDeleted = false`) và `UpdatedAt = null`.

**Test case 03:** Xóa danh mục không tồn tại trong hệ thống
**Điều kiện:** Tiến hành gọi hàm xóa với một mã định danh (`Id`) ngẫu nhiên không khớp với bất kỳ danh mục nào đang có trong cơ sở dữ liệu.
**Kết quả:** Hệ thống lập tức ngăn chặn và ném ra lỗi `NotFoundException` với thông điệp báo lỗi chính xác là `"CATEGORY_NOT_FOUND"`.

**Test case 04:** Xóa danh mục đã bị xóa mềm trước đó
**Điều kiện:** Tiến hành gọi hàm xóa với một mã định danh (`Id`) của danh mục đã mang trạng thái bị xóa từ trước (`IsDeleted = true`).
**Kết quả:** Hệ thống coi như danh mục này không tồn tại, lập tức ngăn chặn và ném ra lỗi `NotFoundException` với thông điệp báo lỗi là `"CATEGORY_NOT_FOUND"`.

**Test case 05:** Xóa danh mục cha khi toàn bộ các danh mục con của nó đều đã bị xóa mềm từ trước
**Điều kiện:** Hệ thống có 1 danh mục cha đang hoạt động, nhưng tất cả các danh mục con trực thuộc nó đều đã bị xóa mềm từ trước (`IsDeleted = true`). Tiến hành xóa danh mục cha.
**Kết quả:** Danh mục cha bị chuyển trạng thái thành đã xóa (`IsDeleted = true`) và cập nhật `UpdatedAt`. Các danh mục con đã bị xóa trước đó vẫn giữ nguyên thời gian cập nhật cũ (`UpdatedAt = null`), chứng tỏ Service hoạt động tối ưu và không tốn công truy vấn lại các danh mục con đã chết.
