# 01 — Domain, dữ liệu và lineage

## 1. Các record chính

| Record | Ý nghĩa trong flow |
|---|---|
| `Product` | Sản phẩm đang bán. Combo cũng là một Product, phân biệt bằng `ProductType = Combo`. |
| `ComboSpecifications` | Danh sách vật liệu của Combo, gồm Core/Support và số lượng cần dùng. |
| `GeneratedFlower` | Một ảnh hoa do AI sinh ra. Nó là ảnh thiết kế, không phải một Product mới. |
| `GeneratedCard` | Một thiệp đã được tạo thành công, bằng AI hoặc HandMade. |
| `ClientHistory` | Dòng truy vết: nguồn nào đã tạo ra kết quả nào. |
| `OrderDetails` | Một dòng Product/Combo trong Order; lưu ảnh hoa đã chọn, ID Card tạo được và snapshot phục vụ đơn hàng. |

## 2. ClientHistory là cầu nối

`ClientHistory` không chỉ là màn hình lịch sử. Nó là cách BE biết một ảnh/thiệp được tạo từ đâu.

| Field | Ý nghĩa |
|---|---|
| `Type` | `flower`, `card`, `handmade_card`… xác định loại output. |
| `BaseId` | Nguồn **trực tiếp** đã sinh ra output. |
| `OutputId` | ID kết quả đã sinh ra: `GeneratedFlower.Id` hoặc `GeneratedCard.Id`. |
| `RootID` | Combo/Product nguồn ban đầu, dùng để gom tất cả kết quả chung gốc và checkout ảnh hoa. |
| `Metadata` | Snapshot/provenance cần giữ lại, nhất là nội dung tạo và quan hệ bổ sung. |

`OutputId` là ID để join chính xác sang bảng Generated. Không được thay bằng `BaseId`.

## 3. Lineage Flower đã chốt

### Generate lần đầu

```text
Combo CA → GF1

ClientHistory:
Type     = flower
RootID   = CA
BaseId   = CA
OutputId = GF1
```

### Regenerate liên tiếp

```text
Combo CA → GF1 → GF2 → GF3

GF2: RootID = CA, BaseId = GF1, OutputId = GF2
GF3: RootID = CA, BaseId = GF2, OutputId = GF3
```

Regenerate luôn phải đi từ ảnh mới nhất của chính phiên đó. Không có nhánh kiểu `GF1 → GF4` nếu `GF1` đã sinh `GF2`; một lượt tạo mới cùng Combo là phiên mới:

```text
Combo CA → GF4

GF4: RootID = CA, BaseId = CA, OutputId = GF4
```

Vì vậy:

- `RootID = CA` gom được tất cả `GF1`, `GF2`, `GF3`, `GF4` theo Combo.
- `BaseId` nối được đúng một phiên: `GF1 → GF2 → GF3` và tách khỏi `GF4`.

## 4. Cách lấy một phiên Flower khi FE gửi bất kỳ ảnh trong chuỗi

Ví dụ FE gửi `GF2` trong chuỗi `GF1 → GF2 → GF3`:

```text
1. Từ GF2 đi ngược qua BaseId để lấy GF1.
2. Đảo danh sách phần trước để thứ tự là GF1, GF2.
3. Từ GF2 đi xuôi: tìm record có BaseId = GF2 để lấy GF3.
4. Trả GF1, GF2, GF3; không trả GF4.
```

`HashSet visited` trong code chỉ để phát hiện dữ liệu vòng/lặp bất thường. `visited.Add(id)` trả `true` và tự thêm ID nếu ID chưa có; trả `false` nếu ID đã đi qua.

## 5. Lineage Card

Card AI mới có thể lấy nguồn từ Product hoặc GeneratedFlower. Card tạo lại lấy nguồn từ Card AI cũ.

```text
Card AI mới từ Combo
→ Card History: BaseId = RootID = ComboId, OutputId = GeneratedCardId

Card AI mới từ GeneratedFlower
→ Card History: BaseId = GeneratedFlowerId,
                 RootID = ComboId gốc của Flower,
                 OutputId = GeneratedCardId

Card AI tạo lại từ GC1
→ Card mới có GeneratedCardId mới
→ History/metadata lưu regeneratedFromCardId = GC1
→ Card cũ GC1 không bị sửa.
```

`regeneratedFromCardId` có hai ý nghĩa:

- `null`: tạo Card AI mới từ Product hoặc ảnh hoa AI.
- Có giá trị: FE đã chọn một `GeneratedCard.Id` AI cũ từ History để tạo Card mới dựa trên nó.

Nó không phải ID của Card sắp tạo và không dùng để ghi đè Card cũ.

## 6. Quan hệ với OrderDetails

Thiết kế đã chốt dùng các field riêng trong `OrderDetails`:

```text
GeneratedFlowerId   : Guid?      — ảnh hoa AI khách đã chọn cho dòng hàng
GeneratedCardIds    : Guid[]    — các Card tạo thành công cho dòng hàng
SnapshotCard        : JSON      — mảng snapshot Card đã chốt khi tạo Order
```

`GeneratedFlowerId` là một cột đơn, vì một `OrderDetails` gắn với một ảnh hoa AI đã chọn. `GeneratedCardIds` là mảng vì một dòng đặt nhiều bó hoa có thể có nhiều thiệp khác nội dung/mẫu.

Không dùng `OrderDetails.ProductId = comboId` để suy ra ảnh hoa đã dùng. Muốn biết một Order đã dùng ảnh nào, phải đọc `OrderDetails.GeneratedFlowerId`.

