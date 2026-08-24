# SePay - Luồng thanh toán webhook đầy đủ

Tài liệu này mô tả toàn bộ luồng SePay trong dự án: từ lúc tạo đơn và QR, đến khi SePay báo giao dịch, hệ thống xác nhận thanh toán, tạo đơn vận chuyển và lưu lịch sử giao dịch.

Các file chính:

- `HoaTheoMua.Service/Order/Service.cs`: tạo Order và URL QR SePay.
- `HoaTheoMua.API/Controllers/SepayController.cs`: HTTP endpoint nhận webhook.
- `HoaTheoMua.Service/Webhook/Service.cs`: xác thực, tìm Order và xử lý thanh toán.
- `HoaTheoMua.Service/Webhook/Request.cs`: DTO body SePay gửi vào.
- `HoaTheoMua.Service/Webhook/SepayOption.cs`: cấu hình API key.
- `HoaTheoMua.Repository/Entity/Transaction.cs`: bảng lịch sử giao dịch.

## Sơ đồ tổng quát

```text
Frontend tạo đơn với Gateway = "Sepay"
    |
    v
Order Service tạo Order Pending + QR có nội dung HTM-{orderId}
    |
    v
Khách quét QR và chuyển khoản đúng số tiền / đúng nội dung
    |
    v
SePay phát hiện giao dịch ngân hàng và POST /api/v1/sepay
    |
    v
Controller xác thực Authorization: Apikey <key>
    |
    v
Tách orderId từ Content hoặc Code, kiểm tra Order Pending và số tiền
    |
    v
FulfillPaidOrder: Ahamove -> NhanhVn -> lưu Order Paid + Transaction
    |
    v
Trả HTTP 200 thành công cho SePay
```

## 1. Tạo Order và QR SePay

Trong `Order.Service.CreateOrder`, hệ thống tạo một Order mới trước. Phần dùng chung khởi tạo các dữ liệu sau:

```csharp
Id = Guid.NewGuid(),
Status = "Pending",
TotalFee = totalProductFee + totalShippingFee + vatFee,
```

- `Id`: GUID duy nhất của Order.
- `Status = "Pending"`: đơn đang chờ thanh toán.
- `TotalFee`: tổng phải thu, gồm tiền hàng, phí giao và VAT nếu áp dụng.

Khi request có `Gateway == "Sepay"`, code tạo QR:

```csharp
decimal totalAmount = newOrder.TotalFee;
string description = $"HTM-{newOrder.Id}";
```

- `totalAmount` là số tiền QR yêu cầu khách chuyển.
- `description` là nội dung chuyển khoản. Ví dụ: `HTM-ddba561c-e7c1-4375-8ca6-49a2c30f91a1`.
- `HTM-` là tiền tố nhận biết thanh toán của Hoa Theo Mùa; phần GUID là khóa để truy ngược Order.

URL QR được ghép như sau:

```csharp
https://qr.sepay.vn/img?acc=<account>&bank=<bank>&amount=<amount>&des=HTM-<orderId>
```

Ý nghĩa các tham số:

| Tham số | Ý nghĩa |
|---|---|
| `acc` | Số tài khoản nhận tiền |
| `bank` | Ngân hàng nhận tiền |
| `amount` | Số tiền phải trả |
| `des` | Nội dung chuyển khoản, chính là `HTM-{orderId}` |
| `template` | Mẫu QR hiển thị |
| `holder` | Tên chủ tài khoản |

Backend chỉ tạo URL ảnh QR, không tạo payment session riêng tại SePay. `QrUrl` được lưu vào Order, sau đó trả về cho frontend qua `PaymentUrl`.

Cuối cùng code lưu Order và OrderDetails vào database. Điều này phải hoàn thành trước khi khách chuyển khoản, vì webhook cần tìm Order đã tồn tại.

## 2. Endpoint nhận webhook

Endpoint tại `SepayController`:

```csharp
[HttpPost("/api/v1/sepay")]
public async Task<IActionResult> SepayWebhookHandler(
    [FromBody] Request.SepayWebhookRequest request)
{
    _sepayService.VerifySepayApiKey(
        HttpContext.Request.Headers.Authorization.FirstOrDefault());
    await _sepayService.SepayWebhookHandler(request);
    return Ok(...);
}
```

- `[HttpPost("/api/v1/sepay")]`: public route mà SePay gọi vào.
- `[FromBody]`: ASP.NET đọc JSON body và map sang DTO `SepayWebhookRequest`.
- Dòng `VerifySepayApiKey(...)` chạy trước: request không có key hợp lệ sẽ không thể chuyển Order sang `Paid`.
- `SepayWebhookHandler(request)` mới làm phần nghiệp vụ thanh toán.
- Nếu không ném exception, API trả HTTP 200.

