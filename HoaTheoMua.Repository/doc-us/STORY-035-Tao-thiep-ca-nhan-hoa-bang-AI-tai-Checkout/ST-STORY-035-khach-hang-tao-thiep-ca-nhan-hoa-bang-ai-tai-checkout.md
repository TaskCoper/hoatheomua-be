# ST — STORY-035 — Khách hàng tạo thiệp cá nhân hóa bằng AI tại Checkout — System Tests

---

## ST-035-01-01 — Smoke: Tạo thiệp AI thành công tại Checkout và xác nhận sử dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6c66168b-37ce-4079-9412-6d93b2ec1250) |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập. Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất. Có ít nhất 1 Template đang khả dụng. Có ít nhất 1 Size đang khả dụng. Khách hàng chưa đạt giới hạn 10 lượt thiệp AI trong ngày. Mẫu hoa nguồn chưa đạt giới hạn 3 lượt tạo ảnh thiệp. AI service đang hoạt động bình thường.

**Steps:**
1. Mở Checkout A.
2. Chọn chức năng “Tạo thiệp”.
3. Quan sát danh sách Template và Size.
4. Nhập đầy đủ Người gửi, Người nhận và Lời chúc.
5. Chọn TEMPLATE-A, SIZE-A và hình thức Gõ máy.
6. Chọn “Tạo thiệp”.
7. Chờ AI xử lý thành công.
8. Quan sát kết quả thiệp.
9. Quan sát ảnh, mục lịch sử và số lượt đã sử dụng.
10. Quan sát thiệp hiện tại của Checkout trước khi chọn “Xác nhận”.

**Test Data:**
- Checkout: Checkout A Template: TEMPLATE-A Size: SIZE-A Hình thức: Gõ máy Người gửi: Nguyễn An Người nhận: Trần Bình Lời chúc: Chúc bạn sinh nhật vui vẻ Ảnh đính kèm: Không Lượt sử dụng trong ngày trước tạo ảnh: 0/10 Lượt của mẫu hoa trước tạo ảnh: 0/3

**Expected Result:**
- Danh sách Template và Size khả dụng được hiển thị đầy đủ. Hệ thống chấp nhận các thông tin Người gửi, Người nhận, Lời chúc, TEMPLATE-A, SIZE-A và hình thức Gõ máy hợp lệ. Khi khách hàng chọn “Tạo thiệp”, hệ thống khởi tạo một quá trình tạo ảnh AI. Quá trình tạo ảnh AI hoàn tất thành công và trả về đúng một ảnh thiệp PNG. Một mục lịch sử mới được tạo cho lần tạo ảnh thành công. Lượt sử dụng thiệp AI trong ngày tăng từ 0/10 lên 1/10. Lượt tạo ảnh thiệp của mẫu hoa tăng từ 0/3 lên 1/3. Kết quả thiệp được hiển thị cho khách hàng sau khi AI xử lý thành công. Thiệp vừa tạo chưa tự động trở thành thiệp hiện tại của Checkout trước khi khách hàng chọn “Xác nhận”.

