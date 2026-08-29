#### Request  
```CSharp  
// GET /api/v1/configs/public?groupName=web&key=title  
// Query params (tất cả đều optional):  
{  
  "groupName": "web",  "key": "title"
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
![[../Store-Image/GetConfigIsPublic.drawio.png]]

#### Test case

**Test case 01: Lọc hợp lệ trả về config Content public**
- **Điều kiện:** Group `web` có config `Content` public và config `Content` riêng tư. Gọi với `groupName = "web"`, `key = "title"`.
- **Kết quả:** Hệ thống trả về đúng một config public `web.title`.

**Test case 02: Không có config nào khớp**
- **Điều kiện:** Chỉ có config `web.title`, gọi với `groupName = "nonexistent"`.
- **Kết quả:** Hệ thống trả về `null`.

**Test case 03: Loại bỏ Kind Secret và Setting**
- **Điều kiện:** Database chỉ có config `Setting` và `Secret` (đều public).
- **Kết quả:** Hệ thống trả về `null` (chỉ nhận `Content`).

**Test case 04: Loại bỏ config Content riêng tư**
- **Điều kiện:** Database chỉ có config `Content` với `IsPublic = false`.
- **Kết quả:** Hệ thống trả về `null`.
