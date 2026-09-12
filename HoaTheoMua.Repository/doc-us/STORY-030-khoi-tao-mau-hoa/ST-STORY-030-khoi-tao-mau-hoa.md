# ST — STORY-030 — Khởi tạo mẫu hoa — System Tests

---

## ST-030-01-01 — Smoke: Tạo yêu cầu mẫu hoa thành công từ đầu đến cuối

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/80dbc890-368a-4862-b3c7-f6def582e82e) |
| **Story** | STORY-030 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Combo A còn khả dụng.
- Có ít nhất một Mockup còn khả dụng.

**Steps:**
1. Khách hàng chọn "Tạo mẫu hoa" trên Combo A.
2. Khách hàng nhập Tên hợp lệ cho yêu cầu.
3. Khách hàng chọn "Tiếp theo".
4. Khách hàng chọn Mockup A.
5. Khách hàng chọn "Hoàn thành".

**Test Data:**
- Combo A còn khả dụng.
- Tên yêu cầu: `Mẫu hoa sinh nhật`.
- Mockup A còn khả dụng.

**Expected Result:**
- Hệ thống mở đúng quy trình tạo yêu cầu mẫu hoa từ Combo A.
- Step 1 hiển thị trường Tên và thông tin Combo A.
- Khách hàng chuyển được sang Step 2 khi Tên hợp lệ.
- Step 2 hiển thị các Mockup còn khả dụng.
- Mockup A được ghi nhận là Mockup được chọn.
- Sau khi hoàn thành, chỉ một yêu cầu tạo mẫu hoa được tạo.
- Yêu cầu thuộc tài khoản khách hàng hiện tại, gắn với Combo A và Mockup A.
- Yêu cầu có trạng thái ban đầu là **"Bản nháp"**.
- Hệ thống thông báo tạo thành công và chuyển khách hàng đến màn hình Chi tiết yêu cầu tạo mẫu hoa.
- Màn hình chi tiết hiển thị đúng thông tin của yêu cầu vừa tạo và có nút **"Tạo mẫu hoa bằng AI"**.
- Hệ thống không tự động tạo mẫu hoa bằng AI khi khách hàng chưa chọn chức năng đó.

**Trace to:**
- [STORY-030/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-013](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-070](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e85f0e09-9b6a-49a8-b62d-ccc467c2bb94) · [BR-072](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2371ded1-c7f4-42a2-98a4-1c4f71d8095c) · [BR-077](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d) · [BR-080](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/25e0fc0c-5927-4bab-8a46-86ff656fe108)

**Rationale:**
> Xác nhận khách hàng tạo được yêu cầu tạo mẫu hoa từ một Combo và được chuyển đến màn hình chi tiết với đúng thông tin đã chọn.

---

## ST-030-02-01 — Combo nguồn được giữ đúng trong toàn bộ quy trình

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a108a3da-5b21-458f-afa3-41dd7e3812de) |
| **Story** | STORY-030 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Combo A và Combo B cùng hiển thị trên danh sách Combo.
- Khách hàng bắt đầu quy trình từ Combo A.

**Steps:**
1. Khách hàng chọn "Tạo mẫu hoa" trên Combo A.
2. Khách hàng xem Step 1 – Thông tin & Combo.
3. Khách hàng hoàn tất quy trình với Tên hợp lệ và một Mockup hợp lệ.
4. Khách hàng mở màn hình Chi tiết yêu cầu tạo mẫu hoa.

**Test Data:**
- Combo A: Combo khách hàng chọn để tạo mẫu hoa.
- Combo B: Combo khác đang tồn tại trong hệ thống.

**Expected Result:**
- Step 1 hiển thị đúng tên, mô tả, thành phần, giá và hình ảnh nguyên liệu của Combo A.
- Combo B không hiển thị như Combo nguồn của quy trình.
- Sau khi hoàn tất, yêu cầu tạo mẫu hoa được gắn với Combo A.
- Yêu cầu chỉ có một Combo nguồn.
- Màn hình Chi tiết yêu cầu tạo mẫu hoa hiển thị Combo nguồn là Combo A.

