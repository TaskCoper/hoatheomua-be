#### Request
```json
GET /api/v1/job-descriptions/senior-software-engineer
GET /api/v1/job-descriptions/55555555-5555-5555-5555-555555555555
```

#### Response
```json
{
  "value": {
    "id": "55555555-5555-5555-5555-555555555555",
    "slug": "senior-software-engineer",
    "title": "Senior Software Engineer",
    "location": "Hồ Chí Minh",
    "avatarUrl": "https://example.com/avatar.png",
    "coverImageUrl": "https://example.com/cover.png",
    "minExperienceYears": 3,
    "maxExperienceYears": 7,
    "level": "senior",
    "employmentType": "fullTime",
    "salaryMin": 30000000,
    "salaryMax": 50000000,
    "salaryNegotiable": false,
    "openings": 2,
    "description": "We are looking for a senior software engineer...",
    "status": "open",
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "updatedAt": "2026-06-23T08:51:30.37969Z"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000004",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625111906.png]]

#### Test case
**Test case 01**: Lấy theo ID thành công  
**Điều kiện:** Key là GUID hợp lệ, tồn tại trong DB.  
**Kết quả:** Trả về thông tin chi tiết job description.

**Test case 02**: Lấy theo Slug thành công  
**Điều kiện:** Key là slug hợp lệ, tồn tại trong DB.  
**Kết quả:** Trả về thông tin chi tiết job description.

**Test case 03**: Key không hợp lệ  
**Điều kiện:** Key rỗng, >300 ký tự, hoặc format không đúng (slug pattern).  
**Kết quả:** Throw BadRequestException "JOB_DESCRIPTION_LOOKUP_KEY_INVALID".

**Test case 04**: Không tìm thấy  
**Điều kiện:** ID hoặc slug không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 06**: Slug tồn tại nhưng job đã bị soft delete  
**Điều kiện:** Slug hợp lệ, job tồn tại nhưng IsDeleted = true.  
**Kết quả:** Ném NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 07**: Key là GUID format nhưng không tồn tại trong DB  
**Điều kiện:** Key = "99999999-9999-9999-9999-999999999999" (GUID hợp lệ nhưng không có trong DB).  
**Kết quả:** Ném NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 08**: Key có chứa khoảng trắng  
**Điều kiện:** Key = "senior developer" (có space).  
**Kết quả:** Ném BadRequestException "JOB_DESCRIPTION_LOOKUP_KEY_INVALID" (slug format không hợp lệ).

**Test case 09**: Key là slug không đúng format (chứa ký tự đặc biệt)  
**Điều kiện:** Key = "senior@developer!".  
**Kết quả:** Ném BadRequestException "JOB_DESCRIPTION_LOOKUP_KEY_INVALID".
