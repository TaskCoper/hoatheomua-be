# ST — STORY-063 — Admin chuyển trạng thái kích thước thiệp — System Tests

---

## ST-063-01-01 — Chuyển trạng thái từ Active sang Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/55193e67-5ffb-488a-9c6c-dda87adcc825) |
| **Story** | STORY-063 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đang thao tác trên một kích thước thiệp được hiển thị trong danh sách quản lý.
- Kích thước thiệp đang ở trạng thái Active.
- Kích thước thiệp còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách kích thước thiệp.
2. Admin chọn thao tác chuyển sang Inactive tại kích thước thiệp đang Active.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát trạng thái kích thước thiệp trong danh sách.
7. Quan sát thao tác được hiển thị sau khi cập nhật.
8. Kiểm tra trạng thái kích thước thiệp trong Core Database.

**Test Data:**
- Một kích thước thiệp có: Trạng thái = Active. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận chuyển kích thước thiệp sang Inactive.
- Hệ thống xác định đúng kích thước thiệp cần thay đổi trạng thái.
- Hệ thống kiểm tra kích thước thiệp còn tồn tại và chưa bị xóa mềm.
- Sau khi Admin xác nhận, hệ thống cập nhật trạng thái từ Active sang Inactive trong Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Inactive.
- Thao tác của kích thước thiệp được thay đổi thành chuyển sang Active.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-223](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/07f5b569-3136-44b8-9a6b-4f06f536c384)
- [BR-225](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/98255c98-7f84-4115-be4a-69fcb75f6c1c)
- [BR-226](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6d68f5ff-d27c-4993-a2de-066cc2204208)
- [BR-227](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2411f028-caea-431a-abbc-eefe9a7a578e)

**Rationale:**
> Xác minh luồng chính khi Admin chuyển kích thước thiệp từ Active sang Inactive và trạng thái được cập nhật nhất quán với Core Database.

---

## ST-063-02-01 — Chuyển trạng thái từ Inactive sang Active

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b92cae83-5508-4221-8958-ccf7424f52d7) |
| **Story** | STORY-063 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đang thao tác trên một kích thước thiệp được hiển thị trong danh sách quản lý.
- Kích thước thiệp đang ở trạng thái Inactive.
- Kích thước thiệp còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách kích thước thiệp.
2. Admin chọn thao tác chuyển sang Active tại kích thước thiệp đang Inactive.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát trạng thái kích thước thiệp trong danh sách.
7. Quan sát thao tác được hiển thị sau khi cập nhật.
8. Kiểm tra trạng thái kích thước thiệp trong Core Database.

**Test Data:**
- Một kích thước thiệp có: Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận chuyển kích thước thiệp sang Active.
- Hệ thống xác định đúng kích thước thiệp cần thay đổi trạng thái.
- Hệ thống kiểm tra kích thước thiệp còn tồn tại và chưa bị xóa mềm.
- Sau khi Admin xác nhận, hệ thống cập nhật trạng thái từ Inactive sang Active trong Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Active.
- Thao tác của kích thước thiệp được thay đổi thành chuyển sang Inactive.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-223](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/07f5b569-3136-44b8-9a6b-4f06f536c384)
- [BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)
- [BR-226](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6d68f5ff-d27c-4993-a2de-066cc2204208)
- [BR-227](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2411f028-caea-431a-abbc-eefe9a7a578e)

**Rationale:**
> Xác minh Alternative Flow chuyển kích thước thiệp từ Inactive sang Active.

---

## ST-063-03-01 — Hủy thao tác chuyển trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf2f8ac4-d24c-4dc7-b1c3-1efb725be998) |
| **Story** | STORY-063 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp đang ở trạng thái Active hoặc Inactive. Kích thước thiệp chưa bị xóa mềm.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái kích thước thiệp.
2. Quan sát popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Quan sát trạng thái kích thước thiệp trong danh sách.
5. Kiểm tra trạng thái trong Core Database.

**Test Data:**
- Thực hiện với:
  - Trường hợp 1: kích thước thiệp Active.
  - Trường hợp 2: kích thước thiệp Inactive.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật trạng thái kích thước thiệp.
- Kích thước thiệp giữ nguyên trạng thái trước khi thao tác.
- Danh sách không thay đổi trạng thái của kích thước đó.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-223](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/07f5b569-3136-44b8-9a6b-4f06f536c384)

