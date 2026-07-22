#### Request
```json
PUT /api/v1/news/66666666-6666-6666-6666-666666666666

{
  "title": "Công Nghệ Mới - Cập nhật",
  "description": "Nội dung đã được cập nhật...",
  "pictureUrl": "https://example.com/image-new.png",
  "status": "Published",
  "categoryIds": ["44444444-4444-4444-4444-444444444441", "44444444-4444-4444-4444-444444444442"]
}
```

#### Response
```json
{
  "value": "Cập nhật thông tin bài viết tin tức thành công.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000004",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625000446.png]]

#### Test case
**Test case 01**: Cập nhật bài viết thành công  
**Điều kiện:** ID tồn tại, categoryIds hợp lệ, quyền Admin.  
**Kết quả:** Cập nhật thông tin và category details, trả về message thành công.

**Test case 02**: CategoryIds rỗng  
**Điều kiện:** categoryIds = [] sau khi normalize.  
**Kết quả:** Throw BadRequestException "CATEGORY_IDS_REQUIRED".

**Test case 03**: Category không tồn tại  
**Điều kiện:** CategoryId mới không có trong DB.  
**Kết quả:** Throw BadRequestException "CATEGORY_NEWS_INVALID".

**Test case 04**: Không tìm thấy news  
**Điều kiện:** ID không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException "NEW_NOT_FOUND".

**Test case 06**: Cập nhật title -> slug giữ nguyên  
**Điều kiện:** Title thay đổi (vd "Title Cũ" -> "Title Mới"), các field khác giữ nguyên.  
**Kết quả:** Service hiện tại KHÔNG regenerate slug, slug giữ nguyên như cũ ("title-cu").

**Test case 07**: categoryIds thay đổi một phần (thêm mới, bớt cũ)  
**Điều kiện:** categoryIds cũ = [A, B], categoryIds mới = [B, C].  
**Kết quả:** CategoryNewsDetail: A bị xóa, C được thêm, B giữ nguyên.

**Test case 08**: categoryIds giữ nguyên (không thay đổi)  
**Điều kiện:** categoryIds mới giống hệt categoryIds cũ.  
**Kết quả:** Không xóa/thêm CategoryNewsDetail nào.

**Test case 09**: Chỉ cập nhật status từ Draft -> Published  
**Điều kiện:** News đang ở Draft, cập nhật status = Published.  
**Kết quả:** Cập nhật thành công, các field khác giữ nguyên.

**Test case 10**: News đã bị soft delete  
**Điều kiện:** NewsId thuộc về news có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND".

**Test case 11**: categoryIds chứa Guid.Empty  
**Điều kiện:** categoryIds = ["00000000-0000-0000-0000-000000000000", CategoryId hợp lệ].  
**Kết quả:** Bỏ qua Guid.Empty, cập nhật thành công với category hợp lệ còn lại.
