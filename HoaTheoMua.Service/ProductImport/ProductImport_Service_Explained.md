# Tóm tắt: `ProductImport/Service.cs`

## 1. Background — feature này sinh ra để làm gì?

Cửa hàng quản lý sản phẩm trên **Nhanh.vn** (POS). Nhanh.vn cho phép xuất toàn bộ danh mục sản phẩm ra file **Excel**. Feature `ProductImport` nhận file Excel đó (upload qua API), đọc từng dòng và **đồng bộ vào bảng `Products` của database nội bộ** theo cơ chế **upsert**:

- Sản phẩm **đã có** trong DB (nhận diện bằng `NhanhProductId`) → **update**.
- Sản phẩm **chưa có** → **insert** mới.

Nhờ vậy có thể import lặp lại nhiều lần mà không tạo bản ghi trùng. Thư viện đọc Excel là **MiniExcel**; dependency duy nhất là `AppDbContext` (EF Core).

## 2. `ImportFromExcel(Stream fileStream)` — hàm public duy nhất

Đây là hàm "nhạc trưởng": không tự làm chi tiết, chỉ gọi 5 hàm con theo thứ tự rồi trả về `Response.ImportResult` (TotalRecords / InsertedCount / UpdatedCount):

```
Stream file Excel
  1. BufferStreamAsync()          → copy vào MemoryStream để đọc được nhiều lần
  2. ValidateFileStructureAsync() → chặn sớm file sai định dạng
  3. ReadRowsAsync()              → parse thành List<ExcelRow>
  4. BuildUpsertBatchAsync()      → phân loại từng dòng: insert hay update
  5. PersistBatchAsync()          → ghi DB trong 1 transaction
→ ImportResult (số liệu thống kê)
```

**Xử lý lỗi:** `AppException` (lỗi nghiệp vụ, vd `BadRequestException`) được ném lại nguyên vẹn để middleware trả đúng mã lỗi; mọi exception khác bị bọc thành `BadRequestException("Excel processing error: …")` để không lộ chi tiết nội bộ.

## 3. Vai trò các hàm con & tại sao cần

### Nhóm chuẩn bị & đọc file (orchestration)

| Hàm | Làm gì | Tại sao cần |
|---|---|---|
| `BufferStreamAsync` | Copy stream upload vào `MemoryStream` | Stream HTTP chỉ đọc được 1 lần, không seek được — nhưng file cần đọc **2 lần** (validate header + parse dữ liệu), nên phải đệm vào bộ nhớ để tua lại (`Position = 0`) |
| `ValidateFileStructureAsync` | Đọc dòng header thô, kiểm tra: cột A chứa `"ID"` và file có ≥ 40 cột | Chặn sớm file không phải export từ Nhanh.vn, báo lỗi rõ ràng thay vì import lệch dữ liệu |
| `ReadRowsAsync` | Parse file thành `List<Request.ExcelRow>` (MiniExcel map theo `[ExcelColumnIndex]`), gán `SourceRowNumber = i + 2` | `SourceRowNumber` là số dòng thật trên Excel (dòng 1 là header) — dùng để truy vết khi dòng nào đó có vấn đề |

### Nhóm upsert (phân loại & ghi DB)

| Hàm | Làm gì | Tại sao cần |
|---|---|---|
| `BuildUpsertBatchAsync` | Gom mọi `NhanhProductId` trong file → **1 query duy nhất** nạp các sản phẩm đã tồn tại (chưa soft-delete) vào Dictionary → duyệt từng dòng, phân vào `ToInsert` / `ToUpdate` | Query 1 lần thay vì query trong vòng lặp → tránh N+1, file lớn vẫn nhanh |
| `ResolveProduct` | Tra `NhanhProductId` trong Dictionary: có → trả entity cũ (`isUpdate = true`); không → tạo entity mới (`Id = Guid.NewGuid()`, `CreatedAt = now`) | Tách riêng quyết định "cũ hay mới" — trái tim của logic upsert |
| `ApplyRowToProduct` | Map ~50 cột từ `ExcelRow` (chuỗi thô) vào `Product` entity, dùng đúng parse helper cho từng kiểu | Hàm dài nhất nhưng logic đơn giản nhất — chỉ là ánh xạ 1-1, tách riêng để không làm rối hàm phân loại |
| `PersistBatchAsync` | `AddRange` + `UpdateRange` + `SaveChanges` trong **1 transaction**, lỗi thì rollback | Đảm bảo **all-or-nothing**: hoặc import trọn vẹn, hoặc DB không thay đổi gì — không bao giờ import nửa vời |

### Nhóm parse helpers (chuỗi thô → kiểu dữ liệu)

Mọi ô Excel đọc lên đều là **chuỗi thô**, và dữ liệu thực tế thường "bẩn". Các hàm này parse **an toàn** — không hợp lệ thì trả `null`, không bao giờ làm sập cả tiến trình import:

| Hàm | Xử lý |
|---|---|
| `NullIfEmpty` | Chuỗi rỗng / toàn khoảng trắng / chữ `"null"` → trả `null`; còn lại trả chuỗi đã `Trim()` |
| `ParseDecimal` | Thử parse `InvariantCulture`; thất bại thì thay `,` → `.` rồi thử lại (chịu được cả 2 kiểu dấu thập phân) |
| `ParseInt` | Parse qua `double` rồi `Math.Round` — chịu được Excel ghi `"3.0"` cho số nguyên |
| `ParseDateTimeOffset` | Thử đúng định dạng `"yyyy-MM-dd HH:mm:ss"` (giờ địa phương) trước, fallback parse tổng quát; luôn `.ToUniversalTime()` → DB lưu UTC |

## 4. Cây gọi hàm

```
ImportFromExcel                       [public — điều phối]
├── BufferStreamAsync
├── ValidateFileStructureAsync        → MiniExcel.QueryAsync (raw)
├── ReadRowsAsync                     → MiniExcel.QueryAsync<ExcelRow>
├── BuildUpsertBatchAsync
│   ├── ResolveProduct                → NullIfEmpty
│   └── ApplyRowToProduct             → NullIfEmpty / ParseDecimal / ParseInt / ParseDateTimeOffset
└── PersistBatchAsync                 → transaction (Add/Update/Save/Commit/Rollback)
```

## 5. Điểm mấu chốt cần nhớ

1. **Khóa nghiệp vụ để upsert là `NhanhProductId`** (ID từ Nhanh.vn), không phải `Id` Guid nội bộ.
2. **Chỉ 1 query DB** cho toàn bộ file (Dictionary lookup) → không N+1.
3. **Transaction bọc toàn bộ** → all-or-nothing.
4. **Parse an toàn** — dữ liệu bẩn thành `null`, không crash.
5. **`SourceRowNumber`** = số dòng gốc trên Excel, phục vụ truy vết/debug.

> ⚠️ **Lưu ý đối chiếu với file Excel thực tế:** `Request.ExcelRow` map **theo chỉ số cột** (`ExcelColumnIndex`) và chỉ khai báo **50 cột (index 0–49)**, trong khi file Excel Nhanh.vn thực tế có **62 cột** với thứ tự khác (ví dụ cột index 2 trong file thật là *"Mã sản phẩm cha"*, còn *"Mã vạch"* nằm ở cột E / index 4). Nếu thứ tự cột trong `Request.cs` chưa khớp đúng thứ tự cột file Excel, dữ liệu sẽ bị **map lệch cột**. Nên kiểm tra lại phần này (đối chiếu với `Product_Excel_Entity_Mapping.md`).
