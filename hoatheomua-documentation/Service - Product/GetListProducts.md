#### Request
```Json
{
  "filters": {
    "ids": [
      0
    ],
    "status": [
      0
    ],
    "name": "string",
    "categoryIds": [
      0
    ],
    "price": [
      0
    ]
  },
  "paginator": {
    "size": 0,
    "sort": {
      "id": "string"
    },
    "next": {
      "id": 0
    }
  }
}
```
#### Response
```json
{
  "value": {
    "code": 1,
    "paginator": {
      "next": null
    },
    "data": [
      {
        "id": 4934210,
        "parentId": -2,
        "code": "HB_CRP_2026",
        "barcode": "2000214275339",
        "name": "Bó Coral Pink_2026",
        "status": 1,
        "priceVatMode": 2,
        "importPriceVatMode": 2,
        "categoryId": 19810,
        "internalCategoryId": 0,
        ...
	]
	  },
	  "isSuccess": true,
	  "isFailed": false,
	  "error": null,
	  "traceId": "0HNMHHB1PCFG2:00000001",
	  "timestampUtc": "2026-06-24T00:55:13.9253375Z"
}
```
#### Flow chart


![[../Store-Image/Pasted image 20260625123528.png]]

#### Test case
**Test case 01: Lấy danh sách sản phẩm thành công**
- **Điều kiện:** Request hợp lệ. NhanhVn API `/product/list` trả về HTTP success, response khác null và `Code != 0`.
- **Kết quả:** Hệ thống trả về `NhanhVnProductResponse`.
**Test case 02: Gửi request có field null**
- **Điều kiện:** Request có một số field null, ví dụ `Filters = null` hoặc `Paginator.Next = null`.
- **Kết quả:** Hệ thống serialize request với `DefaultIgnoreCondition = WhenWritingNull`, các field null không được gửi lên NhanhVn.
**Test case 03: Tạo đúng URL gọi NhanhVn**
- **Điều kiện:** Config có `BaseUrl`, `AppId`, `BusinessId`.
- **Kết quả:** Hệ thống tạo đúng URL:
```
/product/list?appId={appId}&businessId={businessId}
```
**Test case 04: Gửi request có Authorization token**
- **Điều kiện:** Config có `Token`.
- **Kết quả:** HTTP request gửi đến NhanhVn có header `Authorization`.
**Test case 05: NhanhVn API trả về lỗi HTTP**
- **Điều kiện:** NhanhVn API trả về status code lỗi, ví dụ `400`, `401`, `500`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
NhanhVn API HTTP Error ({StatusCode})
```
**Test case 06: NhanhVn response null**
- **Điều kiện:** API trả về HTTP success nhưng deserialize ra `nhanhProductList = null`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
NhanhVnResponse returned null or empty data
```
**Test case 07: NhanhVn trả về Code = 0**
- **Điều kiện:** Response từ NhanhVn có `Code = 0`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message:
```
ERR_INVALID_FORM_FIELDS
```
**Test case 08: NhanhVn trả về Data rỗng nhưng Code khác 0**
- **Điều kiện:** Response có `Code != 0` nhưng danh sách sản phẩm rỗng.
- **Kết quả:** Theo code hiện tại, hệ thống vẫn trả về `nhanhProductList`, vì chưa có kiểm tra `Data` rỗng. -> còn thiếu
**Test case 09: Response JSON sai format**
- **Điều kiện:** NhanhVn trả về HTTP success nhưng body JSON không đúng format với `NhanhVnProductResponse`.
- **Kết quả:** Hệ thống có thể ném lỗi deserialize JSON.
**Test case 12: Log raw request và raw response**
- **Điều kiện:** Gọi API thành công.
- **Kết quả:** Hệ thống in log debug request và response bằng `Console.WriteLine`.