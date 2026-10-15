# Kế hoạch tối ưu truy vấn sản phẩm

Ngày lập: 15/10/2026. Trạng thái: **đề xuất để review, chưa triển khai thay đổi code**.

> Cập nhật sau rà soát: theo yêu cầu riêng, đã đổi nguồn SellableQuantity của sản phẩm thường từ Stock sang AvailableForSale (null/âm → 0, không fallback Stock), gồm cha không có biến thể combo và con không phải combo. Các projection tính kho dùng AvailableForSale. Công thức combo vẫn ưu tiên AvailableForSale của nguyên liệu và fallback Stock khi null. Những mô tả Stock cho sản phẩm thường trong phần baseline bên dưới ghi nhận thời điểm trước thay đổi này; kế hoạch tối ưu hiệu năng vẫn chưa triển khai.

## 1. Mục tiêu và phạm vi

Giảm thời gian phản hồi API sản phẩm bằng cách giảm dữ liệu tải thừa, lượng dữ liệu lặp do JOIN và các lượt truy vấn trung gian. Giữ nguyên bộ lọc, quyền truy cập, nội dung response, cách tính tồn kho và ý nghĩa phân trang.

Ba hàm chính trong [Service.cs](../HoaTheoMua.Service/Product/Service.cs):

- `GetLocalProducts(LocalProductQueryRequest request)` — gọi tắt là V1.
- `GetLocalProductsV2(LocalProductQueryRequest request, ProductHierarchyType? productHierarchy)` — gọi tắt là V2.
- `GetLocalProductDetail(Guid id)` — chi tiết sản phẩm.

Các thành phần liên quan: `CalculateAvailableQty`, `ProductQtyProjection`, `GetQuantityInfo`, `MapToProductDetailResponse`, `LocalProductDetailSelector`, cấu hình index và các test sản phẩm.

**Giới hạn của tài liệu:** luồng hiện tại được xác nhận từ code repository. Chưa đo production, chưa có execution plan hoặc benchmark trên dữ liệu thực. Mọi dung lượng và thời gian minh họa bên dưới đều là giả định, không phải cam kết cải thiện.

## 2. Những tối ưu hiện đã có

- Các truy vấn đọc dùng `AsNoTracking`.
- V1 ở nhánh không sort tồn kho và V2 đã phân trang tại DB; chỉ tính tồn kho cho trang trả về.
- `CalculateAvailableQty` tải dữ liệu theo lô; không có truy vấn riêng trong vòng lặp cho từng combo.
- Hàm detail dùng projection và truyền `variantsArePreloaded` để tái sử dụng biến thể đã tải.
- Model có index `NhanhProductId`, `(ParentProductId, ProductType)`, `(Position, CreatedAt)` và `(SubId, MatId)` của công thức. Cần xác nhận các migration tương ứng đã được áp dụng trên production.

Vì vậy không nên mặc định rằng cả ba hàm đều tải toàn bộ bảng hoặc đang gặp N+1 query.

## 3. Quy ước phân tích độ phức tạp

| Ký hiệu | Ý nghĩa |
|---|---|
| N | Số sản phẩm khớp bộ lọc trước phân trang |
| P | Số sản phẩm trong một trang, hiện giới hạn tối đa 100 |
| A | Số sản phẩm cha khớp bộ lọc trong V2 `All` |
| B | Tổng số sản phẩm con của các cha đó |
| V | Số biến thể combo cần tính tồn kho |
| S | Số dòng công thức active, chưa xóa được tải để tính tồn kho |
| M | Số nguyên liệu CORE khác nhau cần đọc kho |
| C | Số liên kết danh mục trung bình trên một sản phẩm |
| R | Số dòng nguyên liệu trung bình trên một sản phẩm |
| H | Số danh mục con trung bình trên mỗi danh mục |
| W | Dung lượng trung bình của một sản phẩm đầy đủ cùng dữ liệu quan hệ |
| w | Dung lượng projection tối thiểu để tính tồn kho |
| L | Độ trễ đi/về của một lượt ứng dụng gọi DB |

Big-O bên dưới chủ yếu mô tả xử lý và bộ nhớ phía ứng dụng. Chi phí DB phụ thuộc số dòng quét, độ chọn lọc của bộ lọc, index, cache, cấu hình và execution plan. Không thể suy ra rằng một query có index luôn chạy O(log N), hoặc hai query luôn nhanh hơn bốn query.

## 4. V1: sort theo `sellableQuantity`

### 4.1. Luồng hiện tại

```text
Chuẩn hóa request, tạo bộ lọc
  → COUNT sản phẩm khớp bộ lọc
  → Tải TOÀN BỘ sản phẩm khớp bộ lọc
      + CategoryProductDetails → CategoryProduct
      + SubIds → Mat
  → Chuyển toàn bộ sản phẩm sang ProductQtyProjection
  → CalculateAvailableQty cho toàn bộ tập kết quả
  → Sắp xếp theo SellableQuantity trong ứng dụng
  → Skip/Take lấy trang yêu cầu
  → Map response và gán thông tin tồn kho
```

Điểm tốn tài nguyên: mô tả, ảnh và dữ liệu quan hệ được tải cho cả những sản phẩm không xuất hiện trên trang. JOIN danh mục với nguyên liệu còn có thể làm lặp nhiều lần các cột của sản phẩm.

### 4.2. Luồng đề xuất

```text
Chuẩn hóa request, tạo bộ lọc
  → COUNT sản phẩm khớp bộ lọc
  → Chỉ SELECT Id, ProductType, ParentProductId, NhanhProductId, Stock
  → CalculateAvailableQty cho toàn bộ tập kết quả
  → Sắp xếp theo SellableQuantity
  → Lấy danh sách ID đúng thứ tự của trang
  → Tải thông tin đầy đủ và quan hệ cho đúng các ID đó
  → Khôi phục thứ tự theo danh sách ID đã chọn
  → Map response và gán thông tin tồn kho đã tính
```

Không được phân trang trước khi tính tồn kho: nếu chỉ tính cho 20 sản phẩm đầu tiên, kết quả sẽ không còn là 20 sản phẩm có tồn kho cao nhất/thấp nhất của toàn bộ tập lọc.

Truy vấn `WHERE Id IN (...)` không đảm bảo thứ tự. Phải dựng dictionary theo ID và đọc lại theo danh sách ID đã sắp xếp. Với các sản phẩm bằng tồn kho, cần thống nhất tiêu chí phụ ổn định; hiện code không quy định rõ thứ tự ở nhánh này.

### 4.3. Ví dụ khối lượng dữ liệu

Giả định N = 10.000, P = 20, W = 5 KB, w = 150 byte; dùng đơn vị thập phân để dễ tính.