**Trace to:**
- [STORY-035/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-049](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-050](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể tạo thành công một thiệp AI từ Checkout hợp lệ; hệ thống tạo quá trình tạo ảnh AI, ghi nhận đúng lượt sử dụng, lưu đúng một ảnh PNG và một mục lịch sử nhưng chưa tự động chọn kết quả làm thiệp hiện tại của Checkout.

---

## ST-035-02-01 — Validation dữ liệu bắt buộc và giới hạn số từ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/557b7ff4-cfe3-42e6-a663-568cefd10fa4) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Template và Size còn khả dụng. Khách hàng còn lượt sử dụng. Khách hàng đang ở màn tạo thiệp.

**Steps:**
1. Nhập lần lượt từng bộ Test Data.
2. Hoàn thiện các trường còn lại bằng dữ liệu hợp lệ.
3. Chọn “Tạo thiệp”.
4. Quan sát validation.
5. Quan sát việc tạo quá trình tạo ảnh AI, lịch sử và lượt sử dụng.
6. Reset form trước dataset tiếp theo.

**Test Data:**
- | TH | Dữ liệu | Expected | |--------------|--------------------------------------|----------------------| | TD-01 | Người gửi rỗng | Reject | | TD-02 | Người nhận rỗng | Reject | | TD-03 | Lời chúc rỗng | Reject | | TD-04 | Người gửi 20 từ | Accept | | TD-05 | Người gửi 21 từ | Reject | | TD-06 | Người nhận 20 từ | Accept | | TD-07 | Người nhận 21 từ | Reject | | TD-08 | Lời chúc 100 từ | Accept | | TD-09 | Lời chúc 101 từ | Reject | | TD-10 | Nhiều space/tab/newline | Đếm theo rule | | TD-11 | Chỉ chứa khoảng trắng | Tính là 0 từ |

**Expected Result:**
- Người gửi tối đa 20 từ. Người nhận tối đa 20 từ. Lời chúc tối đa 100 từ. Giá trị đúng boundary được chấp nhận. Giá trị vượt boundary bị từ chối. Nhiều khoảng trắng, tab hoặc newline liên tiếp chỉ được tính là dấu phân tách. Khoảng trắng đầu/cuối không được tính. Chuỗi chỉ chứa khoảng trắng được tính là 0 từ. thao tác không hợp lệ không thấy quá trình tạo ảnh AI bắt đầu. Không có ảnh AI mới được tạo. Không tạo mục lịch sử. Số lượt sử dụng còn lại không bị giảm. Hệ thống hiển thị lỗi tại trường tương ứng

**Trace to:**
- [STORY-035/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-051](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống kiểm tra lại các trường bắt buộc và giới hạn số từ theo đúng quy tắc của Story trước khi tạo quá trình tạo ảnh AI.

---

## ST-035-03-01 — Ảnh đính kèm là tùy chọn và xử lý file đính kèm

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/88fab531-4179-4f95-8516-4ced3239690b) |
| **Story** | STORY-035 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Khách hàng còn lượt sử dụng. Các trường bắt buộc khác đều hợp lệ. Template và Size khả dụng.

**Steps:**
--- Tình huống A
2. Upload ảnh PNG hợp lệ.
3. Tạo thiệp.
4. Quan sát ảnh output.
--- Tình huống B
6. Không chọn ảnh.
7. Nhập đầy đủ dữ liệu bắt buộc.
8. Tạo thiệp.
--- Tình huống C/D
10. Chọn file không hợp lệ.
11. Quan sát phản hồi.
12. Chọn file khác hoặc bỏ ảnh.

**Test Data:**
- - Scenario A 1 ảnh PNG hợp lệ ≤10 MB. - Scenario B Không có ảnh. - Scenario C File PDF. - Scenario D Ảnh PNG >10 MB.

**Expected Result:**
- Ảnh hợp lệ được chấp nhận. AI giữ nguyên nội dung gốc của ảnh. Không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng ảnh. Chỉ được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template. Không có ảnh đính kèm vẫn tạo thiệp bình thường. File sai định dạng hoặc >10 MB bị từ chối. Hệ thống hiển thị lý do file không hợp lệ. Khách hàng có thể chọn file khác hoặc tiếp tục không có ảnh. File bị từ chối không làm phát sinh quá trình tạo ảnh AI hoặc lượt sử dụng.

**Trace to:**
- [STORY-035/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-052](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: ảnh đính kèm là tùy chọn, file hợp lệ được sử dụng mà không làm thay đổi nội dung gốc, file không hợp lệ bị từ chối và khách hàng vẫn có thể tạo thiệp khi không có ảnh.

---

## ST-035-04-01 — Xử lý khác nhau giữa Gõ máy và Calligraphy

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/09fc76bd-508f-4e18-9f3e-592dce338594) |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Khách hàng còn lượt sử dụng. Có Template và Size khả dụng. Dữ liệu Người gửi, Người nhận, Lời chúc hợp lệ.

**Steps:**
1. Tạo một thiệp với hình thức Gõ máy.
2. Kiểm tra ảnh output.
3. Tạo một thiệp khác với hình thức Calligraphy.
4. Kiểm tra ảnh output và mục lịch sử.

**Test Data:**
- Người gửi: Nguyễn An Người nhận: Trần Bình Lời chúc: Chúc bạn luôn vui vẻ Tình huống A: Gõ máy Tình huống B: Calligraphy

**Expected Result:**
- - Gõ máy Ảnh output hiển thị nguyên văn Người gửi. Hiển thị nguyên văn Người nhận. Hiển thị nguyên văn Lời chúc. AI không tự sửa, dịch, rút gọn hoặc thay đổi các nội dung này. - Calligraphy Ảnh output không hiển thị Người gửi. Không hiển thị Người nhận. Không hiển thị Lời chúc. Các nội dung này vẫn được lưu trong mục lịch sử.

**Trace to:**
- [STORY-035/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/AC-013](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-154](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: nội dung Người gửi, Người nhận và Lời chúc được xử lý khác nhau đúng theo hình thức thiệp được chọn.

---

## ST-035-05-01 — Giá tạm tính theo Size, hình thức và số từ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/70a8582a-14e4-4edc-bae5-b570a55d5c15) |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang ở Checkout hợp lệ. Có thể chọn Size thiệp. Có thể chọn hình thức Gõ máy hoặc Calligraphy.

**Steps:**
1. Chọn SIZE-A.
2. Chọn Gõ máy.
3. Quan sát giá tạm tính.
4. Chuyển sang Calligraphy.
5. Nhập nội dung ở các mốc số từ cần kiểm tra.
6. Quan sát giá tạm tính sau mỗi thay đổi.

**Test Data:**
- SIZE-A: giá hiển thị 100.000đ Calligraphy: 35 từ. 36 từ. 70 từ. 71 từ. 100 từ.

**Expected Result:**
- Khi chọn Gõ máy, giá tạm tính hiển thị theo giá của Size đã chọn. Với Calligraphy 0-35 từ, giá tạm tính không cộng thêm phụ phí. Với Calligraphy 36-70 từ, giá tạm tính cộng thêm 39.000đ. Với Calligraphy 71-100 từ, giá tạm tính cộng thêm 69.000đ. Giá tạm tính thay đổi ngay khi khách hàng đổi Size, hình thức hoặc số từ. Nếu giá của Size chưa khả dụng, khách hàng thấy thông báo phù hợp và không thể tiếp tục tạo thiệp cho đến khi chọn dữ liệu hợp lệ.

**Trace to:**
- [BR-054](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh khách hàng luôn nhìn thấy giá tạm tính đúng với Size, hình thức thiệp và số từ đã chọn trước khi tiếp tục.

---

## ST-035-06-01 — Xác nhận thiệp mới thay thế thiệp cũ trong Checkout và giữ lịch sử

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/72dc03ce-3e59-4cb8-91ad-3baad506393d) |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Checkout đang có thiệp thiệp A. thiệp thiệp B vừa tạo ảnh thành công và đã có mục lịch sử.

**Steps:**
1. Mở kết quả thiệp thiệp B.
2. Quan sát thiệp thiệp B chưa phải thiệp hiện tại.
3. Chọn “Quan sát”.
4. Quan sát Checkout.
5. Quan sát lịch sử.
6. Quan sát lượt sử dụng và số mục lịch sử.

**Test Data:**
- —

**Expected Result:**
- thiệp thiệp B trở thành thiệp hiện tại của Checkout. thiệp thiệp A bị gỡ khỏi Checkout. thiệp thiệp A vẫn tồn tại trong lịch sử. thiệp thiệp B vẫn giữ mục lịch sử của lần tạo ảnh. Checkout chỉ có tối đa một thiệp được chọn. Khi khách hàng chọn “Xác nhận”, hệ thống không tạo ảnh AI mới. Không tạo mục lịch sử mới. Không ghi nhận thêm lượt tạo ảnh. Đơn hàng Summary hiển thị thiệp được xác nhận và giá tạm tính tương ứng.

**Trace to:**
- [STORY-035/AC-014](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: kết quả AI chỉ trở thành thiệp hiện tại sau khi khách hàng Xác nhận và việc thay thế thiệp không làm mất lịch sử cũ hoặc phát sinh tạo ảnh mới.

---

## ST-035-07-01 — Reload hoặc rời trang không hủy quá trình tạo ảnh AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e661b951-a574-4960-9c70-4d16516aa834) |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Một thao tác hợp lệ đã được chấp nhận. Quá trình tạo ảnh AI đang chạy.

**Steps:**
1. Tạo thiệp.
2. Khi quá trình tạo ảnh AI đang xử lý, reload hoặc rời trang.
3. Quay lại màn hình liên quan.
4. Quan sát trạng thái.
5. Chờ quá trình xử lý hoàn thành.

**Test Data:**
- —

**Expected Result:**
- quá trình tạo ảnh AI không bị hủy. Khách hàng không thấy quá trình tạo ảnh AI mới bị tạo thêm do reload. Hệ thống hiển thị trạng thái “Đang tạo”. Chưa có mục lịch sử khi chưa có ảnh hợp lệ. Lượt sử dụng đã được ghi nhận từ lúc thao tác được chấp nhận. Khi AI hoàn thành, kết quả được xử lý theo luồng chính.

**Trace to:**
- [STORY-035/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: quá trình tạo ảnh AI tiếp tục xử lý độc lập với trạng thái trang của khách hàng và không bị tạo lại khi reload.

---

## ST-035-08-01 — Kiểm tra giới hạn quota 10 lượt/ngày và 3 lượt/mẫu hoa nguồn

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c309aca5-9f80-4fd5-8d8e-5ebf90e76056) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Dữ liệu tạo thiệp hợp lệ.

**Steps:**
1. Thực hiện tạo thiệp với Tình huống A.
2. Quan sát phản hồi.
3. Chuyển sang tình huống B.
4. Thực hiện tạo thiệp.
5. Quan sát phản hồi.

**Test Data:**
- Scenario A - Lượt sử dụng thiệp AI trong ngày: 10/10 - Lượt tạo thiệp của mẫu hoa nguồn: 1/3 Scenario B - Lượt sử dụng thiệp AI trong ngày: 5/10 - Lượt tạo thiệp của mẫu hoa nguồn: 3/3

**Expected Result:**
- Ở tình huống A, khách hàng không thấy quá trình tạo ảnh AI bắt đầu. Không có ảnh AI mới được tạo. Không xuất hiện mục mới trong lịch sử. Số lượt sử dụng hiển thị không tăng thêm. Hệ thống thông báo hết lượt sử dụng thiệp trong ngày. Ở tình huống B, khách hàng cũng không thấy quá trình tạo ảnh AI bắt đầu. Số lượt sử dụng còn lại không bị giảm. Hệ thống thông báo đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này.

**Trace to:**
- [STORY-035/EXXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-09](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: đồng thời hai giới hạn tạo ảnh: tối đa 10 lượt thiệp AI/ngày/khách hàng và tối đa 3 lượt cho một mẫu hoa nguồn.

---

## ST-035-09-01 — Template hoặc Size không còn khả dụng thì chặn tạo thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/11c1cd1f-2f80-4c5d-82c0-eb68a6bcc48e) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Template và Size đang khả dụng tại thời điểm khách hàng mở form tạo thiệp. Môi trường test cho phép thay đổi trạng thái khả dụng của Template hoặc Size tại nguồn dữ liệu trước khi gửi yêu cầu tạo thiệp.

**Steps:**
1. Mở form tạo thiệp.
2. Chọn Template và Size đang khả dụng.
3. Nhập đầy đủ dữ liệu hợp lệ.
4. Trước khi chọn “Tạo thiệp”, cập nhật dữ liệu hệ thống để Template hoặc Size vừa chọn không còn được xem là khả dụng.
5. Chọn “Tạo thiệp”.
6. Quan sát phản hồi của hệ thống.

**Test Data:**
- Scenario A: - Template đã chọn: TEMPLATE-A - Size đã chọn: SIZE-A - Trước khi submit, TEMPLATE-A được thiết lập thành trạng thái không còn khả dụng. Scenario B: - Template đã chọn: TEMPLATE-A - Size đã chọn: SIZE-A - Trước khi submit, SIZE-A được thiết lập thành trạng thái không còn khả dụng.

**Expected Result:**
- Ở Scenario A, hệ thống phát hiện TEMPLATE-A không còn khả dụng tại thời điểm xử lý yêu cầu. Ở Scenario B, hệ thống phát hiện SIZE-A không còn khả dụng tại thời điểm xử lý yêu cầu. Trong cả hai trường hợp: - Hệ thống không bắt đầu quá trình tạo ảnh AI. - Không có ảnh AI mới được tạo. - Không tạo mục lịch sử mới. - Số lượt sử dụng không bị giảm. - Hệ thống thông báo và yêu cầu khách hàng chọn lại Template hoặc Size đang còn khả dụng.

**Trace to:**
- [STORY-035/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống kiểm tra lại Template và Size tại thời điểm gửi thao tác, không tin trạng thái cũ đã hiển thị ở frontend.

---

## ST-035-10-01 — AI không tạo được ảnh sau retry thì hoàn lượt đúng quy tắc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1cbf97c4-3032-4073-ab4d-2810be7aca72) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thao tác hợp lệ. Lượt sử dụng trong ngày và giới hạn mẫu hoa đều còn lượt. Môi trường test có dữ liệu để quan sát AI không tạo được ảnh hợp lệ.

**Steps:**
1. Thực hiện thao tác tạo thiệp hợp lệ.
2. Quan sát hai lượt được ghi nhận.
3. Thực hiện tình huống AI thất bại.
4. Chờ hệ thống thử lại cho đến khi có kết quả cuối cùng.
5. Chờ quá trình xử lý kết thúc thất bại.
6. Quan sát lịch sử và hai lượt sử dụng.

**Test Data:**
- lượt sử dụng trong ngày trước: 5/10 Mẫu hoa trước: 1/3

**Expected Result:**
- Hệ thống tự thử lại trong lúc khách hàng chờ kết quả. Không xuất hiện mục mới trong lịch sử nếu không có ảnh hợp lệ. Hoàn đúng 1 lượt sử dụng trong ngày. Hoàn đúng 1 lượt tạo ảnh của mẫu hoa nguồn. Mỗi loại lượt chỉ được hoàn tối đa một lần cho quá trình tạo ảnh AI. Không phát sinh thêm lượt sử dụng do quá trình thử lại. Hệ thống thông báo khách hàng thử lại sau.

**Trace to:**
- [STORY-035/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-049](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: quá trình xử lý thất bại không tạo lịch sử và hệ thống hoàn đúng các lượt đã ghi nhận sau khi toàn bộ thử lại kết thúc.

---

## ST-035-11-01 — Chống thao tác trùng (double submit)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/436e60c1-d874-4956-a6e2-484e846a2b1f) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout và dữ liệu hợp lệ. Khách hàng còn lượt sử dụng. Có khả năng gửi cùng thao tác nhiều lần.

**Steps:**
1. Thực hiện thao tác tạo thiệp hợp lệ.
2. Thực hiện lại cùng thao tác đó nhiều lần liên tiếp.
3. Chờ xử lý hoàn tất.
4. Quan sát quá trình tạo ảnh AI.
5. Quan sát ảnh.
6. Quan sát lịch sử.
7. Quan sát lượt sử dụng.

**Test Data:**
- —

**Expected Result:**
- Hệ thống nhận diện thao tác trùng. Tối đa chỉ có 1 lần gọi AI cho thao tác đó. Khách hàng chỉ thấy một ảnh kết quả cho thao tác này. Lịch sử chỉ hiển thị một mục mới cho thao tác này. Chỉ ghi nhận 1 lượt sử dụng trong ngày. Chỉ ghi nhận 1 lượt của mẫu hoa nguồn. Hệ thống trả kết quả tương ứng với thao tác hợp lệ đã được xử lý

**Trace to:**
- [STORY-035/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-07](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-055](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: cơ chế chống thao tác trùng ngăn cùng một thao tác tạo ra nhiều quá trình tạo ảnh AI, ảnh, mục lịch sử hoặc lượt lượt sử dụng.

---

## ST-035-12-01 — Bảo vệ Checkout hợp lệ và xử lý khi xác nhận thiệp thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-035-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ab3a29d-a864-4bf5-9c3e-455f93131b6e) |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B. Có Checkout hợp lệ và Checkout của khách khác. Có Checkout đã hoàn tất. Có một kết quả thiệp AI đã tạo ảnh thành công. Checkout hiện tại có thể đã có một thiệp trước đó.

**Steps:**
- Tình huống A - Checkout không hợp lệ
2. Đăng nhập bằng khách hàng A.
3. Thực hiện thao tác tạo thiệp cho Checkout không tồn tại, Checkout của khách hàng B hoặc Checkout đã hoàn tất.
4. Quan sát phản hồi.
- Tình huống B - Quan sát thất bại
6. Mở một kết quả thiệp AI thành công.
7. Thực hiện tình huống lỗi cập nhật Checkout.
8. Chọn “Quan sát”.
9. Quan sát Checkout, lịch sử và lượt sử dụng.

**Test Data:**
- —

**Expected Result:**
- Tình huống A Khách hàng thấy thao tác bị từ chối. Khách hàng không thấy quá trình tạo ảnh AI bắt đầu. Không có ảnh AI mới được tạo. Không xuất hiện mục mới trong lịch sử. Số lượt sử dụng còn lại không bị giảm. Không làm lộ dữ liệu Checkout hoặc khách hàng khác. Tình huống B mục lịch sử của kết quả vừa tạo ảnh vẫn được giữ. Lượt sử dụng đã dùng vẫn được giữ nguyên. Checkout giữ thiệp trước đó nếu có. Kết quả mới chưa trở thành thiệp hiện tại. Hệ thống thông báo Xác nhận chưa thành công. Khách hàng có thể thử Xác nhận lại. Việc thử Xác nhận không gọi lại AI hoặc tạo lịch sử mới.

**Trace to:**
- [STORY-035/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [STORY-035/EXC-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: hệ thống bảo vệ ownership/trạng thái Checkout khi tạo thiệp và không làm mất kết quả AI nếu thao tác Xác nhận thiệp vào Checkout thất bại.

---

## ST-036-09-01 — Hệ thống bảo vệ quyền sở hữu của thiệp nguồn và Checkout, đồng thời không làm lộ ảnh hoặc metadata của khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdc1f3d7-5002-4df1-99dd-a40e7625ef75) |
| **Story** | STORY-036 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B. Có thiệp thiệp B thuộc khách hàng B. Có Checkout của khách hàng B. Có một Checkout đã hoàn tất. khách hàng A đang đăng nhập.

**Steps:**
--- Tình huống A — Thiệp nguồn không thuộc khách hàng
1. Với khách hàng A, thực hiện thao tác Tạo lại từ thiệp thiệp B.
2. Quan sát phản hồi.
--- Tình huống B — Checkout không hợp lệ
1. Thực hiện Tạo lại trong Checkout của khách hàng B, Checkout không tồn tại hoặc Checkout đã hoàn tất.
2. Quan sát phản hồi.

**Test Data:**
- —

**Expected Result:**
- Tình huống A Khách hàng thấy thao tác bị từ chối. Không trả ảnh thiệp nguồn. Không trả metadata nhạy cảm. Khách hàng không thấy quá trình tạo ảnh AI bắt đầu. Không xuất hiện mục mới trong lịch sử. Số lượt sử dụng còn lại không bị giảm. Tình huống B Hệ thống từ chối thao tác Tạo lại. Không bắt đầu AI. Không xuất hiện mục mới trong lịch sử. Số lượt sử dụng còn lại không bị giảm. Không làm lộ dữ liệu Checkout hoặc khách hàng khác. Hiển thị lỗi phù hợp với nguyên nhân Checkout không hợp lệ.

**Trace to:**
- [STORY-036/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-07](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-035/EXC-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống bảo vệ quyền sở hữu của thiệp nguồn và Checkout, đồng thời không làm lộ ảnh hoặc metadata của khách hàng khác.

---

## ST-057-09-01 — Chỉ các mẫu thiệp đang Active và chưa bị xóa mềm mới được sử dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-057-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f8313002-bf72-4c01-a170-ee03bb2e6b22) |
| **Story** | STORY-057 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Core Database có nhiều mẫu thiệp với các trạng thái Active, Inactive và isDelete khác nhau. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Chuẩn bị mẫu thiệp Active và có isDelete = false.
2. Chuẩn bị mẫu thiệp Inactive và có isDelete = false.
3. Chuẩn bị mẫu thiệp Active và có isDelete = true.
4. Khách hàng truy cập chức năng tạo thiệp.
5. Quan sát danh sách mẫu thiệp khả dụng.

**Test Data:**
- Mẫu thiệp 1: Active, isDelete = false. Mẫu thiệp 2: Inactive, isDelete = false. Mẫu thiệp 3: Active, isDelete = true.

**Expected Result:**
- Hệ thống hiển thị mẫu thiệp Active và có isDelete = false cho khách hàng. Hệ thống không hiển thị mẫu thiệp Inactive. Hệ thống không hiển thị mẫu thiệp có isDelete = true.

**Trace to:**
- [STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-199](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a733299c-59cf-4aa9-8f6e-8f90e358b7bc)

**Rationale:**
> Xác minh quy tắc chỉ các mẫu thiệp đang Active và chưa bị xóa mềm mới được sử dụng trong quy trình tạo thiệp mới.

---

## ST-059-10-01 — Mẫu thiệp Inactive không hiển thị cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9c6a1415-d719-4e54-98d2-8a2b56ec4103) |
| **Story** | STORY-059 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Mẫu thiệp đang Active và có isDelete = false. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận mẫu thiệp Active đang khả dụng cho khách hàng.
2. Admin chuyển mẫu thiệp đó từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách mẫu thiệp khả dụng khi tạo thiệp mới.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp: Trạng thái ban đầu = Active. isDelete = false.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, mẫu thiệp có trạng thái Inactive trong Core Database. Mẫu thiệp Inactive không được hiển thị cho khách hàng khi tạo thiệp mới. Mẫu thiệp vẫn tồn tại và có thể được hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-059/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-059/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-207](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6bf44083-df90-4065-b07a-f9dcf537adff)

**Rationale:**
> Xác minh tác động của trạng thái Inactive lên danh sách mẫu thiệp khả dụng của khách hàng.

---

## ST-059-11-01 — Mẫu thiệp Active trở lại trạng thái khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df2c9dc9-980c-415d-95ee-2b1687a59a8a) |
| **Story** | STORY-059 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Mẫu thiệp đang Inactive và có isDelete = false. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận mẫu thiệp Inactive không xuất hiện trong danh sách mẫu thiệp khả dụng của khách hàng.
2. Admin chuyển mẫu thiệp đó từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách mẫu thiệp khả dụng.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp: Trạng thái ban đầu = Inactive. isDelete = false.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, mẫu thiệp có trạng thái Active trong Core Database. Mẫu thiệp Active có isDelete = false có thể được hiển thị cho khách hàng khi tạo thiệp mới.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-059/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-059/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-206](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a696b79-cdf0-4cec-89c3-b897955dfd13)

**Rationale:**
> Xác minh mẫu thiệp được đưa trở lại trạng thái khả dụng cho khách hàng sau khi Admin chuyển từ Inactive sang Active.

---

## ST-060-08-01 — Mẫu thiệp đã bị xóa mềm không hiển thị trong danh sách khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1333637c-aafe-4ed6-b2f0-91f3c3061b8b) |
| **Story** | STORY-060 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Mẫu thiệp tồn tại và có isDelete = false. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Admin thực hiện xóa mềm mẫu thiệp.
2. Xác nhận mẫu thiệp có isDelete = true trong Core Database.
3. Khách hàng truy cập quy trình tạo thiệp mới.
4. Khách hàng tải danh sách mẫu thiệp khả dụng.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp ban đầu có: isDelete = false. Trạng thái = Active hoặc Inactive.

**Expected Result:**
- Sau khi xóa mềm, mẫu thiệp có isDelete = true. Hệ thống không hiển thị mẫu thiệp đã xóa mềm cho khách hàng lựa chọn khi tạo thiệp mới. Mẫu thiệp có isDelete = true không được trả về như mẫu thiệp khả dụng dù trạng thái quản lý trước đó là Active.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-060/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-060/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-211](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987)

**Rationale:**
> Xác minh mẫu thiệp đã bị xóa mềm không còn được sử dụng trong quy trình tạo thiệp mới.

---

## ST-060-10-01 — Chặn tạo thiệp nếu mẫu thiệp bị xóa mềm trước khi hoàn thành

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-060-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7b25dcef-0ac1-4398-807c-56633f49cf83) |
| **Story** | STORY-060 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang thực hiện quy trình tạo thiệp. Khách hàng đã chọn một mẫu thiệp khi mẫu thiệp có isDelete = false. Khách hàng chưa hoàn thành tạo thiệp. Admin đã đăng nhập và có quyền quản lý mẫu thiệp.

**Steps:**
1. Khách hàng chọn một mẫu thiệp khả dụng.
2. Khách hàng giữ nguyên phiên tạo thiệp và chưa hoàn thành.
3. Admin xóa mềm đúng mẫu thiệp mà khách hàng đang sử dụng.
4. Xác nhận mẫu thiệp có isDelete = true trong Core Database.
5. Khách hàng chọn hoàn thành tạo thiệp.
6. Quan sát phản hồi của hệ thống.
7. Kiểm tra việc tạo thiệp mới.

**Test Data:**
- Một mẫu thiệp ban đầu có: isDelete = false. Khách hàng đã chọn mẫu này trước khi Admin thực hiện xóa mềm.

**Expected Result:**
- Khi khách hàng hoàn thành tạo thiệp, hệ thống kiểm tra lại isDelete và trạng thái mẫu thiệp từ Core Database. Hệ thống xác định mẫu thiệp đã bị xóa mềm. Hệ thống không tạo thiệp mới với mẫu thiệp đó. Hệ thống thông báo mẫu thiệp đã chọn không còn khả dụng. Hệ thống yêu cầu khách hàng chọn một mẫu thiệp khác còn khả dụng.

**Trace to:**
- [STORY-060](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-060/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-060/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/070fba60-8f71-4071-8f6f-f2589395d57b) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-211](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987)

**Rationale:**
> Xác minh hệ thống kiểm tra lại tính khả dụng của mẫu thiệp tại thời điểm xử lý cuối cùng để ngăn sử dụng mẫu đã bị Admin xóa mềm.

---

## ST-063-07-01 — Kích thước Inactive không khả dụng cho khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/93e5791f-1760-41de-9c10-be800c98a9de) |
| **Story** | STORY-063 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp. Kích thước thiệp đang Active và có isDelete = false. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận kích thước Active đang khả dụng cho khách hàng.
2. Admin chuyển kích thước đó từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách kích thước thiệp khả dụng.
5. Quan sát danh sách kích thước thiệp.

**Test Data:**
- Một kích thước thiệp: Trạng thái ban đầu = Active. isDelete = false.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, kích thước thiệp có trạng thái Inactive trong Core Database. Kích thước Inactive không được hiển thị hoặc sử dụng trong danh sách kích thước khách hàng có thể chọn khi tạo thiệp mới. Kích thước vẫn tồn tại và vẫn có thể hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c) · [STORY-063/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-225](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/98255c98-7f84-4115-be4a-69fcb75f6c1c)

**Rationale:**
> Xác minh tác động của trạng thái Inactive lên khả năng sử dụng kích thước trong quy trình tạo thiệp mới.

---

## ST-063-08-01 — Kích thước Active trở lại trạng thái khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/56858ae5-a1ed-4512-95e6-7d14b44be557) |
| **Story** | STORY-063 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình thiệp. Kích thước thiệp đang Inactive và có isDelete = false. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận kích thước Inactive không khả dụng cho khách hàng.
2. Admin chuyển kích thước đó từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách kích thước thiệp khả dụng.
5. Quan sát danh sách kích thước thiệp.

**Test Data:**
- Một kích thước thiệp: Trạng thái ban đầu = Inactive. isDelete = false.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, kích thước thiệp có trạng thái Active trong Core Database. Kích thước Active và có isDelete = false được phép sử dụng khi khách hàng tạo thiệp mới.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c) · [STORY-063/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)

**Rationale:**
> Xác minh kích thước được đưa trở lại trạng thái khả dụng sau khi chuyển từ Inactive sang Active.

---

## ST-063-11-01 — Kích thước Active nhưng isDelete=true không khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-063-11-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7348bbc0-ebb5-469c-bc83-4844bd700375) |
| **Story** | STORY-063 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một kích thước thiệp trong Core Database. Kích thước thiệp có trạng thái Active nhưng isDelete = true. Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Khách hàng tải danh sách kích thước thiệp khả dụng.
2. Quan sát danh sách kích thước thiệp.
3. Thử sử dụng kích thước Active nhưng có isDelete = true nếu có thể gửi yêu cầu trực tiếp.

**Test Data:**
- Kích thước thiệp: Trạng thái = Active. isDelete = true.

**Expected Result:**
- Hệ thống không xem kích thước Active có isDelete = true là kích thước khả dụng. Kích thước đó không được hiển thị hoặc sử dụng trong quy trình tạo thiệp mới.

**Trace to:**
- [STORY-063](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9466330-5d1e-4348-9a43-ccc408ca7d1c) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)

**Rationale:**
> Xác minh điều kiện khả dụng yêu cầu đồng thời trạng thái Active và isDelete = false.

---

## ST-064-08-01 — Cấu hình phụ phí Inactive không tham gia tính phụ phí

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9344cd6e-bff1-43ec-b9be-8ee77b3c9bec) |
| **Story** | STORY-064 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp. Cấu hình phụ phí viết tay đang Active và có isDelete = false. Cấu hình phù hợp với số lượng từ của một thiệp Calligraphy mới.

**Steps:**
1. Xác nhận cấu hình Active đang được sử dụng để xác định phụ phí cho khoảng số lượng từ tương ứng.
2. Admin chuyển cấu hình từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ thuộc khoảng của cấu hình.
5. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Giá phụ phí = 20000. Trạng thái ban đầu = Active. isDelete = false. Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, cấu hình có trạng thái Inactive trong Core Database. Hệ thống không sử dụng cấu hình Inactive để xác định phụ phí cho thiệp Calligraphy mới. Cấu hình Inactive vẫn được giữ trong Core Database và có thể hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428) · [STORY-064/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-229](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2)

