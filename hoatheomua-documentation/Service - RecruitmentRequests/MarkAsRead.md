#### Request
```json
PATCH /api/v1/recruitment-requests/77777777-7777-7777-7777-777777777777/read
```

#### Response
```json
{
  "value": {
    "id": "77777777-7777-7777-7777-777777777777",
    "isRead": true,
    "updatedAt": "2026-06-23T09:00:00.00000Z"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000004",
  "timestampUtc": "2026-06-23T09:00:00.00000Z"
}
```

#### Flow diagram

![[../Store-Image/Pasted image 20260625001842.png]]


#### Test case
**Test case 01**: Đánh dấu đã đọc thành công  
**Điều kiện:** ID tồn tại, quyền Admin.  
**Kết quả:** Cập nhật isRead = true, trả về thông tin.

**Test case 02**: Không tìm thấy request  
**Điều kiện:** ID không tồn tại hoặc IsDeleted = true.  
**Kết quả:** Throw NotFoundException "RECRUITMENT_REQUEST_NOT_FOUND".

**Test case 03**: Service không kiểm tra quyền Admin  
**Điều kiện:** User đã đăng nhập (không cần role Admin).  
**Kết quả:** Service hiện tại vẫn cập nhật isRead = true thành công (chưa có kiểm tra role Admin).

**Test case 04**: id là Guid.Empty  
**Điều kiện:** id = "00000000-0000-0000-0000-000000000000".  
**Kết quả:** Service hiện tại không tìm thấy request tương ứng nên ném NotFoundException "RECRUITMENT_REQUEST_NOT_FOUND".

**Test case 05**: Request đã được đánh dấu đã đọc trước đó (isRead = true)  
**Điều kiện:** Request đã có isRead = true trước khi gọi PATCH.  
**Kết quả:** Vẫn cập nhật thành công (idempotent), UpdatedAt được refresh.

**Test case 06**: Request đã bị soft delete  
**Điều kiện:** ID thuộc về request có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "RECRUITMENT_REQUEST_NOT_FOUND".
