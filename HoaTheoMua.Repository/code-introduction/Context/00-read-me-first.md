# Context chốt để tiếp tục code — HoaTheoMua

> Cập nhật: 2026-09-12  
> Mục tiêu: để người mới đọc có thể hiểu ngay luồng Flower AI, Card, History và Order trước khi code.

## Cách đọc bộ tài liệu này

| File | Đọc khi cần hiểu |
|---|---|
| [01-domain-lineage.md](01-domain-lineage.md) | Bảng dữ liệu, `ClientHistory`, `RootID`, `BaseId`, các chuỗi generate/regenerate. |
| [02-flower-history-and-checkout.md](02-flower-history-and-checkout.md) | Tạo hoa AI, xem lịch sử hoa, lấy nhóm/phiên và checkout từ ảnh hoa. |
| [03-create-order-contract.md](03-create-order-contract.md) | Request `CreateOrder`, cách FE gửi Product/Combo, `GeneratedFlowerId` và Card. |
| [04-card-pricing-snapshot-and-fulfillment.md](04-card-pricing-snapshot-and-fulfillment.md) | Giá thiệp, SnapshotCard, HandMade, AI mới, AI từ History và xử lý sau thanh toán. |
| [05-implementation-notes-and-conflicts.md](05-implementation-notes-and-conflicts.md) | Việc cần đồng bộ entity/code, các rule bảo mật và tài liệu cũ không còn đúng. |
| [06-history-selection-and-admin-contracts.md](06-history-selection-and-admin-contracts.md) | Chọn thiệp từ History cho Checkout và các response/search/filter phía Admin. |
| [07-checkout-flow-and-audit.md](07-checkout-flow-and-audit.md) | Flow Checkout 5 bước, nguồn đặt hàng loại trừ nhau và kết quả rà soát. |

## Thứ tự ưu tiên khi có tài liệu mâu thuẫn

```text
1. Quyết định ghi trong các file Context mới đánh số `00` đến `06`.
2. Các file mới ở code-introduction/US-038, US-039, US-040.
3. docs/AI_Card_Context.md và docs/AI_Flower_Context.md.
4. TDD/UT hoặc tài liệu cũ hơn.
5. Entity/code hiện tại nếu chưa được cập nhật theo thiết kế mới.
```

Lý do: một số entity và docs cũ chưa theo kịp quyết định mới. Không tự lấy hành vi của code cũ làm rule nghiệp vụ nếu nó trái với bộ context này.

## Bức tranh tổng thể

```text
Nguồn checkout (chỉ chọn một)
  ├─ Customer chọn Product/Combo đang bán
  └─ Customer chọn GeneratedFlower đã tạo trước đó
       → kiểm tra Combo nguồn còn bán được

Cả hai cách sau đó đi chung flow
  → đi vào CreateOrder cùng một flow
  → mỗi dòng hoa tạo một OrderDetails
  → khách có thể không chọn thiệp hoặc chọn nhiều thiệp cho dòng đó
  → thanh toán
  → Card AI chỉ được tạo sau khi payment thành công
```

## Những rule ngắn phải nhớ

- Chỉ **Combo** mới được dùng để tạo ảnh hoa AI.
- Một Checkout chỉ bắt đầu từ Product/Combo **hoặc** một GeneratedFlower; không vừa chọn Combo trực tiếp vừa gắn một Flower AI trong cùng lựa chọn nguồn.
- `productId` là Product/Combo khách mua; `itemId` không phải Combo ID.
- Nếu checkout từ ảnh hoa AI, FE gửi cả `itemId`, `productId` và `generatedFlowerId`.
- `RootID` là Combo nguồn chung; `BaseId` là nguồn trực tiếp của lần generate.
- Thiệp HandMade không gọi AI, luôn viết tay, không dùng Size và được tạo ngay khi `CreateOrder`.
- Thiệp AI mới và thiệp từ History chốt giá/snapshot khi tạo Order, nhưng tạo `GeneratedCard` sau payment thành công.
- FE chỉ gửi lựa chọn của khách. Giá, prompt, snapshot, ID Card đã tạo đều do BE tự lấy/tính/lưu.
- FE không được gọi API update Order để tự ghi `GeneratedCardIds`.
