# GetImageVersions

`GET /api/v1/posts/{id}/images`

Đã triển khai trên nhánh `feature/post-gen` ở mốc 6 (2026-09-14), chưa deploy. JWT Staff/Admin. Không gọi AI.

Đọc kho ảnh dùng chung theo groups nguồn × tỷ lệ, versions tăng dần theo CreatedAt/Id, gồm versionNo/parent/status/feedback/selectedPlatforms. Giữ lịch sử nguồn đã xóa và đánh dấu sourceIsDeleted. Bài đã xóa trả 404. Chưa phân trang.

Request/response JSON, validation và lỗi: [bàn giao FE mốc 6](../FE-API-Handoff-PostGen-Milestone6.md). Contract chung: [ApiContract.md](ApiContract.md).

Kiểm thử: `HoaTheoMua.Service.Test/PostGenTest/GetImageVersions.cs`, transaction/concurrency tại `ImageVersionsPostgres.cs`, HTTP tại `Verification/Program.cs`. Toàn bộ provider dùng giả lập.