| Chỉ tiêu | Trước | Sau |
|---|---:|---:|
| Sản phẩm cần tính tồn kho | 10.000 | 10.000 |
| Sản phẩm tải đầy đủ | 10.000 | 20 |
| Dữ liệu sản phẩm đầy đủ | 50 MB | 0,1 MB |
| Projection tối thiểu tải riêng | Không | 1,5 MB |
| Tổng phần dữ liệu sản phẩm minh họa | 50 MB | 1,6 MB |

Phần này giảm khoảng 96,8%. Đây là ước lượng tải dữ liệu logic, không phải số byte đo trên mạng hoặc lượng managed heap chính xác. Chưa tính dữ liệu công thức/kho vẫn cần ở cả hai phương án và phần lặp do SQL JOIN.

### 4.4. Ví dụ thời gian giả định

| Công đoạn | Trước | Sau |
|---|---:|---:|
| Tải và dựng dữ liệu sản phẩm ban đầu | 900 ms | 100 ms |
| Tải công thức/kho, tính tồn kho, sắp xếp | 300 ms | 300 ms |
| Tải đầy đủ sản phẩm của trang | Đã nằm trong dòng đầu | 50 ms |
| Tổng các công đoạn này | 1.200 ms | 450 ms |

Trong kịch bản giả định này, giảm 750 ms, tương đương 62,5%, hoặc thời gian cũ bằng khoảng 2,67 lần thời gian mới. Chưa bao gồm COUNT, serialize response, middleware và truyền HTTP tới khách hàng.

Nếu tải sản phẩm chỉ chiếm 10% tổng thời gian API thì dù giảm 90% phần đó, tổng thời gian cũng chỉ giảm khoảng 9%. Không thể lấy mức giảm dữ liệu 96,8% làm mức giảm thời gian API.

### 4.5. Độ phức tạp

| Thành phần | Trước | Sau |
|---|---|---|
| Sắp xếp trong ứng dụng, xét sort đầy đủ | O(N log N) | O(N log N) |
| Tính tồn kho | O(N + V + S + M) | O(N + V + S + M) |
| Dữ liệu sản phẩm cần giữ | O(N × W) | O(N × w + P × W) |
| Map response cuối cùng | Theo dữ liệu quan hệ của P sản phẩm | Tương tự |
| Tải đầy đủ entity | N sản phẩm | P sản phẩm |

O(N log N) là cách ước lượng bảo thủ cho sắp xếp; triển khai LINQ/runtime có thể tối ưu một số thao tác lấy một phần kết quả. Đề xuất này không dựa vào việc đổi thuật toán sort.

Nếu xem W và w là hằng số, bộ nhớ vẫn cùng bậc O(N). Cải thiện nằm ở hệ số rất lớn của object đầy đủ và dữ liệu quan hệ, không phải từ O(N) xuống O(P).

## 5. V1: danh sách thông thường, không sort tồn kho

### Trước

```text
Lọc → COUNT → ORDER BY + Skip/Take tại DB
  → Tải sản phẩm của trang kèm danh mục và nguyên liệu
  → Tính tồn kho theo lô cho trang → Map response
```

### Sau đề xuất

Giữ nguyên phân trang và tính tồn kho theo trang. Đo hai phương án tải quan hệ:

1. Giữ JOIN hiện tại nếu ít dữ liệu quan hệ và độ trễ DB cao.
2. Tải sản phẩm, danh mục và nguyên liệu theo các lô riêng, giới hạn trong trang, nếu JOIN tạo quá nhiều dòng lặp.

Ví dụ P = 20, C = 4, R = 6, giả định mỗi sản phẩm đều có đủ các quan hệ này:

| Phương án | Số dòng logic minh họa |
|---|---:|
| JOIN hai collection cùng lúc | 20 × 4 × 6 = 480 |
| Tải các lô riêng | 20 + 80 + 120 = 220 |

Nếu riêng các cột sản phẩm chiếm 4 KB, JOIN có thể lặp phần này khoảng 480 × 4 KB = 1,92 MB; tải riêng chỉ cần khoảng 20 × 4 KB = 80 KB cho phần sản phẩm. Các cột quan hệ, khóa JOIN và chi phí giao thức cần tính thêm.

Khối lượng dựng graph có thể chuyển từ tỷ lệ O(P × C × R) dòng SQL sang O(P + P × C + P × R). Nếu coi C và R là hằng số, cả hai vẫn là O(P).

Đánh đổi: một lượt JOIN có thể thành ba lượt tải. Với L = 20 ms, hai lượt tăng thêm mất khoảng 40 ms chỉ cho độ trễ. Nếu tiết kiệm truyền dữ liệu và dựng object dưới 40 ms thì tách query có thể chậm hơn.

Không bật `AsSplitQuery` đại trà. Nếu dùng split query cùng phân trang, cần thứ tự duy nhất, chẳng hạn thêm `Id` cuối ORDER BY, để các truy vấn thành phần chọn nhất quán khi nhiều sản phẩm bằng Position/CreatedAt. Việc tách truy vấn cũng cần xem xét dữ liệu thay đổi giữa các lượt đọc.

## 6. V2: `productHierarchy = All`

### 6.1. Luồng hiện tại

```text
Lọc sản phẩm cha
  → Query 1: tải ID và mã Nhanh của toàn bộ cha khớp bộ lọc
  → Query 2: truyền mã cha lên DB, tải ID của các con chưa xóa
  → Ghép và loại trùng ID cha/con trong ứng dụng
  → Query 3: truyền danh sách ID lên DB để COUNT
  → Query 4: truyền danh sách ID lên DB để sắp xếp và lấy trang
  → Tải quan hệ danh mục cùng query trang
  → Tính tồn kho cho trang → Map response
```

Query lấy con được bỏ qua nếu không có mã cha hợp lệ. Số bốn lượt ở trên áp dụng khi có cha/con cần mở rộng, chưa tính truy vấn tồn kho.

### 6.2. Luồng đề xuất

```text
Xây dựng query cha khớp bộ lọc, chưa thực thi
  → Xây dựng điều kiện SQL:
      sản phẩm thuộc tập cha đã lọc
      HOẶC là con chưa xóa của một cha có mã hợp lệ trong tập đó
  → Query 1: COUNT bằng điều kiện trên
  → Query 2: ORDER BY + phân trang bằng điều kiện trên
  → Tải danh mục và tính tồn kho cho trang → Map response
```

Dùng subquery `Contains`/`Any` trên IQueryable để DB xử lý quan hệ. Không materialize toàn bộ ID cha/con thành List trước khi phân trang.

Phải giữ đúng nghiệp vụ hiện tại: bộ lọc áp dụng để chọn CHA trước; các con được lấy theo cha đã chọn. Không tự ý áp dụng lại điều kiện giá, trạng thái hoặc từ khóa của cha lên từng con.

