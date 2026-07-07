#### Request
```json
POST /api/v1/news

{
  "title": "Công Nghệ Mới",
  "description": "Bài viết về công nghệ mới...",
  "pictureUrl": "https://example.com/image.png",
  "status": "Published",
  "categoryIds": ["44444444-4444-4444-4444-444444444441"]
}
```

#### Response
```json
{
  "value": {
    "id": "66666666-6666-6666-6666-666666666666",
    "title": "Công Nghệ Mới",
    "description": "Bài viết về công nghệ mới...",
    "viewCount": 0,
    "pictureUrl": "https://example.com/image.png",
    "slug": "cong-nghe-moi",
    "status": "Published",
    "userId": "11111111-1111-1111-1111-111111111111",
    "categoryIds": ["44444444-4444-4444-4444-444444444441"],
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260624174422.png|595]]

#### Test case
**Test case 01**: Tạo bài viết thành công  
**Điều kiện:** Request hợp lệ, categoryIds tồn tại, quyền Admin.  
**Kết quả:** Tạo news + category details, trả về 201 Created.

**Test case 02**: CategoryIds rỗng  
**Điều kiện:** categoryIds = [] (danh sách rỗng).  
**Kết quả:** Throw BadRequestException "CATEGORY_IDS_REQUIRED".

**Test case 03**: Category không tồn tại  
**Điều kiện:** Một hoặc nhiều categoryId không có trong DB.  
**Kết quả:** Throw BadRequestException "CATEGORY_NEWS_INVALID".

**Test case 04**: Token không chứa UserId (chưa xác thực)  
**Điều kiện:** ICurrentUserService.GetRequiredUserId() ném UnauthorizedAccessException.  
**Kết quả:** Ném UnauthorizedAccessException "Unauthorized", không tạo news, không SaveChanges.

**Test case 05**: Slug bị trùng với bài viết khác  
**Điều kiện:** Title sinh ra slug đã tồn tại trong DB.  
**Kết quả:** Tự động thêm suffix số (slug mới là "cong-nghe-moi-2"), tạo thành công.

**Test case 06**: Title rỗng  
**Điều kiện:** title = "".  
**Kết quả:** Service hiện tại vẫn tạo thành công với slug được generate (không throw), Title = "".

**Test case 07**: categoryIds chứa Guid.Empty  
**Điều kiện:** categoryIds = [CategoryId hợp lệ, "00000000-0000-0000-0000-000000000000"].  
**Kết quả:** Bỏ qua Guid.Empty, tạo thành công với các category hợp lệ còn lại.

**Test case 08**: categoryIds bị trùng lặp  
**Điều kiện:** categoryIds = ["id1", "id1", "id2"].  
**Kết quả:** Deduplicate, chỉ tạo CategoryNewsDetail cho các categoryId khác nhau (2 cái).

**Test case 09**: Tạo news với status = Draft  
**Điều kiện:** status = "Draft".  
**Kết quả:** Tạo thành công, viewCount = 0, status = Draft.
