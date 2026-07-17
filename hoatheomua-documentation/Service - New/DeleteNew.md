#### Request
```json
DELETE /api/v1/news/66666666-6666-6666-6666-666666666666
```

#### Response
```json
{
  "value": "Xóa bài viết thành công.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram

![[../Store-Image/Pasted image 20260625000335.png]]


#### Test case
**Test case 01**: Xóa bài viết thành công  
**Điều kiện:** ID tồn tại, quyền Admin.  
**Kết quả:** Soft delete (IsDeleted = true), trả về message thành công.

**Test case 02**: Không tìm thấy bài viết  
**Điều kiện:** ID không tồn tại hoặc đã bị xóa trước đó.  
**Kết quả:** Throw NotFoundException "NEW_NOT_FOUND".

**Test case 03**: News đã bị soft delete (đã xóa trước đó)  
**Điều kiện:** NewsId thuộc về news có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND", không SaveChanges.

**Test case 04**: id là Guid.Empty  
**Điều kiện:** id = "00000000-0000-0000-0000-000000000000".  
**Kết quả:** Service hiện tại không validate riêng Guid.Empty → ném NotFoundException "NEW_NOT_FOUND".

**Test case 05**: News đã bị soft delete trước đó  
**Điều kiện:** NewsId thuộc về news đã có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "NEW_NOT_FOUND".
