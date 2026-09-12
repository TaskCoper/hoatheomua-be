# ST — STORY-033 — Khách hàng tạo mẫu hoa bằng AI — System Tests

---

## ST-033-01-01 — Smoke: Tạo mẫu hoa AI thành công từ đầu đến cuối

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf9fec94-2982-455f-a5e0-c5ad217ecbb8) |
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
- Không có quá trình tạo ảnh AI nào của yêu cầu đang ở trạng thái “Đang tạo”.
- Khách hàng còn ít nhất 1 lượt Hoa AI trong ngày.
- Combo nguồn còn khả dụng.
- AI service đang hoạt động bình thường.

**Steps:**
1. Khách hàng mở Chi tiết yêu cầu tạo mẫu hoa.
2. Kiểm tra chức năng “Tạo bó hoa AI ngay”.
3. Chọn “Tạo bó hoa AI ngay”.
4. Kiểm tra trạng thái tạo mẫu và số lượt Hoa AI sau khi bắt đầu.
5. Chờ quá trình tạo mẫu hoàn tất thành công.
6. Kiểm tra ảnh kết quả được hiển thị.
7. Kiểm tra lịch sử kết quả của yêu cầu và số lượt Hoa AI còn lại.

**Test Data:**
- 01 khách hàng có yêu cầu tạo mẫu hoa hợp lệ.
- Yêu cầu chưa từng tạo mẫu hoa AI thành công.
- Yêu cầu sử dụng 01 Combo còn khả dụng và đủ dữ liệu cần thiết.
- Số lượt Hoa AI trước khi tạo: 3/3.
- Không có lần tạo AI nào của yêu cầu đang ở trạng thái “Đang tạo”.
- Lần tạo AI này trả về đúng 01 ảnh hợp lệ.

**Expected Result:**
- Hiển thị chức năng “Tạo bó hoa AI ngay”.
- Sau khi khách hàng chọn tạo, hệ thống bắt đầu xử lý và hiển thị trạng thái “Đang tạo”.
- Số lượt Hoa AI giảm từ 3/3 xuống 2/3.
- Khi tạo thành công, hệ thống hiển thị đúng 01 ảnh kết quả mới.
- Kết quả vừa tạo xuất hiện trong lịch sử của đúng yêu cầu.
- Trạng thái lần tạo chuyển thành “Đã tạo”.
- Số lượt Hoa AI vẫn là 2/3 sau khi tạo thành công, không được hoàn lại.

