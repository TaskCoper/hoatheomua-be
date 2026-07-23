# Webhook order payloads

## Endpoint

- `POST /api/v1/nhanh-vn/webhook`
- đổi `businessId` theo config của bạn
- `orderUpdate` dùng lại đúng `info.id` của `orderAdd`

---

## 1) orderAdd - không xuất hoá đơn

```json
{
  "event": "orderAdd",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001001,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 54,
      "createdAt": 1753350000,
      "updatedAt": 1753350000,
      "confirmedAt": 1753350000,
      "packedAt": 1753350000,
      "description": "Khách dặn giao giờ hành chính",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
      "trackingUrl": "https://nhanh.vn/tracking/88001001"
    },
    "channel": {
      "appOrderId": "WEB-88001001",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A",
      "mobile": "0909000001",
      "email": "khachhang@gmail.com",
      "address": "123 Nguyễn Huệ, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 200000,
        "originalPrice": 200000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001001",
      "customerShipFee": 30000,
      "shipFee": 30000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 530000,
        "accountId": 65088
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 0,
        "taxCode": null,
        "taxDate": null
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 500000`
- `ShippingFee = 30000`
- `Vat = 0`
- `TotalFee = 530000`
- `IsExportInvoice = false`

---

## 2) orderAdd - hoá đơn doanh nghiệp

```json
{
  "event": "orderAdd",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001002,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 54,
      "createdAt": 1753350000,
      "updatedAt": 1753350000,
      "confirmedAt": 1753350000,
      "packedAt": 1753350000,
      "description": "Khách cần xuất hoá đơn công ty",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng\n-------------------------\nMST:0312345678\nEmail:hoadon@abc.com\nSĐT:0909123456",
      "trackingUrl": "https://nhanh.vn/tracking/88001002"
    },
    "channel": {
      "appOrderId": "WEB-88001002",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook-business",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A",
      "mobile": "0909000001",
      "email": "khachhang@gmail.com",
      "address": "123 Nguyễn Huệ, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 200000,
        "originalPrice": 200000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001002",
      "customerShipFee": 30000,
      "shipFee": 30000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 570000,
        "accountId": 65087
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 40000,
        "taxCode": "0312345678",
        "taxDate": "2026-07-24"
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 500000`
- `ShippingFee = 30000`
- `Vat = 40000`
- `TotalFee = 570000`
- `TaxCode = 0312345678`
- `ReceiptEmail = hoadon@abc.com`
- `ReceiptPhoneNumber = 0909123456`
- `IsExportInvoice = true`
- `InvoiceType = Business`

---

## 3) orderUpdate - hoá đơn doanh nghiệp

> chạy sau payload `orderAdd - hoá đơn doanh nghiệp`
> giữ nguyên `info.id = 88001002`

```json
{
  "event": "orderUpdate",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001002,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 55,
      "createdAt": 1753350000,
      "updatedAt": 1753353600,
      "confirmedAt": 1753353600,
      "packedAt": 1753353600,
      "description": "Khách đổi sang giao buổi chiều",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng\n-------------------------\nMST:0312345678\nEmail:ketoan@abc.com\nSĐT:0909888777",
      "trackingUrl": "https://nhanh.vn/tracking/88001002-update"
    },
    "channel": {
      "appOrderId": "WEB-88001002",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook-update",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A update",
      "mobile": "0909000002",
      "email": "khachhang-update@gmail.com",
      "address": "456 Lê Lợi, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 250000,
        "originalPrice": 250000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001002-UPDATED",
      "customerShipFee": 35000,
      "shipFee": 35000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 629000,
        "accountId": 65087
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 44000,
        "taxCode": "0312345678",
        "taxDate": "2026-07-24"
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 550000`
- `ShippingFee = 35000`
- `Vat = 44000`
- `TotalFee = 629000`
- update được `TrackNumber`, `NhanhTrackingUrl`, `Remarks`, `FullName`, `Phone`, `Address`
- `ReceiptEmail = ketoan@abc.com`
- `ReceiptPhoneNumber = 0909888777`

---

## 4) orderAdd - hoá đơn cá nhân

```json
{
  "event": "orderAdd",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001003,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 54,
      "createdAt": 1753350000,
      "updatedAt": 1753350000,
      "confirmedAt": 1753350000,
      "packedAt": 1753350000,
      "description": "Khách yêu cầu xuất hoá đơn cá nhân",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng\n-------------------------\nTên:Nguyễn Văn A\nCCCD:079123456789\nĐC:123 Nguyễn Huệ, Quận 1\nEmail:canhan@gmail.com\nSĐT:0909123456",
      "trackingUrl": "https://nhanh.vn/tracking/88001003"
    },
    "channel": {
      "appOrderId": "WEB-88001003",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook-personal",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A",
      "mobile": "0909000001",
      "email": "khachhang@gmail.com",
      "address": "123 Nguyễn Huệ, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 200000,
        "originalPrice": 200000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001003",
      "customerShipFee": 30000,
      "shipFee": 30000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 530000,
        "accountId": 65088
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 0,
        "taxCode": null,
        "taxDate": null
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 500000`
- `ShippingFee = 30000`
- `Vat = 0`
- `TotalFee = 530000`
- `IsExportInvoice = true`
- `InvoiceType = Personal`
- `ReceiptFullName = Nguyễn Văn A`
- `IdentityNumber = 079123456789`
- `ReceiptAddress = 123 Nguyễn Huệ, Quận 1`
- `ReceiptEmail = canhan@gmail.com`
- `ReceiptPhoneNumber = 0909123456`

