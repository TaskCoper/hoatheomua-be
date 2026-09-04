# ST — STORY-064 — Admin chuyển trạng thái cấu hình phụ phí thiệp viết tay — System Tests

---

## ST-064-01-01 — Chuyển trạng thái từ Active sang Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a7e86a14-c7f9-4589-9618-20f053896e3a) |
| **Story** | STORY-064 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí viết tay được hiển thị trong danh sách quản lý.
- Cấu hình đang ở trạng thái Active. Cấu hình còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Admin chọn thao tác chuyển sang Inactive tại cấu hình đang Active.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát trạng thái cấu hình trong danh sách.
7. Quan sát thao tác được hiển thị sau khi cập nhật.
8. Kiểm tra trạng thái cấu hình trong Core Database.

**Test Data:**
- Một cấu hình phụ phí viết tay có: Trạng thái = Active. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Inactive.
- Hệ thống xác định đúng cấu hình cần thay đổi trạng thái.
- Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
- Sau khi Admin xác nhận, hệ thống cập nhật trạng thái từ Active sang Inactive trong Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Inactive.
- Thao tác của cấu hình được thay đổi thành chuyển sang Active.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-218](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61)
- [BR-229](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2)
- [BR-230](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906)
- [BR-231](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1)

**Rationale:**
> Xác minh luồng chính khi Admin chuyển cấu hình phụ phí viết tay từ Active sang Inactive.

---

## ST-064-02-01 — Chuyển từ Inactive sang Active khi không chồng lấn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd126d4c-4314-44cd-9553-ea0359d34d1e) |
| **Story** | STORY-064 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí viết tay được hiển thị trong danh sách quản lý.
- Cấu hình đang ở trạng thái Inactive. Cấu hình còn tồn tại và chưa bị xóa mềm.
- Khoảng số lượng từ của cấu hình không chồng lấn với các cấu hình Active khác có `isDelete = false`.

**Steps:**
1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Admin chọn thao tác chuyển sang Active tại cấu hình đang Inactive.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát trạng thái cấu hình trong danh sách.
7. Quan sát thao tác được hiển thị sau khi cập nhật.
8. Kiểm tra trạng thái cấu hình trong Core Database.

**Test Data:**
- Cấu hình cần chuyển: Khoảng số lượng từ = 11 đến 20. Trạng thái = Inactive. `isDelete = false`.
- Các cấu hình Active khác không có khoảng số lượng từ chồng lấn với 11 đến 20.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Active.
- Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
- Hệ thống kiểm tra khoảng số lượng từ không chồng lấn với các cấu hình Active khác có `isDelete = false`.
- Sau khi Admin xác nhận, hệ thống cập nhật trạng thái từ Inactive sang Active trong Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Active.
- Thao tác của cấu hình được thay đổi thành chuyển sang Inactive.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-218](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61)
- [BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87)
- [BR-230](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906)
- [BR-231](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh Alternative Flow chuyển cấu hình từ Inactive sang Active khi khoảng số lượng từ không chồng lấn.

---

## ST-064-03-01 — Hủy thao tác chuyển trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae976b76-b5c8-476a-b64d-d0e8b08e5958) |
| **Story** | STORY-064 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay đang ở trạng thái Active hoặc Inactive. Cấu hình chưa bị xóa mềm.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái cấu hình phụ phí viết tay.
2. Quan sát popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Quan sát trạng thái cấu hình trong danh sách.
5. Kiểm tra trạng thái trong Core Database.

**Test Data:**
- Thực hiện với:
  - Trường hợp 1: cấu hình Active.
  - Trường hợp 2: cấu hình Inactive.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật trạng thái cấu hình.
- Cấu hình giữ nguyên trạng thái trước khi thao tác.
- Danh sách không thay đổi trạng thái của cấu hình đó.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-218](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61)

**Rationale:**
> Xác minh việc Admin hủy hoặc đóng popup không làm thay đổi trạng thái cấu hình.

---

## ST-064-04-01 — Cấu hình không tồn tại hoặc đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1bab02eb-1e75-4bbd-be33-cd5f36ad3019) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đã tải danh sách cấu hình phụ phí viết tay và chuẩn bị thực hiện chuyển trạng thái.

**Steps:**
1. Chuẩn bị trường hợp cấu hình không còn tồn tại hoặc đã bị xóa mềm sau khi danh sách được tải.
2. Admin xác nhận thao tác chuyển trạng thái cấu hình đó.
3. Quan sát phản hồi của hệ thống.
4. Quan sát danh sách sau phản hồi.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Cấu hình không còn tồn tại trong Core Database.
- Trường hợp 2: Cấu hình có `isDelete = true` trước thời điểm yêu cầu cập nhật được xử lý.

