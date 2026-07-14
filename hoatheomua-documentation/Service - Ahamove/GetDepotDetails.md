#### Request
```CSharp

  "filters": {
  }

```
#### Response
```Csharp
```CSharp

{
  "value": {
    "code": 1,
    "paginator": null,
    "data": [
      {
        "id": 129255,
        "code": "Kho CN head",
        "name": "Kho CN HEAD",
        "mobile": "0988364353",
        "location": {
          "cityId": 255,
          "cityName": "Hồ Chí Minh",
          "districtId": 358,
          "districtName": "Quận Phú Nhuận",
          "wardId": 10376,
          "wardName": "Phường 9",
          "address": "108/3 ĐÀO DUY ANH"
        },
        "locationV2": {
          "cityId": 0,
          "cityName": null,
          "wardId": 0,
          "wardName": null,
          "address": "108/3 ĐÀO DUY ANH"
        }
      },
      {
        "id": 130526,
        "code": "Kho Tổng",
        "name": "Kho Tổng",
        "mobile": "0931734353",
        "location": {
          "cityId": 255,
          "cityName": "Hồ Chí Minh",
          "districtId": 358,
          "districtName": "Quận Phú Nhuận",
          "wardId": 10376,
          "wardName": "Phường 9",
          "address": "108/3 ĐÀO DUY ANH, PHƯỜNG 9, PHÚ NHUẬN"
        },
        "locationV2": {
          "cityId": 0,
          "cityName": null,
          "wardId": 0,
          "wardName": null,
          "address": "108/3 ĐÀO DUY ANH, PHƯỜNG 9, PHÚ NHUẬN"
        }
      }
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580N:00000004",
  "timestampUtc": "2026-06-29T10:48:17.589154Z"
}

```


#### Flow diagram
![[../Store-Image/GetDepotDetails.png]]
#### Test case
**Test case 01**: Response hợp lệ (Happy case)  
**Điều kiện:** NhanhVn trả về HTTP 200, Code != 0 (Code = 1) và có data (danh sách depot hợp lệ).  
**Kết quả:** Trả về DepotResponse hợp lệ với Code = 1, data có đúng 1 phần tử, depot có Code = "DEPOT_1".  
  
**Test case 02**: NhanhVn trả về HTTP status lỗi  
**Điều kiện:** NhanhVn trả về status không thành công (HTTP 500 InternalServerError).  
**Kết quả:** Hệ thống ném BadRequestException với message chứa "NhanhVn API HTTP Error".  
  
**Test case 03**: Body deserialize ra null  
**Điều kiện:** NhanhVn trả về HTTP 200 nhưng body là "null".  
**Kết quả:** Hệ thống ném BadRequestException với message "NhanhVnResponse returned null or empty data".  
  
**Test case 04**: Response Code == 0  
**Điều kiện:** NhanhVn trả về HTTP 200 nhưng Code == 0.  
**Kết quả:** Hệ thống ném BadRequestException với message "ERR_INVALID_FORM_FIELDS".