# ST — STORY-058 — Admin thêm mẫu thiệp — System Tests

---

## ST-058-01-01 — Thêm mẫu thiệp thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f4bfc3cc-840a-4bdc-865d-6f0f6fa43056) |
| **Story** | STORY-058 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Admin chọn thêm mẫu thiệp.
3. Hệ thống hiển thị form thêm mẫu thiệp.
4. Admin nhập tên mẫu và upload ảnh template mẫu thiệp.
5. Admin nhập mô tả nếu có và chọn lưu.
6. Hệ thống kiểm tra dữ liệu đầu vào, độ dài tên mẫu/mô tả và file upload.
7. Quan sát kết quả sau khi lưu.
8. Kiểm tra thông tin mẫu thiệp trong Core Database.
9. Kiểm tra ảnh template/ảnh Preview trong storage.
10. Quan sát danh sách mẫu thiệp sau khi tạo.

**Test Data:**
- Tên mẫu: `Thiệp sinh nhật hoa hồng`.
- Ảnh template/ảnh Preview: file PNG hoặc JPG hợp lệ, dung lượng không vượt quá 10MB.
- Mô tả: `Mẫu thiệp sinh nhật hoa hồng`.

**Expected Result:**
- Hệ thống lưu thông tin mẫu thiệp vào Core Database.
- Hệ thống lưu ảnh template/ảnh Preview vào storage.
- Mẫu thiệp mới có trạng thái mặc định là Active. Mẫu thiệp mới có `isDelete = false`.
- Hệ thống thông báo thêm mẫu thiệp thành công.
- Danh sách mẫu thiệp được cập nhật và hiển thị mẫu thiệp vừa tạo.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-201](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946)
- [BR-202](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20)
- [BR-203](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038)
- [BR-204](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd)

**Rationale:**
> Xác minh luồng chính khi Admin thêm mẫu thiệp mới với dữ liệu hợp lệ và hệ thống lưu đầy đủ dữ liệu vào Core Database và storage.

---

## ST-058-02-01 — Hủy thao tác thêm mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f2629b1e-a08a-4c7b-975f-e52c219a0a0f) |
| **Story** | STORY-058 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form thêm mẫu thiệp.

**Steps:**
1. Admin nhập một phần hoặc toàn bộ thông tin mẫu thiệp.
2. Admin chọn “Hủy”.
3. Quan sát màn hình sau khi hủy.
4. Kiểm tra Core Database.
5. Kiểm tra storage.

**Test Data:**
- Tên mẫu: `Mẫu thiệp chưa lưu`.
- Ảnh template/ảnh Preview: file PNG hợp lệ.
- Mô tả: `Dữ liệu đang nhập dở`.

**Expected Result:**
- Hệ thống không tạo mẫu thiệp mới.
- Hệ thống quay lại danh sách mẫu thiệp.
- Core Database không có bản ghi mẫu thiệp mới từ thao tác đã hủy.
- Storage không lưu ảnh mới từ thao tác đã hủy.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)

**Rationale:**
> Xác minh việc Admin hủy thao tác giữa chừng không làm phát sinh dữ liệu mẫu thiệp mới.

---

## ST-058-03-01 — Bỏ trống các trường bắt buộc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae79f86d-af3f-4bdb-acf8-216dd5ce119f) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form thêm mẫu thiệp.

**Steps:**
1. Thực hiện lưu mẫu thiệp khi không nhập tên mẫu.
2. Quan sát phản hồi của hệ thống.
3. Nhập tên mẫu chỉ chứa khoảng trắng và thực hiện lưu.
4. Quan sát phản hồi của hệ thống.
5. Nhập tên mẫu hợp lệ nhưng không upload ảnh template/ảnh Preview.
6. Thực hiện lưu.
7. Quan sát phản hồi của hệ thống.
8. Kiểm tra Core Database.

**Test Data:**
- Trường hợp 1: Tên mẫu để trống, ảnh hợp lệ.
- Trường hợp 2: Tên mẫu chỉ chứa khoảng trắng, ảnh hợp lệ.
- Trường hợp 3: Tên mẫu hợp lệ, không có ảnh template/ảnh Preview.

**Expected Result:**
- Hệ thống không cho lưu mẫu thiệp trong tất cả các trường hợp.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Không có mẫu thiệp mới được tạo trong Core Database.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-201](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946)

**Rationale:**
> Xác minh các trường bắt buộc của mẫu thiệp được kiểm tra trước khi hệ thống tạo dữ liệu mới.

---

## ST-058-04-01 — Vượt quá giới hạn độ dài thông tin

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eeacee84-4760-462e-bd79-0d9996e53124) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form thêm mẫu thiệp. Ảnh template/ảnh Preview hợp lệ đã được chọn.

