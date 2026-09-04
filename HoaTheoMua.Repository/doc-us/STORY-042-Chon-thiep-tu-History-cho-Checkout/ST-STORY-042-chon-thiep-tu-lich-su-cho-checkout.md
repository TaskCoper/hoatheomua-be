# ST — STORY-042 — Khách hàng chọn thiệp từ lịch sử cho Checkout — System Tests

---

## ST-042-01-01 — Hiển thị danh sách thiệp hợp lệ thuộc khách hàng hiện tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c940da5a-a491-4b69-8d67-c107e4ea7e03) |
| **Story** | STORY-042 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Khách hàng đang có Checkout hợp lệ, chưa hoàn tất.

**Steps:**
1. Khách hàng mở Checkout.
2. Khách hàng chọn **"Chọn thiệp từ lịch sử"**.
3. Khách hàng kiểm tra danh sách thiệp được hiển thị.

**Test Data:**
- Khách hàng A có Checkout `CHK-001`, trạng thái chưa hoàn tất.
- History của khách hàng A có:
  - `CARD-001`: có ảnh hợp lệ.
  - `CARD-002`: có ảnh hợp lệ.
  - `CARD-003`: không có ảnh output hợp lệ.
- Khách hàng B có `CARD-004` với ảnh hợp lệ.

**Expected Result:**
- Hệ thống chỉ hiển thị `CARD-001` và `CARD-002`.
- Hệ thống không hiển thị `CARD-003`.
- Hệ thống không hiển thị `CARD-004` của khách hàng B.

**Trace to:**
- [STORY-042/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-111](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de42c1af-ff3c-46d2-a597-b5c33311de4e)

**Rationale:**
> Xác nhận khách hàng chỉ nhìn thấy các thiệp hợp lệ thuộc History của chính mình khi chọn thiệp cho Checkout.

---

## ST-042-02-01 — Chọn thiệp hợp lệ cho Checkout không phát sinh dữ liệu AI mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b782b780-a719-4c24-addc-55377f84bee8) |
| **Story** | STORY-042 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ và chưa có thiệp đang chọn.

**Steps:**
1. Khách hàng mở **"Chọn thiệp từ lịch sử"**.
2. Khách hàng chọn một thiệp.
3. Khách hàng quay lại Checkout và kiểm tra Order Summary.

**Test Data:**
- Checkout `CHK-001` thuộc khách hàng A, chưa hoàn tất, chưa có thiệp.
- Thiệp `CARD-001` thuộc khách hàng A.
- `CARD-001` có History record hợp lệ và file ảnh còn khả dụng.
- Giá thiệp hiện hành: `20.000 VNĐ`.
- Quota AI trước khi chọn: `2/3`.
- Số History item trước khi chọn: `5`.

**Expected Result:**
- `CARD-001` trở thành thiệp đang chọn của Checkout.
- Order Summary hiển thị `CARD-001` và giá tạm tính hiện hành.
- Hệ thống không tạo History item mới.
- Hệ thống không thay đổi quota AI.
- Hệ thống không tạo ảnh mới hoặc gọi lại AI.

**Trace to:**
- [STORY-042/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [STORY-042/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-110](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7673ca62-beb9-4542-9d77-483c2ecbd865)
- [BR-113](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/415b72c7-5124-4ab3-8155-50c7c0e6e500)
- [BR-114](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed7d5cf8-f07a-4ff2-ac26-5d50bf6791da)
- [BR-116](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/97ef5916-4ca6-460f-a02d-a28b26d6823b)

**Rationale:**
> Xác nhận khách hàng có thể chọn một thiệp hợp lệ cho Checkout mà không phát sinh dữ liệu AI mới.

---

## ST-042-03-01 — Chọn thiệp khác thay thế thiệp đang chọn trong Checkout

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4c203d1e-abed-4b33-9e36-2827aeee50ec) |
| **Story** | STORY-042 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout đang có một thiệp được chọn.