**Rationale:**
> Xác minh cấu hình Inactive không còn tham gia xác định phụ phí cho các yêu cầu thiệp Calligraphy mới.

---

## ST-064-09-01 — Cấu hình phụ phí Active trở lại trạng thái khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/818bebb1-4427-401d-a14f-71651968c989) |
| **Story** | STORY-064 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp. Cấu hình phụ phí viết tay đang Inactive và có isDelete = false. Khoảng số lượng từ không chồng lấn với các cấu hình Active khác.

**Steps:**
1. Xác nhận cấu hình Inactive không được sử dụng để xác định phụ phí.
2. Admin chuyển cấu hình từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ phù hợp với khoảng cấu hình.
5. Quan sát cấu hình được sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 11 đến 20. Giá phụ phí = 30000. Trạng thái ban đầu = Inactive. isDelete = false. Không có cấu hình Active khác chồng lấn khoảng 11 đến 20.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, cấu hình có trạng thái Active trong Core Database. Cấu hình Active và có isDelete = false được phép sử dụng để xác định phụ phí cho thiệp Calligraphy mới có số lượng từ phù hợp.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428) · [STORY-064/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87) · [BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)

**Rationale:**
> Xác minh cấu hình được đưa trở lại trạng thái khả dụng khi chuyển từ Inactive sang Active.

