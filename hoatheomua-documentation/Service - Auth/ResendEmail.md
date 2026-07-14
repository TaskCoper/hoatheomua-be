#### Request
```Json
{
  "email": "string"
}
```
#### Response
```Json
{
  "value": "Đã gửi lại email xác thực. Vui lòng kiểm tra email.",
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH9T4BF37Q:00000009",
  "timestampUtc": "2026-06-23T17:52:36.0690391Z"
}
```
#### Flow chart
![[../Store-Image/Resend Mail.drawio.png]]
#### Test case
**Test case 01: Gửi lại email xác thực thành công**
- **Điều kiện:** Email tồn tại, user chưa verify và `IsDeleted = false`.
- **Kết quả:** Hệ thống tạo email verification token mới, gửi email và trả về message `Đã gửi lại email xác thực. Vui lòng kiểm tra email.`.
**Test case 02: Email không tồn tại**
- **Điều kiện:** Không tìm thấy user theo email.
- **Kết quả:** Hệ thống ném lỗi `NotFoundException` với message `Email Not Found`.
**Test case 03: User đã bị xóa mềm**
- **Điều kiện:** User có email đúng nhưng `IsDeleted = true`.
- **Kết quả:** Hệ thống không lấy user đó và ném lỗi `NotFoundException`.
**Test case 04: Email đã được verify**
- **Điều kiện:** User tồn tại và `IsVerify = true`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Email Is Verified`.
**Test case 05: Email có chữ hoa và khoảng trắng**
- **Điều kiện:** Request gửi email dạng `" User@Gmail.Com "`.
- **Kết quả:** Hệ thống trim và chuyển email về lowercase trước khi tìm user.
**Test case 06: Lỗi khi gửi mail** -> chưa bọc trasaction
- **Điều kiện:** `_mailService.SendMail` ném exception.
- **Kết quả:** Exception được throw ra ngoài.
**Test case 07: Request null**
- **Điều kiện:** Gọi `ReSendMail(null)`.
- **Kết quả:** Exception `Email is required` được throw ra