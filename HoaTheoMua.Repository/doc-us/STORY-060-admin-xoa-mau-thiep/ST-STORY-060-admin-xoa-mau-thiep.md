# ST — STORY-060 — Admin xóa mẫu thiệp — System Tests

---

## ST-060-01-02 — Xóa mềm mẫu thiệp thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-01-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/983fa862-5009-420a-9b5d-6db4f3f16792) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database. Mẫu thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường. Mẫu thiệp có thể ở trạng thái Active hoặc Inactive.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Quan sát danh sách mẫu thiệp chưa bị xóa mềm.
3. Admin chọn thao tác “Xóa” tại một mẫu thiệp.
4. Quan sát popup xác nhận.
5. Admin chọn “Yes”.
6. Quan sát thông báo của hệ thống.
7. Quan sát lại danh sách mẫu thiệp.
8. Kiểm tra dữ liệu mẫu thiệp trong Core Database.

**Test Data:**
- Một mẫu thiệp tồn tại trong Core Database. `isDelete = false`. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Hệ thống hiển thị thao tác “Xóa” tại mẫu thiệp chưa bị xóa mềm.
- Hệ thống hiển thị popup xác nhận với nội dung “Bạn chắc chắn muốn xóa mẫu thiệp này không?”. Popup có nút “Yes”.
- Sau khi Admin chọn “Yes”, hệ thống cập nhật `isDelete = true` trong Core Database.
- Hệ thống thông báo xóa mẫu thiệp thành công.
- Danh sách mặc định được cập nhật. Mẫu thiệp vừa xóa không còn hiển thị trong danh sách mặc định.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)
- [BR-212](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/153d644a-1458-4f7e-afa0-359ae837765c)

**Rationale:**
> Xác minh luồng chính của chức năng xóa mẫu thiệp bằng cơ chế xóa mềm và cập nhật đúng danh sách quản trị sau khi xóa.

---

## ST-060-02-01 — Hủy hoặc đóng popup xác nhận xóa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/21bc334b-feb2-4404-b850-f93a2d3bb9db) |
| **Story** | STORY-060 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database. Mẫu thiệp chưa bị xóa mềm.
- Admin đang ở popup xác nhận xóa mẫu thiệp.

**Steps:**
1. Admin chọn hủy hoặc đóng popup xác nhận xóa.
2. Quan sát popup.
3. Quan sát danh sách mẫu thiệp.
4. Kiểm tra trường `isDelete` trong Core Database.

**Test Data:**
- Một mẫu thiệp có `isDelete = false`.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật trường `isDelete`.
- Mẫu thiệp giữ nguyên trạng thái và dữ liệu trước khi thao tác.
- Mẫu thiệp vẫn hiển thị trong danh sách mẫu thiệp.
- Core Database giữ nguyên `isDelete = false`.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)

**Rationale:**
> Xác minh việc Admin hủy hoặc đóng popup không làm thay đổi dữ liệu hoặc trạng thái xóa mềm của mẫu thiệp.

---

## ST-060-03-01 — Người dùng không có quyền quản lý mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9850bbe2-f106-4bf4-8d85-90d99e572dbd) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập. Người dùng không phải Admin hoặc không có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database. Mẫu thiệp chưa bị xóa mềm.

**Steps:**
1. Người dùng thực hiện yêu cầu xóa mẫu thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trường `isDelete` của mẫu thiệp trong Core Database.
4. Kiểm tra dữ liệu và trạng thái hiện tại của mẫu thiệp.

**Test Data:**
- Tài khoản không có quyền quản lý mẫu thiệp. Một mẫu thiệp có `isDelete = false`.

**Expected Result:**
- Hệ thống kiểm tra quyền trước khi cập nhật `isDelete`.
- Hệ thống từ chối yêu cầu xóa.
- Hệ thống không cập nhật trường `isDelete`.
- Hệ thống hiển thị thông báo phù hợp.
- Dữ liệu và trạng thái hiện tại của mẫu thiệp được giữ nguyên.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý mẫu thiệp mới được phép thực hiện thao tác xóa mềm.

---

## ST-060-04-01 — Xóa mẫu thiệp không tồn tại hoặc đã xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/177e0a00-26dd-404e-af2b-6f672d05351f) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Chuẩn bị trường hợp mẫu thiệp không còn tồn tại hoặc đã có `isDelete = true`.
2. Admin gửi yêu cầu xóa mẫu thiệp đó.
3. Quan sát phản hồi của hệ thống.
4. Quan sát danh sách mẫu thiệp sau phản hồi.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Mẫu thiệp không tồn tại trong Core Database.
- Trường hợp 2: Mẫu thiệp tồn tại nhưng `isDelete = true`.

**Expected Result:**
- Hệ thống không thực hiện cập nhật xóa.
- Hệ thống không thực hiện xóa lại mẫu thiệp đã có `isDelete = true`.
- Hệ thống hiển thị thông báo phù hợp.
- Hệ thống tải lại danh sách mẫu thiệp hiện tại.
- Không phát sinh thay đổi dữ liệu không cần thiết trong Core Database.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)

