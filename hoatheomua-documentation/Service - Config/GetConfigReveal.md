#### Request  
```CSharp  
// POST /api/v1/configs/secrets/reveal/id  
// Body là Id (Guid) của config Secret:  
"a6a27c96-6735-4da0-b809-020c68f8260f"  
```  
#### Response  
```CSharp  
{
  "value": {
    "id": "a6a27c96-6735-4da0-b809-020c68f8260f",
    "key": "api.secret",
    "value": "sk_live_1234567890",
    "revealedAt": "2026-06-23T08:51:30.37969Z",
    "expiresAt": "2026-06-23T08:56:30.37969Z"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
}
```

#### Flow diagram
![[../Store-Image/GetConfigReveal.drawio.png]]

#### Test case

**Test case 01: Tiết lộ secret hợp lệ**
- **Điều kiện:** Config `Kind = Secret`, `IsDeleted = false`, `Id` hợp lệ.
- **Kết quả:** Hệ thống trả về `Value` nguyên bản (không bị che) và `ExpiresAt > RevealedAt`.

**Test case 02: Không tìm thấy config**
- **Điều kiện:** Không có config nào có `Id` được truyền vào.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Config not found or not a secret`.

**Test case 03: Config đã bị xóa mềm**
- **Điều kiện:** Config `Secret` tồn tại nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Config not found or not a secret`.

**Test case 04: Config không phải Kind Secret**
- **Điều kiện:** Config tồn tại nhưng `Kind = Content`.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Config not found or not a secret`.
