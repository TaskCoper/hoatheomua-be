# ST — STORY-068 — Admin cập nhật cấu hình phụ phí thiệp viết tay — System Tests

---

## ST-068-01-01 — Luồng chính cập nhật khoảng số lượng từ hoặc giá phụ phí

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/090fbdce-5b19-48b5-920c-64b76e993ccc) |
| **Story** | STORY-068 |
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
2. Admin chọn thao tác cập nhật tại một cấu hình.
3. Quan sát dữ liệu hiện tại trên form cập nhật.
4. Admin thay đổi một hoặc nhiều thông tin.
5. Admin chọn lưu.
6. Quan sát phản hồi của hệ thống.
7. Quan sát thông tin mới trên danh sách.
8. Kiểm tra dữ liệu cấu hình trong Core Database.

**Test Data:**
- Dữ liệu hiện tại: Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 10. Giá phụ phí = 20000. Trạng thái = Active. `isDelete = false`.
- Dữ liệu cập nhật: Số lượng từ bắt đầu = 1. Số lượng từ kết thúc = 12. Giá phụ phí = 25000.
- Không có cấu hình Active khác có `isDelete = false` chồng lấn khoảng 1 đến 12.

**Expected Result:**
- Hệ thống tải dữ liệu hiện tại từ Core Database. Form hiển thị số lượng từ bắt đầu, số lượng từ kết thúc và giá phụ phí hiện tại.
- Hệ thống kiểm tra dữ liệu nhập.
- Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
- Hệ thống kiểm tra khoảng số lượng từ hợp lệ. Hệ thống kiểm tra giá phụ phí hợp lệ.
- Nếu cấu hình đang Active, hệ thống kiểm tra khoảng mới không chồng lấn với cấu hình Active khác có `isDelete = false` và loại chính cấu hình đang cập nhật khỏi tập so sánh.
- Hệ thống cập nhật thông tin cấu hình trong Core Database.
- Hệ thống giữ nguyên trạng thái Active/Inactive hiện tại.
- Hệ thống thông báo cập nhật thành công. Danh sách hiển thị thông tin mới.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-242](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/58ea7f6a-ef08-48df-abc7-0beed7a7a858)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)
- [BR-243](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2)

**Rationale:**
> Xác minh luồng chính khi Admin cập nhật khoảng số lượng từ hoặc giá phụ phí với dữ liệu hợp lệ.

---

## ST-068-02-01 — Chỉ cập nhật giá, không làm thay đổi khoảng số lượng từ hoặc trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/500f862f-ce0d-43cf-8c23-3b4a38154594) |
| **Story** | STORY-068 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin mở form cập nhật cấu hình.
2. Ghi nhận khoảng số lượng từ và trạng thái hiện tại.
3. Admin chỉ thay đổi giá phụ phí.
4. Admin chọn lưu.
5. Quan sát kết quả.
6. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Dữ liệu hiện tại: Khoảng số lượng từ = 1 đến 10. Giá phụ phí = 20000. Trạng thái = Active.
- Dữ liệu cập nhật: Giá phụ phí = 25000.

**Expected Result:**
- Hệ thống kiểm tra giá mới hợp lệ. Hệ thống cập nhật giá phụ phí.
- Khoảng số lượng từ được giữ nguyên. Trạng thái Active/Inactive được giữ nguyên.
- Hệ thống thông báo cập nhật thành công.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-242](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/58ea7f6a-ef08-48df-abc7-0beed7a7a858)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)
- [BR-243](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2)

**Rationale:**
> Xác minh Admin có thể chỉ cập nhật giá mà không làm thay đổi khoảng số lượng từ hoặc trạng thái.

---

## ST-068-03-01 — Cập nhật cấu hình Inactive không tự động chuyển thành Active

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7893e248-80fa-42d0-b521-3230bc90d9b9) |
| **Story** | STORY-068 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay đang ở trạng thái Inactive. Cấu hình còn tồn tại và chưa bị xóa mềm.

**Steps:**
1. Admin mở form cập nhật cấu hình Inactive.
2. Admin thay đổi khoảng số lượng từ hoặc giá phụ phí.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra trạng thái cấu hình trong Core Database.

