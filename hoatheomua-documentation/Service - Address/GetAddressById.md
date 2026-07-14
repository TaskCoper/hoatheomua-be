#### Request
```CSharp
  
"id": "853f1665-996e-42bd-92a4-0275143346eb"

```
#### Response
```CSharp

{
  "value": {
    "id": "853f1665-996e-42bd-92a4-0275143346eb",
    "userId": "a87c7b64-097b-4cda-a2a2-71dbc08ec8bb",
    "receiverName": "quanquan",
    "phoneNumber": "0981234567",
    "addressLine": "123 Le Van Viet",
    "ward": "Long Thanh My",
    "city": "Ho Chi Minh",
    "country": "Viet Nam",
    "mapId": "ChIJt4qIbx4bdTERB1AQwW0l01g",
    "latitude": 72,
    "longitude": 27,
    "isDefault": true,
    "createdAt": "2026-06-29T09:41:20.598966+00:00",
    "updatedAt": null
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580I:00000001",
  "timestampUtc": "2026-06-29T10:09:57.431945Z"
}

```

#### Flow diagram

![[../Store-Image/Pasted image 20260629165240.png]]

#### Test case

**Test case 01: Lấy address theo id thành công**  
**Điều kiện:** Address tồn tại, thuộc user hiện tại, `IsDeleted = false`.  
**Kết quả:** Trả về `AddressResponse`.

**Test case 02: Address không tồn tại**  
**Điều kiện:** Không tìm thấy address theo id.  
**Kết quả:** Ném `NotFoundException` với message `Address not found`.

**Test case 03: Address thuộc user khác**  
**Điều kiện:** Address tồn tại nhưng `UserId` khác user hiện tại.  
**Kết quả:** Ném `NotFoundException`.

**Test case 04: Address đã bị xóa**  
**Điều kiện:** Address có `IsDeleted = true`.  
**Kết quả:** Ném `NotFoundException`.