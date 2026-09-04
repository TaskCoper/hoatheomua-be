# ST — STORY-067 — Admin thêm cấu hình phụ phí thiệp viết tay — System Tests

---

## ST-067-01-01 — Thêm cấu hình Active hợp lệ và không chồng lấn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0d2466e1-7a78-4275-8620-965587f34735) |
| **Story** | STORY-067 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở chức năng quản lý cấu hình phụ phí thiệp viết tay.
- Không có cấu hình Active chưa bị xóa mềm chồng lấn với khoảng số lượng từ cần tạo.

**Steps:**
1. Admin xem danh sách cấu hình phụ phí thiệp viết tay.
2. Admin chọn “Thêm cấu hình phụ phí thiệp viết tay”.
3. Quan sát form thêm mới.
4. Admin nhập số lượng từ bắt đầu.
5. Admin nhập số lượng từ kết thúc.
6. Admin nhập giá phụ phí.
7. Admin chọn trạng thái Active.
8. Admin chọn lưu.
9. Quan sát phản hồi của hệ thống.
10. Kiểm tra cấu hình mới trong Core Database.
11. Quan sát danh sách sau khi tạo.

**Test Data:**
- Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. Trạng thái = Active.
- Không có cấu hình Active có `isDelete = false` chồng lấn khoảng 1 đến 10.

**Expected Result:**
- Hệ thống hiển thị form gồm số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái.
- Hệ thống kiểm tra dữ liệu bắt buộc và định dạng dữ liệu.
- Hệ thống kiểm tra số lượng từ bắt đầu không lớn hơn số lượng từ kết thúc.
- Hệ thống kiểm tra giá phụ phí hợp lệ và không âm.
- Hệ thống kiểm tra khoảng số lượng từ không chồng lấn với cấu hình Active khác có `isDelete = false`.
- Hệ thống tạo đúng một cấu hình mới trong Core Database.
- Cấu hình mới có trạng thái Active và `isDelete = false`.
- Hệ thống thông báo thêm mới thành công.
- Danh sách được tải lại và hiển thị cấu hình vừa tạo.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)
- [BR-241](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08772035-b783-438f-9129-917061951915)

**Rationale:**
> Xác minh luồng chính khi Admin thêm cấu hình phụ phí viết tay Active với dữ liệu hợp lệ và không chồng lấn.

---

## ST-067-02-01 — Thêm cấu hình Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bdb7f320-a458-4d8b-8eaa-4111aa903997) |
| **Story** | STORY-067 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Admin nhập đầy đủ thông tin cấu hình hợp lệ.
2. Admin chọn trạng thái Inactive.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra cấu hình mới trong Core Database.
6. Quan sát danh sách quản lý.
7. Thực hiện yêu cầu xác định phụ phí cho thiệp Calligraphy mới có số lượng từ thuộc khoảng cấu hình.

**Test Data:**
- Số lượng từ bắt đầu = 11. Số lượng từ kết thúc = 20. Giá phụ phí = 30000. Trạng thái = Inactive.

**Expected Result:**
- Hệ thống tạo cấu hình mới với trạng thái Inactive. Cấu hình có `isDelete = false`.
- Hệ thống thông báo thêm mới thành công.
- Cấu hình được hiển thị trong danh sách quản lý.
- Cấu hình Inactive không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)

**Rationale:**
> Xác minh cấu hình Inactive vẫn được phép tạo và quản lý nhưng không tham gia xác định phụ phí cho thiệp mới.

---

## ST-067-03-01 — Hủy thao tác thêm mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3e070b12-8a6a-487b-b090-e513b51d7445) |
| **Story** | STORY-067 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Admin nhập một phần hoặc toàn bộ dữ liệu trên form.
2. Admin chọn hủy hoặc đóng form.
3. Quan sát màn hình sau khi hủy.
4. Kiểm tra Core Database.

**Test Data:**
- Admin đã nhập dữ liệu nhưng chưa lưu.

**Expected Result:**
- Hệ thống không tạo bản ghi mới.
- Admin quay lại danh sách cấu hình phụ phí thiệp viết tay.
- Core Database không phát sinh cấu hình mới từ thao tác đã hủy.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)

**Rationale:**
> Xác minh hủy thao tác thêm mới không làm phát sinh dữ liệu.

---

## ST-067-04-01 — Để trống các trường bắt buộc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/64f754e1-8ced-49f5-b4d7-eb8d371efc33) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Lần lượt để trống từng trường bắt buộc.
2. Admin chọn lưu.
3. Quan sát lỗi tại trường tương ứng.
4. Kiểm tra Core Database.

**Test Data:**
- Kiểm tra riêng các trường: Số lượng từ bắt đầu để trống. Số lượng từ kết thúc để trống. Giá phụ phí để trống. Trạng thái không được chọn.

