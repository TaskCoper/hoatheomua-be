#### Request
```CSharp

"id": "853f1665-996e-42bd-92a4-0275143346eb"

```
#### Response
```CSharp

{
  "value": "Xóa thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580K:00000001",
  "timestampUtc": "2026-06-29T10:17:34.21082Z"
}

```

#### Flow diagram


![[../Store-Image/Pasted image 20260629165339.png]]

#### Test case

**Test case 01: Xóa address thành công**  
**Điều kiện:** Address tồn tại, thuộc user hiện tại.  
**Kết quả:** `IsDeleted = true`, cập nhật `UpdatedAt`, trả về `"Xóa thành công"`.

**Test case 02: Xóa address không tồn tại**  
**Điều kiện:** Không tìm thấy address.  
**Kết quả:** Ném `NotFoundException` với message `Address not found`.

**Test case 03: Xóa address thuộc user khác**  
**Điều kiện:** Address tồn tại nhưng không thuộc user hiện tại.  
**Kết quả:** Ném `NotFoundException`.

**Test case 04: Xóa address thường**  
**Điều kiện:** Address cần xóa có `IsDefault = false`.  
**Kết quả:** Chỉ soft delete, không đổi default của address khác.

**Test case 05: Xóa address default và còn address khác**  
**Điều kiện:** Address bị xóa là default, user còn address khác.  
**Kết quả:** Address mới nhất còn lại được đặt `IsDefault = true`.

**Test case 06: Xóa address default nhưng không còn address khác**  
**Điều kiện:** Address bị xóa là address cuối cùng.  
**Kết quả:** Không có address nào được set default mới.