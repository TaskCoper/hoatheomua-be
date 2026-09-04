# ST — STORY-047 — Admin xem danh sách mẫu hoa Custom AI — System Tests

---

## ST-047-01-01 — Xem danh sách mẫu hoa Custom AI với đúng thông tin và thứ tự

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a5f3e050-7cbd-4eb3-9504-5b586ce8b744) |
| **Story** | STORY-047 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền xem AI Custom → Mẫu hoa.
- Có nhiều kết quả mẫu hoa Custom AI có ảnh output hợp lệ và ngày tạo khác nhau.

**Steps:**
1. Admin chọn AI Custom → Mẫu hoa trên sidebar.
2. Admin quan sát danh sách mẫu hoa.
3. Admin quan sát thông tin hiển thị trên từng item.
4. Admin quan sát thứ tự các mẫu hoa.
5. Admin quan sát khu vực giá và bill.
6. Admin chọn mã mẫu hoa hoặc **"Xem chi tiết"**.

**Test Data:**
- Mẫu `FLOWER-00121`: tạo lúc `20/08/2026 09:00`.
- Mẫu `FLOWER-00122`: tạo lúc `21/08/2026 14:30`.
- Mẫu `FLOWER-00123`: tạo lúc `22/08/2026 08:15`.

**Expected Result:**
- Hệ thống hiển thị danh sách mẫu hoa Custom AI.
- Mỗi kết quả có ảnh output hợp lệ được hiển thị thành đúng 01 item.
- Mỗi item hiển thị Preview, mã mẫu hoa, khách hàng, Combo nguồn, ngày tạo và thao tác.
- Danh sách được sắp xếp theo ngày tạo mới nhất trước.
- Thứ tự hiển thị là `FLOWER-00123`, `FLOWER-00122`, `FLOWER-00121`.
- Hệ thống không hiển thị giá hoặc bill snapshot trên danh sách.
- Khi chọn mã mẫu hoa hoặc **"Xem chi tiết"**, hệ thống mở đúng Chi tiết mẫu hoa tương ứng.

