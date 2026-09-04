# ST — STORY-059 — Admin chuyển trạng thái mẫu thiệp — System Tests

---

## ST-059-01-01 — Chuyển trạng thái từ Active sang Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b66bfae9-c736-4cef-b5fb-c81e5dfa15fc) |
| **Story** | STORY-059 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.
- Mẫu thiệp đang ở trạng thái Active. Mẫu thiệp chưa bị xóa mềm.
- Vẫn còn ít nhất một mẫu thiệp Active khác có `isDelete = false`.

**Steps:**
1. Admin xem danh sách mẫu thiệp.
2. Admin chọn thao tác chuyển sang Inactive tại mẫu thiệp đang Active.
3. Quan sát popup xác nhận.
4. Admin xác nhận thao tác.
5. Quan sát thông báo của hệ thống.
6. Quan sát trạng thái của mẫu thiệp trong danh sách.
7. Quan sát thao tác được hiển thị sau khi cập nhật.
8. Kiểm tra trạng thái mẫu thiệp trong Core Database.

**Test Data:**
- Mẫu thiệp có: Trạng thái = Active. `isDelete = false`. Không phải mẫu thiệp Active cuối cùng.

**Expected Result:**
- Hệ thống hiển thị popup xác nhận chuyển mẫu thiệp sang Inactive.
- Sau khi Admin xác nhận, hệ thống cập nhật đúng mẫu thiệp từ Active sang Inactive.
- Trạng thái mới được lưu vào Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Inactive.
- Thao tác của mẫu thiệp được thay đổi thành chuyển sang Active.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-205](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0)
- [BR-207](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6bf44083-df90-4065-b07a-f9dcf537adff)
- [BR-208](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dc2e025-0f1f-4848-b240-83574727fa54)
- [BR-209](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceaf3267-aaf7-4dea-a4be-649351f01f57)

**Rationale:**
> Xác minh luồng chính khi Admin chuyển một mẫu thiệp Active sang Inactive và trạng thái được cập nhật nhất quán giữa Core Database và danh sách quản trị.

---

## ST-059-02-01 — Chuyển trạng thái từ Inactive sang Active

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/26b832ee-be74-402a-8195-5bbcd8fa4955) |
| **Story** | STORY-059 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.
- Mẫu thiệp đang ở trạng thái Inactive. Mẫu thiệp chưa bị xóa mềm.

**Steps:**
1. Admin xem danh sách mẫu thiệp.
2. Quan sát thao tác chuyển sang Active tại mẫu thiệp đang Inactive.
3. Admin chọn chuyển sang Active.
4. Quan sát popup xác nhận.
5. Admin xác nhận thao tác.
6. Quan sát thông báo của hệ thống.
7. Quan sát trạng thái của mẫu thiệp trong danh sách.
8. Quan sát thao tác được hiển thị sau khi cập nhật.
9. Kiểm tra trạng thái mẫu thiệp trong Core Database.

**Test Data:**
- Mẫu thiệp có: Trạng thái = Inactive. `isDelete = false`.

**Expected Result:**
- Hệ thống hiển thị thao tác chuyển sang Active cho mẫu thiệp Inactive.
- Hệ thống hiển thị popup xác nhận khi Admin chọn thao tác.
- Sau khi Admin xác nhận, hệ thống cập nhật đúng mẫu thiệp từ Inactive sang Active.
- Trạng thái mới được lưu vào Core Database.
- Hệ thống thông báo cập nhật trạng thái thành công.
- Danh sách hiển thị trạng thái mới là Active.
- Thao tác của mẫu thiệp được thay đổi thành chuyển sang Inactive.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-205](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0)
- [BR-206](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a696b79-cdf0-4cec-89c3-b897955dfd13)
- [BR-208](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dc2e025-0f1f-4848-b240-83574727fa54)
- [BR-209](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceaf3267-aaf7-4dea-a4be-649351f01f57)

**Rationale:**
> Xác minh Alternative Flow chuyển mẫu thiệp từ Inactive sang Active và cập nhật đúng trạng thái quản lý.

---

