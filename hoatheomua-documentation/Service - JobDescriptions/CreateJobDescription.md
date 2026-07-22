#### Request
```json
POST /api/v1/job-descriptions

{
  "title": "Senior Software Engineer",
  "location": "Hồ Chí Minh",
  "avatarUrl": "https://example.com/avatar.png",
  "coverImageUrl": "https://example.com/cover.png",
  "minExperienceYears": 3,
  "maxExperienceYears": 7,
  "level": "senior",
  "employmentType": "FullTime",
  "salaryMin": 30000000,
  "salaryMax": 50000000,
  "salaryNegotiable": false,
  "description": "We are looking for a senior software engineer...",
  "status": "Open",
  "openings": 2
}
```

#### Response
```json
{
  "value": {
    "id": "55555555-5555-5555-5555-555555555555",
    "slug": "senior-software-engineer",
    "title": "Senior Software Engineer",
    "status": "open",
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000001",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
[[]]
#### Flow diagram[]()


![[../Store-Image/Pasted image 20260625111756.png]]

#### Test case
**Test case 01**: Tạo job description thành công  
**Điều kiện:** Request hợp lệ, quyền Admin.  
**Kết quả:** Tạo mới thành công, trả về 201 Created kèm id, slug.

**Test case 02**: Validation thất bại  
**Điều kiện:** Thiếu required fields (title, location, etc.).  
**Kết quả:** Throw ValidationExceptionError với message tương ứng.


**Test case 04**: Slug bị trùng với job description khác  
**Điều kiện:** Title sinh ra slug đã tồn tại trong DB.  
**Kết quả:** Tự động thêm suffix số (slug mới là "senior-software-engineer-2" vì suffix tăng từ 1 trước khi gán), tạo thành công.

**Test case 05**: SalaryMin > SalaryMax  
**Điều kiện:** salaryMin = 50000000, salaryMax = 30000000.  
**Kết quả:** Validation fail (nếu có FluentValidation check), throw ValidationExceptionError.

**Test case 06**: Salary âm  
**Điều kiện:** salaryMin = -1000000.  
**Kết quả:** Validation fail, throw ValidationExceptionError.

**Test case 09**: Openings <= 0  
**Điều kiện:** openings = 0 hoặc openings = -1.  
**Kết quả:** Validation fail, throw ValidationExceptionError.

**Test case 10**: Title rỗng  
**Điều kiện:** title = "" hoặc "   " (whitespace).  
**Kết quả:** Validation fail, throw ValidationExceptionError.

**Test case 11**: Location rỗng  
**Điều kiện:** location = "" hoặc "   " (whitespace).  
**Kết quả:** Validation fail, throw ValidationExceptionError.


