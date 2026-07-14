#### Request
```json
{
  "addressLine": "123 Nguyễn Huệ",
  "ward": "Phường Bến Nghé",
  "city": "Hồ Chí Minh",
  "country": "Việt Nam",
  "latitude": 10.776389,
  "longitude": 106.701111,
  "mapId": "map_abc123"
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
  "traceId": "0HNMH0HLC67KU:00000003",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Service Flow (CreateCompanyAddress)

![[../Store-Image/CreateCompanyAddress.drawio.png]]
#### Test case
**Test case 01**: Tạo địa chỉ công ty thành công  
**Điều kiện:** Chưa có địa chỉ công ty nào active trong DB.  
**Kết quả:** Tạo mới thành công, trả về 201 Created kèm thông tin địa chỉ.

**Test case 02**: Đã tồn tại địa chỉ active  
**Điều kiện:** Đã có địa chỉ công ty với IsDeleted = false.  
**Kết quả:** Throw ConflictException "Chỉ được phép tồn tại một địa chỉ công ty trong cùng một khoảng thời gian".

**Test case 03**: Request không hợp lệ  
**Điều kiện:** Thiếu required fields (AddressLine, City, Country).  
**Kết quả:** Model binding fail, trả về 400 Bad Request.

**Test case 04**: Tất cả field đều để trống  
**Điều kiện:** Gửi JSON rỗng {} hoặc tất cả field null/empty.  
**Kết quả:** Model validation fail, trả về 400 Bad Request với danh sách lỗi validation.

**Test case 05**: Latitude/Longitude không hợp lệ  
**Điều kiện:** latitude > 90 hoặc < -90, longitude > 180 hoặc < -180.  
**Kết quả:** Model validation fail (nếu có validation), hoặc lưu giá trị không hợp lệ vào DB.

**Test case 06**: AddressLine quá dài  
**Điều kiện:** addressLine > 500 ký tự (hoặc max length của cột DB).  
**Kết quả:** Entity Framework throw DbUpdateException hoặc validation error.

**Test case 07**: Tạo địa chỉ trùng thông tin với địa chỉ cũ đã bị soft delete  
**Điều kiện:** Địa chỉ cũ (IsDeleted = true) có cùng thông tin, không còn địa chỉ active nào.  
**Kết quả:** Tạo thành công (không check duplicate nội dung).