**Rationale:**
> Xác minh hệ thống xử lý an toàn khi mẫu thiệp không còn tồn tại hoặc đã được xóa mềm trước đó.

---

## ST-060-05-01 — Cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df9e0232-fd3a-4447-bbed-72dbb7bf0a00) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và có `isDelete = false`.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác “Xóa” tại mẫu thiệp.
2. Admin chọn “Yes” tại popup xác nhận.
3. Tạo tình huống hệ thống không thể cập nhật `isDelete = true` trong Core Database.
4. Quan sát phản hồi của hệ thống.
5. Quan sát mẫu thiệp trong danh sách.
6. Kiểm tra trường `isDelete` trong Core Database.
7. Khôi phục hệ thống về trạng thái bình thường và thử lại.

**Test Data:**
- Một mẫu thiệp có `isDelete = false`. Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không ghi nhận mẫu thiệp là đã xóa.
- Hệ thống không hiển thị thông báo xóa thành công.
- Hệ thống hiển thị thông báo lỗi.
- Hệ thống cho phép Admin thử lại.
- Mẫu thiệp vẫn hiển thị trong danh sách nếu dữ liệu chưa được cập nhật thành công.
- Core Database vẫn giữ `isDelete = false`.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)

**Rationale:**
> Xác minh hệ thống không ghi nhận xóa thành công khi Core Database chưa cập nhật thành công.

---

## ST-060-06-01 — Mất kết nối hoặc lỗi hệ thống trong quá trình xử lý

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e664e233-fdfc-4af8-90d5-85d8ce2bfadf) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và có `isDelete = false`.
- Có thể tạo tình huống mất kết nối hoặc lỗi hệ thống trong quá trình xử lý xóa.

**Steps:**
1. Admin chọn thao tác “Xóa”.
2. Admin chọn “Yes” tại popup xác nhận.
3. Tạo tình huống mất kết nối hoặc lỗi hệ thống trong quá trình xử lý.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra trạng thái mẫu thiệp trong Core Database.
6. Quan sát khả năng thực hiện “Thử lại”.

**Test Data:**
- Một mẫu thiệp có `isDelete = false`. Môi trường test có khả năng mô phỏng mất kết nối hoặc lỗi hệ thống.

**Expected Result:**
- Hệ thống không hiển thị kết quả xóa thành công khi chưa xác nhận cập nhật thành công từ Core Database.
- Hệ thống hiển thị thông báo lỗi.
- Hệ thống hiển thị nút hoặc khả năng “Thử lại”.
- Trạng thái cuối cùng của mẫu thiệp phải phù hợp với dữ liệu thực tế trong Core Database.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)

**Rationale:**
> Xác minh trạng thái trên giao diện không được ghi nhận thành công khi kết quả cập nhật Core Database chưa được xác nhận.

---

## ST-060-07-01 — Không xóa vật lý dữ liệu hoặc file ảnh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/69b09174-07b4-443b-bf8a-40c32e9a16c7) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database. Mẫu thiệp có `isDelete = false`.
- Mẫu thiệp có ảnh template/ảnh Preview được lưu trong storage.

**Steps:**
1. Ghi nhận bản ghi mẫu thiệp trong Core Database.
2. Ghi nhận ảnh template/ảnh Preview trong storage.
3. Admin thực hiện xóa mẫu thiệp.
4. Admin chọn “Yes” tại popup xác nhận.
5. Kiểm tra bản ghi mẫu thiệp trong Core Database.
6. Kiểm tra ảnh template/ảnh Preview trong storage.

**Test Data:**
- Một mẫu thiệp có: `isDelete = false`. Ảnh template/ảnh Preview tồn tại trong storage.

**Expected Result:**
- Hệ thống cập nhật `isDelete = true`.
- Bản ghi mẫu thiệp vẫn tồn tại trong Core Database. Hệ thống không xóa vật lý dữ liệu mẫu thiệp.
- Ảnh template/ảnh Preview vẫn tồn tại trong storage. Hệ thống không xóa vật lý ảnh khi thực hiện xóa mềm.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-210](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c)
- [BR-214](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcf5f504-3483-402c-a3f3-d9216fca6d06)

**Rationale:**
> Xác minh thao tác xóa chỉ cập nhật isDelete và không thực hiện xóa cứng dữ liệu hoặc file ảnh.

---

## ST-060-08-01 — Mẫu thiệp đã xóa mềm không hiển thị cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1333637c-aafe-4ed6-b2f0-91f3c3061b8b) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Admin thực hiện xóa mềm mẫu thiệp.
2. Xác nhận mẫu thiệp có `isDelete = true` trong Core Database.
3. Khách hàng truy cập quy trình tạo thiệp mới.
4. Khách hàng tải danh sách mẫu thiệp khả dụng.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp ban đầu có: `isDelete = false`. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Sau khi xóa mềm, mẫu thiệp có `isDelete = true`.
- Hệ thống không hiển thị mẫu thiệp đã xóa mềm cho khách hàng lựa chọn khi tạo thiệp mới.
- Mẫu thiệp có `isDelete = true` không được trả về như mẫu thiệp khả dụng dù trạng thái quản lý trước đó là Active.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-211](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987)

