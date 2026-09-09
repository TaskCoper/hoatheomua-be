# AI Marketing - Tổng quan luồng User Story

Tài liệu này cô đọng 19 User Story trong `HoaTheoMua.Repository/ai-mkt` để đọc nhanh trước khi phân tích wireframe, thiết kế API và viết TDD. Tên route cụ thể tùy convention của dự án; phần “Góc nhìn BE” chỉ mô tả API/query/command và dữ liệu cần có.

## 1. Nên đọc US nào trước?

### Nhóm cốt lõi - bắt buộc nắm

1. `STORY-004`, `STORY-005`, `STORY-012`, `STORY-013`, `STORY-006`: quản lý System Prompt.
2. `STORY-017`: tạo content và hashtag bằng AI.
3. `STORY-003`: tạo ảnh theo tỷ lệ bằng AI.
4. `STORY-018`, `STORY-021`: thư viện content và ảnh.
5. `STORY-024`: viết lại content theo nền tảng.
6. `STORY-002`: lên lịch và tự động đăng bài.
7. `STORY-014`, `STORY-016`, `STORY-015`: quản lý lịch đăng.
8. `STORY-025`: thu thập, xem và tải báo cáo.

### Nhóm hỗ trợ

- `STORY-019`, `STORY-020`, `STORY-022`, `STORY-023`: sửa/xóa tài nguyên đã lưu.

### Quy ước ưu tiên

- Tất cả 19 User Story đều thuộc phạm vi ưu tiên triển khai.
- `Priority: Won't` trong metadata gốc của `STORY-006`, `STORY-012`, `STORY-013` là thông tin sai/chưa cập nhật.

## 2. Flow nghiệp vụ tổng quát

```text
System Prompt
STORY-004 xem prompt
    |-> STORY-012 tìm kiếm
    |-> STORY-013 lọc
    |-> STORY-005 sửa và tạo phiên bản mới
    `-> STORY-006 xóa prompt không còn được sử dụng

Tạo tài nguyên marketing
STORY-017 tạo content -------> STORY-018 thư viện content
                                  |-> STORY-020 sửa
                                  |-> STORY-019 xóa
                                  |-> STORY-024 viết lại theo nền tảng -> lưu content mới

STORY-003 tạo ảnh -----------> STORY-021 thư viện ảnh
                                  |-> STORY-023 sửa metadata
                                  |-> STORY-022 xóa

Xuất bản
Content + media -> STORY-002 lên lịch -> STORY-014 xem lịch
                                         |-> STORY-016 sửa lịch
                                         |-> STORY-015 xóa lịch
                                         `-> Job tự động đăng theo từng nền tảng

Báo cáo
Nền tảng đã kết nối -> STORY-025 thu thập định kỳ -> xem chi tiết -> tải XLSX
```

Luồng chính của dự án là: **cấu hình AI -> tạo content/ảnh -> quản lý tài nguyên -> lên lịch -> tự động đăng -> thu thập báo cáo**.

## 3. Cụm System Prompt

```text
STORY-004 xem danh sách/chi tiết
    |-> STORY-012 tìm kiếm
    |-> STORY-013 lọc
    |-> STORY-005 sửa -> tạo version mới -> AI dùng version mới
    `-> STORY-006 kiểm tra sử dụng -> xóa
