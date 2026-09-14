# RegenerateImage

`POST /api/v1/posts/{id}/images/regenerate`

Đã triển khai trên nhánh `feature/post-gen` ở mốc 6 (2026-09-14), chưa deploy. JWT Staff/Admin. HeavyPolicy; có gọi AI.

Sinh lại đúng một cặp ảnh nguồn × tỷ lệ, từ ảnh gốc hoặc phiên bản thành công được chọn. Lưu parent, URL nguồn thực tế, feedback mới nhất và metadata AI. Không tự đổi ảnh chốt hoặc trạng thái nền tảng. Provider lỗi lưu Failed và trả 200 ImageVersion; validation, config hoặc bài/nguồn bị xóa trả lỗi tương ứng.

Request/response JSON, validation và lỗi: [bàn giao FE mốc 6](../FE-API-Handoff-PostGen-Milestone6.md). Contract chung: [ApiContract.md](ApiContract.md).

Kiểm thử: `HoaTheoMua.Service.Test/PostGenTest/RegenerateImage.cs`, transaction/concurrency tại `ImageVersionsPostgres.cs`, HTTP tại `Verification/Program.cs`. Toàn bộ provider dùng giả lập.