**Expected Result:**
- Hệ thống không tạo cấu hình khi thiếu bất kỳ trường bắt buộc nào.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Admin có thể chỉnh sửa dữ liệu và thử lại.
- Core Database không phát sinh bản ghi mới.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)

**Rationale:**
> Xác minh đầy đủ dữ liệu bắt buộc phải được cung cấp trước khi tạo cấu hình.

---

## ST-067-05-01 — Validation định dạng và miền giá trị của khoảng số lượng từ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/065c5485-2556-44c6-86da-125564bad573) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Nhập số lượng từ bắt đầu nhỏ hơn 0 và chọn lưu.
2. Quan sát phản hồi.
3. Nhập số lượng từ bắt đầu là số thập phân và chọn lưu.
4. Quan sát phản hồi.
5. Nhập số lượng từ kết thúc bằng 0 và chọn lưu.
6. Quan sát phản hồi.
7. Nhập số lượng từ kết thúc nhỏ hơn 0 và chọn lưu.
8. Quan sát phản hồi.
9. Nhập số lượng từ kết thúc là số thập phân và chọn lưu.
10. Quan sát phản hồi.
11. Nhập ký tự không phải số vào trường số lượng từ.
12. Quan sát phản hồi.

**Test Data:**
- Các trường hợp: `wordFrom = -1`. `wordFrom = 1.5`. `wordTo = 0`. `wordTo = -1`. `wordTo = 10.5`. `wordFrom` hoặc `wordTo = ký tự không phải số`.

**Expected Result:**
- Hệ thống chỉ chấp nhận `wordFrom` là số nguyên lớn hơn hoặc bằng 0.
- Hệ thống chỉ chấp nhận `wordTo` là số nguyên lớn hơn 0.
- Hệ thống không chấp nhận phần thập phân. Hệ thống không chấp nhận ký tự không phải số.
- Hệ thống không tạo cấu hình khi dữ liệu không hợp lệ.
- Hệ thống hiển thị lỗi phù hợp tại trường tương ứng.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh validation định dạng và miền giá trị của khoảng số lượng từ theo Business Rule.

---

## ST-067-06-01 — Số lượng từ bắt đầu bằng 0 (trường hợp không lời chúc)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b7b3f6b-b03a-48d4-b9e1-e0cb1e8d97ea) |
| **Story** | STORY-067 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Không có cấu hình Active chồng lấn với khoảng cần tạo.

**Steps:**
1. Admin mở form thêm cấu hình.
2. Admin nhập số lượng từ bắt đầu bằng 0.
3. Admin nhập số lượng từ kết thúc lớn hơn 0.
4. Admin nhập giá phụ phí hợp lệ.
5. Admin chọn trạng thái phù hợp.
6. Admin chọn lưu.
7. Quan sát kết quả.

**Test Data:**
- `wordFrom = 0`. `wordTo = 5`. Giá phụ phí = 0 hoặc số nguyên không âm hợp lệ.

**Expected Result:**
- Hệ thống chấp nhận `wordFrom = 0`.
- Hệ thống cho phép tạo cấu hình nếu `wordTo` lớn hơn 0 và các dữ liệu còn lại hợp lệ.
- Cấu hình mới được lưu thành công.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh trường hợp đặc biệt wordFrom = 0 được chấp nhận để cấu hình trường hợp không có lời chúc.

---

## ST-067-07-01 — Số lượng từ bắt đầu lớn hơn kết thúc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd8a1169-a7a6-4f5d-b683-8c58a75f79b3) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Admin nhập số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
2. Admin nhập các dữ liệu còn lại hợp lệ.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra Core Database.

**Test Data:**
- Số lượng từ bắt đầu = 20. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Hệ thống không tạo cấu hình.
- Hệ thống thông báo khoảng số lượng từ không hợp lệ.
- Admin có thể điều chỉnh dữ liệu.
- Core Database không phát sinh bản ghi mới.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh wordFrom phải nhỏ hơn hoặc bằng wordTo.

---

## ST-067-08-01 — Số lượng từ bắt đầu bằng kết thúc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8b770ed3-04c1-4d9b-8b21-22f3dd34bad0) |
| **Story** | STORY-067 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Không có cấu hình Active khác chồng lấn với giá trị cần tạo.

**Steps:**
1. Admin mở form thêm cấu hình.
2. Admin nhập cùng một giá trị cho số lượng từ bắt đầu và kết thúc.
3. Admin nhập giá phụ phí hợp lệ.
4. Admin chọn trạng thái.
5. Admin chọn lưu.
6. Quan sát kết quả.

**Test Data:**
- Số lượng từ bắt đầu = 10. Số lượng từ kết thúc = 10. Giá phụ phí = 20000.

