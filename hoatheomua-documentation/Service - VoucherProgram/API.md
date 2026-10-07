# Chương trình voucher: tạo và lấy danh sách

Theo P1 và P2 trong `PLANS.md`. Cả hai API yêu cầu JWT có role `Admin`.

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
| `minOrderAmount`, `maxOrderAmount` | Có giá trị âm | `Order amount must not be negative` |
| `maxOrderAmount` | Nhỏ hơn min khi cả hai có giá trị | `Max order amount cannot be less than min order amount` |

Tên được trim trước khi lưu. Min/max có thể bỏ trống; 0 hợp lệ. Không có yêu cầu tên chương trình phải unique.

## GET /api/v1/admin/voucher-programs

```http
GET /api/v1/admin/voucher-programs?pageIndex=1&pageSize=20&keyword=20%2F10&isActive=false
```

| Query | Mặc định | Cách dùng |
| --- | --- | --- |
| `pageIndex` | 1 | Trang bắt đầu từ 1; giá trị <= 0 dùng 1 |
| `pageSize` | 20 | Giá trị <= 0 dùng 20 |
| `keyword` | Không lọc | Tìm trong tên, trim và không phân biệt hoa/thường |
| `isActive` | Không lọc | `true` hoặc `false` |

Chỉ lấy chương trình chưa xóa mềm. Sắp xếp `createdAt` giảm dần, rồi `id` giảm dần nếu trùng thời gian. Response HTTP 200 là envelope phân trang có `value.items`, `pageIndex`, `pageSize`, `totalCount`, `hasNextPage`, `hasPreviousPage`. Mỗi item có cấu trúc như `value` của POST.

- `voucherCount`: số voucher chưa xóa của chương trình.
- `usedVoucherCount`: số voucher chưa xóa có `UsedCount > 0`, không phải tổng lượt sử dụng. Đọc cột lưu tạm theo P2 nên có thể trễ; không đếm lại đơn hàng trong API này.
- Không có kết quả: `items = []`, `totalCount = 0`.
- Giữ envelope phân trang hiện tại của dự án: `traceId = null`, `timestampUtc = "0001-01-01T00:00:00"`.

## Kiểm tra

```sh
dotnet test HoaTheoMua.Service.Test -c Release --filter FullyQualifiedName~VoucherProgramTest
```

Test gồm lưu UTC, trạng thái ban đầu, validation, lọc/xóa mềm, phân trang, số mã và HTTP 401/403/422/200. Test HTTP dùng EF InMemory; không khởi động API chính hoặc chạy migration trên DB thật.
