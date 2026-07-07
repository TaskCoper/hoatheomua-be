#### Request  
```json  
// GET /api/v1/ahamove/service-details  
// Không có tham số  
```  
#### Response  
```json  
// Controller trả về trực tiếp ServiceDetailsResponse (không bọc envelope ApiResponse)  
{
  "_id": "SGN-BIKE",
  "enable": true,
  "order": 1,
  "city_id": "SGN",
  "name": "AhaMove Bike",
  "name_vi_vn": "Giao hàng xe máy",
  "name_en_us": "AhaMove Bike",
  "cod": 0,
  "max_stop_points": 5,
  "requests": []
}
```
#### Flow diagram
![[../Store-Image/GetServiceDetailsAhaMove.png]]
#### Test case
**Test case 01**: Response hợp lệ (Happy case)  
**Điều kiện:** AhaMove trả về HTTP 200 với body hợp lệ (ServiceDetailsResponse có Id, Name, Enable).  
**Kết quả:** Trả về ServiceDetailsResponse với Id = "SGN-BIKE", Name = "AhaMove Bike", Enable = true.  
  
**Test case 02**: AhaMove trả về HTTP status lỗi  
**Điều kiện:** AhaMove trả về status không thành công (HTTP 401 Unauthorized).  
**Kết quả:** Hệ thống ném BadRequestException với message chứa "Ahamove API HTTP Error".  
  
**Test case 03**: Body deserialize ra null  
**Điều kiện:** AhaMove trả về HTTP 200 nhưng body là "null".  
**Kết quả:** Hệ thống ném BadRequestException với message "Ahamove returned null or empty data".