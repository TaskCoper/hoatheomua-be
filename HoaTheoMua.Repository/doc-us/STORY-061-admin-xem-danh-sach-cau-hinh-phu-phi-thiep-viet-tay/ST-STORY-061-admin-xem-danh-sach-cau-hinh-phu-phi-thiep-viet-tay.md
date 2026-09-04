# ST — STORY-061 — Admin xem danh sách cấu hình phụ phí thiệp viết tay — System Tests

---

## ST-061-01-03 — Xem danh sách cấu hình phụ phí viết tay

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-01-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c2eb857c-514f-489c-b5a7-67f0a70ee354) |
| **Story** | STORY-061 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Hệ thống đang hoạt động bình thường.
- Core Database có cấu hình phụ phí viết tay chưa bị xóa mềm.

**Steps:**
1. Admin truy cập chức năng cấu hình giá viết tay.
2. Hệ thống kiểm tra quyền quản lý cấu hình giá thiệp của Admin.
3. Quan sát danh sách cấu hình phụ phí viết tay.
4. Đối chiếu dữ liệu hiển thị với dữ liệu trong Core Database.

**Test Data:**
- Core Database có nhiều cấu hình phụ phí viết tay có `isDelete = false`. Dữ liệu gồm cấu hình Active và Inactive.

**Expected Result:**
- Hệ thống truy xuất dữ liệu cấu hình phụ phí viết tay từ Core Database.
- Hệ thống chỉ hiển thị các cấu hình có `isDelete = false`.
- Danh sách được hiển thị dưới dạng bảng.
- Dữ liệu hiển thị phù hợp với dữ liệu trong Core Database.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-215](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9552bc4-83eb-49de-8a63-347e4d50d597)
- [BR-217](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d2270398-9b56-45fd-a8ca-e6b48a67c557)

**Rationale:**
> Xác minh luồng chính khi Admin có quyền truy cập danh sách cấu hình phụ phí viết tay và dữ liệu hiển thị được lấy từ Core Database.

---

## ST-061-02-02 — Thông tin hiển thị của từng cấu hình

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-02-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6ee01ca1-9924-45b9-9ea0-a9e53577eb95) |
| **Story** | STORY-061 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Core Database có cấu hình phụ phí viết tay chưa bị xóa mềm.

**Steps:**
1. Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
2. Quan sát thông tin hiển thị của từng cấu hình.
3. Đối chiếu số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái với Core Database.

**Test Data:**
- Cấu hình 1: Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. Trạng thái = Active. `isDelete = false`.
- Cấu hình 2: Số lượng từ bắt đầu = 11. Số lượng từ kết thúc = 20. Giá phụ phí = 30000. Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Mỗi dòng hiển thị số lượng từ bắt đầu.
- Mỗi dòng hiển thị số lượng từ kết thúc.
- Mỗi dòng hiển thị giá phụ phí.
- Mỗi dòng hiển thị trạng thái Active hoặc Inactive.
- Thông tin hiển thị phù hợp với dữ liệu trong Core Database.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-216](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7d4c09e-f10f-4f86-bb2d-ff5b49a5a901)

**Rationale:**
> Xác minh danh sách cung cấp đầy đủ thông tin để Admin nhận diện và theo dõi từng cấu hình phụ phí viết tay.

---

## ST-061-03-02 — Thao tác tương ứng với trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-03-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0d38e1fa-fd37-4896-bb54-0053e57383b8) |
| **Story** | STORY-061 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Danh sách có cấu hình Active và Inactive chưa bị xóa mềm.

**Steps:**
1. Admin truy cập danh sách cấu hình phụ phí viết tay.
2. Quan sát thao tác tại cấu hình đang Active.
3. Quan sát thao tác tại cấu hình đang Inactive.
4. Quan sát các entry point quản lý liên quan.

**Test Data:**
- Cấu hình A: Trạng thái = Active. `isDelete = false`.
- Cấu hình B: Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Cấu hình Active hiển thị thao tác chuyển sang Inactive.
- Cấu hình Inactive hiển thị thao tác chuyển sang Active.
- Hệ thống hiển thị các entry point quản lý liên quan như cập nhật và xóa cấu hình.
- Hệ thống hiển thị entry point “Thêm cấu hình phụ phí viết tay” cho Admin có quyền quản lý cấu hình giá thiệp.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-218](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61)

**Rationale:**
> Xác minh hệ thống hiển thị đúng thao tác tương ứng với trạng thái hiện tại của từng cấu hình phụ phí viết tay.

---

## ST-061-04-02 — Cấu hình xóa mềm bị loại khỏi danh sách mặc định

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-04-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/176ed280-ca56-4081-ac27-b20566d10e07) |
| **Story** | STORY-061 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Core Database có cả cấu hình chưa xóa mềm và cấu hình đã xóa mềm.

