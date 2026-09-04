# ST — STORY-066 — Admin xóa cấu hình phụ phí thiệp viết tay — System Tests

---

## ST-066-01-01 — Luồng chính xóa mềm cấu hình phụ phí thiệp viết tay

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6710f75-f3f6-428d-8894-0a7c2ad96f55) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí thiệp viết tay được hiển thị trong danh sách quản lý.
- Cấu hình còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách cấu hình phụ phí thiệp viết tay.
2. Admin chọn thao tác xóa tại một cấu hình.
3. Quan sát popup xác nhận xóa.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát lại danh sách cấu hình phụ phí thiệp viết tay.
7. Kiểm tra trường `isDelete` của cấu hình trong Core Database.

**Test Data:**
- Cấu hình: Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. Trạng thái = Active. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận trước khi thực hiện xóa.
- Popup hiển thị khoảng số lượng từ và giá phụ phí của cấu hình đang xóa.
- Popup cảnh báo rằng sau khi xóa, các thiệp Calligraphy thuộc khoảng số lượng từ đó sẽ không còn cấu hình phụ phí tương ứng.
- Sau khi Admin xác nhận, hệ thống cập nhật `isDelete = true` trong Core Database.
- Hệ thống thông báo xóa thành công.
- Hệ thống tải lại danh sách cấu hình phụ phí thiệp viết tay.
- Cấu hình vừa xóa không còn hiển thị trong danh sách mặc định.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)
- [BR-247](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7c0d412a-add2-48a2-876c-d385faabb75c)

**Rationale:**
> Xác minh luồng chính khi Admin xóa mềm cấu hình phụ phí viết tay và hệ thống cập nhật đúng Core Database cũng như danh sách quản lý.

---

## ST-066-02-01 — Nhận diện cấu hình và ảnh hưởng khi xóa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c65d5a85-9026-43c9-88b4-ba74f76d14f7) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin chọn thao tác xóa tại cấu hình phụ phí thiệp viết tay.
2. Quan sát nội dung popup xác nhận.
3. Đối chiếu khoảng số lượng từ và giá phụ phí trên popup với cấu hình được chọn.

**Test Data:**
- Cấu hình: Số lượng từ bắt đầu = 11. Số lượng từ kết thúc = 20. Giá phụ phí = 30000. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận xóa.
- Popup hiển thị đúng khoảng số lượng từ từ 11 đến 20.
- Popup hiển thị đúng giá phụ phí 30000.
- Popup cảnh báo rằng sau khi xóa, khoảng số lượng từ đó sẽ không còn cấu hình phụ phí tương ứng cho các thiệp Calligraphy mới.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)

**Rationale:**
> Xác minh Admin nhận diện đúng cấu hình và ảnh hưởng của thao tác trước khi xác nhận xóa.

---

## ST-066-03-01 — Hủy thao tác xóa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1567b516-1c12-44d8-a8f0-6cd457d6b764) |
| **Story** | STORY-066 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại và có `isDelete = false`. Admin đang ở popup xác nhận xóa cấu hình.

**Steps:**
1. Quan sát thông tin cấu hình và cảnh báo trên popup.
2. Admin chọn hủy hoặc đóng popup.
3. Quan sát danh sách cấu hình.
4. Kiểm tra trường `isDelete` trong Core Database.

**Test Data:**
- Một cấu hình phụ phí thiệp viết tay có `isDelete = false`.

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật `isDelete`.
- Cấu hình giữ nguyên dữ liệu trước khi thao tác.
- Cấu hình tiếp tục hiển thị trong danh sách quản lý.
- Core Database giữ nguyên `isDelete = false`.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)

**Rationale:**
> Xác minh việc Admin hủy thao tác không làm thay đổi trạng thái xóa mềm hoặc dữ liệu cấu hình.

---

## ST-066-04-01 — Cấu hình không còn tồn tại hoặc đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2723ea51-5412-43b8-96b3-8a704afb4e12) |
| **Story** | STORY-066 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đã mở danh sách cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Chuẩn bị trường hợp cấu hình không còn tồn tại hoặc đã có `isDelete = true`.
2. Admin gửi yêu cầu xóa cấu hình đó.
3. Quan sát phản hồi của hệ thống.
4. Quan sát danh sách sau phản hồi.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: Cấu hình không còn tồn tại trong Core Database.
- Trường hợp 2: Cấu hình tồn tại nhưng `isDelete = true`.

**Expected Result:**
- Hệ thống kiểm tra lại cấu hình trước khi cập nhật.
- Hệ thống không thực hiện cập nhật.
- Hệ thống không xóa lại cấu hình đã có `isDelete = true`.
- Hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- Hệ thống tải lại danh sách cấu hình phụ phí thiệp viết tay.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)

**Rationale:**
> Xác minh hệ thống xử lý an toàn khi cấu hình không còn hợp lệ tại thời điểm yêu cầu xóa được xử lý.

---

## ST-066-05-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6fa4db2f-4249-4beb-be35-087adc0759d1) |
| **Story** | STORY-066 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại và có `isDelete = false`.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác xóa cấu hình.
2. Admin xác nhận thao tác.
3. Tạo tình huống hệ thống gặp lỗi khi cập nhật `isDelete`.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra cấu hình trong danh sách.
6. Kiểm tra trường `isDelete` trong Core Database.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một cấu hình có `isDelete = false`. Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không thay đổi `isDelete` khi quá trình cập nhật thất bại.
- Cấu hình giữ nguyên dữ liệu trước khi thao tác.
- Hệ thống không hiển thị kết quả xóa thành công.
- Hệ thống thông báo xóa thất bại. Admin có thể thử lại.
- Không phát sinh trạng thái dở dang hoặc không đồng nhất.
- Core Database là nguồn xác thực cuối cùng của `isDelete`.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)

