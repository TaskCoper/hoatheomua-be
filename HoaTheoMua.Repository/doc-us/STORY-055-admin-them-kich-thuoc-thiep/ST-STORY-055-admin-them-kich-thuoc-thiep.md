# ST — STORY-055 — Admin thêm kích thước thiệp — System Tests

---

## ST-055-01-08 — Thêm kích thước thiệp thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-01-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4ba53343-caad-4d42-8e7c-9c214f67fcd1) |
| **Story** | STORY-055 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.
- Chưa tồn tại kích thước có cùng chiều rộng và cùng chiều cao với `isDelete = false`.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thêm kích thước thiệp.
3. Hệ thống hiển thị form thêm kích thước thiệp.
4. Admin nhập kích thước, giá size và số lượng từ tối đa.
5. Admin chọn lưu.
6. Quan sát kết quả sau khi lưu.
7. Kiểm tra dữ liệu kích thước thiệp trong Core Database.
8. Kiểm tra danh sách kích thước thiệp.

**Test Data:**
- Chiều rộng: `5.5 cm`. Chiều cao: `7.5 cm`.
- Giá size: `0 VNĐ`. Số lượng từ tối đa: `1`.

**Expected Result:**
- Hệ thống chấp nhận dữ liệu hợp lệ.
- Hệ thống lưu kích thước thiệp mới vào Core Database.
- Kích thước thiệp mới có trạng thái mặc định là Active. `isDelete = false`.
- Hệ thống thông báo thêm kích thước thiệp thành công.
- Danh sách kích thước thiệp được cập nhật và hiển thị kích thước vừa tạo.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-191](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7ea3513-2bd6-4872-ad0c-da72c7929bac)
- [BR-192](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6adb9749-5127-4c2b-a39e-e2ade6d4f533)
- [BR-250](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e15e3e5b-b45c-4d9a-9f1b-58fa81aee57b)

**Rationale:**
> Bao phủ Main Flow, validation dữ liệu hợp lệ và trạng thái mặc định sau khi tạo.

---

## ST-055-02-04 — Kích thước thiệp có chiều ngược lại không bị coi là trùng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-02-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cd587e3c-f371-4266-97fd-b9a8cf4b3de7) |
| **Story** | STORY-055 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.
- Đã tồn tại kích thước `5 x 7 cm` có `isDelete = false`.
- Chưa tồn tại kích thước `7 x 5 cm` có `isDelete = false`.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thêm kích thước thiệp.
3. Admin nhập chiều rộng 7 cm và chiều cao 5 cm.
4. Admin nhập giá size và số lượng từ tối đa hợp lệ.
5. Admin chọn lưu.
6. Quan sát kết quả.

**Test Data:**
- Kích thước đã tồn tại: `5 x 7 cm`
- Kích thước thêm mới: `7 x 5 cm`
- Giá size: giá trị hợp lệ
- Số lượng từ tối đa: số nguyên lớn hơn 0

**Expected Result:**
- Hệ thống không xem kích thước `7 x 5 cm` là trùng với kích thước `5 x 7 cm`.
- Hệ thống tạo kích thước thiệp mới thành công.
- Kích thước thiệp mới có trạng thái Active và `isDelete = false`.
- Danh sách kích thước thiệp được cập nhật.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-193](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/222f8a1f-c5b1-4bf8-a54e-fadfc0efeb2f)

**Rationale:**
> Bao phủ quy tắc hai kích thước chỉ trùng khi cùng chiều rộng và cùng chiều cao.

---

## ST-055-03-05 — Hủy thao tác thêm mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-03-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b42360cb-d267-4225-9944-12822b81d784) |
| **Story** | STORY-055 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đang ở form thêm kích thước thiệp.

**Steps:**
1. Admin nhập thông tin kích thước thiệp.
2. Admin chọn “Hủy”.
3. Quan sát kết quả.
4. Kiểm tra danh sách kích thước thiệp.