### 6.3. Ví dụ số liệu và độ phức tạp

Giả định A = 2.000, B = 8.000, P = 20.

| Chỉ tiêu | Trước | Sau |
|---|---:|---:|
| ID cha/con trung gian tải về ứng dụng | 10.000 | 0 |
| Lượt query chọn ID, đếm, lấy trang | 4 | 2 |
| Danh sách ID lớn gửi lại DB | Có | Không |
| Bộ nhớ ứng dụng riêng cho bước mở rộng ID | O(A + B) | Không materialize tập ID |
| Xử lý ghép/loại trùng ID trong ứng dụng | O(A + B), trung bình với hashing | Loại bỏ bước này |

10.000 Guid có phần giá trị thô khoảng 160 KB, chưa tính mã Nhanh, các List/HashSet, object trung gian và giao thức. Lợi ích RAM ở nhánh này không nên mô tả như mức tiết kiệm 50 MB của V1 vì hai nhánh tải dữ liệu khác nhau.

Nếu L = 20 ms thì bỏ hai lượt query tiết kiệm khoảng 40 ms độ trễ. Ví dụ tổng thời gian DB xử lý và truyền dữ liệu của bốn query cũ là 160 ms, hai query mới là 120 ms: tổng phần này từ 4 × 20 + 160 = 240 ms xuống 2 × 20 + 120 = 160 ms, giảm khoảng 33%.

Những con số đó chỉ là giả định. DB vẫn phải duyệt/tìm tập cha/con, và subquery xuất hiện trong cả COUNT lẫn query trang. Nếu plan mới xử lý kém, thời gian có thể không giảm. Cần so sánh execution plan; có thể thử EXISTS, IN hoặc UNION tùy SQL thực tế.

## 7. V2: danh sách và quan hệ danh mục

V2 hiện đã phân trang trước khi tính tồn kho. V2 không có nhánh tính toàn bộ tồn kho để sort `sellableQuantity` như V1; không được coi đợt tối ưu này là bổ sung tính năng sort đó.

Query trang hiện tải danh mục, danh mục cha và danh mục con. Với P sản phẩm, C danh mục/sản phẩm, H danh mục con/danh mục, số dòng join có thể tỷ lệ O(P × C × max(1, H)).

Đề xuất: đo lượng dữ liệu lặp, rồi chọn giữ JOIN, split query hoặc projection/tải danh mục theo lô. Giữ nguyên `Parent`, `children` và cờ pin của danh mục trong response. Không bỏ trường response để đổi lấy tốc độ nếu chưa thống nhất với frontend.

Ví dụ P = 20, C = 4, H = 5 tạo khoảng 400 dòng JOIN. Tải theo lô giúp tránh lặp các cột sản phẩm trên từng dòng; tổng số dòng của các lô không nhất thiết giảm, nhưng mỗi dòng có thể nhỏ hơn nhiều.

## 8. `GetLocalProductDetail`

### 8.1. Luồng hiện tại

```text
Kiểm tra role Admin
  → Query sản phẩm chính bằng ID, điều kiện xóa/active/quyền xem
      + projection danh mục và cây danh mục một cấp
  → Không tìm thấy: trả PRODUCT_NOT_FOUND
  → Query nguyên liệu của sản phẩm chính
  → Nếu đang xem biến thể: query ID sản phẩm cha
  → Nếu đang xem cha có mã hợp lệ: query danh sách con cùng projection
  → Gán nguyên liệu cho từng con, nhân Quantity theo Factor
  → Tính tồn kho theo lô, tái sử dụng con đã tải
  → Gán tồn kho cho cha/con → trả response
```

Không có query DB nằm trong vòng lặp gán nguyên liệu cho từng con. Chi phí vòng lặp này thuộc CPU và bộ nhớ của response.

### 8.2. Hướng tối ưu có điều kiện

1. Đo riêng query sản phẩm chính, query biến thể, query nguyên liệu và các query tính tồn kho.
2. Nếu projection danh mục gây lặp dữ liệu lớn, thử tải phần danh mục riêng theo lô cho tập cha/con.
3. Kiểm tra dữ liệu nguyên liệu đã có có đủ dùng cho tính tồn kho không. Hiện response nguyên liệu không chứa đủ `IsCore`, `IsActive`, `AvailableForSale`; không thể dùng thẳng thay dữ liệu tính tồn kho.
4. Chỉ gộp hoặc tái sử dụng dữ liệu nếu giữ được điều kiện lọc hiện tại của từng luồng.

Không chạy `Task.WhenAll` cho các query dùng chung `_dbContext`. Tăng song song bằng context riêng sẽ tăng tải kết nối và không phải bước ưu tiên trong đề xuất này.

### 8.3. Ví dụ và độ phức tạp

Giả định một cha có 12 biến thể, 8 nguyên liệu; cha và mỗi biến thể có 3 danh mục, mỗi danh mục có 4 con.

- Cha + biến thể: 13 sản phẩm.
- Dữ liệu JOIN danh mục có thể tương ứng khoảng 13 × 3 × 4 = 156 dòng.
- Riêng danh sách nguyên liệu của biến thể vẫn cần tạo 12 × 8 = 96 phần tử vì response hiện tại trả nguyên liệu cho từng con.
- Tách tải danh mục có thể giảm lặp thông tin sản phẩm nhưng không làm mất nhu cầu dựng 96 phần tử nguyên liệu đó.

Đặt D là số biến thể detail, K là số nguyên liệu của cha. Chi phí dựng nguyên liệu là O(D × K); vẫn giữ nguyên sau tối ưu nếu response giữ nguyên. Tính tồn kho vẫn theo số sản phẩm, công thức và nguyên liệu liên quan. Không có cơ sở để hứa giảm bậc độ phức tạp tổng thể cho detail.

Ví dụ nếu detail mất 400 ms, trong đó 240 ms thuộc phần tải/dựng dữ liệu danh mục và phần này giảm còn 100 ms, các phần khác giữ 160 ms thì tổng còn 260 ms, giảm 35%. Đây chỉ là phép tính minh họa mức tác động khi đã biết breakdown; chưa xác nhận hệ thống hiện tại có breakdown này.

## 9. `CalculateAvailableQty`: giữ công thức, xem xét giảm lượt đọc

### Hiện tại

```text
Xác định combo trực tiếp và mã cha
  → Tải biến thể combo nếu chưa có sẵn
  → Tải công thức active/chưa xóa của các combo cần tính
  → Gom CORE có Quantity > 0, lấy tập MatId
  → Tải AvailableForSale/Stock của nguyên liệu chưa xóa
  → Mỗi combo: lấy min(floor(kho nguyên liệu / định mức))
  → Cha có biến thể combo: lấy max(quantity các biến thể)
```

