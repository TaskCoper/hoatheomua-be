# ST — STORY-045 — Khách hàng xem History thiệp thiết kế — System Tests

---

## ST-045-01-01 — Xem danh sách History thiệp đã tạo

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a612c3e7-7cc6-40f9-9e8e-484f2e8f93db) |
| **Story** | STORY-045 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Khách hàng có nhiều History record thiệp hợp lệ được tạo từ các lần generate có ảnh output.
- Các thiệp có thời điểm generate khác nhau.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng quan sát danh sách History.
3. Khách hàng quan sát thông tin của từng item.
4. Khách hàng quan sát thứ tự các thiệp theo thời điểm tạo.
5. Khách hàng chọn một thiệp để xem chi tiết.

**Test Data:**
- `CARD-00121`: tạo lúc `20/08/2026 09:00`.
- `CARD-00122`: tạo lúc `21/08/2026 14:30`.
- `CARD-00123`: tạo lúc `22/08/2026 08:15`.

**Expected Result:**
- Hệ thống hiển thị danh sách History thuộc khách hàng hiện tại.
- Mỗi item hiển thị Preview, Mã thiệp, Thông tin thiệp, Thời điểm tạo và Thao tác.
- Danh sách được sắp xếp theo thời điểm generate mới nhất trước.
- Thứ tự hiển thị là `CARD-00123`, `CARD-00122`, `CARD-00121`.
- Khi khách hàng chọn một thiệp, hệ thống mở đúng Chi tiết thiệp tương ứng.
- Hệ thống không hiển thị thiệp thuộc khách hàng khác.

**Trace to:**
- [STORY-045/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-134](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305)
- [BR-137](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/562e4749-d01e-4244-94b8-5bd3db56ec78)
- [BR-139](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b)

**Rationale:**
> Xác minh khách hàng có thể xem danh sách các thiệp đã tạo của mình và danh sách được hiển thị đúng thứ tự.

---

## ST-045-02-01 — Chỉ hiển thị History record có ảnh output hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/932d27ab-3a1f-4f5a-8bcd-0d40e952a1c6) |
| **Story** | STORY-045 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có các History record thiệp hợp lệ đã được tạo từ các lần generate có ảnh output.
- Không có History record tương ứng với các lần generate không có ảnh output.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng quan sát danh sách History.
3. Khách hàng đối chiếu các History item hiện có.

**Test Data:**
- `CARD-00130`: thiệp được generate thành công, có ảnh output và có History record.
- `CARD-00131`: thiệp được generate thành công, có ảnh output và có History record.
- Có 01 lần generate khác không tạo được ảnh output, và không có History record tương ứng.

**Expected Result:**
- Các thiệp có History record hợp lệ được hiển thị.
- Mỗi History record chỉ xuất hiện một lần.
- Không xuất hiện item không có ảnh output.

**Trace to:**
- [STORY-045/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-133](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6e43277-f57a-495b-8f0e-59929386c30e)

**Rationale:**
> Xác minh History chỉ hiển thị các record hợp lệ được tạo từ lần generate có ảnh output.

---

## ST-045-03-01 — Thiệp chưa liên kết Order vẫn hiển thị trong History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f85fa0d4-97e8-4880-a997-b95fd26d512f) |
| **Story** | STORY-045 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một History record thiệp có ảnh output nhưng chưa liên kết với Order nào.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng tìm thiệp chưa liên kết Order.
3. Khách hàng quan sát thiệp trong danh sách History.
4. Khách hàng mở Chi tiết thiệp.
5. Khách hàng quan sát thông tin Order liên quan.
6. Khách hàng quan sát các thông tin và thao tác của thiệp.

**Test Data:**
- Mã thiệp: `CARD-00135`.
- Trạng thái liên kết Order: Chưa liên kết.

**Expected Result:**
- `CARD-00135` vẫn xuất hiện trong History.
- Các thông tin của thiệp vẫn được hiển thị bình thường.
- Thông tin Order được để trống hoặc thể hiện trạng thái chưa liên kết.
- Khách hàng vẫn có thể mở Chi tiết thiệp.
- Các thao tác khả dụng của thiệp vẫn được hiển thị theo trạng thái hiện tại.

