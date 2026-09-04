# Format chung cho `ST-{number}-introduction.md`

Mục tiêu: đọc mục I là hiểu ngay US muốn gì; phần sau là code BE tương ứng.

## 1. Nguồn phải đọc

Theo thứ tự ưu tiên:

1. Quyết định mới nhất đã chốt với team/customer.
2. Story và System Test tương ứng trong `docs-us`.
3. `diagram/db_tong.md`: xem bảng/field trong file này như đã tồn tại thật.
4. Code trong solution: theo đúng convention hiện tại và chỉ tái sử dụng code có thật.

Với DB dynamic:

- Không suy luận FK/navigation từ `docs-db`.
- Tra output bằng `client_histories.type + output_id`.
- Giải thích rõ vai trò `base_id` theo từng US.

## 2. Format bắt buộc

### I. Hiểu nhanh US

Phải viết theo cách giải thích cho người chưa đọc US vẫn hình dung được:

1. Mở đầu bằng một câu: **US này dùng để làm gì và không làm gì**.
2. Mô tả ý tưởng đơn giản bằng chuỗi mũi tên.
3. Cho ví dụ ID/record cụ thể nếu có group hoặc dữ liệu dynamic.
4. Mô tả tuần tự luồng User → FE → BE → FE.
5. Liệt kê ngay các API cần có và mỗi API đáp ứng bước nào.
6. Chốt một câu về điểm quan trọng nhất của US.

Ví dụ cách viết:

```text
US-xxx chỉ dùng để ..., không ...

Hình dung đơn giản:
A → B → C.

Ví dụ:
Record 1: ...
Record 2: ...

Luồng hoạt động:
1. User ...
2. FE gọi ...
3. BE ...
4. FE render ...

Chỉ cần N API:
METHOD /route — mục đích.

Điểm quan trọng: ...
```

Không dùng các câu mô tả chung chung như “xử lý dữ liệu” hoặc “lấy thông tin cần thiết”.

### II. Code implement từng API

Sau mục I đi thẳng vào code. Mỗi API theo đúng thứ tự:

#### API N — Tên API

**Vai trò:** API đáp ứng bước nào trong flow phía trên.

**Request**

```http
METHOD /api/v1/...
```

```csharp
// Request.cs — nested class trong class Request
public class ExampleRequest
{
}
```

**Response**

```csharp
// Response.cs — nested class trong class Response
public class ExampleResponse
{
}
```

**IService**

```csharp
Task<Response.ExampleResponse> Example(
    Request.ExampleRequest request);
```

**Service**

```csharp
public async Task<Response.ExampleResponse> Example(
    Request.ExampleRequest request)
{
    // Implementation đầy đủ, tối giản.
}
```

Route parameter đơn giản không cần tạo Request DTO. Response đã đủ dữ liệu thì tái sử dụng, không tạo DTO/API thừa.

### III. DB còn thiếu — chỉ viết khi có

- Nếu DB đủ: bỏ hẳn mục này.
- Nếu thiếu: đặt cuối file, ghi đúng `table.field`, kiểu dữ liệu, mục đích và lý do bắt buộc.
- Không tự thêm bảng/field để làm code thuận tiện hơn.

## 3. Convention code bắt buộc

```text
HoaTheoMua.Service/{Module}/
├── Request.cs
├── Response.cs
├── IService.cs
└── Service.cs
```

- `Request`, `Response` là class bao ngoài; DTO là nested class.
- `Service : IService`; dependency inject qua constructor.
- Lấy user bằng `ICurrentUserService.GetRequiredUserId()`.
- Dùng exception trong `HoaTheoMua.Service.Exceptions`.
- API phân trang trả `BasePaginationResponse` bằng `ApiResponseFactory.BasePagination(...)`.
- Dùng `PageIndex`, `PageSize`; giới hạn `PageSize` tối đa 100.
- Query đọc dùng `AsNoTracking()`.
- Lọc `UserId`, `IsDeleted`, `Type` sớm và chỉ select field cần trả.
- Phân trang tại DB; tránh N+1 query và không tải binary ảnh qua BE.
- DB dynamic phải join thủ công bằng ID; không dùng `Include`/navigation giả định.
- Không gọi class/helper/method chưa tồn tại hoặc chưa được định nghĩa trong file.
- Method trong `Service` phải khớp `IService`.

## 4. Checklist

- [ ] Mục I đủ rõ để hiểu US và lý do tồn tại của từng API.
- [ ] Sau mục I đi thẳng vào code từng API.
- [ ] Mỗi API có Vai trò, Request, Response, IService, Service.
- [ ] Field khớp `db_tong.md`; code đúng convention solution.
- [ ] Không tạo API, DTO, bảng hoặc field thừa.
- [ ] Chỉ có mục DB còn thiếu khi DB thực sự thiếu.
- [ ] Chỉ giữ core logic, không diễn giải dài dòng.
