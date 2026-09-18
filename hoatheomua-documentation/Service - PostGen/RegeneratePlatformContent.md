# RegeneratePlatformContent

`POST /api/v1/posts/{id:guid}/platforms/{platform}/content/regenerate`

Mốc 5 đã triển khai trên nhánh `feature/post-gen`, chưa deploy. JWT Staff/Admin; bài/nền tảng phải tồn tại và chưa xóa.

Sinh lại nội dung theo sourceGenerationId/feedback, tạo version Ai và tự chốt. Chỉ endpoint này trong mốc 5 gọi AI và áp dụng heavy rate limit.

Contract request/response, validation, trạng thái, giới hạn và cách FE xử lý tại [bàn giao FE mốc 5](../FE-API-Handoff-PostGen-Milestone5.md). API mutation trả PostCoreDetail (chỉ nội dung/ảnh chốt), API lịch sử trả ContentHistory riêng. Không cần migration mới.

Kiểm chứng bằng service test cùng tên trong HoaTheoMua.Service.Test/PostGenTest, ContentVersionsPostgres và HTTP probe. AI dùng fake HttpMessageHandler, không gọi proxy thật. Xem [Implementation.md](Implementation.md).
