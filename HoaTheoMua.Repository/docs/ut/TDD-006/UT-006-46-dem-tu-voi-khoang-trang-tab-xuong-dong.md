# UT-006-46: Đếm đúng số từ khi lời chúc có nhiều loại khoảng cách

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Đếm đúng số từ khi lời chúc có nhiều loại khoảng cách
- **Ghi chú**: Khoảng trắng liên tiếp, phím tab và xuống dòng chỉ dùng để ngăn cách từ, không làm tăng số từ.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-006-46
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: AI CARD - Tạo thiệp thiết kế AI
- **Unit under test (bắt buộc)**: `AICardService.CreateAsync` (phần chương trình tiếp nhận và tạo thiệp AI mới)
- **Loại**: Boundary
- **Precondition / Mock setup**:
  - Khách hàng `customer-01` đã đăng nhập và tài khoản đang hoạt động.
  - Sản phẩm `product-001` tồn tại, chưa bị xóa, đang bán, có công thức hoa, có phần lõi cần thiết và còn 10 sản phẩm có thể bán.
  - Mẫu thiệp `template-001` thuộc loại AI, chưa bị xóa và đang hoạt động.
  - Cấu hình kích thước có đúng một kích thước chung: rộng 15, cao 25, giá 15.000 đồng và tối đa 100 từ.
  - Bảng giá viết tay có ba mức: 0-35 từ thêm 0 đồng, 36-70 từ thêm 39.000 đồng, 71-100 từ thêm 69.000 đồng.
  - Hướng dẫn dành cho AI loại `card` có nội dung dùng được. Phần tạo ảnh trả `rawImage=raw.png` và `imageUrl=final.png`; việc lưu ảnh và dữ liệu đều thành công.
- **Các trường hợp cần kiểm tra**:
  - Lời chúc là `Chúc    bạn` rồi có tab trước `luôn` và xuống dòng trước `vui`; kết quả phải là 4 từ.
- **Input**:
  ```text
  POST /api/ai-cards
  messageContent=Chúc    bạn\tluôn\nvui
  formType=go_may
  ```
- **Expected output (bắt buộc)**:
  ```text
  Hệ thống báo thành công và lưu wordCount=4.
  Khoảng trắng liên tiếp được tính như một dấu ngăn cách; tab và xuống dòng cũng chỉ ngăn cách từ.
  Giá chỉ gồm basePrice=15000 vì hình thức gõ máy không có phụ phí.
  ```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Đếm sai có thể chọn nhầm mức giá hoặc từ chối nội dung vẫn còn vừa. Trường hợp này xác nhận cách đếm giống cách con người tách các từ.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-006
- **Section**: BR-006-08, BR-006-10, BR-006-11
- **Ghi chú**: Quy tắc trực tiếp quy định kết quả phải kiểm tra trong tài liệu này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Tạo thiệp thiết kế
- **Ghi chú**: Nhu cầu của khách hàng mà trường hợp kiểm tra này bảo vệ.
