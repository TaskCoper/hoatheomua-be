# ST — STORY-041 — Tải xuống thiệp AI — System Tests

---

## ST-041-01-01 — Tải xuống thiệp AI từ History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/24a38a2f-8cc1-42db-a78c-47bf381d9ee1) |
| **Story** | STORY-041 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Khách hàng có thiệp AI đã tạo thành công.

**Steps:**
1. Khách hàng mở Chi tiết thiệp từ History.
2. Khách hàng chọn **"Tải xuống thiệp"**.
3. Khách hàng kiểm tra file được tải xuống.

**Test Data:**
- Có 01 thiệp mã `CARD-001`.
- File ảnh PNG của thiệp còn khả dụng.

**Expected Result:**
- Hệ thống tải đúng file ảnh đã tạo trước đó.
- Tên file theo dạng `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`.

**Trace to:**
- [STORY-041/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [STORY-041/BR-103](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [STORY-041/BR-104](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)

**Rationale:**
> Xác nhận khách hàng có thể tải đúng file thiệp đã tạo từ History với tên file đúng quy tắc.

---

## ST-041-02-01 — Tải thiệp từ Order dùng đúng mã Order đang xem

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5707d0ad-e769-49ae-abdf-9a7cf486f7f8) |
| **Story** | STORY-041 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng có Order liên kết với thiệp AI.

**Steps:**
1. Khách hàng mở Order có liên kết với thiệp.
2. Khách hàng chọn tải thiệp.
3. Khách hàng kiểm tra file được tải xuống.

**Test Data:**
- Có 01 thiệp liên kết với Order `ORD-001` và `ORD-002`.
- Khách hàng thực hiện tải từ `ORD-001`.

**Expected Result:**
- Hệ thống tải đúng file ảnh của thiệp.
- Tên file sử dụng mã `ORD-001`.
- Tên file không sử dụng mã của Order khác.

**Trace to:**
- [STORY-041/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [STORY-041/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [BR-101](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d63586ff-117e-4ab3-9df5-762b91a601ad)
- [BR-102](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7d5b6967-1ae9-40e3-a1ca-a0133c0d73dc)
- [BR-104](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2d92e46f-82a0-4e33-89c4-f07a30ad3b17)
- [BR-109](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c92b89-ea26-4783-bc04-5ed60e84d11b)

**Rationale:**
> Xác nhận khách hàng tải đúng thiệp từ Order và tên file sử dụng đúng mã Order đang xem.

---

## ST-041-03-01 — Tải lại cùng một thiệp nhiều lần không ảnh hưởng quota hoặc History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/39a0f15d-1ad8-488a-8b82-eb61ed66cab6) |
| **Story** | STORY-041 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng có thiệp với file còn khả dụng.

**Steps:**
1. Khách hàng tải cùng một thiệp nhiều lần.
2. Khách hàng kiểm tra file sau mỗi lần tải.
3. Khách hàng kiểm tra quota và History sau khi tải.

**Test Data:**
- Có 01 thiệp đã tạo thành công.
- Khách hàng thực hiện tải 3 lần.
- Khách hàng có thể còn hoặc hết quota AI.

**Expected Result:**
- Mỗi lần đều tải được đúng file đã lưu.
- Hệ thống không giới hạn số lần tải.
- Hệ thống không tạo ảnh mới.
- Hệ thống không tạo History item mới.
- Quota AI không thay đổi.
- Quota AI không ảnh hưởng quyền tải file.

**Trace to:**
- [STORY-041/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [STORY-041/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [STORY-041/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [BR-101](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d63586ff-117e-4ab3-9df5-762b91a601ad)
- [BR-105](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ace93ff9-8975-48dd-93a8-5a98dbc3d52e)
- [BR-106](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/dcc6b70e-c0bd-4b31-b75c-5c3eb5ce7fd3)

**Rationale:**
> Xác nhận tải xuống chỉ sử dụng lại file hiện có, không giới hạn số lần và không ảnh hưởng quota hoặc History.

---

## ST-041-04-01 — File ảnh thiệp không còn khả dụng vẫn giữ History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c30bc704-f168-4bee-8a28-84ee2ef15705) |
| **Story** | STORY-041 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- History của thiệp còn tồn tại.

**Steps:**
1. Khách hàng mở Chi tiết thiệp có file ảnh không còn khả dụng.
2. Khách hàng thực hiện tải xuống.

**Test Data:**
- Có 01 thiệp từng tạo ảnh thành công nhưng file hiện bị mất hoặc không truy cập được.

**Expected Result:**
- History của thiệp vẫn được giữ.
- Hệ thống hiển thị **"Ảnh không còn khả dụng"**.
- Hệ thống không tải file.
- Hệ thống không tự tạo lại ảnh.

**Trace to:**
- [STORY-041/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [BR-107](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec5f4d36-7e58-4bb2-8061-cbb0b4a741c4)

**Rationale:**
> Xác nhận khách hàng nhận được trạng thái phù hợp khi file ảnh mất nhưng History của thiệp vẫn được bảo toàn.

---

## ST-041-05-01 — Thiệp không tồn tại thì không cung cấp file thay thế

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5a40e5b4-59dc-4270-86c1-216be8c1b30b) |
| **Story** | STORY-041 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng truy cập chức năng tải bằng thiệp không còn tồn tại.
2. Khách hàng kiểm tra kết quả.

**Test Data:**
- Có 01 mã/URL thiệp không tồn tại.

**Expected Result:**
- Hệ thống không thực hiện tải file.
- Hệ thống không gọi AI tạo file thay thế.
- Hệ thống hiển thị trạng thái thiệp không tồn tại hoặc không thể truy cập.

**Trace to:**
- [STORY-041/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)

**Rationale:**
> Xác nhận hệ thống không cung cấp file hoặc tạo file thay thế khi thiệp được yêu cầu không tồn tại.

---

## ST-041-06-01 — Không cho tải thiệp thuộc khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3097a6d9-9178-45e3-8fbd-516e5d1a5d36) |
| **Story** | STORY-041 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có hai tài khoản khách hàng khác nhau.

**Steps:**
1. Đăng nhập bằng khách hàng A.
2. Truy cập tải thiệp thuộc khách hàng B.

**Test Data:**
- Khách hàng B có 01 thiệp AI đã tạo thành công.

**Expected Result:**
- Khách hàng A không tải được thiệp của khách hàng B.
- Hệ thống không hiển thị file của thiệp.
- Hệ thống không cung cấp file của thiệp.
- Hệ thống không cung cấp thông tin nhạy cảm của thiệp.

**Trace to:**
- [STORY-041/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [BR-108](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f1950c70-8305-42a7-9b0d-7674e8b71f8d)

**Rationale:**
> Xác nhận khách hàng chỉ có thể tải thiệp thuộc tài khoản của mình.

---

## ST-041-07-01 — Không cho tải thiệp từ Order không liên kết với thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e83d2ff8-cfc2-4825-9118-f8791a39c9b8) |
| **Story** | STORY-041 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng có thiệp và Order.

**Steps:**
1. Khách hàng thực hiện tải thiệp trong ngữ cảnh một Order không liên kết với thiệp.
2. Khách hàng kiểm tra kết quả.

**Test Data:**
- Thiệp: `CARD-001`.
- Order: `ORD-001`.
- `CARD-001` và `ORD-001` không có liên kết với nhau.

**Expected Result:**
- Hệ thống không tải file theo ngữ cảnh Order đó.
- Hệ thống không sử dụng mã Order để đặt tên file.
- Hệ thống hiển thị thông báo không thể tải thiệp từ Order này.

**Trace to:**
- [STORY-041/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)
- [BR-102](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7d5b6967-1ae9-40e3-a1ca-a0133c0d73dc)
- [BR-109](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c92b89-ea26-4783-bc04-5ed60e84d11b)

**Rationale:**
> Xác nhận chỉ Order có liên kết hợp lệ với thiệp mới được sử dụng làm ngữ cảnh tải xuống.

---

## ST-041-08-01 — Lỗi cung cấp file hiển thị thông báo và cho thử lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/631bcb0b-5077-4f1b-9d37-65b017459404) |
| **Story** | STORY-041 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thiệp và file hợp lệ.

**Steps:**
1. Khách hàng chọn **"Tải xuống thiệp"** khi xảy ra lỗi cung cấp file.
2. Khách hàng kiểm tra thông báo.
3. Sau khi lỗi được khắc phục, khách hàng thực hiện tải lại.

**Test Data:**
- Có thể mô phỏng lỗi trong quá trình đọc hoặc truyền file.

**Expected Result:**
- Hệ thống hiển thị thông báo tải thất bại.
- Hệ thống cho phép khách hàng thử lại.
- Hệ thống không coi file lỗi là tải thành công.
- Quota, History, thiệp và Order không thay đổi.
- Sau khi thử lại thành công, file được tải bình thường.

**Trace to:**
- [STORY-041/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)

**Rationale:**
> Xác nhận khách hàng được thông báo và có thể thử lại khi quá trình cung cấp file gặp lỗi.

---

## ST-041-09-01 — Tên file tải xuống xử lý ký tự không an toàn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-041-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1dbc61aa-f286-4ae7-9150-1201263c76c4) |
| **Story** | STORY-041 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng có thiệp hợp lệ và file còn khả dụng.

**Steps:**
1. Khách hàng thực hiện tải thiệp.
2. Khách hàng kiểm tra tên file được tải xuống.

**Test Data:**
- Mã thiệp hoặc mã Order có chứa ký tự không an toàn cho tên file.

**Expected Result:**
- Ký tự không an toàn được loại bỏ hoặc thay thế.
- File tải xuống vẫn có phần mở rộng `.png`.

**Trace to:**
- [STORY-041/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44ac11ef-d8aa-4524-bfa7-6adaa0a5c12c)

**Rationale:**
> Xác nhận tên file tải xuống vẫn hợp lệ khi mã thiệp hoặc mã Order chứa ký tự không an toàn.
