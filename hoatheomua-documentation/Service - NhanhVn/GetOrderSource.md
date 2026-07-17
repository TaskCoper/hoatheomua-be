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
        "id": 995,
        "name": "Other (BNI)",
        "status": 1,
        "createdAt": 1663064382
      },
      {
        "id": 996,
        "name": "Zalo (Hotline)",
        "status": 1,
        "createdAt": 1663064392
      },
      {
        "id": 1348,
        "name": "CSKH",
        "status": 1,
        "createdAt": 1663984398
      },
      {
        "id": 1403,
        "name": "Meta Shop",
        "status": 1,
        "createdAt": 1664333012
      },
      {
        "id": 1502,
        "name": "B2B",
        "status": 1,
        "createdAt": 1664967414
      },
      {
        "id": 1519,
        "name": "https://hoatheomua.com",
        "status": 2,
        "createdAt": 1664989890
      },
      {
        "id": 1671,
        "name": "Zalo OA HTM",
        "status": 2,
        "createdAt": 1666155793
      },
      {
        "id": 1672,
        "name": "Shopee",
        "status": 2,
        "createdAt": 1686718293
      },
      {
        "id": 1673,
        "name": "CTV",
        "status": 2,
        "createdAt": 1686718299
      },
      {
        "id": 1674,
        "name": "WEBSITE",
        "status": 1,
        "createdAt": 1686791438
      },
      {
        "id": 1675,
        "name": "Partner",
        "status": 2,
        "createdAt": 1688800655
      },
      {
        "id": 1676,
        "name": "Zalo Mini App",
        "status": 1,
        "createdAt": 1749098296
      },
      {
        "id": 1677,
        "name": "Instagram HTM",
        "status": 2,
        "createdAt": 1749867371
      },
      {
        "id": 1678,
        "name": "MKT",
        "status": 1,
        "createdAt": 1750151843
      },
      {
        "id": 1679,
        "name": "Nội bộ",
        "status": 1,
        "createdAt": 1751339248
      },
      {
        "id": 1680,
        "name": "NGUỒN NEW 1",
        "status": 1,
        "createdAt": 1769393409
      },
      {
        "id": 1681,
        "name": "CRM",
        "status": 1,
        "createdAt": 1769422193
      },
      {
        "id": 1682,
        "name": "TRẠM HOA",
        "status": 1,
        "createdAt": 1778752705
      }
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580N:00000002",
  "timestampUtc": "2026-06-29T10:46:19.021736Z"
}

```

#### Flow diagram


![[../Store-Image/Pasted image 20260629174350.png]]

#### Test case

**Test case 01: Lấy order source thành công**  
**Điều kiện:** API NhanhVn `/order/source` trả HTTP 200, response parse được, `Code != 0`.  
**Kết quả:** Trả về `OrderSourceResponse`.

**Test case 02: NhanhVn trả lỗi HTTP**  
**Điều kiện:** API `/order/source` trả mã lỗi HTTP non-200.  
**Kết quả:** Ném `BadRequestException` với message `NhanhVn API HTTP Error`.

**Test case 03: Response null hoặc empty**  
**Điều kiện:** API trả HTTP 200 nhưng `ReadFromJsonAsync<OrderSourceResponse>()` trả về `null`
**Kết quả:** Ném `BadRequestException` với message `NhanhVnResponse returned null or empty data`.

**Test case 04: Form field không hợp lệ**  
**Điều kiện:** API trả response có `Code == 0`.  
**Kết quả:** Ném `BadRequestException` với message `ERR_INVALID_FORM_FIELDS`.