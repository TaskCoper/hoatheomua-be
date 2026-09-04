# ST — STORY-057 — Admin xem danh sách mẫu thiệp — System Tests

---

## ST-057-01-01 — Xem danh sách mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/465915ef-cbc2-4011-ba12-b7a0b99c8221) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.
- Core Database có mẫu thiệp chưa bị xóa mềm.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Hệ thống kiểm tra quyền quản lý mẫu thiệp của Admin.
3. Quan sát danh sách mẫu thiệp được hiển thị.
4. Đối chiếu dữ liệu hiển thị với dữ liệu mẫu thiệp trong Core Database.

**Test Data:**
- Core Database có nhiều mẫu thiệp có `isDelete = false`. Dữ liệu gồm mẫu thiệp Active và Inactive.

**Expected Result:**
- Hệ thống truy xuất danh sách mẫu thiệp từ Core Database.
- Hệ thống chỉ hiển thị các mẫu thiệp có `isDelete = false`.
- Danh sách được hiển thị dưới dạng bảng.
- Dữ liệu hiển thị phù hợp với dữ liệu trong Core Database.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-197](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9)

**Rationale:**
> Xác minh luồng chính khi Admin có quyền truy cập danh sách mẫu thiệp và dữ liệu hiển thị được lấy từ Core Database.

---

## ST-057-02-01 — Thông tin hiển thị của từng mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/acb1d695-cecf-46cd-ad40-799ece875eb4) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Core Database có mẫu thiệp chưa bị xóa mềm. Danh sách có mẫu thiệp Active và Inactive.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Quan sát thông tin hiển thị của từng mẫu thiệp.
3. Quan sát thao tác tại mẫu thiệp đang Active.
4. Quan sát thao tác tại mẫu thiệp đang Inactive.

**Test Data:**
- Mẫu thiệp 1: Tên mẫu có dữ liệu. Ảnh Preview có dữ liệu. Mô tả có dữ liệu. Trạng thái Active. `isDelete = false`.
- Mẫu thiệp 2: Tên mẫu có dữ liệu. Ảnh Preview có dữ liệu. Không có mô tả. Trạng thái Inactive. `isDelete = false`.

**Expected Result:**
- Mỗi dòng hiển thị tên mẫu, ảnh Preview, mô tả nếu có và trạng thái Active hoặc Inactive.
- Mẫu thiệp không có mô tả vẫn được hiển thị bình thường.
- Mẫu thiệp Active hiển thị thao tác chuyển sang Inactive.
- Mẫu thiệp Inactive hiển thị thao tác chuyển sang Active.
- `isDelete` không được hiển thị như một cột trạng thái trong danh sách.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-198](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed30ed6e-4368-4ac7-a0b0-b04f2ebb889b)

**Rationale:**
> Xác minh Admin có đủ thông tin để nhận diện mẫu thiệp và hệ thống hiển thị đúng thao tác tương ứng với trạng thái hiện tại.

---

## ST-057-03-01 — Danh sách mặc định không hiển thị mẫu thiệp đã xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08296440-37d3-4e1c-8e3f-2441edc00320) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Core Database có cả mẫu thiệp chưa xóa mềm và mẫu thiệp đã xóa mềm.

**Steps:**
1. Chuẩn bị dữ liệu gồm mẫu thiệp có `isDelete = false` và `isDelete = true`.
2. Admin truy cập chức năng quản lý mẫu thiệp.
3. Quan sát danh sách mẫu thiệp.
4. Đối chiếu danh sách với dữ liệu trong Core Database.

**Test Data:**
- Mẫu thiệp 1 có `isDelete = false`.
- Mẫu thiệp 2 có `isDelete = true`.

**Expected Result:**
- Hệ thống hiển thị mẫu thiệp có `isDelete = false`.
- Hệ thống không hiển thị mẫu thiệp có `isDelete = true` trong danh sách mặc định.
- Mẫu thiệp đã xóa mềm vẫn tồn tại trong Core Database.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-197](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9)

**Rationale:**
> Xác minh danh sách quản trị mặc định loại trừ đúng các mẫu thiệp đã bị xóa mềm.

---

## ST-057-04-01 — Xem ảnh Preview bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9b6d5ea1-e3ad-4152-ab70-c321bb4c9aac) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Danh sách có ít nhất một mẫu thiệp có ảnh Preview khả dụng.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Admin chọn ảnh Preview của một mẫu thiệp.
3. Quan sát cách hệ thống hiển thị ảnh.

**Test Data:**
- Một mẫu thiệp có `isDelete = false` và có ảnh Preview khả dụng.

**Expected Result:**
- Hệ thống mở ảnh Preview bằng Image Lightbox.
- Ảnh được hiển thị với kích thước lớn hơn để Admin xem.
- Việc mở Image Lightbox không làm thay đổi dữ liệu mẫu thiệp.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-200](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a06ed0c-7ae5-436d-b08d-c6398eb88148)

**Rationale:**
> Xác minh chức năng xem ảnh Preview kích thước lớn của mẫu thiệp theo đúng quy tắc Image Lightbox.

---

## ST-057-05-01 — Ảnh Preview không khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fe6865c-dd43-420a-8584-a9ba8a16220d) |
| **Story** | STORY-057 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Có mẫu thiệp chưa bị xóa mềm nhưng ảnh Preview không khả dụng.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Quan sát mẫu thiệp có ảnh Preview không khả dụng.
3. Quan sát các thông tin còn lại của mẫu thiệp.
4. Thực hiện thao tác xem ảnh Preview nếu hệ thống cho phép.

