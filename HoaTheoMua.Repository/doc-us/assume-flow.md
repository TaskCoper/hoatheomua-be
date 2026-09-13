# Assume Flow — nhật ký đọc STORY

Tài liệu này được cập nhật tuần tự: đọc toàn bộ nội dung của từng thư mục `STORY` (kể cả `BusinessRules` và `ST` nếu có), ghi nhận flow/API, sau đó đối chiếu lại trước khi chuyển sang STORY kế tiếp.

## Nhật ký

Đã hoàn tất đọc và đối chiếu theo từng STORY. API trong tài liệu không có URL/HTTP method; vì vậy “API” dưới đây là trách nhiệm backend được suy ra trực tiếp từ flow, không phải endpoint tự đặt.

## Customer: Combo → mẫu hoa AI → Checkout/Order

**STORY-003 — danh sách Combo**

`Sản phẩm` → API danh sách Combo → card Combo → search/lọc danh mục–giá/phân trang → **Tạo mẫu hoa** → STORY-030. BR-006…010 (chỉ Combo được phép hiện, empty/error/image fallback); không có BR/ST file cục bộ.

**STORY-030 — khởi tạo yêu cầu mẫu hoa**

`Tạo mẫu hoa trên Combo` → API lấy/xác thực Combo nguồn → Step 1 (tên) → API danh sách Mockup khả dụng → Step 2 (chọn đúng 1 Mockup) → API tạo yêu cầu (khách hàng + Combo/cấu hình theo Size + Mockup) → **Draft** → chi tiết yêu cầu → STORY-033.

BR-068…072, 076…080: tên trim 1–100; Core DB là nguồn xác thực; kiểm tra lại Combo/Mockup lúc hoàn tất; chưa tự gọi AI. Có ST cho flow hai bước, validation, ownership và Draft.

**STORY-033 — tạo mẫu hoa AI (bản chính)**

`Chi tiết yêu cầu Draft` → API trạng thái/quota → `Tạo bó hoa AI ngay` hoặc `Tạo lại` → API tạo AI job → validate owner, dữ liệu STORY-030, không có job chạy, quota, Combo/Size/cấu hình/kiểu bó/Mockup/giấy gói/ruy băng còn hợp lệ → snapshot input → AI service → validate output + gắn logo/lưu storage → history kết quả → màn Kết quả (tải, tạo lại, đặt hàng).

BR-020…032, 070–071, 155, 267–271: Hoa AI tối đa 3 lượt/ngày, trừ khi nhận job và hoàn một lần nếu không có output hợp lệ; chống job/double-click; mỗi job thành công 1 ảnh/1 history; rời trang không hủy job; quota Hoa tách quota Thiệp; snapshot bảo toàn lịch sử. Có ST generate/quota/concurrency/snapshot/output.

**STORY-033 — folder `toa-mau-hoa-bang-AI`**: bản trùng/cũ của STORY-033 (ít validation hơn, không có BR file cục bộ), không phải chức năng riêng.

**STORY-034 — tải mẫu hoa AI**

`Kết quả hoặc History` → API kiểm tra owner + output file → API download đúng PNG đã gắn logo trong Core Storage → `mauhoa_[ma-ket-qua]_[yyyyMMdd_HHmmss].png` không dấu. Không gọi AI/gắn logo lại/trừ quota; tải nhiều lần. BR-058…066; có ST.

**STORY-040 — lịch sử mẫu hoa AI**

API danh sách yêu cầu của chính chủ → nhóm theo yêu cầu, sắp xếp lần generate gần nhất (hoặc ngày tạo) → chọn yêu cầu → API danh sách output hợp lệ → chi tiết/tải hoặc Đặt hàng (STORY-038). Card có tổng output và job mới nhất Chưa tạo/Đang tạo/Đã tạo/Lỗi; job chạy chưa tạo History. BR-095…100; có ST.

**STORY-038 — tạo Checkout từ mẫu hoa**

`Đặt hàng ngay` → API xác thực mẫu hoa/owner/tồn kho Combo → API tạo Checkout tạm gắn đúng 1 mẫu hoa → địa chỉ/VAT → thiệp mới STORY-035 **hoặc** thiệp History STORY-042 → API tạm tính/phí ship Ahamove → STORY-039. Chưa tạo Order; không regenerate thiệp từ History ở Checkout. BR-081…086,153,156; có ST.

**STORY-039 — hoàn tất Checkout/thanh toán**

`Hoàn tất` → API validate Checkout/tính lại tổng → snapshot mẫu hoa và thiệp Custom mới → API tạo Order PENDING + giao dịch → payment provider/xác minh callback → Payment SUCCESS → bước thiệp tương ứng. Retry payment trong 24h; hết hạn tự hủy/giải phóng tồn; bill/snapshot không bị cấu hình mới thay đổi. BR-087…094,272; có ST smoke/full/regression.