**Rationale:**
> Xác minh mẫu thiệp đã bị xóa mềm không còn được sử dụng trong quy trình tạo thiệp mới.

---

## ST-060-09-01 — Không làm mất dữ liệu lịch sử liên quan

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/afbc97eb-4eec-4ac1-b019-8ee4ed595533) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp có `isDelete = false`.
- Mẫu thiệp đã từng được sử dụng trong thiệp, Checkout hoặc Order.
- Ảnh template/ảnh Preview của mẫu thiệp vẫn tồn tại trong storage.

**Steps:**
1. Ghi nhận các thiệp, Checkout hoặc Order đang liên kết với mẫu thiệp.
2. Ghi nhận ảnh template/ảnh Preview hiện tại.
3. Admin thực hiện xóa mềm mẫu thiệp.
4. Kiểm tra bản ghi mẫu thiệp trong Core Database.
5. Kiểm tra các liên kết lịch sử.
6. Kiểm tra ảnh template/ảnh Preview trong storage.

**Test Data:**
- Một mẫu thiệp có ít nhất một liên kết với: Thiệp, Checkout, hoặc Order.

**Expected Result:**
- Mẫu thiệp được cập nhật `isDelete = true`.
- Hệ thống không làm mất liên kết giữa mẫu thiệp và dữ liệu lịch sử đã tồn tại.
- Thiệp, Checkout hoặc Order đã phát sinh trước đó vẫn giữ nguyên dữ liệu.
- Dữ liệu mẫu thiệp vẫn được giữ trong Core Database.
- Ảnh template/ảnh Preview vẫn được giữ trong storage để phục vụ xem lịch sử.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-213](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4205572-ff8e-4108-8dcc-709f86faed53)
- [BR-214](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcf5f504-3483-402c-a3f3-d9216fca6d06)

**Rationale:**
> Xác minh xóa mềm mẫu thiệp không làm mất hoặc sai lệch dữ liệu lịch sử đã sử dụng mẫu thiệp đó.

---

## ST-060-10-01 — Khách hàng đang tạo thiệp khi mẫu thiệp bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7b25dcef-0ac1-4398-807c-56633f49cf83) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang thực hiện quy trình tạo thiệp.
- Khách hàng đã chọn một mẫu thiệp khi mẫu thiệp có `isDelete = false`. Khách hàng chưa hoàn thành tạo thiệp.
- Admin đã đăng nhập và có quyền quản lý mẫu thiệp.

**Steps:**
1. Khách hàng chọn một mẫu thiệp khả dụng.
2. Khách hàng giữ nguyên phiên tạo thiệp và chưa hoàn thành.
3. Admin xóa mềm đúng mẫu thiệp mà khách hàng đang sử dụng.
4. Xác nhận mẫu thiệp có `isDelete = true` trong Core Database.
5. Khách hàng chọn hoàn thành tạo thiệp.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra việc tạo thiệp mới.

**Test Data:**
- Một mẫu thiệp ban đầu có: `isDelete = false`. Khách hàng đã chọn mẫu này trước khi Admin thực hiện xóa mềm.

**Expected Result:**
- Khi khách hàng hoàn thành tạo thiệp, hệ thống kiểm tra lại `isDelete` và trạng thái mẫu thiệp từ Core Database.
- Hệ thống xác định mẫu thiệp đã bị xóa mềm.
- Hệ thống không tạo thiệp mới với mẫu thiệp đó.
- Hệ thống thông báo mẫu thiệp đã chọn không còn khả dụng.
- Hệ thống yêu cầu khách hàng chọn một mẫu thiệp khác còn khả dụng.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-211](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987)

**Rationale:**
> Xác minh hệ thống kiểm tra lại tính khả dụng của mẫu thiệp tại thời điểm xử lý cuối cùng để ngăn sử dụng mẫu đã bị Admin xóa mềm.

---

## ST-060-11-01 — Danh sách mặc định không hiển thị mẫu thiệp đã xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/540a9966-8259-43da-88b3-aff379d5f8fe) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Core Database có cả mẫu thiệp chưa xóa mềm và mẫu thiệp đã xóa mềm.

**Steps:**
1. Chuẩn bị một mẫu thiệp có `isDelete = false`.
2. Chuẩn bị một mẫu thiệp có `isDelete = true`.
3. Admin truy cập danh sách mẫu thiệp mặc định.
4. Quan sát các mẫu thiệp được hiển thị.
5. Đối chiếu với Core Database.

**Test Data:**
- Mẫu thiệp A: `isDelete = false`. Mẫu thiệp B: `isDelete = true`.

**Expected Result:**
- Danh sách mẫu thiệp mặc định chỉ hiển thị mẫu thiệp có `isDelete = false`.
- Mẫu thiệp có `isDelete = true` không được hiển thị.
- `isDelete` không được hiển thị như một cột trạng thái trong danh sách.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [STORY-060/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b)
- [BR-212](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/153d644a-1458-4f7e-afa0-359ae837765c)

**Rationale:**
> Xác minh quy tắc lọc danh sách Admin mặc định sau khi một mẫu thiệp đã được xóa mềm.
