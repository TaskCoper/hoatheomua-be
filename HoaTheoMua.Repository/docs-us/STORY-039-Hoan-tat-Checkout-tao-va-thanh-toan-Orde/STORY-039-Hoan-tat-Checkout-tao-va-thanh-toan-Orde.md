# STORY-039 — Hoàn tất Checkout, tạo và thanh toán Order

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã hoàn thành thông tin Checkout, tôi muốn tạo Order và thanh toán, để cửa hàng tiếp nhận và xử lý đơn hoa của tôi. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

- Order chỉ được tạo khi khách hàng bấm **"Hoàn tất"** và backend xác nhận dữ liệu, giá và tồn kho hợp lệ.
- Order mới được tạo ở trạng thái **PENDING** và chuyển khách hàng tới màn Chi tiết đơn hàng có nút **"Thanh toán ngay"**.
- Order tiếp tục **PENDING** khi khách hàng chưa bấm thanh toán hoặc thanh toán thất bại.
- Khách hàng được thử thanh toán lại **không giới hạn** trong vòng **24 giờ** kể từ lúc Order được tạo.
- Sau **24 giờ** chưa thanh toán thành công, hệ thống tự động chuyển Order sang **CANCELLED**.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout hợp lệ, có đúng **01 mẫu hoa** và địa chỉ giao hàng hợp lệ.
- Core của Combo còn hàng.
- Nếu có thiệp, thiệp thuộc khách hàng, đã được khách hàng **Xác nhận** và là thiệp đang chọn của Checkout.

### Trigger
> Khách hàng chọn **"Hoàn tất"**.

---

## Flow

### MF-01 — Tạo Order

1. Backend kiểm tra lại ownership, dữ liệu Checkout, mẫu hoa, giá và tồn kho.
2. Core hết hàng thì **dừng flow**.
3. Nếu Core còn nhưng một hoặc nhiều thành phần Support hết, hệ thống vẫn cho phép tạo Order, hiển thị thông báo cho khách hàng và ghi nhận các thành phần cần thay thế để cửa hàng chủ động liên hệ.
4. Hệ thống tính phí giao hàng và **tổng bill cuối cùng**.
5. Hệ thống tạo Order ở trạng thái **PENDING**.
6. Nếu Checkout có thiệp đã được khách hàng Xác nhận và đang chọn, hệ thống tạo **liên kết Thiệp–Order** và tính giá thiệp đó vào bill.
7. Nếu sử dụng Custom AI, hệ thống đánh dấu kết quả mẫu hoa đã được dùng để tạo Order.
8. Hệ thống chuyển tới **Chi tiết đơn hàng** và hiển thị **"Thanh toán ngay"**.

### MF-02 — Thanh toán

9. Khách hàng chọn **"Thanh toán ngay"** khi Order còn PENDING và chưa quá 24 giờ.
10. Hệ thống khởi tạo giao dịch thanh toán.
11. Nếu thanh toán thành công, hệ thống cập nhật Order từ **PENDING** sang **PAID**.
12. Nếu thanh toán thất bại, Order vẫn **PENDING** và khách hàng có thể thử lại.
13. Mỗi lần thanh toán phải gắn với đúng Order và có **mã giao dịch riêng**.

### MF-03 — Tự động hủy

14. Hệ thống xác định Order đã ở trạng thái PENDING đủ **24 giờ** kể từ thời điểm tạo.
15. Nếu chưa có thanh toán thành công, hệ thống chuyển Order sang **CANCELLED**.
16. Hệ thống giải phóng toàn bộ tồn kho Combo đã giữ cho Order.
17. Order vẫn được lưu trong lịch sử đơn hàng với trạng thái **"Đã hủy"**.
18. Mẫu hoa Custom AI vẫn được giữ trong History và tiếp tục liên kết với Order đã hủy.
19. Order **CANCELLED** không được tiếp tục thanh toán.

---

### Alternative Flows

#### ALT-01 — Đặt hàng không kèm thiệp
1. Khách hàng hoàn tất Checkout mà không chọn hoặc tạo thiệp.
2. Hệ thống kiểm tra các thông tin bắt buộc còn lại của Checkout.
3. Nếu Checkout hợp lệ và thành phần Core còn đủ hàng, hệ thống vẫn tạo Order.
4. Order **không được liên kết** với thiệp.
5. Bill không hiển thị dòng giá thiệp.
6. Tổng tiền của Order **không bao gồm** giá thiệp.
7. Khách hàng tiếp tục thực hiện thanh toán theo phương thức đã chọn.

