# Kiểm chứng mốc 12

Probe dùng JWT test, PostgreSQL và RabbitMQ local, cùng đăng ký MassTransit của ứng dụng. Adapter và AI được thay bằng fake; không kết nối hoặc đăng bài lên Meta.

Yêu cầu PostgreSQL ở localhost qua `POSTGEN_TEST_CONNECTION_STRING`, user được tạo/xóa database test. RabbitMQ riêng trên `127.0.0.1:55672`, guest/guest, không dùng chung queue với môi trường triển khai. HTTP probe dùng port `55443`.

```bash
dotnet build 'hoatheomua-documentation/Service - PostGen/PublicationVerification/PublicationProbe.csproj' -c Release
dotnet 'hoatheomua-documentation/Service - PostGen/PublicationVerification/bin/Release/net8.0/PublicationProbe.dll'
```

Probe tự tạo database `publication_probe_<uuid>` và xóa trong finally. Queue được tạo trên RabbitMQ test; xóa container RabbitMQ riêng sau khi kiểm chứng để dọn topology. Evidence JSON sinh ở thư mục output của probe; bản đã lưu cho lần bàn giao tại [results/publication-api-evidence.json](results/publication-api-evidence.json).

Unit/integration suite:

```bash
dotnet test HoaTheoMua.Service.Test -c Release --filter FullyQualifiedName~PostPublishingTest
```

Khi có `POSTGEN_TEST_CONNECTION_STRING`, ca PostgreSQL chạy migration Up/Down, khóa đồng thời, snapshot immutable, active-job unique index, bus outbox và rollback sau SQL. Không cấu hình biến này thì ca PostgreSQL được đánh dấu skip; không coi đó là nghiệm thu database.


## Adapter mốc 13

Sau build, chạy cùng `PublicationProbe.dll` với đối số `--adapters` và `POSTGEN_TEST_CONNECTION_STRING` trỏ PostgreSQL local. RabbitMQ local vẫn ở 55672, HTTP probe ở 55444. Dùng đăng ký adapter production, OAuthProvider Meta thật trong code và thay HttpMessageHandler bằng fake provider; không truy cập Meta/AI hoặc tải ảnh ngoài.

Bao phủ Facebook/Instagram single + carousel, timeout, media sai; HTTP 401/403/404/409, idempotency, đọc job, retry bị chặn, recheck không đăng lại và Close. Kiểm DB attempt/snapshot cùng hàng đợi RabbitMQ thật. Mỗi lần tạo/drop database riêng. Kết quả mốc 13: `bin/Release/net8.0/api-integration-tester.json`, bản lưu tại [results/api-integration-tester.json](results/api-integration-tester.json).

Bộ mốc 12 không truyền `--adapters`, giữ fake adapter và kiểm tám endpoint/guard như trước.


## Lịch đăng mốc 14

Chạy `PublicationProbe.dll --scheduling` với `POSTGEN_TEST_CONNECTION_STRING` local, PostgreSQL 55432 và RabbitMQ 55672. Probe dùng HTTP 55444, đăng ký adapter production, fake HTTP Meta/media và TimeProvider điều khiển được để đi qua lịch tương lai mà không chờ thời gian thật. Không truy cập Meta/AI. Database riêng được tạo/drop trong mỗi lượt.

Kết quả: `bin/Release/net8.0/schedule-api-evidence.json`; bản lưu [results/schedule-api-evidence.json](results/schedule-api-evidence.json). 37 HTTP request / 90 checks PASS. Schema kết quả theo api-integration-tester; tổng hợp build/unit/PostgreSQL ở [results/milestone14-verification.json](results/milestone14-verification.json).