**Trace to:**
- [STORY-047/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [BR-144](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40c4ad7-0723-43ab-ba2f-c52bf73b3cf1)
- [BR-145](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96489393-fd4e-4d3c-895a-5a0fd8d27bcd)
- [BR-146](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c57c1f24-ded5-4593-9b67-b049dedb4a4b)
- [BR-150](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312)
- [BR-152](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e98ce2c8-f7f8-4ff9-aae4-4cf51ab5296b)

**Rationale:**
> Xác minh Admin xem được danh sách mẫu hoa Custom AI với đúng thông tin và thứ tự mặc định.

---

## ST-047-02-01 — Tìm kiếm mẫu hoa theo tên khách hàng, mã Order và mã mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9b03401-1ec5-4e15-9b44-eb447d07d0dc) |
| **Story** | STORY-047 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có các mẫu hoa phù hợp để tra cứu theo tên khách hàng, mã Order và mã mẫu hoa.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin tìm kiếm theo tên khách hàng.
3. Admin quan sát kết quả.
4. Admin tìm kiếm theo mã Order.
5. Admin quan sát kết quả.
6. Admin tìm kiếm theo mã mẫu hoa.
7. Admin quan sát kết quả.

**Test Data:**
- Tên khách hàng: `Nguyễn Văn A`.
- Mã Order: `ORD-00110`.
- Mã mẫu hoa: `FLOWER-00125`.

**Expected Result:**
- Khi tìm theo tên khách hàng, hệ thống chỉ hiển thị các mẫu hoa phù hợp với khách hàng đó.
- Khi tìm theo mã Order, hệ thống chỉ hiển thị các mẫu hoa có liên kết với Order tương ứng.
- Khi tìm theo mã mẫu hoa, hệ thống hiển thị mẫu hoa phù hợp.
- Hệ thống không hiển thị các mẫu hoa không thỏa điều kiện tìm kiếm.

**Trace to:**
- [STORY-047/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh Admin có thể tìm kiếm mẫu hoa theo các tiêu chí được hỗ trợ.

---

## ST-047-03-01 — Lọc danh sách mẫu hoa theo một hoặc nhiều Combo

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eb79252d-ecf5-43c8-bf2a-fa3f8294ca20) |
| **Story** | STORY-047 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có các mẫu hoa được tạo từ nhiều Combo khác nhau.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin mở bộ lọc Combo.
3. Admin chọn một Combo.
4. Admin áp dụng bộ lọc.
5. Admin quan sát kết quả.
6. Admin chọn thêm một Combo khác.
7. Admin quan sát kết quả.
8. Admin xóa một điều kiện Combo.
9. Admin quan sát kết quả.
10. Admin đặt lại toàn bộ bộ lọc.

**Test Data:**
- Combo A: `COMBO-001`.
- Combo B: `COMBO-002`.

**Expected Result:**
- Khi chọn `COMBO-001`, hệ thống chỉ hiển thị các mẫu hoa được tạo từ Combo này.
- Khi chọn thêm `COMBO-002`, hệ thống hiển thị các mẫu hoa được tạo từ một trong các Combo đã chọn.
- Hệ thống không hiển thị mẫu hoa thuộc Combo ngoài điều kiện đang áp dụng.
- Khi xóa một điều kiện, danh sách được cập nhật theo các Combo còn được chọn.
- Khi đặt lại bộ lọc, danh sách trở về trạng thái không áp dụng bộ lọc Combo.

**Trace to:**
- [STORY-047/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh Admin có thể lọc danh sách theo một hoặc nhiều Combo và thay đổi điều kiện lọc.

---

## ST-047-04-01 — Thay đổi số item trên trang và cập nhật phân trang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/93b26eca-48b5-4f18-9019-86df67532c09) |
| **Story** | STORY-047 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có đủ mẫu hoa để danh sách hiển thị nhiều hơn 50 item.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin quan sát số item mặc định trên trang.
3. Admin chọn 5 item/trang.
4. Admin quan sát danh sách và phân trang.
5. Admin chuyển sang trang khác.
6. Admin chọn 20 item/trang.
7. Admin quan sát trang hiện tại.
8. Admin lần lượt kiểm tra các lựa chọn 10, 30, 40 và 50 item/trang.

**Test Data:**
- Các giá trị item/trang: `5`, `10`, `20`, `30`, `40`, `50`.

**Expected Result:**
- Mặc định danh sách hiển thị tối đa 10 item/trang.
- Khi thay đổi số item/trang, danh sách hiển thị theo số lượng được chọn.
- Phân trang được cập nhật tương ứng.
- Khi thay đổi số item/trang, Admin được đưa về trang đầu tiên.
- Các giá trị `5`, `10`, `20`, `30`, `40` và `50` đều có thể được lựa chọn.

**Trace to:**
- [STORY-047/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh Admin có thể thay đổi số lượng mẫu hoa hiển thị trên mỗi trang và hệ thống xử lý phân trang đúng.

---

## ST-047-05-01 — Mẫu hoa liên kết nhiều Order chỉ xuất hiện một item

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7229cb3-85c0-4cee-b0e9-f4ea1725aca2) |
| **Story** | STORY-047 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có một mẫu hoa đang liên kết với từ 2 Order trở lên.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin tìm mẫu hoa cần kiểm tra.
3. Admin quan sát số lần mẫu hoa xuất hiện trong danh sách.
4. Admin mở Chi tiết mẫu hoa.
5. Admin quan sát các Order liên quan.

**Test Data:**
- Mã mẫu hoa: `FLOWER-00130`.
- Order liên kết:
  - `ORD-00120`.
  - `ORD-00121`.
  - `ORD-00125`.

**Expected Result:**
- `FLOWER-00130` chỉ xuất hiện đúng 01 item trong danh sách.
- Hệ thống không nhân bản item theo số lượng Order liên kết.
- Khi mở Chi tiết mẫu hoa, Admin có thể xem các Order liên quan.

**Trace to:**
- [STORY-047/ALT-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [BR-144](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40c4ad7-0723-43ab-ba2f-c52bf73b3cf1)

**Rationale:**
> Xác minh một kết quả mẫu hoa chỉ được biểu diễn bằng một item dù đã liên kết với nhiều Order.

---

## ST-047-06-01 — Xem Preview mẫu hoa bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ee1ca3af-f78e-485e-b05e-2a79ea649bdd) |
| **Story** | STORY-047 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có ít nhất một mẫu hoa có Preview ảnh còn khả dụng.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin chọn Preview của một mẫu hoa.
3. Admin quan sát ảnh trong Image Lightbox.
4. Admin đóng Image Lightbox.
5. Admin quan sát vị trí danh sách sau khi đóng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống mở Image Lightbox khi Admin chọn Preview.
- Ảnh được hiển thị đúng tỷ lệ.
- Admin có thể đóng Image Lightbox.
- Sau khi đóng, Admin quay lại danh sách tại vị trí trước đó.

**Trace to:**
- [STORY-047/ALT-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh Admin có thể xem nhanh ảnh mẫu hoa mà không cần rời khỏi danh sách.

---

## ST-047-07-01 — Empty state và no-result state của danh sách mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d0d1d-6c88-46f0-8e49-036ef4ad1039) |
| **Story** | STORY-047 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có thể kiểm tra trường hợp danh sách chưa có mẫu hoa hoặc điều kiện tra cứu không có kết quả phù hợp.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa trong trường hợp không có dữ liệu.
2. Admin quan sát khu vực danh sách.
3. Trong trường hợp danh sách có dữ liệu, Admin nhập điều kiện tìm kiếm không có kết quả.
4. Admin quan sát trạng thái hiển thị.
5. Admin áp dụng bộ lọc không có kết quả phù hợp.
6. Admin quan sát trạng thái hiển thị.
7. Admin thay đổi hoặc đặt lại điều kiện tra cứu.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Khi chưa có mẫu hoa, hệ thống hiển thị empty state.
- Khi tìm kiếm không có kết quả, hệ thống hiển thị trạng thái không có kết quả phù hợp.
- Khi bộ lọc không có kết quả, hệ thống hiển thị trạng thái không có kết quả phù hợp.
- Hệ thống không hiển thị dữ liệu của điều kiện trước đó như kết quả hiện tại.
- Sau khi Admin thay đổi hoặc đặt lại điều kiện, hệ thống cho phép tải lại danh sách phù hợp.

**Trace to:**
- [STORY-047/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh hệ thống phản hồi đúng khi không có mẫu hoa hoặc không có kết quả phù hợp với điều kiện tra cứu.

---

## ST-047-08-01 — Error state khi danh sách mẫu hoa không tải được

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4dceaa70-8d73-43a6-83ec-7410b79dfa36) |
| **Story** | STORY-047 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có thể mô phỏng tình trạng không tải được danh sách bằng cách ngắt kết nối mạng hoặc đặt trình duyệt ở chế độ Offline.
- Sau lần lỗi đầu tiên, có thể khôi phục kết nối để tải lại danh sách.

**Steps:**
1. Đặt trình duyệt ở chế độ Offline hoặc ngắt kết nối mạng.
2. Admin mở AI Custom → Mẫu hoa.
3. Admin quan sát khu vực danh sách.
4. Khôi phục kết nối mạng.
5. Admin chọn thao tác tải lại.
6. Admin quan sát danh sách sau khi tải lại.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống hiển thị error state khi không tải được danh sách.
- Hệ thống không hiển thị dữ liệu thiếu hoặc dữ liệu cũ như một danh sách hoàn chỉnh.
- Có thao tác cho phép Admin tải lại.
- Sau khi kết nối được khôi phục và Admin tải lại, danh sách được hiển thị thành công.
- Nếu vẫn không tải được dữ liệu, hệ thống tiếp tục giữ error state.

**Trace to:**
- [STORY-047/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)

**Rationale:**
> Xác minh Admin nhận biết được lỗi tải danh sách và có thể tiếp tục sử dụng chức năng sau khi kết nối được khôi phục.

---

## ST-047-09-01 — Lỗi tải ảnh chỉ ảnh hưởng Preview tương ứng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/45c76c98-4f71-4064-9204-03c6426e1c5e) |
| **Story** | STORY-047 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có ít nhất một mẫu hoa có ảnh không tải được.
- Có các mẫu hoa khác có ảnh hiển thị bình thường.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin quan sát item có ảnh không tải được.
3. Admin quan sát thông tin còn lại của item.
4. Admin quan sát các item khác trong danh sách.
5. Admin tải lại trang.
6. Admin quan sát lại danh sách.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Item có ảnh không tải được vẫn tồn tại trong danh sách.
- Hệ thống chỉ hiển thị trạng thái lỗi tại vùng Preview của item tương ứng.
- Hệ thống không hiển thị file lỗi như một ảnh hợp lệ.
- Các thông tin còn lại của mẫu hoa vẫn được giữ.
- Các item khác vẫn hiển thị bình thường nếu ảnh và dữ liệu của chúng tải được.
- Một ảnh lỗi không làm hỏng toàn bộ danh sách.
- Admin có thể tải lại trang để thử tải lại ảnh.
- Hệ thống không tự động tạo ảnh mới thay thế.
- Việc ảnh tải lỗi không làm thay đổi mẫu hoa, History record hoặc Order liên kết.

**Trace to:**
- [STORY-047/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [BR-151](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f)

**Rationale:**
> Xác minh lỗi tải một ảnh chỉ ảnh hưởng đến Preview tương ứng và không làm gián đoạn toàn bộ danh sách mẫu hoa.

---

## ST-047-10-01 — Chặn tài khoản không có quyền xem danh sách mẫu hoa Custom AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-047-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4a9b36a9-cfe7-49d2-ae27-21d810406085) |
| **Story** | STORY-047 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một tài khoản Website quản trị không có quyền xem AI Custom → Mẫu hoa.

**Steps:**
1. Đăng nhập bằng tài khoản không có quyền.
2. Quan sát nhóm AI Custom trên sidebar.
3. Thử truy cập trực tiếp trang danh sách mẫu hoa.
4. Quan sát nội dung được hiển thị.

**Test Data:**
- Tài khoản: `staff_no_ai_permission`.

**Expected Result:**
- Mục Mẫu hoa không được hiển thị trên sidebar cho tài khoản không có quyền tương ứng.
- Người dùng không thể xem danh sách mẫu hoa khi truy cập trực tiếp.
- Hệ thống hiển thị trạng thái không có quyền truy cập.
- Hệ thống không hiển thị Preview ảnh, thông tin khách hàng, Combo hoặc dữ liệu mẫu hoa.
- Hệ thống không hiển thị dữ liệu danh sách đã tải từ phiên hoặc trạng thái trước đó.

**Trace to:**
- [STORY-047/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [STORY-047/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ae2c448a-a920-4bc5-9669-3d02e9121223)
- [BR-132](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577)
- [BR-152](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e98ce2c8-f7f8-4ff9-aae4-4cf51ab5296b)

**Rationale:**
> Xác minh danh sách mẫu hoa Custom AI chỉ được truy cập bởi tài khoản có quyền tương ứng.
