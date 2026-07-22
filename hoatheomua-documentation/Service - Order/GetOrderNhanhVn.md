#### Request
```Json
{
  "filters": {
    "createdAtFrom": 0,
    "createdAtTo": 0,
    "ids": [
      0
    ],
    "depotIds": [
      0
    ]
  },
  "paginator": {
    "size": 0
  },
  "dataOptions": {}
}
```
#### Response
```json
{
  "code": 1,
  "paginator": {
    "next": {
      "id": 793850250
    }
  },
  "data": [
    {
      "info": {
        "id": 793850250,
        "type": 1,
        "depotId": 129255,
        "pickupDepotId": 0,
        "mode": 0,
        "exportType": 1,
        "originalOrderId": 0,
        "returnFromId": 0,
        "idReturn": 0,
        "createdById": 0,
        "createdByName": null,
        "saleId": 0,
        "saleName": null,
        "technicalStaffId": 0,
        "packedById": 0,
        "customerCareId": 0,
        "customerCareName": null,
        "createdAt": 1782237066,
        "updatedAt": 1782237066,
        "confirmedAt": 0,
        "packedAt": 0,
        "description": null,
        "privateDescription": "Đến nơi lấy hàng đọc mã đơn để nhận hàng",
        "orderIndex": 1,
        "status": 54,
        "reason": 0,
        "handoverIds": [],
        "tags": [],
        "trackingUrl": "https://nhanh.vn/tracking?businessId=126981&id=793850250&key=6f7c0fda1328"
      },
      "channel": {
        "saleChannel": 10,
        "trafficSource": null,
        "fbAdsId": 0,
        "fbPsid": 0,
        "affiliate": {
          "value": 0,
          "bonus": 0,
          "bonusRate": 0
        },
        "marketing": []
      },
      "shippingAddress": {
        "id": 10062371,
        "name": "Tân Trần",
        "mobile": "0979901550",
        "address": "Metro, Phường Bến Thành, Hồ Chí Minh",
        "cityId": 0,
        "districtId": 0,
        "wardId": 0,
        "location": null
      },
      "products": [
        {
          "id": 4934172,
          "typeId": 0,
          "code": "HB_TC 1B_2026",
          "name": "Tú cầu bó 1 bông_ 2026",
          "barcode": "2000214274967",
          "weight": 1500,
          "quantity": 1,
          "originalQuantity": 1,
          "originalPrice": 0,
          "displaySalePrice": 149000,
          "displayVatAmount": 0,
          "price": 149000,
          "priceVatMode": 2,
          "priceAfterVAT": 149000,
          "discount1QtyAmount": 0,
          "discount": 0,
          "discountAmount": 0,
          "avgCost": 13641,
          "vat": 0,
          "vatPercent": 0,
          "vatPercentSub": 0,
          "vatAmount": 0,
          "imeiId": 0,
          "imeiIds": [],
          "imeiCodes": [],
          "transactionFee": 0,
          "paymentStatus": 0,
          "partialReturnStatus": 0,
          "usedPoints": 0,
          "usedPointAmount": 0,
          "image": "https://pos.nvncdn.com/281991-126981/ps/Tu-cau-bo-1-bong_-2026.png?v=1777817025",
          "shippingWeight": "1500",
          "unit": {
            "id": 0,
            "name": "Bó",
            "quantity": 0,
            "originalQuantity": 0,
            "price": 0,
            "priceAfterVAT": 0
          }
        }
      ],
      "carrier": {
        "id": 12,
        "name": "Tự Vận Chuyển",
        "service": {
          "id": 0,
          "type": 0
        },
        "checkGoods": 0,
        "isPartDelivery": 0,
        "isDeclaredFee": 0,
        "sendCarrierType": 1,
        "sendCarrierAt": 0,
        "deliveryAt": 1782237066,
        "declaredValue": 0,
        "declaredFee": 0,
        "weight": 1500,
        "shipFee": 0,
        "codFee": 0,
        "ecomFee": 0,
        "overWeightShipFee": 0,
        "returnFee": 0,
        "customerShipFee": 85000,
        "carrierPaymentStatus": 0
      },
      "payment": {
        "usedPoints": {
          "points": 0,
          "amount": 0
        },
        "vat": {
          "percent": null,
          "amount": 0
        },
        "discount": {
          "percent": null,
          "amount": 0
        },
        "deposit": {
          "amount": 0,
          "accountId": 0
        },
        "transfer": {
          "amount": 234000,
          "accountId": 0,
          "name": "QUỸ NGÂN HÀNG SHINHAN"
        },
        "credit": {
          "amount": 0,
          "accountId": 0
        },
        "codAmount": 0,
        "businessPayment": 0,
        "businessPaymentStatus": 0
      }
    }
  ]
}
```
#### Flow chart

![[../Store-Image/Pasted image 20260625113453.png]]

#### Test case
**Test case 01: Lấy danh sách order từ NhanhVn thành công**
- **Điều kiện:** NhanhVn API `/order/list` trả về response hợp lệ, `Code != 0`, có dữ liệu order.
- **Kết quả:** Hệ thống trả về `OrderListRootResponse`.
**Test case 02: NhanhVn API trả về lỗi HTTP**
- **Điều kiện:** Gọi API `/order/list` thất bại.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVn API HTTP Error`.
**Test case 03: NhanhVn response null**
- **Điều kiện:** API trả về response null.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVnResponse returned null or empty data`.
**Test case 04: NhanhVn trả về Code = 0**
- **Điều kiện:** Response từ NhanhVn có `Code = 0`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `ERR_INVALID_FORM_FIELDS`.
