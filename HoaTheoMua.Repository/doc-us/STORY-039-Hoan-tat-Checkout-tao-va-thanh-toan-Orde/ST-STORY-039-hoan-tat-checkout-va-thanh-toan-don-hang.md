# ST — STORY-039 — Hoàn tất Checkout và thanh toán đơn hàng — System Tests

---

## ST-039-01-01 — Smoke: Hoàn tất Checkout tạo đơn hàng chờ thanh toán

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9b6d5359-2c0f-4915-889b-06b1c840b38d) |
| **Story** | STORY-039 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ.
- Checkout có đúng 1 mẫu hoa.
- Địa chỉ giao hàng hợp lệ.
- Core của Combo còn đủ hàng.
- Checkout có thiệp `thiệp A` đã được khách hàng Xác nhận và đang chọn.

**Steps:**
1. Khách hàng mở `Checkout A`.
2. Khách hàng quan sát chưa có đơn hàng được tạo.
3. Khách hàng chọn **"Hoàn tất"**.
4. Khách hàng chờ hệ thống xử lý.
5. Khách hàng quan sát màn hình Chi tiết đơn hàng.
6. Khách hàng quan sát đơn hàng, trạng thái và liên kết thiệp.

**Test Data:**
- Checkout: `Checkout A`.
- Mẫu hoa: `mẫu hoa A`.
- Thiệp: `thiệp A`.

**Expected Result:**
- Trước khi bấm **"Hoàn tất"**, không tồn tại đơn hàng/chờ thanh toán.
- Khách hàng thấy một đơn hàng mới được tạo.
- Đơn hàng có trạng thái chờ thanh toán.
- Hệ thống tạo liên kết `thiệp A` – đơn hàng.
- Chỉ `thiệp A` được tính vào bill.
- Hệ thống chuyển tới màn hình Chi tiết đơn hàng.
- Hệ thống hiển thị nút **"Thanh toán ngay"**.
- Nếu khách hàng chưa thanh toán, đơn hàng vẫn giữ trạng thái chờ thanh toán.

**Trace to:**
- [STORY-039/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-087](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-088](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: đơn hàng chỉ được tạo sau khi khách hàng bấm "Hoàn tất"; đơn hàng được tạo đúng một lần ở trạng thái chờ thanh toán và nếu Checkout có thiệp đã xác nhận thì thiệp được liên kết đúng với đơn hàng.

---

## ST-039-02-01 — Thanh toán thành công đơn hàng chờ thanh toán

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/29ce1562-b887-4379-88ea-bc048aa9b664) |
| **Story** | STORY-039 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Đơn hàng thuộc khách hàng hiện tại.
- Đơn hàng đang ở trạng thái chờ thanh toán.
- Đơn hàng chưa quá 24 giờ.
- Payment provider có thể trả kết quả thành công.

**Steps:**
1. Khách hàng mở Chi tiết `đơn hàng A`.
2. Khách hàng chọn **"Thanh toán ngay"**.
3. Khách hàng hoàn tất giao dịch thành công.
4. Khách hàng quay lại hệ thống.
5. Khách hàng quan sát trạng thái đơn hàng.

**Test Data:**
- Đơn hàng: `đơn hàng A`.

**Expected Result:**
- Hệ thống khởi tạo giao dịch cho đúng `đơn hàng A`.
- Giao dịch có mã giao dịch riêng.
- Khi thanh toán được xác nhận thành công, đơn hàng chuyển từ chờ thanh toán sang đã thanh toán.
- Hệ thống không tạo thêm đơn hàng mới.

