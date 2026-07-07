#### Request
```json
GET /api/v1/news?pageIndex=1&pageSize=10&keyword=technology&status=Published
```

#### Response
```json
{
  "value": {
    "items": [
      {
        "id": "66666666-6666-6666-6666-666666666666",
        "title": "Công Nghệ Mới",
        "description": "Bài viết về công nghệ mới...",
        "viewCount": 150,
        "pictureUrl": "https://example.com/image.png",
        "slug": "cong-nghe-moi",
        "status": "Published",
        "createdAt": "2026-06-23T08:51:30.37969Z",
        "updatedAt": null,
        "categories": [
          {
            "categoryId": "44444444-4444-4444-4444-444444444441",
            "categoryName": "Technology"
          }
        ],
        "user": {
          "userId": "11111111-1111-1111-1111-111111111111",
          "email": "admin@example.com",
          "userAvatar": "https://example.com/avatar.png",
          "userFullName": "Admin User"
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


![[../Store-Image/Pasted image 20260625000429.png]]

#### Test case
**Test case 01**: Lấy danh sách news thành công  
**Điều kiện:** Có dữ liệu news trong DB, IsDeleted = false.  
**Kết quả:** Trả về danh sách phân trang kèm thông tin categories và user.

**Test case 02**: Lọc theo keyword  
**Điều kiện:** keyword tìm kiếm được cung cấp.  
**Kết quả:** Lọc news theo Title hoặc Description chứa keyword.

**Test case 03**: Lọc theo status  
**Điều kiện:** status = Published hoặc Draft.  
**Kết quả:** Chỉ trả về news có status tương ứng.

**Test case 04**: PageIndex/PageSize không hợp lệ  
**Điều kiện:** pageIndex <= 0 hoặc pageSize <= 0.  
**Kết quả:** Tự động chuẩn hóa pageIndex = 1, pageSize = 10.

**Test case 05**: Không có dữ liệu  
**Điều kiện:** DB rỗng hoặc tất cả đều IsDeleted = true.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 06**: Lọc theo status = Draft  
**Điều kiện:** status = "Draft".  
**Kết quả:** Chỉ trả về news có status = Draft, không trả về Published.

**Test case 07**: Filter không có kết quả  
**Điều kiện:** keyword/status filter không khớp với bất kỳ news nào.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 08**: Keyword tìm thấy trong Description nhưng không trong Title  
**Điều kiện:** keyword = "nội dung" có trong description nhưng không trong title.  
**Kết quả:** Vẫn trả về news vì filter tìm trong cả Title và Description.

**Test case 09**: Thứ tự sắp xếp  
**Điều kiện:** Có nhiều news trong DB.  
**Kết quả:** News mới nhất (CreatedAt gần nhất) ở đầu danh sách (OrderByDescending).