**Test Data:**
- Kích thước: dữ liệu hợp lệ
- Giá size: dữ liệu hợp lệ
- Số lượng từ tối đa: dữ liệu hợp lệ

**Expected Result:**
- Hệ thống không tạo kích thước thiệp mới.
- Hệ thống quay lại danh sách kích thước thiệp.
- Không có kích thước mới được lưu vào Core Database.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)

**Rationale:**
> Bao phủ Alternative Flow khi Admin hủy thao tác thêm mới.

---

## ST-055-04-03 — Thiếu thông tin bắt buộc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-04-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7585feee-e09e-48e2-b08a-8907a65db96f) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đang ở form thêm kích thước thiệp.

**Steps:**
1. Admin để thiếu một trong các thông tin bắt buộc: kích thước, giá size hoặc số lượng từ tối đa.
2. Admin chọn lưu.
3. Quan sát lỗi hiển thị tại trường tương ứng.
4. Kiểm tra Core Database.
5. Lặp lại với từng trường bắt buộc còn lại.

**Test Data:**
- Trường hợp 1: thiếu kích thước.
- Trường hợp 2: thiếu giá size.
- Trường hợp 3: thiếu số lượng từ tối đa.

**Expected Result:**
- Hệ thống không cho lưu khi thiếu thông tin bắt buộc.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Hệ thống không tạo kích thước thiệp mới trong Core Database.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-191](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7ea3513-2bd6-4872-ad0c-da72c7929bac)

**Rationale:**
> Bao phủ EXC-01 đối với trường hợp thiếu thông tin bắt buộc.

---

## ST-055-05-03 — Dữ liệu không hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-05-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/989b5e55-6be6-4643-9299-9c754aa3b6f8) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đang ở form thêm kích thước thiệp.

**Steps:**
1. Admin lần lượt nhập dữ liệu không hợp lệ cho chiều rộng, chiều cao, giá size và số lượng từ tối đa.
2. Admin chọn lưu sau mỗi trường hợp.
3. Quan sát lỗi tại trường tương ứng.
4. Kiểm tra Core Database sau mỗi lần lưu.

**Test Data:**
- Trường hợp 1: chiều rộng = `0`.
- Trường hợp 2: chiều cao = `0`.
- Trường hợp 3: giá size `< 0`.
- Trường hợp 4: số lượng từ tối đa = `0`.
- Trường hợp 5: số lượng từ tối đa là số thập phân.

**Expected Result:**
- Hệ thống không lưu kích thước thiệp mới đối với từng trường hợp dữ liệu không hợp lệ.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Không tạo dữ liệu kích thước thiệp mới trong Core Database.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-250](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e15e3e5b-b45c-4d9a-9f1b-58fa81aee57b)

**Rationale:**
> Bao phủ validation giá trị kích thước, giá size và số lượng từ tối đa.

---

## ST-055-06-03 — Kích thước thiệp trùng với bản ghi Active

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-06-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7f5f6242-6e35-417e-8b7d-94b416c4f5a1) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Đã tồn tại kích thước thiệp có cùng chiều rộng và cùng chiều cao, `isDelete = false` và trạng thái Active.

**Steps:**
1. Admin truy cập chức năng thêm kích thước thiệp.
2. Admin nhập cùng chiều rộng và cùng chiều cao với kích thước Active đã tồn tại.
3. Admin nhập giá size và số lượng từ tối đa hợp lệ.
4. Admin chọn lưu.
5. Quan sát kết quả.
6. Kiểm tra Core Database.

**Test Data:**
- Kích thước đã tồn tại: `5 x 7 cm` (Trạng thái: Active, `isDelete = false`).
- Kích thước thêm mới: `5 x 7 cm`.

