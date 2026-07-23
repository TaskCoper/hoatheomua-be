# HandleWebhook - Payload mẫu để test trên Swagger

## Endpoint

- **Method:** `POST`
- **URL:** `/api/v1/nhanh-vn/webhook`
- **Content-Type:** `application/json`
- **businessId cố định:** `126981`

## Ghi chú trước khi test

- Bộ payload này được dựng theo DTO hiện tại trong `HoaTheoMua.Service/NhanhVnService/Request.cs` và logic xử lý trong `HoaTheoMua.Service/NhanhVnService/Service.cs`.
- Có **7 event** đúng với `HandleWebhook(...)` hiện tại:
  1. `inventoryChange`
  2. `productAdd`
  3. `productUpdate`
  4. `productDelete`
  5. `orderAdd`
  6. `orderUpdate`
  7. `orderDelete`
- Nếu muốn test theo luồng hợp lý trên Swagger, nên chạy theo thứ tự:
  1. `productAdd`
  2. `inventoryChange`
  3. `productUpdate`
  4. `orderAdd`
  5. `orderUpdate`
  6. `orderDelete`
  7. `productDelete`
- `productUpdate` nên dùng lại `data.id` của `productAdd`.
- `inventoryChange` nên dùng lại `data[0].id` của `productAdd`.
- `orderUpdate` và `orderDelete` nên dùng lại `data.info.id` của `orderAdd`.
- Nếu test lại nhiều lần và gặp lỗi trùng dữ liệu, hãy đổi các ID mẫu sau:
  - **ProductId:** `99000123`
  - **OrderId:** `88000123`

---

## 1) inventoryChange

```json
{
  "event": "inventoryChange",
  "businessId": 126981,
  "data": [
    {
      "id": 99000123,
      "appProductId": "app-99000123",
      "typeId": 1,
      "code": "SKU-99000123",
      "remain": 20,
      "shipping": 2,
      "damaged": 1,
      "holding": 1,
      "transfering": 0,
      "available": 16,
      "warranty": {
        "remain": 1,
        "holding": 0
      },
      "depots": [
        {
          "id": 1,
          "remain": 20,
          "shipping": 2,
          "damaged": 1,
          "holding": 1,
          "transfering": 0,
          "available": 16,
          "warranty": {
            "remain": 1,
            "holding": 0
          }
        }
      ]
    }
  ]
}
```

---

## 2) productAdd

```json
{
  "event": "productAdd",
  "businessId": 126981,
  "data": {
    "id": 99000123,
    "parentId": 0,
    "code": "SKU-99000123",
    "barcode": "BAR-99000123",
    "name": "Hoa Hồng Đỏ Test Webhook",
    "otherName": "Rose Red Test",
    "status": "active",
    "category": {
      "id": 10,
      "code": "HOATUOI",
      "name": "Hoa Tươi"
    },
    "internalCategory": {
      "id": 20,
      "code": "HOAHONG",
      "name": "Hoa Hồng"
    },
    "prices": {
      "retail": 150000,
      "import": 90000,
      "old": 180000,
      "wholesale": 130000,
      "avgCost": 95000
    },
    "vat": 10,
    "images": {
      "avatar": "https://example.com/images/hoa-hong-do.jpg",
      "others": [
        "https://example.com/images/hoa-hong-do-1.jpg",
        "https://example.com/images/hoa-hong-do-2.jpg"
      ]
    },
    "warranty": {
      "month": 0,
      "phone": "0900000001",
      "address": "TP.HCM"
    },
    "brand": {
      "id": 1,
      "name": "HoaTheoMua"
    },
    "type": {
      "id": 1,
      "name": "simple"
    },
    "shipping": {
      "length": 40,
      "width": 20,
      "height": 15,
      "weight": 0.8
    },
    "countryName": "Vietnam",
    "units": {
      "name": "bó",
      "list": [
        {
          "id": 1,
          "name": "bó",
          "quantity": 1,
          "price": {
            "retail": 150000,
            "import": 90000,
            "wholesale": 130000
          }
        }
      ]
    },
    "combos": [],
    "inventory": {
      "remain": 20,
      "shipping": 2,
      "damaged": 0,
      "holding": 1,
      "transfering": 0,
      "warranty": 0,
      "warrantyHolding": 0,
      "holdingNew": 0,
      "shippingExport": 0,
      "available": 17,
      "depots": [
        {
          "id": 1,
          "remain": 20,
          "shipping": 2,
          "damaged": 0,
          "holding": 1,
          "transfering": 0,
          "warranty": 0,
          "warrantyHolding": 0,
          "holdingNew": 0,
          "shippingExport": 0,
          "available": 17
        }
      ]
    },
    "attributes": [
      {
        "id": 1,
        "name": "Size",
        "value": "M"
      },
      {
        "id": 2,
        "name": "Color",
        "value": "Đỏ"
      }
    ],
    "updatedAt": 1785102000,
    "createdAt": 1785101000,
    "description": "Sản phẩm fake để test webhook productAdd",
    "content": "Nội dung mô tả fake để test trên Swagger",
    "suppliers": {
      "id": 5,
      "name": "Nhà vườn Đà Lạt",
      "mobile": "0900000002"
    },
    "videos": [
      {
        "title": "Video giới thiệu",
        "src": "https://example.com/videos/hoa-hong-do.mp4"
      }
    ],
    "branchPrices": [
      {
        "branch": {
          "id": 1,
          "name": "Chi nhánh Quận 1"
        },
        "price": 150000,
        "wholesalePrice": 130000
      }
    ],
    "childs": []
  }
}
```

