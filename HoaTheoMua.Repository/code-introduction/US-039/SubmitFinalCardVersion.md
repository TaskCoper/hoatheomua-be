# Submit version Card cuối cùng vào Order và Nhanh.vn

## Bối cảnh

Khi user bấm đặt đơn, Order được tạo ở trạng thái `Pending`. Chỉ sau khi thanh toán thành công, đơn mới được tạo/đẩy lên Nhanh.vn.

Card AI hoặc Card tạo lại từ History không được chốt ngay khi thanh toán: user có thể tạo nhiều version, so sánh và chọn một version cuối cùng. Nhân viên xử lý kho trên Nhanh.vn cần thấy link ảnh Card đã được user chọn để in.

HandMade là flow khác: Card đã được tạo ngay trong `CreateOrder` và ID đã nằm trong `OrderDetails`.

## API dự kiến

Nên thao tác trên `orderDetailId`, vì một Order có thể có nhiều dòng Product/Combo và Card phải gắn đúng bó hoa/dòng hàng.

```http
POST /api/v1/order-details/{orderDetailId}/submit-final-cards
```

```json
{
  "cards": [
    {
      "generatedCardId": "id-version-cuoi-cung-user-da-chon",
      "quantity": 1
    }
  ]
}
```

Không nên chỉ nhận `orderId`, vì sẽ không xác định được Card thuộc `OrderDetails` nào khi đơn có nhiều dòng hàng. API chỉ được submit **một lần** cho mỗi `OrderDetails`: user phải chọn xong tất cả version cuối cùng trước khi gửi request.

## Flow Card AI / Card từ History

```text
1. User tạo Order
   → Order = Pending.

2. User thanh toán thành công
   → Order = Paid.
   → Đơn được tạo/đẩy lên Nhanh.vn.
   → Chưa có Card AI cuối cùng để nhân viên in.

3. User dùng API tạo/tạo lại Card AI
   → tạo nhiều version trong generation session.
   → so sánh và chọn toàn bộ GeneratedCard cuối cùng cần dùng, ví dụ GC3, GC4.

4. User bấm “Thêm vào đơn hàng”
   → FE gửi orderDetailId + danh sách Card cuối cùng + quantity.
   → BE lưu các Card cùng số lượng in vào danh sách Card của OrderDetails.
   → BE lấy imageUrl của tất cả Card đã submit.
   → BE gọi API Nhanh.vn cập nhật privateDescription của đơn.
   → Nhân viên Nhanh.vn thấy link ảnh Card để mở/in.
```

`privateDescription` là text; cần đưa URL ảnh Card, không đưa binary image trực tiếp. Khi cập nhật phải giữ các nội dung cũ và thay/thêm section riêng, ví dụ:

```text
[HoaTheoMua - Thiệp]
- Card AI: https://storage.example.com/generated-cards/GC3.png
```

## Validate cần có

```text
- User hiện tại sở hữu Order và OrderDetails.
- OrderDetails thuộc đúng Order.
- Order đã Paid và đã có đơn Nhanh.vn để cập nhật.
- GeneratedCard tồn tại, thuộc user, chưa xóa và có imageUrl.
- Card là Card AI hợp lệ.
- Provenance của Card phù hợp Product/Combo hoặc GeneratedFlower của OrderDetails.
- Không thêm trùng GeneratedCardId.
- OrderDetails chưa từng submit Card AI/Card History.
- Tổng quantity Card submit không vượt OrderDetails.Number.
```

## Quantity của Card đã submit

`quantity` thuộc quan hệ giữa `OrderDetails` và `GeneratedCard`, không thuộc record `GeneratedCard` toàn cục. Cùng một Card có thể được dùng ở nhiều đơn với số bản in khác nhau.

Nếu thay `GeneratedCardIds: Guid[]` bằng mảng object, nên đặt tên rõ nghĩa như `GeneratedCards` hoặc `SubmittedCards`:

```json
[
  {
    "generatedCardId": "GCA",
    "quantity": 3
  }
]
```

Ví dụ cùng Card `GCA` được dùng ở hai đơn:

```json
// OrderA / OrderDetailA
[{ "generatedCardId": "GCA", "quantity": 3 }]

// OrderB / OrderDetailB
[{ "generatedCardId": "GCA", "quantity": 2 }]
```

Khi API Card lấy các Order từng dùng `GCA`, BE tìm object có `generatedCardId = GCA` trong từng `OrderDetails` rồi đọc `quantity` của chính object đó. Vì vậy kết quả vẫn chính xác:

```text
OrderA → GCA → quantity = 3
OrderB → GCA → quantity = 2
```

Rule dữ liệu:

```text
Trong cùng một OrderDetails, một GeneratedCardId chỉ xuất hiện một lần.
Nếu chọn lại cùng Card, update quantity của object hiện có; không append object trùng ID.
Tổng quantity của các Card đã submit không vượt OrderDetails.Number.
```

Mảng JSON object trả đúng dữ liệu theo từng Order, nhưng query ngược `Card → các Order` sẽ phải query trong JSON array. Nếu dữ liệu/report tăng lớn, bảng liên kết `OrderDetailGeneratedCards` sẽ tối ưu truy vấn và index hơn; còn JSON object vẫn là lựa chọn hợp lệ nếu quy mô hiện tại phù hợp.

Nếu cập nhật Nhanh.vn thất bại, lựa chọn Card không được mất. BE cần có cơ chế đánh dấu/chạy lại việc đồng bộ privateDescription; không thể dùng transaction chung giữa database nội bộ và API Nhanh.vn.

## Flow HandMade

```text
CreateOrder (Order Pending)
→ tạo GeneratedCard HandMade ngay.
→ lưu Card cùng quantity vào danh sách Card của OrderDetails.

Sau payment/Nhanh.vn tạo đơn
→ BE lấy imageUrl từ các Card ID đã có.
→ cập nhật privateDescription để nhân viên thấy và in.
```

Rule HandMade đã chốt:

```text
Mỗi bó hoa chỉ được một thiệp HandMade.
OrderDetails.Number = 2 bó → tối đa/chính xác theo lựa chọn là 2 thiệp HandMade.
Không được chọn vượt số bó.
```

## Điểm cần chốt khi implement

`OrderDetails` được submit Card AI/Card từ History đúng một lần. Nếu cần thay Card sau submit, đó phải là flow điều chỉnh riêng với điều kiện/quyền rõ ràng, không cho API submit cuối cùng ghi đè tự do.