## Customer: Thiệp Custom

**STORY-035 — cấu hình thiệp tại Checkout**

`Checkout` → API metadata cấu hình còn hiệu lực → Không thiệp/Có thiệp → miễn phí hoặc Custom → Custom: In/Viết tay + Size + Template + tối đa 1 ảnh + nội dung → API validate/đếm từ/tính giá → API lưu Checkout → STORY-039 snapshot. Không tạo ảnh/gọi AI. BR-051…054,056,272; có ST.

**STORY-036 — tạo lại thiệp AI sau thanh toán**

`Order Payment SUCCESS/Thiệp Custom` → API validate Order, owner, snapshot, quota, giới hạn mẫu hoa → API AI job → trừ quota thiệp + lượt mẫu hoa → AI tạo 1 PNG không logo → history mới → kết quả/Xác nhận. Không áp dụng History; nội dung sửa không vượt số từ đã trả; tối đa 3 lượt theo mẫu hoa. BR-044,046,048,154,156,272; có ST.

Các BR chỉ có ở `BusinessRules` cấp gốc nhưng liên quan trực tiếp cụm này đã được đọc thêm: **BR-049** trừ đúng một quota Thiệp khi job được chấp nhận và hoàn một lần khi không có output hợp lệ; **BR-050** mỗi output hợp lệ tạo đúng một History record, không ghi đè kết quả cũ; **BR-055** request generate thiệp trùng phải idempotent (không nhiều job/ảnh/history/quota); **BR-057** AI chỉ trang trí trong phạm vi Template, không đổi nội dung bắt buộc/layout/phong cách/ảnh đính kèm, chỉ tham chiếu Combo nguồn để đồng bộ phong cách.

**STORY-042 — chọn thiệp History cho Checkout**

`Checkout chưa hoàn tất` → API danh sách thiệp History chính chủ, output/file hợp lệ, cùng mẫu hoa → API validate/gắn thiệp được chọn vào Checkout (gỡ liên kết tạm thiệp cũ) → summary → STORY-039. Không AI/copy ảnh/history mới/quota; History được tái sử dụng và bảo toàn. BR-110…117; có ST.

**STORY-045 — lịch sử thiệp customer**

API danh sách History output chính chủ, mới nhất → chi tiết thiệp/Orders liên quan → tải STORY-041. Không có Tạo lại trên list/chi tiết History. BR-044,133…139; có ST.

**STORY-041 — customer tải thiệp**

`Chi tiết History/Order` → API kiểm tra History owner + file, và Order liên kết nếu tải từ Order → API download PNG gốc → tên theo mã Order hoặc mã thiệp. Không AI/copy/history/quota/thay đổi dữ liệu. BR-101…109; có ST.

## Admin: xem tài sản AI đã tạo

**STORY-044 danh sách thiệp AI** → API paged History thiệp output hợp lệ (mới nhất, search/filter, preview/lightbox) → STORY-043. Item: preview, mã, customer, template, size, hình thức, ngày. BR-126…132; có ST.

**STORY-043 chi tiết thiệp AI** → API chi tiết (ảnh/mã/customer/lời chúc/ảnh đính kèm/template/size/hình thức/ngày/file/Orders) → lightbox, link Order, tải STORY-046. BR-121…123,125; có ST.

**STORY-046 admin tải thiệp** → API quyền + History/file → download PNG gốc `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`; không AI/history/quota. BR-140…143; có ST.

**STORY-047 danh sách mẫu hoa AI** → API paged kết quả output hợp lệ (mới nhất, search/filter, preview) → STORY-048. Item: preview, mã, customer, Combo nguồn, ngày. BR-132,144…146,150…152; có ST.

**STORY-048 chi tiết mẫu hoa AI** → API chi tiết: output, customer, Combo nguồn, Core/Support và số lượng snapshot, Mockup, note, ngày/file/Orders → lightbox/link Order. Hiện dữ liệu lúc generate, không dùng status Combo hiện tại. BR-132,147…151; có ST.

## Admin: Mockup (đầu vào mẫu hoa)

`STORY-050 danh sách` → `STORY-051 thêm` → bản ghi có `STORY-052 đổi trạng thái` hoặc `STORY-053 xóa mềm`.

- **050:** API Mockup `isDelete=false`, table/paging (tên/mô tả/preview/status); BR-163; không ST.
- **051:** form tên/mô tả/ảnh → API upload/tạo Core DB → mặc định Active; tên không trùng, tạo atomic. BR-163…168,253; không ST.
- **052:** xác nhận → API Active/Inactive; Inactive không cho customer chọn mới, không xóa; không được Inactive Mockup Active cuối cùng. BR-169…175; không ST.
- **053:** xác nhận → API `isDelete=true`; biến mất khỏi list/customer mới nhưng giữ snapshot lịch sử. BR-176,181…184,186; có ST.

