# ST — STORY-040 — Khách hàng xem lịch sử các mẫu hoa Custom AI đã tạo — System Tests

---

## ST-040-01-01 — Xem danh sách yêu cầu tạo mẫu hoa của khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/afae6899-c90d-4416-a7fb-29c02be1e909) |
| **Story** | STORY-040 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng quan sát các yêu cầu được hiển thị và thứ tự danh sách.
3. Khách hàng chọn một yêu cầu để xem chi tiết.

**Test Data:**
- Có nhiều yêu cầu thuộc khách hàng.
- Danh sách gồm yêu cầu đã tạo mẫu AI và yêu cầu chưa từng tạo mẫu AI.

**Expected Result:**
- Hệ thống hiển thị đầy đủ các yêu cầu thuộc khách hàng.
- Yêu cầu chưa có kết quả AI vẫn được hiển thị.
- Yêu cầu có hoạt động hoặc lần tạo mẫu gần nhất được xếp trước.
- Kết quả AI được hiển thị đúng theo từng yêu cầu.

**Trace to:**
- [STORY-040/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [STORY-040/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-095](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9d83c7a2-8d9f-41b8-833d-f4c6e550a1e9)
- [BR-096](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5d2f87a0-b9f8-4a88-97a3-ca6c941f22e5)
- [BR-098](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b8dabb0d-98cf-4142-a86e-7e74681705ec)

**Rationale:**
> Xác minh khách hàng xem được đầy đủ danh sách yêu cầu tạo mẫu hoa của mình và danh sách được sắp xếp theo hoạt động mới nhất.

---

## ST-040-03-01 — Hiển thị kết quả AI thành công trong lịch sử yêu cầu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd3b486e-b7b7-40c6-9244-bb436dff3672) |
| **Story** | STORY-040 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng chọn yêu cầu có lần tạo mẫu AI thành công.
3. Khách hàng quan sát danh sách kết quả.

**Test Data:**
- Có một yêu cầu có một lần tạo mẫu AI thành công và tạo được ảnh hợp lệ.

**Expected Result:**
- Hệ thống hiển thị đúng một mục lịch sử thuộc yêu cầu.
- Kết quả đó được tính vào tổng số mẫu của yêu cầu.

**Trace to:**
- [STORY-040/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-097](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a616dc95-b28a-4370-94f5-dff3cddab2ea)

**Rationale:**
> Xác minh kết quả AI thành công được hiển thị đúng trong lịch sử của yêu cầu và được tính vào tổng số mẫu.

---

## ST-040-04-01 — Không hiển thị lần tạo mẫu AI không có ảnh hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e3d5b974-a3b9-4f5e-b3c2-979cfa9b7087) |
| **Story** | STORY-040 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng chọn yêu cầu có lần tạo mẫu AI kết thúc nhưng không tạo được ảnh.
3. Khách hàng quan sát danh sách kết quả.

**Test Data:**
- Có một yêu cầu có một lần tạo mẫu AI đã kết thúc nhưng không có ảnh kết quả.

**Expected Result:**
- Lần tạo mẫu không có ảnh không xuất hiện trong mục lịch sử.
- Tổng số mẫu không tăng.

**Trace to:**
- [STORY-040/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-097](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a616dc95-b28a-4370-94f5-dff3cddab2ea)

**Rationale:**
> Xác minh khách hàng chỉ thấy các kết quả AI có ảnh hợp lệ trong lịch sử của yêu cầu.

---

## ST-040-05-01 — Hiển thị đầy đủ nhiều kết quả AI theo thứ tự mới nhất trước

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/95771bda-1a77-4bd8-a4f2-819f793f68e4) |
| **Story** | STORY-040 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng chọn yêu cầu có nhiều kết quả AI.
3. Khách hàng quan sát danh sách kết quả.

**Test Data:**
- Có một yêu cầu có ba kết quả AI thành công ở các thời điểm khác nhau.

**Expected Result:**
- Hệ thống hiển thị đầy đủ các kết quả AI của yêu cầu.
- Kết quả cũ không bị ghi đè.
- Kết quả mới nhất được xếp trước.

**Trace to:**
- [STORY-040/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-095](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9d83c7a2-8d9f-41b8-833d-f4c6e550a1e9)
- [BR-098](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b8dabb0d-98cf-4142-a86e-7e74681705ec)

**Rationale:**
> Xác minh khách hàng xem được toàn bộ lịch sử kết quả AI của một yêu cầu và kết quả mới nhất được ưu tiên hiển thị trước.

---

## ST-040-06-01 — Reload vẫn hiển thị đúng trạng thái đang tạo

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c543bb8c-fcb0-4ec5-b31e-9249f248ecc4) |
| **Story** | STORY-040 |
| **Loại** | 3 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Trong lúc AI đang tạo mẫu, khách hàng reload hoặc mở lại **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng quan sát trạng thái yêu cầu.
3. Khách hàng mở chi tiết yêu cầu.

