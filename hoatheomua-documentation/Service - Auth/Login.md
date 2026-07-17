#### Request
```Json
{
  "email": "string",
  "password": "string"
}
```
#### Response
```Json
{
  "value": "Login successful!",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:00000004",
  "timestampUtc": "2026-06-23T17:49:50.9548363Z"
}
```
#### Flow chart



![[../Store-Image/Login.drawio.png]]



#### Test case

**Test case 01: Login thành công**
- **Điều kiện:** Email tồn tại, user chưa bị xóa, password đúng.
- **Kết quả:** Hệ thống tạo access token, refresh token, lưu refresh token vào database và trả về `LoginResponse`.

**Test case 02: Email không tồn tại**
- **Điều kiện:** Không có user nào có email trong request.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `Email hoặc mật khẩu không đúng`.

**Test case 03: User đã bị xóa mềm**
- **Điều kiện:** User có email đúng nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống không cho login và ném lỗi `UnauthorizedException`.

**Test case 04: Sai password**
- **Điều kiện:** Email đúng nhưng password sai.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `Email hoặc mật khẩu không đúng`.

**Test case 05: Email có chữ hoa và khoảng trắng**
- **Điều kiện:** Request gửi email dạng `" Test@Gmail.Com "`.
- **Kết quả:** Hệ thống trim và chuyển email thành lowercase trước khi kiểm tra.

**Test case 06: Verify password có pepper**
- **Điều kiện:** User nhập password.
- **Kết quả:** Hệ thống cộng password với `_securityOptions.Pepper` trước khi gọi BCrypt verify.

**Test case 07: Lưu refresh token sau khi login thành công**
- **Điều kiện:** Login thành công.
- **Kết quả:** Hệ thống lưu refresh token với:
    - `RefreshTokenHash`
    - `IpAddress`
    - `UserAgent`
    - `IsActive = true`
    - `ExpiredAt = UtcNow + 7 days`
    - `UserId`
    - `CreatedAt`
    - `IsDeleted = false`

**Test case 08: User chưa verify vẫn login được theo code hiện tại**
- **Điều kiện:** User có `IsVerify = false`, email và password đúng.
- **Kết quả:** Code hiện tại vẫn cho login và tạo token. Claim `IsVerify` sẽ là `false`.