## ST-059-03-01 — Hủy thao tác chuyển trạng thái tại popup

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/617039d5-37e6-40a9-86ad-8b46f7cc3164) |
| **Story** | STORY-059 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.
- Mẫu thiệp đang ở trạng thái Active hoặc Inactive.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái mẫu thiệp.
2. Quan sát popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Quan sát trạng thái mẫu thiệp trong danh sách.
5. Kiểm tra trạng thái mẫu thiệp trong Core Database.
6. Thực hiện với: Trường hợp 1: mẫu thiệp Active. Trường hợp 2: mẫu thiệp Inactive.

**Test Data:**
- N/A

**Expected Result:**
- Hệ thống đóng popup xác nhận.
- Hệ thống không cập nhật trạng thái mẫu thiệp.
- Mẫu thiệp giữ nguyên trạng thái trước khi thao tác.
- Danh sách mẫu thiệp không thay đổi trạng thái của mẫu thiệp đó.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-205](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0)

**Rationale:**
> Xác minh thao tác hủy hoặc đóng popup không gây thay đổi trạng thái mẫu thiệp.

---

## ST-059-04-01 — Cảnh báo khi chuyển mẫu thiệp Active cuối cùng sang Inactive

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f2b5dbc-01e6-40f8-9155-324e390c4ae7) |
| **Story** | STORY-059 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Hệ thống chỉ còn một mẫu thiệp Active có `isDelete = false`. Mẫu thiệp đang được hiển thị trong danh sách quản lý.

**Steps:**
1. Admin chọn chuyển mẫu thiệp Active cuối cùng sang Inactive.
2. Quan sát cảnh báo của hệ thống.
3. Admin chọn “Hủy”.
4. Kiểm tra trạng thái mẫu thiệp.
5. Thực hiện lại thao tác chuyển sang Inactive.
6. Admin xác nhận tiếp tục tại cảnh báo.
7. Quan sát kết quả cập nhật.
8. Kiểm tra trạng thái trong Core Database.

**Test Data:**
- Core Database chỉ có một mẫu thiệp thỏa: Trạng thái = Active. `isDelete = false`. Các mẫu thiệp còn lại là Inactive hoặc đã bị xóa mềm.

**Expected Result:**
- Hệ thống xác định đây là mẫu thiệp Active cuối cùng có `isDelete = false`.
- Hệ thống cảnh báo rằng nếu tiếp tục, khách hàng sẽ không còn mẫu thiệp Active để lựa chọn khi tạo thiệp mới.
- Admin có thể hủy hoặc xác nhận tiếp tục thao tác.
- Nếu Admin chọn “Hủy”, trạng thái mẫu thiệp được giữ nguyên là Active.
- Nếu Admin xác nhận tiếp tục, hệ thống chuyển mẫu thiệp sang Inactive theo Main Flow.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-252](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/84d54964-ed81-45f4-a2c0-4260614f8207)

**Rationale:**
> Xác minh hệ thống cảnh báo đúng trường hợp chuyển mẫu thiệp Active cuối cùng sang Inactive nhưng vẫn cho phép Admin quyết định tiếp tục hoặc hủy.

---

## ST-059-05-01 — Không cảnh báo khi không phải mẫu thiệp Active cuối cùng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a279e5f0-b2bc-44e1-8761-cafbc9911431) |
| **Story** | STORY-059 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Có ít nhất hai mẫu thiệp Active có `isDelete = false`.

**Steps:**
1. Admin chọn chuyển một mẫu thiệp Active sang Inactive.
2. Quan sát popup hoặc cảnh báo được hiển thị.
3. Admin xác nhận chuyển trạng thái.
4. Quan sát kết quả cập nhật.

**Test Data:**
- Mẫu thiệp A: Active, `isDelete = false`. Mẫu thiệp B: Active, `isDelete = false`.

**Expected Result:**
- Hệ thống không hiển thị cảnh báo dành cho trường hợp mẫu thiệp Active cuối cùng.
- Hệ thống thực hiện flow chuyển trạng thái thông thường.
- Sau khi Admin xác nhận, mẫu thiệp được chuyển sang Inactive.
- Ít nhất một mẫu thiệp Active có `isDelete = false` vẫn còn lại.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-252](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/84d54964-ed81-45f4-a2c0-4260614f8207)

**Rationale:**
> Xác minh cảnh báo đặc biệt chỉ xuất hiện khi mẫu thiệp được chuyển thực sự là mẫu Active cuối cùng.

