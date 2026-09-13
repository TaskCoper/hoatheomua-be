• Cụm STORY-074 đến STORY-083 là bộ Admin cấu hình đầu vào cho flow Flower Custom AI mới: kiểu gói và Size theo số lượng Combo.

  Flow Flower AI mới của customer
  → chọn Size (do cụm 079–083 cấu hình)
  → chọn nhiều Combo theo số lượng Size cho phép
  → chọn kiểu gói (do cụm 074–078 cấu hình)
  → chọn Mockup, giấy gói, ruy băng
  → tạo GeneratedFlower

  ## Cụm 1 — Quản trị hình ảnh kiểu gói

  STORY-074 xem danh sách kiểu gói
      |-> STORY-075 thêm kiểu gói mới (JPG/PNG, ≤10MB, mặc định Inactive)
      |-> STORY-076 sửa ảnh/tên/mô tả, giữ nguyên trạng thái
      |-> STORY-077 xóa có xác nhận
      `-> STORY-078 đổi Active/Inactive
           → Active mới được customer thấy khi chọn kiểu gói

  - STORY-074: Admin xem/lọc danh sách ảnh kiểu gói: preview, tên, mô tả, trạng thái.
  - STORY-075: Thêm ảnh kiểu gói; ảnh bắt buộc JPG/PNG tối đa 10MB, tên bắt buộc, tạo xong mặc định Inactive.
  - STORY-076: Sửa preview/tên/mô tả; không tự đổi Active/Inactive.
  - STORY-077: Xóa kiểu gói sau xác nhận; không làm thay đổi dữ liệu cũ đã snapshot.
  - STORY-078: Bật/tắt kiểu gói; chỉ Active mới hiện trong luồng customer chọn kiểu gói.

  ## Cụm 2 — Quản trị Size Flower Custom AI

  STORY-079 xem danh sách Size + số lượng Combo
      |-> STORY-080 thêm Size mới (unique, quantity Combo > 0, mặc định Inactive)
      |-> STORY-081 sửa Size / số lượng Combo, giữ trạng thái
      |-> STORY-082 xóa có xác nhận
      `-> STORY-083 đổi Active/Inactive
           → Active mới được customer thấy khi chọn Size

  - STORY-079: Admin xem/tìm/lọc danh sách cấu hình Size + số lượng Combo.
  - STORY-080: Thêm Size, ví dụ S/M/L; số lượng Combo là số nguyên dương, Size không trùng, mặc định Inactive.
  - STORY-081: Sửa Size hoặc số lượng Combo; giữ nguyên trạng thái hiện tại.
  - STORY-082: Xóa Size có xác nhận; không sửa dữ liệu Flower cũ đã snapshot.
  - STORY-083: Bật/tắt Size; chỉ Size Active mới hiện cho customer trong flow chọn Size.

  Điểm cần liên hệ với flow mới bạn vừa chốt: Size trong cụm 079–083 chính là cấu hình quyết định số Combo/bó mà user có thể phải chọn khi tạo một GeneratedFlower. Còn cụm 074–078 cung cấp danh sách kiểu gói hợp lệ để
  user chọn cho mẫu hoa đó.