<!-- # UT-032: Customer chọn thiệp từ History cho Checkout

Các case kiểm tra API danh sách thiệp của TDD-032. Thiệp thuộc Combo Checkout khi lịch sử của nó có cùng `root` — Combo nguồn ban đầu.

---

## UT-032-01: Hiển thị thiệp tạo trực tiếp từ Combo

## Thông tin tài liệu

- **Tiêu đề**: Customer thấy thiệp được tạo trực tiếp từ Combo đang Checkout.
- **Ghi chú**: Thiệp có cùng Combo nguồn với Checkout.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-01
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Customer đang đăng nhập là `customer-001`; Combo đang Checkout là `combo-001`.
  - Lịch sử `history-card-001` của customer có `Root=combo-001`, trỏ tới `card-001` có `imageUrl` đã lưu.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200. Danh sách có card-001 vì thiệp bắt đầu từ đúng Combo đang Checkout.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận customer có thể xem thiệp của đúng Combo nguồn.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-001
- **Ghi chú**: Hiển thị thiệp thuộc khách hàng và đúng mẫu hoa Checkout.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Card hợp lệ cùng root
- **Ghi chú**:

---

## UT-032-02: Hiển thị thiệp tạo từ hoa của Combo

## Thông tin tài liệu

- **Tiêu đề**: Customer thấy thiệp tạo từ hoa thuộc Combo đang Checkout.
- **Ghi chú**: `root` gom đúng thiệp dù nguồn trực tiếp của thiệp là hoa.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-02
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Customer `customer-001` đang Checkout `combo-001`.
  - Thiệp `card-002` được tạo từ hoa `flower-001`; lịch sử thiệp có `Root=combo-001` và `imageUrl` đã lưu.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200. Danh sách có card-002; việc thiệp được tạo từ hoa không làm mất liên hệ với Combo nguồn.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Tránh bỏ sót thiệp không được tạo trực tiếp từ Combo.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: BƯỚC 2 - Lấy thiệp thuộc Combo nguồn
- **Ghi chú**:

---

## UT-032-03: Hiển thị thiệp đã được tạo lại

## Thông tin tài liệu

- **Tiêu đề**: Customer vẫn thấy thiệp sau khi tạo lại từ cùng Combo nguồn.
- **Ghi chú**: Thiệp mới kế thừa `root=combo-001`.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-03
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Customer `customer-001` đang Checkout `combo-001`.
  - Thiệp `card-003` là kết quả tạo lại từ một thiệp hoặc hoa trước đó, có `Root=combo-001` và `imageUrl` đã lưu.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200. Danh sách có card-003 vì Combo nguồn không thay đổi khi tạo lại.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận một chuỗi tạo lại vẫn được gom về đúng Combo ban đầu.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Thiệp tạo lại
- **Ghi chú**:

---

## UT-032-04: Không hiển thị thiệp của Combo khác hoặc người khác

## Thông tin tài liệu

- **Tiêu đề**: Danh sách chỉ có thiệp phù hợp với customer và Combo hiện tại.
- **Ghi chú**: Bảo vệ dữ liệu giữa customer và giữa các Combo.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-04
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Customer hiện tại là `customer-001`, Combo là `combo-001`.
  - `card-valid` thuộc `customer-001`, có `Root=combo-001`.
  - `card-other-combo` thuộc `customer-001`, có `Root=combo-002`.
  - `card-other-customer` có `Root=combo-001` nhưng thuộc `customer-002`.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200. Chỉ có card-valid; hai thiệp còn lại không xuất hiện.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Customer chỉ được xem thiệp của chính mình và đúng Combo Checkout.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-008
- **Ghi chú**: Ownership.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Card thuộc Combo khác hoặc customer khác
- **Ghi chú**:

---

## UT-032-05: Vẫn hiển thị thiệp đã từng đặt hàng

## Thông tin tài liệu

- **Tiêu đề**: Thiệp đã dùng trong Order vẫn có thể được chọn lại.
- **Ghi chú**: Lịch sử hợp lệ không bị loại vì đã từng đặt hàng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-05
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Thiệp `card-ordered` thuộc `customer-001`, có `Root=combo-001`, có `imageUrl` đã lưu và đã liên kết với một Order trước đó.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200. Danh sách vẫn có card-ordered.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Đảm bảo customer được tái sử dụng thiệp đã từng đặt hàng.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Card đã dùng cho Order
- **Ghi chú**:

---

## UT-032-06: Trả danh sách rỗng khi chưa có thiệp phù hợp

## Thông tin tài liệu

- **Tiêu đề**: Customer nhận danh sách rỗng khi Combo chưa có thiệp phù hợp.
- **Ghi chú**: Đây là kết quả bình thường, không phải lỗi.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-06
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Customer `customer-001` không có thiệp nào có `Root=combo-empty`.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-empty&pageIndex=1&pageSize=20
  ```
- **Expected output**:
  ```text
  HTTP 200, items rỗng và totalCount = 0.
  ```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Giao diện cần phân biệt “chưa có thiệp” với lỗi tải dữ liệu.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-042
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: API - Ví dụ 2
- **Ghi chú**:

---

## UT-032-07: Sắp xếp và chia trang ổn định

## Thông tin tài liệu

- **Tiêu đề**: Thiệp mới hơn hiển thị trước và đúng trang được yêu cầu.
- **Ghi chú**: Kết quả giữ thứ tự ổn định khi mở lại danh sách.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-07
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Determinism
- **Precondition / Mock setup**:
  - Customer có năm thiệp cùng `Root=combo-001` với thời điểm tạo khác nhau.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=2&pageSize=2
  ```
- **Expected output**:
  ```text
  HTTP 200. Có đúng hai thiệp ở trang 2; thứ tự toàn bộ danh sách là mới nhất trước và tổng số là 5.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Customer có thể tìm lại thiệp mà không bị đổi thứ tự giữa các lần xem.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Hiển thị thiệp mới tạo trước
- **Ghi chú**:

---

## UT-032-08: Tự dùng phân trang mặc định khi giá trị không phù hợp

## Thông tin tài liệu

- **Tiêu đề**: Giá trị phân trang không phù hợp vẫn trả được danh sách.
- **Ghi chú**: Không trả lỗi validate; hệ thống dùng giá trị mặc định theo code.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-032-08
- **Phiên bản**: v0.2
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module**: CARD HISTORY - Chọn thiệp cho Checkout
- **Unit under test**: CardHistoryCheckout.Service.GetEligibleCards
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Customer có các thiệp hợp lệ với `Root=combo-001`.
- **Input**:
  ```http
  GET /api/v1/card-histories/eligible?comboId=combo-001&pageIndex=-1&pageSize=0
  ```
- **Expected output**:
  ```text
  HTTP 200. Hệ thống thay giá trị trang và số lượng không phù hợp bằng giá trị mặc định đã cấu hình, rồi trả danh sách theo các giá trị đó.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Trải nghiệm xem History không bị gián đoạn bởi giá trị phân trang ngoài khoảng.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-032
- **Section**: Business Rules - Phân trang mặc định
- **Ghi chú**: -->

