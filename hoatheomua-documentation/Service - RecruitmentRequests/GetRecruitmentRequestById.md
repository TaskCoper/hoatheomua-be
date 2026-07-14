#### Request
```json
GET /api/v1/recruitment-requests/77777777-7777-7777-7777-777777777777
```

#### Response
```json
{
  "value": {
    "id": "77777777-7777-7777-7777-777777777777",
    "title": "Application for Senior Developer",
    "coverLetter": "I am writing to apply for...",
    "attachments": ["https://example.com/cv.pdf"],
    "isRead": false,
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "user": {
      "userId": "11111111-1111-1111-1111-111111111111",
      "fullName": "Nguyễn Văn A",
      "email": "nguyenvana@example.com",
      "phoneNumber": null,
      "avatar": "https://example.com/avatar.png"
    },
    "jobDescription": {
      "id": "55555555-5555-5555-5555-555555555555",
      "title": "Senior Software Engineer"
    }
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram



![[../Store-Image/Pasted image 20260625001628.png]]
#### Test case
**Test case 01**: Lấy chi tiết thành công  
**Điều kiện:** ID tồn tại, quyền Admin.  
**Kết quả:** Trả về thông tin chi tiết kèm user và job description.

**Test case 02**: ID không hợp lệ  
**Điều kiện:** id = Guid.Empty.  
**Kết quả:** Throw BadRequestException "INVALID_RECRUITMENT_REQUEST_ID".

**Test case 03**: Không tìm thấy  
**Điều kiện:** ID không tồn tại hoặc IsDeleted = true.  
**Kết quả:** Throw NotFoundException "RECRUITMENT_REQUEST_NOT_FOUND".

**Test case 04**: Service không kiểm tra quyền Admin  
**Điều kiện:** User đã đăng nhập (không cần role Admin).  
**Kết quả:** Service hiện tại vẫn trả về chi tiết request bình thường (chưa có kiểm tra role Admin).

**Test case 05**: ID tồn tại nhưng đã bị soft delete  
**Điều kiện:** ID thuộc về request có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "RECRUITMENT_REQUEST_NOT_FOUND".

**Test case 06**: Request không có coverLetter hoặc attachments  
**Điều kiện:** Request được tạo không có coverLetter và attachments = [].  
**Kết quả:** Trả về response với coverLetter = null, attachments = [].
