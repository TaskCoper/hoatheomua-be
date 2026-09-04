# ST — STORY-065 — Admin cập nhật mẫu thiệp — System Tests

---

## ST-065-01-01 — Cập nhật thông tin mẫu thiệp với dữ liệu hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/37aca09a-aecd-47e4-99f3-c4cdb4ca15d6) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.
- Mẫu thiệp còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách mẫu thiệp.
2. Admin chọn thao tác cập nhật tại một mẫu thiệp.
3. Quan sát dữ liệu hiện tại trên form cập nhật.
4. Admin chỉnh sửa một hoặc nhiều thông tin.
5. Admin chọn “Lưu”.
6. Quan sát phản hồi của hệ thống.
7. Quan sát thông tin mới trong danh sách mẫu thiệp.
8. Kiểm tra dữ liệu mẫu thiệp trong Core Database.

**Test Data:**
- Dữ liệu hiện tại: Tên mẫu = Thiệp sinh nhật. Mô tả = Mẫu thiệp sinh nhật. Trạng thái = Active. `isDelete = false`.
- Dữ liệu cập nhật: Tên mẫu = Thiệp sinh nhật hoa hồng. Mô tả = Mẫu thiệp sinh nhật với hoa hồng.

**Expected Result:**
- Hệ thống tải thông tin hiện tại của mẫu thiệp từ Core Database.
- Form hiển thị tên mẫu, mô tả nếu có và ảnh mẫu thiệp hiện tại.
- Khi có ít nhất một thông tin thay đổi, nút “Lưu” được enable.
- Hệ thống kiểm tra dữ liệu và kiểm tra mẫu thiệp còn tồn tại, chưa bị xóa mềm.
- Hệ thống cập nhật các thông tin được thay đổi trong Core Database.
- Hệ thống thông báo cập nhật thành công.
- Danh sách mẫu thiệp hiển thị thông tin mới.
- Trạng thái Active/Inactive của mẫu thiệp được giữ nguyên.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-233](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdb72537-b0de-4c2a-9673-aa6b52979140)
- [BR-234](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5e941e4-e526-4813-a674-e9c763d05111)

**Rationale:**
> Xác minh luồng chính khi Admin cập nhật thông tin mẫu thiệp với dữ liệu hợp lệ.

---

## ST-065-02-01 — Không cung cấp ảnh mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecbaf5a2-de53-4805-9276-e3b34f2f8d83) |
| **Story** | STORY-065 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại, chưa bị xóa mềm và đã có ảnh template/ảnh Preview hiện tại.

**Steps:**
1. Admin mở form cập nhật mẫu thiệp.
2. Admin thay đổi tên mẫu hoặc mô tả.
3. Admin không chọn ảnh mới.
4. Admin chọn “Lưu”.
5. Quan sát kết quả cập nhật.
6. Kiểm tra tham chiếu ảnh của mẫu thiệp.

**Test Data:**
- Mẫu thiệp hiện tại có ảnh Preview hợp lệ. Admin chỉ thay đổi tên mẫu hoặc mô tả.

**Expected Result:**
- Hệ thống cập nhật các thông tin được thay đổi.
- Hệ thống giữ nguyên ảnh template/ảnh Preview hiện tại.
- Tham chiếu ảnh của mẫu thiệp không bị thay đổi.
- Hệ thống thông báo cập nhật thành công.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-235](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9)

**Rationale:**
> Xác minh ảnh hiện tại được giữ nguyên khi Admin không cung cấp ảnh mới.

---

## ST-065-03-01 — Cung cấp ảnh mới hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e99620a2-633d-4cdd-be25-2d20e7669f90) |
| **Story** | STORY-065 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và chưa bị xóa mềm. Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin chọn một ảnh mới hợp lệ.
2. Quan sát việc kiểm tra file ảnh.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Quan sát ảnh Preview trong danh sách mẫu thiệp.
6. Kiểm tra tham chiếu ảnh mới của mẫu thiệp.

**Test Data:**
- Ảnh mới: Định dạng = PNG hoặc JPG. Dung lượng <= 10 MB.

**Expected Result:**
- Hệ thống chấp nhận ảnh mới hợp lệ.
- Hệ thống lưu ảnh mới và cập nhật tham chiếu ảnh của mẫu thiệp.
- Hệ thống thông báo cập nhật thành công.
- Danh sách mẫu thiệp hiển thị ảnh Preview mới.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-235](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9)

**Rationale:**
> Xác minh Admin có thể thay đổi ảnh template/ảnh Preview bằng một file hợp lệ.

