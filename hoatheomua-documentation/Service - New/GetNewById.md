#### Request
```json
GET /api/v1/news/66666666-6666-6666-6666-666666666666
GET /api/v1/news/cong-nghe-moi
```

#### Response
```json
{
  "value": {
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
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000002",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625000405.png]]

#### Test case
**Test case 01**: Lấy news theo ID thành công  
**Điều kiện:** Key là GUID hợp lệ, news tồn tại.  
**Kết quả:** Trả về thông tin chi tiết bài viết kèm categories và user.

**Test case 02**: Lấy news theo Slug thành công  
**Điều kiện:** Key là slug hợp lệ.  
**Kết quả:** Trả về thông tin chi tiết bài viết.

**Test case 03**: Key không hợp lệ  
**Điều kiện:** Key rỗng, >100 ký tự, hoặc slug format không đúng.  
**Kết quả:** Throw BadRequestException "NEW_LOOKUP_KEY_INVALID".

**Test case 04**: Không tìm thấy  
**Điều kiện:** ID/slug không tồn tại hoặc IsDeleted = true.  
**Kết quả:** Throw NotFoundException "NEW_NOT_FOUND".

**Test case 05**: Key là GUID không tồn tại trong DB  
**Điều kiện:** Key là GUID hợp lệ nhưng không có news nào matching.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND".

**Test case 06**: Key là slug không tồn tại trong DB  
**Điều kiện:** Key là slug hợp lệ nhưng không có news nào có slug này.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND".

**Test case 07**: Lấy news bằng slug khi news đã bị soft delete  
**Điều kiện:** Slug tồn tại nhưng news có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND" (query slug có filter IsDeleted = false).
