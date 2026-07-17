#### Request
#### Response
```Json
{
  "value": {
    "firstName": "Hung",
    "lastName": "Phan",
    "email": "xesoyo3021@disiok.com",
    "avatar": null,
    "isVerify": true,
    "userId": "d8c166e9-18b1-4a28-beff-d653949df566",
    "role": "User"
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:0000000B",
  "timestampUtc": "2026-06-23T17:53:45.1526295Z"
}
```
#### Flow chart


![[../Store-Image/GetMe.drawio.png]]


#### Test case
**Test case 01: Lấy thông tin user thành công**

- **Điều kiện:** Access token có claim `UserId` hợp lệ và user tồn tại.
- **Kết quả:** Hệ thống trả về `GetMeResponse` gồm:
    - `Email`
    - `FirstName`
    - `LastName`
    - `Avatar`
    - `IsVerify`
    - `Role`
    - `UserId`

**Test case 02: Không có access token và không có refresh token**
- **Điều kiện:** Không có claim `UserId`, cookie `HTM-Refresh-Token` cũng không có.
- **Kết quả:** Hệ thống trả về `null`.

**Test case 03: Không có access token nhưng có refresh token**
- **Điều kiện:** Không có claim `UserId`, nhưng có cookie `HTM-Refresh-Token`.
- **Kết quả:** Hệ thống ném lỗi `MissingAccessTokenException`.

**Test case 04: UserId trong access token sai định dạng Guid**
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống lỗi khi gọi `Guid.Parse`.

**Test case 05: User không tồn tại**
- **Điều kiện:** Claim `UserId` hợp lệ nhưng database không có user.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `User not found`.

**Test case 06: User tồn tại nhưng bị xóa mềm**
- **Điều kiện:** User có `IsDeleted = true`.
- **Kết quả:** Code hiện tại vẫn có thể trả về user vì query chưa filter `!IsDeleted`. Nên bổ sung điều kiện nếu không muốn trả user đã xóa.