**Test Data:**
- Cấu hình: Khoảng hiện tại = 11 đến 20. Giá phụ phí = 30000. Trạng thái = Inactive. `isDelete = false`.
- Dữ liệu cập nhật hợp lệ.

**Expected Result:**
- Hệ thống cập nhật thông tin hợp lệ của cấu hình.
- Hệ thống giữ nguyên trạng thái Inactive.
- Cấu hình Inactive không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới cho đến khi được chuyển sang Active.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-242](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/58ea7f6a-ef08-48df-abc7-0beed7a7a858)
- [BR-243](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2)

**Rationale:**
> Xác minh cấu hình Inactive vẫn được phép cập nhật nhưng không tự động trở thành cấu hình khả dụng.

---

## ST-068-04-01 — Hủy hoặc đóng form không làm thay đổi dữ liệu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef9c6fca-8819-44fc-ac8e-a94bed8f7f9c) |
| **Story** | STORY-068 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form cập nhật cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Admin thay đổi một hoặc nhiều thông tin trên form.
2. Admin chọn hủy hoặc đóng form.
3. Quan sát màn hình sau khi hủy.
4. Kiểm tra dữ liệu cấu hình trong Core Database.

**Test Data:**
- Admin đã thay đổi khoảng số lượng từ hoặc giá phụ phí nhưng chưa lưu.

**Expected Result:**
- Hệ thống không cập nhật dữ liệu.
- Cấu hình giữ nguyên thông tin trước khi thao tác.
- Core Database không ghi nhận các thay đổi chưa lưu.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)

**Rationale:**
> Xác minh hủy hoặc đóng form không làm thay đổi dữ liệu cấu hình.

---

## ST-068-05-01 — Xóa giá trị tại các trường bắt buộc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/23b01304-714d-4304-b82d-e1c0ee65663e) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form cập nhật cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Lần lượt xóa giá trị của số lượng từ bắt đầu, số lượng từ kết thúc hoặc giá phụ phí.
2. Admin chọn lưu.
3. Quan sát lỗi tại trường tương ứng.
4. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Các trường hợp: Số lượng từ bắt đầu để trống. Số lượng từ kết thúc để trống. Giá phụ phí để trống.

**Expected Result:**
- Hệ thống xác định dữ liệu bắt buộc bị thiếu.
- Hệ thống không cập nhật cấu hình.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Admin có thể chỉnh sửa và thử lại.
- Core Database giữ nguyên dữ liệu trước lần cập nhật.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)

**Rationale:**
> Xác minh backend và frontend không cho phép cập nhật khi dữ liệu bắt buộc bị thiếu.

---

## ST-068-06-01 — Validation định dạng và miền giá trị của khoảng số lượng từ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4eb32417-4fbf-4ab0-8c22-115093d08544) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form cập nhật cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Nhập `wordFrom` nhỏ hơn 0 và chọn lưu. Quan sát phản hồi.
2. Nhập `wordFrom` là số thập phân và chọn lưu. Quan sát phản hồi.
3. Nhập `wordTo` bằng 0 và chọn lưu. Quan sát phản hồi.
4. Nhập `wordTo` nhỏ hơn 0 và chọn lưu. Quan sát phản hồi.
5. Nhập `wordTo` là số thập phân và chọn lưu. Quan sát phản hồi.
6. Nhập ký tự không phải số vào trường khoảng số lượng từ. Quan sát phản hồi.

**Test Data:**
- Các trường hợp: `wordFrom = -1`. `wordFrom = 1.5`. `wordTo = 0`. `wordTo = -1`. `wordTo = 10.5`. `wordFrom` hoặc `wordTo = ký tự không phải số`.

**Expected Result:**
- Hệ thống chỉ chấp nhận `wordFrom` là số nguyên lớn hơn hoặc bằng 0.
- Hệ thống chỉ chấp nhận `wordTo` là số nguyên lớn hơn 0.
- Hệ thống không chấp nhận số thập phân, không chấp nhận ký tự không phải số.
- Hệ thống không cập nhật cấu hình khi dữ liệu không hợp lệ. Hệ thống hiển thị lỗi tại trường tương ứng.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh validation định dạng và miền giá trị của khoảng số lượng từ khi cập nhật.

