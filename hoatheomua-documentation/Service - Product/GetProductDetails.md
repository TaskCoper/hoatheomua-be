#### Request
```Json
{
  "filters": {
    "id": "string"
  }
}
```
#### Response
```json
{
  "value": {
    "code": 1,
    "data": {
      "id": 4934210,
      "parentId": -2,
      "code": "HB_CRP_2026",
      "barcode": "2000214275339",
      "name": "Bó Coral Pink_2026",
      "status": 1,
      "category": null,
      "categoryId": 19810,
      "internalCategory": null,
      "prices": {
        "retail": 0,
        "retailVat": 0,
        "import": 0,
        "importVat": 0,
        "old": 0,
        "wholesale": 0,
        "avgCost": 0
        , .....
          ],
          "inventory": {
            "remain": 0,
            "shipping": 0,
            "damaged": 0,
            "holding": 0,
            "transfering": 0,
            "warranty": {
              "remain": 0,
              "holding": 0
            },
            "warrantyHolding": 0,
            "holdingNew": 0,
            "shippingExport": 0,
            "available": 0,
            "depots": []
          },
          "attributes": [
            {
              "id": 6159,
              "name": "Màu sắc",
              "value": "Hồng"
            }
          ],
          "updatedAt": 1782189178,
          "createdAt": 1779949456,
          "description": null,
          "content": null,
          "suppliers": [],
          "videos": [],
          "branchPrices": [],
          "childs": []
        }
      ]
    }
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHHB1PCFG0:00000005",
  "timestampUtc": "2026-06-24T00:53:05.2609997Z"
}
```
#### Flow chart

![[../Store-Image/Pasted image 20260625123542.png]]

#### Test case
**Test case 01: Lấy chi tiết sản phẩm thành công**
- **Điều kiện:** Request hợp lệ. NhanhVn API `/product/detail` trả về HTTP success, response khác null và `Code != 0`.
- **Kết quả:** Hệ thống trả về `NhanhVnProductDetailResponse`.
**Test case 02: Tạo đúng URL gọi NhanhVn**
- **Điều kiện:** Config có `BaseUrl`, `AppId`, `BusinessId`.
- **Kết quả:** Hệ thống tạo đúng URL:
```
/product/detail?appId={appId}&businessId={businessId}
```
**Test case 03: Gửi request có Authorization token**
- **Điều kiện:** Config có `Token`.
- **Kết quả:** HTTP request gửi đến NhanhVn có header `Authorization`.
**Test case 04: NhanhVn API trả về lỗi HTTP**
- **Điều kiện:** NhanhVn API trả về status code lỗi, ví dụ `400`, `401`, `500`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
NhanhVn API HTTP Error ({StatusCode})
```
**Test case 05: NhanhVn response null**
- **Điều kiện:** API trả về HTTP success nhưng deserialize ra `nhanhProductDetails = null`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
NhanhVnResponse returned null or empty data
```
**Test case 06: NhanhVn trả về Code = 0**
- **Điều kiện:** Response từ NhanhVn có `Code = 0`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
ERR_INVALID_FORM_FIELDS
```
**Test case 07: Response JSON có tên property khác kiểu chữ**
- **Điều kiện:** NhanhVn trả về JSON có property viết hoa/thường khác model, ví dụ `data`, `Data`, `DATA`.
- **Kết quả:** Hệ thống vẫn deserialize được vì có:
```
PropertyNameCaseInsensitive = true
```
**Test case 08: Response JSON có số được trả về dạng string**
- **Điều kiện:** NhanhVn trả về số dạng string, ví dụ `"Available": "10"`.
- **Kết quả:** Hệ thống vẫn parse được vì có:
```
NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
```
**Test case 09: Response JSON sai format**
- **Điều kiện:** NhanhVn trả về HTTP success nhưng body JSON sai format.
- **Kết quả:** Hệ thống có thể ném lỗi deserialize JSON.
**Test case 11: Product ID không tồn tại trên NhanhVn**
- **Điều kiện:** Request truyền product id không tồn tại.
- **Kết quả:** Tùy response từ NhanhVn:
    - Nếu `Code = 0`, hệ thống ném `BadRequestException("ERR_INVALID_FORM_FIELDS")`.
    - Nếu response hợp lệ nhưng không có data, hệ thống ném `BadRequestException("NhanhVnResponse returned null or empty data").`
