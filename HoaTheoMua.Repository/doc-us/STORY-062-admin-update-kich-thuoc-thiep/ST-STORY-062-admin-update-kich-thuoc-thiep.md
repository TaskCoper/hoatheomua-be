# ST — STORY-062 — Admin update kích thước thiệp — System Tests

---

## ST-062-01-01 — Cập nhật kích thước thiệp thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5a6a36a0-eb93-438d-8193-b66572f0b927) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp có thể đang Active hoặc Inactive. Kích thước thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thao tác “Cập nhật” tại một kích thước thiệp.
3. Quan sát dữ liệu hiện tại trên form cập nhật.
4. Admin chỉnh sửa một hoặc nhiều thông tin.
5. Admin chọn “Lưu”.
6. Quan sát phản hồi của hệ thống.
7. Quan sát dữ liệu mới trong danh sách kích thước thiệp.
8. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Dữ liệu ban đầu: Chiều rộng = 10 cm. Chiều cao = 15 cm. Giá size = 20000. Số lượng từ tối đa = 20. `isDelete = false`.
- Dữ liệu cập nhật: Chiều rộng = 12.5 cm. Chiều cao = 18 cm. Giá size = 30000. Số lượng từ tối đa = 25.5.

**Expected Result:**
- Hệ thống hiển thị form cập nhật với dữ liệu hiện tại gồm chiều rộng, chiều cao, giá size và số lượng từ tối đa.
- Hệ thống kiểm tra dữ liệu đầu vào và kiểm tra trùng kích thước.
- Hệ thống cập nhật dữ liệu kích thước thiệp trong Core Database.
- Hệ thống thông báo cập nhật kích thước thiệp thành công.
- Danh sách kích thước thiệp hiển thị dữ liệu mới.
- Trạng thái Active/Inactive và `isDelete` không bị thay đổi.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-219](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217)
- [BR-220](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4)
- [BR-222](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f625d9a-7d7d-4242-aa77-b448784d6961)

**Rationale:**
> Xác minh luồng chính khi Admin cập nhật các thông tin được phép của kích thước thiệp với dữ liệu hợp lệ.

---

## ST-062-02-01 — Hủy thao tác cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/89843ed7-ac44-415c-b50d-7ee067b482b4) |
| **Story** | STORY-062 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database. Kích thước thiệp chưa bị xóa mềm.
- Admin đang ở form cập nhật kích thước thiệp.

**Steps:**
1. Admin thay đổi một hoặc nhiều thông tin trên form cập nhật.
2. Admin chọn hủy hoặc đóng form mà không lưu.
3. Quan sát màn hình sau khi hủy.
4. Kiểm tra dữ liệu kích thước thiệp trong Core Database.

**Test Data:**
- Kích thước hiện tại: Chiều rộng = 10 cm. Chiều cao = 15 cm. Giá size = 20000. Số lượng từ tối đa = 20. Admin thay đổi dữ liệu nhưng chưa lưu.

**Expected Result:**
- Hệ thống không cập nhật dữ liệu trong Core Database.
- Dữ liệu kích thước thiệp được giữ nguyên.
- Hệ thống quay lại danh sách kích thước thiệp.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)

**Rationale:**
> Xác minh việc Admin hủy hoặc đóng form không làm phát sinh thay đổi dữ liệu.

---

## ST-062-03-01 — Chiều rộng và chiều cao là số thập phân

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/56affd8a-69a5-46da-9815-f754a12d5b31) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Không có kích thước khác trùng với dữ liệu sau cập nhật.

**Steps:**
1. Admin mở form cập nhật kích thước thiệp.
2. Admin nhập chiều rộng là số thập phân lớn hơn 0.
3. Admin nhập chiều cao là số thập phân lớn hơn 0.
4. Admin giữ các dữ liệu còn lại hợp lệ.
5. Admin chọn “Lưu”.
6. Quan sát kết quả cập nhật.
7. Kiểm tra Core Database.

**Test Data:**
- Chiều rộng = 10.5 cm. Chiều cao = 15.75 cm. Giá size = 20000. Số lượng từ tối đa = 20.

**Expected Result:**
- Hệ thống chấp nhận chiều rộng và chiều cao là số thập phân.
- Hệ thống cho phép lưu khi chiều rộng và chiều cao lớn hơn 0 và các dữ liệu còn lại hợp lệ.
- Dữ liệu mới được cập nhật thành công trong Core Database.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-220](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4)

**Rationale:**
> Xác minh chiều rộng và chiều cao được phép sử dụng số thập phân theo quy tắc validation.

---

## ST-062-04-01 — Validation giá trị biên của chiều rộng và chiều cao

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1c9dc4fd-2f95-425a-8a9a-120fa43ba049) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm. Admin đang ở form cập nhật.

