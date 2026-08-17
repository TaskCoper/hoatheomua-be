# Impact analysis - Refactor quan hệ cha-con của Product

Tài liệu được đối chiếu với source hiện tại ngày 2026-08-17. Số dòng là mốc review tại thời điểm này và có thể dịch chuyển sau khi code được sửa.

## 1. Kết luận nhanh

Các điểm phải refactor trực tiếp vì đang xác định cha-con bằng chuỗi nằm chủ yếu trong:

- `HoaTheoMua.Service/Product/Service.cs`: list root, detail parent/children, pin và move.
- `HoaTheoMua.Service/NhanhVnService/Service.cs`: webhook inventory, add và update Product.
- `HoaTheoMua.Service/ProductImport/Service.cs`: import cần tạo/đồng bộ `ParentProductGuid` sau khi upsert.
- Entity, EF configuration, migration, request/response mapping và test Product.

Các query chỉ dùng `NhanhProductId` để đối chiếu với Nhanh.vn/order không phải logic cha-con; chúng nên tiếp tục dùng integration ID và không cần đổi sang Guid.

## 2. API Product và mức độ ảnh hưởng

| API | Controller | Service | Mức độ | Lý do |
|---|---|---|---|---|
| `POST /api/v1/products/nhanh-vn/list` | `ProductController.cs:22-27` | `Product.Service.GetListProducts`, dòng 37 | Không trực tiếp | Proxy/search dữ liệu Nhanh.vn, không query bảng `Products`. |
| `POST /api/v1/products/nhanh-vn/details` | `ProductController.cs:28-33` | `GetProductDetails`, dòng 137 | Không trực tiếp | Lấy detail từ Nhanh.vn, không resolve quan hệ trong DB local. |
| `GET /api/v1/products` | `ProductController.cs:35-40` | `GetLocalProducts`, dòng 211 | **Bắt buộc đổi** | Dòng 226 nhận diện root bằng `ParentProductId` null/rỗng. |
| `GET /api/v2/products` | `ProductController.cs:42-47` | `GetLocalProductsV2`, dòng 308 | **Bắt buộc đổi** | Dòng 323 nhận diện root bằng chuỗi legacy. |
| `GET /api/v1/products/max-price` | `ProductController.cs:49-54` | `GetMaxPrice`, dòng 767 | Không trực tiếp | Có query `Products` nhưng không dùng quan hệ cha-con. Cần xác nhận business có muốn chỉ tính giá root hay cả variant; refactor field không tự thay đổi semantics này. |
| `GET /api/v1/products/{id:guid}` | `ProductController.cs:56-61` | `GetLocalProductDetail`, dòng 439 | **Bắt buộc đổi, ưu tiên cao** | Dòng 454-473 chạy query cha/con qua phép nối chuỗi `NhanhProductId` ↔ `ParentProductId`. |
| `PUT /api/v1/products/{id:guid}/move` | `ProductController.cs:63-75` | `MoveProduct`, dòng 785 | **Bắt buộc đổi** | Dòng 804-812 và 824-828 chỉ cho root dựa trên chuỗi legacy. |
| `PUT /api/v1/products/pin` | `ProductController.cs:77-89` | `PinProduct`, dòng 845 | **Bắt buộc đổi** | Dòng 862-865, 876-881 và 890-894 kiểm tra/count root bằng chuỗi legacy. |
| `GET /api/v1/products-pin` | `ProductController.cs:91-97` | `GetProductPin`, dòng 934 | **Bắt buộc đổi** | Dòng 948-955 và 976-980 lọc root bằng chuỗi; hai nhánh còn đang không thống nhất về case chuỗi rỗng. |
| `PATCH /api/v1/products/{id:guid}/update` | `ProductController.cs:99-105` | `UpdateProduct`, dòng 1001 | **Bắt buộc bảo vệ tính nhất quán** | DTO cho sửa `ParentProductId`, AutoMapper map thẳng nhưng không cập nhật/validate `ParentProductGuid`. |
| `POST /api/v1/products/import-excel` | `ProductImportController.cs:17-22` | `ProductImport.Service.ImportFromExcel`, dòng 20 | **Bắt buộc đổi ở task import sau** | Writer lớn nhất; nếu không dual-write thì dữ liệu Guid stale ngay sau backfill. |
| `POST /api/v1/nhanh-vn/webhook` | `NhanhVnController.cs:45-52` | `NhanhVnService.HandleWebhook`, dòng 127 | **Bắt buộc đổi các event Product** | Một endpoint dispatch inventory/product add/product update/product delete. Chi tiết bên dưới. |