```

### STORY-004 - Xem danh sách và chi tiết System Prompt

**Main flow:** Admin mở danh sách -> BE trả prompt theo `UpdatedAt` giảm dần -> Admin chọn một dòng -> BE trả chi tiết prompt read-only.

**Góc nhìn BE:** Cần query danh sách và query chi tiết theo `promptId`. Danh sách trả tối thiểu `id`, tên/mã prompt, `updatedAt`; chi tiết trả nội dung prompt, version hiện tại và thông tin cập nhật. Không tìm thấy phải trả trạng thái rõ ràng, không trả nhầm bản ghi khác.

**Rule/validate:** `BR-040` bắt buộc sắp xếp prompt mới cập nhật trước. Phải xử lý danh sách rỗng, lỗi tải và prompt bị xóa giữa lúc xem danh sách với lúc lấy chi tiết.

**Done khi:** AC list/rỗng/sort đúng đều pass; lấy đúng detail theo ID; detail chỉ đọc; lỗi và not-found có response để FE hiển thị/retry.

### STORY-012 - Tìm kiếm System Prompt

**Main flow:** Admin nhập từ khóa -> BE tìm theo mã hoặc tên -> trả danh sách phù hợp và tổng số kết quả.

**Góc nhìn BE:** Query nhận `keyword`; áp dụng tìm kiếm lên code/name và kết hợp với thứ tự mặc định của `STORY-004`. Response dùng cùng model với API danh sách để FE không phải xử lý hai kiểu dữ liệu.

**Rule/validate:** Hỗ trợ khớp mã đầy đủ hoặc một phần; từ khóa không có kết quả trả danh sách rỗng, không phải lỗi. Story chưa có BR riêng.

**Done khi:** AC tìm theo mã/tên/chuỗi con pass; tổng số đúng; từ khóa không khớp trả empty result; không phá sort mặc định.

### STORY-013 - Lọc System Prompt

**Main flow:** Admin chọn chức năng áp dụng, trạng thái hoặc khoảng thời gian -> BE áp dụng đồng thời các điều kiện -> trả kết quả và tổng số.

**Góc nhìn BE:** Query list nhận các filter tùy chọn như `function`, `status`, `updatedFrom`, `updatedTo`. Nhiều filter phải là điều kiện AND và có thể kết hợp với `keyword` của `STORY-012`.

**Rule/validate:** Validate enum trạng thái/chức năng và khoảng ngày hợp lệ. Xóa filter phải quay về danh sách mặc định. Story chưa có BR riêng.

**Done khi:** AC từng filter và tổ hợp filter pass; tổng số đúng; không có kết quả trả danh sách rỗng; filter sai trả validation error.

### STORY-005 - Sửa System Prompt

**Main flow:** Admin lấy detail -> sửa nội dung -> gửi nội dung cùng version đã đọc -> BE validate -> tạo version mới -> đặt version mới làm bản hiện tại.

**Góc nhìn BE:** Command cập nhật nhận `promptId`, `content`, version/concurrency token. Việc tạo version, cập nhật current version và audit phải cùng transaction. Response trả version mới, người sửa và thời gian sửa.

**Rule/validate:** `BR-041` nội dung 1-20.000 ký tự; `BR-042` giữ Unicode, Markdown, xuống dòng và khoảng trắng; `BR-045` optimistic locking; `BR-052` nội dung không đổi thì không tạo version. `BR-043`, `BR-044` đang thiếu nội dung trong US gốc.

**Done khi:** AC lưu hợp lệ, rỗng/quá dài, unchanged, conflict và rollback đều pass; version cũ còn trong lịch sử; AI dùng version mới sau khi lưu thành công.

### STORY-006 - Xóa System Prompt

**Main flow:** Admin chọn xóa -> BE kiểm tra prompt có đang được chức năng AI sử dụng -> FE xác nhận -> BE kiểm tra lại và xóa -> trả kết quả để tải lại danh sách.

**Góc nhìn BE:** Cần query điều kiện xóa và command xóa theo `promptId`. Điều kiện sử dụng phải được kiểm tra lại tại thời điểm xác nhận để tránh race condition. Không được làm AI mất prompt đang cấu hình.

**Rule/validate:** Chặn prompt đang được tham chiếu; xử lý not-found và lỗi database; không xóa trước bước xác nhận. US chưa có BR riêng và AC chặn prompt đang dùng chưa mô tả đủ rõ.

**Done khi:** AC xác nhận/hủy/xóa thành công/lỗi pass; prompt đang dùng bị chặn với lý do rõ ràng; sau xóa không còn trong list/detail; AI vẫn có prompt hợp lệ.

## 4. Cụm Content AI

```text
STORY-017 tạo content -> STORY-018 xem thư viện
                            |-> STORY-020 sửa
                            |-> STORY-019 xóa
                            `-> STORY-024 viết lại theo nền tảng -> lưu content mới
```

### STORY-017 - Tự động viết content và hashtag

**Main flow:** Admin nhập yêu cầu -> BE validate -> gọi AI -> trả bản nháp -> Admin sửa/tạo lại -> xác nhận sử dụng -> BE lưu content và hashtag.

**Góc nhìn BE:** API tạo bản nháp nhận `topic`, `goal`, `targetAudience`, `platform`, `tone`, `note`; nếu goal/tone là Khác thì nhận mô tả tương ứng. API lưu nhận content và danh sách hashtag đã được Admin duyệt. Gọi AI và lưu database là hai bước riêng; không tự lưu hoặc đăng trước khi Admin xác nhận.