**Trace to:**
- [STORY-045/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-134](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305)

**Rationale:**
> Xác minh thiệp chưa từng liên kết Order vẫn được giữ và hiển thị bình thường trong History.

---

## ST-045-04-01 — Thiệp liên kết nhiều Order chỉ có một History item

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/31f539e3-8987-4e11-9c38-45cceb4ec843) |
| **Story** | STORY-045 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một History record thiệp đã được sử dụng cho từ 2 Order trở lên.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng tìm thiệp cần kiểm tra.
3. Khách hàng quan sát số lần thiệp xuất hiện trong danh sách History.
4. Khách hàng mở Chi tiết thiệp.
5. Khách hàng quan sát danh sách Order liên quan.
6. Khách hàng chọn một mã Order.
7. Khách hàng quan sát trang được điều hướng đến.

**Test Data:**
- Mã thiệp: `CARD-00140`.
- Order liên kết:
  - `ORD-00120`.
  - `ORD-00125`.

**Expected Result:**
- `CARD-00140` chỉ xuất hiện đúng 01 History item.
- Hệ thống không nhân bản History item theo số lượng Order liên kết.
- Chi tiết thiệp hiển thị đầy đủ `ORD-00120` và `ORD-00125`.
- Khi khách hàng chọn một mã Order, hệ thống điều hướng đến đúng Order tương ứng.

**Trace to:**
- [STORY-045/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-135](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b)

**Rationale:**
> Xác minh một thiệp được sử dụng cho nhiều Order vẫn chỉ có một History item ảnh gốc.

---

## ST-045-05-01 — Tái sử dụng thiệp không tạo History item mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1808c019-60f2-43ef-8a22-4fffef856801) |
| **Story** | STORY-045 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một History record thiệp đã tồn tại và đang liên kết với một Order.
- Thiệp có thể được sử dụng lại cho Checkout mới.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng ghi nhận số History item của thiệp cần kiểm tra.
3. Khách hàng sử dụng lại thiệp cho một Checkout mới.
4. Khách hàng hoàn tất quy trình để thiệp được liên kết với Order mới.
5. Khách hàng quay lại **"Thiệp thiết kế"**.
6. Khách hàng quan sát số History item của thiệp.
7. Khách hàng mở Chi tiết thiệp và quan sát danh sách Order liên quan.

**Test Data:**
- Mã thiệp: `CARD-00145`.
- Order hiện tại: `ORD-00130`.
- Order mới: `ORD-00131`.

**Expected Result:**
- Sau khi tái sử dụng, `CARD-00145` vẫn chỉ xuất hiện đúng 01 History item.
- Không xuất hiện History item mới do thao tác tái sử dụng.
- Chi tiết thiệp hiển thị thêm `ORD-00131` trong danh sách Order liên quan.
- Ảnh History gốc của thiệp không bị thay đổi.

**Trace to:**
- [STORY-045/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-135](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b)
- [BR-136](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0f4ef94-8cc7-4e23-8d2d-2b4a932417fc)

**Rationale:**
> Xác minh việc tái sử dụng thiệp chỉ bổ sung liên kết Order và không tạo History item mới.

---

## ST-045-06-01 — Empty state khi khách hàng chưa có History thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6da6c3aa-f2b7-4b48-8bd2-40e09d01a532) |
| **Story** | STORY-045 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Khách hàng chưa có History record thiệp nào có ảnh output.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng quan sát khu vực danh sách History.
3. Khách hàng quan sát nội dung hướng dẫn trên màn hình.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống không hiển thị danh sách thiệp.
- Hệ thống hiển thị empty state.
- Hệ thống cho biết khách hàng chưa có thiệp đã tạo.
- Hệ thống có nội dung hướng dẫn khách hàng tạo thiệp.
- Hệ thống không hiển thị dữ liệu History của khách hàng khác.

**Trace to:**
- [STORY-045/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)

**Rationale:**
> Xác minh hệ thống hiển thị trạng thái phù hợp khi khách hàng chưa có History thiệp.

---

