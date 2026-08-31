# Structure viết file code-introduction

Áp dụng cho file `ST-{story-number}-introduction.md`.

Mục tiêu: ngắn, đúng US, đúng DB và không dùng class/hàm chưa được định nghĩa.

## I. Nguồn được phép sử dụng

Chỉ viết dựa trên:

1. `HoaTheoMua.Repository/diagram/db_tong.md`.
2. Entity, Configuration, DbSet, Service, Interface, DTO, Controller và helper đang tồn tại trong solution.
3. Class/hàm mới được định nghĩa đầy đủ ngay trong file introduction đang viết.

Không lấy field từ trí nhớ, DB cũ hoặc tự suy đoán.

## II. Quy tắc kiểm tra trước khi viết

Với mỗi class, property hoặc method được dùng:

- Tìm trong solution bằng đúng tên.
- Kiểm tra chữ ký trong interface, không chỉ implementation.
- Kiểm tra method có endpoint hay chưa.
- Kiểm tra Entity/DbSet có thật trong `AppDbContext` hay chưa.
- Kiểm tra field và kiểu dữ liệu khớp `db_tong.md`.

Nếu method chỉ có trong implementation nhưng chưa có trong interface/controller, ghi rõ là **chưa thể tái sử dụng qua contract hiện tại**.

Không viết kiểu sau nếu chưa định nghĩa:

```csharp
await _unknownService.DoSomething();
return Response.From(entity);
return await query.ToPageAsync();
```

Nếu cần helper mới, phải có đủ:

```text
- Tên class/interface.
- Chữ ký method.
- Dependency/constructor.
- Implementation.
- DTO request/response liên quan.
```

## III. Structure bắt buộc

### 0. Phạm vi

Tóm tắt main flow và nói rõ US có hoặc không gọi AI, tạo Order, thanh toán hoặc thay đổi trạng thái.

### I. Nguồn dữ liệu và code có sẵn

Liệt kê:

| Nguồn | Có trong DB | Có trong code | Cách dùng |
|---|---|---|---|
| `table/entity` | Có/Không | Có/Không | Một câu. |

Ghi đúng chữ ký method đang tái sử dụng.

### II. API cần có

| # | Trạng thái | API | Mục đích |
|---|---|---|---|
| 1 | Có sẵn/Cần thêm | `METHOD /route` | Một câu. |

Không gọi API mới là “có sẵn”.

### III. DB và code còn thiếu

Chỉ dùng field có trong `db_tong.md`. Nếu US bắt buộc field mới, ghi rõ đây là **đề xuất thay đổi DB**, không viết như field đã tồn tại.

```text
Table/Entity:
- field: kiểu trong db_tong.md — ý nghĩa.
```

Nếu DB đã có bảng nhưng solution chưa có Entity/DbSet, phải định nghĩa phần code còn thiếu trước khi dùng `_dbContext.TableName`.

### IV. DTO, interface và dependency cần thêm

Định nghĩa trước:

- Request DTO.
- Response DTO.
- Snapshot DTO nếu có.
- Service interface.
- Service constructor và dependency.

Sau phần này, mọi symbol dùng trong implementation phải là code có thật hoặc đã được định nghĩa.

### V. Request, response và implementation từng API

Mỗi API dùng cùng structure:

#### API — Tên `[Có sẵn/Cần thêm]`

**Mục đích**

Một câu.

**Request**

```http
METHOD /api/v1/resource
```

```json
{
  "field": "value"
}
```

**Response**

```json
{
  "value": {}
}
```

**Code implement**

- API có sẵn: trích đúng controller/service hiện có.
- API cần thêm: viết method hoàn chỉnh, không gọi helper chưa định nghĩa.

### VI. Các điểm khó hình dung

Giải thích ngắn bằng dữ liệu mẫu hoặc query:

- Cách liên kết/gom nhóm.
- Cách truy vấn polymorphic `type + output_id`.
- Vị trí lưu snapshot và cách render lại.

### VII. Điểm chưa đáp ứng

Nếu US yêu cầu dữ liệu không tồn tại trong DB/code:

- Nêu đúng field/thông tin bị thiếu.
- Không tự tạo dữ liệu thay thế.
- Ghi lựa chọn cần BA/lead xác nhận.

## IV. Quy tắc riêng cho DB động

Với `client_histories`, luôn giải thích rõ:

```text
type      = xác định bảng output.
base_id   = ID mà output được tạo dựa trên hoặc ID gốc dùng gom nhóm.
output_id = ID record cần truy vấn trong bảng được xác định bởi type.
```

Không tự thêm database `Ref` nếu convention dự án là polymorphic không FK.

Ví dụ:

```text
type = flower -> generated_flowers.id = output_id
type = card   -> generated_cards.id = output_id
```

## V. Quy tắc snapshot

- Dùng đúng kiểu DB: `json` khác `jsonb`.
- Snapshot lưu dữ liệu cần render lại và dữ liệu lịch sử không được thay đổi theo nguồn hiện hành.
- Snapshot phải có `schemaVersion`.
- ID trong snapshot dùng để kiểm tra lại dữ liệu hiện hành trước nghiệp vụ tiếp theo.
- Không gọi snapshot là quan hệ FK.

## VI. Checklist cuối

- [ ] Đã đọc `db_tong.md` hiện tại.
- [ ] Không dùng field ngoài DB mà không note là đề xuất.
- [ ] Mọi method tái sử dụng tồn tại trong interface.
- [ ] Mọi method mới đã có chữ ký và implementation.
- [ ] Mọi `_dbContext.X` đã có Entity và DbSet được định nghĩa.
- [ ] Request/response chỉ chứa dữ liệu phục vụ US.
- [ ] Code không gọi helper giả định.
- [ ] Đã nêu mâu thuẫn giữa US và DB/code.
- [ ] File ngắn, đọc một lần hiểu được luồng.