---

## ST-064-12-01 — Cấu hình phụ phí Active nhưng isDelete=true không khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-064-12-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7b1555ba-9c6e-490c-bf8e-b5e8816a4995) |
| **Story** | STORY-064 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một cấu hình phụ phí viết tay trong Core Database. Cấu hình có trạng thái Active nhưng isDelete = true. Hệ thống đang xác định phụ phí cho thiệp Calligraphy mới.

**Steps:**
1. Chuẩn bị thiệp Calligraphy có số lượng từ thuộc khoảng của cấu hình.
2. Thực hiện yêu cầu xác định phụ phí.
3. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Trạng thái = Active. isDelete = true. Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Hệ thống không sử dụng cấu hình Active có isDelete = true để xác định phụ phí. Chỉ cấu hình Active và có isDelete = false mới được xem là cấu hình khả dụng.

**Trace to:**
- [STORY-064](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87)

**Rationale:**
> Xác minh điều kiện sử dụng cấu hình yêu cầu đồng thời trạng thái Active và isDelete = false.

---

## ST-065-17-01 — Cập nhật mẫu thiệp không làm thay đổi dữ liệu lịch sử đã phát sinh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-065-17-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b54c3708-881f-44bb-beea-c9018d7441c3) |
| **Story** | STORY-065 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp. Mẫu thiệp đã từng được sử dụng cho thiệp, Checkout hoặc Order đã tồn tại.