**Rule/validate:** Chủ đề 5-1.000 ký tự (`BR-015`); đối tượng 5-300 (`BR-016`); đúng một nền tảng hợp lệ/đang kết nối (`BR-056`); goal/tone thuộc enum, mô tả Khác tối đa 200 (`BR-055`). Có 1-30 hashtag, bắt đầu `#`, phần nội dung dài 2-50 ký tự, không có khoảng trắng, chỉ chữ/số/`_`, loại trùng không phân biệt hoa thường (`BR-017` đến `BR-022`). Kết quả AI phải đủ content + hashtag (`BR-057`); lưu toàn vẹn (`BR-058`). Timeout AI là 60 giây.

**Done khi:** AC input hợp lệ/không hợp lệ, AI success/timeout/sai cấu trúc, tạo lại, chỉnh sửa, rời màn hình, session hết hạn và save rollback đều pass; retry lưu không tạo content trùng.

### STORY-018 - Xem content đã lưu

**Main flow:** Admin mở thư viện -> BE trả content mới cập nhật trước -> Admin tìm/lọc -> chọn một content -> BE trả detail version mới nhất.

**Góc nhìn BE:** Query list hỗ trợ `keyword`, hashtag, platform và khoảng `updatedAt`; trả `id`, title, content rút gọn, hashtags, platform, `updatedAt`. Query detail theo `contentId` trả title, nội dung đầy đủ, hashtags, platform và audit; detail read-only.

**Rule/validate:** Sort `UpdatedAt` giảm dần (`BR-023`); detail chỉ đọc (`BR-024`); keyword tìm trong title/content (`BR-030`); filter hashtag (`BR-031`); kết hợp hashtag/platform/thời gian (`BR-059`). Giữ filter là trách nhiệm state của FE nhưng BE phải nhận lại cùng query ổn định.

**Done khi:** AC list/rỗng/sort/search/filter/detail/not-found/lỗi tải pass; tổng số và dữ liệu trả về đúng điều kiện.

### STORY-020 - Sửa content đã lưu

**Main flow:** Admin lấy detail -> sửa content/hashtag -> gửi cùng version -> BE validate -> cập nhật toàn vẹn -> trả dữ liệu mới.

**Góc nhìn BE:** Command nhận `contentId`, nội dung, hashtags và concurrency token. Chỉ cập nhật bản hiện tại, `updatedBy`, `updatedAt`; không tạo lịch sử. Content và hashtag phải nằm trong cùng transaction.

**Rule/validate:** Tổng text + hashtag từ 1-10.000 ký tự (`BR-025`); transaction toàn vẹn (`BR-026`); không lưu history (`BR-061`); chống ghi đè dữ liệu mới hơn (`BR-062`). `BR-027` hiện không đọc được nên chưa thể viết test.

**Done khi:** AC update/hủy/unchanged/validation/rollback/conflict/not-found/session pass; không có trường hợp content mới nhưng hashtag cũ hoặc ngược lại.

### STORY-019 - Xóa content đã lưu

**Main flow:** Admin yêu cầu xóa -> BE kiểm tra quyền và tham chiếu -> Admin xác nhận -> BE kiểm tra lại -> xóa content + hashtag -> trả thành công.

**Góc nhìn BE:** Command theo `contentId`; query dependency phải kiểm tra lịch đăng, bài đăng và nghiệp vụ liên quan. Xóa content và liên kết hashtag trong một transaction.

**Rule/validate:** Chặn nếu đang được tham chiếu (`BR-028`); xóa toàn vẹn (`BR-029`); kiểm tra lại tại thời điểm xác nhận (`BR-060`).

**Done khi:** AC hủy/success/not-found/in-use/rollback/race condition/session pass; bản ghi bị xóa không còn xuất hiện ở list/detail.

### STORY-024 - Viết lại content theo từng nền tảng

**Main flow:** Admin chọn content gốc và nhiều nền tảng -> BE gọi AI riêng từng nền tảng -> trả kết quả/trạng thái riêng -> Admin sửa, tạo lại hoặc lưu từng kết quả -> BE tạo content mới.

**Góc nhìn BE:** Request nhận `sourceContentId` và danh sách `platformIds`. Mỗi nền tảng là một đơn vị xử lý độc lập với status/result/error riêng. Command lưu một kết quả nhận nội dung, hashtag, platform và version của content gốc; tạo ID mới, copy title và lưu liên kết về nguồn.

