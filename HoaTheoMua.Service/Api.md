## 4.4 Update CategoryNews

Cập nhật thông tin của một danh mục tin tức hiện có, hỗ trợ thay đổi tên và di chuyển vị trí trong cấu trúc cây danh mục.

* **HTTP Method:** `PUT`
* **URL Path:** `/category-news/:id`

### Request Payload

```json
{
  "title": "Hoa Cưới Thiết Kế Cao Cấp",
  "parentId": 2
}
Luồng xử lý dữ liệu
Phía Frontend	Phía Backend
• Gửi id danh mục cần cập nhật trên URL Path.


• Gửi title/parentId mới trong Request Body.


• Render lại cấu trúc cây danh mục trên giao diện sau khi nhận phản hồi cập nhật thành công từ Server.

• Kiểm tra danh mục cần sửa có tồn tại trong Database hay không (nếu không, ném lỗi 404 Not Found).


• Kiểm tra Loop Check (Tránh vòng lặp vô hạn): Đảm bảo parentId mới không được trùng với id hiện tại của chính nó và không được nằm trong danh sách các danh mục con/cháu đệ quy của chính nó.


• Tiến hành cập nhật thông tin bản ghi vào bảng CategoryNews.

4.5 Delete CategoryNews
Thực hiện xóa một danh mục tin tức ra khỏi hệ thống bằng cơ chế xóa mềm (Soft Delete).

HTTP Method: DELETE

URL Path: /category-news/:id

Response Body
JSON
{
  "message": "Xóa danh mục thành công."
}
Quy trình xử lý giữa các bên
Bên xử lý	Nội dung xử lý chi tiết
Frontend	Gửi HTTP Request chứa id danh mục lên hệ thống. Nếu danh mục này đang chứa bài viết (News) hoặc đang chứa các danh mục con bên trong, Frontend phải hiển thị một hộp thoại cảnh báo (Confirmation Dialog) nhắc nhở người dùng trước khi chính thức bấm xác nhận xóa.
Backend	
Tìm kiếm danh mục trong Database theo id. Nếu phát hiện danh mục này vẫn còn chứa các phần tử con trực thuộc, lựa chọn xử lý theo một trong hai hướng nghiệp vụ sau:


1. Từ chối xóa: Trả về lỗi yêu cầu người dùng phải dọn sạch phần tử con trước.


2. Tự động dịch chuyển: Cập nhật trường parentId của toàn bộ danh mục con về null (hoặc chuyển lên danh mục cha cấp cao hơn).


Sau khi xử lý xong các ràng buộc, thực hiện cập nhật trạng thái xóa mềm (Soft Delete) cho bản ghi và trả kết quả về cho Client.


---

### Mẹo nhỏ cho thiết kế Backend (Cảnh báo đặc biệt về đoạn code Loop Check):
Khi bạn viết code cho hàm **Update**, việc xử lý **Loop Check** cực kỳ quan trọng. Bạn có thể triển khai một hàm đệ quy nhỏ hoặc một vòng lặp `while` để kiểm tra ngược từ `parentId` mới lên các cấp cha của nó. Nếu dọc đường đi lên mà vô tình gặp lại cái `id` đang sửa, bạn phải chặn ngay và báo lỗi (ví dụ: `400 Bad Request`) để tránh làm sập thuật toán dựng cây (`TryGetValue`) mà chúng ta đã làm ở các bước trước nhé!