---

## 3) productUpdate

> Dùng lại `id = 99000123` để update đúng sản phẩm đã add ở payload phía trên.

```json
{
  "event": "productUpdate",
  "businessId": 126981,
  "data": {
    "id": 99000123,
    "parentId": 0,
    "code": "SKU-99000123",
    "barcode": "BAR-99000123",
    "name": "Hoa Hồng Đỏ Test Webhook - Updated",
    "otherName": "Rose Red Test Updated",
    "status": "active",
    "category": {
      "id": 10,
      "code": "HOATUOI",
      "name": "Hoa Tươi"
    },
    "internalCategory": {
      "id": 20,
      "code": "HOAHONG",
      "name": "Hoa Hồng"
    },
    "prices": {
      "retail": 165000,
      "import": 92000,
      "old": 190000,
      "wholesale": 135000,
      "avgCost": 98000
    },
    "vat": 10,
    "images": {
      "avatar": "https://example.com/images/hoa-hong-do-updated.jpg",
      "others": [
        "https://example.com/images/hoa-hong-do-updated-1.jpg"
      ]
    },
    "warranty": {
      "month": 0,
      "phone": "0900000001",
      "address": "TP.HCM"
    },
    "brand": {
      "id": 1,
      "name": "HoaTheoMua"
    },
    "type": {
      "id": 1,
      "name": "simple"
    },
    "shipping": {
      "length": 42,
      "width": 22,
      "height": 16,
      "weight": 0.9
    },
    "countryName": "Vietnam",
    "units": {
      "name": "bó",
      "list": [
        {
          "id": 1,
          "name": "bó",
          "quantity": 1,
          "price": {
            "retail": 165000,
            "import": 92000,
            "wholesale": 135000
          }
        }
      ]
    },
    "combos": [],
    "inventory": {
      "remain": 25,
      "shipping": 1,
      "damaged": 0,
      "holding": 2,
      "transfering": 0,
      "warranty": 0,
      "warrantyHolding": 0,
      "holdingNew": 0,
      "shippingExport": 0,
      "available": 22,
      "depots": [
        {
          "id": 1,
          "remain": 25,
          "shipping": 1,
          "damaged": 0,
          "holding": 2,
          "transfering": 0,
          "warranty": 0,
          "warrantyHolding": 0,
          "holdingNew": 0,
          "shippingExport": 0,
          "available": 22
        }
      ]
    },
    "attributes": [
      {
        "id": 1,
        "name": "Size",
        "value": "L"
      },
      {
        "id": 2,
        "name": "Color",
        "value": "Đỏ đậm"
      }
    ],
    "updatedAt": 1785102600,
    "createdAt": 1785101000,
    "description": "Sản phẩm fake để test webhook productUpdate",
    "content": "Nội dung mô tả fake sau khi update",
    "suppliers": {
      "id": 5,
      "name": "Nhà vườn Đà Lạt",
      "mobile": "0900000002"
    },
    "videos": [
      {
        "title": "Video giới thiệu updated",
        "src": "https://example.com/videos/hoa-hong-do-updated.mp4"
      }
    ],
    "branchPrices": [
      {
        "branch": {
          "id": 1,
          "name": "Chi nhánh Quận 1"
        },
        "price": 165000,
        "wholesalePrice": 135000
      }
    ],
    "childs": []
  }
}
```

---

## 4) productDelete

> Dùng lại `99000123` để xoá đúng sản phẩm đã add.

```json
{
  "event": "productDelete",
  "businessId": 126981,
  "data": [99000123]
}
```

---

## 5) orderAdd

> Payload này đang tham chiếu đến sản phẩm `99000123`.

