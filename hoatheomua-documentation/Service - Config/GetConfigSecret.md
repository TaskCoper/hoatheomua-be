#### Request  
```CSharp  
// GET /api/v1/configs/secrets?groupName=api&key=secret  
// Query params (tất cả đều optional):  
{
  "groupName": "api",
  "key": "secret"
}
```  
#### Response  
```CSharp  
{
  "value": [
    {
      "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
      "key": "api.secret",
      "group": "api",
      "isPublic": false,
      "value": "••••",
      "kind": "Secret",
      "userId": "528b7881-e6bb-4c83-ad31-bee1a8551525"
    }
  ],
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```
#### Flow diagram
![[../Store-Image/GetConfigSecret.drawio.png]]

#### Test case

**Test case 01: Lọc hợp lệ trả về config Secret đã che**
- **Điều kiện:** Database có config `Secret` (`api.secret`) và config `Content`. Gọi với `groupName = "api"`, `key = "secret"`.
- **Kết quả:** Hệ thống trả về đúng một config `api.secret` với trường `value` bị che (`••••`).

**Test case 02: Lọc theo group không khớp**
- **Điều kiện:** Chỉ có config thuộc group `api`, gọi với `groupName = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 03: Lọc theo key không khớp**
- **Điều kiện:** Chỉ có config `api.secret`, gọi với `key = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 04: Loại bỏ config Secret đã bị xóa mềm**
- **Điều kiện:** Config `Secret` tồn tại nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.