**Rule/validate:** Xử lý độc lập (`BR-068`); mỗi kết quả phải đúng rule nền tảng (`BR-069`); lưu thành content mới, không sửa nguồn (`BR-070`); mỗi lần lưu toàn vẹn (`BR-071`). Phải phát hiện content nguồn bị xóa/thay đổi.

**Done khi:** AC một/nhiều nền tảng, chưa chọn nền tảng, partial success, retry riêng, result invalid, save riêng, nguồn đổi/xóa, rollback và session pass; lỗi một nền tảng không làm mất kết quả khác.

## 5. Cụm Ảnh AI

```text
STORY-003 tạo ảnh -> STORY-021 xem thư viện
                        |-> STORY-023 sửa tên/mô tả
                        `-> STORY-022 xóa ảnh và các phiên bản
```

### STORY-003 - Tự động sinh ảnh theo tỷ lệ từ ảnh core

**Main flow:** Admin upload một ảnh -> chọn một tỷ lệ -> BE validate và gọi AI -> trả một ảnh kết quả -> Admin tạo lại hoặc lưu tên/mô tả -> BE đưa ảnh vào thư viện.

**Góc nhìn BE:** API generate nhận một file và một `ratio`, chống double submit/idempotency. Kết quả tạm trả URL/ID, ratio và kích thước. API lưu nhận result ID, `name`, `description`; lưu file, metadata, người tạo và thời gian trong một transaction logic.

**Rule/validate:** JPG/JPEG/PNG (`BR-032`); tối đa 10 MB (`BR-033`); ratio chỉ `1:1`, `4:5`, `9:16`, `16:9`, `2:1` (`BR-034`); giữ safe zone (`BR-035`); tên 1-200 (`BR-036`); mô tả tối đa 1.000 (`BR-037`); một request = một ảnh + một ratio + một kết quả (`BR-050`); không ghi đè ảnh nguồn và không lưu dở dang (`BR-051`).

**Done khi:** AC file/ratio invalid, generate success/error, double submit, regenerate, save success/error và rollback pass. Tên “đa tỷ lệ” không có nghĩa là chọn nhiều tỷ lệ trong một request.

### STORY-021 - Xem ảnh đã lưu

**Main flow:** Admin mở thư viện -> BE trả ảnh mới cập nhật trước -> Admin tìm/lọc -> chọn ảnh -> BE trả ảnh lớn và metadata read-only.

**Góc nhìn BE:** Query list nhận keyword, format, thời gian, người thực hiện; trả `id`, thumbnail URL, name, ratio, dimensions, format, `updatedAt`. Detail trả thêm description, size, source, audit và các phiên bản tỷ lệ.

**Rule/validate:** Sort giảm dần (`BR-038`); search/filter kết hợp (`BR-063`); detail chỉ đọc và không quản lý tag (`BR-064`). Thumbnail lỗi không được làm hỏng cả danh sách.

**Done khi:** AC list/rỗng/search/filter/reset/detail/not-found/thumbnail lỗi/session pass; response không chứa dữ liệu ngoài phạm vi được phép.

### STORY-023 - Sửa ảnh đã lưu

**Main flow:** Admin lấy detail -> sửa tên/mô tả -> gửi cùng version -> BE validate và cập nhật metadata -> giữ nguyên file ảnh.

**Góc nhìn BE:** Command nhận `imageId`, `name`, `description`, concurrency token. Chỉ cập nhật metadata, `updatedBy`, `updatedAt`; không nhận hoặc thay file/ratio/system fields.

**Rule/validate:** Tên bắt buộc 1-200, mô tả tối đa 1.000 (`BR-066`); cập nhật toàn vẹn và optimistic locking (`BR-067`).

**Done khi:** AC validation/hủy/unchanged/update/conflict/not-found/rollback/session pass; checksum hoặc reference file ảnh không đổi.

### STORY-022 - Xóa ảnh đã lưu

**Main flow:** Admin yêu cầu xóa -> BE kiểm tra tham chiếu -> xác nhận -> kiểm tra lại -> xóa ảnh gốc, phiên bản tỷ lệ và metadata.

**Góc nhìn BE:** Command theo `imageId`; kiểm tra tham chiếu từ content/lịch đăng. Database metadata và object storage cần cơ chế bảo đảm không để trạng thái xóa một phần.

**Rule/validate:** Chặn ảnh đang được tham chiếu (`BR-039`); xóa toàn bộ hoặc giữ nguyên toàn bộ (`BR-065`).

**Done khi:** AC hủy/success/not-found/in-use/storage error/database error/race condition/session pass; sau success không còn file mồ côi hoặc metadata mồ côi.

## 6. Cụm Lên lịch và tự động đăng

```text
STORY-002 tạo lịch -> STORY-014 xem danh sách
                         |-> STORY-016 sửa lịch và job
                         |-> STORY-015 hủy job rồi xóa lịch
                         `-> Đến giờ: đăng và ghi kết quả từng nền tảng
```

