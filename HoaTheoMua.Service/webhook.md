![[Pasted image 20260717144728.png]]

Có một sự kiện nào đó, webhook nó sẽ gọi về một cái url nhất định
response trả về có : event: (string) Tên sự kiện. -> Mình sẽ bắt cái event này để switch case, bởi vì mỗi cái response trả về có data khác nhau.

Hùng sẽ làm phần: **inventoryChange**: Thay đổi tồn kho của sản phẩm

Ta đặt ra một câu hỏi lớn trong đầu: response mà webhook trả ra cho ta, ta sẽ làm những gì với nó ?
	Ví dụ: webhook của sepay call về hàm của chúng ta, nhằm cập nhật trạng thái đơn hàng sản phẩm. 
	Vậy cái response của webhook bên nhanhvn này ta sẽ cần làm những gì?

Á à, theo Hùng nghĩ vấn đề core chỉ là để tránh rate limit thôi, thay vì phải call api của nhanhvn để kiểm tra sản phẩm tồn kho, thì bây giờ, chỉ là thay đổi chiều hướng gọi, mình gọi nó, bây giờ nói gọi ngược lại mình, lấy dữ liệu đó để update cho vào bảng product của mình là được, sau này query là query từ bảng product query ra.



- Cách làm, nhanh vn sẽ gọi chung một cái url, và response của nó sẽ ở chung một dạng:
Kiểu như này:
```HTML
"event": "orderAdd", "businessId": 64692, "data": {}
```
mình sẽ dựa vào cái envent để bắt response , và lấy ra các giá trị và update vào bảng prodcut là file excel,



```HTML
-   
    Khi tồn kho trên Nhanh.vn có thay đổi, Nhanh.vn sẽ bắn webhooks về cho hệ thống của bạn.
    
    - Khi các hóa đơn, đơn hàng phát sinh xuất nhập kho gần nhau, hoặc khi app của bạn lỗi xử lý webhooks: Nhanh bắn webhooks có thể gom nhiều sản phẩm lại chung 1 request webhooks.
        
    
- Để nhận được webhooks tồn kho, app phải có quyền [Danh sách sản phẩm](https://apidocs.nhanh.vn/v3/product/list)
    
- Đối với tồn sản phẩm combo, sẽ trả về cả sản phẩm combo và các sản phẩm trong combo.
    

Copy


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


- Đây là một trong các response về của nhanh webhook về inventoryChange

```CSharp
[nhanh-webhook] {
  event: 'inventoryChange',
  businessId: 126981,
  data: [
    {
      id: 4933641,
      appProductId: '',
      typeId: 9,
      code: 'HB_XL700_2026',
      remain: 5,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 5,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933640,
      appProductId: '',
      typeId: 9,
      code: 'HB_TBK_2026',
      remain: 18,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 18,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933639,
      appProductId: '',
      typeId: 9,
      code: 'HB_H250N_2026',
      remain: 13,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 13,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933638,
      appProductId: '',
      typeId: 9,
      code: 'HB_H500N_2026',
      remain: 8,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 8,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933637,
      appProductId: '',
      typeId: 9,
      code: 'HB_H700N_2026',
      remain: 3,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 3,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933636,
      appProductId: '',
      typeId: 9,
      code: 'HB_CV500N_2026',
      remain: 8,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 8,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933635,
      appProductId: '',
      typeId: 9,
      code: 'HB_CV700K_J_2026',
      remain: 5,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 5,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933634,
      appProductId: '',
      typeId: 9,
      code: 'HB_KD S_2026',
      remain: 29,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 29,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933633,
      appProductId: '',
      typeId: 9,
      code: 'HB_KD L_2026',
      remain: 14,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 14,
      warranty: [Object],
      depots: [Array]
    },
    {
      id: 4933632,
      appProductId: '',
      typeId: 9,
      code: 'HB_TX S_2026',
      remain: 25,
      shipping: 0,
      damaged: 0,
      holding: 0,
      transfering: 0,
      available: 25,
      warranty: [Object],
      depots: [Array]
    }
  ]
}
```