**Expected Result:**
- Hệ thống không tạo kích thước thiệp mới.
- Hệ thống hiển thị thông báo kích thước đã tồn tại.
- Core Database không có thêm bản ghi kích thước 5 x 7 cm từ thao tác này.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-193](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/222f8a1f-c5b1-4bf8-a54e-fadfc0efeb2f)

**Rationale:**
> Bao phủ EXC-02 khi kích thước trùng với bản ghi Active có `isDelete = false`.

---

## ST-055-07-03 — Kích thước thiệp trùng với bản ghi Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-07-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f3802e5-0edc-4b5b-8b7e-f76c08cb3ea5) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Đã tồn tại kích thước thiệp có cùng chiều rộng và cùng chiều cao, `isDelete = false` và trạng thái Inactive.

**Steps:**
1. Admin truy cập chức năng thêm kích thước thiệp.
2. Admin nhập cùng chiều rộng và cùng chiều cao với kích thước Inactive đã tồn tại.
3. Admin nhập giá size và số lượng từ tối đa hợp lệ.
4. Admin chọn lưu.
5. Quan sát kết quả.
6. Kiểm tra Core Database.

**Test Data:**
- Kích thước đã tồn tại: `5 x 7 cm` (Trạng thái: Inactive, `isDelete = false`).
- Kích thước thêm mới: `5 x 7 cm`.

**Expected Result:**
- Hệ thống không tạo kích thước thiệp mới.
- Hệ thống hiển thị thông báo kích thước đã tồn tại.
- Trạng thái Inactive của kích thước đã tồn tại không làm thay đổi kết quả kiểm tra trùng.
- Core Database không có thêm bản ghi kích thước 5 x 7 cm từ thao tác này.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-193](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/222f8a1f-c5b1-4bf8-a54e-fadfc0efeb2f)

**Rationale:**
> Bao phủ EXC-02 và quy tắc kiểm tra trùng không phụ thuộc trạng thái Active hoặc Inactive.

---

## ST-055-08-03 — Cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-08-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7aafeb0a-99a4-470d-ae5c-4a6e770312a1) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Admin đã nhập dữ liệu hợp lệ. Hệ thống không thể lưu dữ liệu vào Core Database.

**Steps:**
1. Admin chọn lưu.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra Core Database.
4. Khôi phục tình trạng lưu dữ liệu về bình thường.
5. Thử lại thao tác lưu.

**Test Data:**
- Kích thước: dữ liệu hợp lệ. Giá size: dữ liệu hợp lệ. Số lượng từ tối đa: dữ liệu hợp lệ.

**Expected Result:**
- Ở lần lưu thất bại, hệ thống không tạo kích thước thiệp mới.
- Hệ thống không tạo dữ liệu dở dang.
- Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.
- Sau khi việc lưu dữ liệu hoạt động bình thường, Admin có thể thử lại thao tác.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [BR-192](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6adb9749-5127-4c2b-a39e-e2ade6d4f533)

**Rationale:**
> Bao phủ EXC-03 và yêu cầu không tạo dữ liệu dở dang khi lưu thất bại.

---

## ST-055-09-03 — Admin không có quyền quản lý cấu hình thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-055-09-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a5018ba4-0361-4025-b65d-cb087808f765) |
| **Story** | STORY-055 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin không có quyền quản lý cấu hình thiệp.

**Steps:**
1. Admin truy cập chức năng thêm kích thước thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra form thêm kích thước thiệp.
4. Kiểm tra Core Database.

**Test Data:**
- Tài khoản Admin không có quyền quản lý cấu hình thiệp.

**Expected Result:**
- Hệ thống từ chối truy cập chức năng thêm kích thước thiệp.
- Hệ thống không hiển thị form thêm kích thước thiệp.
- Hệ thống không tạo dữ liệu mới.
- Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

**Trace to:**
- [STORY-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [STORY-055/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)

**Rationale:**
> Bao phủ EXC-04 và kiểm tra quyền Admin trước khi cho phép thêm kích thước thiệp.
