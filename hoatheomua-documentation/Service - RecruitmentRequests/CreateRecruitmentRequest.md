#### Request
```json
POST /api/v1/recruitment-requests

{
  "jobDescriptionId": "55555555-5555-5555-5555-555555555555",
  "title": "Application for Senior Developer",
  "coverLetter": "I am writing to apply for the Senior Developer position...",
  "attachments": ["https://example.com/cv.pdf"]
}
```

#### Response
```json
{
  "value": "Success",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260625001600.png]]

#### Test case
**Test case 01**: Apply job thành công  
**Điều kiện:** Job tồn tại và đang Open, user chưa apply trước đó.  
**Kết quả:** Tạo mới recruitment request, trả về 201 Created.

**Test case 02**: Job không tồn tại hoặc không mở  
**Điều kiện:** jobDescriptionId không có trong DB hoặc status != Open.  
**Kết quả:** Throw NotFoundException "JOB_DESCRIPTION_NOT_FOUND_OR_NOT_OPEN".

**Test case 03**: Spam (đã apply và chưa được đọc)  
**Điều kiện:** User đã apply cho job này trước đó và isRead = false.  
**Kết quả:** Throw TooManyRequestException "APPLICATION_ALREADY_SUBMITTED_AND_PENDING".

**Test case 04**: Chưa đăng nhập  
**Điều kiện:** Request không có JWT token.  
**Kết quả:** Trả về 401 Unauthorized.

**Test case 05**: Attachments rỗng  
**Điều kiện:** attachments = [] hoặc không gửi attachments.  
**Kết quả:** Vẫn tạo thành công (attachments là optional).

**Test case 06**: CoverLetter quá dài  
**Điều kiện:** coverLetter > 2000 ký tự.  
**Kết quả:** Service hiện tại vẫn tạo thành công (chưa có validation độ dài coverLetter).

**Test case 07**: jobDescriptionId là Guid.Empty  
**Điều kiện:** jobDescriptionId = "00000000-0000-0000-0000-000000000000".  
**Kết quả:** Ném NotFoundException "JOB_DESCRIPTION_NOT_FOUND_OR_NOT_OPEN".

**Test case 08**: User apply lại job đã apply trước đó nhưng request cũ đã được đọc (isRead = true)  
**Điều kiện:** User đã apply job này, request cũ có isRead = true.  
**Kết quả:** Cho phép apply lại (không bị spam check vì isRead = false mới bị chặn).

**Test case 09**: User apply lại job đã apply trước đó nhưng request cũ đã bị soft delete  
**Điều kiện:** User đã apply job này, request cũ IsDeleted = true, isRead = false.  
**Kết quả:** Service hiện tại vẫn chặn spam, ném TooManyRequestException "APPLICATION_ALREADY_SUBMITTED_AND_PENDING" (spam filter chưa loại request đã soft delete).

**Test case 10**: Title rỗng  
**Điều kiện:** title = "".  
**Kết quả:** Service hiện tại vẫn tạo thành công (chưa có validation Required cho title).
