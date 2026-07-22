  
#### Request  
```CSharp  
// GET /api/v1/configs/settings?groupName=setting&key=theme  
// Query params (tất cả đều optional):  
{  
  "groupName": "setting",  "key": "theme"  
}  
```  
#### Response  
```CSharp  
{
  "value": [
    {
      "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
      "key": "setting.theme",
      "group": "setting",
      "isPublic": false,
      "value": "dark",
      "kind": "Setting",
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
![[../Store-Image/GetConfigSetting.drawio.png]]
#### Test case

**Test case 01: Lọc hợp lệ trả về config Setting**
- **Điều kiện:** Database có config `Setting` (`setting.theme`) và config `Content`. Gọi với `groupName = "setting"`, `key = "theme"`.
- **Kết quả:** Hệ thống trả về đúng một config `setting.theme`.

**Test case 02: Lọc theo group không khớp**
- **Điều kiện:** Chỉ có config thuộc group `setting`, gọi với `groupName = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 03: Lọc theo key không khớp**
- **Điều kiện:** Chỉ có config `setting.theme`, gọi với `key = "nonexistent"`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.

**Test case 04: Loại bỏ config Setting đã bị xóa mềm**
- **Điều kiện:** Config `Setting` tồn tại nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống trả về danh sách rỗng.
