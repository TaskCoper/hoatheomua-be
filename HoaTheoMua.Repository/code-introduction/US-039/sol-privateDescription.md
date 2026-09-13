# Giải pháp `privateDescription` Nhanh.vn

Bạn nói đúng về flow. Nếu gộp “Card AI mới” và “Card từ History” thành cùng một cách xử lý sau payment, thì có 6 nhóm chính:

| Nguồn hoa | Loại Card | Cập nhật Nhanh.vn khi nào |
|---|---|---|
| Product thường (`GeneratedFlowerId = null`) | Không Card | Webhook tạo đơn; không có link Flower/Card |
| GeneratedFlower | Không Card | Webhook tạo đơn; có link Flower AI |
| Product thường | HandMade | Webhook tạo đơn; có link Card HandMade + quantity |
| GeneratedFlower | HandMade | Webhook tạo đơn; có link Flower AI + Card HandMade + quantity |
| Product thường | AI / History | Webhook tạo đơn chưa có Card; sau đó API submit version cuối cùng cập nhật link Card + quantity |
| GeneratedFlower | AI / History | Webhook tạo đơn có link Flower AI, chưa có Card; API submit version cuối cùng cập nhật thêm link Card + quantity |

Điểm đúng cần nhớ:

```text
GeneratedFlowerId có thể null.
Card có thể null.
Nếu GeneratedFlowerId có giá trị thì ProductId vẫn là Combo nguồn để tạo OrderDetails.
```

Nếu tách riêng AI mới và History thành hai loại business, sẽ thành 8 trường hợp; nhưng về cập nhật Nhanh.vn chúng giống nhau nên gộp thành 6 là hợp lý.

Và giới hạn ký tự `privateDescription` áp dụng cho tất cả các trường hợp có link Flower/Card; phương án chống bị cắt hiện vẫn chưa chốt.

---

Cách để không rối và không miss case là: xem `privateDescription` là bản render từ dữ liệu Order hiện tại, không phải nơi tự nối chuỗi từng chỗ.

Lưu ý: hàm này hiện nằm ở `HoaTheoMua.Service/Fulfillment/Service.cs:911`, còn Webhook chỉ chuyển Order đã Paid vào flow Fulfillment.

Flow nên là:

```text
Order DB là nguồn dữ liệu thật
→ một hàm BuildPrivateDescriptionAsync duy nhất đọc toàn bộ OrderDetails
→ tạo ra privateDescription hoàn chỉnh
→ dùng cùng hàm ở cả:
   1. tạo đơn Nhanh.vn sau payment;
   2. submit Card version cuối cùng để update đơn Nhanh.vn.
```

Hàm build không cần phân nhánh 6 case thủ công. Với từng `OrderDetails`, chỉ áp dụng ba rule độc lập:

```text
1. Có GeneratedFlowerId?
   → có: thêm thông tin/link Flower AI.
   → không: không thêm phần Flower AI.

2. Có Card đã chốt chưa?
   → HandMade: đã có ngay từ CreateOrder.
   → AI/History đã submit: đã có GeneratedCard + quantity.
   → AI/History chưa submit: chưa có Card cuối cùng.

3. Có Card đã chốt
   → thêm link ảnh Card + quantity.
```

Kết quả tự bao phủ các case:

| Dữ liệu hiện có | Nội dung render lên Nhanh |
|---|---|
| Không Flower, không Card | Thông tin đơn cơ bản |
| Có Flower, không Card | Cơ bản + link Flower |
| Không Flower, HandMade | Cơ bản + link Card/quantity |
| Có Flower, HandMade | Cơ bản + link Flower + Card/quantity |
| AI/History chưa submit | Cơ bản, có thể thêm marker “chờ khách chốt thiệp” |
| AI/History đã submit | Cơ bản + Flower nếu có + link Card/quantity |

Cụ thể:

```text
Webhook payment thành công
→ Fulfillment tạo đơn Nhanh.vn
→ gọi BuildPrivateDescriptionAsync(order,...)
→ lúc này render Flower nếu có và HandMade nếu có.
→ Card AI/History chưa submit thì chưa có link Card.

API submit version cuối cùng
→ lưu GeneratedCard + quantity vào OrderDetails
→ load lại toàn bộ Order
→ gọi lại chính BuildPrivateDescriptionAsync(order,...)
→ gọi Nhanh.vn update privateDescription bằng bản render mới.
```

Điểm quan trọng: API submit không tự append text vào private description cũ. Nó phải render lại toàn bộ từ DB, nếu không sẽ dễ bị link trùng, quantity cũ, hoặc miss Flower/HandMade.

Về giới hạn ký tự Nhanh.vn: builder nên kiểm tra độ dài trước khi gọi Nhanh. Nếu vượt giới hạn, không tự cắt mù dữ liệu Card/quantity; trả trạng thái “over limit” để dùng phương án team sẽ chốt sau. Như vậy chỉ cần thay chiến lược render tại một chỗ, không phải sửa webhook và submit API riêng lẻ.