---

## ST-068-07-01 — Số lượng từ bắt đầu bằng 0 (trường hợp không lời chúc)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c3bbb619-6e33-4944-a042-924b47cb3770) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình tồn tại và chưa bị xóa mềm. Nếu cấu hình đang Active, không có cấu hình Active khác chồng lấn với khoảng sau cập nhật.

**Steps:**
1. Admin mở form cập nhật cấu hình.
2. Admin nhập số lượng từ bắt đầu bằng 0.
3. Admin nhập số lượng từ kết thúc lớn hơn 0.
4. Admin giữ giá phụ phí hợp lệ.
5. Admin chọn lưu.
6. Quan sát kết quả.

**Test Data:**
- `wordFrom = 0`. `wordTo = 5`. Giá phụ phí = 0 hoặc số nguyên không âm hợp lệ.

**Expected Result:**
- Hệ thống chấp nhận `wordFrom = 0`.
- Hệ thống cho phép cập nhật nếu `wordTo` lớn hơn 0 và các dữ liệu còn lại hợp lệ.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh wordFrom = 0 được chấp nhận để hỗ trợ trường hợp không có lời chúc.

---

## ST-068-08-01 — Số lượng từ bắt đầu lớn hơn kết thúc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/416072bc-0c5c-4174-902e-9b7093d8de4f) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang cập nhật một cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Admin nhập số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
2. Admin chọn lưu.
3. Quan sát phản hồi của hệ thống.
4. Kiểm tra dữ liệu cấu hình trong Core Database.

**Test Data:**
- Số lượng từ bắt đầu = 20. Số lượng từ kết thúc = 10. Giá phụ phí hợp lệ.

**Expected Result:**
- Hệ thống không cập nhật dữ liệu.
- Hệ thống thông báo khoảng số lượng từ không hợp lệ.
- Dữ liệu hiện tại trong Core Database được giữ nguyên.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh wordFrom phải nhỏ hơn hoặc bằng wordTo.

---

## ST-068-09-01 — Số lượng từ bắt đầu bằng kết thúc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9683ccc7-4160-4f94-969f-57cbe570a3fd) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình tồn tại và chưa bị xóa mềm. Không có cấu hình Active khác chồng lấn với giá trị cần cập nhật nếu cấu hình đang Active.

**Steps:**
1. Admin mở form cập nhật.
2. Admin nhập cùng một giá trị cho số lượng từ bắt đầu và kết thúc.
3. Admin giữ giá phụ phí hợp lệ.
4. Admin chọn lưu.
5. Quan sát kết quả.

**Test Data:**
- Số lượng từ bắt đầu = 10. Số lượng từ kết thúc = 10. Giá phụ phí hợp lệ.

**Expected Result:**
- Hệ thống xem khoảng 10 đến 10 là hợp lệ.
- Hệ thống cho phép cập nhật nếu các điều kiện còn lại hợp lệ.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)

**Rationale:**
> Xác minh khoảng đơn điểm với wordFrom bằng wordTo được chấp nhận.

---

## ST-068-10-01 — Khoảng mới chồng lấn với cấu hình Active khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/766d942e-f100-4ab1-a268-152018dccfb2) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình đang cập nhật có trạng thái Active và `isDelete = false`.
- Đã tồn tại một cấu hình Active khác có `isDelete = false`.

**Steps:**
1. Admin mở form cập nhật cấu hình Active.
2. Admin thay đổi khoảng số lượng từ thành khoảng chồng lấn với cấu hình Active khác.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Kiểm tra dữ liệu trong Core Database.

**Test Data:**
- Cấu hình A đang cập nhật: Khoảng hiện tại = 1 đến 5. Trạng thái = Active.
- Cấu hình B: Khoảng = 10 đến 20. Trạng thái = Active. `isDelete = false`.
- Khoảng mới của A: 8 đến 15.

**Expected Result:**
- Hệ thống phát hiện khoảng mới chồng lấn với cấu hình Active khác có `isDelete = false`.
- Hệ thống không cập nhật dữ liệu.
- Hệ thống thông báo khoảng số lượng từ bị chồng lấn.
- Hệ thống hiển thị khoảng của cấu hình Active đang gây chồng lấn để Admin nhận biết và điều chỉnh.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh cấu hình Active sau cập nhật không được chồng lấn với cấu hình Active khác.

