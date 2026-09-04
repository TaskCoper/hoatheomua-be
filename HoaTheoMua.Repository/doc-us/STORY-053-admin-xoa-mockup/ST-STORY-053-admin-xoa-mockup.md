# ST — STORY-053 — Admin xóa Mockup — System Tests

---

## ST-053-01-01 — Xóa Mockup thành công bằng cơ chế xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fbd829f-a71e-4e49-9e79-0cd5d8b9e11c) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Mockup tồn tại trong Core Database.
- Mockup chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.
- Mockup có thể ở trạng thái Hoạt động hoặc Không hoạt động.

**Steps:**
1. Admin truy cập chức năng “Quản lý Mockup”.
2. Quan sát danh sách Mockup chưa bị xóa mềm.
3. Admin chọn thao tác “Xóa” tại một Mockup.
4. Quan sát popup xác nhận.
5. Admin chọn “Yes”.
6. Quan sát thông báo của hệ thống.
7. Quan sát lại danh sách Mockup.
8. Kiểm tra trường `isDelete` của Mockup trong Core Database.

**Test Data:**
- Một Mockup có: `isDelete = false`. Trạng thái = Hoạt động hoặc Không hoạt động.

**Expected Result:**
- Hệ thống hiển thị thao tác “Xóa” tại Mockup chưa bị xóa mềm.
- Hệ thống hiển thị popup xác nhận với nội dung “Bạn chắc chắn muốn xóa Mockup này không?”. Popup có nút “Yes”.
- Sau khi Admin xác nhận, hệ thống cập nhật `isDelete = true` trong Core Database.
- Hệ thống thông báo xóa Mockup thành công.
- Danh sách Mockup mặc định được cập nhật.
- Mockup vừa xóa không còn hiển thị trong danh sách mặc định.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-176](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3cfc0039-7193-4c21-a52c-f2c326892a3f)
- [BR-184](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/547bdd7d-ff22-4cb8-81d0-87fb3ff45c3e)
- [BR-186](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4)

**Rationale:**
> Xác minh luồng chính khi Admin xóa Mockup bằng cơ chế xóa mềm và danh sách quản lý được cập nhật nhất quán với Core Database.

---

## ST-053-02-01 — Hủy thao tác xóa Mockup tại popup

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cd923993-3336-405c-9ffd-782caff2b6cc) |
| **Story** | STORY-053 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup tồn tại trong Core Database. Mockup có `isDelete = false`.
- Hệ thống đang hiển thị popup xác nhận xóa Mockup.

**Steps:**
1. Admin chọn hủy hoặc đóng popup xác nhận.
2. Quan sát popup.
3. Quan sát danh sách Mockup.
4. Kiểm tra trường `isDelete` trong Core Database.

**Test Data:**
- Một Mockup có `isDelete = false`.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật trường `isDelete`.
- Mockup giữ nguyên dữ liệu và trạng thái trước khi thao tác.
- Mockup vẫn hiển thị trong danh sách Mockup.
- Core Database giữ nguyên `isDelete = false`.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-184](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/547bdd7d-ff22-4cb8-81d0-87fb3ff45c3e)

**Rationale:**
> Xác minh việc Admin hủy hoặc đóng popup không làm thay đổi dữ liệu Mockup.

---

## ST-053-03-01 — Chặn người dùng không có quyền quản lý Mockup thực hiện xóa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/049ff842-30f9-4673-a37f-e8a16096d07c) |
| **Story** | STORY-053 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không phải Admin hoặc không có quyền quản lý Mockup.
- Mockup tồn tại trong Core Database. Mockup chưa bị xóa mềm.

**Steps:**
1. Người dùng gửi yêu cầu xóa Mockup.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trường `isDelete` của Mockup trong Core Database.
4. Kiểm tra dữ liệu và trạng thái hiện tại của Mockup.

**Test Data:**
- Tài khoản không có quyền quản lý Mockup.
- Một Mockup có `isDelete = false`.

**Expected Result:**
- Hệ thống kiểm tra quyền trước khi cập nhật `isDelete`.
- Hệ thống từ chối thao tác xóa.
- Hệ thống hiển thị thông báo phù hợp về việc người dùng không có quyền thực hiện chức năng.
- Hệ thống không cập nhật `isDelete`.
- Dữ liệu và trạng thái hiện tại của Mockup được giữ nguyên.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý Mockup mới được phép xóa Mockup.