---

## ST-059-06-01 — Mẫu thiệp không hợp lệ tại thời điểm xác nhận

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/36cc4ae8-ac42-438b-8ff3-de4f0e8c839b) |
| **Story** | STORY-059 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Admin đã mở danh sách mẫu thiệp và chuẩn bị thực hiện chuyển trạng thái.

**Steps:**
1. Chuẩn bị trường hợp mẫu thiệp bị xóa hoặc xóa mềm sau khi danh sách đã được tải.
2. Admin xác nhận thao tác chuyển trạng thái mẫu thiệp đó.
3. Quan sát phản hồi của hệ thống.
4. Quan sát danh sách sau phản hồi.
5. Kiểm tra Core Database.

**Test Data:**
- Trường hợp 1: mẫu thiệp không còn tồn tại trong Core Database.
- Trường hợp 2: mẫu thiệp có `isDelete = true` trước thời điểm yêu cầu cập nhật được xử lý.

**Expected Result:**
- Hệ thống không thực hiện cập nhật trạng thái.
- Hệ thống thông báo mẫu thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
- Hệ thống tải lại danh sách mẫu thiệp.
- Không phát sinh thay đổi trạng thái trên dữ liệu không còn hợp lệ.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-205](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0)

**Rationale:**
> Xác minh hệ thống kiểm tra lại dữ liệu tại thời điểm xử lý để tránh cập nhật mẫu thiệp không còn tồn tại hoặc đã bị xóa mềm.

---

## ST-059-07-01 — Cập nhật Core Database thất bại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b1765f0b-e6d7-41cc-8a2e-c3b9ccb99698) |
| **Story** | STORY-059 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Tài khoản Admin đang hoạt động. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại và chưa bị xóa mềm.
- Có thể tạo tình huống cập nhật Core Database thất bại.

**Steps:**
1. Admin chọn thao tác chuyển trạng thái mẫu thiệp.
2. Admin xác nhận thao tác.
3. Tạo tình huống hệ thống gặp lỗi trong quá trình cập nhật trạng thái.
4. Quan sát phản hồi của hệ thống.
5. Quan sát trạng thái mẫu thiệp trong danh sách.
6. Kiểm tra trạng thái trong Core Database.
7. Khôi phục hệ thống về trạng thái bình thường và thử lại.

**Test Data:**
- Một mẫu thiệp Active hoặc Inactive có `isDelete = false`.
- Môi trường test có khả năng tạo lỗi khi cập nhật Core Database.

**Expected Result:**
- Hệ thống không thay đổi trạng thái mẫu thiệp khi quá trình cập nhật thất bại.
- Mẫu thiệp giữ nguyên trạng thái trước khi thao tác.
- Hệ thống thông báo cập nhật trạng thái thất bại.
- Không tồn tại trạng thái dở dang hoặc không đồng nhất.
- Admin có thể thử lại sau khi lỗi được xử lý.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-205](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0)

**Rationale:**
> Xác minh lỗi cập nhật không tạo trạng thái dở dang và Core Database vẫn là nguồn xác thực cuối cùng.

---

## ST-059-08-01 — Người dùng không có quyền quản lý mẫu thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/be898642-a95f-4a16-99e3-608ab82d7bcc) |
| **Story** | STORY-059 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Người dùng đã đăng nhập.
- Người dùng không có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database và chưa bị xóa mềm.

**Steps:**
1. Người dùng gửi yêu cầu chuyển trạng thái mẫu thiệp.
2. Quan sát phản hồi của hệ thống.
3. Kiểm tra trạng thái mẫu thiệp trong Core Database.

**Test Data:**
- Tài khoản không có quyền quản lý mẫu thiệp.
- Một mẫu thiệp có trạng thái Active hoặc Inactive và `isDelete = false`.

**Expected Result:**
- Backend kiểm tra quyền trước khi cập nhật trạng thái.
- Hệ thống từ chối yêu cầu.
- Hệ thống không thay đổi trạng thái mẫu thiệp.
- Hệ thống hiển thị thông báo phù hợp.
- Core Database giữ nguyên trạng thái trước khi thao tác.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-209](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceaf3267-aaf7-4dea-a4be-649351f01f57)