**Steps:**
1. Ghi nhận nội dung và ảnh của các thiệp, Checkout hoặc Order đã sử dụng mẫu thiệp.
2. Admin cập nhật tên mẫu, mô tả hoặc ảnh mẫu thiệp.
3. Admin chọn “Lưu”.
4. Kiểm tra lại các thiệp, Checkout hoặc Order đã tồn tại.

**Test Data:**
- Một mẫu thiệp đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp. Checkout. Order.

**Expected Result:**
- Thông tin mới của mẫu thiệp được lưu để sử dụng cho các lần tạo thiệp mới. Các thiệp, Checkout hoặc Order đã tồn tại không bị thay đổi nội dung hoặc ảnh. Dữ liệu lịch sử được giữ nguyên theo thông tin đã lưu tại thời điểm phát sinh.

**Trace to:**
- [STORY-065](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776) · [STORY-065/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/83f53cc7-53c0-444b-804b-b643a7388776) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-236](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df42784d-c617-4436-9d8f-7f8d00515a4c)

**Rationale:**
> Xác minh cập nhật mẫu thiệp không làm thay đổi dữ liệu lịch sử đã phát sinh.

---

## ST-066-07-01 — Cấu hình đã xóa mềm không còn tham gia tính phụ phí

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/128ba548-026e-4295-848b-c1be26567c0f) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp. Cấu hình phụ phí thiệp viết tay đang khả dụng và có isDelete = false. Cấu hình phù hợp với số lượng từ của một thiệp Calligraphy mới.