---

## ST-065-04-01 — Hủy hoặc đóng form cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78252598-d6d0-482e-813f-4cb506a0b245) |
| **Story** | STORY-065 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin thay đổi một hoặc nhiều thông tin trên form.
2. Admin chọn hủy hoặc đóng form.
3. Quan sát màn hình sau khi hủy.
4. Kiểm tra dữ liệu mẫu thiệp trong Core Database.

**Test Data:**
- Admin đã thay đổi tên mẫu, mô tả hoặc ảnh nhưng chưa lưu.

**Expected Result:**
- Hệ thống không cập nhật dữ liệu mẫu thiệp.
- Mẫu thiệp giữ nguyên thông tin trước khi thao tác.
- Core Database không ghi nhận các thay đổi chưa lưu.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh việc hủy hoặc đóng form không làm thay đổi dữ liệu mẫu thiệp.

---

## ST-065-05-01 — Nút Lưu disable khi không có thay đổi

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef424ae7-24b7-4622-8dd0-173d45b9c593) |
| **Story** | STORY-065 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đã mở form cập nhật mẫu thiệp. Form đang hiển thị dữ liệu hiện tại của mẫu thiệp.

**Steps:**
1. Quan sát trạng thái nút “Lưu” ngay khi form được mở.
2. Không thay đổi tên mẫu.
3. Không thay đổi mô tả.
4. Không thay đổi ảnh.
5. Thử thực hiện yêu cầu cập nhật.

**Test Data:**
- Dữ liệu trên form giữ nguyên hoàn toàn so với dữ liệu hiện tại.

**Expected Result:**
- Nút “Lưu” ở trạng thái disable khi chưa có dữ liệu nào được thay đổi.
- Admin không thể gửi yêu cầu cập nhật khi chưa có dữ liệu thay đổi.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/ALT-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh hệ thống không cho gửi yêu cầu cập nhật khi dữ liệu không thay đổi.

---

## ST-065-06-01 — Trạng thái nút Lưu phản ánh đúng việc form có thay đổi

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d346afa3-0578-4e47-ab91-7c4cb13fe38e) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form cập nhật mẫu thiệp. Nút “Lưu” đang disable vì chưa có thay đổi.

**Steps:**
1. Thay đổi tên mẫu.
2. Quan sát trạng thái nút “Lưu”.
3. Khôi phục tên mẫu ban đầu.
4. Thay đổi mô tả.
5. Quan sát trạng thái nút “Lưu”.
6. Khôi phục dữ liệu.
7. Chọn ảnh mới.
8. Quan sát trạng thái nút “Lưu”.

**Test Data:**
- Thực hiện thay đổi riêng từng trường: Tên mẫu. Mô tả. Ảnh mẫu thiệp.

**Expected Result:**
- Khi có ít nhất một thông tin thay đổi, hệ thống enable nút “Lưu”.
- Khi dữ liệu trở về giống hoàn toàn dữ liệu ban đầu, hệ thống không cho gửi yêu cầu cập nhật nếu không còn thay đổi.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh trạng thái nút Lưu phản ánh đúng việc form có dữ liệu thay đổi hay không.

---

## ST-065-07-01 — Tên mẫu bị xóa rỗng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eb382321-ce89-4752-9606-90a577cf873f) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin xóa toàn bộ nội dung trường tên mẫu.
2. Admin chọn “Lưu”.
3. Quan sát phản hồi của hệ thống.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Tên mẫu = rỗng. Các dữ liệu còn lại hợp lệ.

**Expected Result:**
- Hệ thống không cho phép cập nhật mẫu thiệp.
- Hệ thống hiển thị lỗi tại trường tên mẫu.
- Dữ liệu mẫu thiệp trong Core Database không bị thay đổi.
- Admin có thể chỉnh sửa dữ liệu và thử lại.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh tên mẫu là trường bắt buộc khi cập nhật mẫu thiệp.

---

## ST-065-08-01 — Tên mẫu vượt quá giới hạn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/48f29785-5a6e-415a-893e-6f28d0e34616) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin nhập tên mẫu có hơn 20 từ.
2. Admin chọn “Lưu”.
3. Quan sát phản hồi của hệ thống.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Tên mẫu = chuỗi có 21 từ. Các dữ liệu còn lại hợp lệ.

**Expected Result:**
- Hệ thống không cho phép cập nhật mẫu thiệp.
- Hệ thống hiển thị lỗi tại trường tên mẫu.
- Tên mẫu vượt quá 20 từ không được lưu vào Core Database.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh giới hạn tối đa 20 từ của tên mẫu khi cập nhật.

