# PostGen — Hiện thực mốc 0–3

Ngày thực hiện ban đầu: 2026-09-12; cập nhật 2026-09-14. Mốc 0–2 đã commit/push trên feature/post-gen. Config prompt/schema, AI client, generate của mốc 3 và contract response chỉ trả nội dung/ảnh chốt được đưa vào nhánh feature/post-gen; chưa deploy.

## Quyết định mới: mỗi nền tảng một Config prompt

Đã bỏ namespace postgen.platform.*. Mỗi nền tảng chỉ có Config gồm content và isActive; không còn chuẩn riêng. Facebook, Instagram và Zalo dùng ba system prompt hoàn chỉnh tại `postgen.system-prompt.{platform}`, không ghép prompt Post chung. Migration seed và SystemPromptConfigId đã triển khai. AI client và generate nội dung đã triển khai theo chat completions proxy người dùng cung cấp. Luồng HTTP/DB kiểm bằng fake provider response; một lời gọi proxy thật riêng xác nhận contract text. Xem [ParaphrasePromptData.md](ParaphrasePromptData.md).

## Phạm vi hoàn thành

| Mốc | Kết quả |
| --- | --- |
| 0 — Schema | 4 bảng mới, mở rộng BasePost/GeneratedPost, index/CHECK/FK; chuẩn nền tảng chuyển sang Config và bỏ bảng cũ |
| 1 — Post Core | 5 endpoint tạo, danh sách, chi tiết, sửa, soft delete; bài dùng chung cho mọi Staff/Admin |
| 2 — Cấu hình | Prompt/bật tắt nền tảng và preset quản lý bằng API Config chung với quyền Admin |
| 3 — Sinh nội dung | Config prompt, AiService proxy, POST generate, snapshot/version/official và rollback khi lỗi |

Feature folder: `HoaTheoMua.Service/PostCore/` và `HoaTheoMua.Service/PostGen/` (chỉ còn ánh xạ tỷ lệ ảnh); PostGenController/service/DI chuẩn nền tảng đã bỏ. Quy tắc `HttpUrl()` dùng chung tại PostCore, CardTemplate, Mockup.

Migration `20260912053534_AddPostGenCore` mở rộng các bảng theo [DataModel.md](DataModel.md), không thay native enum hoặc schema SystemPrompt/PostHistory/ClientHistory. Mapping EF dùng quote cho bốn tên enum PostgreSQL có chữ hoa để tránh SQL tham chiếu nhầm type chữ thường; không có `ALTER TYPE` trong SQL của migration này.

Các điều chỉnh triển khai đáng chú ý:

- Tỷ lệ mỗi nền tảng chỉ lưu tại Config → postgen.ratio-preset; Mỗi nền tảng dùng `postgen.system-prompt.{platform}` chứa content và isActive, tách khỏi ratio preset. Preset không phụ thuộc danh sách cho phép theo nền tảng.

- BasePost không có UserId; danh sách/chi tiết/sửa/xóa áp dụng cho tập bài dùng chung. Index dùng CreatedAt DESC. Quyền truy cập vẫn yêu cầu Staff/Admin.

- Preset có partial unique index theo `(Group, Key)`. Từ 2026-09-14, bỏ hai API preset cùng service/DTO/validator chuyên biệt; quản lý bằng Config.Service và API Config chung. Hướng dẫn tại [RatioPresetConfig.md](RatioPresetConfig.md).
- Post Core PUT thay toàn bộ tập ảnh. Giữ ID/URL ảnh cũ; ảnh mới tạo ID mới, ảnh bỏ đi soft delete. Đảo thứ tự thực hiện trong transaction với khóa dòng bài gốc. Chi tiết contract tại [UpdatePostCore.md](UpdatePostCore.md).
- GetPostCoreDetail chỉ SELECT cột cần dùng, đọc không tracking, ghép lịch sử bằng lookup; bỏ query version/slot khi chưa có platform. Chi tiết và kiểm chứng SQL tại [GetPostCoreDetail.md](GetPostCoreDetail.md).

## Kiểm chứng hiện tại — response chỉ gồm nội dung và ảnh chốt (2026-09-14)