**Expected Result:**
- Hệ thống xem khoảng 10 đến 10 là hợp lệ.
- Hệ thống cho phép tạo cấu hình nếu các điều kiện còn lại hợp lệ.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh điều kiện khoảng cho phép wordFrom bằng wordTo.

---

## ST-067-09-01 — Khoảng mới chồng lấn với cấu hình Active đã tồn tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1bc58ad2-f525-4cf3-9505-90b2949c92e6) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Đã tồn tại một cấu hình Active có `isDelete = false`.

**Steps:**
1. Admin mở form thêm cấu hình.
2. Admin nhập khoảng số lượng từ chồng lấn với cấu hình Active đã tồn tại.
3. Admin nhập giá phụ phí hợp lệ.
4. Admin chọn trạng thái Active.
5. Admin chọn lưu.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra Core Database.

**Test Data:**
- Cấu hình đã tồn tại: Khoảng = 1 đến 10. Trạng thái = Active. `isDelete = false`.
- Cấu hình mới: Khoảng = 8 đến 15. Trạng thái = Active.

**Expected Result:**
- Hệ thống phát hiện khoảng mới chồng lấn với cấu hình Active khác có `isDelete = false`.
- Hệ thống không tạo cấu hình mới.
- Hệ thống thông báo khoảng số lượng từ bị chồng lấn.
- Hệ thống hiển thị khoảng của cấu hình Active đang gây chồng lấn để Admin nhận diện và điều chỉnh dữ liệu.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh không có hai cấu hình Active chưa bị xóa mềm cùng bao phủ một số lượng từ.

---

## ST-067-10-01 — Các trường hợp chồng lấn biên và bao phủ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c4178471-427a-4011-8382-52712dfe1301) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Đã tồn tại cấu hình Active có `isDelete = false` với khoảng xác định.

**Steps:**
1. Tạo cấu hình Active mới có điểm bắt đầu trùng điểm kết thúc của cấu hình đã tồn tại. Quan sát kết quả.
2. Thực hiện với trường hợp khoảng mới bao phủ một phần đầu của khoảng hiện có.
3. Thực hiện với trường hợp khoảng mới bao phủ hoàn toàn khoảng hiện có.

**Test Data:**
- Cấu hình hiện có = 1 đến 10, Active, `isDelete = false`.
- Các khoảng mới: 10 đến 20. 5 đến 15. 0 đến 20.

**Expected Result:**
- Hệ thống xác định các khoảng 10 đến 20, 5 đến 15 và 0 đến 20 đều chồng lấn với khoảng 1 đến 10.
- Hệ thống không tạo cấu hình Active mới trong các trường hợp này.
- Hệ thống hiển thị thông tin khoảng đang gây xung đột.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh logic chồng lấn sử dụng khoảng đóng và xử lý đúng các trường hợp biên.

---

## ST-067-11-01 — Cấu hình Inactive không tham gia ràng buộc chồng lấn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c02857c6-f685-41a3-ad3f-79ec2459b9e3) |
| **Story** | STORY-067 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Đã tồn tại cấu hình Active hoặc Inactive với khoảng số lượng từ xác định.

**Steps:**
1. Admin mở form thêm cấu hình.
2. Admin nhập khoảng số lượng từ chồng lấn với cấu hình đã tồn tại.
3. Admin chọn trạng thái Inactive.
4. Admin nhập giá phụ phí hợp lệ.
5. Admin chọn lưu.
6. Quan sát kết quả.

**Test Data:**
- Cấu hình đã tồn tại: Khoảng = 1 đến 10.
- Cấu hình mới: Khoảng = 5 đến 15. Trạng thái = Inactive.

**Expected Result:**
- Hệ thống cho phép tạo cấu hình Inactive nếu các dữ liệu khác hợp lệ.
- Cấu hình Inactive không bị từ chối chỉ vì khoảng số lượng từ chồng lấn.
- Cấu hình mới có `isDelete = false`.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh cấu hình Inactive không tham gia ràng buộc chồng lấn cho đến khi được chuyển sang Active.

---

## ST-067-12-01 — Kiểm tra chồng lấn chỉ xét cấu hình chưa bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/61526997-6454-4573-ab76-17ea9e7032c5) |
| **Story** | STORY-067 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Đã tồn tại một cấu hình Active nhưng có `isDelete = true`.

**Steps:**
1. Admin mở form thêm cấu hình.
2. Admin nhập khoảng giống hoặc chồng lấn với cấu hình đã xóa mềm.
3. Admin chọn trạng thái Active.
4. Admin nhập giá phụ phí hợp lệ.
5. Admin chọn lưu.
6. Quan sát kết quả.

**Test Data:**
- Cấu hình cũ: Khoảng = 1 đến 10. Trạng thái = Active. `isDelete = true`.
- Cấu hình mới: Khoảng = 1 đến 10. Trạng thái = Active.

