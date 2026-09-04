# ST — STORY-033 — Khách hàng tạo mẫu hoa bằng AI — System Tests

---

## ST-033-01-01 — Smoke: Tạo mẫu hoa AI thành công từ đầu đến cuối

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-01-01 |
| **Story** | STORY-033 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Yêu cầu tạo mẫu hoa tồn tại và thuộc khách hàng hiện tại.
- Yêu cầu có đầy đủ dữ liệu cần thiết để tạo ảnh.
- Yêu cầu chưa có kết quả AI trước đó.
- Không có quá trình tạo ảnh AI nào của yêu cầu đang ở trạng thái **"Đang tạo"**.
- Khách hàng còn ít nhất 1 lượt Hoa AI trong ngày.
- Combo nguồn còn khả dụng.
- AI service đang hoạt động bình thường.

**Steps:**
1. Khách hàng mở màn hình Chi tiết yêu cầu tạo mẫu hoa.
2. Khách hàng kiểm tra chức năng **"Tạo bó hoa AI ngay"**.
3. Khách hàng chọn **"Tạo bó hoa AI ngay"**.
4. Khách hàng kiểm tra trạng thái tạo mẫu và số lượt Hoa AI sau khi bắt đầu.
5. Khách hàng chờ quá trình tạo mẫu hoàn tất thành công.
6. Khách hàng kiểm tra ảnh kết quả được hiển thị.
7. Khách hàng kiểm tra lịch sử kết quả của yêu cầu và số lượt Hoa AI còn lại.

**Test Data:**
- Có 01 khách hàng với một yêu cầu tạo mẫu hoa hợp lệ.
- Yêu cầu chưa từng tạo mẫu hoa AI thành công.
- Yêu cầu sử dụng 01 Combo còn khả dụng và đủ dữ liệu cần thiết.
- Số lượt Hoa AI trước khi tạo: `3/3`.
- Không có lần tạo AI nào của yêu cầu đang ở trạng thái **"Đang tạo"**.
- Lần tạo AI này trả về đúng 01 ảnh hợp lệ.

**Expected Result:**
- Hệ thống hiển thị chức năng **"Tạo bó hoa AI ngay"**.
- Sau khi khách hàng chọn tạo, hệ thống bắt đầu xử lý và hiển thị trạng thái **"Đang tạo"**.
- Số lượt Hoa AI giảm từ `3/3` xuống `2/3`.
- Khi tạo thành công, hệ thống hiển thị đúng 01 ảnh kết quả mới.
- Kết quả vừa tạo xuất hiện trong lịch sử của đúng yêu cầu.
- Trạng thái lần tạo chuyển thành **"Đã tạo"**.
- Số lượt Hoa AI vẫn là `2/3` sau khi tạo thành công và không được hoàn lại.

**Trace to:**
- STORY-033/AC-001
- STORY-033/AC-003
- STORY-033/AC-006
- STORY-033/AC-007
- STORY-033/AC-009

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể thực hiện lần tạo mẫu hoa AI đầu tiên thành công từ một yêu cầu hợp lệ; hệ thống tạo quá trình tạo ảnh AI, chuyển trạng thái sang "Đang tạo", trừ đúng một lượt AI và khi AI trả ảnh hợp lệ thì lưu đúng một kết quả cho lần tạo ảnh.

---

## ST-033-02-01 — Input gửi sang AI sử dụng đúng dữ liệu và thứ tự ưu tiên

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-02-01 |
| **Story** | STORY-033 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một yêu cầu tạo mẫu hoa hợp lệ thuộc khách hàng.
- Khách hàng còn ít nhất 1 lượt AI.
- Không có quá trình tạo ảnh AI đang chạy.
- Yêu cầu có đầy đủ Combo, Mockup và các thông tin tùy chỉnh.
- Có khả năng kiểm tra dữ liệu đầu vào được hệ thống gửi sang AI service.