## 3. Chi tiết từng hàm bị ảnh hưởng

### 3.1 Entity và EF Core

#### `HoaTheoMua.Repository/Entity/Product.cs`

- Dòng 9-12: vùng field integration/parent hiện tại; thêm `Guid? ParentProductGuid` tại đây.
- Dòng 89-91: vùng navigation; thêm `ParentProduct` và `ChildProducts`.
- Không xóa `NhanhProductId` dòng 9 hoặc `ParentProductId` dòng 10 trong giai đoạn chuyển đổi.

#### `HoaTheoMua.Repository/Configurations/ProductConfiguration.cs`

- Dòng 13-18: giữ unique index `NhanhProductId`; nó vẫn cần cho upsert và đối soát Nhanh.vn.
- Dòng 20-21: giữ cấu hình field legacy trong giai đoạn chuyển đổi.
- Sau dòng 21: thêm index `ParentProductGuid` và self-reference FK tới `Product.Id`, delete behavior `Restrict/NoAction`.

#### Migration

- Tạo migration mới; không sửa migration cũ đã áp dụng.
- File migration mới cần add column, backfill, index, FK và rollback đúng thứ tự.
- `HoaTheoMua.Repository/Migrations/AppDbContextModelSnapshot.cs` sẽ được EF cập nhật; không sửa tay nếu không có lý do đặc biệt.

### 3.2 `HoaTheoMua.Service/Product/Service.cs`

#### `GetLocalProducts` - dòng 211-306

- Dòng 226: đổi điều kiện root từ `(ParentProductId == null || ParentProductId == string.Empty)` thành `ParentProductGuid == null` sau khi cutover.
- Dòng 235 vẫn tìm theo `NhanhProductId`; đây là search integration ID, không phải traversal, nên giữ nguyên.

#### `GetLocalProductsV2` - dòng 308-420

- Dòng 323: đổi root predicate sang `ParentProductGuid == null`.
- Dòng 332 vẫn có thể giữ search theo `NhanhProductId`.

#### `GetLocalProductDetail` - dòng 439-477

- Dòng 454-463: bỏ lookup parent bằng `response.ParentProductId` đối chiếu `p.NhanhProductId`.
- Dòng 465-473: bỏ lookup children bằng `response.NhanhProductId` đối chiếu `p.ParentProductId`.
- Hướng mới: query theo `p.Id == response.ParentProductGuid` và `p.ParentProductGuid == response.Id`, hoặc dùng navigation `ParentProduct`/`ChildProducts` trong một projection được kiểm soát.
- Vẫn phải lọc `!IsDeleted` cho parent và children vì dự án không có global query filter.
- Tránh project đệ quy toàn bộ `LocalProductDetailResponse`; chỉ project parent/children một cấp để không tạo graph vòng khi serialize.

#### `LocalProductDetailSelector` - dòng 479 trở đi

- Dòng 485-486: hiện chỉ map hai integration ID. Nếu API cần công khai khóa quan hệ mới, thêm `ParentProductGuid` vào response và selector.
- Việc giữ `ParentProductId` trong response để tương thích client là độc lập với việc traversal chuyển sang Guid.

#### `MapToProductDetailResponse` - dòng 623-663

- Dòng 630-631: tương tự selector trên; thêm `ParentProductGuid` nếu contract cần trả field này.
- Không bắt buộc xóa hai field legacy khỏi response ngay.

#### `MoveProduct` - dòng 785-843

- Dòng 804-812: nhánh theo category phải xác nhận root bằng `x.Product.ParentProductGuid == null`.
- Dòng 824-828: nhánh global phải xác nhận root bằng `x.ParentProductGuid == null`.

#### `PinProduct` - dòng 845-932

- Dòng 862-865: kiểm tra product là root bằng `ParentProductGuid == null`.
- Dòng 876-881: count pinned root trong category bằng field mới.
- Dòng 890-894: count pinned root global bằng field mới.
- Sau refactor không còn special case chuỗi rỗng.

#### `GetProductPin` - dòng 934-999

- Dòng 948-955: nhánh category đổi sang `x.Product.ParentProductGuid == null`.
- Dòng 976-980: nhánh global đổi sang `x.ParentProductGuid == null`.
- Đây cũng sửa được inconsistency hiện tại: nhánh category chấp nhận null/rỗng, nhánh global dòng 980 chỉ chấp nhận null.

#### `UpdateProduct` - dòng 1001-1018