### STORY-002 - Lên lịch và tự động đăng bài

**Main flow:** Admin chọn một content, media, nền tảng và thời gian -> BE validate -> tạo lịch “Đã lên lịch” và job -> đến giờ đăng riêng từng nền tảng -> cập nhật kết quả.

**Góc nhìn BE:** Command tạo lịch nhận `contentId`, danh sách media/image, `platformIds`, `scheduledAt`. Phải lưu schedule, platform tasks và job nhất quán. Worker đăng từng nền tảng, lưu `status`, external post ID và error reason riêng; trạng thái tổng chỉ thành công khi tất cả thành công.

**Rule/validate:** Ảnh JPG/JPEG/PNG, tối đa 10 MB/file và 10 ảnh (`BR-001` đến `BR-003`); thời gian tương lai (`BR-004`); ít nhất một nền tảng (`BR-005`); không trùng Content + Platform + Time (`BR-006`); đúng một content tồn tại (`BR-046`); platform hỗ trợ, kết nối hợp lệ, không trùng (`BR-047`); trạng thái riêng từng nền tảng (`BR-048`); retry theo loại lỗi, tối đa 3 lần (`BR-049`). Token hết hạn yêu cầu kết nối lại; nội dung vi phạm không tự retry.

**Done khi:** AC validation/duplicate/create job/all success/partial success/token/rate-limit/policy rejection/retry exhausted pass; retry không đăng trùng; dữ liệu thay đổi trước lúc lưu được phát hiện.

**Cần chốt:** Media được chọn từ `STORY-021`, gắn sẵn với content hay upload trực tiếp khi lên lịch.

### STORY-014 - Xem lịch đăng bài

**Main flow:** Admin mở danh sách -> BE trả lịch theo thời gian đăng tăng dần -> trả tổng số và trạng thái từng lịch.

**Góc nhìn BE:** Query list trả tối thiểu schedule ID, content, platforms, `scheduledAt`, status và kết quả nền tảng nếu cần. Khi tải lỗi không trả danh sách một phần; dữ liệu lần thành công trước có thể được FE giữ lại.

**Rule/validate:** Sort tăng dần (`BR-007`); lịch cùng thời điểm không bị gộp (`BR-012`); total khớp (`BR-013`); chỉ cập nhật khi tải đủ (`BR-014`).

**Done khi:** AC list/rỗng/sort/same timestamp/total/load error/session pass. Cần chốt tie-breaker khi cùng `scheduledAt`, ví dụ `CreatedAt` rồi `Id`.

### STORY-016 - Sửa lịch đăng bài

**Main flow:** Admin lấy lịch “Đã lên lịch” -> sửa content/media/platform/time -> gửi cùng version -> BE validate -> cập nhật schedule và job.

**Góc nhìn BE:** Command nhận schedule ID, các field thay đổi và concurrency token. Chỉ cho sửa trước khi job chạy. Cập nhật database và reschedule job phải nhất quán; lỗi một phía phải rollback/khôi phục.

**Rule/validate:** Chỉ trạng thái “Đã lên lịch” (`BR-008`); cho sửa content/ảnh/thời gian/nền tảng và vẫn chống trùng (`BR-009`); áp dụng lại rule tạo lịch (`BR-053`); optimistic locking (`BR-054`).

**Done khi:** AC từng field/validation/unchanged/hủy/conflict/status changed/job error/rollback/session pass; job cũ không còn chạy sau update thành công.

### STORY-015 - Xóa lịch đăng bài

**Main flow:** Admin xác nhận xóa lịch “Đã lên lịch” -> BE hủy Cron Job -> hủy thành công mới xóa schedule -> trả kết quả.

**Góc nhìn BE:** Command theo schedule ID; khóa/kiểm tra lại trạng thái trước thao tác. Thứ tự bắt buộc là cancel job trước, xóa dữ liệu sau; cần idempotency để retry an toàn.

**Rule/validate:** Chỉ xóa trạng thái “Đã lên lịch” (`BR-010`); không xóa schedule nếu hủy job thất bại (`BR-011`).