---

## ST-068-11-01 — Các trường hợp chồng lấn biên và bao phủ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a536b835-5889-44ec-a626-c9f9eba20ea1) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Đã tồn tại một cấu hình Active khác có `isDelete = false`.

**Steps:**
1. Cập nhật cấu hình Active thành khoảng có điểm kết thúc hoặc điểm bắt đầu trùng biên với cấu hình Active khác. Admin chọn lưu. Quan sát phản hồi.
2. Thực hiện với nhiều kiểu chồng lấn khác nhau (bao phủ, nằm trong, giao nhau).

**Test Data:**
- Cấu hình Active khác: Khoảng = 10 đến 20. `isDelete = false`.
- Các khoảng cập nhật cần kiểm tra: 1 đến 10, 20 đến 30, 5 đến 15, 15 đến 25, 5 đến 25.

**Expected Result:**
- Các khoảng có chung ít nhất một giá trị với khoảng 10 đến 20 được xem là chồng lấn.
- Hệ thống không cập nhật cấu hình trong các trường hợp chồng lấn.
- Hệ thống hiển thị khoảng cấu hình Active đang gây xung đột.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh kiểm tra chồng lấn xử lý đúng cả các trường hợp giao nhau tại biên.

---

## ST-068-12-01 — Không tự xung đột với chính record đang cập nhật

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e297bb7b-ec4d-4825-977f-24d67094af65) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình đang cập nhật có trạng thái Active và `isDelete = false`.

**Steps:**
1. Admin mở form cập nhật cấu hình Active.
2. Admin giữ nguyên khoảng số lượng từ hiện tại nhưng thay đổi giá phụ phí. Admin chọn lưu. Quan sát kết quả.
3. Tiếp tục thay đổi khoảng số lượng từ nhưng vẫn không chồng lấn với cấu hình Active khác. Admin chọn lưu. Quan sát kết quả.

**Test Data:**
- Cấu hình đang cập nhật: Khoảng hiện tại = 1 đến 10. Trạng thái = Active. `isDelete = false`.
- Không có cấu hình Active khác chồng lấn khoảng 1 đến 10.

**Expected Result:**
- Hệ thống loại chính cấu hình đang cập nhật khỏi tập dữ liệu dùng để kiểm tra chồng lấn.
- Hệ thống không coi chính record hiện tại là dữ liệu xung đột.
- Hệ thống cho phép lưu khi không có cấu hình Active khác chồng lấn.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh logic kiểm tra chồng lấn không tự xung đột với chính record đang được cập nhật.

---

## ST-068-13-01 — Cấu hình Inactive không tham gia ràng buộc chồng lấn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-13-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/745fc919-c749-4263-93e0-f92d3a2202fa) |
| **Story** | STORY-068 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình đang cập nhật có trạng thái Inactive và `isDelete = false`.
- Đã tồn tại cấu hình Active với khoảng số lượng từ xác định.

**Steps:**
1. Admin mở form cập nhật cấu hình Inactive.
2. Admin cập nhật khoảng số lượng từ thành khoảng chồng lấn với cấu hình Active hiện có.
3. Admin giữ dữ liệu còn lại hợp lệ.
4. Admin chọn lưu.
5. Quan sát kết quả.

**Test Data:**
- Cấu hình Active: Khoảng = 1 đến 10. `isDelete = false`.
- Cấu hình Inactive đang cập nhật: Khoảng mới = 5 đến 15.

**Expected Result:**
- Hệ thống cho phép cập nhật cấu hình Inactive nếu dữ liệu còn lại hợp lệ.
- Cấu hình Inactive không tham gia ràng buộc chồng lấn cho đến khi được chuyển sang Active.
- Trạng thái Inactive được giữ nguyên.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)
- [BR-243](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2)

**Rationale:**
> Xác minh quy tắc chồng lấn chỉ áp dụng cho cấu hình đang Active.

---