#### ALT-02 — Thành phần Support không còn đủ hàng
1. Khách hàng ấn chọn **"Hoàn tất"**.
2. Hệ thống kiểm tra lại tồn kho của các thành phần thuộc Combo.
3. Hệ thống xác định: Core vẫn còn đủ hàng, một hoặc nhiều Support không còn đủ hàng.
4. Hệ thống **vẫn cho phép** tạo Order.
5. Hệ thống ghi nhận các thành phần Support cần được thay thế bằng thành phần tương đương.
6. Việc xác định thành phần Support thay thế được thực hiện sau khi Staff liên hệ và thống nhất với khách hàng, **không phải tại thời điểm tạo Order**.
7. Hệ thống hiển thị thông báo cho khách hàng và cửa hàng chủ động liên hệ để thống nhất thành phần thay thế trước khi thực hiện đơn.
8. Bill và Order vẫn được tạo theo thông tin, giá và phí đã xác định tại thời điểm hoàn tất Checkout.
9. Việc Support hết hàng **không tự động làm thay đổi** thành phần Core của Combo.

#### ALT-03 — Thanh toán lại Order đang PENDING
1. Một Order đã được tạo nhưng giao dịch thanh toán trước đó không thành công.
2. Order đang ở trạng thái **PENDING** và chưa vượt quá thời hạn thanh toán 24 giờ.
3. Khách hàng chọn **"Thanh toán lại"**.
4. Backend kiểm tra: Order thuộc khách hàng hiện tại, Order vẫn ở trạng thái PENDING, Order chưa vượt quá thời hạn thanh toán.
5. Hệ thống khởi tạo một lần thanh toán mới cho **cùng Order**, không tạo Order mới.
6. Khách hàng được phép thử thanh toán lại **không giới hạn** số lần trong thời hạn 24 giờ.
7. Mỗi lần thanh toán thất bại **không làm thay đổi** hoặc kéo dài thời điểm hết hạn ban đầu của Order.
8. Nếu giao dịch thành công, hệ thống cập nhật trạng thái Order theo luồng thanh toán thành công.

#### ALT-04 — Chốt bill tại thời điểm hoàn tất Checkout
1. Khách hàng đã cung cấp đầy đủ thông tin Checkout và ấn chọn **"Hoàn tất"**.
2. Hệ thống kiểm tra lại các dữ liệu cần thiết để tính bill: Giá Combo, giá thiệp (nếu có), phí giao hàng theo địa chỉ giao hàng đã chọn, các khoản phí hoặc giảm giá hợp lệ khác (nếu có).
3. Hệ thống tính **tổng tiền lần cuối** tại thời điểm hoàn tất Checkout.
4. Hệ thống tạo Order và lưu **snapshot** của toàn bộ dữ liệu dùng để tính bill.
5. Bill được **gắn cố định** với Order ngay sau khi Order được tạo.
6. Mọi thay đổi về giá Combo, giá thiệp, phí giao hàng hoặc chi phí thực tế phát sinh sau thời điểm tạo Order **không làm thay đổi bill**.
7. Nếu chi phí thực tế sau đó cao hơn số tiền đã chốt, **phần chênh lệch do cửa hàng chịu**.
8. Hệ thống không yêu cầu khách hàng thanh toán bổ sung cho phần chênh lệch phát sinh sau khi Order được tạo.

---

### Exception Flows

#### EXC-01 — Thành phần Core hết hàng tại thời điểm hoàn tất Checkout
1. Khách hàng ấn chọn **"Hoàn tất"**.
2. Hệ thống kiểm tra lại tồn kho của các thành phần Core ngay trước khi tạo Order.
3. Hệ thống phát hiện ít nhất một thành phần Core không còn đủ số lượng cần thiết.
4. Hệ thống **không tạo Order**, không tạo bill cuối cùng hoặc yêu cầu thanh toán.
5. Hệ thống không tạo liên kết giữa thiệp và Order, nếu Checkout có thiệp.
6. Hệ thống giữ lại thông tin Checkout mà khách hàng đã nhập nếu dữ liệu vẫn còn hợp lệ.
7. Hệ thống hiển thị thông báo: **"Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn sản phẩm khác."**
8. Khách hàng có thể quay lại để chọn mẫu hoa hoặc Combo khác.

