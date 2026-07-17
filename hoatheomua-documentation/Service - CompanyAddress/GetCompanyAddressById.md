#### Request
```json
GET /api/v1/company-addresses/33333333-3333-3333-3333-333333333333
```

#### Response
```json
{
  "value": {
    "id": "33333333-3333-3333-3333-333333333333",
    "addressLine": "123 Nguyễn Huệ",
    "ward": "Phường Bến Nghé",
    "city": "Hồ Chí Minh",
    "country": "Việt Nam",
    "latitude": 10.776389,
    "longitude": 106.701111,
    "mapId": "map_abc123",
    "createdAt": "2026-06-23T08:51:30.37969Z",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000002",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Service Flow (GetCompanyAddressById)
![[../Store-Image/GetCompanyAddressById.drawio.png]]

#### Test case
**Test case 01**: Lấy địa chỉ theo ID thành công  
**Điều kiện:** ID tồn tại trong DB, IsDeleted = false.  
**Kết quả:** Trả về thông tin địa chỉ công ty.

**Test case 03**: ID không tồn tại  
**Điều kiện:** ID không có trong DB hoặc đã bị soft delete.  
**Kết quả:** Trả về null, message "Không tìm thấy địa chỉ công ty."

**Test case 04**: Địa chỉ tồn tại nhưng thiếu các trường nullable (ward, mapId, lat/lng)  
**Điều kiện:** Địa chỉ không có ward, mapId, latitude, longitude.  
**Kết quả:** Trả về địa chỉ với các field null, không gây lỗi.