**Done khi:** AC hủy/success/invalid status/job already running/cancel failure/delete failure/retry pass; không còn job chạy sau khi API báo thành công.

## 7. Cụm Báo cáo nền tảng

```text
Lịch hệ thống -> STORY-025 tạo collection run
                    |-> kiểm tra từng nền tảng
                    |-> lấy đủ dữ liệu và lưu report riêng
                    `-> Admin xem danh sách -> chi tiết -> tải XLSX
```

### STORY-025 - Thu thập và quản lý báo cáo

**Main flow:** Đến lịch -> BE tạo lần chạy “Đang xử lý” -> lấy các nền tảng hợp lệ -> gọi API và đọc đủ dữ liệu -> tạo report riêng -> Admin lấy list/detail và tải `.xlsx`.

**Góc nhìn BE:** Scheduler tạo collection run có idempotency key theo platform + thời điểm. Worker lưu trạng thái/error riêng từng nền tảng và đọc hết pagination trước khi tạo report. Cần query report list/detail và command/query tạo file download từ đúng report đã lưu.

**Dữ liệu chính:** Lịch gồm loại ngày/tuần/tháng, giờ, thứ hoặc ngày trong tháng. Collection run gồm platform, thời điểm bắt đầu/kết thúc, status, record count, error. Report gồm ID, platform, collectedAt, createdAt, data, status.

**Rule/validate:** Lịch ngày cần giờ, tuần cần thứ + giờ, tháng cần ngày + giờ (`BR-072`); timezone `Asia/Ho_Chi_Minh`, ngày không tồn tại chạy ngày cuối tháng (`BR-073`); không tạo run trùng (`BR-074`); platform phải bật, kết nối hợp lệ và có quyền bên ngoài (`BR-075`); xử lý độc lập (`BR-076`); chỉ tạo report từ dữ liệu đủ, zero data thì không tạo report (`BR-077`); chỉ đánh dấu thành công sau khi lưu đủ (`BR-078`); file chỉ `.xlsx`, tên `[Bao_Cao]_[Ten_Nen_Tang]_[YYYYMMDD_HHmmss].xlsx` (`BR-079`).

**Done khi:** AC lịch ngày/tuần/tháng/ngày cuối tháng/timezone/idempotency/platform invalid/pagination/partial failure/no data/incomplete data/retry/save rollback/list empty/detail/download/file error đều pass.

**Cần chốt:** Out of Scope nói không tạo/sửa lịch nhưng AC và BR lại mô tả cấu hình lịch; cần xác nhận owner của API cấu hình. Số lần retry khi lấy report cũng chưa được quy định.

## 8. Checklist để một Story được xem là done ở BE

1. Có query/command đúng main flow và contract request/response rõ ràng.
2. Validate đầy đủ BR ở application/domain layer, không chỉ dựa vào FE.
3. Có authorization, xử lý session và giới hạn phạm vi dữ liệu Admin được xem.
4. Có transaction cho thao tác nhiều bảng/file/job; lỗi không để dữ liệu dở dang.
5. Có concurrency check cho update/delete và idempotency cho create/retry/job.
6. Có mapping lỗi rõ cho validation, not-found, conflict, dependency, AI/platform timeout.
7. Có audit fields và trạng thái/lý do lỗi theo yêu cầu story.
8. Unit test cho BR/domain logic và integration test cho database, job, AI/platform adapter.
9. Các AC happy path, alternative flow và exception flow đều có test tương ứng.

## 9. Các điểm phải làm rõ trước khi khóa TDD

1. `STORY-005`: thiếu nội dung `BR-043`, `BR-044`.
2. `STORY-020`: `BR-027` không đọc được.
3. `STORY-003`: tên “đa tỷ lệ” nhưng mỗi request chỉ cho một tỷ lệ.
4. `STORY-017`: chưa thống nhất nhãn “Sử dụng nội dung” và “Lưu nội dung”.
5. `STORY-002`: chưa rõ nguồn media và quan hệ Content-Image.
6. `STORY-014`: chưa có tie-breaker cho lịch cùng thời điểm.
7. `STORY-025`: phạm vi cấu hình lịch đang mâu thuẫn và chưa chốt retry.
8. `STORY-006`: thiếu AC cụ thể khi prompt đang được AI sử dụng.
9. `STORY-026` được nhắc tới nhưng không có trong folder hiện tại.