## ST-045-07-01 — Error state khi không tải được danh sách History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b277caf1-ebb5-4a15-a961-cc7ca9f26aaf) |
| **Story** | STORY-045 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có thể mô phỏng tình trạng không tải được danh sách History bằng cách ngắt kết nối mạng hoặc đặt trình duyệt ở chế độ Offline.
- Sau lần lỗi đầu tiên, có thể khôi phục kết nối để tải lại danh sách.

**Steps:**
1. Đặt trình duyệt ở chế độ Offline hoặc ngắt kết nối mạng.
2. Khách hàng mở **"Thiệp thiết kế"**.
3. Khách hàng quan sát khu vực danh sách History.
4. Khôi phục kết nối mạng.
5. Khách hàng chọn thao tác **"Thử lại"** hoặc tải lại danh sách.
6. Khách hàng quan sát danh sách sau khi tải lại.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Hệ thống hiển thị error state khi không tải được danh sách History.
- Hệ thống không hiển thị dữ liệu History không đầy đủ như một danh sách hoàn chỉnh.
- Có thao tác cho phép khách hàng thử tải lại.
- Sau khi kết nối được khôi phục và khách hàng thử lại, danh sách History được tải thành công.

**Trace to:**
- [STORY-045/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)

**Rationale:**
> Xác minh khách hàng có thể nhận biết lỗi tải dữ liệu và tiếp tục sử dụng chức năng sau khi kết nối được khôi phục.

---

## ST-045-08-01 — Không cho xem Chi tiết thiệp thuộc khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f756b3d9-f70e-40cb-b54f-32e2b51b6764) |
| **Story** | STORY-045 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có hai tài khoản khách hàng khác nhau.
- Mỗi khách hàng có ít nhất một History record thiệp riêng.
- Có thể truy cập trực tiếp Chi tiết thiệp bằng URL hoặc mã thiệp.

**Steps:**
1. Đăng nhập bằng tài khoản khách hàng A.
2. Truy cập trực tiếp vào Chi tiết thiệp thuộc khách hàng B.
3. Quan sát nội dung được hiển thị.

**Test Data:**
- Khách hàng A: `customer_a`.
- Khách hàng B: `customer_b`.
- Thiệp thuộc khách hàng B: `CARD-00200`.

**Expected Result:**
- Khách hàng A không xem được Chi tiết `CARD-00200`.
- Hệ thống không hiển thị ảnh thiệp.
- Hệ thống không hiển thị metadata của thiệp.
- Hệ thống không hiển thị các Order liên quan.
- Hệ thống không hiển thị thiệp như một tài nguyên hợp lệ thuộc khách hàng A.

**Trace to:**
- [STORY-045/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-139](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b)

**Rationale:**
> Xác minh khách hàng chỉ được truy cập History thiệp thuộc tài khoản của mình.

---

## ST-045-09-01 — File ảnh thiệp không còn khả dụng nhưng History vẫn được giữ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-045-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8b16f5a0-cdf7-41fe-82f0-cfd097f4bcdc) |
| **Story** | STORY-045 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một History record vẫn tồn tại nhưng file ảnh của thiệp không còn khả dụng.

**Steps:**
1. Khách hàng mở **"Thiệp thiết kế"**.
2. Khách hàng mở Chi tiết thiệp có file ảnh không còn khả dụng.
3. Khách hàng quan sát trạng thái ảnh.
4. Khách hàng quan sát các metadata còn lại.
5. Khách hàng quan sát chức năng tải xuống.

**Test Data:**
- Mã thiệp: `CARD-00210`.
- Trạng thái file: `Không khả dụng`.

**Expected Result:**
- History record `CARD-00210` vẫn tồn tại.
- Hệ thống vẫn hiển thị các metadata còn khả dụng của thiệp.
- Hệ thống hiển thị thông báo **"Ảnh không còn khả dụng."**
- Chức năng tải xuống bị vô hiệu hóa.
- File ảnh không còn khả dụng không làm mất History record.

**Trace to:**
- [STORY-045/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [STORY-045/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [BR-138](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a36b64f9-2e28-4fa1-8336-01082bd8a7c5)

**Rationale:**
> Xác minh History record được giữ lại ngay cả khi file ảnh của thiệp không còn khả dụng.