#### EXC-02 — Tạo Order thất bại
1. Checkout đã có đầy đủ dữ liệu hợp lệ, Core còn đủ hàng và Checkout đáp ứng điều kiện tạo Order.
2. Hệ thống gặp lỗi trong quá trình tạo Order.
3. Toàn bộ thao tác tạo Order phải được **hoàn tác**.
4. Hệ thống không được để lại Order thiếu dữ liệu hoặc Order được tạo một phần.
5. Hệ thống không tạo hoặc phải hoàn tác các dữ liệu liên quan chưa hoàn chỉnh: bill, liên kết thiệp–Order, yêu cầu thanh toán, các thay đổi trạng thái phát sinh.
6. Hệ thống giữ lại Checkout và thông tin khách hàng đã nhập nếu có thể.
7. Hệ thống hiển thị thông báo: **"Không thể tạo đơn hàng. Vui lòng thử lại."**
8. Nếu khách hàng thử lại, hệ thống phải kiểm tra để **không tạo trùng Order** từ cùng một lần xác nhận.

#### EXC-03 — Thanh toán thất bại
1. Order đã được tạo thành công và đang trong thời hạn thanh toán.
2. Khách hàng thực hiện thanh toán.
3. Cổng thanh toán trả về kết quả thất bại hoặc giao dịch không được xác nhận thành công.
4. Hệ thống không tạo Order mới.
5. Hệ thống giữ Order hiện tại ở trạng thái **PENDING**.
6. Hệ thống ghi nhận lần thanh toán thất bại phục vụ việc đối soát.
7. Hệ thống **không làm thay đổi** thời điểm hết hạn 24 giờ đã xác định khi Order được tạo.
8. Hệ thống hiển thị thông báo: **"Thanh toán chưa thành công. Bạn có thể thử lại trong thời hạn thanh toán của đơn hàng."**
9. Khách hàng có thể chọn **"Thanh toán lại"** nếu Order vẫn còn trong thời hạn.

#### EXC-04 — Thanh toán Order đã hết hạn hoặc bị hủy
1. Khách hàng gửi yêu cầu thanh toán hoặc thanh toán lại.
2. Backend kiểm tra trạng thái và thời điểm hết hạn của Order.
3. Hệ thống phát hiện Order đã ở trạng thái **CANCELLED** hoặc đã vượt quá 24 giờ.
4. Backend từ chối khởi tạo giao dịch thanh toán mới.
5. Hệ thống không cập nhật Order thành trạng thái đã thanh toán, không tạo Order thay thế.
6. Nếu Order đã quá hạn nhưng chưa được cập nhật trạng thái, hệ thống chuyển Order sang **CANCELLED** theo quy tắc hết hạn.
7. Hệ thống hiển thị thông báo: **"Đơn hàng đã hết thời hạn thanh toán và không thể tiếp tục thanh toán."**
8. Khách hàng phải thực hiện lại quy trình đặt hàng nếu vẫn muốn mua sản phẩm.

#### EXC-05 — Cửa hàng không thể đáp ứng Order sau khi đặt
1. Order đã được tạo thành công.
2. Sau khi kiểm tra hoặc chuẩn bị đơn hàng, cửa hàng phát hiện không thể đáp ứng Order.
3. Hệ thống **không tự động** chuyển Order sang REFUNDED.
4. Cửa hàng hoặc Admin kiểm tra: trạng thái thanh toán, lý do không thể đáp ứng, số tiền khách hàng đã thanh toán.
5. Nếu Order chưa thanh toán, Admin xử lý hủy Order theo nghiệp vụ hủy đơn.
6. Nếu Order đã thanh toán, Admin quyết định và thực hiện quy trình hoàn tiền.
7. Chỉ sau khi nghiệp vụ hoàn tiền được xác nhận thành công, Admin mới chuyển Order sang **REFUNDED**.
8. Hệ thống phải lưu người thực hiện, thời điểm và lý do hủy hoặc hoàn tiền.
9. Khách hàng được thông báo về kết quả xử lý Order.

#### EXC-06 — Khách hàng không có quyền thao tác với Order
1. Khách hàng gửi request xem, thanh toán hoặc thanh toán lại một Order.
2. Backend kiểm tra quyền sở hữu Order.
3. Hệ thống phát hiện Order **không thuộc** khách hàng hiện tại.
4. Backend từ chối request, không tạo giao dịch thanh toán, không thay đổi trạng thái Order.
5. Backend không trả thông tin bill, địa chỉ giao hàng, thông tin thanh toán hoặc dữ liệu nhạy cảm của Order.
6. Hệ thống hiển thị thông báo: **"Bạn không có quyền thực hiện thao tác này."**

---

## Acceptance Criteria

### AC-001 — Chưa hoàn tất chưa có Order
- **Given:** Khách hàng đang ở Checkout.
- **When:** Chưa bấm **"Hoàn tất"**.
- **Then:** Không tồn tại Order/PENDING.

