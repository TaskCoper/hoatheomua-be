#### Request
```json
GET /api/v1/job-descriptions?page=1&pageSize=10&title=senior&location=HCM&employmentType=FullTime&salaryMin=30000000&salaryMax=50000000&status=Open
```

#### Response
```json
{
  "value": {
    "items": [
      {
        "id": "55555555-5555-5555-5555-555555555555",
        "slug": "senior-software-engineer",
        "title": "Senior Software Engineer",
        "description": "We are looking for a senior software engineer...",
        "coverImageUrl": "https://example.com/cover.png",
        "avatarUrl": "https://example.com/avatar.png",
        "location": "Hồ Chí Minh",
        "employmentType": "fullTime",
        "salaryMin": 30000000,
        "salaryMax": 50000000,
        "salaryNegotiable": false,
        "openings": 2,
        "status": "open"
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000002",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625111925.png]]

#### Test case
**Test case 01**: Lấy danh sách thành công (không filter)  
**Điều kiện:** Có dữ liệu job descriptions trong DB.  
**Kết quả:** Trả về danh sách phân trang theo CreatedAt giảm dần.

**Test case 02**: Lọc theo title, location, employmentType, salary, status  
**Điều kiện:** Các query params được cung cấp.  
**Kết quả:** Danh sách được lọc theo điều kiện tương ứng.

**Test case 03**: Salary âm  
**Điều kiện:** salaryMin < 0 hoặc salaryMax < 0.  
**Kết quả:** Throw BadRequestException "SALARY_CANNOT_BE_NEGATIVE".

**Test case 04**: SalaryMin > SalaryMax  
**Điều kiện:** salaryMin > salaryMax.  
**Kết quả:** Throw BadRequestException "MIN_SALARY_GREATER_THAN_MAX_SALARY".

**Test case 05**: Page/PageSize không hợp lệ  
**Điều kiện:** page <= 0 hoặc pageSize <= 0.  
**Kết quả:** Tự động chuẩn hóa page = 1, pageSize = 10.

**Test case 06**: Filter theo employmentType không tồn tại trong DB  
**Điều kiện:** employmentType = "PartTime" nhưng không có job nào PartTime.  
**Kết quả:** Trả về danh sách rỗng, totalCount = 0.

**Test case 07**: Filter với salaryNegotiable = true  
**Điều kiện:** salaryNegotiable = true nên job không có salaryMin/salaryMax cụ thể.  
**Kết quả:** Job vẫn xuất hiện trong kết quả lọc salary vì có OR salaryNegotiable.

**Test case 08**: Kết hợp tất cả filter cùng lúc  
**Điều kiện:** Cung cấp title, location, employmentType, salaryMin, salaryMax, status.  
**Kết quả:** Tất cả filter kết hợp bằng AND, trả về job thỏa tất cả điều kiện.

**Test case 09**: pageSize = 1  
**Điều kiện:** Có nhiều hơn 1 job trong DB, pageSize = 1.  
**Kết quả:** Trả về 1 item mỗi trang, hasNextPage = true nếu còn job.
