# GetContentVersions

`GET /api/v1/posts/{id:guid}/platforms/{platform}/content/versions`

Mốc 5 đã triển khai trên nhánh `feature/post-gen`, chưa deploy. JWT Staff/Admin; bài/nền tảng phải tồn tại và chưa xóa.

Đọc riêng lịch sử theo nền tảng, thứ tự (CreatedAt, Id), versionNo tính lúc đọc, có Feedback/IsOfficial. Không phân trang; không trả metadata provider/prompt.

Contract request/response, validation, trạng thái, giới hạn và cách FE xử lý tại [bàn giao FE mốc 5](../FE-API-Handoff-PostGen-Milestone5.md). API mutation trả PostCoreDetail (chỉ nội dung/ảnh chốt), API lịch sử trả ContentHistory riêng. Không cần migration mới.

Kiểm chứng bằng service test cùng tên trong HoaTheoMua.Service.Test/PostGenTest, ContentVersionsPostgres và HTTP probe. AI dùng fake HttpMessageHandler, không gọi proxy thật. Xem [Implementation.md](Implementation.md).