```json
{
  "event": "orderAdd",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88000123,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 1,
      "createdBy": {
        "id": 10,
        "name": "Admin Test"
      },
      "sale": {
        "id": 11,
        "name": "Sale Test"
      },
      "technicalStaff": {
        "id": 12,
        "name": "Technical Test"
      },
      "customerCare": {
        "id": 13,
        "name": "CSKH Test"
      },
      "createdAt": 1785103000,
      "updatedAt": 1785103000,
      "confirmedAt": 1785103060,
      "packedAt": 1785103120,
      "description": "Đơn hàng fake để test webhook orderAdd",
      "privateDescription": "Ghi chú nội bộ fake",
      "trackingUrl": "https://example.com/tracking/order-88000123"
    },
    "channel": {
      "appOrderId": "APP-88000123",
      "appShopId": "SHOP-HTM-01",
      "appShopName": "Hoa Theo Mùa Shop",
      "saleChannel": 1,
      "trafficSource": {
        "id": 100,
        "name": "Facebook"
      },
      "facebook": {
        "pageId": "page-001",
        "conversationId": "conv-001",
        "adId": "ad-001",
        "postId": "post-001"
      },
      "utmCampaign": "campaign-test",
      "domain": "hoatheomua.test"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn Test",
      "mobile": "0901234567",
      "email": "test@example.com",
      "address": "123 Đường Test, Quận 1",
      "cityId": 1,
      "districtId": 101,
      "wardId": 1001,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 99000123,
        "typeId": 1,
        "code": "SKU-99000123",
        "name": "Hoa Hồng Đỏ Test Webhook - Updated",
        "avatar": "https://example.com/images/hoa-hong-do-updated.jpg",
        "weight": 0.9,
        "quantity": 2,
        "price": 165000,
        "originalPrice": 190000,
        "discount": 10000,
        "available": 22,
        "avgCost": 98000,
        "vat": {
          "value": 10,
          "amount": 0
        },
        "description": "Dòng sản phẩm fake trong order",
        "imei": {
          "id": 1,
          "code": "IMEI-001"
        },
        "batch": {
          "id": 1,
          "name": "BATCH-001",
          "manufactureDate": "2026-07-01",
          "expiredDate": "2026-12-31"
        },
        "combos": [],
        "unit": {
          "id": 1,
          "name": "bó",
          "quantity": 1
        },
        "gifts": []
      }
    ],
    "carrier": {
      "id": 1,
      "name": "Ahamove",
      "orderPackageSize": {
        "height": 16,
        "merge": 0,
        "length": 42,
        "weight": 1,
        "width": 22
      },
      "service": {
        "id": 1,
        "name": "Siêu tốc",
        "code": "SUPER_FAST"
      },
      "carrierCode": "TRACK-88000123",
      "isPartDelivery": 0,
      "sendCarrier": {
        "type": 1,
        "date": "2026-07-23"
      },
      "deliveryDate": "2026-07-24",
      "weight": 1,
      "shipFee": 30000,
      "codFee": 0,
      "declaredFee": 0,
      "returnFee": 0,
      "customerShipFee": 30000
    },
    "payment": {
      "couponCode": "TEST10",
      "credit": {
        "amount": 0,
        "accountId": 1,
        "code": "CREDIT-01"
      },
      "deposit": {
        "amount": 0,
        "accountId": 1
      },
      "discount": {
        "type": "cash",
        "amount": 10000
      },
      "transfer": {
        "amount": 0,
        "accountId": 1
      },
      "usedPoints": {
        "points": 0,
        "amount": 0
      },
      "vat": {
        "type": "percent",
        "value": 10,
        "amount": 0,
        "taxCode": null,
        "taxDate": null
      }
    },
    "originalOrder": {
      "id": 500001,
      "channel": {
        "appOrderId": "ORIGINAL-ORDER-001",
        "appShopId": "SHOP-HTM-01",
        "saleChannel": 1
      }
    }
  }
}
```

---

## 6) orderUpdate

> Dùng lại `info.id = 88000123` để update đúng đơn đã add.