Chi phí xử lý trung bình O(N + V + S + M), dùng dictionary/lookup để truy cập theo khóa. Số lượt query phụ thêm thường là 0–3 tùy loại sản phẩm, có biến thể, công thức CORE và dữ liệu đã preload; không tăng một query cho mỗi sản phẩm.

### Phương án xem xét sau khi đo

Có thể thử lấy kho nguyên liệu cùng query công thức qua JOIN/projection để bỏ lượt query kho riêng. Tuy nhiên:

- Kho một nguyên liệu dùng chung nhiều công thức sẽ bị lặp trong kết quả JOIN.
- INNER JOIN hoặc lọc sai có thể làm mất dòng công thức, đổi `no_formula`/`no_core` thành kết quả khác.
- Nguyên liệu đã xóa hoặc không có kho phải giữ đúng cách tính về 0.
- Khi ít công thức CORE, tải kho kèm tất cả công thức có thể lãng phí hơn cách hiện tại.

Ví dụ 500 dòng CORE chỉ dùng 20 nguyên liệu: cách hiện tại lấy kho 20 dòng; JOIN có thể lặp kho trên 500 dòng. Bỏ một lượt mạng chưa chắc đáng với dữ liệu tăng thêm. Vì vậy chưa đưa thay đổi này thành yêu cầu bắt buộc của đợt đầu.

Các quy tắc phải giữ: `AvailableForSale` ưu tiên hơn `Stock`; giá trị âm clamp về 0; chia định mức và làm tròn xuống; cha lấy MAX thay vì SUM; giữ `no_formula`, `no_core`, `calculation_error` và quy tắc StockStatus.

## 10. Phạm vi ảnh hưởng dự kiến

| Thành phần | Thay đổi đề xuất | Điều cần bảo vệ |
|---|---|---|
| V1 | Projection nhẹ trước sort tồn kho; tải chi tiết theo ID trang | Tổng số, thứ tự, danh mục, nguyên liệu và tồn kho |
| V2 `All` | Mở rộng cha/con tại DB | Bộ lọc chọn cha, các con chưa xóa, không trùng sản phẩm |
| V1/V2 tải quan hệ | Chỉ đổi cách tải nếu benchmark có lợi | Đủ response, không lệch trang khi sort bằng nhau |
| Detail | Đo projection và thử giảm dữ liệu lặp | Quyền Admin/customer, cha/con, Factor và nguyên liệu |
| `ProductQtyProjection` | Tái sử dụng cho nhánh V1 | Đủ các trường tính tồn kho |
| `MapToProductDetailResponse` | Ưu tiên giữ nguyên | Không mất trường khi entity được tải cách khác |
| `LocalProductDetailSelector` | Có thể tách phần danh mục nếu cần | Đủ các trường, Parent và children |
| `CalculateAvailableQty` | Ưu tiên giữ nguyên đợt đầu | Nếu sửa sẽ ảnh hưởng cả các luồng khác gọi helper |
| Index/migration | Chỉ bổ sung sau khi xem plan và index production | Chi phí ghi, dung lượng và triển khai migration |
| Controller, IService, request/response | Không dự kiến đổi contract | Frontend tiếp tục dùng API hiện tại |

`CalculateAvailableQty` còn được dùng ngoài ba hàm này, ví dụ `GetProductFamily` và các luồng sản phẩm khác trong Service.cs. Nếu thay helper, cần rà soát toàn bộ call site và mở rộng test; không chỉ chạy test ba endpoint.

## 11. Kiểm tra DB và tìm kiếm

- Xác nhận index thực tế thay vì chỉ nhìn khai báo model.
- Dùng SQL sinh từ EF và `EXPLAIN (ANALYZE, BUFFERS)` trên môi trường kiểm thử/staging có dữ liệu đại diện để xem dòng quét, JOIN, sort, bộ nhớ và đọc đĩa. ANALYZE thực thi query; không chạy tải nặng tùy tiện trên production.
- Tìm kiếm hiện có `ToLower().Contains(...)`; lọc label còn chuẩn hóa chuỗi. Index B-tree thông thường không đảm bảo tăng tốc tìm chuỗi con ở giữa văn bản.
- Nếu tìm kiếm là nút thắt, đánh giá riêng index phục vụ substring của PostgreSQL hoặc thiết kế trường tìm kiếm. Chưa tự đổi sang prefix/full-text vì có thể đổi kết quả tìm kiếm.
- Trang sâu vẫn có chi phí OFFSET. Keyset pagination là phương án dài hạn nhưng có thể đổi contract và cách chuyển trang, nên nằm ngoài đợt này.

## 12. Kế hoạch thực hiện và tiêu chí nghiệm thu

### Bước 1 — Lập baseline

Đo trên cùng bộ dữ liệu và hạ tầng cho: V1 mặc định; V1 sort tồn kho tăng/giảm; V2 Parent/All; tìm kiếm; một/nhiều danh mục; trang đầu/trang sâu; detail thường/combo/cha nhiều biến thể.

Ghi nhận số query/request, thời gian từng query, tổng thời gian API, p50/p95, allocation/GC, kích thước response, số dòng SQL và execution plan. Tách lần chạy cold và warm; không chỉ dựa vào một request nhanh nhất.

### Bước 2 — Triển khai ưu tiên

1. V1 sort tồn kho: projection nhẹ rồi tải trang theo ID.
2. V2 All: bỏ các List ID trung gian, dùng subquery DB.
3. Thử cách tải quan hệ cho từng endpoint, giữ phương án thắng benchmark.
4. Chỉ xử lý helper tồn kho/index khi số đo chứng minh cần thiết.

### Bước 3 — Kiểm thử đúng nghiệp vụ

- Bộ lọc: giá, từ khóa, nhãn, danh mục, trạng thái và loại sản phẩm.
- Phân trang: tổng số, trang rỗng, trang cuối, thứ tự bằng nhau, chiều sort tồn kho và đúng thứ tự sau tải theo ID.
- V2 All: cha khớp lọc nhưng con không khớp giá/từ khóa vẫn theo hành vi hiện tại; loại bản ghi đã xóa; không lặp ID.
- Detail: quyền customer/Admin, sản phẩm không tồn tại/không active, biến thể, Factor và cây danh mục.
- Tồn kho: CORE, không công thức, không CORE, kho âm, nguyên liệu đã xóa, fallback Stock, nhiều biến thể và mã lý do lỗi.
- Test dịch LINQ sang SQL và thực thi trên PostgreSQL: test InMemory không đủ để chứng minh query subquery/projection mới chạy đúng trên DB thật.