**Trace to:**
- [STORY-030/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-070](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e85f0e09-9b6a-49a8-b62d-ccc467c2bb94) · [BR-071](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/30761eaa-4df8-4e44-8d1e-efaba44782bf) · [BR-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71fd8ff3-1fbd-4ab8-969b-1256d2e70618)

**Rationale:**
> Xác nhận Combo khách hàng chọn ban đầu được giữ làm Combo nguồn trong toàn bộ quy trình.

---

## ST-030-03-01 — Validation Tên yêu cầu tạo mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b0e836a0-9e06-4eca-ae41-bb9b442b524a) |
| **Story** | STORY-030 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập và đang ở Step 1 – Thông tin & Combo.

**Steps:**
1. Khách hàng lần lượt nhập các giá trị Tên trong phần Test Data.
2. Với mỗi giá trị, khách hàng chọn "Tiếp theo".
3. Kiểm tra thông báo lỗi hoặc khả năng chuyển sang Step 2.

**Test Data:**

| # | Giá trị Tên | Kỳ vọng |
|---|---|---|
| 1 | _(rỗng)_ | Lỗi — không chuyển sang Step 2 |
| 2 | _(chỉ chứa khoảng trắng)_ | Lỗi — xem như chưa nhập Tên |
| 3 | `" A "` | Hợp lệ — trim thành `"A"`, chuyển sang Step 2 |
| 4 | 1 ký tự | Hợp lệ — chuyển sang Step 2 |
| 5 | Đúng 100 ký tự | Hợp lệ — chuyển sang Step 2 |
| 6 | 101 ký tự | Lỗi — vượt quá giới hạn, không chuyển sang Step 2 |

**Expected Result:**
- Với Tên rỗng, hệ thống hiển thị lỗi tại trường Tên và không chuyển sang Step 2.
- Với Tên chỉ chứa khoảng trắng, hệ thống xem như chưa nhập Tên, hiển thị lỗi và không chuyển sang Step 2.
- Với Tên `" A "`, hệ thống bỏ khoảng trắng thừa, ghi nhận giá trị `"A"` và cho phép chuyển sang Step 2.
- Với Tên dài 1 ký tự, hệ thống cho phép chuyển sang Step 2.
- Với Tên dài đúng 100 ký tự, hệ thống cho phép chuyển sang Step 2.
- Với Tên dài 101 ký tự, hệ thống hiển thị lỗi vượt quá giới hạn và không chuyển sang Step 2.

**Trace to:**
- [STORY-030/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-015](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-016](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b1017f3c-b500-4d66-83a3-46b3a488d37f) · [BR-069](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a925fb05-3c5e-4592-8e0c-22a516df58a2)

**Rationale:**
> Xác nhận khách hàng chỉ có thể đi tiếp khi Tên yêu cầu hợp lệ sau khi bỏ khoảng trắng thừa ở đầu và cuối.

---

## ST-030-04-01 — Quay lại bước trước để sửa thông tin

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/461c5777-3401-4947-8390-af34b9a917b6) |
| **Story** | STORY-030 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã nhập Tên hợp lệ ở Step 1 và đang ở Step 2 – Mockup.

**Steps:**
1. Khách hàng chọn "Quay lại" từ Step 2.
2. Khách hàng kiểm tra thông tin tại Step 1.
3. Khách hàng sửa Tên yêu cầu.
4. Khách hàng chọn "Tiếp theo" để quay lại Step 2.

**Test Data:**
- Tên ban đầu: `Mẫu hoa sinh nhật`.
- Tên sau chỉnh sửa: `Mẫu hoa sinh nhật cho mẹ`.
- Combo nguồn: Combo A.

**Expected Result:**
- Hệ thống đưa khách hàng từ Step 2 về Step 1 – Thông tin & Combo.
- Trường Tên vẫn hiển thị giá trị đã nhập trước đó.
- Combo nguồn vẫn là Combo A và không bị thay đổi.
- Khách hàng chỉnh sửa được Tên yêu cầu.
- Tên mới được ghi nhận sau khi khách hàng chọn "Tiếp theo".
- Hệ thống cho phép khách hàng quay lại Step 2 khi Tên sau chỉnh sửa hợp lệ.
- Dữ liệu đã nhập và lựa chọn trước đó không bị mất khi quay lại bước trước.