```json
{
  "event": "orderUpdate",
  "businessId": 126981,
  "data": {
    "info": {
      "id": 88000123,
      "depotId": 1,
      "pickupDepotId": 1,
      "type": 1,
      "status": 2,
      "createdBy": {
        "id": 10,
        "name": "Admin Test"
      },
      "sale": {
        "id": 11,
        "name": "Sale Test"
      },
      "technicalStaff": {
        "id": 12,
        "name": "Technical Test"
      },
      "customerCare": {
        "id": 13,
        "name": "CSKH Test"
      },
      "createdAt": 1785103000,
      "updatedAt": 1785103600,
      "confirmedAt": 1785103060,
      "packedAt": 1785103120,
      "description": "Đơn hàng fake để test webhook orderUpdate",
      "privateDescription": "Đã cập nhật thông tin đơn",
      "trackingUrl": "https://example.com/tracking/order-88000123-updated"
    },
    "channel": {
      "appOrderId": "APP-88000123",
      "appShopId": "SHOP-HTM-01",
      "appShopName": "Hoa Theo Mùa Shop",
      "saleChannel": 1,
      "trafficSource": {
        "id": 101,
        "name": "Website"
      },
      "facebook": {
        "pageId": "page-001",
        "conversationId": "conv-001",
        "adId": "ad-001",
        "postId": "post-001"
      },
      "utmCampaign": "campaign-update",
      "domain": "hoatheomua.test"
    },
    "shippingAddress": {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "Nguyễn Văn Test Updated",
      "mobile": "0901234568",
      "email": "test.updated@example.com",
      "address": "456 Đường Test Mới, Quận 3",
      "cityId": 1,
      "districtId": 102,
      "wardId": 1002,
      "locationVersion": "v2"
    },
    "products": [
      {
        "id": 99000123,
        "typeId": 1,
        "code": "SKU-99000123",
        "name": "Hoa Hồng Đỏ Test Webhook - Updated",
        "avatar": "https://example.com/images/hoa-hong-do-updated.jpg",
        "weight": 0.9,
        "quantity": 3,
        "price": 170000,
        "originalPrice": 190000,
        "discount": 5000,
        "available": 22,
        "avgCost": 98000,
        "vat": {
          "value": 10,
          "amount": 0
        },
        "description": "Dòng sản phẩm fake sau khi update",
        "imei": {
          "id": 1,
          "code": "IMEI-001"
        },
        "batch": {
          "id": 1,
          "name": "BATCH-001",
          "manufactureDate": "2026-07-01",
          "expiredDate": "2026-12-31"
        },
        "combos": [],
        "unit": {
          "id": 1,
          "name": "bó",
          "quantity": 1
        },
        "gifts": []
      }
    ],
    "carrier": {
      "id": 1,
      "name": "Ahamove",
      "orderPackageSize": {
        "height": 16,
        "merge": 0,
        "length": 42,
        "weight": 2,
        "width": 22
      },
      "service": {
        "id": 2,
        "name": "Giao chuẩn",
        "code": "STANDARD"
      },
      "carrierCode": "TRACK-88000123-UPD",
      "isPartDelivery": 0,
      "sendCarrier": {
        "type": 1,
        "date": "2026-07-24"
      },
      "deliveryDate": "2026-07-25",
      "weight": 2,
      "shipFee": 35000,
      "codFee": 0,
      "declaredFee": 0,
      "returnFee": 0,
      "customerShipFee": 35000
    },
    "payment": {
      "couponCode": "TEST-UPD",
      "credit": {
        "amount": 0,
        "accountId": 1,
        "code": "CREDIT-01"
      },
      "deposit": {
        "amount": 0,
        "accountId": 1
      },
      "discount": {
        "type": "cash",
        "amount": 5000
      },
      "transfer": {
        "amount": 0,
        "accountId": 1
      },
      "usedPoints": {
        "points": 0,
        "amount": 0
      },
      "vat": {
        "type": "percent",
        "value": 10,
        "amount": 0,
        "taxCode": null,
        "taxDate": null
      }
    },
    "originalOrder": {
      "id": 500001,
      "channel": {
        "appOrderId": "ORIGINAL-ORDER-001",
        "appShopId": "SHOP-HTM-01",
        "saleChannel": 1
      }
    }
  }
}
```

---

## 7) orderDelete

> Dùng lại `88000123` để xoá đúng đơn đã add.

```json
{
  "event": "orderDelete",
  "businessId": 126981,
  "data": [88000123]
}
```

---

## Mẹo test nhanh trên Swagger

### Luồng test đầy đủ

1. Paste payload `productAdd`
2. Paste payload `inventoryChange`
3. Paste payload `productUpdate`
4. Paste payload `orderAdd`
5. Paste payload `orderUpdate`
6. Paste payload `orderDelete`
7. Paste payload `productDelete`

### Nếu muốn test lại từ đầu

- Đổi toàn bộ các giá trị ID liên quan:
  - `99000123` → một ProductId mới
  - `88000123` → một OrderId mới
- Nhớ sửa đồng bộ ở các payload có liên quan.

### Nếu muốn test lỗi businessId

Có thể đổi:

```json
"businessId": 126981
```

thành:

```json
"businessId": 999999
```

Khi đó API sẽ trả lỗi `Business Id không khớp`.
