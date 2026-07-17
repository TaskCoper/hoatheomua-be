#### Request  
```CSharp  
// GET /api/v1/configs/group?groupName=web  
{  
  "groupName": "web"
}  
```  
#### Response  
```CSharp  
{
  "value": [
    {
      "key": "web.title",
      "kind": "Content",
      "value": "Hoa Theo Mùa"
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
![[../Store-Image/GetConfigGroups.drawio.png]]

#### Test case

**Test case 01: Group hợp lệ trả về các config public trong group**
- **Điều kiện:** Group `web` có một config public và một config riêng tư. Gọi với `groupName = "web"`.
- **Kết quả:** Hệ thống chỉ trả về config public `web.title`.

**Test case 02: Group không tồn tại**
- **Điều kiện:** Chỉ có config thuộc group `web`, gọi với `groupName = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 03: Loại bỏ config riêng tư**
- **Điều kiện:** Group `web` chỉ có config `IsPublic = false`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 04: Loại bỏ config đã bị xóa mềm**
- **Điều kiện:** Group `web` chỉ có config public nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.