**Steps:**
1. Khách hàng mở yêu cầu tạo mẫu hoa.
2. Khách hàng thực hiện **"Tạo bó hoa AI ngay"**.
3. Kiểm tra dữ liệu hệ thống thu thập cho lần tạo ảnh.
4. Kiểm tra nội dung đầu vào gửi sang AI.
5. Đối chiếu các dữ liệu có khả năng ảnh hưởng hoặc xung đột nhau.

**Test Data:**
- Combo: `Combo A`.
- Thành phần Combo: `Hồng 3`, `Baby 5`.
- Mockup: `Mockup A`.
- Bố cục Mockup A: bó hoa dáng tròn, hoa tập trung ở trung tâm.
- Ghi chú/Yêu cầu thêm: `Sắp xếp hoa theo dáng dài, lệch về một phía.`
- Phong cách: `Tự nhiên, phóng khoáng`.

**Expected Result:**
- Input được lấy từ đúng yêu cầu hiện tại.
- Input bao gồm Combo, thành phần và số lượng, dịp sử dụng, phong cách, ngân sách, kích thước, ghi chú/yêu cầu thêm và Mockup.
- Hệ thống truyền đầy đủ danh sách loại hoa và số lượng của Combo nguồn.
- Khi dữ liệu xung đột, hệ thống áp dụng thứ tự ưu tiên: **Combo > Mockup > Ghi chú/Yêu cầu thêm > Phong cách**.
- Mockup chỉ đóng vai trò tham chiếu về kiểu dáng, bố cục hoặc bối cảnh và không làm thay đổi loại hoa hoặc số lượng của Combo.

**Trace to:**
- STORY-033/AC-004
- STORY-033/AC-005

**Rationale:**
> Xác minh hệ thống sử dụng đúng dữ liệu của yêu cầu hiện tại để tạo input cho AI và áp dụng đúng thứ tự ưu tiên khi các nguồn dữ liệu có nội dung ảnh hưởng hoặc xung đột nhau.

---

## ST-033-03-01 — Hiển thị kết quả AI và các chức năng liên quan

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-03-01 |
| **Story** | STORY-033 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Một lần tạo ảnh AI đã hoàn tất thành công.
- Ảnh và History item đã được lưu thành công.

**Steps:**
1. Hoàn tất một lần tạo ảnh AI thành công.
2. Khách hàng quan sát màn hình Kết quả bó hoa AI.
3. Khách hàng kiểm tra ảnh kết quả.
4. Khách hàng kiểm tra các chức năng trên màn hình.
5. Khách hàng kiểm tra cảnh báo liên quan đến ảnh AI.

**Test Data:**
- Kết quả AI: `AI-RESULT-A`.

**Expected Result:**
- Hệ thống hiển thị đúng ảnh vừa được tạo.
- Hệ thống hiển thị các chức năng **"Tải xuống"**, **"Tạo lại"**, **"Đặt hàng ngay"**, **"Thêm giỏ hàng"**.
- Hệ thống hiển thị thông báo ảnh AI chỉ mang tính minh họa và có thể sai khác với Combo thực tế.
- Nếu UI thể hiện mức **"độ tương đồng khoảng 80%"**, nội dung phải cho biết đây là mức ước lượng/tham khảo, không phải cam kết chính xác tuyệt đối.

**Trace to:**
- STORY-033/AC-015
- STORY-033/AC-017

**Rationale:**
> Xác minh sau khi tạo ảnh thành công, khách hàng xem được đúng ảnh vừa tạo cùng các chức năng liên quan và được thông báo rõ ảnh AI chỉ mang tính minh họa.

---

## ST-033-04-01 — Tạo lại tạo kết quả mới và giữ kết quả cũ

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-04-01 |
| **Story** | STORY-033 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Yêu cầu thuộc khách hàng hiện tại.
- Yêu cầu đã có ít nhất 1 kết quả AI thành công.
- Khách hàng còn ít nhất 1 lượt AI.
- Không có quá trình xử lý AI đang chạy.

