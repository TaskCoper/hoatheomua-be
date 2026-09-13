# Group phiên tạo Card AI

## Hai khái niệm không được trộn

```text
regeneratedFromCardId
→ Card AI nào là Card cha trực tiếp.
→ Dùng để xem lineage toàn bộ qua nhiều lần tạo.

generationSessionId
→ Mã Guid của một phiên user đang tạo nhiều version để so sánh.
→ Dùng để group đúng các Card trong cùng một màn hình/phiên thao tác.
```

Ví dụ:

```text
Phiên S1: GC1 → GC2
Phiên S2: GC3

Lineage đầy đủ: GC1 → GC2 → GC3
```

`GC3` có thể được tạo từ `GC2`, nhưng vẫn thuộc phiên `S2`; vì vậy lineage và session là hai cách nhóm khác nhau.

## Flow FE và BE

### 1. Bắt đầu phiên mới

```text
FE gọi API tạo Card AI mới hoặc tạo lại từ Card History
→ không gửi generationSessionId
→ BE tự sinh Guid mới, ví dụ S1
→ BE lưu S1 vào ClientHistory.Metadata của Card tạo thành công
→ BE trả GeneratedCard và S1 cho FE
```

### 2. Tạo thêm version trong cùng phiên

```text
FE đang mở màn hình so sánh và giữ S1 trong state
→ user chọn GC1 để tạo biến thể
→ FE gửi regeneratedFromCardId = GC1 và generationSessionId = S1
→ BE tạo GC2, lưu parent = GC1 và session = S1
```

BE chỉ cho dùng lại session nếu Card/session thuộc đúng customer hiện tại.

### 3. Thoát màn hình rồi tạo lại

```text
User thoát màn hình
→ FE mất state S1, không lưu draft/session tự động

Ngày sau user chọn GC2 từ History để tạo lại
→ FE chỉ gửi regeneratedFromCardId = GC2
→ không có generationSessionId
→ BE sinh S2 và tạo GC3 thuộc S2
```

Kết quả:

```text
Session S1: GC1 → GC2
Session S2: GC3
Lineage:    GC1 → GC2 → GC3
```

Nếu UI có chức năng **Tiếp tục phiên cũ**, FE phải lấy `generationSessionId` từ Card History rồi gửi lại ID đó. Nếu không, mọi thao tác bắt đầu từ History được hiểu là phiên mới.

## Dữ liệu cần lưu/trả

Trong `ClientHistory.Metadata` của Card AI thành công:

```json
{
  "regeneratedFromCardId": "generated-card-cha-id hoặc null",
  "generationSessionId": "guid-cua-phien"
}
```

Card History/API group đọc `generationSessionId` để lấy các Card cùng phiên; dùng `regeneratedFromCardId` để dựng quan hệ cha-con trong phiên hoặc xem lineage xuyên phiên.

`generationSessionId` thuộc flow API tạo/tạo lại Card AI và Card History; không thuộc response hay request của `CreateOrder`.