**Expected Result:**
- Hệ thống kiểm tra lại cấu hình trước khi cập nhật.
- Hệ thống không thực hiện cập nhật trạng thái.
- Hệ thống thông báo cấu hình không còn tồn tại hoặc dữ liệu đã thay đổi.
- Hệ thống tải lại danh sách cấu hình phụ phí viết tay.
- Không phát sinh thay đổi trạng thái trên dữ liệu không còn hợp lệ.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)

**Rationale:**
> Xác minh hệ thống kiểm tra lại sự tồn tại và trạng thái xóa mềm của cấu hình trước khi cập nhật.

---

## ST-064-05-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5664f296-220a-4d10-9fa8-c536961c6b4f) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình tồn tại và chưa bị xóa mềm.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái cấu hình.
2. Admin xác nhận thao tác.
3. Tạo tình huống hệ thống gặp lỗi trong quá trình cập nhật.
4. Quan sát phản hồi của hệ thống.
5. Quan sát trạng thái cấu hình trong danh sách.
6. Kiểm tra trạng thái trong Core Database.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một cấu hình Active hoặc Inactive có `isDelete = false`. Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không thay đổi trạng thái cấu hình khi cập nhật thất bại.
- Cấu hình giữ nguyên trạng thái trước khi thao tác.
- Hệ thống thông báo cập nhật trạng thái thất bại.
- Không tồn tại trạng thái dở dang hoặc không đồng nhất.
- Admin có thể thử lại sau khi lỗi được xử lý.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)

**Rationale:**
> Xác minh lỗi cập nhật không tạo trạng thái cấu hình dở dang hoặc không nhất quán.

---

## ST-064-06-01 — Người dùng không có quyền quản lý

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/37690755-d9cf-49dc-83de-0c5996b2e1e0) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay tồn tại và chưa bị xóa mềm.

**Steps:**
1. Người dùng gửi yêu cầu chuyển trạng thái cấu hình phụ phí viết tay.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trạng thái cấu hình trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình giá thiệp. Một cấu hình có trạng thái Active hoặc Inactive và `isDelete = false`.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật trạng thái.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi trạng thái cấu hình.
- Hệ thống hiển thị thông báo phù hợp.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-231](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình giá thiệp mới được chuyển trạng thái cấu hình phụ phí viết tay.

---

## ST-064-07-01 — Chuyển Inactive sang Active khi có chồng lấn (không cho phép)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/480cc9d6-2970-40da-a3ee-ed8115f5c76a) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình cần chuyển đang Inactive và có `isDelete = false`.
- Đã tồn tại ít nhất một cấu hình Active khác có `isDelete = false`.

**Steps:**
1. Chuẩn bị một cấu hình Inactive có khoảng số lượng từ chồng lấn với cấu hình Active khác.
2. Admin chọn chuyển cấu hình Inactive sang Active.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát phản hồi của hệ thống.
6. Kiểm tra trạng thái cấu hình trong Core Database.

**Test Data:**
- Cấu hình A: Khoảng số lượng từ = 1 đến 10. Trạng thái = Active. `isDelete = false`.
- Cấu hình B: Khoảng số lượng từ = 8 đến 15. Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Hệ thống phát hiện khoảng số lượng từ của cấu hình B chồng lấn với cấu hình Active khác.
- Hệ thống không chuyển cấu hình B sang Active.
- Cấu hình B giữ nguyên trạng thái Inactive.
- Hệ thống hiển thị thông báo khoảng số lượng từ đang chồng lấn với cấu hình khác.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh không thể có hai cấu hình Active chưa bị xóa mềm có khoảng số lượng từ chồng lấn.

---

## ST-064-08-01 — Cấu hình Inactive không còn được áp dụng cho thiệp mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9344cd6e-bff1-43ec-b9be-8ee77b3c9bec) |
| **Story** | STORY-064 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay đang Active và có `isDelete = false`.
- Cấu hình phù hợp với số lượng từ của một thiệp Calligraphy mới.

**Steps:**
1. Xác nhận cấu hình Active đang được sử dụng để xác định phụ phí cho khoảng số lượng từ tương ứng.
2. Admin chuyển cấu hình từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ thuộc khoảng của cấu hình.
5. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Giá phụ phí = 20000. Trạng thái ban đầu = Active. `isDelete = false`.
- Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, cấu hình có trạng thái Inactive trong Core Database.
- Hệ thống không sử dụng cấu hình Inactive để xác định phụ phí cho thiệp Calligraphy mới.
- Cấu hình Inactive vẫn được giữ trong Core Database và có thể hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-229](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2)