Các test sẵn có cần tận dụng: `LocalProductSearchTest.cs`, `LocalProductSearchV2Test.cs`, `GetProductV2.cs`, `CalculateAvailableQtyTests.cs` trong `HoaTheoMua.Service.Test/ProductTest`.

### Bước 4 — So sánh trước/sau và quyết định

Chỉ nhận thay đổi khi kết quả nghiệp vụ tương đương và số đo cho thấy có lợi ở tình huống mục tiêu, không tạo hồi quy đáng kể ở tình huống phổ biến. Ngưỡng p95 cụ thể cần chốt sau baseline; tài liệu không đặt mục tiêu phần trăm tùy ý.

| Kịch bản | Query trước/sau | p50 trước/sau | p95 trước/sau | Allocation trước/sau | Kết quả đúng |
|---|---|---|---|---|---|
| V1 sort tồn kho | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa kiểm thử |
| V1 mặc định | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa kiểm thử |
| V2 All | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa kiểm thử |
| V2 Parent | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa kiểm thử |
| Detail nhiều biến thể | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa kiểm thử |

## 13. Kỳ vọng và phần chưa đưa vào đợt đầu

Kỳ vọng rõ nhất là giảm mạnh dữ liệu đầy đủ tải thừa ở V1 sort tồn kho và bỏ hai bước lấy ID trung gian của V2 All. Với danh sách mặc định và detail đơn giản, mức cải thiện có thể nhỏ vì code đã giới hạn dữ liệu khá tốt.

Đợt đầu chưa bao gồm cache tồn kho, bảng tồn kho tính sẵn, đổi công thức combo, thay contract response, thêm sort tồn kho cho V2 hoặc đổi phân trang sang cursor. Các hướng này cần thiết kế riêng về tính nhất quán và tương thích frontend.

**Quyết định cần review:** thống nhất phạm vi tối ưu ưu tiên và các quy tắc phải giữ. Sau đó mới sửa implementation, đo trước/sau và cập nhật số liệu thực vào bảng nghiệm thu.

## 14. Rà soát bổ sung 12 hàm sản phẩm

Phần này bổ sung theo yêu cầu tiếp theo, vẫn chỉ là kế hoạch. Đã đọc implementation trong `Product/Service.cs`, `ProductMapping.cs`, request và `Common/ConfigHelper.cs`. Các số liệu tiếp tục là ví dụ giả định.

| Hàm | Nhận định | Ưu tiên trong nhóm bổ sung |
|---|---|---|
| `GetListProducts` | Thừa vòng serialize/deserialize request và chuỗi response không dùng | Cao về khả năng giảm cấp phát, thời gian phụ thuộc Nhanh.vn |
| `GetProductDetails` | Có thể bỏ chuỗi JSON response trung gian | Trung bình |
| `GetProductFamily` | Khi đầu vào là cha, đọc lại chính cha bằng query thứ hai | Trung bình |
| `GetMaxPrice` | Đã aggregate tại DB; cần plan/index trước khi sửa | Thấp, trừ khi đây là query nóng |
| `MoveProduct` | Có thể thay đọc rồi ghi bằng cập nhật có điều kiện | Trung bình; kiểm tra cơ chế SaveChanges trước |
| `PinProduct` | Có thể giảm tải entity; giới hạn pin cần xét cạnh tranh đồng thời | Trung bình, gắn với tính đúng dữ liệu |
| `GetProductPin` | Đã projection ít cột, một query | Thấp |
| `UpdateProduct` | `Update` entity đã tracked có thể đánh dấu cả các cột không đổi | Cao, thay đổi nhỏ nhưng cần xem SQL |
| `GetComboSpecificationAsync` | Include entity rộng chỉ để lấy một số trường | Cao |
| `GetMaterialsAsync` | Đã phân trang/projection; điểm cần đo là lọc chuỗi và COUNT | Trung bình/cao nếu bảng lớn |
| `GetProductWithList` | Đã projection ba cột, một query | Thấp |
| `GetProductByIds` | Tải hết trạng thái con rồi GroupBy/All trong RAM | Cao nếu nhiều con hoặc gọi thường xuyên |

### 14.1. `GetListProducts`: giảm xử lý JSON thừa

**Hiện tại:** đọc cấu hình Nhanh.vn từ DB → serialize request thành string với bỏ null → deserialize string thành object → JsonContent serialize lại để gửi HTTP → chờ Nhanh.vn → đọc response thành string không sử dụng → deserialize response qua `ReadFromJsonAsync` → kiểm tra Code.

**Đề xuất:** đọc cấu hình → tạo JsonContent trực tiếp từ request với options tương đương → gửi HTTP → deserialize response trực tiếp → giữ nguyên kiểm tra lỗi HTTP, null và Code.

- Bỏ vòng request object → string → object → JSON; chỉ serialize request một lần.
- Bỏ `rawJsonString` không dùng ở nhánh thành công. Đây không phải bỏ một lần gọi HTTP: code hiện chỉ gọi Nhanh.vn một lần, việc đọc body lại diễn ra trên nội dung đã được buffer.
- Nếu chuyển sang đọc stream với `ResponseHeadersRead`, phải xử lý timeout/cancellation cho cả giai đoạn đọc body, và dispose request/response đúng thời điểm.
- So sánh JSON gửi trước/sau, đặc biệt tên property và bỏ null. Truyền options trực tiếp không được vô tình đổi casing do default serializer khác nhau.

Đặt J là kích thước JSON request, Q là kích thước JSON response. CPU vẫn O(J + Q), nhưng bỏ các lượt xử lý và bản sao trung gian. Bộ nhớ của object response vẫn O(Q); không thể tuyên bố thành O(1) vì hàm vẫn trả response đã deserialize.

Ví dụ request 100 KB, response UTF-8 2 MB gồm chủ yếu ký tự ASCII: riêng string UTF-16 không dùng có thể chiếm khoảng 4 MB, chưa tính header và buffer byte. 50 request đồng thời đang giữ chuỗi này có thể tạo khoảng 200 MB chuỗi trung gian. Đây là tình huống minh họa cấp phát/đỉnh bộ nhớ, không phải số RAM thực đo.

Nếu Nhanh.vn và mạng mất 900 ms, xử lý nội bộ giảm từ 100 xuống 40 ms thì tổng từ 1.000 xuống 940 ms: chỉ giảm 6%. Lợi ích RAM có thể rõ hơn lợi ích latency.

### 14.2. `GetProductDetails`: deserialize trực tiếp, giữ options

**Trước:** query config → gửi request → đọc toàn bộ body thành string → `JsonSerializer.Deserialize` với options hỗ trợ số dạng chuỗi → kiểm tra lỗi → trả kết quả.

**Sau:** query config → gửi request → deserialize trực tiếp từ content/stream với cùng options và converter → kiểm tra lỗi → trả kết quả.

