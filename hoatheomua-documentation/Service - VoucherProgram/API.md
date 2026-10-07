# API quản lý chương trình voucher

Theo P1, P2, P3, P5 và P6 trong `PLANS.md`, với P3 được mở rộng để gộp bật/tắt theo yêu cầu mới. Tất cả API yêu cầu JWT có role `Admin`.

## POST /api/v1/admin/voucher-programs

Tạo chương trình chưa bật (`isActive = false`), chưa có mã voucher. Thời gian nhận offset và được lưu/trả về UTC.

```json
{
  "name": "20/10",
  "startAt": "2026-10-15T00:00:00+07:00",
  "endAt": "2026-10-31T23:59:59+07:00",
  "minOrderAmount": 200000,
  "maxOrderAmount": null
}
```

Response HTTP 200, bọc bằng `ApiResponseFactory.Base`. Ví dụ `value`:

```json
{
  "id": "9c8b7a6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d",
  "name": "20/10",
  "isActive": false,
  "isDeleted": false,
  "startAt": "2026-10-14T17:00:00+00:00",
  "endAt": "2026-10-31T16:59:59+00:00",
  "minOrderAmount": 200000,
  "maxOrderAmount": null,
  "voucherCount": 0,
  "usedVoucherCount": 0,
  "createdAt": "2026-10-07T03:00:00+00:00",
  "updatedAt": null
}
```

Validation HTTP 422, `messageCode = VALIDATION_ERROR`:

| Field | Điều kiện | Message |
| --- | --- | --- |
| `name` | Rỗng hoặc chỉ có khoảng trắng | `Name is required` |
| `name` | Dài hơn giới hạn cột DB (200 ký tự) | `Name must not exceed 200 characters` |
| `startAt` | Không có thời gian bắt đầu | `Start time is required` |
| `endAt` | Không có thời gian kết thúc | `End time is required` |
| `endAt` | Không lớn hơn thời gian bắt đầu | `End time must be later than start time` |
| `minOrderAmount` | Có giá trị âm | `Min order amount must not be negative` |
| `maxOrderAmount` | Có giá trị âm | `Max order amount must not be negative` |
| `maxOrderAmount` | Lớn hơn 0 nhưng nhỏ hơn min khi cả hai có giá trị | `Max order amount cannot be less than min order amount` |

Tên được trim trước khi lưu. Min/max có thể bỏ trống. `minOrderAmount = 0` không đặt mức tối thiểu lớn hơn 0; `maxOrderAmount = 0` được chuyển thành `null` trước khi lưu/trả về, nghĩa là không giới hạn tối đa. Vì vậy gửi cả min và max bằng 0 sẽ không giới hạn khoảng giá trị đơn. Gửi min lớn hơn 0 và max bằng 0 thì chỉ có giới hạn tối thiểu. Giá trị âm vẫn bị từ chối. Không có yêu cầu tên chương trình phải unique.

### Validation định dạng thời gian

`startAt` và `endAt` trong request được nhận dưới dạng chuỗi để FluentValidation kiểm tra trước khi service chuyển sang `DateTimeOffset`. Giá trị hợp lệ phải là ngày giờ ISO 8601, ví dụ `2026-10-31T23:59:59+07:00` hoặc `2026-10-31T16:59:59Z`.

Chuỗi ngày không hợp lệ hoặc định dạng như `31/10/2026 23:59:59` trả HTTP 422, `messageCode = VALIDATION_ERROR`. Ví dụ phần `errors`:

```json
[
  {
    "field": "endAt",
    "message": "End time must be a valid ISO 8601 date-time, for example 2026-10-31T23:59:59+07:00"
  }
]
```

Validation trả đúng field `startAt` hoặc `endAt`. Sau khi cả hai chuỗi hợp lệ, validator mới kiểm tra `endAt` phải muộn hơn `startAt`. JSON sai cú pháp hoặc field có kiểu JSON không đúng vẫn do model binding chung của ASP.NET xử lý trước FluentValidation.

## GET /api/v1/admin/voucher-programs

```http
GET /api/v1/admin/voucher-programs?pageIndex=1&pageSize=20&keyword=20%2F10&isActive=false&isDelete=false
```

| Query | Mặc định | Cách dùng |
| --- | --- | --- |
| `pageIndex` | 1 | Trang bắt đầu từ 1; giá trị <= 0 dùng 1 |
| `pageSize` | 20 | Giá trị <= 0 dùng 20 |
| `keyword` | Không lọc | Tìm trong tên, trim và không phân biệt hoa/thường |
| `isActive` | Không lọc | `true` hoặc `false` |
| `isDelete` | Không lọc | `true`: đã xóa mềm; `false`: chưa xóa mềm; không gửi: lấy cả hai |