**Rationale:**
> Xác minh cấu hình Inactive không còn tham gia xác định phụ phí cho các yêu cầu thiệp Calligraphy mới.

---

## ST-064-09-01 — Cấu hình chuyển về Active được áp dụng cho thiệp mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/818bebb1-4427-401d-a14f-71651968c989) |
| **Story** | STORY-064 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay đang Inactive và có `isDelete = false`.
- Khoảng số lượng từ không chồng lấn với các cấu hình Active khác.

**Steps:**
1. Xác nhận cấu hình Inactive không được sử dụng để xác định phụ phí.
2. Admin chuyển cấu hình từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ phù hợp với khoảng cấu hình.
5. Quan sát cấu hình được sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 11 đến 20. Giá phụ phí = 30000. Trạng thái ban đầu = Inactive. `isDelete = false`.
- Không có cấu hình Active khác chồng lấn khoảng 11 đến 20.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, cấu hình có trạng thái Active trong Core Database.
- Cấu hình Active và có `isDelete = false` được phép sử dụng để xác định phụ phí cho thiệp Calligraphy mới có số lượng từ phù hợp.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh cấu hình được đưa trở lại trạng thái khả dụng khi chuyển từ Inactive sang Active.

---

## ST-064-10-01 — Chuyển trạng thái không thay đổi dữ liệu cấu hình khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1fb763e5-dbd0-4a7f-a7e0-d56171c42a3d) |
| **Story** | STORY-064 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay tồn tại và chưa bị xóa mềm. Cấu hình có khoảng số lượng từ và giá phụ phí xác định.

**Steps:**
1. Ghi nhận số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và `isDelete` hiện tại.
2. Admin chuyển trạng thái cấu hình.
3. Admin xác nhận thao tác.
4. Kiểm tra dữ liệu cấu hình trong Core Database sau cập nhật.

**Test Data:**
- Cấu hình: Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. `isDelete = false`. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái Active/Inactive của cấu hình.
- Số lượng từ bắt đầu không bị thay đổi.
- Số lượng từ kết thúc không bị thay đổi.
- Giá phụ phí không bị thay đổi.
- `isDelete` không bị thay đổi.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-230](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906)

**Rationale:**
> Xác minh chuyển trạng thái chỉ thay đổi trạng thái quản lý và không làm thay đổi dữ liệu cấu hình khác.

---

## ST-064-11-01 — Không làm thay đổi dữ liệu lịch sử đã áp dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9fefaeea-5fd5-48c1-bd33-1684e7cc99b4) |
| **Story** | STORY-064 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí viết tay tồn tại và chưa bị xóa mềm.
- Cấu hình đã được áp dụng cho thiệp, Checkout hoặc Order đã tồn tại.

**Steps:**
1. Ghi nhận dữ liệu thiệp, Checkout hoặc Order đã áp dụng cấu hình phụ phí.
2. Admin chuyển trạng thái cấu hình từ Active sang Inactive hoặc ngược lại.
3. Admin xác nhận thao tác.
4. Kiểm tra lại các dữ liệu đã tồn tại sau khi chuyển trạng thái.

**Test Data:**
- Một cấu hình đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp, Checkout, Order.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái cấu hình phụ phí viết tay.
- Các thiệp, Checkout hoặc Order đã tồn tại không bị thay đổi bởi thao tác chuyển trạng thái.
- Dữ liệu phụ phí đã áp dụng trước đó được giữ nguyên.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-064/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [BR-230](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906)

**Rationale:**
> Xác minh thay đổi trạng thái cấu hình không làm thay đổi dữ liệu lịch sử đã được áp dụng trước đó.

---

## ST-064-12-01 — Cấu hình Active nhưng đã xóa mềm không được sử dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7b1555ba-9c6e-490c-bf8e-b5e8816a4995) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một cấu hình phụ phí viết tay trong Core Database.
- Cấu hình có trạng thái Active nhưng `isDelete = true`.
- Hệ thống đang xác định phụ phí cho thiệp Calligraphy mới.

**Steps:**
1. Chuẩn bị thiệp Calligraphy có số lượng từ thuộc khoảng của cấu hình.
2. Thực hiện yêu cầu xác định phụ phí.
3. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Trạng thái = Active. `isDelete = true`.
- Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Hệ thống không sử dụng cấu hình Active có `isDelete = true` để xác định phụ phí.
- Chỉ cấu hình Active và có `isDelete = false` mới được xem là cấu hình khả dụng.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87)

**Rationale:**
> Xác minh điều kiện sử dụng cấu hình yêu cầu đồng thời trạng thái Active và isDelete = false.