Không có bước parse request thừa như hàm list. Phải giữ `PropertyNameCaseInsensitive`, `NumberHandling` và các converter trên model; không thay bằng parser mặc định làm mất hỗ trợ dữ liệu Nhanh.vn.

Ví dụ response ASCII 200 KB: có thể tránh khoảng 400 KB string UTF-16 trung gian. CPU parse và object response vẫn O(Q); giảm bản sao và allocation, không giảm bậc thuật toán hoặc số lần gọi Nhanh.vn.

**Cấu hình dùng chung hai hàm:** `GetConfigsByGroupsAsync` hiện query entity Config và parse JSON mỗi lần. Có thể dùng no-tracking/projection cho luồng chỉ đọc; cache cấu hình là bước riêng, cần invalidation khi đổi token/config và khóa theo đúng phạm vi tài khoản. Không đưa cache token vào thay đổi nhỏ một cách ngầm định. Helper dùng chung nên phải rà soát call site nếu sửa.

### 14.3. `GetProductFamily`: tránh đọc lại cha khi đầu vào đã là cha

**Trước:** đọc sản phẩm được chọn với ba trường → xác định mã cha → đọc cha đầy đủ bằng mã → đọc nguyên liệu cha → đọc con active → tạo nguyên liệu con theo Factor → tính tồn kho với con đã preload.

**Sau đề xuất:** query đầu lấy đủ projection cần cho một family item và tính kho → nếu đầu vào là cha có mã hợp lệ, dùng lại bản ghi đó → nếu là con, tiếp tục query cha → các bước nguyên liệu, con và tồn kho giữ nguyên.

| Tình huống | Trước | Sau |
|---|---:|---:|
| Chọn cha, lượt đọc cơ sở trước helper tồn kho | 4 | 3 |
| Chọn con, lượt đọc cơ sở trước helper tồn kho | 4 | 4 |

Đánh đổi: query đầu khi chọn con lấy thêm vài trường chưa dùng cho response. Cần đo tỷ lệ chọn cha/con và độ dài Description để quyết định. Có thể thử projection kết hợp tra cha, nhưng không gộp tất cả collection vào một JOIN lớn chỉ để giảm số query.

Với L = 20 ms, chọn cha tiết kiệm khoảng một lượt mạng 20 ms và một lần tra DB. Với D con và K nguyên liệu cha, việc tạo D × K phần tử response vẫn O(DK), ví dụ 12 × 8 = 96 phần tử. Tính tồn kho và kích thước response không đổi bậc.

Giữ riêng lỗi `PRODUCT_NOT_FOUND` và `PRODUCT_PARENT_NOT_FOUND`, điều kiện active/chưa xóa, bỏ qua con inactive như hiện tại. Không thay bằng danh sách con của hàm detail vì hai hàm có điều kiện active khác nhau.

### 14.4. `GetMaxPrice`: hiện đã làm đúng chỗ

**Trước:** lọc chưa xóa, có giá → nếu có danh mục thì EXISTS liên kết chưa xóa → SQL MAX → null trả 0.

Không có tải toàn bộ giá về ứng dụng. Bộ nhớ phía ứng dụng cho kết quả O(1); DB có thể quét O(N) ứng viên hoặc dùng đường truy cập tốt hơn tùy index/plan.

**Đề xuất:** giữ SQL aggregate; chuẩn hóa ID trùng nếu cần; đo query với/không danh mục. Chỉ thử index phù hợp với lọc và giá khi execution plan chứng minh có lợi. Không tự đổi sang tải danh sách rồi `.Max()` trong RAM.

Ví dụ 100.000 sản phẩm, 5.000 sản phẩm thuộc danh mục: response vẫn chỉ một số trước và sau. Index có thể giảm số dòng DB phải đọc, nhưng chưa thể nêu số mili-giây tiết kiệm nếu chưa xem plan. Không tự thêm `IsActive` hoặc điều kiện chỉ cha: hàm hiện không có các điều kiện đó.

### 14.5. `MoveProduct`: thử cập nhật trực tiếp có điều kiện

**Trước:** validate → SELECT entity product hoặc category-product hợp lệ → đổi Position/UpdatedAt → SaveChanges.

**Phương án thử:** validate → một UPDATE với đầy đủ điều kiện hợp lệ, chỉ SET Position/UpdatedAt → kiểm tra số dòng bị ảnh hưởng; 0 dòng trả lỗi tương ứng.

Ví dụ phần này từ một SELECT + một UPDATE thành một UPDATE. Nếu L = 20 ms và DB thực thi mỗi lệnh 3 ms, thời gian minh họa từ 46 xuống 23 ms. Đây không phải thời gian toàn API.

CPU/bộ nhớ ứng dụng vẫn O(1) cho một bản ghi; giảm round trip và tránh đọc entity rộng. Trước khi chọn `ExecuteUpdateAsync`, kiểm tra override SaveChanges/interceptor/audit, concurrency token và entity đang được tracking: cập nhật trực tiếp không đi qua cùng toàn bộ đường SaveChanges và không tự đồng bộ entity tracked. Nếu các hook cần thiết không được bảo toàn, giữ cách hiện tại.

### 14.6. `PinProduct`: giảm dữ liệu đọc, không đánh đổi giới hạn 32

**Trước:** tải product cha hợp lệ → nếu pin thì COUNT số pin trong phạm vi → nếu theo danh mục thì tải liên kết → sửa IsPin/UpdatedAt → SaveChanges.

- Pin toàn cục: thường 2 SELECT + 1 ghi.
- Pin theo danh mục: thường 3 SELECT + 1 ghi.
- Unpin bỏ lượt COUNT.

**Đề xuất:** với nhánh danh mục, chỉ lấy các trường hoặc kiểm tra tồn tại cần thiết của product thay vì toàn bộ entity; xem xét UPDATE có điều kiện nếu giữ đủ lỗi và hook. Với ngưỡng 32, có thể thử truy vấn chỉ xác định có ít nhất 32 dòng bằng giới hạn số dòng thay cho đếm hết, nhưng lợi ích thường nhỏ vì dữ liệu đúng nghiệp vụ vốn chỉ khoảng 32 pin.

Ví dụ product rộng 5 KB nhưng kiểm tra chỉ cần ID/trạng thái: tránh tải phần lớn 5 KB cho bước kiểm tra. Không kỳ vọng thay đổi này làm trang sản phẩm nhanh hơn đáng kể nếu thao tác pin chỉ dùng ở admin.

**Điểm đúng dữ liệu cần tách riêng:** COUNT rồi UPDATE không tự đảm bảo giới hạn khi có request đồng thời. Ví dụ đang có 31 pin, hai request cùng thấy 31 rồi cùng pin có thể thành 33. Transaction với isolation mặc định không mặc nhiên giải quyết việc này; cần thiết kế khóa theo phạm vi pin hoặc isolation/retry phù hợp nếu xử lý. Đây là công việc đảm bảo tính đúng, không gắn nhãn tối ưu tốc độ.

