# ST — STORY-043 — Admin xem chi tiết thiệp AI — System Tests

---

## ST-043-01 — Xem chi tiết thiệp AI đầy đủ thông tin

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76cdf1dc-4331-4f58-81cb-92c733bba1eb) |
| **Story** | STORY-043 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Thiệp cần kiểm tra tồn tại và file ảnh còn khả dụng.

**Steps:**
1. Admin mở danh sách thiệp AI.
2. Admin chọn mã thiệp hoặc **"Xem chi tiết"**.
3. Admin quan sát ảnh kết quả và thông tin cơ bản của thiệp.
4. Admin quan sát thông tin khách hàng.
5. Admin quan sát Người gửi, Người nhận, Lời chúc và ảnh đính kèm nếu có.
6. Admin quan sát danh sách Order liên kết.
7. Admin quan sát khu vực giá và bill.
8. Admin quan sát thao tác tải ảnh thiệp.

**Test Data:**
- Mã thiệp: `CARD-00125`.
- Khách hàng: `Nguyễn Văn A`.
- Số điện thoại: `0901234567`.
- Email: `nguyenvana@example.com`.
- Người gửi: `Minh`.
- Người nhận: `Lan`.
- Lời chúc: `Chúc mừng sinh nhật`.
- Template: `Cổ điển`.
- Size: `10×15`.
- Hình thức: `In`.
- Order liên kết: `ORD-00101`.

**Expected Result:**
- Hệ thống mở đúng Chi tiết của `CARD-00125`.
- Hệ thống hiển thị ảnh kết quả, mã thiệp, Template, Size, Hình thức, ngày tạo và trạng thái file.
- Hệ thống hiển thị đúng họ tên, số điện thoại và email của khách hàng.
- Hệ thống hiển thị đúng Người gửi, Người nhận và Lời chúc.
- Ảnh người dùng đính kèm được hiển thị nếu History record có ảnh đính kèm.
- Hệ thống hiển thị đúng Order `ORD-00101` trong danh sách Order liên kết.
- Hệ thống không hiển thị giá thiệp hoặc bill snapshot tại Chi tiết thiệp.
- Có thao tác **"Tải ảnh thiệp xuống"** khi file ảnh còn khả dụng.

**Trace to:**
- [STORY-043/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [BR-121](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f6e87379-c166-460c-ab30-43cac75150a8)
- [BR-122](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d387e4a2-b452-469c-b3b4-61db7d654416)

**Rationale:**
> Xác minh Admin có thể xem đầy đủ và đúng thông tin của một thiệp AI tại trang Chi tiết thiệp.

---

## ST-043-02 — Thiệp liên kết nhiều Order hiển thị đầy đủ và mở đúng Order

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/29f00233-28a2-4434-8c2b-eeb9dd28cb83) |
| **Story** | STORY-043 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Có thiệp đang liên kết từ 2 Order trở lên.

**Steps:**
1. Admin mở Chi tiết thiệp.
2. Admin quan sát danh sách Order liên kết.
3. Admin chọn mã Order thứ nhất.
4. Admin quay lại Chi tiết thiệp.
5. Admin chọn mã Order thứ hai.
6. Admin quan sát thông tin giá và bill tại Chi tiết thiệp.

**Test Data:**
- Mã thiệp: `CARD-00126`.
- Order liên kết:
  - `ORD-00110`.
  - `ORD-00115`.

**Expected Result:**
- Hệ thống hiển thị đầy đủ `ORD-00110` và `ORD-00115`.
- Hệ thống không làm mất hoặc gộp sai các Order liên kết.
- Khi chọn `ORD-00110`, hệ thống mở đúng Chi tiết Order `ORD-00110`.
- Khi chọn `ORD-00115`, hệ thống mở đúng Chi tiết Order `ORD-00115`.
- Giá và bill của từng Order không hiển thị tại Chi tiết thiệp.
- Admin có thể xem giá và bill tại Chi tiết Order tương ứng.