- Bỏ `value.images` và `platforms[].versions` trong DTO dùng chung cho tạo/sửa/chi tiết/generate. `baseImages` giữ ảnh nguồn; `platforms[].content` là object chốt hoặc null. `platforms[].images` chỉ gồm ảnh thành công đang được chọn, kèm URL/kích thước theo thứ tự. Lịch sử vẫn giữ trong database; không cần migration.
- Release build solution và HTTP probe PASS. **53 test PostGen PASS, 0 FAIL, 0 SKIP**, gồm 5 PostgreSQL integration test. **48 HTTP request, 93 kiểm tra PASS** trên database mới, fake AI và PostgreSQL thật.
- Kiểm bản cũ được chốt vẫn đúng versionNo, chưa chốt trả null, không lộ lịch sử trong JSON; không trả ô ảnh chưa chọn hoặc ảnh lỗi, ảnh chốt có URL. SQL dùng 1/3/6 query cho bài không tồn tại/chưa có platform/có platform.
- Database integration và container/volume tạm đã dọn. Tài liệu API, Plan và hai bản bàn giao FE đã đồng bộ. Báo cáo: [database-flow-validator.json](Verification/results/database-flow-validator.json).

## Kiểm chứng bước trước — bỏ Config standard riêng (2026-09-14)

- Migration `20260914073414_ConsolidatePostGenPlatformConfigs`: giữ nội dung/ID/FK prompt và snapshot lịch sử, chuyển isActive sang prompt, xóa postgen.platform.* trong group postgen và index cũ. Prompt đã có isActive thì giữ nguyên; nguồn/đích sai kiểu dừng nguyên transaction. Down bị chặn vì không thể tái tạo chính xác dữ liệu đã bỏ; cần backup trước migration.
- PostGeneration chỉ đọc prompt có content và isActive. AiService không còn nhận/gửi MaxLength từ platform standard. Ba payload Data đã cập nhật để Admin quản trị qua Config API.
- Release build solution và probe PASS. **108 test Config/PostGen PASS, 0 FAIL, 0 SKIP**, gồm 5 PostgreSQL integration test. **48 HTTP request, 89 kiểm tra PASS**.
- HTTP kiểm generate không có standard, tắt/bật bằng Config prompt, key cũ không còn, giữ ratio preset, quyền Admin, snapshot/official, lỗi AI và heavy rate limit. Không gọi lại AI thật trong thay đổi này vì contract proxy không đổi.
- Các DB integration test và container/volume PostgreSQL tạm đã được dọn. Chưa commit/deploy hoặc chạy migration lên database triển khai.
- Báo cáo: [consolidated-config-flow-20260914.json](Verification/results/consolidated-config-flow-20260914.json).

## Kiểm chứng bước trước — AI client và sinh nội dung (2026-09-14)

- Endpoint mới: POST `/api/v1/posts/{id}/generate`, Staff/Admin, heavy rate limit. Request `{platforms:[{platform:"facebook"}]}`. Sinh text qua chat completions, parse JSON title/content/hashtag, lưu Config reference/snapshot và provider metadata. Chi tiết có payload thật tại [GeneratePlatformPosts.md](GeneratePlatformPosts.md).
- Release build solution và probe PASS. **105 test Config/PostGen PASS, 0 FAIL, 0 SKIP**, gồm 4 PostgreSQL integration test. **55 request HTTP, 102 kiểm tra PASS**.
- Đã kiểm guard cùng nền tảng, chặn sửa/xóa trong lúc sinh, hủy request và thu hồi trạng thái, lỗi nền tảng thứ hai không tạo version mới, rollback khi ghi official thất bại, missing prompt không fallback, 429 kèm Retry-After.
- HTTP probe dùng fake AI HttpMessageHandler và stub cache revalidation; controller/service/JWT/validation/rate limit/PostgreSQL là thật. Một lời gọi proxy thật riêng trả 200 và JSON hợp lệ: [proxy-content-smoke.json](Verification/results/proxy-content-smoke.json).
- Các database integration test đã được dọn; container/volume PostgreSQL tạm đã xóa. Chưa deploy/migrate môi trường triển khai. Chưa triển khai hoặc kiểm thử sinh ảnh.
- Báo cáo bước trước: [content-generation-flow-20260914.json](Verification/results/content-generation-flow-20260914.json).

## Kiểm chứng bước trước — Config prompt và tham chiếu lịch sử (2026-09-14)

