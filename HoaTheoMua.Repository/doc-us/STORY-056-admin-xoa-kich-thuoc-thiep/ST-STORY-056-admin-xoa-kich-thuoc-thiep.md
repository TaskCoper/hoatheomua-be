# ST — STORY-056 — Admin xóa kích thước thiệp — System Tests

---

## ST-056-01-01 — Xóa kích thước thiệp thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9221969c-ede5-43bd-91df-1b3e0ff259de) |
| **Story** | STORY-056 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Quan sát danh sách kích thước thiệp chưa bị xóa mềm.
3. Admin chọn thao tác xóa tại một kích thước thiệp.
4. Quan sát popup xác nhận.
5. Admin chọn “Yes”.
6. Quan sát thông báo sau khi xóa.
7. Quan sát lại danh sách kích thước thiệp.
8. Kiểm tra dữ liệu kích thước thiệp trong Core Database.

**Test Data:**
- Một kích thước thiệp tồn tại trong Core Database và có `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận “Bạn chắc chắn muốn xóa kích thước thiệp này không?”. Popup có nút “Yes”.
- Sau khi Admin chọn “Yes”, hệ thống cập nhật `isDelete = true` trong Core Database.
- Hệ thống thông báo xóa kích thước thiệp thành công.
- Kích thước vừa xóa không còn hiển thị trong danh sách mặc định.
- Dữ liệu kích thước thiệp vẫn tồn tại trong Core Database và không bị xóa cứng.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)

**Rationale:**
> Xác minh luồng chính của chức năng xóa kích thước thiệp sử dụng cơ chế xóa mềm và cập nhật đúng danh sách sau khi xóa.

---

## ST-056-02-01 — Hủy thao tác xóa tại popup

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e5f89c25-05ee-4d01-9c14-bd7c855501f9) |
| **Story** | STORY-056 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thao tác xóa tại một kích thước thiệp.
3. Quan sát popup xác nhận.
4. Admin hủy hoặc đóng popup.
5. Quan sát danh sách kích thước thiệp.
6. Kiểm tra dữ liệu kích thước thiệp trong Core Database.

**Test Data:**
- Một kích thước thiệp tồn tại trong Core Database và có `isDelete = false`.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật `isDelete`.
- Kích thước thiệp vẫn hiển thị như trước đó trong danh sách.
- Dữ liệu kích thước thiệp trong Core Database được giữ nguyên.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)

**Rationale:**
> Xác minh việc đóng hoặc hủy popup không làm thay đổi dữ liệu kích thước thiệp.

---

## ST-056-03-01 — Chặn người dùng không có quyền quản lý cấu hình thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a283472-15de-405f-9929-53b3c007cb8a) |
| **Story** | STORY-056 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin không có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin thực hiện yêu cầu xóa một kích thước thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra dữ liệu kích thước thiệp trong Core Database.

**Test Data:**
- Tài khoản Admin không có quyền quản lý cấu hình thiệp.
- Một kích thước thiệp tồn tại và có `isDelete = false`.

**Expected Result:**
- Hệ thống không cho phép Admin xóa kích thước thiệp.
- Hệ thống hiển thị thông báo phù hợp.
- Hệ thống không cập nhật `isDelete`.
- Dữ liệu kích thước thiệp trong Core Database được giữ nguyên.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)

**Rationale:**
> Xác minh Backend kiểm tra quyền Admin trước khi cho phép cập nhật isDelete.

---

## ST-056-04-01 — Không cho xóa kích thước thiệp không tồn tại hoặc đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8d17ebb9-a5f8-44d8-9b68-8689205417fa) |
| **Story** | STORY-056 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.
- Chuẩn bị trường hợp kích thước thiệp không tồn tại hoặc đã có `isDelete = true`.

**Steps:**
1. Admin thực hiện yêu cầu xóa kích thước thiệp đó.
2. Quan sát phản hồi của hệ thống.
3. Quan sát danh sách kích thước thiệp sau phản hồi.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Kích thước thiệp không tồn tại trong Core Database.
- Trường hợp 2: Kích thước thiệp tồn tại nhưng đã có `isDelete = true`.

**Expected Result:**
- Hệ thống không cập nhật dữ liệu.
- Hệ thống hiển thị thông báo phù hợp.
- Hệ thống tải lại danh sách hiện tại.
- Không phát sinh thêm thay đổi đối với dữ liệu kích thước thiệp trong Core Database.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)

**Rationale:**
> Xác minh hệ thống xử lý an toàn khi dữ liệu mục tiêu không còn hợp lệ tại thời điểm thực hiện thao tác xóa.

---

## ST-056-05-01 — Xử lý khi cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/474133a6-5267-41ae-bd73-6f926706232b) |
| **Story** | STORY-056 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp chưa bị xóa mềm.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thao tác xóa tại một kích thước thiệp.
3. Admin chọn “Yes” tại popup xác nhận.
4. Tạo tình huống hệ thống không thể cập nhật `isDelete = true`.
5. Quan sát phản hồi của hệ thống.
6. Kiểm tra dữ liệu kích thước thiệp trong Core Database.
7. Thử lại thao tác xóa sau khi hệ thống hoạt động bình thường.

**Test Data:**
- Một kích thước thiệp tồn tại và có `isDelete = false`.
- Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không ghi nhận xóa thành công khi Core Database chưa cập nhật thành công.
- Hệ thống hiển thị thông báo lỗi.
- Hệ thống cho phép Admin thử lại.
- Kích thước thiệp không bị ghi nhận ở trạng thái xóa dở dang.
- Sau khi lỗi được khắc phục, Admin có thể thử lại thao tác xóa.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)

**Rationale:**
> Xác minh hệ thống không hiển thị hoặc lưu trạng thái xóa thành công khi việc cập nhật Core Database thực tế thất bại.

---

## ST-056-06-01 — Không xóa cứng dữ liệu kích thước thiệp trong Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f9e696fe-18ce-43e4-aa1b-bfcd2aa95c45) |
| **Story** | STORY-056 |
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
1. Xác định một kích thước thiệp đã được sử dụng trong dữ liệu lịch sử.
2. Ghi nhận dữ liệu thiệp, Checkout hoặc Order đang liên kết với kích thước đó.
3. Admin thực hiện xóa kích thước thiệp.
4. Admin chọn “Yes” tại popup xác nhận.
5. Kiểm tra kích thước thiệp trong Core Database.
6. Kiểm tra lại các thiệp, Checkout hoặc Order đã sử dụng kích thước đó.

**Test Data:**
- Một kích thước thiệp có `isDelete = false` và đã được sử dụng bởi ít nhất một dữ liệu lịch sử gồm thiệp, Checkout hoặc Order.

**Expected Result:**
- Hệ thống cập nhật `isDelete = true` cho kích thước thiệp.
- Hệ thống không xóa cứng dữ liệu kích thước khỏi Core Database.
- Các thiệp, Checkout hoặc Order đã tồn tại vẫn giữ nguyên dữ liệu và liên kết với kích thước thiệp.
- Việc xóa mềm không làm mất dữ liệu lịch sử liên quan.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)
- [BR-196](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/673fbaef-59fb-4a3a-985e-0dda27ffb300)

**Rationale:**
> Xác minh xóa mềm kích thước thiệp không gây mất hoặc phá vỡ dữ liệu lịch sử đã sử dụng kích thước đó.

---

## ST-056-07-01 — Kích thước thiệp đã xóa mềm không hiển thị cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f3500f1-1b2b-4c14-a676-60fee6372bef) |
| **Story** | STORY-056 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Admin thực hiện xóa mềm một kích thước thiệp.
2. Admin chọn “Yes” tại popup xác nhận.
3. Xác nhận kích thước đã có `isDelete = true`.
4. Khách hàng truy cập bước chọn kích thước thiệp trong quy trình tạo thiệp mới.
5. Quan sát danh sách kích thước thiệp mà khách hàng có thể chọn.

**Test Data:**
- Một kích thước thiệp có `isDelete = false` trước khi Admin xóa.

**Expected Result:**
- Sau khi Admin xóa, kích thước thiệp có `isDelete = true`.
- Kích thước đã xóa mềm không hiển thị trong danh sách kích thước mà khách hàng có thể chọn khi tạo thiệp mới.
- Các kích thước hợp lệ khác vẫn được hiển thị bình thường.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-195](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7d9328de-3fbc-46ab-bd3b-bef2b2c09ab2)

**Rationale:**
> Xác minh kích thước đã xóa mềm được loại khỏi dữ liệu sử dụng cho quy trình tạo thiệp mới của khách hàng.

---

## ST-056-08-01 — Khách hàng đang tạo thiệp bằng kích thước vừa bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-056-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76a4c94f-43fd-4d1b-a057-0c4db8ae5540) |
| **Story** | STORY-056 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang thực hiện quy trình tạo thiệp.
- Khách hàng đã chọn một kích thước thiệp nhưng chưa hoàn tất tạo thiệp.
- Kích thước thiệp đang có `isDelete = false`.
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.

**Steps:**
1. Khách hàng chọn một kích thước thiệp trên form tạo thiệp nhưng chưa hoàn tất yêu cầu.
2. Admin xóa mềm đúng kích thước thiệp mà khách hàng đang chọn.
3. Xác nhận kích thước đã được cập nhật `isDelete = true`.
4. Khách hàng tiếp tục gửi yêu cầu tạo thiệp với kích thước đã chọn trước đó.
5. Quan sát phản hồi của hệ thống.
6. Kiểm tra việc xử lý yêu cầu tạo thiệp.

**Test Data:**
- Một kích thước thiệp ban đầu có `isDelete = false`.
- Khách hàng đã chọn kích thước này trước thời điểm Admin thực hiện xóa mềm.

**Expected Result:**
- Hệ thống cập nhật `isDelete = true` khi Admin xóa kích thước thiệp.
- Khi khách hàng tiếp tục gửi yêu cầu, hệ thống kiểm tra lại tình trạng kích thước.
- Hệ thống không cho phép sử dụng kích thước đã bị xóa cho yêu cầu tạo thiệp mới.
- Hệ thống từ chối yêu cầu và yêu cầu khách hàng chọn kích thước khác.

**Trace to:**
- [STORY-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [STORY-056/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)
- [BR-195](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7d9328de-3fbc-46ab-bd3b-bef2b2c09ab2)

**Rationale:**
> Xác minh hệ thống kiểm tra lại trạng thái kích thước tại thời điểm xử lý yêu cầu để ngăn khách hàng sử dụng kích thước vừa bị Admin xóa mềm.