**Trace to:**
- [STORY-043/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [BR-121](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f6e87379-c166-460c-ab30-43cac75150a8)
- [BR-122](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d387e4a2-b452-469c-b3b4-61db7d654416)

**Rationale:**
> Xác minh thiệp liên kết nhiều Order được hiển thị đầy đủ và Admin có thể truy cập đúng từng Order liên quan.

---

## ST-043-03 — Xem ảnh thiệp bằng Image Lightbox

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9a4614dd-1947-49a1-8b8b-7125987712bf) |
| **Story** | STORY-043 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Có thiệp có file ảnh còn khả dụng.

**Steps:**
1. Admin mở Chi tiết thiệp.
2. Admin chọn ảnh kết quả thiệp.
3. Admin quan sát ảnh trong Image Lightbox.
4. Admin đóng Image Lightbox.
5. Admin quan sát vị trí màn hình sau khi đóng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống mở Image Lightbox khi Admin chọn ảnh.
- Ảnh được hiển thị đúng tỷ lệ và không bị méo.
- Admin có thể đóng Image Lightbox.
- Sau khi đóng, Admin quay lại Chi tiết thiệp.
- Vị trí trước khi mở Lightbox được giữ lại.

**Trace to:**
- [STORY-043/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)

**Rationale:**
> Xác minh Admin có thể xem nhanh ảnh thiệp ở kích thước lớn mà không làm gián đoạn việc xem Chi tiết thiệp.

---

## ST-043-04 — Hiển thị nội dung thiệp Calligraphy trong Chi tiết thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/80e57d06-8d14-4a8c-bc23-746ba94a395e) |
| **Story** | STORY-043 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Có thiệp sử dụng Hình thức Calligraphy.

**Steps:**
1. Admin mở Chi tiết thiệp Calligraphy.
2. Admin quan sát ảnh kết quả.
3. Admin quan sát phần Người gửi.
4. Admin quan sát phần Người nhận.
5. Admin quan sát phần Lời chúc.

**Test Data:**
- Mã thiệp: `CARD-00130`.
- Hình thức: `Calligraphy`.
- Người gửi: `Minh`.
- Người nhận: `Lan`.
- Lời chúc: `Chúc bạn luôn hạnh phúc`.

**Expected Result:**
- Hệ thống mở đúng Chi tiết `CARD-00130`.
- Ảnh output không hiển thị Người gửi, Người nhận và Lời chúc.
- Thông tin Người gửi vẫn hiển thị là `Minh`.
- Thông tin Người nhận vẫn hiển thị là `Lan`.
- Lời chúc vẫn hiển thị là `Chúc bạn luôn hạnh phúc`.
- Các nội dung này được hiển thị riêng tại phần thông tin Chi tiết thiệp.