### AC-002 — Tạo Order PENDING
- **Given:** Checkout và tồn kho hợp lệ.
- **When:** Khách hàng bấm **"Hoàn tất"**.
- **Then:** Hệ thống tạo đúng **01 Order PENDING**.
- **And:** Hiển thị Chi tiết đơn hàng và nút **"Thanh toán ngay"**.

### AC-003 — Thiệp liên kết Order
- **Given:** Checkout có thiệp B đã được khách hàng Xác nhận và đang chọn.
- **When:** Order được tạo.
- **Then:** Tạo record liên kết **Thiệp B–Order**.
- **And:** Chỉ B được tính vào bill.

### AC-004 — Chưa thanh toán
- **Given:** Order vừa tạo.
- **When:** Khách hàng chưa chọn **"Thanh toán ngay"**.
- **Then:** Order vẫn **PENDING**.

### AC-005 — Thanh toán thất bại
- **Given:** Order còn PENDING trong thời hạn.
- **When:** Giao dịch thất bại.
- **Then:** Order vẫn **PENDING**.
- **And:** Khách hàng được thử lại.

### AC-006 — Thử lại không giới hạn
- **Given:** Order còn PENDING và chưa quá 24 giờ.
- **When:** Khách hàng thử thanh toán lại.
- **Then:** Hệ thống **không giới hạn** số lần thử.
- **And:** Không kéo dài thời hạn PENDING.

### AC-007 — Hủy sau 24 giờ
- **Given:** Order PENDING chưa thanh toán thành công.
- **When:** Đủ 24 giờ kể từ lúc tạo.
- **Then:** Hệ thống chuyển Order sang **CANCELLED**.
- **And:** Không cho thanh toán tiếp.

### AC-008 — Core hết
- **Given:** Core hết hàng ở lần kiểm tra cuối.
- **When:** Khách hàng hoàn tất.
- **Then:** Hệ thống **không tạo Order**.

### AC-009 — Support hết
- **Given:** Core còn và Support hết.
- **When:** Khách hàng hoàn tất.
- **Then:** Hệ thống vẫn tạo Order.
- **And:** Thông báo Staff sẽ liên hệ khách hàng để thống nhất thành phần Support thay thế.

### AC-010 — Xử lý khi cửa hàng không thể đáp ứng Order
- **Given:** Order đã được tạo nhưng cửa hàng phát hiện không thể chuẩn bị hoặc giao đơn hàng.
- **When:** Staff Hoa Theo Mùa xử lý Order.
- **Then:** Staff phải liên hệ trực tiếp với khách hàng để thông báo và thống nhất phương án xử lý.
- **And:** Hệ thống không được tự động chuyển Order sang REFUNDED.
- **And:** Nếu Order chưa thanh toán và hai bên thống nhất hủy, Admin chuyển Order sang **CANCELLED**.
- **And:** Nếu Order đã thanh toán và hai bên thống nhất hủy, Admin thực hiện hoàn tiền.
- **And:** Order chỉ được chuyển sang **REFUNDED** sau khi việc hoàn tiền được xác nhận thành công.
- **And:** Hệ thống phải giải phóng tồn kho của Combo đang được giữ cho Order nếu chưa được giải phóng trước đó.
- **And:** Hệ thống phải lưu người thực hiện, thời điểm và lý do hủy hoặc hoàn tiền.
- **And:** Order vẫn được lưu trong lịch sử đơn hàng với trạng thái tương ứng.