---

## ST-065-09-01 — Mô tả vượt quá giới hạn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bec0c828-ff61-4f7e-90c1-3b332c8e08ce) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin nhập mô tả vượt quá 200 ký tự.
2. Admin chọn “Lưu”.
3. Quan sát phản hồi của hệ thống.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Mô tả = chuỗi có 201 ký tự. Tên mẫu hợp lệ.

**Expected Result:**
- Hệ thống không cho phép cập nhật mẫu thiệp.
- Hệ thống hiển thị lỗi tại trường mô tả.
- Mô tả vượt quá 200 ký tự không được lưu vào Core Database.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh giới hạn tối đa 200 ký tự của mô tả mẫu thiệp.

---

## ST-065-10-01 — Mô tả không bắt buộc nhập

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/41f9e055-132c-474b-8653-7ef53adc5752) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin mở form cập nhật mẫu thiệp.
2. Admin để trống hoặc xóa nội dung mô tả.
3. Admin giữ tên mẫu hợp lệ.
4. Admin chọn “Lưu”.
5. Quan sát kết quả cập nhật.

**Test Data:**
- Tên mẫu hợp lệ. Mô tả = rỗng.

**Expected Result:**
- Hệ thống cho phép cập nhật khi mô tả để trống.
- Mô tả không phải trường bắt buộc.
- Hệ thống cập nhật thành công nếu các dữ liệu còn lại hợp lệ.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh mô tả là trường không bắt buộc theo Context của STORY-065.

---

## ST-065-11-01 — Ảnh mới không đúng định dạng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03447fd1-16dd-4457-82df-d1a1ef7375a2) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin chọn ảnh mới không thuộc định dạng PNG hoặc JPG.
2. Quan sát phản hồi của hệ thống.
3. Thử lưu nếu hệ thống cho phép.
4. Kiểm tra tham chiếu ảnh của mẫu thiệp.

**Test Data:**
- File ảnh mới có định dạng GIF, WEBP, PDF hoặc định dạng khác PNG/JPG.

**Expected Result:**
- Hệ thống xác định file không đáp ứng điều kiện ảnh được hỗ trợ.
- Hệ thống không sử dụng ảnh đó để cập nhật mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp. Admin có thể chọn ảnh khác.
- Ảnh template/ảnh Preview hiện tại không bị thay thế bởi file không hợp lệ.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-235](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9)

**Rationale:**
> Xác minh hệ thống chỉ chấp nhận ảnh PNG hoặc JPG khi cập nhật mẫu thiệp.

---

## ST-065-12-01 — Ảnh mới vượt quá dung lượng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/89db2903-71cf-46a5-b290-cac19e57893c) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Admin đang ở form cập nhật mẫu thiệp.

**Steps:**
1. Admin chọn ảnh PNG hoặc JPG có dung lượng vượt quá 10 MB.
2. Quan sát phản hồi của hệ thống.
3. Thử lưu nếu hệ thống cho phép.
4. Kiểm tra tham chiếu ảnh của mẫu thiệp.

**Test Data:**
- Ảnh PNG hoặc JPG có dung lượng > 10 MB.

**Expected Result:**
- Hệ thống không sử dụng ảnh vượt quá 10 MB để cập nhật mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp. Admin có thể chọn ảnh khác.
- Ảnh hiện tại của mẫu thiệp được giữ nguyên.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-235](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9)

**Rationale:**
> Xác minh giới hạn dung lượng ảnh tối đa 10 MB.

---

## ST-065-13-01 — Mẫu thiệp không còn hợp lệ khi lưu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-13-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6b167120-6206-44f9-89d3-88f65866d4ed) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Admin đã mở form cập nhật một mẫu thiệp.

**Steps:**
1. Sau khi form được mở, làm cho mẫu thiệp không còn tồn tại hoặc có `isDelete = true`.
2. Admin thay đổi dữ liệu hợp lệ.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Trường hợp 1: Mẫu thiệp không còn tồn tại trong Core Database.
- Trường hợp 2: Mẫu thiệp có `isDelete = true` trước thời điểm lưu.

**Expected Result:**
- Backend kiểm tra lại mẫu thiệp trước khi cập nhật.
- Hệ thống không cập nhật dữ liệu.
- Hệ thống thông báo mẫu thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
- Hệ thống tải lại danh sách mẫu thiệp.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh hệ thống không cập nhật dữ liệu đã không còn hợp lệ tại thời điểm xử lý yêu cầu.