**Steps:**
1. Nhập tên mẫu vượt quá 100 ký tự.
2. Nhập mô tả hợp lệ hoặc để trống.
3. Chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Nhập tên mẫu hợp lệ.
6. Nhập mô tả vượt quá 200 ký tự.
7. Chọn lưu.
8. Quan sát phản hồi của hệ thống.
9. Kiểm tra Core Database.

**Test Data:**
- Trường hợp 1: Tên mẫu: `101 ký tự`. Mô tả: hợp lệ. Ảnh: hợp lệ.
- Trường hợp 2: Tên mẫu: không vượt quá 100 ký tự. Mô tả: `201 ký tự`. Ảnh: hợp lệ.

**Expected Result:**
- Hệ thống không cho lưu mẫu thiệp khi tên mẫu vượt quá 100 ký tự. Hệ thống hiển thị lỗi tại trường tên mẫu.
- Hệ thống không cho lưu mẫu thiệp khi mô tả vượt quá 200 ký tự. Hệ thống hiển thị lỗi tại trường mô tả.
- Không có mẫu thiệp mới được tạo trong Core Database.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-201](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946)

**Rationale:**
> Xác minh giới hạn độ dài của tên mẫu và mô tả được kiểm tra trước khi lưu mẫu thiệp.

---

## ST-058-05-01 — File không thuộc định dạng PNG hoặc JPG

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/407827bf-7d0d-42f8-80d5-2ab66855bcd6) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form thêm mẫu thiệp.

**Steps:**
1. Admin nhập tên mẫu hợp lệ.
2. Admin upload file không thuộc định dạng PNG hoặc JPG.
3. Quan sát phản hồi của hệ thống.
4. Thực hiện lưu nếu hệ thống cho phép.
5. Kiểm tra Core Database.
6. Kiểm tra storage.

**Test Data:**
- Tên mẫu: `Mẫu thiệp kiểm tra file`.
- File upload: PDF, GIF, WEBP hoặc định dạng khác PNG/JPG.

**Expected Result:**
- Hệ thống từ chối file không thuộc định dạng PNG hoặc JPG.
- Hệ thống hiển thị lý do file không hợp lệ.
- Hệ thống cho phép Admin chọn file khác.
- Hệ thống không lưu mẫu thiệp. Core Database không có bản ghi mẫu thiệp mới. Storage không lưu file không hợp lệ.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-202](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20)

**Rationale:**
> Xác minh hệ thống chỉ chấp nhận định dạng ảnh PNG hoặc JPG khi thêm mẫu thiệp.

---

## ST-058-06-01 — File PNG hoặc JPG vượt quá 10MB

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/69f90a01-d213-4e27-9917-1c5615f16ff6) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang ở form thêm mẫu thiệp.

**Steps:**
1. Admin nhập tên mẫu hợp lệ.
2. Admin upload file PNG hoặc JPG vượt quá 10MB.
3. Quan sát phản hồi của hệ thống.
4. Thực hiện lưu nếu hệ thống cho phép.
5. Kiểm tra Core Database.
6. Kiểm tra storage.

**Test Data:**
- Tên mẫu: `Mẫu thiệp kiểm tra dung lượng`.
- File upload: PNG hoặc JPG có dung lượng lớn hơn 10MB.

**Expected Result:**
- Hệ thống từ chối file vượt quá 10MB.
- Hệ thống hiển thị lý do file không hợp lệ.
- Hệ thống cho phép Admin chọn file khác.
- Hệ thống không lưu mẫu thiệp. Core Database không có bản ghi mẫu thiệp mới. Storage không lưu file vượt quá dung lượng cho phép.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-202](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20)

**Rationale:**
> Xác minh giới hạn dung lượng ảnh tối đa 10MB được kiểm tra trước khi lưu mẫu thiệp.

---

## ST-058-07-01 — Cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3b6b5d07-e425-4d69-a04c-5c9aa73fb637) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đã nhập dữ liệu mẫu thiệp hợp lệ.
- Có thể tạo tình huống lưu Core Database thất bại.

**Steps:**
1. Admin nhập tên mẫu hợp lệ.
2. Admin upload ảnh template/ảnh Preview hợp lệ.
3. Admin nhập mô tả nếu có.
4. Tạo tình huống lưu thông tin mẫu thiệp vào Core Database thất bại.
5. Admin chọn lưu.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra Core Database.
8. Kiểm tra storage.
9. Khôi phục hệ thống về trạng thái bình thường và thử lại.

**Test Data:**
- Tên mẫu hợp lệ. Ảnh PNG hoặc JPG hợp lệ, dung lượng không vượt quá 10MB.
- Môi trường test có khả năng tạo lỗi khi lưu Core Database.

**Expected Result:**
- Hệ thống không thông báo thêm mẫu thiệp thành công.
- Hệ thống hiển thị thông báo lỗi để Admin thử lại.
- Hệ thống không tạo mẫu thiệp thành công.
- Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tạo thành công. Không tồn tại dữ liệu mẫu thiệp dở dang.
- Admin có thể thực hiện lại thao tác sau khi lỗi được xử lý.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-203](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038)
- [BR-204](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd)