## 3. Body SePay được map thành DTO

```csharp
public class SepayWebhookRequest
{
   public string Gateway { get; set; } = null!;
   public string TransactionDate { get; set; } = null!;
   public string AccountNumber { get; set; } = null!;
   public string? SubAccount { get; set; }
   public string Code { get; set; } = null!;
   public string Content { get; set; } = null!;
   public string TransferType { get; set; } = null!;
   public string Description { get; set; } = null!;
   public decimal TransferAmount { get; set; }
   public string ReferenceCode { get; set; } = null!;
   public decimal Accumulated { get; set; }
   public long Id { get; set; }
}
```

| Field | Mục đích trong hệ thống |
|---|---|
| `Gateway` | Lưu nguồn/gateway của giao dịch |
| `TransactionDate` | Lưu thời điểm chuyển tiền |
| `AccountNumber` | Lưu tài khoản nhận tiền |
| `SubAccount` | Lưu tài khoản phụ; null được đổi thành chuỗi rỗng |
| `Code` | Mã SePay đã tách từ nội dung; dùng dự phòng để tìm Order |
| `Content` | Memo/nội dung chuyển khoản gốc; được ưu tiên để tìm Order |
| `TransferType` | Kiểu giao dịch, thường là giao dịch tiền vào |
| `Description` | Mô tả giao dịch để đối soát |
| `TransferAmount` | Số tiền thực nhận; được so với `Order.TotalFee` |
| `ReferenceCode` | Mã tham chiếu giao dịch ngân hàng |
| `Accumulated` | Số dư tích lũy SePay gửi về |
| `Id` | ID giao dịch SePay; lưu thành `Transaction.RequestId` |

`null!` chỉ làm compiler không cảnh báo null. Nó không tự validation body. Nếu gateway gửi thiếu field, giá trị vẫn có thể null lúc chạy.

## 4. Xác thực Authorization API key

Cấu hình có kiểu:

```csharp
public class SepayOption
{
    public string ApiKey { get; set; } = null!;
}
```

Service bind configuration section `SepayOption`:

```csharp
_sepayOption = new SepayOption();
configuration.GetSection(nameof(SepayOption)).Bind(_sepayOption);
```

Nghĩa là app cần có cấu hình tương đương:

```json
{
  "SepayOption": {
    "ApiKey": "<api-key-do-sepay-cung-cap>"
  }
}
```

Trong môi trường thật, nên dùng secret hoặc environment variable thay vì commit key:

```text
SepayOption__ApiKey=<api-key-do-sepay-cung-cap>
```

Header webhook được yêu cầu có dạng:

```http
Authorization: Apikey <api-key>
```

Hàm `VerifySepayApiKey` xử lý theo thứ tự:

```csharp
if (string.IsNullOrWhiteSpace(_sepayOption.ApiKey))
    throw new ServerException(...);
```

Không cấu hình key thì fail closed: trả lỗi 500 và không xử lý thanh toán. Đây là an toàn hơn việc chấp nhận webhook không xác thực.

```csharp
if (string.IsNullOrWhiteSpace(authorizationHeader))
    throw new UnauthorizedException(...);
```

Không gửi header `Authorization` thì trả 401.

```csharp
var parts = authorizationHeader.Split(' ', 2,
    StringSplitOptions.RemoveEmptyEntries);
```

Ví dụ `Apikey abc123` được tách thành scheme `Apikey` và key `abc123`.

```csharp
if (parts.Length != 2 ||
    !string.Equals(parts[0], "Apikey",
        StringComparison.OrdinalIgnoreCase))
    throw new UnauthorizedException(...);
```

Header phải có đúng hai phần và scheme phải là `Apikey`, không phân biệt chữ hoa/thường.

```csharp
var providedKeyBytes = Encoding.UTF8.GetBytes(parts[1]);
var expectedKeyBytes = Encoding.UTF8.GetBytes(_sepayOption.ApiKey);

if (providedKeyBytes.Length != expectedKeyBytes.Length ||
    !CryptographicOperations.FixedTimeEquals(
        providedKeyBytes, expectedKeyBytes))
    throw new UnauthorizedException(...);
```

- Chuyển hai key sang byte để so sánh.
- Kiểm tra độ dài trước vì `FixedTimeEquals` yêu cầu hai mảng bằng độ dài.
- `FixedTimeEquals` giảm nguy cơ timing attack khi đo thời gian so sánh.

Lưu ý: API key là credential dùng chung; nó không phải HMAC ký trực tiếp lên body. Vì vậy body không được ràng buộc mật mã với request, và chống replay/trùng giao dịch vẫn cần xử lý ở nghiệp vụ/database.

## 5. Tìm Order từ nội dung chuyển khoản

