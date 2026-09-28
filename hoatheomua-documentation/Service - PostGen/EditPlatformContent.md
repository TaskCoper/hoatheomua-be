# EditPlatformContent

`PUT /api/v1/posts/{id:guid}/platforms/{platform}/content`

Mốc 5 đã triển khai trên nhánh `feature/post-gen`, chưa deploy. JWT Staff/Admin; bài/nền tảng phải tồn tại và chưa xóa.

Sửa tay title/content/hashtag thành Manual và tự chốt. Lưu liên tiếp bản Manual đang chốt/mới nhất/chưa có con sẽ ghi đè dòng đó, giữ CreatedAt và số version; chọn lại bản cũ rồi sửa sẽ tạo nhánh mới.

Contract request/response, validation, trạng thái, giới hạn và cách FE xử lý tại [bàn giao FE mốc 5](../FE-API-Handoff-PostGen-Milestone5.md). API mutation trả PostCoreDetail (chỉ nội dung/ảnh chốt), API lịch sử trả ContentHistory riêng. Không cần migration mới.

Kiểm chứng bằng service test cùng tên trong HoaTheoMua.Service.Test/PostGenTest, ContentVersionsPostgres và HTTP probe. AI dùng fake HttpMessageHandler, không gọi proxy thật. Xem [Implementation.md](Implementation.md).