**Trace to:**
- [STORY-030/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)

**Rationale:**
> Xác nhận khách hàng có thể quay lại bước trước để sửa thông tin mà không mất dữ liệu đã chọn.

---

## ST-030-05-01 — Chọn Mockup và chỉ chọn được một Mockup tại một thời điểm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e2f4d0a9-f844-472c-a8bc-a538721896d4) |
| **Story** | STORY-030 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã hoàn tất Step 1 với Tên hợp lệ.
- Có ít nhất hai Mockup còn khả dụng.

**Steps:**
1. Khách hàng chuyển sang Step 2 – Mockup.
2. Khách hàng xem danh sách Mockup.
3. Khách hàng chọn Mockup A.
4. Khách hàng chọn tiếp Mockup B.

**Test Data:**
- Mockup A và Mockup B còn khả dụng.

**Expected Result:**
- Step 2 hiển thị các Mockup còn khả dụng dưới dạng thẻ Mockup.
- Mockup A và Mockup B đều hiển thị trong danh sách.
- Sau khi khách hàng chọn Mockup A, Mockup A hiển thị là lựa chọn hiện tại.
- Khi khách hàng chọn Mockup B, Mockup B trở thành lựa chọn hiện tại.
- Mockup A không còn ở trạng thái được chọn.
- Tại một thời điểm, khách hàng chỉ có một Mockup được chọn.

**Trace to:**
- [STORY-030/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-076](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c5e7d260-7968-449d-b0a6-8de5602056d0) · [BR-077](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d)

**Rationale:**
> Xác nhận khách hàng nhìn thấy danh sách Mockup còn khả dụng và chỉ chọn được một Mockup tại một thời điểm.

---

## ST-030-06-01 — Phân trang Mockup và giữ lựa chọn khi chuyển trang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a8f2e389-26a5-4ae6-bc7f-ae6ecf565b4d) |
| **Story** | STORY-030 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang ở Step 2 – Mockup.
- Có nhiều hơn 4 Mockup còn khả dụng.

**Steps:**
1. Khách hàng xem trang đầu tiên của danh sách Mockup.
2. Khách hàng chọn Mockup A ở trang đầu tiên.
3. Khách hàng chuyển sang trang tiếp theo.
4. Khách hàng quay lại trang đầu tiên.

**Test Data:**
- Có 5 Mockup còn khả dụng.
- Mockup A nằm ở trang đầu tiên.

**Expected Result:**
- Trang đầu tiên hiển thị tối đa 4 Mockup.
- Hệ thống hiển thị chức năng chuyển trang.
- Sau khi khách hàng chọn Mockup A, Mockup A được ghi nhận là lựa chọn hiện tại.
- Khi chuyển sang trang tiếp theo, danh sách Mockup của trang mới hiển thị đúng.
- Khi quay lại trang đầu tiên, Mockup A vẫn ở trạng thái được chọn.
- Việc chuyển trang không làm mất lựa chọn của khách hàng.
- Tại mọi thời điểm, khách hàng chỉ có một Mockup được chọn.

**Trace to:**
- [STORY-030/AC-014](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-076](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c5e7d260-7968-449d-b0a6-8de5602056d0) · [BR-077](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d)

**Rationale:**
> Xác nhận danh sách Mockup được phân trang đúng và lựa chọn của khách hàng không bị mất khi chuyển trang.

---

## ST-030-07-01 — Xem ảnh Mockup phóng lớn (Image Lightbox)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3e96cdce-4e2d-47d0-9a3f-a6652bc7f8f6) |
| **Story** | STORY-030 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P3 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang ở Step 2 – Mockup.
- Mockup A đang hiển thị trong danh sách.

**Steps:**
1. Khách hàng chọn "Xem Mockup" trên Mockup A.
2. Khách hàng xem ảnh Mockup được phóng lớn.
3. Khách hàng đóng màn hình xem ảnh.

