# Assume US — cụm User Story và bối cảnh chung

> Bản đồ này nhóm các US theo quan hệ nghiệp vụ. “API” chi tiết theo từng US nằm trong `assume-flow.md`; tài liệu nguồn không định nghĩa URL/method endpoint.

## 1. Hành trình đặt hoa Custom AI

```text
STORY-003 Danh sách Combo
    └─ chọn Combo / Tạo mẫu hoa
       └─ STORY-030 Khởi tạo yêu cầu mẫu hoa (Combo nguồn + tên + 1 Mockup, Draft)
          └─ STORY-033 Tạo mẫu hoa AI
             ├─ AI job + quota Hoa AI + snapshot + output gắn logo
             ├─ STORY-034 Tải PNG mẫu hoa
             ├─ STORY-040 Lịch sử yêu cầu/kết quả mẫu hoa
             │  └─ mở lại kết quả → STORY-034 hoặc Đặt hàng ngay
             └─ STORY-038 Checkout tạm từ đúng 1 mẫu hoa
                ├─ cấu hình thiệp mới → cụm Thiệp Custom
                ├─ chọn thiệp đã có → STORY-042
                └─ STORY-039 Hoàn tất Checkout → Order PENDING → thanh toán → SUCCESS
```

- STORY-033 có một folder `toa-…` là bản trùng/cũ; flow chính dùng folder `tao-…` có đầy đủ BR.
- STORY-030 tạo Draft, không tự gọi AI. STORY-033 là lúc trừ quota và tạo job.
- Tồn kho được kiểm tra lại khi khởi tạo Checkout; Order chỉ sinh lúc hoàn tất Checkout. Payment có retry 24 giờ rồi tự hủy nếu chưa trả.

## 2. Thiệp Custom: cấu hình, generate và tái sử dụng

```text
STORY-038 Checkout
    ├─ STORY-035 Cấu hình thiệp mới
    │  └─ Thiệp miễn phí | Thiệp Custom (nội dung + hình thức + Size + Template + ảnh)
    │     └─ STORY-039 snapshot vào Order sau thanh toán
    │        └─ STORY-036 Tạo lại/generate thiệp AI sau Payment SUCCESS
    │           └─ History thiệp mới (PNG, không logo)
    └─ STORY-042 Chọn thiệp History cùng mẫu hoa
       └─ gắn liên kết tạm với Checkout → STORY-039

STORY-045 Lịch sử thiệp customer
    └─ chi tiết/history + Order liên quan → STORY-041 Tải thiệp PNG
```

- History chỉ được chọn nếu thuộc khách hàng, có output/file hợp lệ và cùng mẫu hoa Checkout.
- Chọn/tải History không gọi AI, tạo ảnh/history mới hay trừ quota. History không có thao tác Tạo lại.
- Quota Thiệp AI tách quota Hoa AI; nội dung sau thanh toán không vượt số từ khách đã trả; mỗi mẫu hoa giới hạn 3 lượt thiệp.
- Generate thiệp cần idempotent; chỉ có 1 quota/job/ảnh/history cho một request. AI chỉ trang trí trong vùng Template cho phép, không được đổi nội dung, layout, phong cách hay ảnh đính kèm.

## 3. Admin giám sát kết quả AI

```text
AI Custom / Thiệp
  STORY-044 Danh sách thiệp AI → STORY-043 Chi tiết → STORY-046 Tải PNG

AI Custom / Mẫu hoa
  STORY-047 Danh sách mẫu hoa AI → STORY-048 Chi tiết
```

- Cả hai cụm đọc History/output hợp lệ, hỗ trợ search/filter/paging/lightbox; chi tiết hiển thị owner/snapshot/Order liên quan theo quyền Admin.
- Tải Admin cũng lấy file đã generate, không gây AI job hoặc thay quota.

## 4. Admin quản trị Mockup — đầu vào của STORY-030/033

```text
STORY-050 Danh sách Mockup
    ├─ STORY-051 Thêm Mockup (mặc định Active)
    ├─ STORY-052 Active ↔ Inactive
    └─ STORY-053 Xóa mềm

Mockup Active, chưa xóa → khách hàng có thể chọn trong STORY-030
Mockup Inactive/xóa → không hiện ở lựa chọn mới; snapshot/lịch sử cũ vẫn giữ
```

## 5. Admin cấu hình Thiệp Custom — đầu vào STORY-035/036

```text
Kích thước thiệp:
  STORY-054 Danh sách → STORY-055 Thêm | STORY-062 Cập nhật
                         | STORY-063 Đổi status | STORY-056 Xóa mềm

Template/mẫu thiệp:
  STORY-057 Danh sách → STORY-058 Thêm | STORY-065 Cập nhật
                         | STORY-059 Đổi status | STORY-060 Xóa mềm

Phụ phí viết tay:
  STORY-061 Danh sách → STORY-067 Thêm | STORY-068 Cập nhật
                         | STORY-064 Đổi status | STORY-066 Xóa mềm
```

- Active là điều kiện dùng cho cấu hình mới; Inactive/xóa không hồi tố snapshot Order/History.
- Size phải không trùng; Template có thể trùng tên; phụ phí Active không được overlap khoảng số từ.

## 6. Admin cấu hình thêm cho Hoa Custom AI

```text
Hình ảnh kiểu gói:
  STORY-074 Danh sách → STORY-075 Thêm (Inactive)
                         | STORY-076 Cập nhật
                         | STORY-078 Đổi status
                         | STORY-077 Xóa

Size Hoa Custom AI:
  STORY-079 Danh sách → STORY-080 Thêm (Inactive)
                         | STORY-081 Cập nhật
                         | STORY-083 Đổi status
                         | STORY-082 Xóa
```

- Bản ghi Active mới được dùng/hiển thị cho customer trong bước chọn tương ứng của luồng tạo mẫu hoa; Inactive bị loại khỏi lựa chọn mới.
- Kiểu gói nhận JPG/PNG tối đa 10 MB; Size Hoa AI không trùng và số Combo là số nguyên dương.

## Quy tắc xuyên suốt

```text
Customer data  → luôn kiểm tra ownership
Config mới     → Active + chưa xóa mới được dùng trong luồng mới
AI generate    → validate → quota/concurrency → snapshot → job → output/history
History/Order  → snapshot là nguồn lịch sử, không bị config/xóa sau này làm thay đổi
Download       → trả file đã generate, không tạo AI/file/history/quota mới
```