**Test Data:**
- Một mẫu thiệp có: `isDelete = false`. Tên mẫu hợp lệ. Trạng thái Active hoặc Inactive. Ảnh Preview không khả dụng.

**Expected Result:**
- Hệ thống vẫn hiển thị các thông tin còn lại của mẫu thiệp.
- Mẫu thiệp không bị loại khỏi danh sách chỉ vì ảnh Preview không khả dụng.
- Hệ thống hiển thị thông báo hoặc trạng thái phù hợp thay vì mở ảnh lỗi.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-198](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed30ed6e-4368-4ac7-a0b0-b04f2ebb889b)
- [BR-200](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a06ed0c-7ae5-436d-b08d-c6398eb88148)

**Rationale:**
> Xác minh lỗi của riêng ảnh Preview không làm mất các thông tin quản trị còn lại của mẫu thiệp.

---

## ST-057-06-01 — Danh sách rỗng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae5c5bc3-1f64-4bb3-a902-86dcd5dbd693) |
| **Story** | STORY-057 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.
- Không có mẫu thiệp nào có `isDelete = false`.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Hệ thống truy xuất danh sách mẫu thiệp từ Core Database.
3. Quan sát trạng thái danh sách.
4. Quan sát entry point “Thêm mẫu thiệp”.

**Test Data:**
- Core Database không có mẫu thiệp nào có `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
- Hệ thống không hiển thị thông báo lỗi tải dữ liệu.
- Hệ thống vẫn hiển thị entry point “Thêm mẫu thiệp” cho Admin có quyền quản lý mẫu thiệp.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-197](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9)

**Rationale:**
> Xác minh hệ thống phân biệt đúng trường hợp không có dữ liệu với trường hợp tải dữ liệu thất bại.

---

## ST-057-07-01 — Chặn Admin không có quyền quản lý mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ce392738-b3c6-4e9f-be9c-600055153d8e) |
| **Story** | STORY-057 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin không có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra dữ liệu danh sách mẫu thiệp có được trả về hay không.

**Test Data:**
- Tài khoản Admin không có quyền quản lý mẫu thiệp.

**Expected Result:**
- Backend kiểm tra quyền của Admin trước khi trả dữ liệu.
- Hệ thống không hiển thị danh sách mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp.
- Dữ liệu danh sách mẫu thiệp không được trả về cho tài khoản không có quyền.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)

**Rationale:**
> Xác minh quyền quản lý mẫu thiệp được kiểm tra trước khi hệ thống cung cấp dữ liệu quản trị.

---

## ST-057-08-01 — Lỗi truy xuất danh sách và chức năng thử lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0f6b560b-e990-4996-87a9-5c98bad68cc0) |
| **Story** | STORY-057 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Có thể tạo tình huống hệ thống không thể truy xuất danh sách mẫu thiệp từ Core Database.

**Steps:**
1. Tạo tình huống không thể truy xuất danh sách mẫu thiệp từ Core Database.
2. Admin truy cập chức năng quản lý mẫu thiệp.
3. Quan sát phản hồi của hệ thống.
4. Quan sát dữ liệu hiển thị trên danh sách.
5. Chọn “Thử lại” sau khi Core Database có thể truy cập bình thường.
6. Quan sát kết quả tải lại.

**Test Data:**
- Môi trường test có khả năng tạo lỗi khi truy xuất danh sách mẫu thiệp từ Core Database.

**Expected Result:**
- Hệ thống không hiển thị dữ liệu không đầy đủ như một kết quả tải thành công.
- Hệ thống hiển thị thông báo lỗi.
- Hệ thống hiển thị nút “Thử lại”.
- Khi Admin chọn “Thử lại” sau khi Core Database hoạt động bình thường, hệ thống tải lại danh sách mẫu thiệp.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-057/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [BR-197](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9)

**Rationale:**
> Xác minh hệ thống xử lý rõ ràng lỗi truy xuất Core Database và cung cấp khả năng thử tải lại dữ liệu.

---

## ST-057-09-01 — Mẫu thiệp khả dụng cho quy trình tạo thiệp mới của khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f8313002-bf72-4c01-a170-ee03bb2e6b22) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Core Database có nhiều mẫu thiệp với các trạng thái Active, Inactive và `isDelete` khác nhau.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Chuẩn bị mẫu thiệp Active và có `isDelete = false`.
2. Chuẩn bị mẫu thiệp Inactive và có `isDelete = false`.
3. Chuẩn bị mẫu thiệp Active và có `isDelete = true`.
4. Khách hàng truy cập chức năng tạo thiệp.
5. Quan sát danh sách mẫu thiệp khả dụng.

**Test Data:**
- Mẫu thiệp 1: Active, `isDelete = false`.
- Mẫu thiệp 2: Inactive, `isDelete = false`.
- Mẫu thiệp 3: Active, `isDelete = true`.

**Expected Result:**
- Hệ thống hiển thị mẫu thiệp Active và có `isDelete = false` cho khách hàng.
- Hệ thống không hiển thị mẫu thiệp Inactive.
- Hệ thống không hiển thị mẫu thiệp có `isDelete = true`.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-199](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a733299c-59cf-4aa9-8f6e-8f90e358b7bc)

**Rationale:**
> Xác minh quy tắc chỉ các mẫu thiệp đang Active và chưa bị xóa mềm mới được sử dụng trong quy trình tạo thiệp mới.
