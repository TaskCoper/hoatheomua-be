# Danh Mục 213 Business Rules — HTM_AI_Customize (`hoa-theo-mua-ai-customize`)

> **Phân hệ**: `HTM_AI_Customize`  
> **Tổng số quy tắc nghiệp vụ**: **212 Business Rules** chuẩn hóa (BR-020 đến BR-273)  
> **Mục tiêu**: Định nghĩa toàn bộ logic nghiệp vụ, ràng buộc, điều kiện tiên quyết và ngoại lệ theo chuẩn Document-First  

---

## 📑 Phân Nhóm Nghiệp Vụ Cốt Lõi

### 1. Nhóm Quota & Tạo Mẫu Hoa AI (BR-020 -> BR-035, BR-155)
- **BR-020 -> BR-024**: Quota tạo hoa AI, kiểm tra lượt khả dụng, trừ quota khi request được chấp nhận.
- **BR-025 -> BR-029**: Hoàn quota khi retry thất bại, giới hạn tối đa 3 lần retry cho mỗi job AI.
- **BR-030 -> BR-035**: Tham chiếu combo nguồn, kiểm tra tồn kho core, đóng watermark logo thương hiệu.
- **BR-155**: Quy tắc ưu tiên prompt và ràng buộc tham số đầu vào cho model sinh hoa AI.

### 2. Nhóm Cấu Hình Thiệp Tại Checkout (BR-036 -> BR-055, BR-160 -> BR-180)
- **BR-036 -> BR-040**: Phân loại thiệp (Miễn phí / Trả phí - In máy / Viết tay).
- **BR-041 -> BR-048**: Đếm từ lời chúc, tính phụ phí viết tay theo bậc số từ, snapshot cấu hình thiệp vào Order Item.
- **BR-049**: Quy tắc trừ và hoàn quota thiệp (chỉ trừ quota khi chấp nhận kết quả).
- **BR-050**: Quy tắc tạo đúng 01 History record cho mỗi lần generate có ảnh output hợp lệ.
- **BR-055**: Xử lý idempotent chống trùng lặp request tạo thiệp.
- **BR-057**: Phạm vi sáng tạo của AI (chỉ trang trí trong phạm vi template, tham chiếu combo nguồn).

### 3. Nhóm Quản Trị Mockup Bình / Hộp Hoa (BR-181 -> BR-210)
- Ràng buộc định dạng ảnh mockup (JPG, PNG, WEBP, tối đa 10MB).
- Kiểm tra tính duy nhất của tên mockup.
- Cơ chế xóa mềm (`is_deleted = true`), chặn xóa mockup đang được tham chiếu trong đơn hàng chờ xử lý.
- Phân quyền quản trị viên (Admin).

### 4. Nhóm Quản Trị Cấu Hình Thiệp & Phụ Phí (BR-211 -> BR-240)
- Quản lý kích thước thiệp (Size Configs).
- Quản lý mẫu thiệp (Template Configs).
- Quản lý bảng phụ phí thiệp viết tay theo số lượng ký tự/từ.
- Bảo vệ snapshot lịch sử khi cấu hình gốc bị sửa hoặc xóa.

### 5. Nhóm Quản Trị Kiểu Gói & Cấu Hình Size Hoa (BR-241 -> BR-277)
- **BR-241 -> BR-260**: Quản lý hình ảnh kiểu gói (Wrapping styles), kiểm tra file ảnh hợp lệ, cập nhật trạng thái hoạt động.
- **BR-261 -> BR-277**: Quản lý cấu hình Size cho hoa Custom AI, công thức tính số lượng cành hoa theo từng size, phân quyền Admin ([BR-274](file:///d:/VNZ/document-first-brief/hoa-theo-mua-ai-customize/BusinessRules/BR-274.md)).

---

## 📂 Tra Cứu File Business Rules

Toàn bộ 213 file Business Rule độc lập được lưu trữ trực tiếp tại thư mục [`BusinessRules/`](file:///d:/VNZ/document-first-brief/hoa-theo-mua-ai-customize/BusinessRules/) với định dạng chuẩn `BR-xxx.md`.
