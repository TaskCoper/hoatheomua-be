# Context tạo thiệp AI từ bó hoa

Cập nhật: 2026-09-22.
Trạng thái: quyết định nghiệp vụ đã được người dùng xác nhận; không phải xác nhận code đã triển khai đầy đủ.

## Hai trường hợp bó hoa

Đối với tạo thiệp AI, chỉ phân biệt hai trường hợp:

| Trường hợp | Nguồn bó hoa | Ảnh tham chiếu để tạo thiệp |
|---|---|---|
| Bó thường | Product được khách chọn | `Product.ImageUrl` |
| Bó hoa AI | GeneratedFlower được khách chọn | `GeneratedFlower.ImageUrl` |

GeneratedFlower được chọn là kết quả cuối cùng của bó hoa đối với bước tạo thiệp. Không chia thêm trường hợp theo việc tăng nguyên liệu, không tăng nguyên liệu, tạo lần đầu hay tạo lại F1/F2/F3. “Kết quả cuối cùng” là kết quả khách chọn, không mặc định là bản được tạo gần nhất.

Thiệp phải tham chiếu hình ảnh của đúng bó hoa khách chọn. Với bó AI, không thay ảnh GeneratedFlower bằng ảnh Product gốc.

## Áp dụng cho CreateCardAi

- Create theo `orderDetailId + virtualId`: dữ liệu tạo thiệp lấy từ snapshot của đúng bó trong đơn hàng đã thanh toán. Khi chuẩn bị snapshot, cần bảo toàn nguồn ảnh tương ứng với hai trường hợp trên.
- Regenerate theo `sourceCardId`: kế thừa nguồn ảnh bó hoa đã chụp trong History của thiệp nguồn, theo luồng đã mô tả; không dùng ảnh thiệp làm ảnh bó hoa.
- Product gốc vẫn có thể được giữ để truy vết lineage. ID Product phục vụ truy vết không có nghĩa ảnh tham chiếu luôn phải là ảnh Product.

## Điều chỉnh so với tài liệu tổng thể ngày 2026-09-17

Bản nguyên văn người dùng cung cấp được lưu tại [Luồng tổng thể: Tạo hoa → Tạo lại hoa → Order → Tạo thiệp → Tạo lại thiệp](./AI-Flower-Order-Card-End-to-End.md). Bản đó giữ nguyên nội dung ngày 2026-09-17; quyết định cập nhật về hai loại bó hoa và ảnh tham chiếu được ghi trong file context này.

Tài liệu tổng thể trước đó dùng `snapShotCard.source.imageUrl` của Product P1 cho cả bó thường và bó AI. Quyết định ngày 2026-09-22 cập nhật cách chọn ảnh: bó thường dùng ảnh Product, bó AI dùng ảnh GeneratedFlower khách chọn.

Hình dạng cụ thể của các field source/lineage trong snapshot và History cần được đồng bộ khi triển khai. Context này chốt quy tắc nghiệp vụ về nguồn bó hoa và ảnh tham chiếu, chưa tự chốt thay đổi schema hoặc sửa code.

## Triển khai

Chi tiết source ảnh, config `imagegen.card_prompt`, job API, migration và kiểm thử: [AI Card Implementation](./AI-Card-Implementation.md).