---

## 5) orderUpdate - hoá đơn doanh nghiệp

> chạy sau payload `orderAdd - hoá đơn doanh nghiệp`
> giữ nguyên `info.id = 88001002`

```json
{
  "event": "orderUpdate",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001002,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 55,
      "createdAt": 1753350000,
      "updatedAt": 1753353600,
      "confirmedAt": 1753353600,
      "packedAt": 1753353600,
      "description": "Khách đổi sang giao buổi chiều",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng\n-------------------------\nMST:0312345678\nEmail:ketoan@abc.com\nSĐT:0909888777",
      "trackingUrl": "https://nhanh.vn/tracking/88001002-update"
    },
    "channel": {
      "appOrderId": "WEB-88001002",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook-update",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A update",
      "mobile": "0909000002",
      "email": "khachhang-update@gmail.com",
      "address": "456 Lê Lợi, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 250000,
        "originalPrice": 250000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001002-UPDATED",
      "customerShipFee": 35000,
      "shipFee": 35000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 629000,
        "accountId": 65087
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 44000,
        "taxCode": "0312345678",
        "taxDate": "2026-07-24"
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 550000`
- `ShippingFee = 35000`
- `Vat = 44000`
- `TotalFee = 629000`
- update được `TrackNumber`, `NhanhTrackingUrl`, `Remarks`, `FullName`, `Phone`, `Address`
- `ReceiptEmail = ketoan@abc.com`
- `ReceiptPhoneNumber = 0909888777`

---

## 6) orderUpdate - hoá đơn cá nhân

> test parse nhánh cá nhân

```json
{
  "event": "orderUpdate",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88001003,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 56,
      "createdAt": 1753350000,
      "updatedAt": 1753357200,
      "confirmedAt": 1753357200,
      "packedAt": 1753357200,
      "description": "Khách đổi sang xuất hoá đơn cá nhân",
      "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng\n-------------------------\nTên:Nguyễn Văn A\nCCCD:079123456789\nĐC:123 Nguyễn Huệ, Quận 1\nEmail:canhan@gmail.com\nSĐT:0909123456",
      "trackingUrl": "https://nhanh.vn/tracking/88001003-personal"
    },
    "channel": {
      "appOrderId": "WEB-88001003",
      "appShopId": "SHOP-01",
      "appShopName": "Hoa Theo Mua",
      "saleChannel": 1,
      "trafficSource": {
        "id": 996,
        "name": "WEBSITE"
      },
      "facebook": {
        "pageId": "",
        "conversationId": "",
        "adId": "",
        "postId": ""
      },
      "utmCampaign": "test-webhook-personal",
      "domain": "hoatheomua.vn"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn A update",
      "mobile": "0909000002",
      "email": "khachhang-update@gmail.com",
      "address": "456 Lê Lợi, Quận 1",
      "cityId": 79,
      "districtId": 760,
      "wardId": 26734,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 101001,
        "typeId": 1,
        "code": "SP001",
        "name": "Hoa hồng đỏ",
        "avatar": "",
        "weight": 500,
        "quantity": 2,
        "price": 150000,
        "originalPrice": 150000,
        "discount": 0,
        "available": 10,
        "avgCost": 100000,
        "description": ""
      },
      {
        "id": 101002,
        "typeId": 1,
        "code": "SP002",
        "name": "Hoa tulip trắng",
        "avatar": "",
        "weight": 300,
        "quantity": 1,
        "price": 250000,
        "originalPrice": 250000,
        "discount": 0,
        "available": 10,
        "avgCost": 140000,
        "description": ""
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Ahamove",
      "carrierCode": "AHM88001003-PERSONAL",
      "customerShipFee": 35000,
      "shipFee": 35000
    },
    "payment": {
      "couponCode": "",
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 585000,
        "accountId": 65088
      },
      "vat": {
        "type": "percent",
        "value": 8,
        "amount": 0,
        "taxCode": null,
        "taxDate": null
      }
    },
    "originalOrder": null
  }
}
```

Kỳ vọng:
- `ProductFee = 550000`
- `ShippingFee = 35000`
- `Vat = 0`
- `TotalFee = 585000`
- `IsExportInvoice = true`
- `InvoiceType = Personal`
- `ReceiptFullName = Nguyễn Văn A`
- `IdentityNumber = 079123456789`
- `ReceiptAddress = 123 Nguyễn Huệ, Quận 1`
- `ReceiptEmail = canhan@gmail.com`
- `ReceiptPhoneNumber = 0909123456`