**Rationale:**
> Xác minh thao tác hủy hoặc đóng popup không làm thay đổi trạng thái kích thước thiệp.

---

## ST-063-04-01 — Kích thước không tồn tại hoặc đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/598ce8ab-fe13-4ebe-a117-48041cc88fec) |
| **Story** | STORY-063 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đã tải danh sách kích thước thiệp và chuẩn bị thực hiện chuyển trạng thái.

**Steps:**
1. Chuẩn bị trường hợp kích thước thiệp không còn tồn tại hoặc đã bị xóa mềm sau khi danh sách được tải.
2. Admin xác nhận thao tác chuyển trạng thái kích thước thiệp đó.
3. Quan sát phản hồi của hệ thống.
4. Quan sát danh sách sau phản hồi.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Kích thước thiệp không còn tồn tại trong Core Database.
- Trường hợp 2: Kích thước thiệp có `isDelete = true` trước thời điểm yêu cầu cập nhật được xử lý.

**Expected Result:**
- Hệ thống kiểm tra lại kích thước thiệp trước khi cập nhật.
- Hệ thống không thực hiện cập nhật trạng thái.
- Hệ thống thông báo kích thước thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
- Hệ thống tải lại danh sách kích thước thiệp.
- Không phát sinh thay đổi trạng thái trên dữ liệu không còn hợp lệ.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)

**Rationale:**
> Xác minh hệ thống kiểm tra lại sự tồn tại và trạng thái xóa mềm của kích thước trước khi cập nhật.

---

## ST-063-05-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d1c183d4-049a-40d4-ada7-8635783fcc5b) |
| **Story** | STORY-063 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái kích thước thiệp.
2. Admin xác nhận thao tác.
3. Tạo tình huống hệ thống gặp lỗi trong quá trình cập nhật.
4. Quan sát phản hồi của hệ thống.
5. Quan sát trạng thái kích thước thiệp trong danh sách.
6. Kiểm tra trạng thái trong Core Database.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một kích thước thiệp Active hoặc Inactive có `isDelete = false`. Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không thay đổi trạng thái kích thước thiệp khi cập nhật thất bại.
- Kích thước thiệp giữ nguyên trạng thái trước khi thao tác.
- Hệ thống thông báo cập nhật trạng thái thất bại.
- Không tồn tại trạng thái dở dang hoặc không đồng nhất.
- Admin có thể thử lại sau khi lỗi được xử lý.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-223](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/07f5b569-3136-44b8-9a6b-4f06f536c384)

**Rationale:**
> Xác minh lỗi cập nhật không tạo trạng thái dở dang hoặc không nhất quán.

---

## ST-063-06-01 — Người dùng không có quyền quản lý cấu hình thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ddf36b4f-e77a-4922-8a5c-a34a9b8d1f6d) |
| **Story** | STORY-063 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database và chưa bị xóa mềm.

**Steps:**
1. Người dùng gửi yêu cầu chuyển trạng thái kích thước thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trạng thái kích thước thiệp trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình thiệp. Một kích thước thiệp có trạng thái Active hoặc Inactive và `isDelete = false`.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật trạng thái.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi trạng thái kích thước thiệp.
- Hệ thống hiển thị thông báo phù hợp.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-227](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2411f028-caea-431a-abbc-eefe9a7a578e)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình thiệp mới được chuyển trạng thái kích thước thiệp.

---

## ST-063-07-01 — Kích thước Inactive không khả dụng cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/93e5791f-1760-41de-9c10-be800c98a9de) |
| **Story** | STORY-063 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp đang Active và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận kích thước Active đang khả dụng cho khách hàng.
2. Admin chuyển kích thước đó từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách kích thước thiệp khả dụng.
5. Quan sát danh sách kích thước thiệp.

**Test Data:**
- Một kích thước thiệp: Trạng thái ban đầu = Active. `isDelete = false`.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, kích thước thiệp có trạng thái Inactive trong Core Database.
- Kích thước Inactive không được hiển thị hoặc sử dụng trong danh sách kích thước khách hàng có thể chọn khi tạo thiệp mới.
- Kích thước vẫn tồn tại và vẫn có thể hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-225](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/98255c98-7f84-4115-be4a-69fcb75f6c1c)

**Rationale:**
> Xác minh tác động của trạng thái Inactive lên khả năng sử dụng kích thước trong quy trình tạo thiệp mới.

---