Hiện pin lại sản phẩm đã pin khi phạm vi có 32 mục vẫn bị chặn bởi COUNT. Không tự đổi hành vi này khi tối ưu. Các điều kiện soft-delete trong COUNT theo danh mục cũng cần giữ hoặc sửa bằng yêu cầu nghiệp vụ riêng.

### 14.7. `GetProductPin`: ưu tiên thấp, có chênh lệch điều kiện cần ghi nhận

**Trước:** lọc pin toàn cục hoặc theo danh mục → sort Position/ProductName → projection tám trường → trả list. Chỉ một query, không Include collection.

**Sau đề xuất:** giữ projection; xem plan/index nếu API gọi nhiều; thêm tie-breaker theo ID nếu cần thứ tự ổn định và được thống nhất. Nếu dữ liệu có 32 pin, chỉ cần dựng 32 DTO trước và sau. CPU map O(K), bộ nhớ O(K) với K số kết quả; chi phí sort tại DB phụ thuộc index.

DTO chỉ gồm scalar, không materialize entity Product, nên thiếu `AsNoTracking` ở đây không đồng nghĩa EF đang tracking toàn bộ Product. Thêm nó để thể hiện ý định không phải cải thiện lớn.

Nhánh toàn cục hiện chỉ nhận `ParentProductId == null`; `PinProduct` chấp nhận cả null và chuỗi rỗng. Đây là chênh lệch nghiệp vụ tiềm năng, cần test/quyết định riêng, không âm thầm sửa trong đợt performance. Không thêm `Take(32)` chỉ để che dữ liệu vượt ngưỡng.

### 14.8. `UpdateProduct`: bỏ đánh dấu cập nhật toàn entity khi không cần

**Trước:** SELECT entity có tracking → AutoMapper gán các giá trị request khác null → đổi UpdatedAt → gọi `Products.Update(product)` → SaveChanges.

**Sau đề xuất:** SELECT có tracking → giữ mapping bỏ qua null → đổi UpdatedAt → SaveChanges trực tiếp. Change tracker xác định trường thực sự thay đổi, không cần gọi Update lại cho entity vừa được context tải.

Ví dụ entity có 80 cột có thể ghi, request chỉ đổi Price: hiện Update có thể đánh dấu gần như toàn bộ thuộc tính có thể cập nhật; phương án sau thường chỉ SET Price và UpdatedAt. Số cột thực tế cần xác nhận từ SQL sinh ra, không lấy 80 làm số cột đã đo của model.

Số lượt DB vẫn là SELECT + UPDATE. Giảm kích thước câu lệnh/tham số và tránh ghi lại các giá trị cũ không liên quan. PostgreSQL vẫn cập nhật phiên bản dòng; không được suy ra giảm 80 cột xuống 2 cột đồng nghĩa giảm 97,5% I/O đĩa hoặc WAL.

Nếu F là số trường entity và U là số trường đổi, tải entity/change detection vẫn có chi phí theo F; phần cột/tham số UPDATE có thể giảm từ tỷ lệ O(F) xuống O(U). Không giải quyết hoàn toàn concurrent update cùng một trường; cần cơ chế concurrency riêng nếu có yêu cầu.

Test bắt buộc: null vẫn có nghĩa bỏ qua; đổi một trường không ghi đè các trường không được request; UpdatedAt vẫn cập nhật; hook/audit và response không đổi.

### 14.9. `GetComboSpecificationAsync`: thay Include rộng bằng projection

**Trước:** query một công thức chưa xóa, Include toàn bộ Sub chưa xóa → query các công thức, Include toàn bộ Mat chưa xóa → dựng DTO → sort CORE trước.

**Sau đề xuất đợt đầu:** query metadata Sub với đúng bốn trường response bằng projection, giữ kiểm tra có công thức → query projection công thức/nguyên liệu chỉ gồm trường cần thiết → tính MaxComboPossible và sort → trả DTO. Giữ hai query trước để giảm rủi ro; thử gộp sau nếu cần.

Ví dụ một combo có 20 nguyên liệu, mỗi entity sản phẩm rộng 5 KB: trước tải xấp xỉ 21 × 5 = 105 KB chỉ riêng các entity sản phẩm. Nếu projection mỗi nguyên liệu 250 byte và metadata cha 200 byte thì khoảng 5,2 KB. Giảm khoảng 95% phần dữ liệu này, chưa tính cột công thức, giao thức và khác biệt độ dài chuỗi.

Với K nguyên liệu, CPU map O(K), sort đầy đủ O(K log K) và bộ nhớ O(K) vẫn giữ; giảm hệ số dung lượng. Nếu cần có thể chia hai nhóm CORE/non-CORE O(K), nhưng thường K nhỏ nên không ưu tiên so với giảm tải entity.

Giữ các trường hợp đặc biệt: không có công thức trả null; có công thức nhưng mọi Mat bị xóa có thể trả metadata với Materials rỗng; không tự thêm lọc IsActive vào công thức. `MaxComboPossible` ở đây dùng AvailableForSale, không fallback Stock như helper tồn kho; không hợp nhất hai công thức tính khi chưa có quyết định nghiệp vụ.

### 14.10. `GetMaterialsAsync`: nút thắt có thể ở lọc chuỗi

**Trước:** lọc nhãn vật liệu/phụ liệu bằng Lower + Replace + Contains → loại ExcludeIds → lọc nhãn/từ khóa bổ sung → COUNT → sort CreatedAt và phân trang → projection bảy trường.

Code đã hạn chế tối đa 100 kết quả/trang và không tải toàn bộ entity. Phần cần đo là DB đánh giá hàm chuỗi và tìm chuỗi con trên nhiều dòng, xảy ra ở cả COUNT và query trang.

**Sau theo từng mức:**

1. Giữ query/projection, chuẩn hóa ID loại trừ trùng; cân nhắc tie-breaker Id.
2. Xem plan để thử expression index hoặc cấu trúc tìm kiếm phù hợp với đúng biểu thức lọc; không mặc định index ProductLabel thông thường giải quyết được Contains.
3. Nếu đây là truy vấn nóng, thiết kế nhãn đã chuẩn hóa/bảng liên kết nhãn với index, hoặc cờ phân loại vật liệu. Đây là thay đổi schema và đồng bộ luồng import/update, không chỉ sửa một hàm.