**Steps:**
1. Nhập chiều rộng bằng 0 và chọn “Lưu”.
2. Quan sát phản hồi của hệ thống.
3. Nhập chiều rộng nhỏ hơn 0 và chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Nhập chiều rộng hợp lệ và chiều cao bằng 0. Chọn “Lưu”.
6. Quan sát phản hồi của hệ thống.
7. Nhập chiều cao nhỏ hơn 0 và chọn “Lưu”.
8. Quan sát phản hồi của hệ thống.
9. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Trường hợp 1: chiều rộng = 0.
- Trường hợp 2: chiều rộng = -1.
- Trường hợp 3: chiều cao = 0.
- Trường hợp 4: chiều cao = -1.
- Các trường còn lại hợp lệ.

**Expected Result:**
- Hệ thống không cập nhật kích thước thiệp trong tất cả các trường hợp.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Chiều rộng và chiều cao chỉ hợp lệ khi lớn hơn 0.
- Dữ liệu hiện tại trong Core Database được giữ nguyên. Admin có thể chỉnh sửa dữ liệu và thử lưu lại.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-220](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4)

**Rationale:**
> Xác minh validation giá trị biên của chiều rộng và chiều cao trước khi cập nhật.

---

## ST-062-05-01 — Giá size bằng 0 hoặc nhỏ hơn 0

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/549cf3ae-9754-458b-8694-057056cf4314) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm. Admin đang ở form cập nhật.

**Steps:**
1. Nhập giá size bằng 0 và giữ các dữ liệu khác hợp lệ.
2. Chọn “Lưu”.
3. Quan sát kết quả.
4. Mở lại form cập nhật.
5. Nhập giá size nhỏ hơn 0.
6. Chọn “Lưu”.
7. Quan sát phản hồi của hệ thống.
8. Kiểm tra Core Database.

**Test Data:**
- Trường hợp 1: Giá size = 0.
- Trường hợp 2: Giá size = -1.
- Các trường còn lại hợp lệ.

**Expected Result:**
- Hệ thống chấp nhận giá size bằng 0. Hệ thống cập nhật thành công khi giá size = 0 và các dữ liệu còn lại hợp lệ.
- Hệ thống từ chối khi giá size nhỏ hơn 0.
- Hệ thống hiển thị lỗi tại trường giá size khi dữ liệu không hợp lệ.
- Không lưu giá size âm vào Core Database.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-220](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4)

**Rationale:**
> Xác minh giá size được phép bằng 0 nhưng không được nhỏ hơn 0.

---

## ST-062-06-01 — Validation số lượng từ tối đa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7145961f-dee0-4fd7-a0c5-b99a0db94955) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm. Admin đang ở form cập nhật.

**Steps:**
1. Nhập số lượng từ tối đa là số thập phân lớn hơn 0. Chọn “Lưu”.
2. Quan sát kết quả.
3. Mở lại form cập nhật.
4. Nhập số lượng từ tối đa bằng 0. Chọn “Lưu”.
5. Quan sát phản hồi của hệ thống.
6. Nhập số lượng từ tối đa nhỏ hơn 0. Chọn “Lưu”.
7. Quan sát phản hồi của hệ thống.

**Test Data:**
- Trường hợp 1: số lượng từ tối đa = 20.5.
- Trường hợp 2: số lượng từ tối đa = 0.
- Trường hợp 3: số lượng từ tối đa = -1.
- Các trường còn lại hợp lệ.

**Expected Result:**
- Hệ thống chấp nhận số lượng từ tối đa là số thập phân lớn hơn 0.
- Hệ thống từ chối số lượng từ tối đa bằng 0.
- Hệ thống từ chối số lượng từ tối đa nhỏ hơn 0.
- Hệ thống hiển thị lỗi tại trường tương ứng khi dữ liệu không hợp lệ.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-220](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4)

**Rationale:**
> Xác minh số lượng từ tối đa phải lớn hơn 0 nhưng được phép sử dụng số thập phân.

---

## ST-062-07-01 — Cập nhật trùng kích thước thiệp đã tồn tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/dd2072a1-b60b-4b36-b6a9-147322fac12d) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Có ít nhất hai kích thước thiệp có `isDelete = false`. Admin đang cập nhật một trong các kích thước.

**Steps:**
1. Xác định chiều rộng và chiều cao của một kích thước khác đang tồn tại.
2. Admin thay đổi chiều rộng và chiều cao của kích thước đang cập nhật thành đúng cặp giá trị đó.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Kích thước A đang cập nhật: 10 x 15 cm.
- Kích thước B đã tồn tại: 12 x 18 cm. `isDelete = false`.
- Dữ liệu cập nhật của A: 12 x 18 cm.

