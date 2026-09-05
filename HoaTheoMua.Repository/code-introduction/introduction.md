# Format chung cho file giới thiệu US

Mục tiêu: đọc nhanh để biết US cần bao nhiêu API, từng API làm gì và có code triển khai đầy đủ.

## 1. Nguồn đối chiếu

Theo thứ tự ưu tiên:

1. Quyết định mới nhất đã chốt với customer/team.
2. Story và System Test tương ứng trong `doc-us`.
3. `diagram/db_tong.md`: xem bảng và field trong đây như đã tồn tại.
4. Code hiện tại trong solution: giữ đúng convention của team.

Với DB dynamic:

- Không tự giả định FK/navigation.
- Tra output bằng `client_histories.type + output_id`.
- Hiểu `base_id` theo đúng flow của từng US.

## 2. Cách chọn API

- Một US có thể có một hoặc nhiều API.
- Chỉ tạo API thật sự cần để FE đi hết Main Flow, Alternative Flow và các System Test liên quan.
- Gộp dữ liệu vào cùng API khi chúng cùng màn hình, cùng quyền truy cập và số lượng dữ liệu nhỏ. Ví dụ: chi tiết thiệp kèm các Order liên quan.
- Tách API khi dữ liệu có phân trang riêng, quyền khác nhau, xử lý nặng hoặc được gọi ở thời điểm khác.
- Tái sử dụng API của US khác nếu đã đáp ứng đúng chức năng; không tạo API trùng.
- Không tạo một API riêng cho mỗi thao tác FE nếu có thể lấy hợp lý trong cùng response.
- API danh sách lớn phải phân trang; dữ liệu nhỏ, giới hạn cố định thì không cần.

## 3. Format file bắt buộc

## I. Các API cần làm

Mở đầu bằng một câu ngắn về mục đích US, sau đó liệt kê API:

```text
US-xxx dùng để ...; không xử lý ...

Cần N API:

1. METHOD /api/v1/...
   - FE gọi khi nào.
   - API trả gì.

2. METHOD /api/v1/...
   - FE gọi khi nào.
   - API trả gì.
```

Nếu cần mô tả flow, chỉ viết một chuỗi ngắn:

```text
User thao tác
→ FE gọi API 1
→ User click
→ FE gọi API 2
```

Không diễn giải dài, không lặp nội dung Story.

## II. Code từng API

Mỗi API trình bày đúng thứ tự sau:

### API N — Tên API

**Vai trò:** một câu ngắn.

### IService

```csharp
Task<Response.ExampleResponse> Example(
    Request.ExampleRequest request);
```

### Request

```http
METHOD /api/v1/...
```

```csharp
// Request.cs
public class Request
{
    public class ExampleRequest
    {
    }
}
```

Route/query parameter đơn giản không cần tạo Request DTO.

### Response

```csharp
// Response.cs
public class Response
{
    public class ExampleResponse
    {
    }
}
```

Chỉ thêm payload JSON mẫu khi response khó hình dung.

### Service

```csharp
public async Task<Response.ExampleResponse> Example(
    Request.ExampleRequest request)
{
    // Code đầy đủ, tối giản và chạy đúng flow.
}
```

Không để pseudo-code, TODO hoặc gọi helper chưa được định nghĩa.

## III. DB còn thiếu — chỉ thêm khi cần

- DB đủ thì bỏ hẳn mục này.
- DB thiếu thì ghi ngắn gọn `table.field`, kiểu dữ liệu và mục đích.
- Không tự thêm bảng/field chỉ để code thuận tiện hơn.

## 4. Convention code

```text
HoaTheoMua.Service/{Module}/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

- `Request` và `Response` là class bao ngoài; DTO là nested class.
- `Service : IService`; dependency inject qua constructor.
- Lấy user bằng `ICurrentUserService.GetRequiredUserId()`.
- Dùng exception trong `HoaTheoMua.Service.Exceptions`.
- Query đọc dùng `AsNoTracking()`.
- Lọc sớm `UserId`, `IsDeleted`, `Type` và ownership.
- DB dynamic join thủ công bằng ID; không dùng navigation giả định.
- API phân trang dùng `PageIndex`, `PageSize ≤ 100` và trả `BasePaginationResponse`.
- Chỉ select field FE cần; tránh N+1 query.
- Không truyền binary ảnh qua BE khi FE có thể tải trực tiếp từ URL storage.
- Dữ liệu nguồn có thể inactive/xóa phải đọc từ snapshot đã lưu.
- Method trong `Service` phải khớp `IService`.

## 5. Checklist

- [ ] Mục I cho biết ngay số API và vai trò từng API.
- [ ] API đủ để FE hoàn thành Main Flow và System Test.
- [ ] Không thiếu API và không có API trùng/thừa.
- [ ] Mỗi API đúng thứ tự `IService → Request → Response → Service`.
- [ ] Code đầy đủ, ngắn gọn và đúng DB/convention.
- [ ] Chỉ thêm mục DB còn thiếu khi thật sự cần.
