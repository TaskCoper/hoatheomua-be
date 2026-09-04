# ST — STORY-054 — Admin xem danh sách kích thước thiệp — System Tests

---

## ST-054-01-01 — Xem danh sách kích thước thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de029eb9-e49a-4206-838f-4edba6b66e8c) |
| **Story** | STORY-054 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp.
- Core Database có nhiều kích thước thiệp với `isDelete = false`, gồm cả Active và Inactive.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Quan sát danh sách hiển thị.
3. Kiểm tra từng dòng dữ liệu.
4. Kiểm tra thao tác tương ứng theo trạng thái.

**Test Data:**
- Size Active: `5x5`, giá `20.000`, số lượng từ tối đa `35`.
- Size Inactive: `7x7`, giá `30.000`, số lượng từ tối đa `50`.

**Expected Result:**
- Hệ thống hiển thị danh sách kích thước thiệp chưa bị xóa mềm.
- Mỗi dòng hiển thị kích thước, giá size, số lượng từ tối đa, trạng thái Active/Inactive.
- Size Active hiển thị thao tác chuyển sang Inactive.
- Size Inactive hiển thị thao tác chuyển sang Active.
- Hiển thị entry point “Thêm kích thước thiệp”.

**Trace to:**
- [STORY-054/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [BR-187](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f1dbd3f-73c8-45de-a685-339c6ae46fbb)
- [BR-188](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/18bfed12-4045-4071-a071-eba05b86e317)

**Rationale:**
> Kiểm tra luồng chính xem danh sách và thông tin hiển thị.

---

## ST-054-02-01 — Dữ liệu xóa mềm không xuất hiện trong danh sách

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4b99b04c-0088-468d-a4ba-22d8855756fd) |
| **Story** | STORY-054 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Core Database có kích thước thiệp đã bị xóa mềm.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Quan sát danh sách mặc định.

**Test Data:**
- Size `10x10` có `isDelete = true`.

**Expected Result:**
- Hệ thống không hiển thị kích thước thiệp có `isDelete = true` trong danh sách mặc định.

