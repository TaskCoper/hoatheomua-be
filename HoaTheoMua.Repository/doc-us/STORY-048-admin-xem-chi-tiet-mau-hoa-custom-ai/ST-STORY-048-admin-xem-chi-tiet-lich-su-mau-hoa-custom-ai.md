# ST — STORY-048 — Admin xem chi tiết lịch sử mẫu hoa Custom AI — System Tests

---

## ST-048-01-01 — Xem chi tiết mẫu hoa Custom AI đầy đủ thông tin

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/28f3d9f3-2622-4270-9f1a-2163b496929f) |
| **Story** | STORY-048 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền xem AI Custom → Mẫu hoa.
- Có một mẫu hoa Custom AI tồn tại và dữ liệu chi tiết còn khả dụng.

**Steps:**
1. Admin mở AI Custom → Mẫu hoa.
2. Admin chọn mã mẫu hoa hoặc **"Xem chi tiết"**.
3. Admin quan sát ảnh kết quả và mã mẫu hoa.
4. Admin quan sát thông tin khách hàng.
5. Admin quan sát thông tin Combo nguồn.
6. Admin quan sát danh sách Core và số lượng.
7. Admin quan sát danh sách Support và số lượng.
8. Admin quan sát Mockup nguồn và yêu cầu tùy chỉnh.
9. Admin quan sát ngày tạo, trạng thái file và danh sách Order liên kết.
10. Admin quan sát khu vực giá và bill.

**Test Data:**
- Mã mẫu hoa: `FLOWER-00125`.
- Khách hàng: `Nguyễn Văn A`.
- Số điện thoại: `0901234567`.
- Email: `nguyenvana@example.com`.
- Combo nguồn: `COMBO-001 – Set hoa mùa xuân`.
- Core:
  - `Hoa hồng`: `3`.
  - `Hoa cúc`: `2`.
- Support:
  - `Baby`: `5`.
- Mockup nguồn: `MOCKUP-001`.
- Yêu cầu tùy chỉnh: `Tone màu nhẹ, bố cục tròn`.
- Order liên kết: `ORD-00110`.

**Expected Result:**
- Hệ thống mở đúng Chi tiết `FLOWER-00125`.
- Hệ thống hiển thị ảnh kết quả và đúng mã mẫu hoa.
- Hệ thống hiển thị đúng họ tên, số điện thoại và email của khách hàng.
- Hệ thống hiển thị đúng mã, tên và thông tin Combo nguồn.
- Hệ thống hiển thị đầy đủ thành phần Core cùng số lượng tương ứng.
- Hệ thống hiển thị đầy đủ thành phần Support cùng số lượng tương ứng.
- Hệ thống hiển thị đúng Mockup nguồn.
- Hệ thống hiển thị đúng yêu cầu hoặc ghi chú tùy chỉnh.
- Hệ thống hiển thị ngày tạo, trạng thái file và các Order liên quan.
- Hệ thống không hiển thị giá hoặc bill snapshot tại Chi tiết mẫu hoa.

**Trace to:**
- [STORY-048/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [BR-147](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d7ba22e1-39f3-4c7a-8033-fd13c480478a)
- [BR-148](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71698060-d5a9-4f17-8009-8dfe848db5b9)
- [BR-149](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96d99b5e-1045-4ec3-a8a8-4f91cd38e2ff)
- [BR-150](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312)

**Rationale:**
> Xác minh Admin có thể xem đầy đủ thông tin của một kết quả mẫu hoa Custom AI tại trang Chi tiết mẫu hoa.

---

## ST-048-03-01 — Mẫu hoa liên kết nhiều Order hiển thị đầy đủ và mở đúng Order

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00ed89e6-c65c-46ab-bbec-32956f9b6476) |
| **Story** | STORY-048 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có một mẫu hoa đang liên kết với từ 2 Order trở lên.

**Steps:**
1. Admin mở Chi tiết mẫu hoa.
2. Admin quan sát danh sách Order liên kết.
3. Admin chọn mã Order thứ nhất.
4. Admin quay lại Chi tiết mẫu hoa.
5. Admin chọn mã Order thứ hai.
6. Admin quan sát khu vực giá và bill tại Chi tiết mẫu hoa.

**Test Data:**
- Mã mẫu hoa: `FLOWER-00135`.
- Order liên kết:
  - `ORD-00120`.
  - `ORD-00125`.