- Migration `20260914065606_AddPostGenSystemPromptConfigs`: thêm ba prompt hoàn chỉnh từ Data, giữ nguyên bản active có sẵn và lịch sử xóa mềm; key active trùng chặn Up nguyên transaction. Snapshot/Designer khớp model, không thay native enum.
- FK `GeneratedPost.SystemPromptConfigId` nullable → Config, `Restrict`. Giữ FK SystemPromptId cũ. Sửa/xóa mềm Config không thay PromptSnapshot của version; hard delete Config đang được tham chiếu bị chặn.
- Down giữ toàn bộ Config, chỉ bỏ schema mới khi không còn version tham chiếu. Có tham chiếu thì dừng nguyên transaction để giữ provenance. Up lại không ghi đè hoặc nhân đôi key active.
- Release build solution và HTTP probe PASS. Có warning nullable/analyzer hiện hữu và NU1900 từ nguồn NuGet audit trong môi trường build; không có lỗi build.
- **75 test Config/PostGen PASS, 0 FAIL, 0 SKIP**, gồm ba test tích hợp PostgreSQL. **42 HTTP request, 74 kiểm tra PASS**: Admin đọc/sửa/xóa/tạo lại prompt, Admin khác đọc được, Staff bị chặn, prompt private, sửa Facebook giữ Instagram/Zalo.
- Đã xác nhận các database test được dọn; container PostgreSQL tạm cùng volume đã xóa. Chưa áp dụng migration lên môi trường triển khai.
- Báo cáo: [prompt-config-flow-20260914.json](Verification/results/prompt-config-flow-20260914.json). Chỉ cache revalidation bên ngoài được stub; chưa gọi AI.

## Kiểm chứng bước trước — chuyển chuẩn nền tảng sang Config (2026-09-14)

- Migration `20260914055741_MovePlatformStandardsToConfig`: chuyển toàn bộ dữ liệu, chặn xung đột, drop bảng sau khi copy; Down khôi phục dữ liệu và từ chối JSON/lịch sử không thể biểu diễn bằng bảng cũ. Hướng dẫn sử dụng tại [PlatformConfig.md](PlatformConfig.md).
- Build API và HTTP probe: PASS; không có lỗi. Có warning nullable hiện hữu và warning truy cập nguồn NuGet audit trong môi trường kiểm thử.
- Test Config/PostGen trên PostgreSQL 16 riêng: **74 PASS, 0 FAIL, 0 SKIP**, gồm cả hai test tích hợp migration/PostGen.
- HTTP probe: **31 request, 53 kiểm tra PASS**. Đã kiểm Admin CRUD từng bản ghi Config, đọc từ Admin khác, chặn Staff/ẩn danh, giữ Config private, giữ nền tảng khác và ratio preset, bốn route cũ trả 404.
- SQL cuối: bảng PlatformStandard không tồn tại, 3 Config chuẩn còn hiệu lực và 1 dòng lịch sử xóa mềm do probe tạo. Không còn database migration test; container probe cùng volume đã xóa.
- Báo cáo có traceId: [platform-config-flow-20260914.json](Verification/results/platform-config-flow-20260914.json). Chỉ cache revalidation bên ngoài được stub. Chưa triển khai hoặc chạy migration trên môi trường deployed.

## Kiểm chứng bước trước: bỏ API preset — 2026-09-14

Số liệu lịch sử trước khi chuyển chuẩn nền tảng sang Config.

- Build `Verification/PostGenProbe.csproj` (bao gồm API và dependency): PASS, 0 warning, 0 error.
- Chạy test `PostGenTest` và `ConfigTest`: **75 PASS, 0 FAIL, 1 SKIP**. Test bị skip là PostgreSQL integration vì chưa đặt `POSTGEN_TEST_CONNECTION_STRING`.
- Test mới `ConfigTest/PostGenRatioConfig.cs` xác nhận tạo, đọc giữa hai user, thay toàn bộ JSON và xóa mềm preset bằng Config.Service.
- Không còn route GET/PUT preset, method service, DTO hoặc validator preset trong mã API/service. HTTP probe đã cập nhật để kiểm hai route cũ trả 404; chưa chạy lại probe HTTP hoặc PostgreSQL trong đợt này.

## Kiểm chứng trước thay đổi ngày 2026-09-14

Các số liệu bên dưới là lịch sử trước khi bỏ API preset, không đại diện cho contract hiện tại. Test preset cũ đã được thay bằng `ConfigTest/PostGenRatioConfig.cs`; probe hiện kiểm hai route đã bỏ trả 404.

