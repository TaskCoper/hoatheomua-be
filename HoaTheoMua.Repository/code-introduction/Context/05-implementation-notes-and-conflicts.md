# 05 — Hướng dẫn implement và điểm cần đồng bộ

## 1. Việc code cần làm theo thứ tự

```text
1. Đồng bộ entity/migration theo các field đã chốt.
2. Có GetLimitCoreProduct dùng chung.
3. Làm API checkout availability của GeneratedFlower.
4. Bổ sung request CreateOrder: GeneratedFlowerId + Items[].Cards.
5. Trong CreateOrder: validate source, tính giá, tạo snapshot và xử lý HandMade.
6. Trong webhook/payment-success: gọi Card AI và cập nhật GeneratedCardIds bằng service nội bộ.
7. Làm các API History đọc đúng RootID/BaseId/OutputId.
```

## 2. Entity/migration cần phản ánh

| Nơi | Cần có theo context mới |
|---|---|
| `ClientHistory` | `RootID`/`Root` được mapping; Flower generate đầu `BaseId = RootID = ComboId`; Flower regenerate giữ Root và đổi Base sang Flower cha. |
| `OrderDetails` | `GeneratedFlowerId` riêng, `GeneratedCardIds` dạng mảng riêng, `SnapshotCard` JSON riêng. |
| Create Order DTO | `Items[].GeneratedFlowerId`, `Items[].Cards`; không có `Cards[].ItemId`. |
| Card snapshot | JSON array theo đúng schema ở file 04. |

Tên property thực tế (`Root` hay `RootID`, `GeneratedFlower` hay `GeneratedFlowerId`) cần nhất quán giữa entity, migration, LINQ và DTO. Đừng sửa một phía mà quên phía khác.

## 3. Các validation không được bỏ

### Flower source trong CreateOrder

- customer sở hữu Flower;
- Flower có History Type flower;
- `History.RootID == item.ProductId`;
- `item.ProductId` là Combo active/chưa xóa;
- Core của Combo còn bán được.

### Card source từ History

- `regeneratedFromCardId` trỏ tới `GeneratedCard.Id`, không phải History ID;
- Card thuộc customer;
- Có Card History hợp lệ;
- Chỉ cho Card AI làm nguồn; HandMade bị từ chối cho regenerate AI;
- giá mới luôn tính từ lựa chọn mới, không dùng giá cũ.

### Bảo mật

- Lấy user từ JWT/current user, không tin UserId từ FE.
- Không tin giá, snapshot, prompt, word count hay `GeneratedCardIds` FE gửi.
- API đọc History/ảnh/order phải kiểm tra ownership trước khi trả dữ liệu.

## 4. API đã định hình trong session

| API | Ý nghĩa |
|---|---|
| `GET /api/v1/generated-flowers/{generatedFlowerId}/checkout-availability` | Kiểm tra ảnh hoa AI còn đi Checkout được và trả Combo nguồn/trạng thái vật liệu. |
| `GET /api/v1/flower-histories` | Danh sách Flower AI mới nhất trước. |
| `GET /api/v1/flower-histories/{id}/root-group` | Nhóm mọi Flower chung Root Combo, từng ảnh kèm Order dùng chính ảnh đó. |
| `GET /api/v1/flower-histories/{id}/session` | Các version trong đúng một chuỗi generate/regenerate. |
| `GET /api/v1/card-histories` | Lịch sử Card của customer; bao gồm `card` và `handmade_card` theo quyết định mới. Có thể filter `formType` là gõ máy/viết tay; không filter thì lấy tất cả. |
| `GET /api/v1/card-histories/{generatedCardId}` | Chi tiết Card và Order liên quan. |

## 5. Tài liệu cũ có điểm trái với quyết định mới

Không copy nguyên các rule sau từ tài liệu cũ mà không đối chiếu:

| Tài liệu/ý cũ | Quyết định mới phải dùng |
|---|---|
| Snapshot Card có `senderName`, `receiverName`. | Đã bỏ hai field này. |
| Snapshot Card cần `generatedCardId` để map object. | Không có field này trong snapshot. ID nằm ở `OrderDetails.GeneratedCardIds` riêng sau khi Card được tạo. |
| Card AI phải có `rawImage` ngay từ Order/snapshot. | Không cần. Order chỉ chốt snapshot/giá; AI chạy sau payment và output hiện có `imageUrl`. |
| `generated_flowers.input_snapshot` là nguồn chuẩn để resolve Combo. | Dùng `ClientHistory.RootID` làm Combo nguồn chuẩn. |
| `OrderDetails.GeneratedFlowerIds` là mảng. | Quyết định mới dùng `GeneratedFlowerId` đơn cho một dòng OrderDetails. |
| Cards ở cấp `CreateOrderRequest.Cards` cần `Cards[].ItemId`. | Quyết định mới đặt `Cards` trong `Items[]`, không có Card itemId riêng. |
| HandMade có trong History nhưng có thể regenerate AI. | HandMade có thể hiển thị History, nhưng không là source cho regenerate AI. |

## 6. Điểm cần chốt thêm nếu implement gặp

Hai điểm này chưa nên tự suy diễn khi code:

1. **Snapshot của HandMade:** HandMade đã được tạo ngay trong CreateOrder nên không cần Snapshot để gọi AI. Có thể vẫn lưu snapshot phục vụ audit/hiển thị, nhưng phải chốt rõ schema và mục đích trước khi thêm.
2. **Retry Card AI sau payment:** cần thống nhất trạng thái/marker job để một webhook bị gửi lại không tạo trùng Card. Dù không tạo bảng mới, ít nhất service phải idempotent theo OrderDetails + snapshot item đang xử lý.

## 7. Tóm tắt một câu cho người code Order

```text
CreateOrder chỉ chốt thứ khách chọn và dữ liệu/giá bất biến; HandMade được tạo luôn, còn Card AI được tạo sau payment bằng snapshot; mọi ID kết quả do BE tự ghi lại vào OrderDetails.
```
