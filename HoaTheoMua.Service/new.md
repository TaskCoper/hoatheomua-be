# TÀI LIỆU ĐẶC TẢ HỆ THỐNG API

## 1. Tổng quan hệ thống
Tài liệu này mô tả cấu trúc dữ liệu và đặc tả API cho ba phân hệ chính của hệ thống. Nội dung được tổng hợp từ ảnh ERD và file đặc tả API được cung cấp.

### Các phân hệ chính
| Phân hệ | Mục đích | Đối tượng sử dụng chính |
| :--- | :--- | :--- |
| **News API** | Tạo, cập nhật, xóa mềm bài viết tin tức; liên kết bài viết với danh mục. | Admin, Staff, người tạo bài viết |
| **Category News API** | Quản lý danh mục tin tức dạng cây cha - con. | Admin, Staff |
| **Company Address API** | Quản lý địa chỉ công ty, chi nhánh và trụ sở chính. | Admin |

### 1.1 Nguyên tắc thiết kế
* Dữ liệu quan trọng không xóa cứng, ưu tiên **Soft Delete** để bảo toàn lịch sử.
* Các thao tác tạo/cập nhật có liên quan nhiều bảng cần chạy trong **Database Transaction**.
* **Frontend (FE)** chịu trách nhiệm gửi đúng dữ liệu và hiển thị trạng thái.
* **Backend (BE)** chịu trách nhiệm kiểm tra quyền, validation, transaction và ràng buộc nghiệp vụ.
* Những trường như `created_at`, `updated_at`, `CreatedAt`, `UpdatedAt` phải được Backend tự động gán, không để Frontend tự truyền.

---

## 2. Cấu trúc dữ liệu và Quan hệ

### 2.1 Danh sách bảng dữ liệu
| Bảng | Khóa chính | Trường chính | Vai trò |
| :--- | :--- | :--- | :--- |
| **User** | `UserID` | Email, Phone, Password(hash), Role, IsVerify, IsDelete, created_at, updated_at | Lưu thông tin tài khoản, quyền và trạng thái xác thực/xóa mềm. |
| **News** | `NewsID` | UserId, Title, Description, ViewCount, PictureUrl, Status, created_at, updated_at | Lưu bài viết tin tức. Mỗi bài viết thuộc về một người tạo. |
| **CategoryNews** | `CategoryNewsID` | UserID, ParentID, Title | Lưu danh mục tin tức; hỗ trợ danh mục cha - con thông qua ParentID. |
| **CategoryNewsDetail** | `CategoryNewsDetailID` | CategoryNewsID, NewsID | Bảng trung gian thể hiện quan hệ nhiều - nhiều ($N - N$) giữa News và CategoryNews. |
| **CompanyAddress** | `Id` (Guid) | AddressLine, Ward, District, City, Country, Latitude, Longitude, IsPrimary, CreatedAt, UpdatedAt, IsDeleted | Bảng quản lý địa chỉ công ty/chi nhánh (không xuất hiện trong ERD nhưng có trong API). |

### 2.2 Quan hệ giữa các bảng
* **User - News ($1 - N$):** Một User có thể tạo nhiều News. Mỗi News có một `UserId` tham chiếu về `User.UserID`.
* **User - CategoryNews ($1 - N$):** Một User có thể tạo nhiều CategoryNews. `CategoryNews.UserID` tham chiếu về `User.UserID`.
* **CategoryNews - CategoryNews ($1 - N$ tự tham chiếu):** `ParentID` cho phép tạo cấu trúc cây danh mục cha - con. `ParentID` có thể null đối với danh mục gốc.
* **News - CategoryNews ($N - N$):** Một bài viết có thể thuộc nhiều danh mục, một danh mục có thể chứa nhiều bài viết. Quan hệ đi qua bảng trung gian `CategoryNewsDetail`.

---

## 3. News API

### 3.1 Create News
* **Endpoint:** `POST /news`
* **Mô tả:** Tạo bài viết tin tức mới.