**Steps:**
1. Khách hàng mở kết quả AI hiện tại.
2. Khách hàng kiểm tra nút **"Tạo lại"**.
3. Khách hàng chọn **"Tạo lại"**.
4. Khách hàng chờ lần tạo ảnh mới hoàn tất thành công.
5. Khách hàng kiểm tra kết quả vừa tạo.
6. Khách hàng kiểm tra lịch sử kết quả AI.

**Test Data:**
- Lịch sử hiện tại: `RESULT-A`.
- Kết quả lần tạo lại: `RESULT-B`.

**Expected Result:**
- Hệ thống hiển thị nút **"Tạo lại"**.
- **"Tạo lại"** được xử lý như một thao tác tạo ảnh mới.
- Thao tác hợp lệ bị trừ đúng 1 lượt AI.
- Hệ thống tạo `RESULT-B` thành một mục lịch sử mới.
- `RESULT-A` vẫn được giữ nguyên.
- Hệ thống không ghi đè hoặc xóa kết quả trước đó.

**Trace to:**
- STORY-033/AC-002
- STORY-033/AC-011
- STORY-033/AC-014

**Rationale:**
> Xác minh "Tạo lại" được xử lý như một lần tạo ảnh độc lập, tạo kết quả mới mà không ghi đè hoặc xóa các kết quả AI trước đó.

---

## ST-033-05-01 — Reload hoặc rời trang không hủy quá trình tạo ảnh AI

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-05-01 |
| **Story** | STORY-033 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Một quá trình tạo ảnh AI đang ở trạng thái **"Đang tạo"**.

**Steps:**
1. Khách hàng bắt đầu tạo ảnh AI.
2. Khách hàng kiểm tra quá trình xử lý ở trạng thái **"Đang tạo"**.
3. Khách hàng reload hoặc rời màn hình.
4. Quá trình xử lý tiếp tục thực hiện.
5. Khách hàng quay lại yêu cầu.
6. Khách hàng kiểm tra trạng thái hiện tại.
7. Nếu quá trình xử lý đã hoàn thành, khách hàng kiểm tra kết quả.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Reload hoặc rời trang không hủy quá trình tạo ảnh AI.
- Quá trình xử lý vẫn tiếp tục cho đến khi có kết quả cuối cùng.
- Khi khách hàng quay lại, hệ thống hiển thị đúng trạng thái hiện tại.
- Nếu quá trình xử lý đã thành công, hệ thống hiển thị kết quả cuối cùng.

**Trace to:**
- STORY-033/AC-012

**Rationale:**
> Xác minh quá trình tạo ảnh AI đã được hệ thống chấp nhận không phụ thuộc vào việc khách hàng tiếp tục mở trang.

---

## ST-033-06-01 — Hết lượt Hoa AI thì không thể tạo thêm

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-06-01 |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Yêu cầu hợp lệ.
- Lượt sử dụng Hoa AI còn lại = `0`.
- Không có quá trình tạo ảnh AI đang chạy.

**Steps:**
1. Khách hàng mở màn hình có chức năng tạo AI khi lượt sử dụng Hoa AI còn lại = `0`.
2. Khách hàng kiểm tra trạng thái nút **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**.
3. Khách hàng kiểm tra thông báo về việc đã hết lượt AI trong ngày.
4. Khách hàng kiểm tra trạng thái xử lý AI và số lượt sử dụng.

**Test Data:**
- Số lượt Hoa AI còn lại: `0`.

**Expected Result:**
- Nút tạo AI ở trạng thái disabled.
- Hệ thống thông báo khách hàng đã hết lượt AI trong ngày.
- Hệ thống thông báo lượt được cấp lại vào ngày tiếp theo.
- Không có quá trình tạo ảnh AI mới được bắt đầu.
- Số lượt sử dụng hiển thị không tăng thêm.

**Trace to:**
- STORY-033/AC-003
- STORY-033/AC-016

**Rationale:**
> Xác minh khách hàng đã sử dụng hết lượt Hoa AI không thể tạo thêm quá trình xử lý mới hoặc phát sinh thêm lượt sử dụng.

---