## ST-063-08-01 — Kích thước Active khả dụng cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/56858ae5-a1ed-4512-95e6-7d14b44be557) |
| **Story** | STORY-063 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp đang Inactive và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận kích thước Inactive không khả dụng cho khách hàng.
2. Admin chuyển kích thước đó từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách kích thước thiệp khả dụng.
5. Quan sát danh sách kích thước thiệp.

**Test Data:**
- Một kích thước thiệp: Trạng thái ban đầu = Inactive. `isDelete = false`.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, kích thước thiệp có trạng thái Active trong Core Database.
- Kích thước Active và có `isDelete = false` được phép sử dụng khi khách hàng tạo thiệp mới.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)

**Rationale:**
> Xác minh kích thước được đưa trở lại trạng thái khả dụng sau khi chuyển từ Inactive sang Active.

---

## ST-063-09-01 — Chuyển trạng thái không thay đổi dữ liệu cấu hình khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1511c2eb-92c2-4ed8-a95a-60cf8c19147d) |
| **Story** | STORY-063 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Kích thước thiệp có dữ liệu chiều rộng, chiều cao, giá size và số lượng từ tối đa.

**Steps:**
1. Ghi nhận chiều rộng, chiều cao, giá size, số lượng từ tối đa và `isDelete` hiện tại.
2. Admin chuyển trạng thái kích thước thiệp.
3. Admin xác nhận thao tác.
4. Kiểm tra dữ liệu kích thước thiệp trong Core Database sau cập nhật.

**Test Data:**
- Một kích thước thiệp có: Chiều rộng = 10 cm. Chiều cao = 15 cm. Giá size = 20000. Số lượng từ tối đa = 20. `isDelete = false`. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái Active/Inactive của kích thước thiệp.
- Chiều rộng không bị thay đổi.
- Chiều cao không bị thay đổi.
- Giá size không bị thay đổi.
- Số lượng từ tối đa không bị thay đổi.
- `isDelete` không bị thay đổi.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-226](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6d68f5ff-d27c-4993-a2de-066cc2204208)

**Rationale:**
> Xác minh thao tác chuyển trạng thái chỉ thay đổi trường trạng thái và không làm thay đổi dữ liệu cấu hình khác.

---

## ST-063-10-01 — Không làm mất hoặc thay đổi dữ liệu lịch sử liên quan

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d303cbba-82f3-409b-8f13-dba8b4aba0aa) |
| **Story** | STORY-063 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Kích thước thiệp đã từng được sử dụng trong thiệp, Checkout hoặc Order.

**Steps:**
1. Ghi nhận các dữ liệu thiệp, Checkout hoặc Order đang sử dụng kích thước thiệp.
2. Admin chuyển trạng thái kích thước thiệp từ Active sang Inactive hoặc ngược lại.
3. Admin xác nhận thao tác.
4. Kiểm tra lại các dữ liệu lịch sử sau khi cập nhật.

**Test Data:**
- Một kích thước thiệp đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp, Checkout, Order.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái của kích thước thiệp.
- Các thiệp, Checkout hoặc Order đã sử dụng kích thước trước đó không bị xóa hoặc thay đổi.
- Các liên kết lịch sử với kích thước thiệp được giữ nguyên.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-063/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [BR-226](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6d68f5ff-d27c-4993-a2de-066cc2204208)

**Rationale:**
> Xác minh chuyển trạng thái kích thước thiệp không làm mất hoặc thay đổi dữ liệu lịch sử đã phát sinh.

---

## ST-063-11-01 — Kích thước Active nhưng đã bị xóa mềm không khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7348bbc0-ebb5-469c-bc83-4844bd700375) |
| **Story** | STORY-063 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một kích thước thiệp trong Core Database.
- Kích thước thiệp có trạng thái Active nhưng `isDelete = true`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Khách hàng tải danh sách kích thước thiệp khả dụng.
2. Quan sát danh sách kích thước thiệp.
3. Thử sử dụng kích thước Active nhưng có `isDelete = true` nếu có thể gửi yêu cầu trực tiếp.

**Test Data:**
- Kích thước thiệp: Trạng thái = Active. `isDelete = true`.

**Expected Result:**
- Hệ thống không xem kích thước Active có `isDelete = true` là kích thước khả dụng.
- Kích thước đó không được hiển thị hoặc sử dụng trong quy trình tạo thiệp mới.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)

**Rationale:**
> Xác minh điều kiện khả dụng yêu cầu đồng thời trạng thái Active và isDelete = false.