---

## ST-053-04-01 — Không cho xóa Mockup không tồn tại hoặc đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/19609b09-04c1-4e00-bf1a-9f6cb9db0b4c) |
| **Story** | STORY-053 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Admin đã tải danh sách Mockup trước đó.
- Chuẩn bị trường hợp Mockup không còn tồn tại hoặc đã có `isDelete = true`.

**Steps:**
1. Admin gửi yêu cầu xóa Mockup đó.
2. Quan sát phản hồi của hệ thống.
3. Quan sát danh sách sau phản hồi.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Mockup không còn tồn tại trong Core Database.
- Trường hợp 2: Mockup tồn tại nhưng `isDelete = true`.

**Expected Result:**
- Hệ thống không thực hiện cập nhật xóa.
- Hệ thống không cập nhật lại Mockup đã có `isDelete = true`.
- Hệ thống hiển thị thông báo phù hợp.
- Hệ thống tải lại danh sách Mockup hiện tại.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)

**Rationale:**
> Xác minh hệ thống xử lý an toàn khi Mockup không còn tồn tại hoặc đã bị xóa mềm.

---

## ST-053-05-01 — Xử lý khi cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a7aac5f8-d0d1-4fae-a530-db1db8cba745) |
| **Story** | STORY-053 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup tồn tại và có `isDelete = false`.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác “Xóa” tại Mockup.
2. Admin chọn “Yes” tại popup xác nhận.
3. Tạo tình huống hệ thống không thể cập nhật `isDelete = true` trong Core Database.
4. Quan sát phản hồi của hệ thống.
5. Quan sát Mockup trong danh sách.
6. Kiểm tra trường `isDelete` trong Core Database.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một Mockup có `isDelete = false`.
- Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không ghi nhận Mockup là đã xóa.
- Hệ thống không hiển thị kết quả xóa thành công.
- Hệ thống hiển thị thông báo lỗi.
- Mockup vẫn hiển thị trong danh sách nếu Core Database chưa cập nhật thành công.
- Core Database vẫn giữ `isDelete = false`.
- Admin có thể thử lại.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-186](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4)

**Rationale:**
> Xác minh hệ thống không ghi nhận xóa thành công khi Core Database chưa cập nhật thành công.

---

## ST-053-06-01 — Xử lý lỗi hệ thống hoặc mất kết nối khi xóa Mockup

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/944a2d24-9013-4098-9dbf-5f736f5a09fc) |
| **Story** | STORY-053 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup tồn tại và có `isDelete = false`.
- Có thể tạo tình huống mất kết nối hoặc lỗi hệ thống trong quá trình xử lý xóa.

**Steps:**
1. Admin chọn thao tác “Xóa”.
2. Admin chọn “Yes” tại popup xác nhận.
3. Tạo tình huống mất kết nối hoặc lỗi hệ thống trong quá trình xử lý.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra trạng thái Mockup trong Core Database.
6. Quan sát khả năng thực hiện “Thử lại”.

**Test Data:**
- Một Mockup có `isDelete = false`.
- Môi trường test có khả năng mô phỏng mất kết nối hoặc lỗi hệ thống.

**Expected Result:**
- Hệ thống không hiển thị kết quả xóa thành công khi chưa xác nhận Core Database đã cập nhật thành công.
- Hệ thống hiển thị thông báo lỗi.
- Hệ thống cho phép Admin “Thử lại”.
- Trạng thái hiển thị cuối cùng phải nhất quán với giá trị `isDelete` trong Core Database.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-186](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4)

**Rationale:**
> Xác minh tính nhất quán giữa kết quả hiển thị và trạng thái thực tế trong Core Database khi xảy ra lỗi hệ thống.

---

## ST-053-07-01 — Không xóa vật lý dữ liệu Mockup và ảnh Preview

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec60e477-f8af-4c7c-844c-c9e94e2afc38) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup tồn tại trong Core Database. Mockup có `isDelete = false`.
- Mockup có ảnh Preview được lưu trong storage.

