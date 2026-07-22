payload
```CSharp
{
    "event": "orderAdd",
    "businessId": 10000,
    "data": 
    {
  "info": {
    "id": "(int) ID đơn hàng",
    "depotId": "(int) ID kho hàng",
    "pickupDepotId": "(int) ID kho nhặt hàng",
    "type": "(int) Loại đơn",
    "status": "(int) Trạng thái",
    "createdBy": {
      "id": "(int) ID người tạo đơn",
      "name": "(string) Tên người tạo đơn"
    },
    "sale": {
      "id": "(int) ID nhân viên bán hàng",
      "name": "(string) Tên nhân viên bán hàng"
    },
    "technicalStaff": {
      "id": "(int) ID nhân viên kỹ thuật",
      "name": "(string) Tên nhân viên kỹ thuật"
    },
    "customerCare": {
      "id": "(int) ID nhân viên chăm sóc",
      "name": "(string) Tên nhân viên chăm sóc"
    },
    "createdAt": "(int) Thời gian tạo",
    "updatedAt": "(int) Thời gian cập nhật",
    "confirmedAt": "(int) Thời gian xác nhận",
    "packedAt": "(int) Thời gian đóng gói",
    "description": "(string) Ghi chú khách hàng",
    "privateDescription": "(string) Ghi chú chăm sóc khách hàng",
    "trackingUrl": "(string) Link xem lịch trình đơn hàng"
  },
  "channel": {
    "appOrderId": "(string) ID website tích hợp",
    "appShopId": "(string) ID shop trên website tích hợp",
    "appShopName": "(string) Tên shop trên website tích hợp",
    "saleChannel": "(int) Kênh bán",
    "trafficSource": {
      "id": "(int) ID nguồn đơn hàng",
      "name": "(string) Tên nguồn đơn hàng"
    },
    "facebook": {
      "pageId": "(string) ID page",
      "conversationId": "(string) ID cuộc trò chuyện",
      "adId": "(string) ID Ads",
      "postId": "(string) ID bài viết"
    },
    "utmCampaign": "(string) Chiến dịch",
    "domain": "(string) Tên miền"
  },
  "shippingAddress": {
    "id": "(string) ID khách hàng",
    "name": "(string) Tên khách hàng",
    "mobile": "(string) Số điện thoại",
    "email": "(string) Email",
    "address": "(string) Địa chỉ",
    "cityId": "(int) ID Tỉnh/Thành phố",
    "districtId": "(int) ID Quận/Huyện",
    "wardId": "(int) ID Phường/xã",
    "locationVersion": "(string) v1: Địa chỉ 3 cấp, v2: Địa chỉ 2 cấp"
  },
  "products": [
    {
      "id": "(int) ID sản phẩm",
      "typeId": "(int) Loại sản phẩm",
      "code": "(string) Mã sản phẩm",
      "name": "(string) Tên sản phẩm",
      "avatar": "(string) Ảnh đại diện",
      "weight": "(double) Trọng lượng",
      "quantity": "(double) Số lượng",
      "price": "(double) Giá bán",
      "originalPrice": "(double) Giá bán gốc",
      "discount": "(double) Số tiền chiết khấu",
      "available": "(double) Số có thể bán",
      "avgCost": "(double) Giá vốn",
      "vat": {
        "value": "(int) Giá trị VAT",
        "amount": "(double) Số tiền VAT"
      },
      "description": "(string) Ghi chú sản phẩm",
      "imei": {
          "id": "(id) ID IMEI",
          "code": "(string) Số IMEI"
      },
      "batch": {
        "id": "(int) ID lô hàng",
        "name": "(string) Tên lô hàng",
        "manufactureDate": "(string) Ngày sản xuất",
        "expiredDate": "(string) Ngày hết hạn"
      },
      "combos": [
        {
          "id": "(int) ID sản phẩm",
          "name": "(string) Tên sản phẩm",
          "quantity": "(int) Số lượng"
        }
      ],
      "unit": {
        "id": "(int) ID đơn vị tính",
        "name": "(string) Tên đơn vị tính",
        "quantity": "(int) Số lượng"
      },
      "gifts": [
        {
          "id": "(int) ID sản phẩm",
          "name": "(string) Tên sản phẩm",
          "quantity": "(int) Số lượng"
        }
      ]
    }
  ],
  "carrier": {
    "id": "(int) ID hãng vận chuyển",
    "name": "(string) Tên hãng vận chuyển",
    "orderPackageSize": {
      "height": "(int) Chiều cao gói hàng",
      "merge": "(int) 1: Gộp đơn hàng, 0: Không gộp đơn",
      "length": "(int) Chiều dài gói hàng",
      "weight": "(int) Tổng trọng lượng gói hàng",
      "width": "(int) Chiều rộng gói hàng"
    },
    "service": {
      "id": "(int) ID dịch vụ vận chuyển",
      "name": "(string) Tên dịch vụ vận chuyển",
      "code": "(string) Mã dịch vụ vận chuyển"
    },
    "carrierCode": "(string) Mã vận đơn hãng vận chuyển",
    "isPartDelivery": "(int) 1: Giao hàng một phần",
    "sendCarrier": {
      "type": "(int) Loại kết nối",
      "date": "(string) Ngày gửi vận chuyển"
    },
    "deliveryDate": "(string) Ngày giao hàng thành công",
    "weight": "(int) Trọng lượng",
    "shipFee": "(int) Phí vận chuyển",
    "codFee": "(int) Phí thu tiền hộ",
    "declaredFee": "(int) Phí khai giá",
    "returnFee": "(int) Phí hoàn hàng",
    "customerShipFee": "(int) Phí ship báo khách"
  },
  "payment": {
    "couponCode": "(string) Mã giảm giá",
    "credit": {
      "amount": "(int) Tiền quẹt thẻ",
      "accountId": "(int) ID tài khoản kế toán",
      "code": "(string) Mã quẹt thẻ"
    },
    "deposit": {
      "amount": "(int) Tiền đặt cọc",
      "accountId": "(int) ID tài khoản kế toán"
    },
    "discount": {
      "type": "(string) Loại chiết khấu",
      "amount": "(int) Số tiền chiết khấu"
    },
    "transfer": {
      "amount": "(int) Tiền chuyển khoản",
      "accountId": "(int) ID tài khoản kế toán"
    },
    "usedPoints": {
      "points": "(int) Số điểm đã tiêu",
      "amount": "(int) Số tiền đã tiêu"
    },
    "vat": {
      "type": "(string) Loại VAT. cash: Tiền mặt, percent: Phần trăm",
      "value": "(int) Giá trị VAT. VD: 10, nếu type = percent thì hiểu là 10%",
      "amount": "(int) Số tiền VAT. VD: 10000 => Số tiền VAT, tình huống type = cash thì value sẽ bằng amount",
      "taxCode": "(string) Số hóa đơn VAT",
      "taxDate": "(string) Ngày xuất hóa đơn (định dạng năm-tháng-ngày)"
    }
  },
  "originalOrder": { 
    "id": "(int) ID đơn gốc",
    "channel": {
        "appOrderId": "(string) ID đơn sàn / ID đơn website tích hợp",
        "appShopId": "(string) ID Shop trên sàn (nếu có)",
        "saleChannel" : "(int) Kênh bán"
    }
  }
}
}
```
