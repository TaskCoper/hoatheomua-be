#### Request
```CSharp

"id": "853f1665-996e-42bd-92a4-0275143346eb"

{
  "receiverName": "quanquan",
  "phoneNumber": "0981234567",
  "addressLine": "12345 Le Van Viet",
  "ward": "Long Binh",
  "city": "Ho Chi Minh",
  "country": "Viet Nam",
  "mapId": "ChIJt4qIbx4bdTERB1AQwW0l01g",
  "latitude": 77,
  "longitude": 97,
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
  "traceId": "0HNMLOJUN580J:00000002",
  "timestampUtc": "2026-06-29T10:14:58.990733Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260629165305.png]]

#### Test case

**Test case 01: Cập nhật address thành công**  
**Điều kiện:** Address tồn tại, thuộc user hiện tại.  
**Kết quả**: Cập nhật thông tin và trả về `"Cập nhật thành công"`.

**Test case 02: Address không tồn tại khi update**  
**Điều kiện:** Không tìm thấy address theo id.  
**Kết quả:** Ném `NotFoundException` với message `Address not found`.

**Test case 03: Update address thuộc user khác**  
**Điều kiện:** Address tồn tại nhưng không thuộc user hiện tại.  
**Kết quả:** Ném `NotFoundException`.

**Test case 04: Update address đã bị xóa**  
**Điều kiện:** Address có `IsDeleted = true`.  
**Kết quả:** Ném `NotFoundException`.

**Test case 05: Update address thành default**  
**Điều kiện:** Request `IsDefault = true`, address hiện tại chưa default.  
**Kết quả:** address này thành default.

**Test case 06: Update address đã là default**  
**Điều kiện:** Address hiện tại đã `IsDefault = true`.  
**Kết quả:** Không cần clear default, chỉ update thông tin.