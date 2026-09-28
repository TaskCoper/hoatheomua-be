# UpdatePlatformImages

`PUT /api/v1/posts/{id}/platforms/{platform}/images`

Đã triển khai trên nhánh `feature/post-gen` ở mốc 6 (2026-09-14), chưa deploy. JWT Staff/Admin. Không gọi AI.

Thay tập ô ảnh trong một transaction khóa BasePost: chọn version, đổi thứ tự, thêm hoặc bỏ ô. Mảng images bắt buộc, 0–100 ô, [] bỏ hết. Kiểm đúng bài/cặp, Succeeded, source còn hiệu lực và thứ tự không trùng. Không sinh ảnh/nội dung. Ready khi có content và ảnh chốt; giữ OfficialGenerationId và nền tảng khác.

Request/response JSON, validation và lỗi: [bàn giao FE mốc 6](../FE-API-Handoff-PostGen-Milestone6.md). Contract chung: [ApiContract.md](ApiContract.md).

Kiểm thử: `HoaTheoMua.Service.Test/PostGenTest/UpdatePlatformImages.cs`, transaction/concurrency tại `ImageVersionsPostgres.cs`, HTTP tại `Verification/Program.cs`. Toàn bộ provider dùng giả lập.
