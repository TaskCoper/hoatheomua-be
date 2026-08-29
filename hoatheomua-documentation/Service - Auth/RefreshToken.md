#### Request
#### Response
```Json
{
  "value": "Refresh token thành công",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:0000000A",
  "timestampUtc": "2026-06-23T17:53:05.7708086Z"
}
```
#### Flow chart
![[../Store-Image/RefreshToken.drawio.png]]
#### Test case
**Test case 01: Refresh token thành công**
- **Điều kiện:** Cookie `HTM-Refresh-Token` tồn tại, refresh token có trong database, chưa revoke, chưa hết hạn.
- **Kết quả:** Hệ thống revoke refresh token cũ, tạo refresh token mới, tạo access token mới và trả về `AuthResponse`.
**Test case 02: Không có refresh token trong cookie**
- **Điều kiện:** Request không có cookie `HTM-Refresh-Token`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Refresh token is invalid or already revoked`.
**Test case 03: Refresh token không tồn tại trong database**
- **Điều kiện:** Cookie có refresh token nhưng database không có token tương ứng.
- **Kết quả:** Hệ thống xóa auth cookies và ném lỗi `ExpiredRefreshTokenException`.
**Test case 04: Refresh token đã bị revoke**
- **Điều kiện:** Refresh token có `RevokedAt != null`.
- **Kết quả:** Hệ thống xóa auth cookies và ném lỗi `ExpiredRefreshTokenException`.
**Test case 05: Refresh token đã hết hạn**
- **Điều kiện:** Refresh token có `ExpiredAt <= DateTimeOffset.UtcNow`.
- **Kết quả:** Hệ thống xóa auth cookies và ném lỗi `ExpiredRefreshTokenException`.
**Test case 06: Refresh token cũ được revoke sau khi refresh thành công**
- **Điều kiện:** Refresh token hợp lệ.
- **Kết quả:** Token cũ được cập nhật:
    - `RevokedAt = DateTimeOffset.UtcNow`
    - `IsActive = false`
**Test case 07: Tạo refresh token mới thành công**
- **Điều kiện:** Refresh token cũ hợp lệ.
- **Kết quả:** Hệ thống tạo refresh token mới với:
    - `ExpiredAt = UtcNow + 7 days`
    - `IsActive = true`
    - `UserId = storedToken.User.Id`
    - `IpAddress`
    - `UserAgent`
    - `DeviceLabel`
**Test case 08: Không có access token trong claim**
- **Điều kiện:** `CheckAccessToken()` trả về `Guid.Empty`.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `USER_NOT_FOUND`.
**Test case 09: UserId trong access token sai định dạng Guid**
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống lỗi khi gọi `Guid.Parse`.
**Test case 10: Tạo access token mới thành công**
- **Điều kiện:** Refresh token hợp lệ và access token có UserId hợp lệ.
- **Kết quả:** Hệ thống tạo access token mới với claim:
    - `UserId`
    - `Role`
    - `IsVerify`