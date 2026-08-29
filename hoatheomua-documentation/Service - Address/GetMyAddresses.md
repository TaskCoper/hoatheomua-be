#### Request
```CSharp

UserId lấy trong AccessToken

```
#### Response
```CSharp

{
  "value": [
    {
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
    {
      "id": "63141f8c-3c91-496d-afbd-8f3a872cb178",
      "userId": "a87c7b64-097b-4cda-a2a2-71dbc08ec8bb",
      "receiverName": "string",
      "phoneNumber": "string",
      "addressLine": "string",
      "ward": "string",
      "city": "string",
      "country": "string",
      "mapId": "string",
      "latitude": 0,
      "longitude": 0,
      "isDefault": false,
      "createdAt": "2026-06-29T09:38:51.894295+00:00",
      "updatedAt": null
    }
  ],
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580G:00000004",
  "timestampUtc": "2026-06-29T09:59:58.988133Z"
}

```

#### Flow diagram

![[../Store-Image/Pasted image 20260629165106.png|697]]

#### Test case

**Test case 01: Lấy danh sách address thành công**  
**Điều kiện:** User đã đăng nhập, có nhiều address `IsDeleted = false`.  
**Kết quả:** Trả về `List<AddressResponse>`.

**Test case 02: Sắp xếp address đúng**  
**Điều kiện:** User có address default và address thường.  
**Kết quả:** Address `IsDefault = true` nằm trước, sau đó sắp xếp theo `CreatedAt` mới nhất.

**Test case 03: User chưa có address**  
**Điều kiện:** User đã đăng nhập nhưng chưa có address.  
**Kết quả:** Trả về list rỗng.

**Test case 04: Không có userId trong token**  
**Điều kiện:** `_currentUser.GetRequiredUserId()` không lấy được user.  
**Kết quả:** Hệ thống ném lỗi unauthorized.