## ST-033-07-01 — Chặn tạo đồng thời nhiều quá trình AI cho cùng yêu cầu

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-07-01 |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Yêu cầu hợp lệ.
- Khách hàng còn lượt sử dụng.
- Có khả năng gửi nhiều thao tác tạo ảnh liên tiếp hoặc đồng thời.

**Steps:**
1. Khách hàng thực hiện thao tác tạo ảnh đầu tiên.
2. Xác nhận màn hình hiển thị trạng thái **"Đang tạo"**.
3. Trong khi lần tạo đầu tiên vẫn đang xử lý, thực hiện thêm thao tác tạo ảnh cho cùng yêu cầu từ cùng màn hình hoặc tab/client khác.
4. Kiểm tra trạng thái hiển thị của yêu cầu sau các thao tác bổ sung.
5. Kiểm tra số lượt sử dụng.

**Test Data:**
- Không cần dữ liệu kiểm thử đặc biệt.

**Expected Result:**
- Thao tác tạo ảnh đầu tiên được chấp nhận và hiển thị trạng thái **"Đang tạo"**.
- Các thao tác tạo ảnh tiếp theo cho cùng yêu cầu không khởi tạo thêm một lần xử lý mới.
- Khách hàng vẫn chỉ thấy một trạng thái **"Đang tạo"** tương ứng với yêu cầu hiện tại.
- Các thao tác bị từ chối không làm tăng thêm lượt sử dụng.
- Quá trình xử lý đầu tiên tiếp tục cho đến khi có kết quả cuối cùng.

**Trace to:**
- STORY-033/AC-008

**Rationale:**
> Xác minh một yêu cầu tạo mẫu hoa chỉ có tối đa một quá trình tạo ảnh AI đang xử lý, kể cả khi có double-click hoặc thao tác đồng thời từ nhiều tab/client.

---

## ST-033-08-01 — AI thất bại sau retry thì hoàn đúng một lượt

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-08-01 |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thao tác tạo ảnh hợp lệ.
- Khách hàng còn lượt sử dụng.
- Môi trường test có dữ liệu để AI service liên tục trả lỗi hoặc không có ảnh hợp lệ.

**Steps:**
1. Khách hàng thực hiện tạo ảnh.
2. Kiểm tra thao tác được chấp nhận và lượt sử dụng bị trừ 1.
3. Mô phỏng tình huống AI service thất bại.
4. Theo dõi các lần tự động thử lại.
5. Chờ quá trình thử lại kết thúc.
6. Kiểm tra trạng thái quá trình xử lý, lượt sử dụng và lịch sử.

**Test Data:**
- Lượt sử dụng trước khi tạo ảnh: `2`.
- AI service thất bại ở thao tác ban đầu và cả 2 lần thử lại.

**Expected Result:**
- Hệ thống tự động thử lại tối đa 2 lần trong cùng quá trình tạo ảnh AI.
- Trong quá trình thử lại, quá trình xử lý giữ trạng thái **"Đang tạo"**.
- Không trừ thêm lượt sử dụng cho các lần thử lại.
- Sau lần thất bại cuối cùng, quá trình xử lý chuyển sang **"Lỗi"**.
- Hệ thống hoàn lại đúng 1 lượt AI.
- Mỗi quá trình xử lý chỉ được hoàn lượt sử dụng tối đa một lần.
- Không tạo mục lịch sử.

**Trace to:**
- STORY-033/AC-010
- STORY-033/AC-013

**Rationale:**
> Xác minh lỗi AI được tự động thử lại trong cùng quá trình xử lý mà không trừ thêm lượt sử dụng; nếu vẫn thất bại sau toàn bộ thử lại thì quá trình xử lý chuyển "Lỗi", hoàn đúng một lượt và không tạo mục lịch sử.

---

## ST-033-09-01 — Không cho phép tạo AI từ yêu cầu không thuộc khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-09-01 |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có `REQ-B` thuộc khách hàng B.
- Khách hàng A đã đăng nhập.

