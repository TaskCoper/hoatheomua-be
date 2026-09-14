# PostGen — Hiện thực mốc 0–2

Ngày thực hiện ban đầu: 2026-09-12; cập nhật chuyển chuẩn nền tảng sang Config: 2026-09-14. Các thay đổi đang ở working tree, chưa commit hoặc deploy.

## Phạm vi hoàn thành

| Mốc | Kết quả |
| --- | --- |
| 0 — Schema | 4 bảng mới, mở rộng BasePost/GeneratedPost, index/CHECK/FK; chuẩn nền tảng chuyển sang Config và bỏ bảng cũ |
| 1 — Post Core | 5 endpoint tạo, danh sách, chi tiết, sửa, soft delete; bài dùng chung cho mọi Staff/Admin |
| 2 — Cấu hình | Chuẩn nền tảng và preset quản lý bằng API Config chung với quyền Admin |

Feature folder: `HoaTheoMua.Service/PostCore/` và `HoaTheoMua.Service/PostGen/` (chỉ còn ánh xạ tỷ lệ ảnh); PostGenController/service/DI chuẩn nền tảng đã bỏ. Quy tắc `HttpUrl()` dùng chung tại PostCore, CardTemplate, Mockup.

Migration `20260912053534_AddPostGenCore` mở rộng các bảng theo [DataModel.md](DataModel.md), không thay native enum hoặc schema SystemPrompt/PostHistory/ClientHistory. Mapping EF dùng quote cho bốn tên enum PostgreSQL có chữ hoa để tránh SQL tham chiếu nhầm type chữ thường; không có `ALTER TYPE` trong SQL của migration này.

Các điều chỉnh triển khai đáng chú ý:

- Tỷ lệ mỗi nền tảng chỉ lưu tại Config → postgen.ratio-preset; Chuẩn nền tảng nằm tại `postgen.platform.{platform}`, tách khỏi ratio preset. Preset không phụ thuộc danh sách cho phép theo nền tảng.

- BasePost không có UserId; danh sách/chi tiết/sửa/xóa áp dụng cho tập bài dùng chung. Index dùng CreatedAt DESC. Quyền truy cập vẫn yêu cầu Staff/Admin.

- Preset có partial unique index theo `(Group, Key)`. Từ 2026-09-14, bỏ hai API preset cùng service/DTO/validator chuyên biệt; quản lý bằng Config.Service và API Config chung. Hướng dẫn tại [RatioPresetConfig.md](RatioPresetConfig.md).
- Post Core PUT thay toàn bộ tập ảnh. Giữ ID/URL ảnh cũ; ảnh mới tạo ID mới, ảnh bỏ đi soft delete. Đảo thứ tự thực hiện trong transaction với khóa dòng bài gốc. Chi tiết contract tại [UpdatePostCore.md](UpdatePostCore.md).
- GetPostCoreDetail chỉ SELECT cột cần dùng, đọc không tracking, ghép lịch sử bằng lookup; bỏ query version/slot khi chưa có platform. Chi tiết và kiểm chứng SQL tại [GetPostCoreDetail.md](GetPostCoreDetail.md).

## Kiểm chứng hiện tại — chuyển chuẩn nền tảng sang Config (2026-09-14)

- Migration `20260914055741_MovePlatformStandardsToConfig`: chuyển toàn bộ dữ liệu, chặn xung đột, drop bảng sau khi copy; Down khôi phục dữ liệu và từ chối JSON/lịch sử không thể biểu diễn bằng bảng cũ. Hướng dẫn sử dụng tại [PlatformConfig.md](PlatformConfig.md).
- Build API và HTTP probe: PASS; không có lỗi. Có warning nullable hiện hữu và warning truy cập nguồn NuGet audit trong môi trường kiểm thử.
- Test Config/PostGen trên PostgreSQL 16 riêng: **74 PASS, 0 FAIL, 0 SKIP**, gồm cả hai test tích hợp migration/PostGen.
- HTTP probe: **31 request, 53 kiểm tra PASS**. Đã kiểm Admin CRUD từng bản ghi Config, đọc từ Admin khác, chặn Staff/ẩn danh, giữ Config private, giữ nền tảng khác và ratio preset, bốn route cũ trả 404.
- SQL cuối: bảng PlatformStandard không tồn tại, 3 Config chuẩn còn hiệu lực và 1 dòng lịch sử xóa mềm do probe tạo. Không còn database migration test; container probe cùng volume đã xóa.
- Báo cáo có traceId: [database-flow-validator.json](Verification/results/database-flow-validator.json). Chỉ cache revalidation bên ngoài được stub. Chưa triển khai hoặc chạy migration trên môi trường deployed.

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

Probe dùng `POSTGEN_TEST_CONNECTION_STRING`, tự tạo JWT kiểm thử, tạo/sửa/xóa mềm dữ liệu test và kiểm CRUD Config chuẩn nền tảng trong DB test; chỉ lời gọi revalidation ra ngoài được thay bằng stub. Evidence JSON ghi vào thư mục temp hệ thống với tên `postgen-api-evidence.json`.

## Phần tiếp theo

Mốc 3–7 chưa hiện thực. Theo Plan.md, cần xác nhận contract AI Service (endpoint, auth, request/response) trước khi viết `AiService/`. Các API sinh nội dung/ảnh, lịch sử tương tác, chọn official và publishing chưa có endpoint thực thi. Các trường publishing hiện chỉ dành sẵn trong schema.

Seed giới hạn nền tảng lấy từ kế hoạch; chưa được xác minh để dùng cho phase đăng bài. Migration dừng nếu BasePost/GeneratedPost cũ có dữ liệu ngoài giả định để không tự xóa nguồn ảnh cũ; cần backfill riêng trong trường hợp đó.