---

## ST-065-14-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-14-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8c882ec4-e122-4c65-9ad6-baccc70fb191) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và chưa bị xóa mềm. Admin đã nhập dữ liệu cập nhật hợp lệ.
- Có thể tạo tình huống cập nhật thất bại.

**Steps:**
1. Admin nhập dữ liệu hợp lệ.
2. Tạo tình huống hệ thống gặp lỗi trong quá trình cập nhật.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra dữ liệu mẫu thiệp trong Core Database.
6. Kiểm tra ảnh nếu có thay đổi ảnh.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Dữ liệu cập nhật hợp lệ. Môi trường test có khả năng tạo lỗi khi cập nhật mẫu thiệp.

**Expected Result:**
- Hệ thống không lưu dữ liệu cập nhật dở dang hoặc không đồng nhất.
- Mẫu thiệp giữ nguyên dữ liệu hợp lệ trước lần cập nhật.
- Hệ thống thông báo cập nhật thất bại. Admin có thể thử lại.
- Core Database là nguồn xác thực cuối cùng của thông tin mẫu thiệp.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)

**Rationale:**
> Xác minh lỗi cập nhật không làm mẫu thiệp rơi vào trạng thái dữ liệu dở dang.

---

## ST-065-15-01 — Người dùng không có quyền quản lý mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-15-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0497bbb1-473d-4750-862f-45f7759a5639) |
| **Story** | STORY-065 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database.

**Steps:**
1. Người dùng gửi yêu cầu cập nhật mẫu thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra dữ liệu mẫu thiệp trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý mẫu thiệp.

**Expected Result:**
- Backend kiểm tra quyền trước khi thực hiện cập nhật.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi dữ liệu mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-232](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ab34cf24-bed6-4353-84ca-b0dcdef83794)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý mẫu thiệp mới được cập nhật mẫu thiệp.

---

## ST-065-16-01 — Cập nhật thông tin tách biệt với thay đổi trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-16-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/29291d11-175c-4218-9974-6894f23f439d) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Có mẫu thiệp Active hoặc Inactive chưa bị xóa mềm.

**Steps:**
1. Ghi nhận trạng thái hiện tại của mẫu thiệp.
2. Admin cập nhật tên mẫu, mô tả hoặc ảnh bằng dữ liệu hợp lệ.
3. Admin chọn “Lưu”.
4. Kiểm tra trạng thái mẫu thiệp trong Core Database sau cập nhật.

**Test Data:**
- Thực hiện với:
  - Trường hợp 1: mẫu thiệp Active.
  - Trường hợp 2: mẫu thiệp Inactive.

**Expected Result:**
- Hệ thống cập nhật thông tin mẫu thiệp thành công.
- Trạng thái Active/Inactive hiện tại của mẫu thiệp được giữ nguyên.
- Việc cập nhật thông tin không tự động chuyển trạng thái mẫu thiệp.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [BR-234](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5e941e4-e526-4813-a674-e9c763d05111)

**Rationale:**
> Xác minh chức năng cập nhật thông tin được tách biệt với chức năng chuyển trạng thái mẫu thiệp.

---

## ST-065-17-01 — Không làm thay đổi dữ liệu lịch sử đã phát sinh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-17-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b54c3708-881f-44bb-beea-c9018d7441c3) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp đã từng được sử dụng cho thiệp, Checkout hoặc Order đã tồn tại.

**Steps:**
1. Ghi nhận nội dung và ảnh của các thiệp, Checkout hoặc Order đã sử dụng mẫu thiệp.
2. Admin cập nhật tên mẫu, mô tả hoặc ảnh mẫu thiệp.
3. Admin chọn “Lưu”.
4. Kiểm tra lại các thiệp, Checkout hoặc Order đã tồn tại.

**Test Data:**
- Một mẫu thiệp đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp, Checkout, Order.

**Expected Result:**
- Thông tin mới của mẫu thiệp được lưu để sử dụng cho các lần tạo thiệp mới.
- Các thiệp, Checkout hoặc Order đã tồn tại không bị thay đổi nội dung hoặc ảnh.
- Dữ liệu lịch sử được giữ nguyên theo thông tin đã lưu tại thời điểm phát sinh.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-236](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df42784d-c617-4436-9d8f-7f8d00515a4c)

**Rationale:**
> Xác minh cập nhật mẫu thiệp không làm thay đổi dữ liệu lịch sử đã phát sinh.