**Trace to:**
- [STORY-033/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể thực hiện lần tạo mẫu hoa AI đầu tiên thành công từ một yêu cầu hợp lệ; hệ thống tạo quá trình tạo ảnh AI, chuyển trạng thái sang “Đang tạo”, trừ đúng một lượt AI và khi AI trả ảnh hợp lệ thì lưu đúng một kết quả cho lần tạo ảnh.

---

## ST-033-02-01 — Input tạo ảnh AI và thứ tự ưu tiên dữ liệu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35c813a1-f163-4ea9-bad7-188691152397) |
| **Story** | STORY-033 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một yêu cầu tạo mẫu hoa hợp lệ thuộc khách hàng.
- Khách hàng còn ít nhất 1 lượt AI.
- Không có quá trình tạo ảnh AI đang chạy.
- Yêu cầu có đầy đủ Combo, Mockup và các thông tin tùy chỉnh.
- Có khả năng kiểm tra dữ liệu đầu vào được hệ thống gửi sang AI service.

**Steps:**
1. Mở yêu cầu tạo mẫu hoa.
2. Thực hiện “Tạo bó hoa AI ngay”.
3. Quan sát dữ liệu hệ thống thu thập cho lần tạo ảnh.
4. Quan sát nội dung đầu vào gửi sang AI.
5. Đối chiếu các dữ liệu có khả năng xung đột.

**Test Data:**
- Combo: Combo A.
- Thành phần Combo: Hồng 3, Baby 5.
- Mockup: Mockup A.
- Bố cục Mockup A: bó hoa dáng tròn, hoa tập trung ở trung tâm.
- Ghi chú/Yêu cầu thêm: “Sắp xếp hoa theo dáng dài, lệch về một phía.”
- Phong cách: Tự nhiên, phóng khoáng.

**Expected Result:**
- Input được lấy từ đúng yêu cầu hiện tại.
- Input bao gồm Combo, thành phần và số lượng, dịp sử dụng, phong cách, ngân sách, kích thước, ghi chú/yêu cầu thêm và Mockup.
- Hệ thống truyền đầy đủ danh sách loại hoa và số lượng của Combo nguồn.
- Khi dữ liệu xung đột, hệ thống áp dụng thứ tự ưu tiên Combo > Mockup > Ghi chú/Yêu cầu thêm > Phong cách.
- Mockup chỉ đóng vai trò tham chiếu về kiểu dáng, bố cục hoặc bối cảnh và không làm thay đổi loại hoa/số lượng của Combo.

**Trace to:**
- [STORY-033/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: hệ thống sử dụng đúng dữ liệu của yêu cầu hiện tại để tạo input cho AI và áp dụng đúng thứ tự ưu tiên khi các nguồn dữ liệu có nội dung ảnh hưởng hoặc xung đột nhau.

---

## ST-033-03-01 — Hiển thị kết quả AI và cảnh báo minh họa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/27a78a35-d161-4b98-a0ee-b16b3dfddc8d) |
| **Story** | STORY-033 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Một lần generate AI đã hoàn tất thành công.
- Ảnh và History item đã được lưu thành công.

**Steps:**
1. Hoàn tất một lần generate thành công.
2. Quan sát màn hình Kết quả bó hoa AI.
3. Quan sát ảnh kết quả.
4. Quan sát các chức năng trên màn hình.
5. Quan sát cảnh báo liên quan đến ảnh AI.

**Test Data:**
- Kết quả: AI-RESULT-A.

**Expected Result:**
- Hệ thống hiển thị đúng ảnh vừa được tạo.
- Hiển thị các chức năng “Tải xuống”, “Tạo lại”, “Đặt hàng ngay”, “Thêm giỏ hàng”.
- Hiển thị thông báo ảnh AI chỉ mang tính minh họa và có thể sai khác với Combo thực tế.
- Nếu UI thể hiện mức “độ tương đồng khoảng 80%”, nội dung phải cho biết đây là mức ước lượng/tham khảo, không phải cam kết chính xác tuyệt đối.

**Trace to:**
- [STORY-033/AC-015](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-017](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh sau khi generate thành công, khách hàng xem được đúng ảnh vừa tạo cùng các chức năng liên quan và được thông báo rõ ảnh AI chỉ mang tính minh họa.

---

## ST-033-04-01 — Tạo lại mẫu hoa AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4b4ff0d-2abd-4d87-8932-76fa9d17d38f) |
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
1. Mở kết quả AI hiện tại.
2. Quan sát nút tạo AI.
3. Chọn “Tạo lại”.
4. Chờ lần tạo ảnh mới hoàn tất thành công.
5. Quan sát kết quả vừa tạo.
6. Quan sát lịch sử kết quả AI.

**Test Data:**
- Lịch sử hiện tại: RESULT-A.
- Kết quả lần tạo lại: RESULT-B.

**Expected Result:**
- Hệ thống hiển thị nút “Tạo lại”.
- “Tạo lại” được xử lý như một thao tác tạo ảnh mới.
- Thao tác hợp lệ bị trừ đúng 1 lượt AI.
- Hệ thống tạo RESULT-B thành một mục lịch sử mới.
- RESULT-A vẫn được giữ nguyên.
- Không ghi đè hoặc xóa kết quả trước đó.

**Trace to:**
- [STORY-033/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-014](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: “Tạo lại” được xử lý như một lần tạo ảnh độc lập, tạo kết quả mới mà không ghi đè hoặc xóa các kết quả AI trước đó.

---

## ST-033-05-01 — Xử lý khi reload hoặc rời trang trong lúc AI đang tạo

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/28453181-9a47-403a-84d5-131c69f47908) |
| **Story** | STORY-033 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Một quá trình tạo ảnh AI đang ở trạng thái “Đang tạo”.

**Steps:**
1. Bắt đầu tạo ảnh AI.
2. Quan sát quá trình xử lý ở trạng thái “Đang tạo”.
3. Reload hoặc rời màn hình.
4. Chờ quá trình xử lý tiếp tục xử lý.
5. Quay lại yêu cầu.
6. Quan sát trạng thái hiện tại.
7. Nếu quá trình xử lý đã hoàn thành, quan sát kết quả.

**Test Data:**
- Không cần data test.

**Expected Result:**
- Reload/rời trang không hủy quá trình tạo ảnh AI.
- Quá trình xử lý vẫn tiếp tục cho đến khi có kết quả cuối cùng.
- Khi khách hàng quay lại, hệ thống hiển thị đúng trạng thái hiện tại.
- Nếu quá trình xử lý đã thành công, hệ thống hiển thị kết quả cuối cùng.

**Trace to:**
- [STORY-033/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: quá trình tạo ảnh AI đã được hệ thống chấp nhận không phụ thuộc vào việc khách hàng tiếp tục mở trang.

---

## ST-033-06-01 — Hết lượt Hoa AI trong ngày

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/15df12c6-65e9-4e84-82c2-5b10eaa3d290) |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Yêu cầu hợp lệ.
- Lượt sử dụng Hoa AI còn lại = 0.
- Không có quá trình tạo ảnh AI đang chạy.

**Steps:**
1. Mở màn hình có chức năng tạo AI khi lượt sử dụng Hoa AI còn lại = 0.
2. Quan sát trạng thái nút “Tạo bó hoa AI ngay” hoặc “Tạo lại”.
3. Quan sát thông báo về việc đã hết lượt AI trong ngày.
4. Quan sát trạng thái xử lý AI và số lượt sử dụng.

**Test Data:**
- Không cần data test.

**Expected Result:**
- Nút tạo AI ở trạng thái disabled.
- Hệ thống thông báo khách hàng đã hết lượt AI trong ngày.
- Hệ thống thông báo lượt được cấp lại vào ngày tiếp theo.
- Khách hàng không thấy quá trình tạo ảnh AI mới bắt đầu.
- Số lượt sử dụng hiển thị không tăng thêm.

**Trace to:**
- [STORY-033/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-016](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng đã sử dụng hết lượt sử dụng Hoa AI không thể tạo thêm quá trình xử lý mới hoặc phát sinh thêm lượt sử dụng.

---

## ST-033-07-01 — Chống tạo nhiều job AI đồng thời cho cùng yêu cầu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5536e08b-9d07-41b7-8cfa-14439221e47d) |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Yêu cầu hợp lệ.
- Khách hàng còn lượt sử dụng.
- Có khả năng gửi nhiều thao tác tạo ảnh liên tiếp hoặc đồng thời.

**Steps:**
1. Thực hiện thao tác tạo ảnh đầu tiên.
2. Xác nhận màn hình hiển thị trạng thái “Đang tạo”.
3. Trong khi yêu cầu đầu tiên vẫn đang xử lý, thực hiện thêm thao tác tạo ảnh cho cùng yêu cầu từ cùng màn hình hoặc tab/client khác.
4. Quan sát trạng thái hiển thị của yêu cầu sau các thao tác bổ sung.
5. Quan sát số lượt sử dụng.

**Test Data:**
- Không cần data test.

**Expected Result:**
- Thao tác tạo ảnh đầu tiên được chấp nhận và hiển thị trạng thái “Đang tạo”.
- Các thao tác tạo ảnh tiếp theo cho cùng yêu cầu không khởi tạo thêm một lần xử lý mới.
- Khách hàng vẫn chỉ thấy một trạng thái “Đang tạo” tương ứng với yêu cầu hiện tại.
- Các thao tác bị từ chối không làm tăng thêm lượt sử dụng.
- Quá trình xử lý đầu tiên tiếp tục cho đến khi có kết quả cuối cùng.

**Trace to:**
- [STORY-033/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: một yêu cầu tạo mẫu hoa chỉ có tối đa một quá trình tạo ảnh AI đang xử lý, kể cả khi có double-click hoặc thao tác đồng thời từ nhiều tab/client.

---

## ST-033-08-01 — Tự động thử lại khi AI service gặp lỗi và hoàn lượt khi thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d6359b75-35b9-4593-b1ee-b4b28fa0a5c8) |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- thao tác tạo ảnh hợp lệ.
- Khách hàng còn lượt sử dụng.
- Môi trường test có dữ liệu để quan sát AI service liên tục trả lỗi/không có ảnh hợp lệ.

**Steps:**
1. Thực hiện tạo ảnh.
2. Quan sát thao tác được chấp nhận và lượt sử dụng bị trừ 1.
3. Thực hiện tình huống AI service thất bại.
4. Theo dõi các lần tự động thử lại.
5. Chờ quá trình thử lại kết thúc.
6. Quan sát trạng thái quá trình xử lý, lượt sử dụng và lịch sử.

**Test Data:**
- lượt sử dụng trước tạo ảnh: 2.
- AI service thất bại ở thao tác ban đầu và cả 2 lần thử lại.

**Expected Result:**
- Hệ thống tự động thử lại tối đa 2 lần trong cùng quá trình tạo ảnh AI.
- Trong quá trình thử lại, quá trình xử lý giữ trạng thái “Đang tạo”.
- Không trừ thêm lượt sử dụng cho các lần thử lại.
- Sau lần thất bại cuối cùng, quá trình xử lý chuyển sang “Lỗi”.
- Hệ thống hoàn lại đúng 1 lượt AI.
- Mỗi quá trình xử lý chỉ được hoàn lượt sử dụng tối đa một lần.
- Không tạo mục lịch sử.

**Trace to:**
- [STORY-033/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9) · [STORY-033/AC-013](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: lỗi AI được tự động thử lại trong cùng quá trình xử lý mà không trừ thêm lượt sử dụng; nếu vẫn thất bại sau toàn bộ thử lại thì quá trình xử lý chuyển “Lỗi”, hoàn đúng một lượt và không tạo mục lịch sử.

---

## ST-033-09-01 — Yêu cầu tạo mẫu hoa không hợp lệ hoặc thuộc tài khoản khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/537dc9d6-1717-47d6-a952-3a91274122cb) |
| **Story** | STORY-033 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có REQ-B thuộc khách hàng B.
- khách hàng A đã đăng nhập.

**Steps:**
1. Với khách hàng A, gửi yêu cầu tạo ảnh sử dụng REQ-B.
2. Quan sát phản hồi.
3. Thực hiện tương tự với một thao tác ID không tồn tại.
4. Quan sát quá trình tạo ảnh AI và lượt sử dụng.

**Test Data:**
- —

**Expected Result:**
- Hệ thống từ chối thao tác.
- Khách hàng không thấy quá trình tạo ảnh AI bắt đầu.
- Không thử lại.
- Số lượt sử dụng còn lại không bị giảm.
- Không trả về dữ liệu của khách hàng B.
- Không làm lộ thông tin kỹ thuật nhạy cảm.

**Trace to:**
- [STORY-033/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng không thể tạo ảnh AI từ yêu cầu không tồn tại hoặc thuộc tài khoản khác và hệ thống không làm lộ dữ liệu của khách hàng khác.

---

## ST-033-10-01 — Combo nguồn hết hàng trước khi generate

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/69b41be7-5f81-4947-84ce-c9ee9ea708a9) |
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
1. Mở yêu cầu tạo mẫu hoa.
2. Thực hiện “Tạo bó hoa AI ngay” hoặc “Tạo lại”.
3. Quan sát phản hồi.
4. Quan sát việc tạo quá trình tạo ảnh AI, lượt sử dụng và thao tác sang AI service.

**Test Data:**
- —

**Expected Result:**
- Hệ thống phát hiện Combo hết hàng.
- Không gửi thao tác tạo ảnh sang AI service.
- Không tạo ảnh mới.
- Không cho phép tiếp tục tạo ảnh.
- Hệ thống thông báo Combo đã hết hàng.
- Số lượt sử dụng còn lại không bị giảm cho thao tác bị từ chối.

**Trace to:**
- [BR-155](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống kiểm tra tồn kho hiện tại của Combo nguồn trước khi gửi thao tác sang AI và chặn tạo ảnh nếu Combo đã hết hàng.

---

## ST-033-11-01 — Tách biệt quota Hoa AI và Thiệp AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-033-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/638e93a0-40b2-4650-bd21-9a4d9b2fb0a8) |
| **Story** | STORY-033 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Lượt sử dụng Hoa AI và lượt sử dụng Thiệp AI đều còn lượt.
- Có một yêu cầu tạo mẫu hoa đủ điều kiện tạo ảnh.

**Steps:**
1. Ghi nhận lượt sử dụng Hoa AI và Thiệp AI hiện tại.
2. Thực hiện một lần tạo mẫu hoa AI thành công.
3. Quan sát lại hai lượt sử dụng.

**Test Data:**
- Hoa AI trước tạo ảnh: 3.
- Thiệp AI trước tạo ảnh: 10.

**Expected Result:**
- lượt sử dụng Hoa AI giảm đúng 1 lượt.
- Lượt sử dụng Thiệp AI không thay đổi.
- Hai lượt sử dụng tiếp tục được quản lý độc lập.
- Không cộng gộp hai lượt sử dụng thành một hạn mức chung.

**Trace to:**
- [BR-032](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

**Rationale:**
> Xác minh trải nghiệm khách hàng: việc sử dụng lượt sử dụng tạo mẫu hoa AI chỉ ảnh hưởng đến lượt sử dụng Hoa AI và không làm thay đổi lượt sử dụng Thiệp AI.