API danh sách của admin mặc định lấy cả chương trình đã xóa mềm, đã hết hạn, đang diễn ra và chưa bắt đầu. Không tự lọc theo thời gian; `keyword`, `isActive` và `isDelete` chỉ lọc khi được gửi, có thể kết hợp với nhau. Query `isDelete` lọc cột `IsDeleted` trong DB; response vẫn dùng tên `isDeleted`. Sắp xếp `createdAt` giảm dần, rồi `id` giảm dần nếu trùng thời gian. Response HTTP 200 là envelope phân trang có `value.items`, `pageIndex`, `pageSize`, `totalCount`, `hasNextPage`, `hasPreviousPage`. Mỗi item có cấu trúc như `value` của POST; `totalCount` tính các chương trình khớp toàn bộ bộ lọc trước khi phân trang.

- `isDeleted`: cho biết chương trình đã xóa mềm hay chưa; `endAt` cho biết thời điểm hết hạn. `isActive` là trạng thái bật/tắt, độc lập với lịch và trạng thái xóa.
- `voucherCount`: số voucher chưa xóa của chương trình.
- `usedVoucherCount`: số voucher chưa xóa có `UsedCount > 0`, không phải tổng lượt sử dụng. Đọc cột lưu tạm theo P2 nên có thể trễ; không đếm lại đơn hàng trong API này.
- Không có kết quả: `items = []`, `totalCount = 0`.
- Giữ envelope phân trang hiện tại của dự án: `traceId = null`, `timestampUtc = "0001-01-01T00:00:00"`.

## PUT /api/v1/admin/voucher-programs/{id}

```json
{
  "name": "Chương trình 20/10",
  "endAt": "2026-11-05T23:59:59+07:00",
  "isActive": true
}
```

Chỉ gửi trường muốn đổi: `name`, `startAt`, `endAt`, `minOrderAmount`, `maxOrderAmount`, `isActive`. Trường bỏ qua hoặc `null` được giữ nguyên. Gửi `maxOrderAmount = 0` để xóa giới hạn tối đa hiện có; DB và response sẽ có `maxOrderAmount = null`. Muốn áp dụng cho mọi giá trị đơn, gửi `minOrderAmount = 0` và `maxOrderAmount = 0`. Request rỗng trả HTTP 422, `At least one voucher program field must be provided for update`.

`isActive` là tùy chọn. Khi gửi `true` hoặc `false`, cập nhật chương trình và mọi voucher chưa xóa của chương trình trong cùng transaction với các trường khác. Khi bật lại, cả voucher từng bị tắt riêng cũng được bật. Voucher đã xóa không được khôi phục. Muốn chỉ bật/tắt thì gửi `{"isActive": true}` hoặc `{"isActive": false}`.

Tên được trim và tối đa 200 ký tự. Lịch được lưu UTC; thời gian kết thúc phải lớn hơn bắt đầu. Min/max không âm; khi có giới hạn tối đa (max lớn hơn 0), max không nhỏ hơn min. Các điều kiện được kiểm tra sau khi chuyển max bằng 0 thành `null` và ghép request với DB, kể cả khi chỉ gửi một đầu của khoảng.

Response HTTP 200, envelope có `value` là `VoucherProgramResponse` sau cập nhật, cùng cấu trúc với POST tạo chương trình. Route PATCH `/{id}/active` đã được gộp vào PUT này.

## DELETE /api/v1/admin/voucher-programs/{id}

Không có request body. Xóa mềm chương trình và toàn bộ voucher của chương trình trong cùng transaction. Giữ nguyên mã, số lượt dùng và snapshot voucher trong các đơn cũ.

Response HTTP 200, envelope có `value = "Program deleted"`. Chương trình đã xóa vẫn xuất hiện trong API danh sách admin với `isDeleted = true`, nhưng không được sửa hoặc nhận thêm mã.

## POST /api/v1/admin/voucher-programs/{id}/vouchers

```json
{
  "codePrefix": "HTM",
  "codeLength": 7,
  "codeSuffix": "",
  "quantity": 2,
  "maxUsageCount": 1,
  "discountType": "percent",
  "discountValue": 20,
  "maxDiscountAmount": 100000,
  "description": "Quà tặng ngày Phụ nữ Việt Nam"
}
```

Response HTTP 200, ví dụ `value`:

```json
{
  "programId": "9c8b7a6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d",
  "createdCount": 2,
  "codes": ["HTMK7Q2M4X", "HTM9P4XRB2"]
}
```

