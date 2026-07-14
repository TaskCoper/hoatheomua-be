#### Request
```json
PUT /api/v1/company-addresses/33333333-3333-3333-3333-333333333333

{
  "addressLine": "456 Lê Lợi",
  "ward": "Phường Bến Thành",
  "city": "Hồ Chí Minh",
  "country": "Việt Nam",
  "latitude": 10.772345,
  "longitude": 106.698765,
  "mapId": "map_def456"
}
```

#### Response
```json
{
  "value": "Cập nhật địa chỉ công ty thành công.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000004",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Service Flow (UpdateCompanyAddress)
![[../Store-Image/UpDateCompanyAddress.drawio.png]]

#### Test case
**Test case 01**: Cập nhật địa chỉ thành công  
**Điều kiện:** ID tồn tại, quyền Admin.  
**Kết quả:** Cập nhật thông tin, trả về message thành công.

**Test case 03**: Không tìm thấy địa chỉ  
**Điều kiện:** ID không tồn tại trong DB.  
**Kết quả:** Throw NotFoundException.

**Test case 04**: Không có quyền Admin  
**Điều kiện:** User không có role Admin.  
**Kết quả:** Trả về 403 Forbidden.

**Test case 05**: Cập nhật với thông tin giống hệt cũ  
**Điều kiện:** Tất cả field trong body giống với giá trị hiện tại trong DB.  
**Kết quả:** Cập nhật thành công (idempotent), UpdatedAt được cập nhật.

**Test case 06**: Cập nhật với dữ liệu trống  
**Điều kiện:** Gửi JSON rỗng {} hoặc tất cả field null.  
**Kết quả:** Model validation fail, trả về 400 Bad Request.

**Test case 07**: Địa chỉ đã bị soft delete  
**Điều kiện:** ID thuộc địa chỉ có IsDeleted = true.  
**Kết quả:** Ném NotFoundException "Không tìm thấy địa chỉ công ty".

**Test case 08**: Cập nhật latitude/longitude không hợp lệ  
**Điều kiện:** latitude > 90 hoặc < -90, longitude > 180 hoặc < -180.  
**Kết quả:** Model validation fail hoặc lưu giá trị không hợp lệ vào DB.