Ví dụ bảng có 100.000 dòng, 2.000 dòng là nguyên liệu và trang có 20 dòng. Plan quét tuần tự có thể phải đánh giá nhãn trên phần lớn 100.000 dòng; cấu trúc/index phù hợp có thể thu hẹp tập ứng viên về nhóm nguyên liệu trước khi sort. Không cam kết chỉ đọc 20 dòng vì COUNT, tìm kiếm và OFFSET vẫn cần xử lý thêm.

Đặt T là số dòng DB phải đánh giá và Z là độ dài chuỗi trung bình: phần xử lý chuỗi khi quét xấp xỉ O(TZ). Sau index/chuẩn hóa, T có thể nhỏ hơn hoặc bớt phép biến đổi mỗi dòng; độ phức tạp DB chính xác phụ thuộc plan. Bộ nhớ ứng dụng cho kết quả vẫn O(P).

Giữ so khớp nguyên nhãn có dấu phẩy, phân biệt vật liệu/phụ liệu, dấu tiếng Việt, khoảng trắng và ExcludeIds. Nếu thiết kế nhãn riêng phải cập nhật cả import từ Nhanh.vn và mọi đường sửa nhãn, tránh dữ liệu cũ/mới lệch nhau.

### 14.11. `GetProductWithList`: không cần viết lại query đang gọn

**Trước:** WHERE ID thuộc request, chưa xóa → SELECT Id/CoverImage/ProductName → ToList. Không tải entity rộng hoặc quan hệ.

**Sau đề xuất nhỏ:** loại ID trùng để giảm kích thước tham số; trả list rỗng sớm nếu input rỗng. Với input null, thống nhất validation/error riêng vì hiện không xử lý rõ. Không tự đặt giới hạn batch làm thay đổi request đang được chấp nhận.

Ví dụ request chứa 1.000 ID nhưng chỉ 100 ID khác nhau: tham số có thể giảm từ 1.000 xuống 100 giá trị. Kết quả hiện tại vốn đã không trả trùng sản phẩm chỉ vì input trùng. CPU chuẩn hóa O(I), bộ nhớ O(U), I là số ID đầu vào và U là số ID khác nhau.

Response O(K) với K sản phẩm tìm thấy trước và sau. Projection scalar đã không materialize entity để tracking; thêm AsNoTracking không phải tối ưu chính. Hàm chưa cam kết thứ tự theo request; không tự thêm cam kết đó trong thay đổi performance.

### 14.12. `GetProductByIds`: tổng hợp trạng thái con tại DB

**Trước:** chuẩn hóa ID, bỏ trùng/rỗng → tải projection sản phẩm → tải trạng thái cha cho đầu vào là con → tải toàn bộ ParentProductId/IsActive của con cho đầu vào là cha → GroupBy/All trong RAM → tính tồn kho theo lô → trả theo thứ tự ID đã chuẩn hóa.

**Sau đề xuất:** giữ bước tải sản phẩm và quy tắc thứ tự → tổng hợp trạng thái con bằng GROUP BY/aggregate tại DB, chỉ trả một dòng/cha có con → giữ bước tra trạng thái cha và tính tồn kho. Phương án khác là EXISTS con inactive; phải kiểm tra kỹ sự khác nhau giữa không có con và có toàn bộ con active để giữ nhánh xử lý hiện tại.

Ngoài ra, truy vấn `parentActiveByNhanhProductId` hiện gọi `ToDictionaryAsync` trực tiếp trên entity query. Nên thêm Select rõ ràng chỉ gồm NhanhProductId/IsActive trước khi materialize, để tránh tải cả entity cha khi tạo dictionary.

Ví dụ request có 200 cha, mỗi cha có 50 con:

| Dữ liệu trạng thái con | Trước | Sau aggregate tại DB |
|---|---:|---:|
| Dòng chuyển về ứng dụng | 10.000 | Tối đa 200 |
| Với giả định 80 byte/dòng | 800 KB | 16 KB |
| GroupBy/All trong ứng dụng | 10.000 dòng | Không cần nhóm lại toàn bộ con |

Phần chuyển dữ liệu trạng thái con giảm khoảng 98% theo giả định. DB vẫn phải kiểm tra con để tổng hợp. Số lượt query có thể không giảm. Các query tồn kho vẫn có thể cần đọc biến thể/công thức; không tính 98% này thành giảm 98% tổng API.

Đặt I là số ID đầu vào, K là số sản phẩm tìm được, B là số con được kiểm tra, A là số cha có con. Chi phí xử lý ứng dụng ngoài tồn kho từ O(I + K + B) có thể thành O(I + K + A); bộ nhớ trung gian trạng thái con O(B) thành O(A). Phần DB vẫn xử lý quan hệ con theo plan.

Điều kiện phải giữ: con active phụ thuộc cha; cha active phụ thuộc toàn bộ con chưa xóa; input rỗng trả lỗi; trả đúng thứ tự input sau loại trùng. Query trạng thái cha hiện không lọc IsDeleted, còn query trạng thái con có lọc; đây là khác biệt hiện hữu cần quyết định nghiệp vụ riêng, không tự sửa khi tối ưu.

## 15. Thứ tự triển khai tổng hợp sau khi review

1. Đo baseline ba endpoint ban đầu và các API bổ sung đang thực sự được frontend gọi thường xuyên; tách latency Nhanh.vn khỏi latency nội bộ.
2. Ưu tiên giảm tải thừa rõ ràng: V1 sort tồn kho, V2 All, projection của GetComboSpecificationAsync và GetProductByIds.
3. Thay đổi nhỏ có thể review độc lập: bỏ Update entity đã tracked; bỏ JSON/string trung gian ở hai hàm Nhanh.vn.
4. Giảm một query GetProductFamily khi đầu vào là cha; thử cập nhật trực tiếp MoveProduct nếu tương thích hook.
5. Đo rồi quyết định tải quan hệ, index/tìm kiếm nguyên liệu và MAX giá. Các hàm projection đơn giản giữ nguyên nếu không có bằng chứng chậm.
6. Xử lý cạnh tranh giới hạn pin và các chênh lệch nghiệp vụ bằng hạng mục riêng, không trộn vào benchmark hiệu năng.

Kiểm thử bổ sung cần có: so sánh payload HTTP Nhanh.vn trước/sau bằng HttpMessageHandler giả lập; response có số dạng chuỗi và mã lỗi; family chọn cha/con; MAX tập rỗng; move không hợp lệ; pin/unpin và giới hạn; update null/field không đổi; combo không có công thức hoặc Mat đã xóa; nhãn vật liệu tiếng Việt; batch ID trùng và thứ tự; active của cha/con. Chạy PostgreSQL integration test cho aggregate/subquery và kiểm tra SQL UPDATE, không chỉ dựa vào InMemory.

**Phạm vi repository sau rà soát:** chỉ cập nhật tài liệu này; chưa sửa code, migration, cấu hình hoặc chạy thay đổi trên production.
