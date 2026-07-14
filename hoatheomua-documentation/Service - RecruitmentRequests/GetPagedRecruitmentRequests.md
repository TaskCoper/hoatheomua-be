#### Request
```json
GET /api/v1/recruitment-requests?page=1&pageSize=10&isRead=false
```

#### Response
```json
{
  "value": {
    "items": [
      {
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
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000001",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625001006.png]]

#### Test case
**Test case 01**: Lấy danh sách thành công (không filter)  
**Điều kiện:** Có dữ liệu recruitment requests, quyền Admin.  
**Kết quả:** Trả về danh sách phân trang.

**Test case 02**: Lọc theo isRead  
**Điều kiện:** isRead = true/false.  
**Kết quả:** Chỉ trả về các request có trạng thái isRead tương ứng.

**Test case 03**: Service không kiểm tra quyền Admin  
**Điều kiện:** User đã đăng nhập (không cần role Admin).  
**Kết quả:** Service hiện tại vẫn trả về danh sách bình thường (chưa có kiểm tra role Admin).

**Test case 04**: Lọc isRead = true  
**Điều kiện:** isRead = true.  
**Kết quả:** Chỉ trả về các request đã được đọc.

**Test case 05**: Không có request nào trong DB  
**Điều kiện:** DB không có recruitment request nào (hoặc tất cả đều IsDeleted = true).  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 06**: Phân trang với pageSize quá lớn  
**Điều kiện:** pageSize > 100.  
**Kết quả:** Tự động chuẩn hóa pageSize = 100.

**Test case 07**: Sắp xếp theo CreatedAt giảm dần  
**Điều kiện:** Có nhiều request với CreatedAt khác nhau.  
**Kết quả:** Danh sách trả về được sắp xếp theo CreatedAt giảm dần (mới nhất trước).