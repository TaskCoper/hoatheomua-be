#### Request
```json
DELETE /api/v1/job-descriptions/55555555-5555-5555-5555-555555555555
```

#### Response
```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625111823.png]]

#### Test case
**Test case 01**: Xóa job thành công  
**Điều kiện:** ID tồn tại, không có recruitment request nào tham chiếu.  
**Kết quả:** Soft delete (IsDeleted = true), trả về 204 No Content.

**Test case 02**: Job có ứng viên (candidates)  
**Điều kiện:** Job đang có recruitment requests.  
**Kết quả:** Throw ConflictException "CANNOT_DELETE_JOB_HAS_CANDIDATES".

**Test case 03**: ID không hợp lệ  
**Điều kiện:** id = Guid.Empty.  
**Kết quả:** Throw BadRequestException "INVALID_JOB_ID".

**Test case 04**: Không tìm thấy job  
**Điều kiện:** ID không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 06**: Job đã bị soft delete trước đó  
**Điều kiện:** Job đã có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 07**: Xóa job có recruitment requests (kể cả đã bị soft delete)  
**Điều kiện:** Job có recruitment request tham chiếu, dù IsDeleted = true hay false.  
**Kết quả:** Ném ConflictException "CANNOT_DELETE_JOB_HAS_CANDIDATES" (không filter IsDeleted khi check).