**Expected Result:**
- Hệ thống không dùng cấu hình có `isDelete = true` để chặn việc tạo cấu hình Active mới.
- Hệ thống cho phép tạo nếu không có cấu hình Active khác có `isDelete = false` chồng lấn.
- Cấu hình mới có `isDelete = false`.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh phép kiểm tra chồng lấn chỉ xét các cấu hình Active chưa bị xóa mềm.

---

## ST-067-13-01 — Giá phụ phí (bằng 0, âm, thập phân, chữ)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-13-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2777fefd-abbd-497d-b9e8-fbb019d566f6) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Nhập giá phụ phí bằng 0 và lưu với các dữ liệu khác hợp lệ. Quan sát kết quả.
2. Nhập giá phụ phí nhỏ hơn 0. Chọn lưu. Quan sát phản hồi.
3. Nhập giá phụ phí là số thập phân. Chọn lưu. Quan sát phản hồi.
4. Nhập giá phụ phí là ký tự không phải số. Chọn lưu. Quan sát phản hồi.

**Test Data:**
- Trường hợp hợp lệ: Giá phụ phí = 0.
- Trường hợp không hợp lệ: Giá phụ phí = -1. Giá phụ phí = 20000.5. Giá phụ phí = "abc".

**Expected Result:**
- Hệ thống chấp nhận giá phụ phí bằng 0.
- Hệ thống chỉ chấp nhận giá phụ phí là số nguyên không âm.
- Hệ thống từ chối giá âm. Hệ thống từ chối giá có phần thập phân. Hệ thống từ chối giá không phải số.
- Hệ thống không tạo cấu hình với giá không hợp lệ và hiển thị lỗi phù hợp.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)

**Rationale:**
> Xác minh giá phụ phí phải là số nguyên VNĐ không âm và cho phép giá bằng 0.

---

## ST-067-14-01 — Lỗi lưu Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-14-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/34ec13aa-f489-427d-8ba7-6e0b8a54c0e5) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đã nhập dữ liệu hợp lệ. Có thể tạo tình huống lỗi khi lưu Core Database.

**Steps:**
1. Admin nhập đầy đủ dữ liệu hợp lệ.
2. Tạo tình huống hệ thống gặp lỗi trong quá trình lưu.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra Core Database.
6. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một cấu hình hợp lệ. Môi trường test có khả năng tạo lỗi khi lưu.

**Expected Result:**
- Hệ thống không tạo bản ghi dở dang hoặc không đồng nhất.
- Hệ thống không ghi nhận thêm mới thành công.
- Hệ thống thông báo thêm mới thất bại. Admin có thể thử lại.
- Core Database là nguồn xác thực cuối cùng.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)

**Rationale:**
> Xác minh lỗi lưu không làm phát sinh cấu hình ở trạng thái không hoàn chỉnh.

---

## ST-067-15-01 — Người dùng không có quyền quản lý

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-15-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e53d95c6-8ea3-4d6c-ac46-8b6f0192b5f0) |
| **Story** | STORY-067 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý cấu hình giá thiệp.

**Steps:**
1. Người dùng gửi yêu cầu thêm cấu hình phụ phí thiệp viết tay.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình giá thiệp.

**Expected Result:**
- Backend kiểm tra quyền trước khi tạo cấu hình.
- Hệ thống từ chối yêu cầu.
- Hệ thống không tạo dữ liệu mới.
- Hệ thống hiển thị thông báo phù hợp.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/EXC-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [BR-241](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08772035-b783-438f-9129-917061951915)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình giá thiệp mới được tạo cấu hình phụ phí viết tay.

---

## ST-067-16-01 — Thêm mới tạo duy nhất một bản ghi

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-067-16-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5399c9a8-3790-48f3-94a9-56d48db18f61) |
| **Story** | STORY-067 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Dữ liệu cấu hình hợp lệ và không chồng lấn.

**Steps:**
1. Ghi nhận số lượng bản ghi cấu hình hiện tại.
2. Admin gửi một yêu cầu thêm cấu hình hợp lệ.
3. Chờ thao tác hoàn tất.
4. Kiểm tra Core Database.
5. Đối chiếu số lượng bản ghi trước và sau thao tác.

**Test Data:**
- Một cấu hình hợp lệ được gửi trong một yêu cầu thêm mới.

**Expected Result:**
- Yêu cầu thành công chỉ tạo đúng một cấu hình phụ phí mới.
- Không phát sinh bản ghi trùng do cùng một thao tác.
- Cấu hình mới có `isDelete = false`.

**Trace to:**
- [STORY-067](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)
- [STORY-067/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7fe025c1-fd01-425b-81af-a1abb5521e04)

**Rationale:**
> Xác minh yêu cầu tạo thành công không tạo nhiều bản ghi cấu hình ngoài dự kiến.