**Steps:**
1. Khách hàng mở lại **"Chọn thiệp từ lịch sử"**.
2. Khách hàng chọn một thiệp khác.
3. Khách hàng kiểm tra Checkout và History.

**Test Data:**
- Checkout `CHK-001` đang chọn `CARD-001`.
- `CARD-002` thuộc cùng khách hàng và có file ảnh hợp lệ.
- Cả `CARD-001` và `CARD-002` đều còn tồn tại trong History trước thao tác.

**Expected Result:**
- `CARD-002` thay thế `CARD-001` trong Checkout.
- Checkout chỉ còn một thiệp đang chọn.
- `CARD-001` vẫn tồn tại trong History.
- `CARD-002` vẫn giữ History record gốc.

**Trace to:**
- [STORY-042/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-110](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7673ca62-beb9-4542-9d77-483c2ecbd865)
- [BR-117](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f6b23f3-7321-4b92-a7e6-1ccedf83e959)

**Rationale:**
> Xác nhận một Checkout chỉ có tối đa một thiệp và việc thay thiệp không làm mất History cũ.

---

## ST-042-04-01 — Tái sử dụng thiệp đã từng liên kết Order cho Checkout mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec81384d-2497-4498-bbd0-011d4022d0f2) |
| **Story** | STORY-042 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng có thiệp từng liên kết với Order trước đó.

**Steps:**
1. Khách hàng mở Checkout mới.
2. Khách hàng chọn **"Chọn thiệp từ lịch sử"**.
3. Khách hàng chọn thiệp đã từng được dùng cho Order.
4. Khách hàng kiểm tra Checkout.

**Test Data:**
- Checkout mới `CHK-002` thuộc khách hàng A.
- Thiệp `CARD-003` thuộc khách hàng A.
- `CARD-003` từng liên kết với:
  - `ORD-001`.
  - `ORD-002`.
- File ảnh `CARD-003` còn khả dụng.

**Expected Result:**
- Khách hàng vẫn chọn được `CARD-003` cho `CHK-002`.
- Các Order và liên kết trước đó không bị thay đổi.
- Hệ thống không tạo History item mới.

**Trace to:**
- [STORY-042/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-112](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e3417531-fad6-4974-8c70-2c26555684f5)

**Rationale:**
> Xác nhận thiệp đã từng được sử dụng vẫn có thể được tái sử dụng cho Checkout mới.

---

## ST-042-05-01 — Template nguồn ngừng khả dụng không chặn chọn lại thiệp đã tạo

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/04090b9e-cd86-47c0-9d6d-1709f53eb3e1) |
| **Story** | STORY-042 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thiệp đã được generate thành công trước khi Template ngừng khả dụng.

**Steps:**
1. Khách hàng mở **"Chọn thiệp từ lịch sử"**.
2. Khách hàng chọn thiệp có Template nguồn đã ngừng khả dụng.
3. Khách hàng kiểm tra Checkout.

**Test Data:**
- Thiệp `CARD-004` thuộc khách hàng A.
- File ảnh của `CARD-004` còn khả dụng.
- Template nguồn `TPL-010` của thiệp đang ở trạng thái không còn khả dụng.

**Expected Result:**
- Khách hàng vẫn chọn được `CARD-004` cho Checkout.
- Thiệp được hiển thị bình thường trong Order Summary.
- Việc Template ngừng khả dụng không chặn thao tác chọn lại.

**Trace to:**
- [STORY-042/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-115](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f4478f91-dd86-4d5a-a825-ea423e0480d8)

**Rationale:**
> Xác nhận trạng thái Template nguồn không làm mất quyền sử dụng lại ảnh thiệp đã tạo thành công.

---

## ST-042-06-01 — Gỡ thiệp khỏi Checkout và tiếp tục Checkout không có thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d9571dd-c9a1-42d0-9143-f9b8edceb80a) |
| **Story** | STORY-042 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout đang có thiệp được chọn.

