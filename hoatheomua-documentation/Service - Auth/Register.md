#### Request
```Json
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string"
}
```
#### Response
```Json
{
  "value": "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH173ERRPV:00000003",
  "timestampUtc": "2026-06-23T09:31:59.5903939Z"
}
```
#### Flow chart
![[../Store-Image/Register.drawio.png]]
#### Test case
**Test case 01: Đăng ký tài khoản thành công**
- **Điều kiện:** Email chưa tồn tại, `Password` trùng `ConfirmPassword`.
- **Kết quả:** Hệ thống tạo user mới, hash password, set `Role = User`, `IsVerify = false`, gửi email xác thực và trả về message đăng ký thành công.
**Test case 02: Password và ConfirmPassword không khớp**
- **Điều kiện:** `request.Password != request.ConfirmPassword`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Mật khẩu xác nhận không trùng khớp!`.
**Test case 03: Email đã tồn tại**
- **Điều kiện:** Database đã có user với email giống email request.
- **Kết quả:** Hệ thống ném lỗi `ConflictException` với message `Email {email} đã tồn tại trong hệ thống.`.
**Test case 04: Email có chữ hoa và khoảng trắng**
- **Điều kiện:** Request gửi email dạng `" Test@Gmail.Com "`.
- **Kết quả:** Hệ thống trim và chuyển email thành lowercase trước khi lưu, ví dụ `test@gmail.com`.
**Test case 05: Hash password có pepper**
- **Điều kiện:** Đăng ký user thành công.
- **Kết quả:** Password được cộng thêm `_securityOptions.Pepper` trước khi hash bằng BCrypt SHA256.
**Test case 06: User mới có role mặc định là User**
- **Điều kiện:** Đăng ký user thành công.
- **Kết quả:** User mới được lưu với `Role = Role.User`.
**Test case 07: User mới chưa verify**
- **Điều kiện:** Đăng ký user thành công.
- **Kết quả:** User mới được lưu với `IsVerify = false`.
**Test case 08: Gửi email xác thực thành công**
- **Điều kiện:** User được tạo thành công.
- **Kết quả:** Hệ thống tạo email verification token và gọi `_mailService.SendMail`.
**Test case 09: Lỗi khi gửi mail**
- **Điều kiện:** `_mailService.SendMail` ném exception.
- **Kết quả:** Transaction rollback, user không được lưu hoàn tất.
**Test case 10: Lỗi khi SaveChanges**
- **Điều kiện:** Database lỗi khi `SaveChangesAsync`.
- **Kết quả:** Transaction rollback và lỗi được throw ra ngoài.
**Test case 11: Request Validate**
- **Điều kiện:** Validate tất cả những gì mà người dùng nhập vào, đúng định dạng email, emails chưa tồn tại trong hệ thống, password mạnh, password và confirmpassword khớp với nhau
- **Kết quả:** Người dùng đăng kí thành công.