#### Request Payload
```json
{
  "title": "Ra mắt dòng sản phẩm hoa tươi công nghệ mới năm 2026",
  "description": "Chi tiết về quy trình chăm sóc và bảo quản dòng sản phẩm mới giúp hoa tươi lâu hơn...",
  "pictureUrl": "[https://hoatheomua.com/images/news/new-product-2026.png](https://hoatheomua.com/images/news/new-product-2026.png)",
  "status": "Draft",
  "categoryIds": [1, 5, 12]
}
Validation Rules & Business LogicFieldStateData TypeValidation Rules & Business LogictitleRequiredStringTừ 5 đến 255 ký tự. Không được để trống.descriptionRequiredText StringNội dung bài viết; có thể chấp nhận HTML hoặc Markdown.pictureUrlOptionalString URLURL hợp lệ, dùng làm ảnh đại diện hiển thị ngoài danh sách.statusRequiredStringChỉ chấp nhận các giá trị: Draft, Published hoặc Archived.categoryIdsRequiredArray[Int]Mảng chứa ít nhất một ID danh mục hợp lệ đang tồn tại trong Database.Response Body (201 Created)JSON{
  "message": "Tạo bài viết tin tức mới thành công.",
  "data": {
    "newsId": 1024,
    "title": "Ra mắt dòng sản phẩm hoa tươi công nghệ mới năm 2026",
    "description": "Chi tiết về quy trình chăm sóc và bảo quản dòng sản phẩm mới giúp hoa tươi lâu hơn...",
    "viewCount": 0,
    "pictureUrl": "[https://hoatheomua.com/images/news/new-product-2026.png](https://hoatheomua.com/images/news/new-product-2026.png)",
    "status": "Draft",
    "userId": 57,
    "created_at": "2026-06-11T08:58:55Z",
    "updated_at": null
  }
}
Luồng xử lý dữ liệuPhía Frontend:Gửi request kèm accessToken qua Cookie hoặc Header.Nhận phản hồi và hiển thị thông báo thành công.Điều hướng người dùng sang trang chi tiết bài viết vừa tạo.Phía Backend:Giải mã accessToken để lấy UserId người tạo.Kiểm tra mảng categoryIds có tồn tại và hợp lệ trong Database hay không.Khởi chạy Database Transaction.Thêm mới bản ghi News với ViewCount = 0 và IsDelete = false.Chèn các bản ghi liên kết tương ứng vào CategoryNewsDetail.Commit Transaction và trả dữ liệu thành công.3.2 Update NewsEndpoint: PUT /news/:idMô tả: Cập nhật thông tin bài viết.Request PayloadJSON{
  "title": "Cập nhật dòng sản phẩm hoa tươi công nghệ mới 2026",
  "description": "Nội dung bài viết đã được chỉnh sửa bổ sung các lưu ý khi vận chuyển...",
  "pictureUrl": "[https://hoatheomua.com/images/news/new-product-2026-v2.png](https://hoatheomua.com/images/news/new-product-2026-v2.png)",
  "status": "Published",
  "categoryIds": [1, 8]
}
📌 Lưu ý nghiệp vụ:Tham số :id trên URL Path phải là số nguyên dương hợp lệ.Các trường trong Request Payload tuân thủ bộ validation tương tự như hàm Create News.Chỉ chủ sở hữu bài viết (khớp UserId) hoặc tài khoản có quyền Admin mới được phép cập nhật bài viết.Response Body (200 OK)JSON{
  "message": "Cập nhật thông tin bài viết tin tức thành công."
}
Luồng xử lý dữ liệuPhía Frontend:Truyền id bài viết trên URL Path.Gửi dữ liệu thay đổi trong Request Body.Hiển thị thông báo cập nhật thành công.Phía Backend:Kiểm tra bài viết tồn tại và trạng thái chưa bị xóa mềm (IsDelete == false).Kiểm tra quyền sở hữu hoặc quyền Admin.Khởi chạy Database Transaction.Cập nhật dữ liệu trong bảng News và gán updated_at = NOW().Xóa toàn bộ liên kết danh mục cũ trong bảng CategoryNewsDetail.Chèn lại danh sách categoryIds mới vào CategoryNewsDetail.Commit Transaction.3.3 Delete NewsEndpoint: DELETE /news/:idMô tả: Xóa bài viết tin tức (Xóa mềm).Response Body (200 OK)JSON{
  "message": "Xóa bài viết thành công."
}
Luồng xử lý dữ liệuPhía Frontend:Gửi request chứa ID bài viết cần xóa trên URL.Cập nhật lại giao diện, loại bỏ bài viết khỏi danh sách hiển thị.Phía Backend:Xác thực token và kiểm tra quyền truy cập (Admin/Staff/Owner).Tìm bài viết theo NewsID.Thực hiện Soft Delete: Cập nhật IsDelete = true và updated_at = NOW().Tuyệt đối không xóa cứng để bảo toàn dữ liệu liên quan ở các bảng trung gian.Trả thông báo thành công.4. Category News API4.1 Create CategoryNewsEndpoint: POST /category-newsMô tả: Tạo danh mục tin tức mới.Request PayloadJSON{
  "title": "Hoa Cưới Cầm Tay",
  "parentId": 4
}
Validation Rules & Business LogicFieldStateData TypeValidation Rules & Business LogictitleRequiredStringTừ 2 đến 100 ký tự. Tên danh mục không được để trống.parentIdOptionalIntegerCó thể null hoặc bỏ trống đối với danh mục gốc. Nếu truyền thì phải là số nguyên dương và phải tồn tại trong DB.Response Body (201 Created)JSON{
  "message": "Tạo danh mục tin tức mới thành công.",
  "data": {
    "categoryNewsId": 18,
    "title": "Hoa Cưới Cầm Tay",
    "parentId": 4,
    "userId": 57
  }
}
Luồng xử lý dữ liệuPhía Frontend:Gửi title và parentId (nếu danh mục này là con của danh mục khác).Cập nhật lại Tree View trên màn hình quản trị sau khi nhận phản hồi.Phía Backend:Kiểm tra quyền tạo danh mục (thường chỉ dành cho quyền Admin hoặc Staff).Nếu parentId có giá trị, kiểm tra danh mục cha đó có tồn tại và chưa bị xóa hay không.Thêm mới bản ghi vào bảng CategoryNews.Trả dữ liệu vừa tạo về cho phía client.4.2 Update CategoryNewsEndpoint: PUT /category-news/:idMô tả: Cập nhật thông tin danh mục hoặc thay đổi vị trí trong cây danh mục.Request PayloadJSON{
  "title": "Hoa Cưới Thiết Kế Cao Cấp",
  "parentId": 2
}
Luồng xử lý dữ liệuPhía Frontend:Gửi id danh mục cần sửa trên URL Path.Gửi title và parentId mới trong Request Body.Render lại cấu trúc cây danh mục (Tree View) sau khi cập nhật thành công.Phía Backend:Kiểm tra danh mục cần sửa có tồn tại trong Database hay không.Kiểm tra vòng lặp (Loop Check): parentId mới không được trùng với id hiện tại và không được nằm trong danh sách các con/cháu ruột của chính nó.Tiến hành cập nhật bản ghi CategoryNews.4.3 Delete CategoryNewsEndpoint: DELETE /category-news/:idMô tả: Gỡ bỏ danh mục tin tức khỏi hệ thống hiển thị.Response Body (200 OK)JSON{
  "message": "Xóa danh mục thành công."
}
Luồng xử lý dữ liệuBên xử lýNội dung xử lýFrontendGửi request chứa ID danh mục. Nếu danh mục đang chứa bài viết (kiểm tra từ trước hoặc nhận cảnh báo từ BE) hoặc chứa danh mục con, cần hiển thị popup cảnh báo người dùng trước khi thực hiện xóa.BackendTìm danh mục theo id. Nếu còn phần tử con (parentId trỏ về id này), chọn một trong hai hướng xử lý nghiệp vụ: từ chối xóa, hoặc chuyển toàn bộ danh mục con về null (hoặc về danh mục cha cao hơn). Sau đó thực hiện Soft Delete danh mục hiện tại và trả kết quả.5. Company Address API5.1 Create CompanyAddressEndpoint: POST /company-addressesMô tả: Thêm mới địa chỉ văn phòng, cửa hàng hoặc chi nhánh của công ty.Request PayloadJSON{
  "addressLine": "123 Nguyễn Huệ",
  "ward": "Phường Bến Nghé",
  "district": "Quận 1",
  "city": "Thành phố Hồ Chí Minh",
  "country": "Việt Nam",
  "latitude": 10.7756,
  "longitude": 106.7019,
  "isPrimary": true
}
Validation Rules & Business LogicFieldStateData TypeValidation Rules & Business LogicaddressLineRequiredStringTừ 5 đến 255 ký tự. Không được để trống.wardOptionalStringTối đa 100 ký tự. Có thể null.districtOptionalStringTối đa 100 ký tự. Có thể null.cityRequiredStringTừ 2 đến 100 ký tự.countryRequiredStringTừ 2 đến 100 ký tự.latitudeOptionalDecimalVĩ độ hợp lệ trong phạm vi từ -90 đến 90.longitudeOptionalDecimalKinh độ hợp lệ trong phạm vi từ -180 đến 180.isPrimaryRequiredBooleantrue nếu là trụ sở chính; false nếu là chi nhánh phụ.Response Body (201 Created)JSON{
  "message": "Tạo địa chỉ công ty thành công.",
  "data": {
    "id": "e3b0c442-98fc-4c14-9c2e-4b4f514b2d32",
    "addressLine": "123 Nguyễn Huệ",
    "ward": "Phường Bến Nghé",
    "district": "Quận 1",
    "city": "Thành phố Hồ Chí Minh",
    "country": "Việt Nam",
    "latitude": 10.7756,
    "longitude": 106.7019,
    "isPrimary": true,
    "createdAt": "2026-06-11T08:58:55Z",
    "updatedAt": null
  }
}
Luồng xử lý dữ liệuPhía Frontend:Gửi request chứa dữ liệu địa chỉ kèm token xác thực của Admin.Hiển thị thông báo tạo thành công và làm mới danh sách địa chỉ trên màn hình quản trị.Phía Backend:Xác thực quyền Admin.Kiểm tra tính hợp lệ của tọa độ địa lý (nếu có truyền).Khởi chạy Database Transaction.Ràng buộc nghiệp vụ: Nếu isPrimary = true, tiến hành cập nhật toàn bộ tất cả địa chỉ cũ trong DB thành IsPrimary = false (vì hệ thống chỉ có duy nhất 1 trụ sở chính).Sinh Guid mới cho bản ghi, gán CreatedAt = NOW().Commit Transaction và trả dữ liệu thành công.5.2 Update CompanyAddressEndpoint: PUT /company-addresses/:idMô tả: Sửa thông tin chi tiết địa chỉ hoặc thay đổi trạng thái trụ sở chính.Request PayloadJSON{
  "addressLine": "456 Lê Lợi",
  "ward": "Phường Bến Thành",
  "district": "Quận 1",
  "city": "Thành phố Hồ Chí Minh",
  "country": "Việt Nam",
  "latitude": 10.7712,
  "longitude": 106.6983,
  "isPrimary": false
}
Luồng xử lý dữ liệuPhía Frontend:Truyền id dạng Guid trên URL Path.Gửi dữ liệu chỉnh sửa cấu trúc trong Request Body.Cập nhật lại trạng thái hiển thị mới trên giao diện.Phía Backend:Kiểm tra địa chỉ mục tiêu tồn tại và chưa bị xóa mềm (IsDeleted == false).Khởi chạy Database Transaction.Kiểm tra ràng buộc trụ sở chính (Nếu chuyển từ false thành true, phải hạ toàn bộ các địa chỉ khác xuống false).Cập nhật dữ liệu mới và gán UpdatedAt = NOW().Commit Transaction.5.3 Delete CompanyAddressEndpoint: DELETE /company-addresses/:idMô tả: Gỡ bỏ địa chỉ khỏi hệ thống hiển thị của công ty (Xóa mềm).Response Body (200 OK)JSON{
  "message": "Xóa địa chỉ công ty thành công."
}
Luồng xử lý dữ liệuBên xử lýNội dung xử lýFrontendHiển thị một popup xác nhận rõ ràng với người dùng: "Hành động này sẽ gỡ bỏ địa chỉ khỏi hệ thống hiển thị. Bạn có chắc chắn muốn tiếp tục?"BackendYêu cầu quyền Admin. Kiểm tra xem địa chỉ mục tiêu có phải là trụ sở chính duy nhất đang kích hoạt hay không (isPrimary == true). Nếu đúng, từ chối xóa và trả về mã lỗi 400 Bad Request. Nếu hợp lệ, thực hiện Soft Delete bằng cách cập nhật IsDeleted = true và UpdatedAt = NOW().6. Mã lỗi hệ thống gợi ý (Error Codes Reference)Dưới đây là danh sách các HTTP Status Code và kịch bản áp dụng chung cho toàn hệ thống API:HTTP StatusÝ nghĩaTrường hợp sử dụng cụ thể200 OKSuccessCập nhật dữ liệu hoặc xóa (soft delete) thành công.201 CreatedCreatedTạo mới thành công bài viết, danh mục hoặc địa chỉ.400 Bad RequestBad RequestDữ liệu không hợp lệ, vi phạm ràng buộc trụ sở chính (xóa trụ sở chính duy nhất), mảng categoryIds rỗng hoặc parentId gây vòng lặp cây.401 UnauthorizedUnauthorizedThiếu token xác thực hoặc token đã hết hạn/không hợp lệ.403 ForbiddenForbiddenToken hợp lệ nhưng người dùng không đủ quyền hạn để thực hiện hành động (ví dụ: Staff đòi xóa địa chỉ công ty).404 Not FoundNot FoundKhông tìm thấy bài viết, danh mục hoặc địa chỉ cần thao tác dựa theo ID truyền lên.500 Internal Server ErrorServer ErrorLỗi hệ thống ngoài dự kiến, lỗi kết nối Database hoặc lỗi rollback transaction.