**Steps:**
1. Ghi nhận bản ghi Mockup trong Core Database.
2. Ghi nhận ảnh Preview hiện tại trong storage.
3. Admin thực hiện xóa Mockup.
4. Admin xác nhận thao tác.
5. Kiểm tra bản ghi Mockup trong Core Database.
6. Kiểm tra ảnh Preview trong storage.

**Test Data:**
- Một Mockup có: `isDelete = false`. Ảnh Preview tồn tại trong storage.

**Expected Result:**
- Hệ thống cập nhật `isDelete = true`.
- Bản ghi Mockup vẫn tồn tại trong Core Database.
- Hệ thống không xóa vật lý dữ liệu Mockup.
- Ảnh Preview vẫn tồn tại trong storage.
- Hệ thống không xóa vật lý ảnh Preview khi thực hiện xóa mềm.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-176](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3cfc0039-7193-4c21-a52c-f2c326892a3f)

**Rationale:**
> Xác minh thao tác xóa Mockup chỉ là xóa mềm và không làm mất dữ liệu hoặc ảnh Preview.

---

## ST-053-08-01 — Danh sách quản lý mặc định không hiển thị Mockup đã xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/85d2a951-3bbe-495b-a8df-386e4f696ca9) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Core Database có cả Mockup chưa xóa mềm và Mockup đã xóa mềm.

**Steps:**
1. Chuẩn bị một Mockup có `isDelete = false`.
2. Chuẩn bị một Mockup có `isDelete = true`.
3. Admin truy cập màn hình Quản lý Mockup.
4. Quan sát danh sách mặc định.
5. Đối chiếu với Core Database.

**Test Data:**
- Mockup A: `isDelete = false`.
- Mockup B: `isDelete = true`.

**Expected Result:**
- Danh sách Mockup mặc định chỉ hiển thị Mockup chưa bị xóa mềm.
- Mockup có `isDelete = true` không được hiển thị trong danh sách mặc định.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-181](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7940d811-5cc4-4324-9950-2db88693fb66)
- [BR-186](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4)

**Rationale:**
> Xác minh danh sách quản lý mặc định loại bỏ Mockup đã bị xóa mềm.

---

## ST-053-09-01 — Khách hàng không thấy Mockup đã xóa mềm trong quy trình tạo mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0d080cb5-de2e-4d8d-a87a-36a56ab8970b) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup đang khả dụng cho khách hàng và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo mẫu hoa mới.

**Steps:**
1. Admin xóa mềm Mockup.
2. Xác nhận Mockup có `isDelete = true` trong Core Database.
3. Khách hàng truy cập bước chọn Mockup trong quy trình tạo mẫu hoa mới.
4. Quan sát danh sách Mockup có thể chọn.

**Test Data:**
- Một Mockup ban đầu có: `isDelete = false`. Đang khả dụng cho khách hàng.

**Expected Result:**
- Sau khi xóa mềm, Mockup có `isDelete = true`.
- Hệ thống không hiển thị Mockup đó cho khách hàng lựa chọn trong quy trình tạo mẫu hoa mới.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-030](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-182](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/62e46d0d-d5b3-4af9-9ce5-565ae25fa60c)

**Rationale:**
> Xác minh Mockup đã xóa mềm không còn khả dụng cho yêu cầu tạo mẫu hoa mới.

---

## ST-053-10-01 — Xóa mềm Mockup không làm mất dữ liệu lịch sử đã tồn tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9dde1052-1f3e-47a4-a92b-70e5cf2f1dae) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý Mockup.
- Mockup chưa bị xóa mềm.
- Mockup đã từng được sử dụng trong một yêu cầu tạo mẫu hoa.
- Mockup có ảnh Preview và liên kết dữ liệu lịch sử.

**Steps:**
1. Ghi nhận dữ liệu Mockup, ảnh Preview và các yêu cầu tạo mẫu hoa đang liên kết.
2. Admin thực hiện xóa mềm Mockup.
3. Kiểm tra bản ghi Mockup sau khi xóa.
4. Kiểm tra ảnh Preview.
5. Kiểm tra các liên kết với yêu cầu tạo mẫu hoa đã tồn tại.
6. Mở dữ liệu lịch sử có sử dụng Mockup.

**Test Data:**
- Một Mockup đã được sử dụng trong ít nhất một yêu cầu tạo mẫu hoa đã tồn tại.

