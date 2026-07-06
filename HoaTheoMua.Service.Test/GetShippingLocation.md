#### Request
```Json
{
  "locationVersion": "string",
  "type": "string",
  "parentId": 0
}
```
#### Response
```Json
{
  "value": {
    "code": 1,
    "data": [
      {
        "id": 254,
        "name": "Hà Nội"
      },
      .....
      {
        "id": 307,
        "name": "Vĩnh Long"
      },
      {
        "id": 310,
        "name": "Đắk Lắk"
      },
      {
        "id": 311,
        "name": "Đồng Tháp"
      },
      {
        "id": 316,
        "name": "Điện Biên"
      }
    ]
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:0000000F",
  "timestampUtc": "2026-06-23T19:24:21.9967589Z"
}
```
#### Flow chart


![[Pasted image 20260625113532.png]]

#### Test case
**Test case 01: Lấy danh sách địa điểm giao hàng thành công**
- **Điều kiện:** NhanhVn API `/shipping/location` trả về response hợp lệ, `Code != 0`.
- **Kết quả:** Hệ thống trả về `NhanhVnLocationResponse`.
**Test case 02: NhanhVn API trả về lỗi HTTP**
- **Điều kiện:** Gọi API `/shipping/location` thất bại.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVn API HTTP Error`.
**Test case 03: NhanhVn response null**
- **Điều kiện:** API trả về response null.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVnResponse returned null or empty data`.
**Test case 04: NhanhVn trả về Code = 0**
- **Điều kiện:** Response có `Code = 0`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `ERR_INVALID_FORM_FIELDS`.
**Test case 05: Gửi filter LocationVersion, Type, ParentId hợp lệ**
- **Điều kiện:** Request có `LocationVersion`, `Type`, `ParentId`.
- **Kết quả:** Hệ thống tạo `LocationRequest` đúng với các filter được truyền vào.