## ST-068-14-01 — Bỏ qua kiểm tra chồng lấn với cấu hình đã bị xóa mềm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-14-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/86a3edad-37c2-4ce3-85f4-443a03fcc439) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình đang cập nhật có trạng thái Active. Có một cấu hình khác đang Active nhưng `isDelete = true`.

**Steps:**
1. Admin mở form cập nhật cấu hình Active.
2. Admin thay đổi khoảng thành khoảng trùng hoặc chồng lấn với cấu hình Active đã xóa mềm.
3. Admin chọn lưu.
4. Quan sát kết quả.

**Test Data:**
- Cấu hình đã xóa mềm: Khoảng = 1 đến 10. Trạng thái = Active. `isDelete = true`.
- Khoảng mới của cấu hình đang cập nhật = 1 đến 10.

**Expected Result:**
- Hệ thống không sử dụng cấu hình có `isDelete = true` để chặn cập nhật.
- Hệ thống cho phép lưu nếu không có cấu hình Active khác có `isDelete = false` chồng lấn.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh kiểm tra chồng lấn chỉ xét các cấu hình Active chưa bị xóa mềm.

---

## ST-068-15-01 — Giá phụ phí (bằng 0, âm, thập phân, chữ)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-15-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3dc752f8-60ca-414b-9326-cd53481755dd) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở form cập nhật cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Nhập giá phụ phí bằng 0 và lưu với các dữ liệu khác hợp lệ. Quan sát kết quả.
2. Nhập giá phụ phí nhỏ hơn 0 và chọn lưu. Quan sát phản hồi.
3. Nhập giá phụ phí là số thập phân và chọn lưu. Quan sát phản hồi.
4. Nhập giá phụ phí là ký tự không phải số và chọn lưu. Quan sát phản hồi.

**Test Data:**
- Trường hợp hợp lệ: Giá phụ phí = 0.
- Trường hợp không hợp lệ: Giá phụ phí = -1. Giá phụ phí = 20000.5. Giá phụ phí = "abc".

**Expected Result:**
- Hệ thống chấp nhận giá phụ phí bằng 0. Hệ thống chỉ chấp nhận giá phụ phí là số nguyên không âm.
- Hệ thống từ chối giá âm. Hệ thống từ chối giá có phần thập phân. Hệ thống từ chối dữ liệu không phải số.
- Hệ thống không cập nhật cấu hình với giá không hợp lệ. Hệ thống hiển thị lỗi phù hợp.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)

**Rationale:**
> Xác minh giá phụ phí phải là số nguyên VNĐ không âm và cho phép giá bằng 0.

---

## ST-068-16-01 — Không cập nhật record không còn hợp lệ (đã bị xóa)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-16-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cdd7f858-77cd-40d9-bf6b-0aee5c55a78b) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Admin đã mở form cập nhật cấu hình phụ phí thiệp viết tay.

**Steps:**
1. Sau khi form được mở, làm cho cấu hình không còn tồn tại hoặc có `isDelete = true`.
2. Admin nhập dữ liệu cập nhật hợp lệ.
3. Admin chọn lưu.
4. Quan sát phản hồi của hệ thống.
5. Quan sát danh sách sau phản hồi.

**Test Data:**
- Trường hợp 1: Cấu hình không còn tồn tại trong Core Database.
- Trường hợp 2: Cấu hình có `isDelete = true` trước thời điểm lưu.

**Expected Result:**
- Backend kiểm tra lại cấu hình trước khi cập nhật.
- Hệ thống không cập nhật dữ liệu.
- Hệ thống thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- Hệ thống tải lại danh sách.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)

**Rationale:**
> Xác minh hệ thống không cập nhật record không còn hợp lệ tại thời điểm xử lý.

---

## ST-068-17-01 — Lỗi cập nhật Core Database

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-17-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceac4550-d6f2-4cc2-b282-56db98f6bea9) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình tồn tại và chưa bị xóa mềm. Admin đã nhập dữ liệu cập nhật hợp lệ.
- Có thể tạo tình huống lỗi khi cập nhật Core Database.