- `codeLength`: số ký tự ngẫu nhiên, từ 5 đến 10; tiền tố/hậu tố chỉ nhận `A-Z0-9`; cả mã tối đa 20 ký tự. Tiền tố/hậu tố có thể bỏ trống.
- `quantity`: 1–1.000 mỗi lô; tổng không quá 10.000 mã đã cấp cho chương trình, kể cả mã đã xóa mềm. Có thể sinh nhiều lô.
- `maxUsageCount`: lớn hơn 0, áp dụng cho từng mã.
- `discountType`: `cash` hoặc `percent`; `discountValue > 0`, với `percent` không quá 100. `maxDiscountAmount` chỉ nhận khi `percent` và phải lớn hơn 0 nếu gửi.
- `description`: tùy chọn, tối đa 500 ký tự theo cột DB.
- Mọi mã mới đều private, `isActive = false`, `usedCount = 0`, kể cả khi chương trình đang bật. Lịch và khoảng tiền của voucher để `null`, kế thừa chương trình. Dùng PUT phía trên với `isActive = true` để bật cả lô.
- Sinh bằng `RandomNumberGenerator`, phần ngẫu nhiên bỏ `0`, `O`, `1`, `I`. Unique index bảo vệ mã trên toàn hệ thống, kể cả mã đã xóa. PostgreSQL dùng `ON CONFLICT DO NOTHING`, thử tối đa 5 vòng.
- Mỗi lô là một transaction: tạo đủ số lượng hoặc rollback toàn bộ. Khóa dòng chương trình khi sinh mã/bật/tắt/xóa để các request đồng thời không vượt hạn mức hoặc bỏ sót voucher.

Cả ba API sửa/xóa/sinh mã trả HTTP 404, `messageCode = NOT_FOUND`, với `detail = "Voucher program does not exist."` nếu không tồn tại và `"Voucher program has been deleted."` nếu đã xóa. Sinh mã vượt hạn mức trả HTTP 409, `CONFLICT`, `Voucher program cannot exceed 10000 vouchers.`; không sinh đủ sau 5 vòng trả HTTP 500. Request không hợp lệ trả HTTP 422.

## Message lỗi cụ thể

Lỗi validation giữ `messageCode = VALIDATION_ERROR`; mỗi mục trong `errors` có `field` và `message` riêng. Cùng một nguyên nhân ở API tạo và sửa dùng cùng message; các nguyên nhân khác nhau được tách để xác định đúng field cần sửa.

| Trường hợp | Field | Message |
| --- | --- | --- |
| Min âm | `minOrderAmount` | `Min order amount must not be negative` |
| Max âm | `maxOrderAmount` | `Max order amount must not be negative` |
| Max dương nhỏ hơn min khi gửi cả hai | `maxOrderAmount` | `Max order amount cannot be less than min order amount` |
| Chỉ sửa min và vượt max đang lưu | `minOrderAmount` | `Min order amount cannot exceed the current max order amount` |
| Chỉ sửa max dương và nhỏ hơn min đang lưu | `maxOrderAmount` | `Max order amount cannot be less than the current min order amount` |
| Chỉ sửa thời gian bắt đầu và không sớm hơn kết thúc đang lưu | `startAt` | `Start time must be earlier than the current end time` |
| Chỉ sửa thời gian kết thúc và không muộn hơn bắt đầu đang lưu | `endAt` | `End time must be later than the current start time` |
| Gửi cả hai mốc nhưng kết thúc không muộn hơn bắt đầu | `endAt` | `End time must be later than start time` |
| Phần ngẫu nhiên quá ngắn | `codeLength` | `Code length must be at least 5` |
| Phần ngẫu nhiên quá dài | `codeLength` | `Code length must not exceed 10` |
| Tiền tố sai ký tự | `codePrefix` | `Code prefix must contain only uppercase letters or digits` |
| Hậu tố sai ký tự | `codeSuffix` | `Code suffix must contain only uppercase letters or digits` |
| Tổng độ dài mã vượt 20 | `codeLength` | `Voucher code must not exceed 20 characters including prefix and suffix` |
| Số lượng nhỏ hơn 1 | `quantity` | `Quantity must be at least 1` |
| Số lượng vượt 1.000 | `quantity` | `Quantity must not exceed 1000` |
| Loại giảm giá rỗng | `discountType` | `Discount type is required` |
| Loại giảm giá không được hỗ trợ | `discountType` | `Discount type must be cash or percent` |

Các lỗi tên, thiếu thời gian, số lần dùng, giá trị giảm, mức giảm tối đa và mô tả tiếp tục có message riêng theo từng điều kiện. `maxOrderAmount = 0` vẫn được chuẩn hóa thành `null` trước khi kiểm tra khoảng tiền.

## Kiểm tra

```sh
dotnet test HoaTheoMua.Service.Test -c Release --filter FullyQualifiedName~VoucherProgramTest
```

Test gồm lưu UTC, trạng thái ban đầu, validation, lọc/xóa mềm, phân trang, số mã, bật/tắt, sinh nhiều lô, trùng mã, thất bại giữa lô, giới hạn số mã và HTTP 401/403/404/422/200. Test HTTP dùng EF InMemory; không khởi động API chính hoặc chạy migration trên DB ứng dụng.

Hai test PostgreSQL được bật bằng `VOUCHER_TEST_CONNECTION_STRING`, trỏ tới PostgreSQL test trên `localhost` hoặc `127.0.0.1` và tài khoản có quyền tạo DB. Test tự tạo DB `voucher_test_<guid>`, áp dụng migration voucher, kiểm tra `ON CONFLICT`, rollback, cập nhật cả chương trình và hai request sinh mã đồng thời, rồi xóa DB test. Không có biến này thì hai test được skip.
