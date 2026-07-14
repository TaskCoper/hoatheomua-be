#### Request  
```CSharp  
// GET /api/v1/configs/content?isPublic=true&groupName=web&key=title  
// Query params (tất cả đều optional):  
{  
  "isPublic": true,  "groupName": "web",  "key": "title"
}  
```  
#### Response  
```CSharp  
{
  "value": [
    {
      "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
      "key": "web.title",
      "group": "web",
      "isPublic": true,
      "value": "Hoa Theo Mùa",
      "kind": "Content",
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
![[../Store-Image/GetConfigContent.drawio.png]]
#### Test case

**Test case 01: Lọc hợp lệ theo isPublic, group và key**
- **Điều kiện:** Database có config `Content` public thuộc group `web` với key chứa `title`, cùng vài config khác không thỏa. Gọi với `isPublic = true`, `groupName = "web"`, `key = "title"`.
- **Kết quả:** Hệ thống trả về đúng một config `web.title`.

**Test case 02: Lọc theo group không khớp**
- **Điều kiện:** Chỉ có config thuộc group `web`, gọi với `groupName = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 03: Lọc theo key không khớp**
- **Điều kiện:** Chỉ có config `web.title`, gọi với `key = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 04: Lọc isPublic = false chỉ lấy config Content riêng tư**
- **Điều kiện:** Database có config `Content` public và config `Content` riêng tư. Gọi với `isPublic = false`.
- **Kết quả:** Hệ thống chỉ trả về config `Content` có `IsPublic = false`.