**Test Data:**
- Mockup A có ảnh hợp lệ.

**Expected Result:**
- Hệ thống hiển thị đúng ảnh của Mockup A ở kích thước lớn hơn ảnh trên thẻ Mockup.
- Khách hàng có thể đóng màn hình xem ảnh.
- Sau khi đóng, khách hàng trở lại Step 2 – Mockup.
- Danh sách Mockup vẫn hiển thị bình thường.
- Việc xem ảnh không làm mất Tên đã nhập, Combo nguồn hoặc Mockup đang chọn.
- Khách hàng vẫn có thể tiếp tục chọn Mockup và hoàn tất quy trình.

**Trace to:**
- [STORY-030/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)

**Rationale:**
> Xác nhận khách hàng có thể xem ảnh Mockup lớn hơn và quay lại quy trình chọn Mockup mà không mất dữ liệu.

---

## ST-030-08-01 — Chưa chọn Mockup thì không tạo được yêu cầu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c8651cc-8e85-4d3b-9208-6e4a04538a41) |
| **Story** | STORY-030 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã nhập Tên hợp lệ và đang ở Step 2 – Mockup.
- Chưa có Mockup nào được chọn.

**Steps:**
1. Khách hàng không chọn Mockup nào.
2. Khách hàng chọn "Hoàn thành".
3. Khách hàng chọn một Mockup còn khả dụng.
4. Khách hàng tiếp tục quy trình.

**Test Data:**
- Tên yêu cầu: `Mẫu hoa sinh nhật`.
- Danh sách có Mockup A còn khả dụng.

**Expected Result:**
- Khi chưa chọn Mockup, hệ thống không tạo yêu cầu tạo mẫu hoa.
- Hệ thống yêu cầu khách hàng chọn một Mockup trước khi tiếp tục.
- Khách hàng vẫn ở Step 2 – Mockup.
- Tên và Combo nguồn đã nhập trước đó không bị mất.
- Sau khi khách hàng chọn Mockup A, hệ thống ghi nhận Mockup A là lựa chọn hiện tại.
- Không có yêu cầu tạo mẫu hoa nào được tạo từ lần khách hàng bấm "Hoàn thành" khi chưa chọn Mockup.

**Trace to:**
- [STORY-030/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-078](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e2249f4-842f-4159-a04c-295904f50ec3)

**Rationale:**
> Xác nhận khách hàng phải chọn một Mockup trước khi tạo yêu cầu tạo mẫu hoa.

---

## ST-030-09-01 — Combo không còn khả dụng tại thời điểm hoàn thành

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/82dda5de-7b3e-4bed-a04a-e16b1ad03fc3) |
| **Story** | STORY-030 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã nhập Tên hợp lệ và chọn một Mockup hợp lệ.
- Combo A là Combo nguồn nhưng không còn khả dụng trước khi khách hàng hoàn thành.

**Steps:**
1. Khách hàng thực hiện quy trình từ Combo A.
2. Khách hàng nhập Tên hợp lệ.
3. Khách hàng chọn Mockup A.
4. Khách hàng chọn "Hoàn thành".

**Test Data:**
- Combo A không còn khả dụng tại thời điểm hoàn thành.
- Mockup A còn khả dụng.

**Expected Result:**
- Hệ thống không tạo yêu cầu tạo mẫu hoa.
- Hệ thống hiển thị thông báo: **"Combo đã chọn không còn khả dụng. Vui lòng chọn combo khác."**
- Khách hàng được đưa về danh sách Combo để chọn Combo khác.
- Không có yêu cầu tạo mẫu hoa mới nào được gắn với Combo A từ lần thực hiện này.
- Không có dữ liệu lưu dở dang do quy trình bị từ chối.

**Trace to:**
- [STORY-030/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-070](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e85f0e09-9b6a-49a8-b62d-ccc467c2bb94) · [BR-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71fd8ff3-1fbd-4ab8-969b-1256d2e70618)