- Build Release toàn solution: PASS.
- Test PostGen sau tối ưu GetPostCoreDetail: **27/27 PASS**, gồm **26** test service/validator và **1** bài kiểm thử tích hợp PostgreSQL; kiểm tra số query, projection và không tracking trên DB thật.
- Toàn bộ test suite Release cùng PostgreSQL integration trước tối ưu query: **859/859 PASS**, không có test skip. Sau tối ưu đã chạy lại phạm vi PostGen nêu trên.
- Migration đã chạy thành công từ PostgreSQL 16 rỗng trong container riêng; đủ 1 prompt Post, 3 PlatformStandard và 2 Config toàn hệ thống.
- PostgreSQL integration kiểm tra: migration lên/xuống/lên; chặn schema cũ có dữ liệu chưa backfill; bảo toàn prompt Post đã tồn tại; không lệch model/snapshot; FK vòng official; CHECK ngăn ảnh Succeeded thiếu URL; đảo thứ tự ảnh; soft delete vẫn giữ version; 8 user lưu preset đồng thời chỉ một dòng dùng chung.
- HTTP probe dùng controller, FluentValidation filter, JWT policies và middleware thực tế, cùng PostgreSQL riêng. Cả **9 API** trả thành công đúng contract. Kiểm lỗi: không đăng nhập 401, Customer 403, thiếu ảnh 422, Staff/Admin khác đọc/sửa/xóa cùng bài thành công, sai shape preset 422, mã tỷ lệ không hợp lệ hoặc trùng trong preset 422, Staff sửa chuẩn 403 và Admin sửa được 200.
- Hai JWT khác UserId đọc cùng một preset; thay đổi của user này được user khác nhìn thấy. Payload và traceId từ lần chạy thật nằm trong 9 tài liệu endpoint.

Probe khởi tạo một web host kiểm thử chỉ đăng ký dependency cần cho PostGen. Nó không kiểm hạ tầng deployment, RabbitMQ, Cloudflare hoặc AI Service. Chưa chạy gọi AI/provider.

## Chạy lại

Test service/validator:

```bash
dotnet test HoaTheoMua.Service.Test --filter FullyQualifiedName~PostGenTest
```

Test PostgreSQL tự tạo DB tên `postgen_test_<guid>` và xóa DB đó khi kết thúc. Cung cấp connection string tới server PostgreSQL **cục bộ dành riêng cho test**, user có quyền CREATE DATABASE. Không dùng database deployed.

```bash
export POSTGEN_TEST_CONNECTION_STRING='Host=127.0.0.1;Port=55439;Database=postgen_test;Username=postgen;Password=postgen-local-test'
dotnet test HoaTheoMua.Service.Test --filter FullyQualifiedName~PostGenPostgres
```

Nếu không đặt biến môi trường này, chỉ test PostgreSQL được skip; các test service vẫn chạy.

HTTP probe cần database cục bộ đã áp dụng migration và chỉ dùng cho test, cổng HTTP `55440` trống:

```bash
dotnet run --project 'hoatheomua-documentation/Service - PostGen/Verification/PostGenProbe.csproj'
```

Probe dùng `POSTGEN_TEST_CONNECTION_STRING`, tự tạo JWT kiểm thử, tạo/sửa/xóa mềm dữ liệu test và kiểm CRUD Config chuẩn nền tảng trong DB test; lời gọi revalidation được stub, AI HttpMessageHandler trả response giả lập đúng contract. Evidence JSON ghi vào thư mục temp hệ thống với tên `postgen-api-evidence.json`.

## Phần tiếp theo

Mốc 3 đã hoàn thành. Tiếp theo là mốc 4: sinh ảnh qua cùng chat completions endpoint, giữ thứ tự text/ảnh, đọc URL từ Markdown và xử lý ảnh dùng chung/partial failure. Mốc 5–7 bổ sung API version/chọn official. Publishing vẫn ngoài phạm vi; các trường tương ứng chỉ dành sẵn trong schema.

Seed giới hạn nền tảng lấy từ kế hoạch; chưa được xác minh để dùng cho phase đăng bài. Migration dừng nếu BasePost/GeneratedPost cũ có dữ liệu ngoài giả định để không tự xóa nguồn ảnh cũ; cần backfill riêng trong trường hợp đó.