**Steps:**
1. Xác nhận cấu hình đang được sử dụng cho khoảng số lượng từ tương ứng.
2. Admin xóa mềm cấu hình.
3. Xác nhận cấu hình có isDelete = true trong Core Database.
4. Thực hiện yêu cầu xác định phụ phí cho một thiệp Calligraphy mới có số lượng từ thuộc khoảng đã xóa.
5. Quan sát cấu hình được hệ thống sử dụng.

**Test Data:**
- Cấu hình: Khoảng số lượng từ = 1 đến 10. Giá phụ phí = 20000. isDelete = false trước khi xóa. Thiệp Calligraphy mới có số lượng từ thuộc khoảng 1 đến 10.

**Expected Result:**
- Sau khi xóa thành công, cấu hình có isDelete = true. Hệ thống không sử dụng cấu hình đã xóa mềm để xác định phụ phí cho thiệp Calligraphy mới. Chỉ cấu hình có isDelete = false và thỏa điều kiện khả dụng mới được sử dụng.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-066/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-247](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7c0d412a-add2-48a2-876c-d385faabb75c)

**Rationale:**
> Xác minh cấu hình đã xóa mềm không còn tham gia tính phụ phí cho các thiệp Calligraphy mới.

---

## ST-066-10-01 — Xóa mềm cấu hình không làm thay đổi dữ liệu nghiệp vụ đã phát sinh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-066-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/860cbf12-4e4b-4b5b-8080-bd5b210f2f6f) |
| **Story** | STORY-066 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp. Cấu hình phụ phí thiệp viết tay đã từng được sử dụng để xác định phụ phí. Có thiệp, Checkout hoặc Order đã tồn tại với giá được xác định từ cấu hình đó.

