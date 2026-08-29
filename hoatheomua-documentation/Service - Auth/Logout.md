#### Request
#### Response
```json
{
  "value": "LOGOUT_SUCCESSFUL",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:0000000C",
  "timestampUtc": "2026-06-23T17:54:12.0318642Z"
}
```
#### Flow chart
![[../Store-Image/Logout.drawio.png]]
#### Test case
**Test case 01: Logout thành công**
- **Điều kiện:** Cookie `HTM-Refresh-Token` tồn tại, token có trong database, `IsActive = true`, `RevokedAt = null`.
- **Kết quả:** Hệ thống revoke token, set `IsActive = false`, lưu database và trả về message `LOGOUT_SUCCESSFUL`.
**Test case 02: Không có refresh token trong cookie**
- **Điều kiện:** Request không có cookie `HTM-Refresh-Token`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Refresh token is invalid or already revoked`.
**Test case 03: Refresh token không tồn tại trong database**
- **Điều kiện:** Cookie có token nhưng database không có token tương ứng.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `Refresh token is invalid`.
**Test case 04: Refresh token đã bị revoke**
- **Điều kiện:** Token có `RevokedAt != null`.
- **Kết quả:** Query không tìm được token hợp lệ, hệ thống ném lỗi `UnauthorizedException`.
**Test case 05: Refresh token không active**
- **Điều kiện:** Token có `IsActive = false`.
- **Kết quả:** Query không tìm được token hợp lệ, hệ thống ném lỗi `UnauthorizedException`.
**Test case 06: Lỗi khi SaveChanges** -> rollback
- **Điều kiện:** Database lỗi khi cập nhật refresh token.
- **Kết quả:** Exception được throw ra ngoài.