**Expected Result:**
- Hệ thống phát hiện một kích thước khác có cùng chiều rộng và cùng chiều cao.
- Hệ thống không cập nhật kích thước thiệp.
- Hệ thống hiển thị thông báo kích thước đã tồn tại.
- Trạng thái Active hoặc Inactive của kích thước đã tồn tại không ảnh hưởng đến việc kiểm tra trùng.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-221](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00bd8c36-85db-4424-998e-19e9c3eb79ce)

**Rationale:**
> Xác minh hệ thống không cho phép tạo ra hai kích thước chưa xóa mềm có cùng chiều rộng và chiều cao.

---

## ST-062-08-01 — Cập nhật kích thước đảo ngược

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b0376a55-25f4-43d9-b4a4-9f7d66fa9515) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Đã tồn tại một kích thước có `isDelete = false`.

**Steps:**
1. Xác định một kích thước đã tồn tại có chiều rộng và chiều cao cụ thể.
2. Admin cập nhật kích thước khác thành cặp chiều rộng và chiều cao đảo ngược.
3. Admin chọn “Lưu”.
4. Quan sát kết quả.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Kích thước đã tồn tại: Chiều rộng = 5 cm. Chiều cao = 7 cm.
- Kích thước sau cập nhật: Chiều rộng = 7 cm. Chiều cao = 5 cm.

**Expected Result:**
- Hệ thống không xem 5x7 và 7x5 là hai kích thước trùng nhau.
- Hệ thống cho phép cập nhật nếu các dữ liệu còn lại hợp lệ.
- Dữ liệu mới được lưu vào Core Database.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-221](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00bd8c36-85db-4424-998e-19e9c3eb79ce)

**Rationale:**
> Xác minh quy tắc chỉ xem kích thước là trùng khi đồng thời cùng chiều rộng và cùng chiều cao.

---

## ST-062-09-01 — Không kiểm tra trùng với chính kích thước đang cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1e182dd2-bcb3-42d1-986f-cae53aff6779) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và có `isDelete = false`. Admin đang cập nhật kích thước đó.

**Steps:**
1. Admin giữ nguyên chiều rộng và chiều cao hiện tại.
2. Admin chỉ thay đổi giá size hoặc số lượng từ tối đa.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Dữ liệu hiện tại: Chiều rộng = 10 cm. Chiều cao = 15 cm. Giá size = 20000. Số lượng từ tối đa = 20.
- Dữ liệu cập nhật: Chiều rộng = 10 cm. Chiều cao = 15 cm. Giá size = 25000. Số lượng từ tối đa = 25.

**Expected Result:**
- Hệ thống không coi chính record đang cập nhật là kích thước trùng.
- Hệ thống cho phép lưu khi các dữ liệu còn lại hợp lệ.
- Giá size hoặc số lượng từ tối đa được cập nhật thành công.
- Chiều rộng và chiều cao được giữ nguyên.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-221](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00bd8c36-85db-4424-998e-19e9c3eb79ce)

**Rationale:**
> Xác minh record hiện tại được loại khỏi phép kiểm tra trùng kích thước.

---

## ST-062-10-01 — Trạng thái Active/Inactive không cản trở cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ebc34e9b-df57-4a18-b999-b1077fa0f132) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Có kích thước Active và Inactive đều có `isDelete = false`.

**Steps:**
1. Admin cập nhật một kích thước đang Active bằng dữ liệu hợp lệ.
2. Quan sát kết quả.
3. Admin cập nhật một kích thước đang Inactive bằng dữ liệu hợp lệ.
4. Quan sát kết quả.
5. Kiểm tra trạng thái của hai kích thước sau cập nhật.

**Test Data:**
- Kích thước A: Trạng thái = Active. `isDelete = false`.
- Kích thước B: Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Hệ thống cho phép cập nhật kích thước Active.
- Hệ thống cho phép cập nhật kích thước Inactive.
- Dữ liệu được cập nhật khi hợp lệ.
- Trạng thái Active/Inactive của từng kích thước không bị thay đổi bởi thao tác cập nhật.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-219](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217)
- [BR-222](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f625d9a-7d7d-4242-aa77-b448784d6961)

**Rationale:**
> Xác minh trạng thái Active hoặc Inactive không ảnh hưởng đến quyền cập nhật thông tin kích thước.

---

## ST-062-11-01 — Kích thước thiệp bị xóa mềm trước khi cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c4ccecc6-d89b-4a00-a00b-bafb2375a64f) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Admin đã mở form cập nhật khi kích thước còn `isDelete = false`.