**Steps:**
1. Chuẩn bị cấu hình có `isDelete = false`.
2. Chuẩn bị cấu hình có `isDelete = true`.
3. Admin truy cập danh sách cấu hình phụ phí viết tay mặc định.
4. Quan sát các cấu hình được hiển thị.
5. Đối chiếu với dữ liệu trong Core Database.

**Test Data:**
- Cấu hình A: `isDelete = false`.
- Cấu hình B: `isDelete = true`.

**Expected Result:**
- Hệ thống hiển thị cấu hình có `isDelete = false`.
- Hệ thống không hiển thị cấu hình có `isDelete = true` trong danh sách mặc định.
- Danh sách mặc định chỉ sử dụng dữ liệu chưa bị xóa mềm.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-217](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d2270398-9b56-45fd-a8ca-e6b48a67c557)

**Rationale:**
> Xác minh cấu hình phụ phí viết tay đã xóa mềm được loại khỏi danh sách quản trị mặc định.

---

## ST-061-05-01 — Danh sách rỗng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2c6cb223-352f-4db7-8d78-5a09e7a1fa39) |
| **Story** | STORY-061 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Hệ thống đang hoạt động bình thường.
- Không có cấu hình phụ phí viết tay nào có `isDelete = false`.

**Steps:**
1. Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
2. Hệ thống truy xuất dữ liệu từ Core Database.
3. Quan sát trạng thái danh sách.
4. Quan sát thông báo trên màn hình.
5. Quan sát entry point “Thêm cấu hình phụ phí viết tay”.

**Test Data:**
- Core Database không có cấu hình phụ phí viết tay nào có `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
- Hệ thống không hiển thị thông báo lỗi tải dữ liệu.
- Hệ thống vẫn hiển thị entry point “Thêm cấu hình phụ phí viết tay”.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-215](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9552bc4-83eb-49de-8a63-347e4d50d597)
- [BR-217](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d2270398-9b56-45fd-a8ca-e6b48a67c557)

**Rationale:**
> Xác minh hệ thống phân biệt đúng trường hợp danh sách không có dữ liệu với trường hợp truy xuất dữ liệu thất bại.

---

## ST-061-06-01 — Chặn Admin không có quyền quản lý cấu hình giá thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/66877807-2a38-43ff-ad9f-839aa21d1395) |
| **Story** | STORY-061 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin không có quyền quản lý cấu hình giá thiệp.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra danh sách cấu hình phụ phí viết tay có được hiển thị hay không.
4. Kiểm tra dữ liệu có được trả về cho tài khoản hay không.

**Test Data:**
- Tài khoản Admin không có quyền quản lý cấu hình giá thiệp.

**Expected Result:**
- Backend kiểm tra quyền Admin trước khi trả dữ liệu.
- Hệ thống không hiển thị danh sách cấu hình phụ phí viết tay.
- Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.
- Dữ liệu cấu hình không được trả về như một kết quả truy cập thành công.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)

**Rationale:**
> Xác minh quyền quản lý cấu hình giá thiệp được kiểm tra trước khi hệ thống cung cấp dữ liệu cấu hình phụ phí viết tay.

---

## ST-061-07-01 — Lỗi truy xuất danh sách và chức năng thử lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-061-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60e6f775-b68a-4b18-9ff7-b616b0d9f95d) |
| **Story** | STORY-061 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Có thể tạo tình huống hệ thống không thể truy xuất danh sách cấu hình phụ phí viết tay từ Core Database.

**Steps:**
1. Tạo tình huống không thể truy xuất danh sách cấu hình phụ phí viết tay từ Core Database.
2. Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
3. Quan sát phản hồi của hệ thống.
4. Quan sát dữ liệu hiển thị trên danh sách.
5. Quan sát nội dung thông báo lỗi.
6. Chọn “Thử lại” sau khi Core Database hoạt động bình thường.
7. Quan sát kết quả tải lại.

**Test Data:**
- Môi trường test có khả năng tạo lỗi khi truy xuất dữ liệu từ Core Database.

**Expected Result:**
- Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
- Hệ thống hiển thị thông báo lỗi. Thông báo lỗi không chứa stack trace hoặc thông tin kỹ thuật nhạy cảm.
- Hệ thống cho phép Admin thực hiện “Thử lại”.
- Sau khi Core Database hoạt động bình thường và Admin chọn “Thử lại”, hệ thống tải lại danh sách cấu hình phụ phí viết tay.

**Trace to:**
- [STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [STORY-061/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [BR-215](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9552bc4-83eb-49de-8a63-347e4d50d597)

**Rationale:**
> Xác minh hệ thống xử lý rõ ràng lỗi truy xuất Core Database, không làm dữ liệu lỗi trông như tải thành công và cho phép Admin thử lại.
