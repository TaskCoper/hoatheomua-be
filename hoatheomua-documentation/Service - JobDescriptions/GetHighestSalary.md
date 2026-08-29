#### Request
```json
GET /api/v1/job-descriptions/highest-salary
```

#### Response
```json
{
  "value": {
    "salary": 50000000
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625111848.png]]

#### Test case
**Test case 01**: Lấy mức lương cao nhất thành công  
**Điều kiện:** Có job descriptions với salaryMax.  
**Kết quả:** Trả về mức lương cao nhất.

**Test case 02**: Không có dữ liệu  
**Điều kiện:** DB rỗng (không có job nào).  
**Kết quả:** Trả về salary = null.

**Test case 04**: Có nhiều job với salaryMax khác nhau  
**Điều kiện:** Nhiều job có salaryMax, giá trị cao nhất là 100000000.  
**Kết quả:** Trả về salary = 100000000.

**Test case 05**: Tất cả job đều có salaryMax = null  
**Điều kiện:** Không có job nào có salaryMax (đều null).  
**Kết quả:** Trả về salary = null.

**Test case 06**: Job có salaryMax = 0  
**Điều kiện:** salaryMax = 0 (mức lương tối đa là 0).  
**Kết quả:** Vẫn được tính là giá trị hợp lệ, trả về salary = 0 nếu là max.
