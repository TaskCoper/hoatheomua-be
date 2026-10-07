# Voucher Database

## 1. Bảng `Voucher`

Mỗi bản ghi là một mã voucher và chứa trực tiếp chính sách giảm giá. Không sử dụng bảng chương trình voucher.

| Field | Kiểu dữ liệu | Bắt buộc | Mô tả |
|---|---|---:|---|
| `Id` | `uuid` | Có | Khóa chính. |
| `Code` | `varchar` | Có | Mã khách sử dụng; unique và không phân biệt hoa/thường khi kiểm tra. |
| `Name` | `varchar` | Có | Tên hiển thị/quản trị voucher. |
| `DiscountType` | `varchar` | Có | `cash`: giảm tiền cố định; `percent`: giảm theo phần trăm. |
| `DiscountValue` | `decimal` | Có | Giá trị tiền hoặc phần trăm tùy `DiscountType`. |
| `Visibility` | `varchar` | Có | `public`: hiển thị tại checkout; `private`: khách phải nhập mã. |
| `MaxUsageCount` | `int` | Có | Tổng số lượt tối đa được phép sử dụng; phải lớn hơn `0`. |
| `UsedCount` | `int` | Có | Số lượt đã sử dụng; mặc định `0`, không được âm hoặc vượt `MaxUsageCount`. |
| `StartAt` | `timestamptz` | Có | Thời điểm voucher bắt đầu có hiệu lực. |
| `EndAt` | `timestamptz` | Có | Thời điểm hết hiệu lực; phải lớn hơn `StartAt`. |
| `IsActive` | `bool` | Có | Cho phép bật/tắt voucher; mặc định `true`. |
| `CreatedAt` | `timestamptz` | Có | Thời điểm tạo. |
| `UpdatedAt` | `timestamptz` | Không | Thời điểm cập nhật gần nhất. |
| `IsDeleted` | `bool` | Có | Soft delete; mặc định `false`. |

### Ràng buộc

- `Code` phải unique trên các voucher chưa bị xóa.
- `DiscountValue > 0`.
- Nếu `DiscountType = percent` thì `DiscountValue <= 100`.
- `UsedCount >= 0` và `UsedCount <= MaxUsageCount`.
- Mã dùng chung: một voucher có `MaxUsageCount > 1`.
- Mã dùng riêng: mỗi voucher thường có `MaxUsageCount = 1`.

### Index

- Unique index cho `Code` đã chuẩn hóa hoa/thường.
- Index phục vụ danh sách checkout: `(Visibility, IsActive, IsDeleted, StartAt, EndAt)`.

## 2. Bổ sung vào bảng `Order`

Thêm một field:

| Field | Kiểu dữ liệu | Bắt buộc | Mô tả |
|---|---|---:|---|
| `VoucherSnapshot` | `jsonb` | Không | Snapshot voucher và kết quả giảm giá tại thời điểm tạo đơn. `null` nếu không dùng voucher. |

Cấu trúc `VoucherSnapshot`:

```json
{
  "voucherId": "guid",
  "code": "SALE20",
  "discountType": "percent",
  "discountValue": 20,
  "discountAmount": 50000
}
```

- `discountValue`: giá trị cấu hình của voucher.
- `discountAmount`: số tiền thực tế được giảm trên đơn hàng.
- Snapshot không được cập nhật theo những thay đổi về sau của bảng `Voucher`.

## 3. Quan hệ với `Order`

- Quan hệ nghiệp vụ: một `Voucher` có thể được nhiều `Order` sử dụng.
- Không tạo foreign key vật lý từ `Order` sang `Voucher`; `voucherId` được lưu trong `VoucherSnapshot`.
- Việc sửa hoặc soft delete voucher không làm thay đổi lịch sử đơn hàng.

## 4. Nội dung đang pending

- Thời điểm tăng `UsedCount`: khi tạo đơn `Pending` hay khi thanh toán thành công.
- Có giữ trước lượt voucher trong thời gian đơn đang `Pending` hay không.
- `BatchId`/export và tích hợp voucher với hệ thống bên ngoài chưa nằm trong phạm vi hiện tại.