## Admin: cấu hình Thiệp Custom

### Size thiệp

`STORY-054 danh sách` → `STORY-055 thêm` / `STORY-062 cập nhật` / `STORY-063 đổi status` / `STORY-056 xóa mềm`.

- **054:** API list Size chưa xóa; Size/giá/max words/status, paging/filter. BR-187…190; có ST.
- **055:** API create width/height/giá/max words, mặc định Active; format/cặp Size không trùng. BR-191…193,250; có ST.
- **062:** API detail/update; chống trùng cặp Size chưa xóa. BR-219…222; có ST.
- **063:** API đổi Active/Inactive; Active được chọn mới, Inactive không, snapshot giữ nguyên. BR-223…227; có ST.
- **056:** API soft delete; không hiện/customer mới, không đổi lịch sử. BR-194…196; có ST.

### Mẫu thiệp/Template

`STORY-057 danh sách` → `STORY-058 thêm` / `STORY-065 cập nhật` / `STORY-059 đổi status` / `STORY-060 xóa mềm`.

- **057:** API list Template chưa xóa, preview lightbox. BR-197…200; có ST.
- **058:** API upload/create tên–mô tả–template, mặc định Active; tên được phép trùng. BR-201…204,251; có ST.
- **065:** API detail/update tên/mô tả/ảnh, giữ status, snapshot cũ không đổi. BR-232…236; có ST.
- **059:** API đổi status; Inactive không hiện customer, cảnh báo nếu Template Active cuối cùng. BR-205…209,252; có ST.
- **060:** API soft delete; không hard-delete ảnh, history giữ nguyên. BR-210…214; có ST.

### Phụ phí viết tay

`STORY-061 danh sách` → `STORY-067 thêm` / `STORY-068 cập nhật` / `STORY-064 đổi status` / `STORY-066 xóa mềm`.

- **061:** API list cấu hình `isDelete=false` (from-word/to-word/phụ phí/status). BR-215…218; có ST.
- **067:** API create range/giá/status; đầu ≤ cuối, giá ≥0, range Active không overlap. BR-237…241; có ST.
- **068:** API detail/update, kiểm tra range/giá/overlap, giữ status/snapshot cũ. BR-238…245; có ST.
- **064:** API đổi status; Active được áp dụng, Inactive không cho Calligraphy mới. BR-218,228…231,239; có ST.
- **066:** API soft delete; không áp dụng mới, dữ liệu cũ không đổi. BR-246…249; có ST.

## Admin: cấu hình thêm cho Hoa Custom AI

### Hình ảnh kiểu gói

`STORY-074 danh sách` → `STORY-075 thêm` / `STORY-076 cập nhật` / `STORY-078 đổi status` / `STORY-077 xóa`.

- **074:** API list table/grid, preview/tên/mô tả/status, lọc; không có BR/ST file cục bộ.
- **075:** API upload/create JPG/PNG ≤10MB, tên/mô tả, mặc định Inactive. BR-274; không ST.
- **076:** API detail/update, preview mới phải JPG/PNG ≤10MB; giữ status. BR-275; không ST.
- **078:** API Active/Inactive; Active hiện trong customer chọn kiểu gói mới, Inactive không. BR-274,277; không ST.
- **077:** confirm → API xóa theo cơ chế lưu trữ hiện hành → refresh; snapshot cũ không đổi. BR-276; không ST.

### Size Hoa Custom AI

`STORY-079 danh sách` → `STORY-080 thêm` / `STORY-081 cập nhật` / `STORY-083 đổi status` / `STORY-082 xóa`.

- **079:** API list config Size chưa xóa, search/filter/status/action.
- **080:** API create Size + Combo count nguyên dương; Size không rỗng/không trùng; mặc định Inactive.
- **081:** API detail/update; Size không trùng record chưa xóa, count nguyên dương, giữ status.
- **083:** API Active/Inactive; Active mới hiện ở luồng chọn Size customer.
- **082:** confirm → API xóa theo cơ chế lưu trữ hiện hành → refresh.
- 079–083 chỉ tham chiếu BR-273 trong story, không có BR/ST file cục bộ.

## Rà soát cuối

- Đã đọc đủ 47 thư mục `STORY-*`; hai thư mục STORY-033 đã được tách rõ bản chính/bản trùng.
- Đã đối chiếu các ràng buộc xuyên suốt: ownership, quota Hoa/Thiệp tách biệt, snapshot/history, soft delete, Active/Inactive, tồn kho và payment retry/24h. Đã rà thêm 5 file chỉ ở `BusinessRules` cấp gốc (BR-049, BR-050, BR-055, BR-057 và README).
- Các flow trên đều ghi điểm nối UI → API/chức năng backend → dữ liệu/trạng thái tiếp theo; không tự tạo URL hay method API ngoài tài liệu.