**Steps:**
1. Ghi nhận giá và dữ liệu của thiệp, Checkout hoặc Order đã sử dụng cấu hình.
2. Admin xóa mềm cấu hình phụ phí thiệp viết tay.
3. Xác nhận cấu hình có isDelete = true.
4. Kiểm tra lại giá và dữ liệu của thiệp, Checkout hoặc Order đã tồn tại.

**Test Data:**
- Một cấu hình đã từng được áp dụng cho ít nhất một trong các dữ liệu: Thiệp. Checkout. Order.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái xóa mềm của cấu hình. Hệ thống không tính lại giá của thiệp đã tồn tại. Hệ thống không thay đổi giá hoặc dữ liệu của Checkout đã tồn tại. Hệ thống không thay đổi giá hoặc dữ liệu của Order đã tồn tại. Dữ liệu nghiệp vụ phát sinh trước thời điểm xóa được giữ nguyên.

**Trace to:**
- [STORY-066](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-066/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-066/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1979dd4-f275-4ac7-85c9-9afa7b079717) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-248](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c7fcf4-0616-45f7-b2df-dc75c7aaf952)

**Rationale:**
> Xác minh xóa mềm cấu hình không làm thay đổi dữ liệu nghiệp vụ hoặc giá đã phát sinh trước đó.

