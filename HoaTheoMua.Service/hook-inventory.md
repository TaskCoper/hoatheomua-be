```CSharp
{
  "id": "(int) ID sản phẩm",
  "parentId": "(int) ID sản phẩm cha (-1: Sản phẩm độc lập, -2: Sản phẩm cha)",
  "code": "(string) Mã sản phẩm",
  "barcode": "(string) Mã vạch",
  "name": "(string) Tên sản phẩm",
  "otherName": "(string) Tên khác",
  "status": "(string) Trạng thái sản phẩm",
  "category": {
    "id": "(int) ID danh mục",
    "code": "(string) Mã danh mục",
    "name": "(string) Tên danh mục"
  },
  "internalCategory": {
    "id": "(int) ID danh mục nội bộ",
    "code": "(string) Mã danh mục nội bộ",
    "name": "(string) Tên danh mục nội bộ"
  },
  "prices": {
    "retail": "(double) Giá bán lẻ",
    "import": "(double) Giá nhập",
    "old": "(double) Giá bán cũ",
    "wholesale": "(double) Giá bán sỉ",
    "avgCost": "(double) Giá vốn"
  },
  "vat": "(int) VAT",
  "images": {
    "avatar": "(string) Đường dẫn tới file ảnh",
    "others": [
      "(string) Đường dẫn tới file ảnh"
    ]
  },
  "warranty": {
    "month": "(int) Số tháng bảo hành",
    "phone": "(string) Số điện thoại bảo hành",
    "address": "(string) Địa chỉ bảo hảnh"
  },
  "brand": {
    "id": "(int) ID thương hiệu",
    "name": "(string) Tên thương hiệu"
  },
  "type": {
    "id": "(int) ID loại sản phẩm",
    "name": "(string) Tên loại sản phẩm"
  },
  "shipping": {
    "length": "(int) Chiều dài",
    "width": "(int) Chiều rộng",
    "height": "(int) Chiều cao",
    "weight": "(int) Khối lượng"
  },
  "countryName": "(string) Tên quốc gia",
  "units": {
    "name": "(string) Đơn vị tính",
    "list": [
      {
        "id": "(int) ID đơn vị tính",
        "name": "(string) Tên đơn vị tính",
        "quantity": "(double) Số lượng",
        "price": {
          "retail": "(double) Giá bán lẻ",
          "import": "(double) Giá nhập",
          "wholesale": "(double) Giá bán sỉ"
        }
      }
    ]
  },
  "combos": [
    {
      "id": "(int) ID sản phẩm",
      "code": "(string) Mã sản phẩm",
      "name": "(string) Tên sản phẩm",
      "quantity": "(double) Số lượng"
    }
  ],
  "inventory": {
    "remain": "(double) Số lượng tồn kho tất cả các kho",
    "shipping": "(double) Số lượng đang giao hàng tất cả các kho",
    "damaged": "(double) Số lượng lỗi tất cả các kho",
    "holding": "(double) Số lượng tạm giữ tất cả các kho",
    "transfering": "(double) Số lượng đang chuyển kho tất cả các kho",
    "warranty": "(double) Số lượng bảo hành tất cả các kho",
    "warrantyHolding": "(double) Số lượng tạm giữ bảo hành tất cả các kho",
    "holdingNew": "(double) Số lượng tạm giữ (loại xuất S) tất cả các kho",
    "shippingExport": "(double) Số lượng đang chuyển (loại xuất S) tất cả các kho",
    "available": "(double) Số lượng có thể bán tất cả các kho",
    "depots": [
      {
        "id": "(int) ID kho hàng",
        "remain": "(double) Số lượng tồn kho",
        "shipping": "(double) Số lượng đang giao hàng",
        "damaged": "(double) Số lượng lỗi",
        "holding": "(double) Số lượng tạm giữ",
        "transfering": "(double) Số lượng chuyển kho",
        "warranty": "(double) Số lượng bảo hành",
        "warrantyHolding": "(double) Số lượng tạm giữ bảo hành",
        "holdingNew": "(double) Số lượng tạm giữ (loại xuất S)",
        "shippingExport": "(double) Số lượng đang chuyển (loại xuất S)",
        "available": "(double) Số lượng có thể bán"
      }
    ]
  },
  "attributes": [
    {
      "id": "(int) ID thuộc tính",
      "name": "(string) Tên thuộc tính",
      "value": "(string) Giá trị"
    }
  ],
  "updatedAt": "(int) Thời gian cập nhật, định dạng timestamp",
  "createdAt": "(int) Thời gian tạo, định dạng timestamp",
  "description": "(string) Mô tả sản phẩm",
  "content": "(string) Nội dung sản phẩm",
  "suppliers": {
    "id": "(int) ID nhà cung cấp",
    "name": "(string) Tên nhà cung cấp",
    "mobile": "(string) Số điện thoại"
  },
  "videos": [
    {
      "title": "(string) Tiêu đề",
      "src": "(string) Link đường dẫn tới video"
    }
  ],
  "branchPrices": [
    {
      "branch": {
        "id": "(int) ID chi nhánh",
        "name": "(string) Tên chi nhánh"
      },
      "price": "(int) Giá bán lẻ",
      "wholesalePrice": "(int) Giá bán sỉ"
    }
  ],
  "childs": "(array) Mảng thông tin sản phẩm con"
}
```