- Dòng 1011 gọi AutoMapper cho toàn request. Nếu vẫn cho sửa `ParentProductId`, cần resolve parent tương ứng và đồng bộ `ParentProductGuid` trong cùng transaction.
- Validate parent tồn tại, không self-reference và không tạo cycle.
- Phương án an toàn hơn: không cho endpoint generic sửa `NhanhProductId`, `ParentProductId`, `ParentProductGuid`; tạo command riêng để đổi parent.

### 3.3 DTO và mapping Product

#### `HoaTheoMua.Service/Product/Request.cs`

- `UpdateProductRequest`, dòng 117-190; đặc biệt dòng 120-123.
- Nếu thêm `ParentProductGuid` vào request, không được tin trực tiếp cả Guid và legacy ID cùng lúc mà thiếu validation.
- Khuyến nghị bỏ các field quan hệ khỏi generic update DTO hoặc chỉ nhận một nguồn duy nhất.

#### `HoaTheoMua.Service/Product/ProductMapping.cs`

- Dòng 8-10 map mọi member non-null. Mapping hiện tại có thể làm `ParentProductId` thay đổi nhưng để `ParentProductGuid` giữ giá trị cũ.
- Cần ignore field quan hệ rồi xử lý có chủ đích trong service, hoặc dùng resolver có validation.

#### `HoaTheoMua.Service/Product/Response.cs`

- `ProductDetailResponse`: dòng 9-17, đặc biệt dòng 14-15.
- `LocalProductDetailResponse`: dòng 99-107, đặc biệt dòng 104-105.
- Dòng 175-176 chứa `Parent` và `Children`; khi thêm navigation ở entity, tuyệt đối không serialize trực tiếp entity graph để tránh reference loop. Các response DTO hiện tại nên tiếp tục được dùng.

### 3.4 Import Excel - để triển khai ở task sau

#### `HoaTheoMua.Service/ProductImport/Request.cs`

- Dòng 15-19 map cột Nhanh ID và parent Nhanh ID. Giữ nguyên vì file nguồn không có internal Guid.

#### `HoaTheoMua.Service/ProductImport/Service.cs`

- `ImportFromExcel`, dòng 20-46: cần thêm phase resolve quan hệ sau khi mọi row đã có internal `Id`.
- `BuildUpsertBatchAsync`, dòng 107-140: existing map hiện chỉ phục vụ upsert theo `NhanhProductId`. Cần thêm map gồm cả existing và newly-created product để resolve parent không phụ thuộc thứ tự dòng Excel.
- `ResolveProduct`, dòng 143-162: vẫn upsert theo integration ID; logic này không nên đổi sang `ParentProductGuid`.
- `ApplyRowToProduct`, dòng 165-239; dòng 170-173 gán field legacy. Không nên resolve parent trong hàm map từng row vì parent có thể nằm ở dòng sau.
- `PersistBatchAsync`, dòng 242-261: trước khi save phải bảo đảm tất cả entity trong batch đã được gán `ParentProductGuid`; transaction hiện tại nên được giữ.
- `NullIfEmpty`, dòng 266-271 đã trim và đổi chuỗi rỗng/`null` thành null; nên tái sử dụng cùng quy tắc khi build parent lookup.

Luồng đề xuất cho task import:

```text
Read rows
  -> resolve/create toàn bộ Product và cấp Id
  -> tạo map NhanhProductId -> Product cho existing + insert + update
  -> apply scalar fields
  -> resolve ParentProductGuid cho toàn batch
  -> validate orphan/self/cycle
  -> save một transaction
```

### 3.5 Nhanh.vn webhook - writer dễ bị bỏ sót

#### `WebhookNhanhVnInventory` - `NhanhVnService/Service.cs:249-318`

- Dòng 274-276 query parent bằng `NhanhProductId == product.ParentProductId`; đổi sang lookup bằng `product.ParentProductGuid` hoặc navigation.
- Query lấy product theo incoming Nhanh ID ở dòng 256-267 vẫn đúng và nên giữ.
- Cần xem lại phép cộng tồn kho parent ở dòng 282-293 để bảo đảm webhook retry không cộng lặp; đây là vấn đề hiện hữu, ngoài phạm vi field Guid nhưng đáng test khi chạm hàm.

#### `WebhookNhanhVnProductAdd` - dòng 320-461

- Dòng 330-346: sau khi tạo product và đọc `item.ParentId`, resolve parent local để gán `ParentProductGuid`.
- Phải hỗ trợ tình huống child event tới trước parent. Các lựa chọn: để null và chạy reconciliation khi parent được add; retry; hoặc lưu pending relationship. Không được coi child này là root vĩnh viễn.

