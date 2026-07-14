#### Request
```CSharp

"id": "63141f8c-3c91-496d-afbd-8f3a872cb178"

```
#### Response
```CSharp
{
  "value": "Đặt địa chỉ mặc định thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580L:00000003",
  "timestampUtc": "2026-06-29T10:21:00.174392Z"
}
```

#### Flow diagram


![[../Store-Image/Pasted image 20260629165412.png]]

#### Test case

**Test case 01: Set default thành công**  
**Điều kiện:** Address tồn tại, chưa default.  
**Kết quả:** Clear default cũ, set address này `IsDefault = true`, trả về `"Đặt địa chỉ mặc định thành công"`.

**Test case 02: Address không tồn tại khi set default**  
**Điều kiện:** Không tìm thấy address.  
**Kết quả:** Ném `NotFoundException` với message `Address not found`.

**Test case 03: Address đã là default**  
**Điều kiện:** Address hiện tại đã `IsDefault = true`.  
**Kết quả:** Không cần update DB, vẫn trả về thành công.

**Test case 04: Set default address thuộc user khác**  
**Điều kiện:** Address tồn tại nhưng không thuộc user hiện tại.  
**Kết quả:** Ném `NotFoundException`.