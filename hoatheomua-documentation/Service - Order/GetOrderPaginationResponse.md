#### Request
```Json
query param:
pageSize
pageIndex
```
![[../Store-Image/Pasted image 20260624022757.png]]
#### Response
```json
{
  "value": {
    "items": [
      {
        "orderId": "ddba561c-e7c1-4375-8ca6-49a2c30f91a1",
        "status": "Pending",
        "trackNumber": "",
        "ahahaMoveOrderId": null,
        "ahahaMoveTrackingUrl": null,
        "remark": "TEST",
        "orderTime": 1782234000,
        "createAt": "2026-06-23T19:16:43.050442+00:00",
        "updateAt": null
      }
    ],
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": null,
  "timestampUtc": "0001-01-01T00:00:00"
}
```
#### Flow chart


![[../Store-Image/Pasted image 20260625113511.png]]

#### Test case
**Test case 01: Lấy danh sách order thành công**
- **Điều kiện:** User đã đăng nhập, token có claim `UserId` hợp lệ. Database có order của user với `IsDeleted = false`
- **Kết quả:** Hệ thống trả về `BasePaginationResponse` chứa danh sách order của user.
**Test case 02: Không tìm thấy UserId trong token**
- **Điều kiện:** Request không có claim `UserId`.
- **Kết quả:** Hệ thống ném lỗi `UnauthorizedException` với message `User not found`.
**Test case 03: UserId trong token sai định dạng Guid** -> chưa xử lí
- **Điều kiện:** Claim `UserId` có giá trị không phải Guid.
- **Kết quả:** Hệ thống ném lỗi khi gọi `Guid.Parse(user)`.
**Test case 04: pageIndex nhỏ hơn hoặc bằng 0**
- **Điều kiện:** Gửi `pageIndex <= 0`.
- **Kết quả:** Hệ thống tự động gán `pageIndex = 1`.
**Test case 05: pageSize nhỏ hơn hoặc bằng 0**
- **Điều kiện:** Gửi `pageSize <= 0`.
- **Kết quả:** Hệ thống tự động gán `pageSize = 10`.
**Test case 06: pageSize lớn hơn 100**
- **Điều kiện:** Gửi `pageSize > 100`.
- **Kết quả:** Hệ thống giới hạn `pageSize = 100`.
**Test case 07: Không có order nào**
- **Điều kiện:** User hợp lệ nhưng không có order nào trong database.
- **Kết quả:** Hệ thống trả về danh sách rỗng và `totalCount = 0`.
**Test case 08: Không lấy order đã bị xóa mềm**
- **Điều kiện:** Database có order `IsDeleted = true`.
- **Kết quả:** Hệ thống không trả về order đó.
**Test case 09: Không lấy order của user khác**
- **Điều kiện:** Database có order của nhiều user khác nhau.
- **Kết quả:** Hệ thống chỉ trả về order có `UserId` trùng với user đang đăng nhập.
**Test case 10: Order được sắp xếp theo CreatedAt giảm dần**
- **Điều kiện:** User có nhiều order với `CreatedAt` khác nhau.
- **Kết quả:** Order mới nhất được trả về trước.