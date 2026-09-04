# ST — STORY-044 — Admin xem danh sách lịch sử thiệp AI — System Tests

---

## ST-044-01-01 — Xem danh sách thiệp AI với đầy đủ thông tin và đúng thứ tự

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/713d3463-9f9a-4a61-b3de-371becaaa7ac) |
| **Story** | STORY-044 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền xem AI Custom → Thiệp.
- Có nhiều thiệp AI có ảnh output hợp lệ thuộc nhiều khách hàng và có ngày tạo khác nhau.

**Steps:**
1. Admin chọn AI Custom → Thiệp trên sidebar.
2. Admin quan sát danh sách thiệp.
3. Admin quan sát thông tin của từng item.
4. Admin quan sát thứ tự các thiệp theo ngày tạo.
5. Admin quan sát khu vực giá và bill.
6. Admin chọn mã thiệp hoặc **"Xem chi tiết"** của một item.

**Test Data:**
- Thiệp `CARD-00121`: ngày tạo `20/08/2026 09:00`.
- Thiệp `CARD-00122`: ngày tạo `21/08/2026 14:30`.
- Thiệp `CARD-00123`: ngày tạo `22/08/2026 08:15`.

**Expected Result:**
- Hệ thống hiển thị danh sách thiệp AI.
- Mỗi History record có ảnh output hợp lệ được hiển thị thành đúng 01 item.
- Mỗi item hiển thị Preview, mã thiệp, khách hàng, Template, Size, Hình thức, ngày tạo và thao tác.
- Danh sách được sắp xếp theo ngày tạo mới nhất trước: `CARD-00123`, `CARD-00122`, `CARD-00121`.
- Hệ thống không hiển thị giá thiệp hoặc bill snapshot trong danh sách.
- Khi chọn mã thiệp hoặc **"Xem chi tiết"**, hệ thống mở đúng Chi tiết thiệp tương ứng.

