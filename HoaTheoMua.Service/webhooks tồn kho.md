payload
```CSharp
{
  "event": "inventoryChange",
  "businessId": "(int) ID doanh nghiệp",
  "data": [
    {
      "id": "(int) ID sản phẩm 1",
      "typeId": "(int) Loại sản phẩm",
      "code": "(string) Mã sản phẩm",
      "remain": "(double) Tổng số lượng tồn các kho",
      "shipping": "(double) Tổng số lượng đang giao hàng các kho",
      "damaged": "(double) Tổng số lượng hàng lỗi các kho",
      "holding": "(double) Tổng số lượng tạm giữ các kho",
      "transfering": "(double) Tổng số lượng đang chuyển kho tất cả các kho",
      "available": "(double) Tổng số lượng có thể bán các kho, dùng để chặn việc đặt các sản phẩm hết hàng",
      "warranty": {
        "remain": "(double) Tổng số lượng bảo hành các kho",
        "holding": "(double) Tổng số lượng bảo hành tạm giữ các kho"
      },
      "depots": [
        {
          "id": "(int) ID kho hàng 1",
          "remain": "(double) Số lượng tồn kho",
          "shipping": "(double) Số lượng đang giao hàng",
          "damaged": "(double) Số lượng hàng lỗi",
          "holding": "(double) Số lượng tạm giữ",
          "transfering": "(double) Số lượng đang chuyển kho",
          "available": "(double) Số lượng có thể bán",
          "warranty": {
            "remain": "(double) Số lượng bảo hành",
            "holding": "(double) Số lượng bảo hành tạm giữ"
          }
        }
      ]
    }
  ]
}
```