#### Request
```Json
Query param: taxCode (mã số thuế)
```
#### Response
```json
{
  "value": "CONG TY TNHH ABC\n123 Duong Test, Quan 1, HCM",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMHBET9MJ1B:00000005",
  "timestampUtc": "2026-06-23T19:18:33.2517535Z"
}
```
#### Flow chart
![[../Store-Image/GetInfoTaxCode.png]]

#### Test case
**Test case 01: Lấy thông tin mã số thuế thành công**
- **Điều kiện:** VietQR API trả về HTTP 200, `Code = "00"` và có `Data`.
- **Kết quả:** Hệ thống trả về chuỗi `{Name}\n{Address}` (ví dụ `CONG TY TNHH ABC\n123 Duong Test, Quan 1, HCM`).
**Test case 02: VietQR API trả về lỗi HTTP**
- **Điều kiện:** Gọi API `vietqr.io` trả về status không thành công.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `VietQR API HTTP Error`.
**Test case 03: VietQR response có data null**
- **Điều kiện:** API trả về HTTP 200 nhưng `Data = null`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `VietQR API returned null or empty data`.
**Test case 04: VietQR trả về Code khác "00"**
- **Điều kiện:** API trả về HTTP 200, có `Data` nhưng `Code != "00"`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `VietQR API Error: {Desc}`.