**Rationale:**
> Xác minh chỉ Admin có quyền quản lý mẫu thiệp mới được phép chuyển trạng thái mẫu thiệp.

---

## ST-059-09-01 — Không làm mất dữ liệu lịch sử liên quan

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-059-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6dbc7143-1097-4bf8-9c3c-b552e77b2c1d) |
| **Story** | STORY-059 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp đang Active và có `isDelete = false`.
- Mẫu thiệp đã được sử dụng để tạo thiệp hoặc đã liên kết với Checkout, History hoặc Order.

**Steps:**
1. Ghi nhận các dữ liệu thiệp, Checkout, History hoặc Order đang liên kết với mẫu thiệp.
2. Admin chuyển mẫu thiệp từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Kiểm tra trạng thái mẫu thiệp trong Core Database.
5. Kiểm tra ảnh template/ảnh Preview.
6. Kiểm tra lại các dữ liệu lịch sử đã sử dụng mẫu thiệp.

**Test Data:**
- Một mẫu thiệp Active, `isDelete = false` đã được sử dụng bởi ít nhất một trong các dữ liệu: Thiệp, Checkout, History, Order.

**Expected Result:**
- Hệ thống chỉ cập nhật trạng thái mẫu thiệp từ Active sang Inactive.
- Mẫu thiệp vẫn tồn tại trong Core Database. Ảnh template/ảnh Preview không bị xóa.
- Các thiệp, Checkout, History hoặc Order đã sử dụng mẫu thiệp trước thời điểm chuyển trạng thái không bị xóa hoặc thay đổi.
- Các liên kết lịch sử vẫn được giữ nguyên.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [BR-208](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dc2e025-0f1f-4848-b240-83574727fa54)

**Rationale:**
> Xác minh việc chuyển trạng thái chỉ ảnh hưởng khả năng sử dụng mẫu thiệp cho yêu cầu mới và không làm mất dữ liệu lịch sử.

---

## ST-059-10-01 — Tác động của trạng thái Inactive lên danh sách khả dụng của khách hàng

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
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp đang Active và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận mẫu thiệp Active đang khả dụng cho khách hàng.
2. Admin chuyển mẫu thiệp đó từ Active sang Inactive.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách mẫu thiệp khả dụng khi tạo thiệp mới.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp: Trạng thái ban đầu = Active. `isDelete = false`.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, mẫu thiệp có trạng thái Inactive trong Core Database.
- Mẫu thiệp Inactive không được hiển thị cho khách hàng khi tạo thiệp mới.
- Mẫu thiệp vẫn tồn tại và có thể được hiển thị trong màn hình quản trị.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/MAIN](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-207](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6bf44083-df90-4065-b07a-f9dcf537adff)

**Rationale:**
> Xác minh tác động của trạng thái Inactive lên danh sách mẫu thiệp khả dụng của khách hàng.

---

## ST-059-11-01 — Tác động của trạng thái Active lên danh sách khả dụng của khách hàng

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
- Admin đã đăng nhập. Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp đang Inactive và có `isDelete = false`.
- Khách hàng có thể truy cập quy trình tạo thiệp mới.

**Steps:**
1. Xác nhận mẫu thiệp Inactive không xuất hiện trong danh sách mẫu thiệp khả dụng của khách hàng.
2. Admin chuyển mẫu thiệp đó từ Inactive sang Active.
3. Admin xác nhận thao tác.
4. Khách hàng tải lại danh sách mẫu thiệp khả dụng.
5. Quan sát danh sách mẫu thiệp.

**Test Data:**
- Một mẫu thiệp: Trạng thái ban đầu = Inactive. `isDelete = false`.

**Expected Result:**
- Sau khi chuyển trạng thái thành công, mẫu thiệp có trạng thái Active trong Core Database.
- Mẫu thiệp Active có `isDelete = false` có thể được hiển thị cho khách hàng khi tạo thiệp mới.

**Trace to:**
- [STORY-059](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-059/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/75cb2abb-9eb0-40d9-8d7b-1033fbda15b8)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [BR-206](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a696b79-cdf0-4cec-89c3-b897955dfd13)

**Rationale:**
> Xác minh mẫu thiệp được đưa trở lại trạng thái khả dụng cho khách hàng sau khi Admin chuyển từ Inactive sang Active.