**Rationale:**
> Xác minh tính nguyên tử khi việc lưu thông tin mẫu thiệp vào Core Database thất bại.

---

## ST-058-08-01 — Lưu ảnh template/ảnh Preview vào storage thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c06103ae-6d7a-40be-9782-4f1161f79836) |
| **Story** | STORY-058 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đã nhập dữ liệu mẫu thiệp hợp lệ.
- Có thể tạo tình huống lưu ảnh template/ảnh Preview vào storage thất bại.

**Steps:**
1. Admin nhập tên mẫu hợp lệ.
2. Admin upload ảnh template/ảnh Preview hợp lệ.
3. Admin nhập mô tả nếu có.
4. Tạo tình huống lưu ảnh vào storage thất bại.
5. Admin chọn lưu.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra Core Database.
8. Kiểm tra storage.
9. Khôi phục hệ thống về trạng thái bình thường và thử lại.

**Test Data:**
- Tên mẫu hợp lệ. Ảnh PNG hoặc JPG hợp lệ, dung lượng không vượt quá 10MB.
- Môi trường test có khả năng tạo lỗi khi lưu storage.

**Expected Result:**
- Hệ thống không thông báo thêm mẫu thiệp thành công.
- Hệ thống hiển thị thông báo lỗi để Admin thử lại.
- Hệ thống không tạo mẫu thiệp thành công. Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tạo thành công.
- Không tồn tại mẫu thiệp dở dang chỉ có thông tin trong Core Database nhưng không có ảnh hợp lệ.
- Admin có thể thực hiện lại thao tác sau khi lỗi được xử lý.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-203](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038)
- [BR-204](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd)

**Rationale:**
> Xác minh việc thêm mẫu thiệp chỉ được xem là thành công khi cả dữ liệu Core Database và ảnh trên storage đều được lưu thành công.

---

## ST-058-09-01 — Chặn Admin không có quyền quản lý mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c77a1441-1989-4c74-947c-b3b002387cee) |
| **Story** | STORY-058 |
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
1. Admin truy cập chức năng thêm mẫu thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra form thêm mẫu thiệp có được hiển thị hay không.
4. Kiểm tra Core Database.
5. Kiểm tra storage.

**Test Data:**
- Tài khoản Admin không có quyền quản lý mẫu thiệp.

**Expected Result:**
- Hệ thống từ chối truy cập chức năng thêm mẫu thiệp.
- Hệ thống không hiển thị form thêm mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.
- Hệ thống không tạo dữ liệu mới trong Core Database. Hệ thống không lưu file mới vào storage.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)

**Rationale:**
> Xác minh Backend kiểm tra quyền Admin trước khi cho phép truy cập và tạo mẫu thiệp mới.

---

## ST-058-10-01 — Cho phép tạo mẫu thiệp có tên trùng với mẫu thiệp đã tồn tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-058-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/875a6556-dd80-44ab-8018-eae2cdb22965) |
| **Story** | STORY-058 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Core Database đã có một mẫu thiệp với tên xác định.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Xác định tên của một mẫu thiệp đã tồn tại trong hệ thống.
2. Admin chọn thêm mẫu thiệp.
3. Admin nhập tên mẫu trùng với mẫu thiệp đã tồn tại.
4. Admin upload ảnh template/ảnh Preview hợp lệ.
5. Admin nhập mô tả nếu có.
6. Admin chọn lưu.
7. Quan sát kết quả.
8. Kiểm tra Core Database.
9. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Mẫu thiệp đã tồn tại: Tên mẫu: `Thiệp sinh nhật hoa hồng`.
- Mẫu thiệp mới: Tên mẫu: `Thiệp sinh nhật hoa hồng`. Ảnh PNG hoặc JPG hợp lệ, không vượt quá 10MB. Các dữ liệu còn lại hợp lệ.

**Expected Result:**
- Hệ thống cho phép tạo mẫu thiệp có tên trùng với mẫu thiệp đã tồn tại.
- Mẫu thiệp mới được lưu vào Core Database.
- Ảnh template/ảnh Preview được lưu vào storage.
- Mẫu thiệp mới có trạng thái Active và `isDelete = false`.
- Hệ thống thông báo thêm mẫu thiệp thành công.
- Danh sách có thể tồn tại nhiều mẫu thiệp cùng tên.

**Trace to:**
- [STORY-058](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [STORY-058/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/22b556cd-96c9-4d9a-ac2e-18bbb5649a9f)
- [BR-251](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a46e2499-1305-449d-8091-97312508ab2f)

**Rationale:**
> Xác minh tên mẫu thiệp không yêu cầu duy nhất và hệ thống cho phép tạo mẫu trùng tên khi các dữ liệu còn lại hợp lệ.