### AC-011 — Thanh toán thành công
- **Given:** Order đang PENDING và còn trong thời hạn thanh toán.
- **When:** Giao dịch được xác nhận thành công.
- **Then:** Hệ thống cập nhật Order từ **PENDING** sang **PAID**.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-087** | Thời điểm tạo Order | Hoàn tất Checkout | Order chỉ được tạo sau khi khách hàng bấm "Hoàn tất" và backend xác nhận toàn bộ điều kiện cuối. | Khách hàng bấm "Hoàn tất". | Backend xác nhận toàn bộ điều kiện cuối trước khi tạo Order. | Không tạo Order nếu điều kiện cuối không hợp lệ. | Draft | v0 | 2026-08-14 |
| **BR-088** | Trạng thái ban đầu | Hoàn tất Checkout | Order mới có trạng thái PENDING dù khách hàng chưa bấm thanh toán hoặc lần thanh toán đầu thất bại. | Order mới được tạo. | Hệ thống gán trạng thái **PENDING** cho Order. | Trạng thái PENDING vẫn được giữ khi khách hàng chưa bấm thanh toán hoặc lần thanh toán đầu thất bại. | Draft | v0 | 2026-08-14 |
| **BR-089** | Thời hạn PENDING và giải phóng tồn kho | Hoàn tất Checkout | Order ở trạng thái PENDING được duy trì tối đa 24 giờ kể từ thời điểm tạo. Trong thời gian này, hệ thống giữ tồn kho thực tế của các thành phần đã được ghi nhận cho Order. Support hết hàng được ghi nhận là cần thay thế và không reserve vượt quá tồn kho thực tế. Việc xác định Support thay thế thực hiện sau khi Staff thống nhất với khách hàng. | Order đã ở trạng thái PENDING đủ 24 giờ và chưa có giao dịch thanh toán thành công. | Hệ thống chuyển Order sang **CANCELLED**, giải phóng tồn kho các thành phần của Combo đã giữ cho Order, số lượng giải phóng trở lại thành tồn kho khả dụng. Order vẫn được lưu trong lịch sử với trạng thái "Đã hủy". Mẫu hoa Custom AI vẫn được giữ và tiếp tục liên kết với Order đã hủy. | Việc giải phóng tồn kho chỉ thực hiện đúng một lần cho mỗi Order. Không giải phóng lại nếu tồn kho của Order đã được giải phóng trước đó. Mẫu hoa Custom AI không bị xóa hoặc chuyển về Bản nháp. Order CANCELLED không được tiếp tục thanh toán. Thanh toán lại trước khi hết hạn không làm thay đổi mốc hết hạn ban đầu. | Draft | v0 | 2026-08-14 |
| **BR-090** | Thử lại thanh toán | Hoàn tất Checkout | Khách hàng được thử thanh toán không giới hạn số lần khi Order còn PENDING và chưa hết hạn. | Khách hàng thử thanh toán lại. | Hệ thống cho phép thử thanh toán không giới hạn số lần. | Chỉ áp dụng khi Order còn PENDING và chưa hết hạn. | Draft | v0 | 2026-08-14 |
| **BR-091** | Tự động hủy | Hoàn tất Checkout | Sau 24 giờ chưa thanh toán thành công, hệ thống tự động chuyển Order sang CANCELLED và chặn thanh toán. | Order chưa thanh toán thành công sau 24 giờ. | Hệ thống tự động chuyển Order sang **CANCELLED**. | Order CANCELLED bị chặn thanh toán. | Draft | v0 | 2026-08-14 |
| **BR-092** | Chốt bill | Hoàn tất Checkout | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng tại thời điểm hoàn tất Checkout. | Khách hàng hoàn tất Checkout. | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng. | Bill được chốt tại thời điểm hoàn tất Checkout. | Draft | v0 | 2026-08-14 |
| **BR-093** | Tồn kho | Hoàn tất Checkout | Core hết chặn tạo Order. Support hết không chặn nếu Core còn và cửa hàng thay thế tương ứng. | Backend kiểm tra tồn kho trước khi tạo Order. | Core hết chặn tạo Order. Support hết không chặn nếu Core còn và cửa hàng thay thế tương ứng. | Không tạo Order khi Core hết. | Draft | v0 | 2026-08-14 |
| **BR-094** | Thiệp sau khi tạo Order | Hoàn tất Checkout | Khách hàng không được đổi hoặc gỡ thiệp sau khi Order đã được tạo. | Order đã được tạo. | Hệ thống không cho khách hàng đổi hoặc gỡ thiệp. | Không áp dụng trước thời điểm Order được tạo. | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules
`BR-087` · `BR-088` · `BR-089` · `BR-090` · `BR-091` · `BR-092` · `BR-093` · `BR-094`

### Dependencies
- **STORY-038** — Khởi tạo Checkout từ mẫu hoa

---

## Non-Functional Requirements

- Tạo Order và các liên kết liên quan phải **nhất quán**; không để Order hoặc link bị tạo trùng do double click/retry.
- Backend kiểm tra **trạng thái Order** trước mọi lần thanh toán.
- Job tự hủy phải **idempotent**.
- Không hiển thị lỗi cổng thanh toán hoặc stack trace nội bộ.
- API tạo Order mục tiêu **p95 ≤ 3 giây**, không tính thời gian hệ thống bên thứ ba.
- Event/giao dịch thanh toán phải **truy vết được** theo Order.

---

## Out of Scope

- Admin xử lý fulfillment.
- Chi tiết tích hợp payment provider.
- Chính sách refund tự động.
- Quản trị tồn kho.