#### Request
```CSharp

  "filters": {
  }

```
#### Response
```CSharp

{
  "value": {
    "code": 1,
    "paginator": {
      "next": null
    },
    "data": [
      {
        "id": 293,
        "businessId": 126981,
        "name": "GIÀU",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1782381582
      },
      {
        "id": 292,
        "businessId": 126981,
        "name": "Shipper Bảo",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1775210830
      },
      {
        "id": 291,
        "businessId": 126981,
        "name": "Shipper Huy",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1775210815
      },
      {
        "id": 290,
        "businessId": 126981,
        "name": "Shipper Hưng",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1775210703
      },
      {
        "id": 287,
        "businessId": 126981,
        "name": "Ship Aha",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1772803743
      },
      {
        "id": 274,
        "businessId": 126981,
        "name": "Ngọc Trân",
        "status": 1,
        "bgColor": null,
        "textColor": null,
        "createdAt": 1761891422
      },
      {
        "id": 273,
        "businessId": 126981,
        "name": "18h - 20h",
        "status": 1,
        "bgColor": "#000000",
        "textColor": "#f5eaea",
        "createdAt": 1760243376
      },
      {
        "id": 272,
        "businessId": 126981,
        "name": "Purchasing",
        "status": 1,
        "bgColor": null,
        "textColor": null,
        "createdAt": 1759804132
      },
      {
        "id": 270,
        "businessId": 126981,
        "name": "6h - 7h",
        "status": 1,
        "bgColor": "#240606",
        "textColor": "#ff3636",
        "createdAt": 1748346370
      },
      {
        "id": 262,
        "businessId": 126981,
        "name": "ĐIỆN HOA",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1715483653
      },
      {
        "id": 259,
        "businessId": 126981,
        "name": "Mỹ Linh",
        "status": 1,
        "bgColor": null,
        "textColor": null,
        "createdAt": 1711611535
      },
      {
        "id": 257,
        "businessId": 126981,
        "name": "Ngọc Hưởng",
        "status": 1,
        "bgColor": null,
        "textColor": null,
        "createdAt": 1711611511
      },
      {
        "id": 254,
        "businessId": 126981,
        "name": "Trà My",
        "status": 1,
        "bgColor": null,
        "textColor": null,
        "createdAt": 1710645834
      },
      {
        "id": 253,
        "businessId": 126981,
        "name": "VAT",
        "status": 1,
        "bgColor": null,
        "textColor": "#000",
        "createdAt": 1710645820
      },
      {
        "id": 250,
        "businessId": 126981,
        "name": "upsale",
        "status": 1,
        "bgColor": "#000000",
        "textColor": "#fcebeb",
        "createdAt": 1692177487
      },
      {
        "id": 248,
        "businessId": 126981,
        "name": "DOANH NGHIỆP",
        "status": 1,
        "bgColor": "rgba(0,0,0,0.43)",
        "textColor": "#000",
        "createdAt": 1685331678
      },
      {
        "id": 246,
        "businessId": 126981,
        "name": "7h - 8h",
        "status": 1,
        "bgColor": "#000000",
        "textColor": "#fff",
        "createdAt": 1664191287
      },
      {
        "id": 238,
        "businessId": 126981,
        "name": "17h - 19h",
        "status": 1,
        "bgColor": "#000000",
        "textColor": "#fff",
        "createdAt": 1663064653
      },
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580N:00000001",
  "timestampUtc": "2026-06-29T10:44:23.12788Z"
}

```

#### Flow diagram

![[Pasted image 20260629174229.png]]

#### Test case

**Test case 01: Lấy order label thành công**  
**Điều kiện:** API NhanhVn `/order/tags` trả HTTP 200, response parse được, `Code != 0`.  
**Kết quả:** Trả về `OrderLabelResponse`.

**Test case 02: NhanhVn trả lỗi HTTP**  
**Điều kiện:** API `/order/tags` trả mã lỗi HTTP non-200.  
**Kết quả:** Ném `BadRequestException` với message `NhanhVn API HTTP Error`.

**Test case 03: Response null hoặc empty**  
**Điều kiện:** API trả HTTP 200 nhưng `ReadFromJsonAsync<OrderLabelResponse>()` trả về `null`. 
**Kết quả:** Ném `BadRequestException` với message `NhanhVnResponse returned null or empty data`.

**Test case 04: Form field không hợp lệ**  
**Điều kiện:** API trả response có `Code == 0`.  
**Kết quả:** Ném `BadRequestException` với message `ERR_INVALID_FORM_FIELDS`.