#### Request
```json
GET /api/v1/public/job-descriptions/senior-software-engineer
GET /api/v1/public/job-descriptions/55555555-5555-5555-5555-555555555555
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
  "traceId": "0HNMH0HLC67KU:00000007",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625112023.png]]

#### Test case
**Test case 01**: Lấy public job theo ID/Slug thành công  
**Điều kiện:** Key hợp lệ, job tồn tại và đang Open.  
**Kết quả:** Trả về thông tin chi tiết job.

**Test case 02**: Job không ở trạng thái Open  
**Điều kiện:** Job tồn tại nhưng status = Closed.  
**Kết quả:** Throw NotFoundException (do query filter status == Open).

**Test case 03**: Key không hợp lệ  
**Điều kiện:** Key rỗng, >300 ký tự, hoặc format không đúng.  
**Kết quả:** Throw BadRequestException "JOB_DESCRIPTION_LOOKUP_KEY_INVALID".

**Test case 04**: Không tìm thấy  
**Điều kiện:** ID/slug không tồn tại.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 05**: Key là GUID không tồn tại trong DB  
**Điều kiện:** Key là GUID hợp lệ nhưng không có job nào matching.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 06**: Slug tồn tại nhưng status != Open  
**Điều kiện:** Slug hợp lệ, job tồn tại nhưng status = Closed hoặc Draft.  
**Kết quả:** Throw NotFoundException (do filter status == Open).

**Test case 07**: Job đã bị soft delete  
**Điều kiện:** Slug hợp lệ, job tồn tại nhưng IsDeleted = true.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".