**Steps:**
1. Với khách hàng A, gửi yêu cầu tạo ảnh sử dụng `REQ-B`.
2. Kiểm tra phản hồi.
3. Thực hiện tương tự với một ID yêu cầu không tồn tại.
4. Kiểm tra quá trình tạo ảnh AI và lượt sử dụng.

**Test Data:**
- Khách hàng hiện tại: `Khách hàng A`.
- Yêu cầu thuộc tài khoản khác: `REQ-B`.
- Một ID yêu cầu không tồn tại.

**Expected Result:**
- Hệ thống từ chối thao tác.
- Khách hàng không thấy quá trình tạo ảnh AI bắt đầu.
- Hệ thống không thực hiện retry.
- Số lượt sử dụng còn lại không bị giảm.
- Hệ thống không trả về dữ liệu của khách hàng B.
- Hệ thống không làm lộ thông tin kỹ thuật nhạy cảm.

**Trace to:**
- STORY-033/EXC-04

**Rationale:**
> Xác minh khách hàng không thể tạo ảnh AI từ yêu cầu không tồn tại hoặc thuộc tài khoản khác và hệ thống không làm lộ dữ liệu của khách hàng khác.

---

## ST-033-10-01 — Combo nguồn hết hàng thì chặn tạo ảnh AI

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-10-01 |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | REGRESSION |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Yêu cầu tạo mẫu hoa hợp lệ.
- Khách hàng còn lượt sử dụng.
- Combo nguồn được xác định là hết hàng tại Nhanh.vn.
- Không có quá trình tạo ảnh AI đang chạy.

**Steps:**
1. Khách hàng mở yêu cầu tạo mẫu hoa.
2. Khách hàng thực hiện **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**.
3. Khách hàng kiểm tra phản hồi.
4. Kiểm tra việc tạo quá trình tạo ảnh AI, lượt sử dụng và thao tác gửi sang AI service.

**Test Data:**
- Combo nguồn: hết hàng tại Nhanh.vn.
- Khách hàng còn ít nhất 1 lượt Hoa AI.

**Expected Result:**
- Hệ thống phát hiện Combo hết hàng.
- Hệ thống không gửi thao tác tạo ảnh sang AI service.
- Không tạo ảnh mới.
- Không cho phép tiếp tục tạo ảnh.
- Hệ thống thông báo Combo đã hết hàng.
- Số lượt sử dụng còn lại không bị giảm cho thao tác bị từ chối.

**Trace to:**
- STORY-033/BR-155

**Rationale:**
> Xác minh hệ thống kiểm tra tồn kho hiện tại của Combo nguồn trước khi gửi thao tác sang AI và chặn tạo ảnh nếu Combo đã hết hàng.

---

## ST-033-11-01 — Lượt Hoa AI và Thiệp AI được quản lý độc lập

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-033-11-01 |
| **Story** | STORY-033 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Lượt sử dụng Hoa AI và lượt sử dụng Thiệp AI đều còn lượt.
- Có một yêu cầu tạo mẫu hoa đủ điều kiện tạo ảnh.

**Steps:**
1. Ghi nhận lượt sử dụng Hoa AI và Thiệp AI hiện tại.
2. Khách hàng thực hiện một lần tạo mẫu hoa AI thành công.
3. Kiểm tra lại hai loại lượt sử dụng.

**Test Data:**
- Hoa AI trước khi tạo ảnh: `3`.
- Thiệp AI trước khi tạo ảnh: `10`.

**Expected Result:**
- Lượt sử dụng Hoa AI giảm đúng 1 lượt.
- Lượt sử dụng Thiệp AI không thay đổi.
- Hai lượt sử dụng tiếp tục được quản lý độc lập.
- Hệ thống không cộng gộp hai lượt sử dụng thành một hạn mức chung.

**Trace to:**
- STORY-033/BR-032

**Rationale:**
> Xác minh việc sử dụng lượt tạo mẫu hoa AI chỉ ảnh hưởng đến lượt sử dụng Hoa AI và không làm thay đổi lượt sử dụng Thiệp AI.