**Steps:**
1. Khách hàng chọn bỏ thiệp hiện tại khỏi Checkout.
2. Khách hàng kiểm tra Order Summary.
3. Khách hàng tiếp tục Checkout.

**Test Data:**
- Checkout `CHK-001` đang chọn `CARD-001`.
- `CARD-001` có History record hợp lệ.

**Expected Result:**
- `CARD-001` được gỡ khỏi Checkout.
- History của `CARD-001` vẫn còn.
- Khách hàng vẫn có thể tiếp tục Checkout không có thiệp.

**Trace to:**
- [STORY-042/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-117](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f6b23f3-7321-4b92-a7e6-1ccedf83e959)

**Rationale:**
> Xác nhận khách hàng có thể tiếp tục Checkout không có thiệp mà không làm mất History.

---

## ST-042-07-01 — Empty state khi không có thiệp có thể chọn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e903be7c-4e22-4647-93e5-fe61b904db1c) |
| **Story** | STORY-042 |
| **Loại** | 4 |
| **Suite** | REGRESSION |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ.

**Steps:**
1. Khách hàng mở **"Chọn thiệp từ lịch sử"**.
2. Khách hàng kiểm tra màn hình.

**Test Data:**
- Checkout `CHK-001` hợp lệ.
- Khách hàng không có History item nào có ảnh hợp lệ.
- Có thể có History record không có ảnh hoặc file đã không còn khả dụng.

**Expected Result:**
- Hệ thống hiển thị empty state.
- Khách hàng có thể quay lại tạo thiệp mới hoặc tiếp tục Checkout không có thiệp.

