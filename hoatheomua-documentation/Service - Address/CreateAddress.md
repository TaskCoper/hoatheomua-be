#### Request
```CSharp

  {
  "receiverName": "quanquan",
  "phoneNumber": "0981234567",
  "addressLine": "123 Le Van Viet",
  "ward": "Long Thanh My",
  "city": "Ho Chi Minh",
  "country": "Viet Nam",
  "mapId": "ChIJt4qIbx4bdTERB1AQwW0l01g",
  "latitude": 72,
  "longitude": 27,
  "isDefault": true
}

```
#### Response
```CSharp
{
  "value": "Cập nhật thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLO89J9RIO:00000001",
  "timestampUtc": "2026-06-29T09:41:20.632014Z"
}
```

#### Flow diagram

![[../Store-Image/Pasted image 20260629164846.png|697]]

#### Test case

**Test case 01: Tạo address thành công**  
**Điều kiện:** User đã đăng nhập, request hợp lệ.  
**Kết quả:** Tạo address mới và trả về `"Cập nhật thành công"`.

**Test case 02: Address đầu tiên tự động làm default**  
**Điều kiện:** User chưa có address nào.  
**Kết quả:** Address mới có `IsDefault = true`.

**Test case 03: Tạo address với IsDefault = true**  
**Điều kiện:** User đã có address khác, request `IsDefault = true`.  
**Kết quả:**  address mới thành default.

**Test case 04: Tạo address không default**  
**Điều kiện:** User đã có address, request `IsDefault = false`.  
**Kết quả:** Address mới có `IsDefault = false`.

**Test case 05: Trim dữ liệu request**  
**Điều kiện:** ReceiverName, PhoneNumber, AddressLine có khoảng trắng đầu/cuối.  
**Kết quả:** Dữ liệu được lưu sau khi `Trim()`.