#### `WebhookNhanhVnProductUpdate` - dòng 759-895

- Dòng 769-779: khi Nhanh thay `ParentId`, phải cập nhật đồng thời legacy `ParentProductId` và `ParentProductGuid`.
- Nếu `ParentId <= 0`, đặt cả hai về null.
- Validate self-reference/cycle trước save.

#### `WebhookNhanhVnProductDelete` - dòng 897 trở đi

- Dòng 906-907 chỉ tìm product theo incoming Nhanh ID, không phải traversal nên không cần đổi lookup sang Guid.
- Tuy nhiên với FK `Restrict`, cần xác định hành vi khi soft-delete parent có child. Hiện soft delete không vi phạm FK, nhưng API đọc phải tiếp tục lọc `IsDeleted` ở navigation.

### 3.6 Test bị ảnh hưởng

#### `HoaTheoMua.Service.Test/ProductTest/PinAndMoveParentOnlyTest.cs`

- Dòng 64, 97, 126: fixture child phải tạo parent thật và gán `ParentProductGuid = parent.Id`, thay vì chỉ dùng chuỗi giả `"parent-1"`.
- Test dòng 141-161 và 164-185 về `ParentProductId = ""` trở thành test migration/legacy compatibility, không còn là tiêu chí nhận diện root sau cutover.
- Bổ sung test root với `ParentProductGuid = null`, child với Guid khác null, orphan Guid bị FK chặn và soft-deleted parent.

#### `HoaTheoMua.Service.Test/ProductTest/LocalProductSearchTest.cs`

- Dòng 295-336 dựng parent/children bằng `NhanhProductId`/`ParentProductId`; thêm `ParentProductGuid` cho các child.
- Assertion dòng 352-360 vẫn nên xác nhận đúng parent và children theo internal Guid; có thể giữ assertion `NhanhProductId` để kiểm tra response compatibility.

## 4. Các nơi có truy cập `Products` nhưng không cần đổi sang Guid

Không nên thay mọi `NhanhProductId` bằng Guid. Các điểm sau đang dùng đúng vai trò integration key:

- `Order/Service.cs`, `GetOrderById`, dòng 495-505: map `OrderDetails.ItemId` từ Nhanh.vn sang Product local để lấy ảnh/Guid. Endpoint liên quan là `GET /api/v1/orders/{id}` tại `OrderController.cs:52-57`.
- `Webhook/Service.cs`, đoạn dòng 754-764 trong flow tạo đơn Nhanh/revalidate: đổi Nhanh item IDs sang local Product IDs. Được gọi gián tiếp từ payment webhook; không traversal cha-con.
- `NhanhVnService/Service.cs`, dòng 330-331, 769-770 và 906-907: tìm chính product nhận event bằng Nhanh ID. Giữ nguyên.
- `NhanhVnService/Service.cs`, dòng 259-267: tìm product nhận inventory event bằng incoming Nhanh ID. Giữ nguyên; chỉ parent lookup dòng 274-276 cần đổi.
- `CategoryProduct/Service.cs`, các query `Products` ở dòng 155, 296 và 481 chỉ kiểm tra active/existence theo local `Product.Id`, không dùng quan hệ cha-con.

## 5. Thứ tự triển khai đề xuất

1. Task 1: thêm nullable column, index, FK và backfill dữ liệu hiện có.
2. Cập nhật writer theo thứ tự: Excel import, product add/update webhook, generic update endpoint.
3. Chạy reconciliation/backfill lần cuối và xử lý toàn bộ orphan.
4. Chuyển read-path: detail trước, list root, sau đó pin/move.
5. Cập nhật test và theo dõi số dòng `ParentProductId` có giá trị nhưng `ParentProductGuid` null.
6. Khi đã ổn định mới cân nhắc deprecate field legacy khỏi public request; vẫn nên giữ Nhanh IDs trong DB để tích hợp.

## 6. Checklist khi refactor từng hàm

- [ ] Không còn join/traversal `ParentProductId == NhanhProductId`.
- [ ] Lookup cha dùng `ParentProductGuid -> Product.Id`.
- [ ] Lookup con dùng `Product.ParentProductGuid == parent.Id`.
- [ ] Root dùng duy nhất `ParentProductGuid == null`.
- [ ] Mọi writer cập nhật legacy ID và Guid nhất quán trong giai đoạn chuyển đổi.
- [ ] Mọi query navigation vẫn lọc `IsDeleted` thủ công.
- [ ] Không serialize trực tiếp self-referencing entity graph.
- [ ] Có test orphan, self-reference, cycle, child-before-parent và soft-deleted parent.