**Rationale:**
> Xác nhận hệ thống không tạo yêu cầu tạo mẫu hoa khi Combo nguồn không còn khả dụng tại thời điểm khách hàng hoàn thành.

---

## ST-030-10-01 — Mockup không còn khả dụng tại thời điểm hoàn thành

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c1a9811a-8f16-4b0f-afe9-933170491aa5) |
| **Story** | STORY-030 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã nhập Tên hợp lệ và chọn Mockup A.
- Mockup A không còn khả dụng trước khi khách hàng hoàn thành.
- Combo nguồn vẫn còn khả dụng.

**Steps:**
1. Khách hàng thực hiện quy trình từ Combo A.
2. Khách hàng chọn Mockup A ở Step 2.
3. Mockup A không còn khả dụng trước khi khách hàng chọn "Hoàn thành".
4. Khách hàng chọn "Hoàn thành".

**Test Data:**
- Combo A còn khả dụng.
- Mockup A không còn khả dụng tại thời điểm hoàn thành.
- Mockup B còn khả dụng.

**Expected Result:**
- Hệ thống không tạo yêu cầu tạo mẫu hoa.
- Hệ thống hiển thị thông báo phù hợp.
- Không có yêu cầu tạo mẫu hoa mới nào được gắn với Mockup A từ lần thực hiện này.
- Không có dữ liệu lưu dở dang.

**Trace to:**
- [STORY-030/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-076](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c5e7d260-7968-449d-b0a6-8de5602056d0) · [BR-077](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d)

**Rationale:**
> Xác nhận hệ thống không tạo yêu cầu tạo mẫu hoa khi Mockup khách hàng chọn không còn khả dụng tại thời điểm hoàn thành.

---

## ST-030-11-01 — Regression: Lưu thất bại lần đầu, thử lại thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-030-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff7c64cf-3221-4511-83de-71d79342c9d6) |
| **Story** | STORY-030 |
| **Loại** | 4 |
| **Suite** | REGRESSION |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Dữ liệu cần thiết để tạo yêu cầu hợp lệ.
- Hệ thống được cấu hình để lần lưu đầu tiên thất bại do lỗi hệ thống.
- Sau lần lỗi đầu tiên, chức năng lưu hoạt động bình thường.

**Steps:**
1. Mở chức năng tạo yêu cầu.
2. Nhập đầy đủ dữ liệu hợp lệ.
3. Thực hiện thao tác hoàn thành/lưu yêu cầu.
4. Quan sát kết quả khi hệ thống không thể lưu.
5. Chọn **Thử lại** sau khi lỗi hệ thống đã được khắc phục.
6. Kiểm tra yêu cầu vừa tạo trong danh sách/chi tiết.

**Test Data:**
- Tên yêu cầu: `Mẫu hoa sinh nhật`.
- Combo A còn khả dụng.
- Mockup A còn khả dụng.
- Lần lưu đầu tiên thất bại.
- Lần thử lại thành công.

**Expected Result:**
- Lần lưu đầu tiên thất bại và hệ thống hiển thị thông báo phù hợp.
- Không tạo yêu cầu hoàn chỉnh từ lần lưu thất bại.
- Không phát sinh bản ghi trùng.
- Dữ liệu người dùng đã nhập được giữ lại nếu requirement quy định không mất dữ liệu khi retry.
- Người dùng có thể thực hiện lại thao tác lưu.
- Khi thử lại thành công, hệ thống chỉ tạo **một yêu cầu duy nhất**.
- Yêu cầu được lưu đúng toàn bộ dữ liệu đã nhập.
- Trạng thái và các liên kết liên quan được tạo đúng theo business rule.

**Trace to:**
- [STORY-030/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) · [STORY-030/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [BR-072](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2371ded1-c7f4-42a2-98a4-1c4f71d8095c) · [BR-080](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/25e0fc0c-5927-4bab-8a46-86ff656fe108)

**Rationale:**
> Xác nhận khi lưu yêu cầu thất bại, khách hàng nhận được thông báo phù hợp, dữ liệu không bị lưu dở dang và có thể thử lại.