**Rationale:**
> Xác minh lỗi cập nhật không làm cấu hình rơi vào trạng thái xóa dở dang hoặc không nhất quán.

---

## ST-066-06-01 — Người dùng không có quyền quản lý cấu hình giá thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7dbddfa8-65bd-4652-bf01-475e6b0fc582) |
| **Story** | STORY-066 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại và chưa bị xóa mềm.

**Steps:**
1. Người dùng gửi yêu cầu xóa cấu hình phụ phí thiệp viết tay.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trường `isDelete` và dữ liệu cấu hình trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình giá thiệp. Một cấu hình có `isDelete = false`.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật `isDelete`.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi dữ liệu cấu hình.
- Hệ thống không cập nhật `isDelete`.
- Hệ thống hiển thị thông báo phù hợp.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-249](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1cc6e9f-ad07-4a0c-9b75-131c95f5047c)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình giá thiệp mới được phép xóa cấu hình phụ phí viết tay.

---

## ST-066-07-01 — Cấu hình đã xóa mềm không còn tham gia tính phụ phí cho thiệp mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/128ba548-026e-4295-848b-c1be26567c0f) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay đang khả dụng và có `isDelete = false`.
- Cấu hình phù hợp với số lượng từ của một thiệp Calligraphy mới.

**Steps:**
1. Xác nhận cấu hình đang được sử dụng cho khoảng số lượng từ tương ứng.
2. Admin xóa mềm cấu hình.
3. Xác nhận cấu hình có `isDelete = true` trong Core Database.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ thuộc khoảng đã xóa.
5. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Giá phụ phí = 20000. `isDelete = false` trước khi xóa.
- Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Sau khi xóa thành công, cấu hình có `isDelete = true`.
- Hệ thống không sử dụng cấu hình đã xóa mềm để xác định phụ phí cho thiệp Calligraphy mới.
- Chỉ cấu hình có `isDelete = false` và thỏa điều kiện khả dụng mới được sử dụng.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-247](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7c0d412a-add2-48a2-876c-d385faabb75c)

**Rationale:**
> Xác minh cấu hình đã xóa mềm không còn tham gia tính phụ phí cho các thiệp Calligraphy mới.

---

## ST-066-08-01 — Tác động của xóa mềm lên danh sách quản lý

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a8d5ce94-4a6f-4501-b014-ac072bdda3c5) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Core Database có cấu hình phụ phí viết tay chưa xóa mềm.

**Steps:**
1. Admin xóa mềm một cấu hình phụ phí thiệp viết tay.
2. Xác nhận cấu hình có `isDelete = true`.
3. Admin tải lại danh sách cấu hình phụ phí thiệp viết tay mặc định.
4. Quan sát danh sách.

**Test Data:**
- Cấu hình A: `isDelete = false`. Sau khi xóa: `isDelete = true`.

**Expected Result:**
- Cấu hình vừa xóa không còn hiển thị trong danh sách quản lý mặc định.
- Dữ liệu cấu hình vẫn tồn tại trong Core Database với `isDelete = true`.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)

**Rationale:**
> Xác minh tác động của xóa mềm lên danh sách quản lý mặc định mà không xóa vật lý bản ghi.

---

## ST-066-09-01 — Cơ chế xóa mềm thay vì xóa vật lý

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4194f146-997c-414b-9edb-a90bbae5bb71) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại trong Core Database và có `isDelete = false`.

**Steps:**
1. Ghi nhận bản ghi cấu hình trong Core Database.
2. Admin thực hiện xóa cấu hình.
3. Admin xác nhận thao tác.
4. Kiểm tra lại bản ghi cấu hình trong Core Database.

**Test Data:**
- Một cấu hình có: `isDelete = false`. Khoảng số lượng từ và giá phụ phí xác định.

**Expected Result:**
- Hệ thống cập nhật `isDelete = true`.
- Bản ghi cấu hình vẫn tồn tại trong Core Database.
- Hệ thống không xóa vật lý cấu hình khỏi Core Database.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [BR-246](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf)

**Rationale:**
> Xác minh thao tác xóa cấu hình sử dụng đúng cơ chế xóa mềm thay vì xóa vật lý.

---

## ST-066-10-01 — Không làm thay đổi dữ liệu nghiệp vụ đã phát sinh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/860cbf12-4e4b-4b5b-8080-bd5b210f2f6f) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay đã từng được sử dụng để xác định phụ phí.
- Có thiệp, Checkout hoặc Order đã tồn tại với giá được xác định từ cấu hình đó.

**Steps:**
1. Ghi nhận giá và dữ liệu của thiệp, Checkout hoặc Order đã sử dụng cấu hình.
2. Admin xóa mềm cấu hình phụ phí thiệp viết tay.
3. Xác nhận cấu hình có `isDelete = true`.
4. Kiểm tra lại giá và dữ liệu của thiệp, Checkout hoặc Order đã tồn tại.

**Test Data:**
- Một cấu hình đã từng được áp dụng cho ít nhất một trong các dữ liệu: Thiệp, Checkout, Order.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái xóa mềm của cấu hình.
- Hệ thống không tính lại giá của thiệp đã tồn tại.
- Hệ thống không thay đổi giá hoặc dữ liệu của Checkout đã tồn tại.
- Hệ thống không thay đổi giá hoặc dữ liệu của Order đã tồn tại.
- Dữ liệu nghiệp vụ phát sinh trước thời điểm xóa được giữ nguyên.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-066/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-248](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c7fcf4-0616-45f7-b2df-dc75c7aaf952)

**Rationale:**
> Xác minh xóa mềm cấu hình không làm thay đổi dữ liệu nghiệp vụ hoặc giá đã phát sinh trước đó.