**Test Data:**
- Có một yêu cầu đang trong quá trình tạo mẫu AI.
- Yêu cầu đã có một kết quả thành công trước đó.

**Expected Result:**
- Thẻ yêu cầu hiển thị trạng thái **"Đang tạo"**.
- Lần tạo mẫu đang chạy chưa được tính vào tổng số mẫu.
- Lần tạo mẫu đang chạy chưa xuất hiện trong lịch sử kết quả.
- Kết quả thành công trước đó vẫn hiển thị.

**Trace to:**
- [STORY-040/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-097](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a616dc95-b28a-4370-94f5-dff3cddab2ea)

**Rationale:**
> Xác minh khách hàng vẫn thấy đúng trạng thái yêu cầu đang tạo sau khi reload hoặc mở lại màn hình.

---

## ST-040-07-01 — Empty state khi khách hàng chưa có yêu cầu tạo mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/09c1b532-29b2-4759-bbfd-de43a18eb65f) |
| **Story** | STORY-040 |
| **Loại** | 5 |
| **Suite** | REGRESSION |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.

**Test Data:**
- Khách hàng chưa có yêu cầu tạo mẫu hoa nào.

**Expected Result:**
- Hệ thống hiển thị empty state cho biết khách hàng chưa có yêu cầu tạo mẫu hoa.

**Trace to:**
- [STORY-040/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)

**Rationale:**
> Xác minh khách hàng được hướng dẫn rõ ràng khi danh sách yêu cầu tạo mẫu hoa đang rỗng.

---

## ST-040-08-01 — Error state và tải lại khi dữ liệu không tải được

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f918d3d-f5f8-41ad-917d-49e23dbe126d) |
| **Story** | STORY-040 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở danh sách hoặc chi tiết yêu cầu khi dữ liệu không tải được.
2. Khách hàng quan sát trạng thái lỗi.
3. Sau khi lỗi được khắc phục, khách hàng thực hiện tải lại.

**Test Data:**
- Có tình huống danh sách yêu cầu hoặc danh sách kết quả không tải được.

**Expected Result:**
- Hệ thống hiển thị error state khi tải dữ liệu thất bại.
- Hệ thống có chức năng tải lại.
- Sau khi tải lại thành công, dữ liệu hiển thị bình thường.

**Trace to:**
- [STORY-040/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)

**Rationale:**
> Xác minh khách hàng thấy trạng thái lỗi rõ ràng và có thể tải lại dữ liệu khi màn hình gặp lỗi tải.

---

## ST-040-09-01 — Không xem được yêu cầu hoặc kết quả của khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/24488ad1-cde7-446f-846c-85afe964c15f) |
| **Story** | STORY-040 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có hai tài khoản khách hàng khác nhau.

**Steps:**
1. Đăng nhập bằng khách hàng A.
2. Truy cập trực tiếp yêu cầu hoặc kết quả thuộc khách hàng B.

**Test Data:**
- Khách hàng B có ít nhất một yêu cầu và một kết quả AI.

**Expected Result:**
- Khách hàng A không xem được yêu cầu của khách hàng B.
- Khách hàng A không xem được kết quả AI của khách hàng B.
- Hệ thống không hiển thị ảnh hoặc thông tin của dữ liệu đó.

**Trace to:**
- [STORY-040/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-100](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fc0831c-0ef8-421f-8732-543580b05027)

**Rationale:**
> Xác minh khách hàng không thể xem dữ liệu yêu cầu tạo mẫu hoa hoặc kết quả AI thuộc tài khoản khác.

---

## ST-040-10-01 — Ảnh lịch sử không còn khả dụng không làm lỗi toàn bộ màn hình

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-040-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de5c4dcf-994b-47cd-9b4d-ab9ac0031af2) |
| **Story** | STORY-040 |
| **Loại** | 5 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.

**Steps:**
1. Khách hàng mở **"Yêu cầu tạo mẫu hoa"**.
2. Khách hàng chọn yêu cầu có mục lịch sử bị mất file ảnh.
3. Khách hàng quan sát mục lịch sử và màn hình.

**Test Data:**
- Có một mục lịch sử từng tạo ảnh thành công nhưng file ảnh hiện không còn khả dụng.

**Expected Result:**
- Mục lịch sử vẫn được giữ trong danh sách.
- Mục đó hiển thị **"Ảnh không còn khả dụng"**.
- Lỗi ảnh không làm lỗi toàn bộ danh sách hoặc màn hình.

**Trace to:**
- [STORY-040/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)
- [BR-099](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c34f1cbc-902b-49e9-bfeb-b0cf83e585dd)

**Rationale:**
> Xác minh khi ảnh lịch sử không còn khả dụng, khách hàng vẫn xem được màn hình và các dữ liệu khác không bị ảnh hưởng.