**Expected Result:**
- Mockup được cập nhật `isDelete = true` nhưng vẫn tồn tại trong Core Database.
- Ảnh Preview của Mockup vẫn được giữ.
- Liên kết giữa Mockup và các yêu cầu tạo mẫu hoa đã tồn tại không bị mất.
- Dữ liệu Mockup vẫn có thể được sử dụng để hiển thị lịch sử của các yêu cầu đã tồn tại.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-183](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942b4bdb-cd38-4332-802f-c0cb8b540541)

**Rationale:**
> Xác minh xóa mềm Mockup không làm mất dữ liệu và liên kết lịch sử đã phát sinh.

---

## ST-053-11-01 — Không cho phép generate lại mẫu hoa bằng Mockup đã xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/87760566-8279-4690-beb2-8fb9f5ea97c8) |
| **Story** | STORY-053 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Mockup đã từng được sử dụng trong một yêu cầu tạo mẫu hoa đã tồn tại.
- Mockup hiện có `isDelete = true`.
- Khách hàng có thể xem lại dữ liệu lịch sử của yêu cầu.

**Steps:**
1. Khách hàng mở yêu cầu tạo mẫu hoa đã từng sử dụng Mockup.
2. Quan sát dữ liệu lịch sử.
3. Thực hiện hoặc gửi yêu cầu generate lại bằng Mockup đã bị xóa mềm.
4. Quan sát phản hồi của hệ thống.

**Test Data:**
- Một yêu cầu lịch sử liên kết với Mockup có `isDelete = true`.

**Expected Result:**
- Hệ thống vẫn giữ và hiển thị dữ liệu lịch sử liên quan đến Mockup.
- Hệ thống không cho sử dụng Mockup đã xóa mềm để generate lại.
- Nếu khách hàng muốn tạo mẫu hoa mới, khách hàng phải thực hiện lại quy trình tạo mới và chọn Mockup còn khả dụng.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [BR-183](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942b4bdb-cd38-4332-802f-c0cb8b540541)
- [BR-182](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/62e46d0d-d5b3-4af9-9ce5-565ae25fa60c)

**Rationale:**
> Xác minh Mockup đã xóa mềm chỉ còn phục vụ dữ liệu lịch sử và không được tái sử dụng cho yêu cầu generate mới.

---

## ST-053-12-01 — Chặn khách hàng hoàn thành quy trình nếu Mockup đã bị xóa mềm giữa chừng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-053-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8572337c-e4a3-46d6-9a3d-4ca4bd585ff9) |
| **Story** | STORY-053 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang thực hiện quy trình tạo mẫu hoa mới.
- Khách hàng đã chọn một Mockup khi Mockup có `isDelete = false`.
- Khách hàng chưa hoàn thành yêu cầu.
- Admin đã đăng nhập và có quyền quản lý Mockup.

**Steps:**
1. Khách hàng chọn một Mockup khả dụng.
2. Khách hàng giữ nguyên phiên tạo mẫu hoa và chưa hoàn thành.
3. Admin xóa mềm đúng Mockup mà khách hàng đang sử dụng.
4. Xác nhận Mockup có `isDelete = true` trong Core Database.
5. Khách hàng chọn “Hoàn thành”.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra việc tạo yêu cầu mẫu hoa mới.

**Test Data:**
- Một Mockup ban đầu có: `isDelete = false`. Khách hàng đã chọn Mockup trước khi Admin thực hiện xóa mềm.

**Expected Result:**
- Khi khách hàng chọn “Hoàn thành”, hệ thống kiểm tra lại Mockup từ Core Database.
- Hệ thống xác định `isDelete = true`.
- Hệ thống không tạo yêu cầu với Mockup đã bị xóa mềm.
- Hệ thống thông báo Mockup không còn khả dụng.
- Hệ thống yêu cầu khách hàng chọn một Mockup khác còn khả dụng.

**Trace to:**
- [STORY-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-053/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df6d9c58-838d-4ef6-9360-b0b34ceb9cb2)
- [STORY-030](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-182](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/62e46d0d-d5b3-4af9-9ce5-565ae25fa60c)

**Rationale:**
> Xác minh hệ thống kiểm tra lại tính khả dụng của Mockup tại thời điểm hoàn thành để tránh sử dụng dữ liệu đã bị xóa mềm giữa chừng.