---

## ST-068-20-01 — Cập nhật cấu hình không làm thay đổi dữ liệu giá đã phát sinh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-068-20-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4013a769-d495-4752-afac-b99dcc3851c4) |
| **Story** | STORY-068 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý cấu hình giá thiệp. Cấu hình đã từng được sử dụng để xác định phụ phí cho thiệp, Checkout hoặc Order đã tồn tại.

**Steps:**
1. Ghi nhận giá và dữ liệu của thiệp, Checkout hoặc Order đã sử dụng cấu hình.
2. Admin cập nhật khoảng số lượng từ hoặc giá phụ phí của cấu hình.
3. Admin chọn lưu.
4. Kiểm tra lại các dữ liệu nghiệp vụ đã tồn tại.

**Test Data:**
- Một cấu hình đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp. Checkout. Order.

**Expected Result:**
- Thông tin cấu hình mới chỉ được sử dụng cho các lần xác định phụ phí mới. Hệ thống không tính lại giá của thiệp đã tồn tại. Hệ thống không thay đổi giá của Checkout đã tồn tại. Hệ thống không thay đổi giá của Order đã tồn tại. Dữ liệu lịch sử được giữ nguyên theo thông tin tại thời điểm phát sinh.

**Trace to:**
- [STORY-068](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3) · [STORY-068/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3) · [STORY-068/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ea36e44e-2ef0-4e8a-b2d3-01a31a2871e3) · [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43) · [BR-244](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3a150e28-1a62-4b82-a8f3-c838238d674a)

**Rationale:**
> Xác minh cập nhật cấu hình không làm thay đổi dữ liệu giá đã phát sinh trước đó.