**Trace to:**
- [STORY-054/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [BR-189](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b)

**Rationale:**
> Đảm bảo dữ liệu xóa mềm không xuất hiện trong danh sách quản lý mặc định.

---

## ST-054-03-01 — Trạng thái danh sách rỗng (Empty state)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d929d39f-6ab7-49df-8aa2-b729249c45de) |
| **Story** | STORY-054 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Core Database không có kích thước thiệp nào có `isDelete = false`.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Quan sát trạng thái màn hình.

**Test Data:**
- Tất cả size đều `isDelete = true` hoặc không có size nào trong hệ thống.

**Expected Result:**
- Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
- Không hiển thị thông báo lỗi tải dữ liệu.
- Vẫn hiển thị entry point “Thêm kích thước thiệp”.

**Trace to:**
- [STORY-054/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [BR-189](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b)

**Rationale:**
> Kiểm tra trạng thái rỗng là dữ liệu hợp lệ, không phải lỗi tải dữ liệu.

---

## ST-054-04-01 — Phân trang danh sách kích thước thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/69119482-908c-4763-bb16-cf9b91df919c) |
| **Story** | STORY-054 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Danh sách có số lượng bản ghi vượt quá số lượng hiển thị trên một trang.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Admin xem trang đầu tiên của danh sách.
3. Admin chuyển sang trang tiếp theo.
4. Admin quay lại trang trước đó.

**Test Data:**
- Có hơn 10 kích thước thiệp có `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị chức năng phân trang.
- Mỗi trang chỉ hiển thị các bản ghi thuộc trang tương ứng.
- Admin có thể chuyển giữa các trang để xem các bản ghi còn lại.

**Trace to:**
- [STORY-054/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

**Rationale:**
> Kiểm tra danh sách hoạt động đúng khi dữ liệu nhiều.

---

## ST-054-05-01 — Lọc danh sách theo trạng thái Active / Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e85e8f3-e7da-4bed-a2c4-243fbe666335) |
| **Story** | STORY-054 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Core Database có size Active, Inactive và size đã xóa mềm.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Chọn bộ lọc Active.
3. Chọn bộ lọc Inactive.
4. Chọn bộ lọc “Tất cả”.

**Test Data:**
- Size Active có `isActive = true` và `isDelete = false`.
- Size Inactive có `isActive = false` và `isDelete = false`.
- Size deleted có `isDelete = true`.

**Expected Result:**
- Khi lọc Active, chỉ hiển thị size có `isActive = true` và `isDelete = false`.
- Khi lọc Inactive, chỉ hiển thị size có `isActive = false` và `isDelete = false`.
- Khi chọn “Tất cả”, hiển thị tất cả size có `isDelete = false`.
- Không hiển thị size đã xóa mềm ở mọi bộ lọc.

**Trace to:**
- [STORY-054/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [BR-189](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b)

**Rationale:**
> Kiểm tra bộ lọc trạng thái luôn loại trừ dữ liệu xóa mềm.

---

## ST-054-06-01 — Không có kết quả khi lọc (No-result state)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3c7099af-5c59-4ca2-a3c2-adf4f0bbaf69) |
| **Story** | STORY-054 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Không có kích thước thiệp nào thỏa điều kiện lọc đã chọn.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Chọn bộ lọc trạng thái không có dữ liệu phù hợp.

**Test Data:**
- Chỉ có size Active, Admin lọc Inactive.

**Expected Result:**
- Hệ thống hoàn tất tải dữ liệu.
- Hiển thị trạng thái không có kết quả phù hợp.
- Không hiển thị thông báo lỗi tải dữ liệu.

**Trace to:**
- [STORY-054/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

**Rationale:**
> Phân biệt trạng thái không có kết quả với lỗi tải dữ liệu.

---

## ST-054-07-01 — Giữ nguyên bộ lọc khi phân trang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9ec85ad-67b3-437a-a422-10731039d09f) |
| **Story** | STORY-054 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Danh sách sau khi lọc có nhiều trang.

**Steps:**
1. Admin truy cập danh sách kích thước thiệp.
2. Chọn bộ lọc Active hoặc Inactive.
3. Chuyển sang trang khác.

**Test Data:**
- Có hơn 10 size Active hoặc hơn 10 size Inactive.

**Expected Result:**
- Hệ thống giữ nguyên trạng thái lọc đã chọn khi chuyển trang.
- Chỉ hiển thị dữ liệu của trang tương ứng thỏa điều kiện lọc.

**Trace to:**
- [STORY-054/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

**Rationale:**
> Đảm bảo phân trang không làm mất điều kiện lọc.

---

## ST-054-08-01 — Chặn Admin không có quyền quản lý cấu hình thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1af0c4a0-0f16-44a7-aac6-f4914f515c20) |
| **Story** | STORY-054 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động.
- Admin không có quyền quản lý cấu hình thiệp.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Quan sát phản hồi của hệ thống.

**Test Data:**
- Admin thiếu quyền quản lý cấu hình thiệp.

**Expected Result:**
- Hệ thống không hiển thị danh sách kích thước thiệp.
- Hệ thống không trả dữ liệu quản trị kích thước thiệp.
- Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

**Trace to:**
- [STORY-054/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

**Rationale:**
> Kiểm tra phân quyền truy cập danh sách.

---

## ST-054-09-01 — Lỗi truy xuất danh sách và chức năng thử lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-054-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff2d0730-1c37-4917-bab5-df5c72ab9516) |
| **Story** | STORY-054 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền quản lý cấu hình thiệp.
- Core Database hoặc API danh sách kích thước thiệp gặp lỗi.

**Steps:**
1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Hệ thống không thể truy xuất dữ liệu.
3. Admin chọn “Thử lại”.

**Test Data:**
- Giả lập lỗi truy xuất Core Database lần đầu, lần thử lại thành công hoặc tiếp tục lỗi.

**Expected Result:**
- Khi không thể tải dữ liệu, hệ thống hiển thị thông báo lỗi phù hợp.
- Không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
- Hệ thống cung cấp nút “Thử lại”.
- Khi Admin chọn “Thử lại”, hệ thống thực hiện lại việc truy xuất danh sách.

**Trace to:**
- [STORY-054/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [STORY-054/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [BR-187](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f1dbd3f-73c8-45de-a685-339c6ae46fbb)

**Rationale:**
> Kiểm tra lỗi tải dữ liệu và khả năng retry.