**Trace to:**
- [STORY-044/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [BR-126](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8b3abc1f-d5cc-4ea4-9217-38dfe0c52c35)
- [BR-127](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fc21993-cf12-4c58-bf64-db5e9b4f2763)
- [BR-128](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb1e1878-9fac-44b1-9b14-1137cc43bc0b)
- [BR-129](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3fda951f-01f8-40e1-8129-5e2ee16ea7f3)
- [BR-131](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/270060ac-9a31-4c28-b059-3879c464bace)

**Rationale:**
> Xác minh Admin xem được danh sách thiệp AI với đầy đủ thông tin cần thiết và đúng thứ tự mặc định.

---

## ST-044-02-01 — Tìm kiếm thiệp theo tên khách hàng, mã Order và mã thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cd09c20-5a19-42ce-9d2b-8e80ef4f03c2) |
| **Story** | STORY-044 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Có dữ liệu cho phép tìm kiếm theo tên khách hàng, mã Order và mã thiệp.
- Có ít nhất một điều kiện tìm kiếm cho kết quả nhiều hơn một trang.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin tìm kiếm theo tên khách hàng.
3. Admin quan sát kết quả.
4. Admin tìm kiếm theo mã Order.
5. Admin quan sát kết quả.
6. Admin tìm kiếm theo mã thiệp.
7. Admin quan sát kết quả.
8. Admin sử dụng một từ khóa có kết quả nhiều hơn một trang.
9. Admin chuyển sang trang 2.
10. Admin quan sát từ khóa tìm kiếm và danh sách kết quả.

**Test Data:**
- Tên khách hàng: `Nguyễn Văn A`.
- Mã Order: `ORD-00110`.
- Mã thiệp: `CARD-00125`.

**Expected Result:**
- Khi tìm theo tên khách hàng, hệ thống chỉ hiển thị các thiệp thuộc khách hàng phù hợp.
- Khi tìm theo mã Order, hệ thống chỉ hiển thị các thiệp liên quan đến Order phù hợp.
- Khi tìm theo mã thiệp, hệ thống hiển thị thiệp phù hợp.
- Hệ thống không hiển thị các thiệp không thỏa điều kiện tìm kiếm.
- Khi chuyển trang, từ khóa tìm kiếm hiện tại vẫn được giữ nguyên.
- Dữ liệu trên trang mới tiếp tục đáp ứng cùng điều kiện tìm kiếm.

**Trace to:**
- [STORY-044/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh Admin có thể tra cứu thiệp theo các tiêu chí được hỗ trợ và điều kiện tìm kiếm không bị mất khi chuyển trang.

---

## ST-044-03-01 — Lọc danh sách thiệp theo Size và Hình thức

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/693e7ef3-6957-4e42-af19-5e4f473eca89) |
| **Story** | STORY-044 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Danh sách có thiệp thuộc nhiều Size và cả hai Hình thức Calligraphy, Gõ máy.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin mở bộ lọc.
3. Admin chọn một Size.
4. Admin áp dụng bộ lọc.
5. Admin quan sát kết quả.
6. Admin chọn thêm Hình thức.
7. Admin áp dụng đồng thời hai điều kiện.
8. Admin quan sát kết quả.
9. Admin xóa một điều kiện lọc.
10. Admin quan sát kết quả.
11. Admin đặt lại toàn bộ bộ lọc.

**Test Data:**
- Size: `10×15`.
- Hình thức: `Calligraphy`.

**Expected Result:**
- Khi lọc theo Size `10×15`, hệ thống chỉ hiển thị các thiệp có Size `10×15`.
- Khi chọn thêm `Calligraphy`, hệ thống chỉ hiển thị các thiệp đồng thời thỏa Size `10×15` và Hình thức `Calligraphy`.
- Khi xóa một điều kiện, hệ thống giữ và áp dụng các điều kiện còn lại.
- Khi đặt lại toàn bộ bộ lọc, danh sách trở về trạng thái không áp dụng bộ lọc.
- Hệ thống không hiển thị item không thỏa các điều kiện lọc đang áp dụng.

**Trace to:**
- [STORY-044/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh Admin có thể lọc danh sách theo một hoặc nhiều điều kiện và chủ động xóa hoặc đặt lại bộ lọc.

---

## ST-044-04-01 — Thay đổi số item trên trang và cập nhật phân trang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/352918e2-7a9b-46fc-bbcd-69252dd231d2) |
| **Story** | STORY-044 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Danh sách có nhiều hơn 50 thiệp để quan sát các lựa chọn số item trên trang.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin quan sát số item mặc định trên trang.
3. Admin chọn 5 item/trang.
4. Admin quan sát danh sách và phân trang.
5. Admin chuyển sang một trang khác.
6. Admin chọn 20 item/trang.
7. Admin quan sát trang hiện tại và số item hiển thị.
8. Admin lần lượt kiểm tra các lựa chọn 10, 30, 40 và 50 item/trang.

**Test Data:**
- Các giá trị item/trang: `5`, `10`, `20`, `30`, `40`, `50`.

**Expected Result:**
- Mặc định danh sách hiển thị tối đa 10 item/trang.
- Khi chọn một giá trị khác, số item hiển thị trên trang được cập nhật theo lựa chọn.
- Phân trang được tính lại phù hợp với số item/trang.
- Khi thay đổi số item/trang, hệ thống đưa Admin về trang đầu tiên.
- Các giá trị `5`, `10`, `20`, `30`, `40` và `50` đều có thể được lựa chọn.

**Trace to:**
- [STORY-044/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh Admin có thể thay đổi số lượng thiệp hiển thị trên một trang và hệ thống cập nhật phân trang đúng.

---

## ST-044-05-01 — Thiệp liên kết nhiều Order chỉ xuất hiện một item

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9c8477e0-b95d-4172-9f6a-6275f7d14044) |
| **Story** | STORY-044 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Có một thiệp đang liên kết với từ 2 Order trở lên.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin tìm kiếm thiệp cần kiểm tra.
3. Admin quan sát số lần thiệp xuất hiện trong danh sách.

**Test Data:**
- Mã thiệp: `CARD-00130`.
- Order liên kết:
  - `ORD-00120`.
  - `ORD-00121`.
  - `ORD-00125`.

**Expected Result:**
- `CARD-00130` chỉ xuất hiện đúng 01 item trong danh sách.
- Hệ thống không tạo thêm item cho từng Order liên kết.
- Việc thiệp liên kết nhiều Order không làm thay đổi số lượng History item của thiệp.

**Trace to:**
- [STORY-044/ALT-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [BR-126](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8b3abc1f-d5cc-4ea4-9217-38dfe0c52c35)

**Rationale:**
> Xác minh mỗi History record thiệp chỉ tương ứng với một item trong danh sách, không phụ thuộc số Order liên kết.

---

## ST-044-06-01 — Xem Preview thiệp bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8dd9586b-0ea7-4588-bc19-53bdaadecd13) |
| **Story** | STORY-044 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Có ít nhất một thiệp có Preview ảnh còn khả dụng.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin chọn Preview của một thiệp.
3. Admin quan sát ảnh trong Image Lightbox.
4. Admin đóng Image Lightbox.
5. Admin quan sát vị trí trong danh sách sau khi đóng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống mở Image Lightbox khi Admin chọn Preview.
- Ảnh được hiển thị đúng tỷ lệ và không bị méo.
- Admin có thể đóng Image Lightbox.
- Sau khi đóng, Admin quay lại danh sách tại vị trí trước đó.

**Trace to:**
- [STORY-044/ALT-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh Admin có thể xem nhanh ảnh thiệp mà không cần rời khỏi danh sách.

---

## ST-044-07-01 — Empty state và no-result state của danh sách thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44c9d87c-8acf-4fb8-a108-f25e2dd15870) |
| **Story** | STORY-044 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Có thể kiểm tra trường hợp danh sách chưa có thiệp hoặc áp dụng điều kiện tra cứu không có kết quả.

**Steps:**
1. Admin mở AI Custom → Thiệp trong trường hợp không có dữ liệu.
2. Admin quan sát khu vực danh sách.
3. Trong trường hợp danh sách có dữ liệu, Admin nhập từ khóa không có kết quả.
4. Admin quan sát khu vực danh sách.
5. Admin áp dụng bộ lọc không có kết quả phù hợp.
6. Admin quan sát trạng thái hiển thị.

**Test Data:**
- Từ khóa không tồn tại: `CARD-NOT-FOUND-999`.
- Bộ lọc: một tổ hợp Size và Hình thức không có dữ liệu phù hợp trong môi trường test.

**Expected Result:**
- Khi chưa có thiệp, hệ thống hiển thị empty state.
- Khi tìm kiếm không có kết quả, hệ thống hiển thị trạng thái không có kết quả phù hợp.
- Khi bộ lọc không có kết quả, hệ thống hiển thị trạng thái không có kết quả phù hợp.
- Hệ thống không hiển thị dữ liệu cũ như thể vẫn đáp ứng điều kiện tìm kiếm hoặc lọc.

**Trace to:**
- [STORY-044/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh danh sách phản hồi rõ ràng khi chưa có dữ liệu hoặc không có thiệp phù hợp với điều kiện tra cứu.

---

## ST-044-08-01 — Error state khi danh sách thiệp không tải được

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0d08956d-d695-4174-81c6-13bb07ad87aa) |
| **Story** | STORY-044 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Có thể mô phỏng tình trạng danh sách thiệp không tải được và sau đó khôi phục về bình thường.

**Steps:**
1. Tạo tình huống danh sách thiệp tải lỗi.
2. Admin mở AI Custom → Thiệp.
3. Admin quan sát khu vực danh sách.
4. Khôi phục tình trạng tải dữ liệu về bình thường.
5. Admin chọn thao tác tải lại.
6. Admin quan sát danh sách sau khi tải lại.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống hiển thị error state khi danh sách không tải được.
- Hệ thống không hiển thị dữ liệu thiếu như một danh sách hoàn chỉnh.
- Có thao tác cho phép Admin tải lại.
- Sau khi tình trạng được khôi phục và Admin tải lại, danh sách được hiển thị thành công.

**Trace to:**
- [STORY-044/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

**Rationale:**
> Xác minh Admin có thể nhận biết lỗi tải danh sách và tiếp tục sử dụng chức năng sau khi lỗi tạm thời được khắc phục.

---

## ST-044-09-01 — Lỗi Preview ảnh không làm hỏng toàn bộ danh sách

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/20054ec0-be9b-47ac-a95c-0682e53dbd5e) |
| **Story** | STORY-044 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Thiệp.
- Danh sách có:
  - Một thiệp có file ảnh không còn khả dụng.
  - Một thiệp có ảnh tải lỗi tạm thời.
  - Một số thiệp có ảnh hiển thị bình thường.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin quan sát thiệp có file ảnh không còn khả dụng.
3. Admin quan sát thiệp có ảnh tải lỗi tạm thời.
4. Admin quan sát các item còn lại trong danh sách.
5. Admin thực hiện thao tác tải lại đối với trường hợp ảnh lỗi tạm thời nếu có.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Thiệp có file ảnh không còn khả dụng vẫn xuất hiện trong danh sách.
- Preview của thiệp đó được thay bằng placeholder.
- Thiệp có ảnh tải lỗi tạm thời chỉ hiển thị placeholder tại Preview tương ứng.
- Lỗi của một ảnh không làm hỏng hoặc gián đoạn toàn bộ danh sách.
- Các thiệp còn lại vẫn hiển thị thông tin và Preview bình thường.
- Hệ thống cho phép Admin tải lại trang hoặc tải lại ảnh tương ứng đối với lỗi tạm thời.
- Hệ thống không xuất hiện ảnh mới do hệ thống tự tạo lại để thay thế ảnh không còn khả dụng.

**Trace to:**
- [STORY-044/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [BR-130](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2c5fddec-4c32-49d6-abe8-477073969fd6)

**Rationale:**
> Xác minh lỗi của file ảnh chỉ ảnh hưởng đến Preview tương ứng và không làm mất History record hoặc ảnh hưởng toàn bộ danh sách.

---

## ST-044-10-01 — Chặn tài khoản không có quyền xem danh sách thiệp AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-044-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/861741e6-1703-47b8-92e6-b57044f76ecb) |
| **Story** | STORY-044 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một tài khoản không có quyền xem AI Custom → Thiệp.

**Steps:**
1. Đăng nhập bằng tài khoản không có quyền.
2. Quan sát nhóm AI Custom trên sidebar.
3. Thử truy cập trực tiếp vào trang danh sách thiệp.
4. Quan sát nội dung được hiển thị.

**Test Data:**
- Tài khoản: `staff_no_ai_permission`.

**Expected Result:**
- Mục Thiệp không được hiển thị cho tài khoản không có quyền tương ứng.
- Người dùng không thể xem danh sách thiệp khi truy cập trực tiếp.
- Hệ thống không hiển thị Preview ảnh, thông tin khách hàng hoặc dữ liệu thiệp cho người dùng không có quyền.
- Hệ thống không hiển thị dữ liệu cũ hoặc dữ liệu của một phiên đăng nhập trước đó.

**Trace to:**
- [STORY-044/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [STORY-044/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)
- [BR-132](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577)
- [BR-131](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/270060ac-9a31-4c28-b059-3879c464bace)

**Rationale:**
> Xác minh danh sách thiệp AI chỉ được truy cập bởi tài khoản có quyền tương ứng.
