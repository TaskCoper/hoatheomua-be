#### Request
```json
PUT /api/v1/job-descriptions/55555555-5555-5555-5555-555555555555

{
  "title": "Senior Software Engineer Updated",
  "location": "Hà Nội",
  "avatarUrl": "https://example.com/avatar-new.png",
  "coverImageUrl": "https://example.com/cover-new.png",
  "minExperienceYears": 3,
  "maxExperienceYears": 7,
  "level": "senior",
  "employmentType": "FullTime",
  "salaryMin": 35000000,
  "salaryMax": 55000000,
  "salaryNegotiable": false,
  "description": "Updated description...",
  "status": "Open",
  "openings": 3
}
```

#### Response
```json
{
  "value": {
    "id": "55555555-5555-5555-5555-555555555555",
    "slug": "senior-software-engineer-updated",
    "title": "Senior Software Engineer Updated",
    "location": "Hà Nội",
    "avatarUrl": "https://example.com/avatar-new.png",
    "coverImageUrl": "https://example.com/cover-new.png",
    "minExperienceYears": 3,
    "maxExperienceYears": 7,
    "level": "senior",
    "employmentType": "fullTime",
    "salaryMin": 35000000,
    "salaryMax": 55000000,
    "salaryNegotiable": false,
    "openings": 3,
    "description": "Updated description...",
    "status": "open",
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "updatedAt": "2026-06-23T09:00:00.00000Z"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T09:00:00.00000Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625112209.png]]

#### Test case
**Test case 01**: Cập nhật thành công  
**Điều kiện:** ID tồn tại, quyền Admin, request hợp lệ.  
**Kết quả:** Cập nhật thông tin, regenerate slug nếu title thay đổi.

**Test case 02**: ID không hợp lệ  
**Điều kiện:** id = Guid.Empty.  
**Kết quả:** Throw BadRequestException "INVALID_JOB_ID".

**Test case 03**: Validation thất bại  
**Điều kiện:** Thiếu required fields.  
**Kết quả:** Throw ValidationExceptionError.

**Test case 04**: Không tìm thấy job  
**Điều kiện:** ID không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 06**: Cập nhật salaryMin > salaryMax  
**Điều kiện:** salaryMin = 50000000, salaryMax = 30000000.  
**Kết quả:** Validation fail, throw ValidationExceptionError.

**Test case 07**: Cập nhật title -> slug được regenerate  
**Điều kiện:** Title thay đổi từ "Senior" thành "Junior".  
**Kết quả:** Slug được tạo mới từ title, không giữ slug cũ.

**Test case 08**: Cập nhật title giống hệt cũ (không thay đổi)  
**Điều kiện:** Title giữ nguyên.  
**Kết quả:** Slug giữ nguyên, không regenerate.

**Test case 09**: Cập nhật status từ Open -> Closed  
**Điều kiện:** Job đang Open, cập nhật status = Closed.  
**Kết quả:** Cập nhật thành công, status chuyển sang Closed.

**Test case 10**: Cập nhật openings = 0  
**Điều kiện:** openings = 0.  
**Kết quả:** Throw with Messgae "OPENINGS_MUST_BE_GREATER_THAN_ZERO"

**Test case 11**: Job đã bị soft delete  
**Điều kiện:** Job có IsDeleted = true.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND".

**Test case 12**: Cập nhật với title rỗng  
**Điều kiện:** title = "" hoặc "   " (whitespace).  
**Kết quả:** Validation fail, throw ValidationExceptionError.

**Test case 13**: Salary âm  
**Điều kiện:** salaryMin = -1000000.  
**Kết quả:** Validation fail, throw ValidationExceptionError.