**Trace to:**
- [STORY-039/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: một đơn hàng chờ thanh toán còn trong thời hạn được thanh toán thành công và chuyển đúng sang đã thanh toán.

---

## ST-01 / ST-039-03-01 — Thanh toán thất bại vẫn cho thử lại trong thời hạn 24 giờ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/207f6ed9-8a13-43d8-999a-0e715a8d1dfc) (tức [ST-039-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/207f6ed9-8a13-43d8-999a-0e715a8d1dfc)) |
| **Story** | STORY-039 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- `Đơn hàng A` đang chờ thanh toán.
- Đơn hàng chưa quá 24 giờ.
- Môi trường test có dữ liệu để quan sát lần thanh toán đầu thất bại và lần sau thành công.

**Steps:**
1. Khách hàng chọn **"Thanh toán ngay"**.
2. Thực hiện tình huống giao dịch thất bại.
3. Quan sát trạng thái đơn hàng.
4. Khách hàng chọn **"Thanh toán lại"**.
5. Thực hiện thêm một hoặc nhiều lần thanh toán.
6. Cho một lần thanh toán thành công.
7. Quan sát thời điểm hết hạn và trạng thái cuối.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Sau lần thanh toán thất bại, đơn hàng vẫn chờ thanh toán.
- Hệ thống không tạo đơn hàng mới.
- Lần thanh toán thất bại được ghi nhận.
- Khách hàng có thể thử thanh toán lại.
- Không giới hạn số lần thử trong thời hạn 24 giờ.
- Mỗi lần thử có giao dịch riêng.
- Các lần thử không làm thay đổi mốc hết hạn ban đầu.
- Khi một giao dịch thành công, đơn hàng chuyển sang đã thanh toán.

**Trace to:**
- [STORY-039/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: thanh toán thất bại không làm mất đơn hàng và khách hàng có thể thử lại nhiều lần trong thời hạn 24 giờ mà không kéo dài thời hạn chờ thanh toán.

---

## ST-039-04-01 — Tự hủy đơn hàng sau 24 giờ chờ thanh toán

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2325c8cf-7998-422b-83e4-2aaad49afc31) |
| **Story** | STORY-039 |
| **Loại** | 4 |
| **Suite** | REGRESSION |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Đơn hàng đang chờ thanh toán.
- Đơn hàng chưa có giao dịch thanh toán thành công.
- Đơn hàng có tồn kho Combo đang được giữ.
- Môi trường test có dữ liệu để quan sát thời điểm đơn hàng đạt đủ 24 giờ.

**Steps:**
1. Chuẩn bị đơn hàng chờ thanh toán gần thời điểm hết hạn.
2. Đưa thời gian đơn hàng đạt đủ 24 giờ.
3. Chạy hoặc chờ cơ chế tự hủy.
4. Quan sát trạng thái đơn hàng.
5. Quan sát tồn kho.
6. Thử thanh toán đơn hàng đã hủy.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Đơn hàng chuyển sang đã hủy.
- Tồn kho đã giữ cho đơn hàng được giải phóng.
- Việc giải phóng chỉ xảy ra một lần.
- Đơn hàng vẫn tồn tại trong lịch sử đơn hàng.
- Mẫu hoa Custom AI vẫn được giữ trong lịch sử.
- Không cho thanh toán đơn hàng đã hủy.

**Trace to:**
- [STORY-039/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-089](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-091](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: đơn hàng chờ thanh toán chưa thanh toán thành công bị tự động hủy sau 24 giờ và tồn kho đã giữ được giải phóng đúng.

---

## ST-039-05-01 — Core không đủ hàng tại thời điểm Hoàn tất thì chặn tạo đơn hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d161a4-653e-4cef-b1b2-a9786f7b6b53) |
| **Story** | STORY-039 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và có đầy đủ thông tin bắt buộc.
- Tại thời điểm mở Checkout, các thành phần Core còn đủ hàng.
- Có khả năng thay đổi tồn kho Core trước khi khách hàng bấm **"Hoàn tất"**.

**Steps:**
1. Mở `Checkout A` khi thành phần chính `thành phần chính A` còn đủ hàng.
2. Hoàn thiện thông tin Checkout hợp lệ.
3. Trước khi bấm **"Hoàn tất"**, giảm tồn kho thành phần chính `thành phần chính A` xuống thấp hơn số lượng Combo yêu cầu.
4. Chọn **"Hoàn tất"**.
5. Quan sát phản hồi của hệ thống.
6. Quan sát đơn hàng, bill và liên kết Thiệp–đơn hàng nếu Checkout có thiệp.

**Test Data:**
- Checkout: `Checkout A`.
- Combo: `Combo A`.
- Core: `thành phần chính A`.
- Số lượng cần: `2`.
- Số lượng tồn tại thời điểm Hoàn tất: `1`.

**Expected Result:**
- Hệ thống phát hiện `thành phần chính A` không đủ số lượng cần thiết.
- Hệ thống không tạo đơn hàng.
- Khách hàng không thấy hóa đơn cuối được tạo.
- Hệ thống không khởi tạo yêu cầu thanh toán.
- Hệ thống không tạo liên kết Thiệp–đơn hàng.
- Checkout hiện tại vẫn được giữ nếu dữ liệu đã nhập còn hợp lệ.
- Hệ thống hiển thị: **"Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn sản phẩm khác."**

**Trace to:**
- [STORY-039/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-093](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: tồn kho được kiểm tra lại trước khi tạo đơn hàng; Core hết phải chặn đơn hàng, còn Support hết không chặn nếu Core vẫn đủ.

---

## ST-039-06-01 — Checkout không có thiệp vẫn tạo đơn hàng và chốt bill

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7bf2632b-429d-4f9a-8a6a-343390a55a0b) |
| **Story** | STORY-039 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ.
- Không có thiệp được chọn.
- Core còn đủ hàng.
- Có giá Combo và phí giao hàng hợp lệ.

**Steps:**
1. Mở Checkout không có thiệp.
2. Ghi nhận giá Combo và phí giao hàng hiện tại.
3. Chọn **"Hoàn tất"**.
4. Quan sát đơn hàng và bill.
5. Sau khi đơn hàng được tạo, thay đổi giá nguồn.
6. Quan sát lại bill của đơn hàng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Đơn hàng vẫn được tạo bình thường.
- Đơn hàng không có liên kết thiệp.
- Bill không có dòng giá thiệp.
- Tổng tiền không bao gồm giá thiệp.
- Hệ thống chốt giá Combo, phí giao hàng và các khoản hợp lệ tại thời điểm Hoàn tất.
- Bill được lưu cố định với đơn hàng.
- Thay đổi giá sau đó không làm thay đổi bill đã chốt.

**Trace to:**
- [STORY-039/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/ALT-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [BR-092](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Checkout không có thiệp vẫn tạo được đơn hàng và bill được chốt theo dữ liệu tại đúng thời điểm Hoàn tất.

---

## ST-039-07-01 — Lỗi trong quá trình tạo đơn hàng không để lại dữ liệu dở dang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ba279586-f343-4d81-891c-4754239ed952) |
| **Story** | STORY-039 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ.
- Core còn hàng.
- Môi trường test có dữ liệu để quan sát lỗi trong quá trình tạo đơn hàng.

**Steps:**
1. Chuẩn bị Checkout hợp lệ.
2. Tạo tình huống hệ thống gặp lỗi khi hoàn tất đơn hàng.
3. Chọn **"Hoàn tất"**.
4. Quan sát phản hồi.
5. Quan sát đơn hàng, bill, liên kết thiệp và thanh toán.
6. Khôi phục tình huống về bình thường.
7. Thử Hoàn tất lại.

**Test Data:**
- Tình huống: hệ thống gặp lỗi trong lúc khách hàng hoàn tất đơn hàng.

**Expected Result:**
- Lần đầu không để lại đơn hàng được tạo một phần.
- Khách hàng không thấy bill chưa hoàn chỉnh.
- Khách hàng không thấy liên kết Thiệp–đơn hàng dở dang.
- Hệ thống không tạo thanh toán cho thao tác không hợp lệ.
- Checkout được giữ lại nếu có thể.
- Hệ thống hiển thị: **"Không thể tạo đơn hàng. Vui lòng thử lại."**
- Khi thử lại, hệ thống không tạo trùng lặp đơn hàng từ lần xác nhận trước.

**Trace to:**
- [STORY-039/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh khi hệ thống gặp lỗi trong quá trình tạo đơn hàng, khách hàng không thấy đơn hàng hoặc dữ liệu liên quan chưa hoàn chỉnh bị để lại.

---

## ST-039-08-01 — Không cho thanh toán đơn hàng quá hạn hoặc đơn hàng của khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-039-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/47d5d381-feff-4116-8b97-83bdbf51d3dd) |
| **Story** | STORY-039 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một đơn hàng đã quá thời hạn thanh toán.
- Có một đơn hàng thuộc tài khoản khách hàng khác.

**Steps:**
1. **Scenario A — Đơn hàng quá hạn:** Khách hàng mở đơn hàng đã quá 24 giờ.
2. Khách hàng chọn **"Thanh toán ngay"**.
3. Quan sát phản hồi và trạng thái đơn hàng.
4. **Scenario B — Đơn hàng của khách hàng khác:** Khách hàng A truy cập đơn hàng không thuộc tài khoản của mình.
5. Khách hàng A thực hiện thao tác thanh toán trên đơn hàng đó.
6. Quan sát phản hồi, bill, địa chỉ giao hàng và thông tin thanh toán.

**Test Data:**
- Scenario A: Đơn hàng đã quá thời hạn thanh toán 24 giờ.
- Scenario B: Đơn hàng thuộc tài khoản khách hàng khác.

**Expected Result:**
- **Scenario A:** Khách hàng không thể tiếp tục thanh toán đơn hàng đã hết hạn.
- **Scenario A:** Đơn hàng hiển thị trạng thái đã hủy hoặc hết hạn thanh toán.
- **Scenario A:** Đơn hàng không chuyển sang trạng thái đã thanh toán.
- **Scenario A:** Hệ thống hiển thị thông báo hết thời hạn thanh toán.
- **Scenario B:** Khách hàng không thể xem hoặc thanh toán đơn hàng đó.
- **Scenario B:** Hệ thống không hiển thị bill, địa chỉ giao hàng hoặc thông tin thanh toán của khách hàng khác.
- **Scenario B:** Trạng thái đơn hàng của khách hàng khác không bị thay đổi.
- **Scenario B:** Hệ thống hiển thị thông báo không có quyền truy cập hoặc không có quyền thực hiện thao tác này.

**Trace to:**
- [STORY-039/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-039/EXC-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

**Rationale:**
> Xác minh khách hàng chỉ có thể thanh toán đơn hàng còn hiệu lực và thuộc tài khoản của mình.

---