### Mã GUID đầy đủ

Regex chính:

```regex
HTM[-_]?([0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12})
```

Nó chấp nhận:

```text
HTM-ddba561c-e7c1-4375-8ca6-49a2c30f91a1
HTMddba561ce7c143758ca649a2c30f91a1
htm_ddba561c-e7c1-4375-8ca6-49a2c30f91a1
Thanh toan HTM-ddba561c-e7c1-4375-8ca6-49a2c30f91a1
```

`GeneratedRegex` giúp .NET sinh implementation regex đã tối ưu lúc build.

Hàm `TryExtractOrderId`:

```csharp
orderId = Guid.Empty;
if (string.IsNullOrWhiteSpace(value)) return false;

var match = PaymentCodeRegex().Match(value);
if (!match.Success) return false;

var hex = match.Groups[1].Value.Replace("-", "");
return Guid.TryParseExact(hex, "N", out orderId);
```

- Khởi tạo `out orderId` bằng `Guid.Empty`.
- Chuỗi rỗng/null hoặc không match regex thì trả `false`.
- `Groups[1]` là phần GUID trong ngoặc.
- Bỏ dấu gạch ngang.
- `TryParseExact(..., "N", ...)` chỉ nhận GUID 32 ký tự hex, không có dấu `-`.

### Mã bị SePay cắt ngắn

Một số giao dịch có thể bị trả về mã ngắn hơn 32 ký tự hex. Regex prefix:

```regex
HTM[-_]?([0-9a-fA-F][0-9a-fA-F-]*)
```

Code chỉ chấp nhận prefix dài 24 đến 31 ký tự hex:

```csharp
private const int MinPrefixLength = 24;

if (hex.Length < MinPrefixLength || hex.Length >= 32)
    return false;
```

- Dưới 24 ký tự: không đủ chắc chắn để xác định đơn.
- Từ 32 ký tự: phải dùng parser GUID đầy đủ.

Prefix được chuyển về dạng có dấu `-` giống `Guid.ToString()` để query PostgreSQL. Sau đó code chỉ lấy tối đa 2 kết quả:

```csharp
var matches = await _dbContext.Orders
    .Where(o => o.Id.ToString().StartsWith(prefix))
    .Take(2)
    .ToListAsync();
```

Nếu có hơn một Order cùng prefix thì trả HTTP 409 `Ambiguous payment code`, không chọn ngẫu nhiên một đơn.

## 6. Quy trình `SepayWebhookHandler`

```csharp
if (TryExtractOrderId(request.Content, out var orderId) ||
    TryExtractOrderId(request.Code, out orderId))
```

Ưu tiên `Content`, do đây thường là memo gốc đầy đủ hơn. Nếu không tìm được mã đầy đủ, code thử `Code`.

Khi đã có GUID:

```csharp
order = await _dbContext.Orders
    .Include(x => x.OrderDetails)
    .FirstOrDefaultAsync(o => o.Id == orderId);
```

`Include(OrderDetails)` cần thiết vì bước tạo Ahamove/NhanhVn dùng danh sách sản phẩm của đơn.

Sau khi có `order`, ba kiểm tra nghiệp vụ là:

```csharp
if (order == null)
    throw new NotFoundException("Order not found in description");
```

Mã transfer hợp lệ nhưng không có Order tương ứng trong database: HTTP 404.

```csharp
if (order.Status != "Pending")
    throw new NotFoundException("Order not on Pending");
```

Chỉ Order đang chờ thanh toán mới được hoàn tất. Dòng dấu `;` sau `throw` trong code là dư thừa nhưng không làm thay đổi hành vi.

```csharp
if (order.TotalFee != request.TransferAmount)
    throw new ConflictException(...);
```

Số tiền thật SePay báo về phải bằng chính xác `TotalFee`. Chuyển thiếu hoặc thừa đều không tự động đánh dấu Paid.

## 7. Tạo Transaction cho SePay

Sau các kiểm tra, hệ thống tạo entity `Transaction`:

```csharp
RequestId = request.Id.ToString(),
Gateway = request.Gateway,
paymentMethod = PaymentMethod.VNPayQR,
TransactionDate = DateTime.TryParse(request.TransactionDate,
    out DateTime parsedDate)
    ? parsedDate.ToUniversalTime()
    : DateTime.UtcNow,
```

- `RequestId`: ID số của SePay, đổi sang string để dùng chung schema với BaoKim.
- `Gateway`: giá trị SePay gửi về.
- `paymentMethod`: hệ thống gán `VNPayQR`.
- `TransactionDate`: nếu parse được ngày từ body thì chuyển UTC; nếu không thì dùng giờ UTC hiện tại.

Các giá trị giao dịch còn lại được lưu nguyên vẹn để phục vụ đối soát:

```csharp
AccountNumber = request.AccountNumber,
SubAccount = request.SubAccount ?? "",
Code = request.Code,
Content = request.Content,
TransferType = request.TransferType,
Description = request.Description,
TransferAmount = request.TransferAmount,
Accumulated = request.Accumulated,
ReferenceCode = request.ReferenceCode,
OrderId = order.Id,
CreatedAt = DateTimeOffset.UtcNow,
```

`Transaction` liên kết với Order bằng `OrderId`. Trước đây cột gateway ID từng tên `SePayId`; migration `20260717105135_TransactionRequestIdAndPaymentMethod` đã đổi sang `RequestId` và chuyển kiểu thành `varchar(100)` để chứa cả ID chữ-số từ BaoKim.

## 8. `FulfillPaidOrder` làm gì?

SePay handler gọi:

```csharp
await FulfillPaidOrder(order, transactionEntity);
```

Đây là hàm dùng chung cho SePay và BaoKim. Theo trình tự, nó:

1. Gán `order.Status = "Paid"` trong memory và cập nhật `UpdatedAt`.
2. Lấy địa chỉ công ty, ghép mô tả giao hàng và thông tin hóa đơn.
3. Với đơn giao ngay đủ điều kiện, gọi Ahamove tạo chuyến; lưu `AhahaMoveOrderId` và `AhahaMoveTrackingUrl` lên Order.
4. Chuyển `OrderDetails` thành danh sách sản phẩm NhanhVn.
5. Gọi NhanhVn tạo đơn; chỉ chấp nhận response có `code == 1` và `data` là object.
6. Lưu `NhanhOrderId`, `NhanhTrackingUrl`.
7. Cuối cùng mới ghi database:

```csharp
_dbContext.Orders.Update(order);
_dbContext.Transactions.Add(transaction);
await _dbContext.SaveChangesAsync();
```

Như vậy, Order và Transaction chỉ được persist sau khi các bước tích hợp ngoài không báo lỗi.

## 9. Mã lỗi thường gặp

| Điều kiện | Exception | HTTP |
|---|---|---:|
| Chưa cấu hình `SepayOption.ApiKey` | `ServerException` | 500 |
| Không có Authorization | `UnauthorizedException` | 401 |
| Header/key sai | `UnauthorizedException` | 401 |
| Không có mã `HTM` hợp lệ | `BadRequestException` | 400 |
| Prefix trùng nhiều đơn | `ConflictException` | 409 |
| Không tìm thấy Order | `NotFoundException` | 404 |
| Order không Pending | `NotFoundException` | 404 |
| Số tiền không khớp | `ConflictException` | 409 |
| Ahamove/NhanhVn báo lỗi | `BadRequestException` | 400 |

## 10. Các điểm cần lưu ý trước production

1. **Cần cấu hình API key SePay.** Nếu không có `SepayOption:ApiKey`, webhook cố tình bị chặn để tránh giả mạo thanh toán.

2. **Webhook chưa idempotent hoàn chỉnh.** Khi webhook thành công rồi bị gửi lại, Order đã `Paid` sẽ trả 404. Nên tra `Transaction.RequestId` trước và trả HTTP 200 khi cùng giao dịch đã được xử lý thành công.

3. **Thiếu unique index cho giao dịch gateway.** `RequestId` mới chỉ có giới hạn độ dài, chưa có unique constraint. Nên thêm unique index theo `(Gateway, RequestId)`.

4. **Có rủi ro tạo đơn giao trùng khi lỗi giữa chừng.** Ahamove/NhanhVn được gọi trước `SaveChangesAsync`. Ví dụ Ahamove thành công nhưng NhanhVn lỗi: database chưa lưu ID Ahamove; khi webhook retry, Ahamove có thể bị tạo thêm đơn. Cần cơ chế idempotency với API ngoài, lưu checkpoint, hoặc outbox/saga.

5. **Có race condition khi hai webhook đến cùng lúc.** Cả hai có thể cùng thấy Order `Pending`, cùng gọi Ahamove/NhanhVn, rồi cùng cố ghi Transaction. Cần khóa/cập nhật có điều kiện cho Order cùng unique index transaction.

6. **Tài khoản ngân hàng đang hard-code trong source.** Nên đưa `bank`, `account`, `holder` vào configuration/secret để đổi mà không cần deploy code.

7. **Nên kiểm tra chặt hơn dữ liệu inbound.** Ví dụ `TransferType`, `Gateway`, dữ liệu null và định dạng `TransactionDate`. Với ngày giờ không có timezone, `DateTime.ToUniversalTime()` có thể phụ thuộc timezone của server; `DateTimeOffset` hoặc định dạng có offset sẽ rõ ràng hơn.
