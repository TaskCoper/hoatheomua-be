```CSharp
event=orderUpdate businessId=126981 data={"shippingAddress":{"id":10062389,"name":"Danh Nguyen","mobile":"0384875317","address":"72 Lê Thánh Tôn, Phường Sài Gòn, Hồ Chí Minh","cityId":312,"districtId":0,"wardId":13638,"locationVersion":"v2"},"info":{"id":813865542,"depotId":129255,"pickupDepotId":0,"type":1,"status":54,"createdBy":{"id":3652568,"name":"Đỗ Hữu Thiện"},"sale":{"id":3652568,"name":"Đỗ Hữu Thiện"},"technicalStaff":{"id":0},"customerCare":{"id":0},"createdAt":1784954484,"updatedAt":1784954966,"description":"TEST TEST111","privateDescription":"TEST TEST","orderPartDeliveries":[{"id":0,"type":0,"totalQuantity":0,"totalProductAmount":0,"status":0}],"trackingUrl":"https://nhanh.vn/tracking?businessId=126981&id=813865542&key=d04ddedbfc9c"},"products":[{"id":4933683,"typeId":0,"code":"HB_TAMBBKL_2026","name":"Bó Hoa Cúc Tana mix Hoa Baby khổng lồ_2026","avatar":"https://pos.nvncdn.com/281991-126981/ps/Bo-Hoa-Cuc-Tana-mix-Hoa-Baby-khong-lo_2026-2.jpg?v=1784277442","weight":500,"quantity":1,"price":969000,"originalPrice":0,"discount":0,"available":0,"avgCost":0,"vat":{"value":0,"amount":0},"imei":{"id":"0"},"batch":{"id":0},"combos":[{"id":0,"quantity":0}],"unit":{"id":0,"name":"set","quantity":0},"gifts":[{"id":0,"quantity":0}]}],"carrier":{"id":12,"name":"Tự Vận Chuyển","orderPackageSize":{"height":0,"merge":0,"length":0,"weight":0,"width":0},"service":{"id":0},"isPartDelivery":0,"sendCarrier":{"type":1},"deliveryDate":"2026-07-25","weight":500,"shipFee":0,"codFee":0,"declaredFee":0,"returnFee":0,"customerShipFee":0},"channel":{"ecomOrderId":0,"appShopId":"0","saleChannel":1,"trafficSource":{"id":995,"name":"Other (BNI)"},"facebook":{"adId":"0","postId":"0"}},"payment":{"businessPaymentStatus":99,"credit":{"amount":0,"accountId":0},"deposit":{"amount":0,"accountId":0},"discount":{"type":"cash","amount":0},"transfer":{"amount":0,"accountId":0},"usedPoints":{"points":0,"amount":0},"vat":{"type":"percent","value":0,"amount":0}},"originalOrder":{}}
```

- Config
```CSharp
{
  "event": "orderUpdate",
  "businessId": 126981,
  "data": {
    "shippingAddress": {
      "id": 10062389,
      "name": "Danh Nguyen",
      "mobile": "0384875317",
      "address": "72 Lê Thánh Tôn, Phường Sài Gòn, Hồ Chí Minh",
      "cityId": 312,
      "districtId": 0,
      "wardId": 13638,
      "locationVersion": "v2"
    },
    "info": {
      "id": 813865542,
      "depotId": 129255,
      "pickupDepotId": 0,
      "type": 1,
      "status": 54,
      "createdBy": {
        "id": 3652568,
        "name": "Đỗ Hữu Thiện"
      },
      "sale": {
        "id": 3652568,
        "name": "Đỗ Hữu Thiện"
      },
      "technicalStaff": {
        "id": 0
      },
      "customerCare": {
        "id": 0
      },
      "createdAt": 1784954484,
      "updatedAt": 1784954966,
      "description": "TEST TEST111",
      "privateDescription": "TEST TEST",
      "orderPartDeliveries": [
        {
          "id": 0,
          "type": 0,
          "totalQuantity": 0,
          "totalProductAmount": 0,
          "status": 0
        }
      ],
      "trackingUrl": "https://nhanh.vn/tracking?businessId=126981&id=813865542&key=d04ddedbfc9c"
    },
    "products": [
      {
        "id": 4933683,
        "typeId": 0,
        "code": "HB_TAMBBKL_2026",
        "name": "Bó Hoa Cúc Tana mix Hoa Baby khổng lồ_2026",
        "avatar": "https://pos.nvncdn.com/281991-126981/ps/Bo-Hoa-Cuc-Tana-mix-Hoa-Baby-khong-lo_2026-2.jpg?v=1784277442",
        "weight": 500,
        "quantity": 1,
        "price": 969000,
        "originalPrice": 0,
        "discount": 0,
        "available": 0,
        "avgCost": 0,
        "vat": {
          "value": 0,
          "amount": 0
        },
        "imei": {
          "id": "0"
        },
        "batch": {
          "id": 0
        },
        "combos": [
          {
            "id": 0,
            "quantity": 0
          }
        ],
        "unit": {
          "id": 0,
          "name": "set",
          "quantity": 0
        },
        "gifts": [
          {
            "id": 0,
            "quantity": 0
          }
        ]
      }
    ],
    "carrier": {
      "id": 12,
      "name": "Tự Vận Chuyển",
      "orderPackageSize": {
        "height": 0,
        "merge": 0,
        "length": 0,
        "weight": 0,
        "width": 0
      },
      "service": {
        "id": 0
      },
      "isPartDelivery": 0,
      "sendCarrier": {
        "type": 1
      },
      "deliveryDate": "2026-07-25",
      "weight": 500,
      "shipFee": 0,
      "codFee": 0,
      "declaredFee": 0,
      "returnFee": 0,
      "customerShipFee": 0
    },
    "channel": {
      "ecomOrderId": 0,
      "appShopId": "0",
      "saleChannel": 1,
      "trafficSource": {
        "id": 995,
        "name": "Other (BNI)"
      },
      "facebook": {
        "adId": "0",
        "postId": "0"
      }
    },
    "payment": {
      "businessPaymentStatus": 99,
      "credit": {
        "amount": 0,
        "accountId": 0
      },
      "deposit": {
        "amount": 0,
        "accountId": 0
      },
      "discount": {
        "type": "cash",
        "amount": 0
      },
      "transfer": {
        "amount": 0,
        "accountId": 0
      },
      "usedPoints": {
        "points": 0,
        "amount": 0
      },
      "vat": {
        "type": "percent",
        "value": 0,
        "amount": 0
      }
    },
    "originalOrder": {}
  }
}
```