**Trace to:**
- [STORY-042/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

**Rationale:**
> Xác nhận khách hàng nhận được empty state phù hợp khi không có thiệp có thể chọn.

---

## ST-042-08-01 — Không chọn được thiệp có file ảnh mất hoặc hỏng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b852e957-36e8-4487-b0b1-cd47e41d6aa2) |
| **Story** | STORY-042 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- History record của thiệp vẫn tồn tại.

**Steps:**
1. Khách hàng mở danh sách History.
2. Khách hàng chọn thiệp có file ảnh bị mất hoặc hỏng.
3. Khách hàng kiểm tra Checkout.

**Test Data:**
- Thiệp `CARD-005` thuộc khách hàng A.
- History record của `CARD-005` còn tồn tại.
- File ảnh của `CARD-005` đã bị mất hoặc không thể truy cập.
- Checkout `CHK-001` đang chọn `CARD-001`.

**Expected Result:**
- Hệ thống không gắn `CARD-005` vào Checkout.
- Thiệp đang chọn `CARD-001` không bị thay đổi.
- Hệ thống hiển thị **"Ảnh không còn khả dụng."**
- History record của `CARD-005` vẫn được giữ.

**Trace to:**
- [STORY-042/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-112](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e3417531-fad6-4974-8c70-2c26555684f5)

**Rationale:**
> Xác nhận thiệp chỉ được chọn lại khi file ảnh thực tế còn khả dụng.

---

## ST-042-09-01 — Không cho chọn thiệp thuộc khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e080665c-ff29-4058-98ce-62b1573123f6) |
| **Story** | STORY-042 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có hai tài khoản khách hàng khác nhau.

**Steps:**
1. Đăng nhập bằng khách hàng A.
2. Khách hàng A thực hiện chọn thiệp thuộc khách hàng B bằng ID/URL.
3. Khách hàng kiểm tra Checkout.

**Test Data:**
- Khách hàng A có Checkout `CHK-A01`.
- Khách hàng B có thiệp `CARD-B01` với file ảnh hợp lệ.
- `CARD-B01` không thuộc History của khách hàng A.

**Expected Result:**
- Hệ thống không gắn `CARD-B01` vào Checkout của khách hàng A.
- Hệ thống không hiển thị ảnh hoặc metadata nhạy cảm của thiệp.
- Thiệp hiện tại của Checkout, nếu có, không bị thay đổi.

**Trace to:**
- [STORY-042/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)
- [BR-111](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de42c1af-ff3c-46d2-a597-b5c33311de4e)

**Rationale:**
> Xác nhận khách hàng không thể sử dụng thiệp thuộc tài khoản khác cho Checkout của mình.

---

## ST-042-10-01 — Không cho chọn thiệp cho Checkout không hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2cd4bbc3-eb06-498c-8331-c9b8b490083b) |
| **Story** | STORY-042 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng thực hiện chọn thiệp cho Checkout không hợp lệ.
2. Khách hàng kiểm tra kết quả.

**Test Data:**
- 01 Checkout không tồn tại.
- 01 Checkout tồn tại nhưng thuộc khách hàng B.
- 01 Checkout thuộc khách hàng A nhưng đã hoàn tất.
- 01 thiệp hợp lệ thuộc khách hàng A để thực hiện thao tác chọn.

**Expected Result:**
- Hệ thống không gắn thiệp vào các Checkout không hợp lệ.
- Hệ thống không thay đổi thiệp đang chọn trước đó.
- Hệ thống không làm thay đổi History hoặc quota.

**Trace to:**
- [STORY-042/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

**Rationale:**
> Xác nhận thao tác chọn thiệp chỉ được thực hiện trên Checkout hợp lệ của khách hàng.

---

## ST-042-11-01 — Lỗi tải History không làm mất trạng thái Checkout hiện tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/408b873b-d8d8-44e4-ac0e-1400d98789b9) |
| **Story** | STORY-042 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ và đang có thiệp được chọn.

**Steps:**
1. Khách hàng mở **"Chọn thiệp từ lịch sử"** khi xảy ra lỗi tải dữ liệu.
2. Khách hàng kiểm tra trạng thái lỗi.
3. Sau khi lỗi được khắc phục, khách hàng chọn tải lại.

**Test Data:**
- Checkout `CHK-001` đang chọn `CARD-001`.
- Có thể mô phỏng lỗi không tải được danh sách History.
- Sau khi lỗi được khắc phục, History có `CARD-001`, `CARD-002`, `CARD-003`.

**Expected Result:**
- Hệ thống hiển thị error state.
- `CARD-001` vẫn là thiệp đang chọn của Checkout.
- Hệ thống có chức năng tải lại.
- Sau khi tải lại thành công, danh sách History hiển thị bình thường.

**Trace to:**
- [STORY-042/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

**Rationale:**
> Xác nhận lỗi tải History không làm mất trạng thái Checkout hiện tại và khách hàng có thể thử lại.

---

## ST-042-12-01 — Request chọn thiệp bị gửi lặp vẫn chỉ chọn một thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-042-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/30f42586-f987-4efa-a7c7-201a1012fb2f) |
| **Story** | STORY-042 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout và thiệp đều hợp lệ.

**Steps:**
1. Khách hàng chọn một thiệp cho Checkout.
2. Cùng thao tác chọn thiệp bị gửi lặp lại.
3. Khách hàng kiểm tra Checkout, History và quota.

**Test Data:**
- Checkout `CHK-001` chưa có thiệp.
- Thiệp `CARD-001` thuộc khách hàng A và có file ảnh hợp lệ.
- Quota AI trước thao tác: `2/3`.
- Số History item trước thao tác: `5`.
- Mô phỏng cùng request chọn `CARD-001` được gửi 2–3 lần.

**Expected Result:**
- Checkout chỉ có một `CARD-001` đang được chọn.
- Hệ thống không tạo nhiều liên kết tạm cho cùng thiệp.
- Số History item vẫn là `5`.
- Quota AI vẫn là `2/3`.

**Trace to:**
- [STORY-042/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

**Rationale:**
> Xác nhận thao tác chọn thiệp được xử lý nhất quán khi cùng một yêu cầu bị gửi nhiều lần.