**Trace to:**
- [STORY-043/AC – Hiển thị nội dung thiệp Calligraphy](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [BR-121](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f6e87379-c166-460c-ab30-43cac75150a8)

**Rationale:**
> Xác minh nội dung dùng cho thiệp Calligraphy vẫn được Admin xem đầy đủ dù không xuất hiện trực tiếp trên ảnh output.

---

## ST-043-05 — File ảnh không còn khả dụng vẫn hiển thị metadata thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e40de92-93d8-445b-9fd0-f3cdb4af013b) |
| **Story** | STORY-043 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Có thiệp vẫn còn History record nhưng file ảnh không còn khả dụng.

**Steps:**
1. Admin mở Chi tiết thiệp.
2. Admin quan sát trạng thái ảnh.
3. Admin quan sát các thông tin metadata của thiệp.
4. Admin quan sát thao tác **"Tải ảnh thiệp xuống"**.
5. Admin di chuột vào thao tác tải xuống bị vô hiệu hóa.
6. Admin quan sát trang trong một khoảng thời gian sau khi mở.

**Test Data:**
- Mã thiệp: `CARD-00135`.
- Trạng thái file: `Không khả dụng`.

**Expected Result:**
- Trang Chi tiết `CARD-00135` vẫn được hiển thị.
- Hệ thống hiển thị trạng thái **"Không khả dụng"**.
- Hệ thống hiển thị thông báo **"Ảnh không còn khả dụng."**
- Các thông tin metadata của thiệp vẫn được giữ và hiển thị.
- Thao tác **"Tải ảnh thiệp xuống"** bị vô hiệu hóa.
- Khi hover vào thao tác tải xuống, hệ thống hiển thị tooltip **"Ảnh không còn khả dụng."**
- Hệ thống không tự xuất hiện ảnh mới để thay thế ảnh đã mất.

**Trace to:**
- [STORY-043/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [BR-123](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/058623b9-bafc-420d-b0fe-5b7f8776a1f8)

**Rationale:**
> Xác minh việc file ảnh bị mất hoặc không thể truy cập không làm mất History record và Admin nhận biết rõ trạng thái hiện tại của file.

---

## ST-043-06 — Lỗi tải Chi tiết thiệp hiển thị error state và cho tải lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54c88fa3-5626-4150-ac6f-b3a8f20e3cd6) |
| **Story** | STORY-043 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền quản lý AI Custom → Thiệp.
- Có thể mô phỏng tình trạng không tải được Chi tiết thiệp và sau đó khôi phục tình trạng tải dữ liệu về bình thường.

**Steps:**
1. Tạo tình huống Chi tiết thiệp không tải được.
2. Admin mở Chi tiết thiệp.
3. Admin quan sát trạng thái màn hình.
4. Khôi phục tình trạng tải dữ liệu về bình thường.
5. Admin chọn thao tác tải lại.
6. Admin quan sát Chi tiết thiệp sau khi tải lại.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống hiển thị trạng thái lỗi khi không tải được Chi tiết thiệp.
- Hệ thống không hiển thị dữ liệu thiếu như một kết quả hoàn chỉnh.
- Có thao tác cho phép Admin tải lại.
- Sau khi tình trạng được khôi phục và Admin tải lại, Chi tiết thiệp được hiển thị thành công.
- Hệ thống không yêu cầu Admin tạo lại hoặc thay đổi thiệp.

**Trace to:**
- [STORY-043/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)

**Rationale:**
> Xác minh Admin có thể nhận biết lỗi tải tạm thời và tiếp tục xem Chi tiết thiệp sau khi hệ thống phục hồi.

---

## ST-043-07 — Chặn người không có quyền và xử lý thiệp không tồn tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-043-07](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/67f1d71c-eb38-42c2-a697-c25641a221eb) |
| **Story** | STORY-043 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một tài khoản không có quyền quản lý AI Custom → Thiệp.
- Có một Admin có quyền quản lý AI Custom → Thiệp.
- Có thể truy cập Chi tiết thiệp bằng mã thiệp trực tiếp.

**Steps:**
1. Đăng nhập bằng tài khoản không có quyền quản lý AI Custom → Thiệp.
2. Truy cập Chi tiết của một thiệp tồn tại.
3. Quan sát nội dung được hiển thị.
4. Đăng nhập lại bằng Admin có quyền.
5. Truy cập Chi tiết bằng một mã thiệp không tồn tại.
6. Quan sát trạng thái màn hình.

**Test Data:**
- Tài khoản không có quyền: `staff_no_ai_permission`.
- Mã thiệp tồn tại: `CARD-00125`.
- Mã thiệp không tồn tại: `CARD-99999`.

**Expected Result:**
- Tài khoản không có quyền không xem được Chi tiết `CARD-00125`.
- Hệ thống không hiển thị ảnh, thông tin khách hàng, Người gửi, Người nhận, Lời chúc hoặc Order liên kết cho tài khoản không có quyền.
- Khi Admin truy cập `CARD-99999`, hệ thống không hiển thị dữ liệu của một thiệp khác.
- Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.
- Hệ thống không hiển thị nội dung Chi tiết thiệp như một kết quả hợp lệ.

**Trace to:**
- [STORY-043/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [STORY-043/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [BR-125](https://document-first.vnzdna.com/projects/117393d8-1afc-4c40-86be-56420f2ae609)

**Rationale:**
> Xác minh Chi tiết thiệp chỉ được hiển thị cho người dùng có quyền và hệ thống xử lý đúng khi tài nguyên không tồn tại.