**Expected Result:**
- Hệ thống hiển thị đầy đủ `ORD-00120` và `ORD-00125`.
- Các Order liên kết không bị thiếu hoặc gộp sai.
- Khi chọn `ORD-00120`, hệ thống mở đúng Chi tiết Order tương ứng.
- Khi chọn `ORD-00125`, hệ thống mở đúng Chi tiết Order tương ứng.
- Giá và bill của các Order không hiển thị tại Chi tiết mẫu hoa.
- Admin có thể xem giá và bill tại Chi tiết Order tương ứng.

**Trace to:**
- [STORY-048/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [BR-150](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312)

**Rationale:**
> Xác minh Admin xem được đầy đủ các Order liên quan đến mẫu hoa và có thể điều hướng tới từng Order.

---

## ST-048-04-01 — Xem ảnh kết quả mẫu hoa bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/387d91af-407c-47f1-9a24-64f3815fd6f2) |
| **Story** | STORY-048 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Ảnh kết quả mẫu hoa còn khả dụng.

**Steps:**
1. Admin mở Chi tiết mẫu hoa.
2. Admin chọn ảnh kết quả.
3. Admin quan sát ảnh trong Image Lightbox.
4. Admin đóng Image Lightbox.
5. Admin quan sát vị trí màn hình sau khi đóng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống mở Image Lightbox khi Admin chọn ảnh kết quả.
- Ảnh được hiển thị đúng tỷ lệ.
- Admin có thể đóng Image Lightbox.
- Sau khi đóng, Admin quay lại Chi tiết mẫu hoa tại vị trí trước đó.

**Trace to:**
- [STORY-048/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)

**Rationale:**
> Xác minh Admin có thể xem nhanh ảnh kết quả ở kích thước lớn mà không rời khỏi Chi tiết mẫu hoa.

---

## ST-048-05-01 — Xem ảnh Mockup nguồn bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fb041b62-2af6-441c-aeb1-037f243237b0) |
| **Story** | STORY-048 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Mẫu hoa có Mockup nguồn và ảnh Mockup còn khả dụng.

**Steps:**
1. Admin mở Chi tiết mẫu hoa.
2. Admin chọn ảnh Mockup nguồn.
3. Admin quan sát ảnh trong Image Lightbox.
4. Admin đóng Image Lightbox.
5. Admin quan sát vị trí màn hình sau khi đóng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống mở Image Lightbox khi Admin chọn Mockup.
- Ảnh Mockup được hiển thị đúng tỷ lệ.
- Admin có thể đóng Image Lightbox.
- Sau khi đóng, Admin quay lại Chi tiết mẫu hoa tại vị trí trước đó.

**Trace to:**
- [STORY-048/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [BR-147](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d7ba22e1-39f3-4c7a-8033-fd13c480478a)

**Rationale:**
> Xác minh Admin có thể xem nhanh Mockup nguồn được sử dụng cho mẫu hoa.

---

## ST-048-06-01 — Error state khi không tải được Chi tiết mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eda69a00-3639-40c9-8a8a-6d719a1f9c94) |
| **Story** | STORY-048 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có thể mô phỏng tình trạng không tải được dữ liệu Chi tiết mẫu hoa bằng cách ngắt kết nối mạng hoặc đặt trình duyệt ở chế độ Offline.
- Sau lần lỗi đầu tiên, có thể khôi phục kết nối để tải lại dữ liệu.

**Steps:**
1. Đặt trình duyệt ở chế độ Offline hoặc ngắt kết nối mạng.
2. Admin mở Chi tiết mẫu hoa.
3. Admin quan sát trạng thái màn hình.
4. Khôi phục kết nối mạng.
5. Admin chọn thao tác tải lại.
6. Admin quan sát Chi tiết mẫu hoa sau khi tải lại.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống hiển thị error state khi không tải được dữ liệu.
- Hệ thống không hiển thị dữ liệu thiếu, dữ liệu cũ hoặc dữ liệu không đầy đủ như kết quả hoàn chỉnh.
- Có thao tác cho phép Admin tải lại.
- Sau khi kết nối được khôi phục và Admin tải lại, Chi tiết mẫu hoa được hiển thị thành công.
- Nếu vẫn không tải được dữ liệu, hệ thống tiếp tục giữ error state.

**Trace to:**
- [STORY-048/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)

**Rationale:**
> Xác minh Admin có thể nhận biết lỗi tải dữ liệu và tiếp tục xem Chi tiết mẫu hoa sau khi kết nối được khôi phục.

---

## ST-048-07-01 — Chặn tài khoản không có quyền xem Chi tiết mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a2882642-fe43-41f9-a2d2-c54e01c6d24b) |
| **Story** | STORY-048 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một tài khoản Website quản trị không có quyền xem AI Custom → Mẫu hoa.
- Có một mẫu hoa Custom AI tồn tại.

**Steps:**
1. Đăng nhập bằng tài khoản không có quyền.
2. Thử truy cập trực tiếp Chi tiết mẫu hoa.
3. Quan sát trạng thái màn hình.
4. Quan sát các thông tin được hiển thị.

**Test Data:**
- Tài khoản: `staff_no_ai_permission`.
- Mã mẫu hoa: `FLOWER-00125`.

**Expected Result:**
- Người dùng không thể xem Chi tiết `FLOWER-00125`.
- Hệ thống hiển thị trạng thái không có quyền truy cập.
- Hệ thống không hiển thị ảnh kết quả.
- Hệ thống không hiển thị thông tin khách hàng.
- Hệ thống không hiển thị Combo, Core, Support, Mockup hoặc yêu cầu tùy chỉnh.
- Hệ thống không hiển thị các Order liên kết.
- Hệ thống không cho phép mở ảnh hoặc truy cập Order liên quan từ mẫu hoa này.

**Trace to:**
- [STORY-048/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [BR-132](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577)

**Rationale:**
> Xác minh Chi tiết mẫu hoa chỉ được hiển thị cho tài khoản có quyền tương ứng.

---

## ST-048-08-01 — Mẫu hoa không tồn tại hoặc không thể truy cập

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/150c564a-f360-4646-b77c-afd6687b769e) |
| **Story** | STORY-048 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có thể truy cập trực tiếp Chi tiết mẫu hoa bằng mã hoặc đường dẫn.

**Steps:**
1. Admin truy cập Chi tiết bằng một mã mẫu hoa không tồn tại.
2. Admin quan sát trạng thái màn hình.
3. Admin quan sát dữ liệu hiển thị.
4. Admin chọn thao tác quay lại danh sách nếu có.

**Test Data:**
- Mã mẫu hoa không tồn tại: `FLOWER-99999`.

**Expected Result:**
- Hệ thống không hiển thị Chi tiết `FLOWER-99999` như một mẫu hoa hợp lệ.
- Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.
- Hệ thống không hiển thị ảnh, thông tin khách hàng, Combo, Core, Support, Mockup, yêu cầu tùy chỉnh hoặc Order liên kết.
- Admin có thể quay lại danh sách mẫu hoa.
- Hệ thống không tự động chuyển Admin sang một mẫu hoa khác.

**Trace to:**
- [STORY-048/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)

**Rationale:**
> Xác minh hệ thống xử lý đúng khi Admin truy cập một mẫu hoa không tồn tại hoặc không thể truy cập.

---

## ST-048-09-01 — Lỗi tải ảnh chỉ ảnh hưởng vùng ảnh trong Chi tiết mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-048-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/301aa91e-e6f0-448c-bbfd-d3d9c9e29d15) |
| **Story** | STORY-048 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền xem AI Custom → Mẫu hoa.
- Có một History record mẫu hoa hợp lệ nhưng ảnh kết quả không tải được tại thời điểm hiển thị.
- Các metadata của mẫu hoa vẫn còn khả dụng.

**Steps:**
1. Admin mở Chi tiết mẫu hoa.
2. Admin quan sát vùng ảnh kết quả.
3. Admin quan sát các thông tin còn lại của mẫu hoa.
4. Admin tải lại trang.
5. Admin quan sát lại trạng thái ảnh và metadata.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Admin vẫn mở được Chi tiết mẫu hoa.
- Hệ thống hiển thị trạng thái lỗi tại vùng ảnh.
- Hệ thống không hiển thị ảnh lỗi như một ảnh hợp lệ.
- Các metadata còn lại của mẫu hoa vẫn được hiển thị.
- Admin có thể tải lại trang.
- Lỗi ảnh không làm mất hoặc thay đổi thông tin mẫu hoa.
- Hệ thống không tự động tạo ảnh mới thay thế.
- History record và các Order liên kết không bị thay đổi do lỗi tải ảnh.

**Trace to:**
- [STORY-048/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [STORY-048/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/008fcd50-b7f4-49ff-9411-2544f76386a3)
- [BR-151](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f)

**Rationale:**
> Xác minh lỗi tải ảnh chỉ ảnh hưởng vùng ảnh và không làm mất dữ liệu Chi tiết mẫu hoa.
