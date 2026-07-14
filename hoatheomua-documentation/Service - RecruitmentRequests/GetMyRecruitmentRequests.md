#### Request
```json
GET /api/v1/recruitment-requests/me?pageIndex=1&pageSize=10
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
  "traceId": "0HNMH0HLC67KU:00000002",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625000950.png]]

#### Test case
**Test case 01**: Lấy danh sách của tôi thành công  
**Điều kiện:** User đã đăng nhập, có dữ liệu recruitment requests.  
**Kết quả:** Trả về các request của user hiện tại.

**Test case 02**: Chưa đăng nhập  
**Điều kiện:** Request không có JWT token.  
**Kết quả:** Trả về 401 Unauthorized.

**Test case 03**: Không có dữ liệu  
**Điều kiện:** User chưa từng apply job nào.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 04**: User có nhiều request, phân trang hoạt động đúng  
**Điều kiện:** User có > pageSize request.  
**Kết quả:** Trả về đúng số lượng theo pageSize, hasNextPage = true.
