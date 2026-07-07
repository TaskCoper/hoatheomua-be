#### Request
```json
{
	"id" = "..."
}
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
  "traceId": "0HNMH0HLC67KU:00000001",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Service Flow (GetCompanyAddress)
![[../Store-Image/GetCompanyAddress.drawio.png]]
#### Test case
**Test case 01**: Lấy địa chỉ công ty thành công  
**Điều kiện:** Có ít nhất một địa chỉ trong DB, IsDeleted = false.  
**Kết quả:** Trả về địa chỉ mới nhất (theo CreatedAt) kèm thông tin chi tiết.

**Test case 02**: Không có địa chỉ nào  
**Điều kiện:** DB rỗng hoặc tất cả đều IsDeleted = true.  
**Kết quả:** Trả về null, message "Không tìm thấy địa chỉ công ty."

**Test case 04**: Địa chỉ hợp lệ nhưng thiếu các trường nullable (ward, mapId, latitude, longitude)  
**Điều kiện:** Địa chỉ không có ward, mapId, latitude, longitude (các field này = null).  
**Kết quả:** Trả về địa chỉ với các field null, không gây lỗi.
