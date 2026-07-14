#### Request
```Json
{
  "token": "string"
}
```
#### Response
```CSharp
{
  "value": "Xác thực tài khoản thành công! Chào mừng bạn đến với Hoa Theo Mùa.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:00000003",
  "timestampUtc": "2026-06-23T17:48:30.0046546Z"
}
```
#### Flow chart
![[../Store-Image/VerifyEmail.drawio.png]]
#### Test case
**Test case 01: Verify email thành công**
- **Điều kiện:** Token hợp lệ, user tồn tại và chưa verify.
- **Kết quả:** Hệ thống cập nhật `IsVerify = true`, tạo access token, refresh token, lưu refresh token vào database và trả về message xác thực thành công.
**Test case 02: Token hết hạn hoặc không hợp lệ**
- **Điều kiện:** `_jwtService.ValidateToken(request.Token)` trả về null.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Liên kết xác thực đã hết hạn hoặc không hợp lệ.`.
**Test case 03: Token không có claim UserId**
- **Điều kiện:** Token hợp lệ nhưng không có claim `UserId`.
- **Kết quả:** Code hiện tại có thể lỗi khi gọi `Guid.Parse(userIdStr!)`.
**Test case 04: UserId trong token sai định dạng Guid**
- **Điều kiện:** Claim `UserId` không phải Guid.
- **Kết quả:** Hệ thống lỗi khi gọi `Guid.Parse`.
**Test case 05: User không tồn tại**
- **Điều kiện:** Token hợp lệ nhưng database không có user tương ứng.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Người dùng không tồn tại trên hệ thống.`.
**Test case 06: User đã verify trước đó**
- **Điều kiện:** User tồn tại và `IsVerify = true`.
- **Kết quả:** Hệ thống trả về response với:
    - `AccessToken = null`
    - `RefreshToken = null`
    - `Message = Người dùng đã xác thực trước đó rồi.`

**Test case 07: Tạo refresh token và accesstoken sau khi verify thành công**
- **Điều kiện:** User verify thành công.
- **Kết quả:** Hệ thống lưu refresh token vào database và tạo mới accesstoken
**Test case 08: Lỗi khi SaveChanges**
- **Điều kiện:** Database lỗi khi lưu user hoặc refresh token.
- **Kết quả:** Transaction rollback.