**Steps:**
1. Admin nhập dữ liệu cập nhật hợp lệ.
2. Tạo tình huống hệ thống gặp lỗi trong quá trình cập nhật.
3. Admin chọn lưu.
4. Quan sát phản hồi.
5. Kiểm tra dữ liệu trong Core Database.
6. Khôi phục hệ thống và thử lại.

**Test Data:**
- Một cấu hình hợp lệ. Môi trường test có khả năng tạo lỗi khi cập nhật.

**Expected Result:**
- Hệ thống không lưu dữ liệu dở dang hoặc không đồng nhất.
- Cấu hình giữ nguyên dữ liệu trước lần cập nhật.
- Hệ thống thông báo cập nhật thất bại. Admin có thể thử lại.
- Core Database là nguồn xác thực cuối cùng.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)

**Rationale:**
> Xác minh lỗi cập nhật không làm cấu hình rơi vào trạng thái dữ liệu không nhất quán.

---

## ST-068-18-01 — Người dùng không có quyền quản lý cấu hình giá thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-18-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c5fc294-1c84-4797-b1b1-e98da4b6c092) |
| **Story** | STORY-068 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập. Người dùng không có quyền quản lý cấu hình giá thiệp.
- Cấu hình phụ phí thiệp viết tay tồn tại trong Core Database.

**Steps:**
1. Người dùng gửi yêu cầu cập nhật cấu hình phụ phí thiệp viết tay.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra dữ liệu cấu hình trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý cấu hình giá thiệp.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật dữ liệu.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi dữ liệu cấu hình.
- Hệ thống hiển thị thông báo phù hợp.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/EXC-07](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-245](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08765b97-4257-488b-a705-9383ae56056e)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý cấu hình giá thiệp mới được cập nhật cấu hình.

---

## ST-068-19-01 — Cập nhật dữ liệu tách biệt với chuyển trạng thái

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-19-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/491abdd7-00ca-40d9-83a2-fe0a41434c91) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Có cấu hình Active hoặc Inactive chưa bị xóa mềm.

**Steps:**
1. Ghi nhận trạng thái hiện tại của cấu hình.
2. Admin cập nhật khoảng số lượng từ hoặc giá phụ phí bằng dữ liệu hợp lệ.
3. Admin chọn lưu.
4. Kiểm tra trạng thái cấu hình trong Core Database sau cập nhật.

**Test Data:**
- Thực hiện với: Trường hợp 1: cấu hình Active. Trường hợp 2: cấu hình Inactive.

**Expected Result:**
- Hệ thống cập nhật thông tin cấu hình thành công.
- Trạng thái Active/Inactive hiện tại được giữ nguyên.
- Việc cập nhật dữ liệu không tự động chuyển trạng thái cấu hình.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [BR-243](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2)

**Rationale:**
> Xác minh chức năng cập nhật dữ liệu được tách biệt với chức năng chuyển trạng thái.

---

## ST-068-20-01 — Không làm thay đổi dữ liệu giá đã phát sinh trước đó

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-20-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4013a769-d495-4752-afac-b99dcc3851c4) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp.
- Cấu hình đã từng được sử dụng để xác định phụ phí cho thiệp, Checkout hoặc Order đã tồn tại.

**Steps:**
1. Ghi nhận giá và dữ liệu của thiệp, Checkout hoặc Order đã sử dụng cấu hình.
2. Admin cập nhật khoảng số lượng từ hoặc giá phụ phí của cấu hình.
3. Admin chọn lưu.
4. Kiểm tra lại các dữ liệu nghiệp vụ đã tồn tại.

**Test Data:**
- Một cấu hình đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp, Checkout, Order.

**Expected Result:**
- Thông tin cấu hình mới chỉ được sử dụng cho các lần xác định phụ phí mới.
- Hệ thống không tính lại giá của thiệp đã tồn tại.
- Hệ thống không thay đổi giá của Checkout đã tồn tại.
- Hệ thống không thay đổi giá của Order đã tồn tại.
- Dữ liệu lịch sử được giữ nguyên theo thông tin tại thời điểm phát sinh.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-068/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-244](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3a150e28-1a62-4b82-a8f3-c838238d674a)

**Rationale:**
> Xác minh cập nhật cấu hình không làm thay đổi dữ liệu giá đã phát sinh trước đó.