**Steps:**
1. Admin mở form cập nhật kích thước thiệp.
2. Trước khi Admin lưu, cập nhật kích thước đó thành `isDelete = true` từ một thao tác khác.
3. Admin chỉnh sửa dữ liệu hợp lệ.
4. Admin chọn “Lưu”.
5. Quan sát phản hồi của hệ thống.
6. Quan sát danh sách kích thước thiệp.

**Test Data:**
- Kích thước ban đầu: `isDelete = false`.
- Trước thời điểm lưu: `isDelete = true`.

**Expected Result:**
- Backend kiểm tra lại trạng thái `isDelete` trước khi lưu.
- Hệ thống từ chối cập nhật kích thước đã bị xóa mềm.
- Hệ thống hiển thị thông báo kích thước không còn khả dụng.
- Hệ thống không cập nhật dữ liệu.
- Hệ thống tải lại danh sách kích thước thiệp hiện tại.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-219](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217)

**Rationale:**
> Xác minh dữ liệu được kiểm tra lại tại thời điểm lưu để tránh cập nhật record đã bị xóa mềm giữa chừng.

---

## ST-062-12-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f272eb2b-9b61-4fa5-89ac-51f284a7af4c) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và chưa bị xóa mềm.
- Admin đã nhập dữ liệu cập nhật hợp lệ.
- Có thể tạo tình huống Core Database cập nhật thất bại.

**Steps:**
1. Admin nhập dữ liệu cập nhật hợp lệ.
2. Tạo tình huống Core Database không thể cập nhật dữ liệu.
3. Admin chọn “Lưu”.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra dữ liệu trong Core Database.
6. Quan sát dữ liệu trên danh sách.
7. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một kích thước có dữ liệu hợp lệ. Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không ghi nhận cập nhật thành công.
- Hệ thống không hiển thị thông báo cập nhật thành công.
- Dữ liệu kích thước thiệp không được lưu ở trạng thái dở dang.
- Hệ thống hiển thị thông báo lỗi.
- Admin có thể thử lại sau khi lỗi được xử lý.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)

**Rationale:**
> Xác minh lỗi lưu không tạo dữ liệu không nhất quán và Core Database vẫn là nguồn xác thực cuối cùng.

---

## ST-062-13-01 — Người dùng không có quyền quản lý cấu hình thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-13-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3df48cd2-71f5-40c3-a9cc-ac012a08c8e3) |
| **Story** | STORY-062 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không phải Admin hoặc không có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database.

**Steps:**
1. Người dùng gửi yêu cầu cập nhật kích thước thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra dữ liệu kích thước thiệp trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình thiệp. Một kích thước thiệp tồn tại và chưa bị xóa mềm.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật dữ liệu.
- Hệ thống từ chối thao tác cập nhật.
- Hệ thống không cập nhật dữ liệu kích thước thiệp.
- Hệ thống hiển thị thông báo phù hợp.
- Core Database giữ nguyên dữ liệu trước khi thao tác.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình thiệp mới được phép cập nhật kích thước thiệp.

---

## ST-062-14-01 — Xác minh phạm vi cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-062-14-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/090a709f-e5c7-4a5a-af7e-2a0fed2e51c9) |
| **Story** | STORY-062 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại và có `isDelete = false`.
- Kích thước đang ở trạng thái Active hoặc Inactive.

**Steps:**
1. Ghi nhận trạng thái Active/Inactive và `isDelete` hiện tại.
2. Admin mở form cập nhật kích thước thiệp.
3. Admin thay đổi chiều rộng, chiều cao, giá size hoặc số lượng từ tối đa bằng dữ liệu hợp lệ.
4. Admin chọn “Lưu”.
5. Kiểm tra dữ liệu sau cập nhật trong Core Database.

**Test Data:**
- Kích thước: Trạng thái = Active hoặc Inactive. `isDelete = false`.
- Cập nhật một hoặc nhiều trường: Chiều rộng, Chiều cao, Giá size, Số lượng từ tối đa.

**Expected Result:**
- Hệ thống chỉ cập nhật các trường chiều rộng, chiều cao, giá size và số lượng từ tối đa được thay đổi.
- Trạng thái Active/Inactive không bị thay đổi.
- `isDelete` không bị thay đổi.

**Trace to:**
- [STORY-062](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [STORY-062/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9bb6f0c3-33bd-429e-863c-7ff582ee36bc)
- [BR-219](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217)
- [BR-222](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f625d9a-7d7d-4242-aa77-b448784d6961)

**Rationale:**
> Xác minh phạm vi STORY-062 chỉ cho phép cập nhật thông tin kích thước và không thay đổi trạng thái hoặc isDelete.
