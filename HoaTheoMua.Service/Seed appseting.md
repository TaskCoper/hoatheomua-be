```Json
 Key: nhanhvn.token
 Value: {
  "group": "nhanhvn",
  "label": "Token của NhanhVN",
  "value": "qC6szW89FwrjJTiwHGT7FtJOjW4tNSRwrp080cT9klzcIq7IWW54nH4C4JZL3jU9yrBlQ71jZ952BBu1nswx1eLrIZoNRxDN4JxIgFdn6yUsAXbcTByGH1J8ue3FonfdL0Jc9OmGK4rDBA8f7tnhjUGVP3f8QPj4I8Jh5Ly9RkZPncG0QwyE7kEvO97vrc",
  "options": [],
  "required": true,
  "inputType": "text",
  "validations": []
}
```

```Json
Key: ahamove.api_key
Value: {
  "group": "ahamove",
  "label": "Ahamove API Key",
  "value": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0eXAiOiJ1c2VyIiwiY2lkIjoiODQ5Nzk5MDE1NTAiLCJzdGF0dXMiOiJBQ1RJVkFUSU5HIiwiZW9jIjoiaW5mb0B2bnpkbmEuY29tIiwibm9jIjoiUElFRFRFQU0gQWRtaW4iLCJjdHkiOiJTR04iLCJ0eXBlIjoiYXBpIiwicGFydG5lciI6InBpZWR0ZWFtIiwiZXhwIjoxODQ0NjQ5Mjc1LCJpYXQiOjE3ODE1NzcyNzUsImlzcyI6ImFoYSJ9.nu4YlyR-63fv29ArZOkcrym8Tder7TaxmqZn4LG25xk",
  "options": [],
  "required": true,
  "inputType": "text",
  "description": "API key của ahamove cho các dịch vụ giao hàng",
  "validations": []
}

```

```Json
Key: nhanhvn.app_id
Value: {
  "group": "nhanhvn",
  "label": "App ID",
  "value": 77683,
  "options": [],
  "required": true,
  "inputType": "number",
  "validations": []
}
```

```Json
Key: nhanhvn.default_depot
Value: {
  "group": "nhanhvn",
  "label": "Kho mặc định",
  "value": 129255,
  "options": [
    {
      "label": "Kho CN HEAD",
      "value": 129255
    },
    {
      "label": "Kho Tổng",
      "value": 130526
    }
  ],
  "required": true,
  "inputType": "select",
  "description": "Cho đặt hàng trên web",
  "validations": []
}
```

```Json
Key: nhanhvn.business_id
Value: {
  "group": "nhanhvn",
  "label": "Business ID",
  "value": 126981,
  "options": [],
  "required": true,
  "inputType": "number",
  "validations": []
}
```

```Json
Key: ahamove.base_url
Value: {
  "group": "ahamove",
  "label": "Đường dẫn mặc định cho API",
  "value": "https://partner-apistg.ahamove.com",
  "options": [],
  "required": false,
  "inputType": "text",
  "description": "Đường dẫn mặc định cho API của Ahamove",
  "validations": [
    {
      "url": true
    }
  ]
}
```

```Json
Key: ahamove.shipping.sgn_bike_reim_bulky
Value: {
  "group": "ahamove",
  "label": "Hàng hoá cồng kềnh",
  "value": "{\n  \"_id\": \"SGN-BIKE-REIM-BULKY\",\n  \"name\": \"Hàng hoá cồng kềnh\",\n  \"price\": 50000\n}",
  "options": [],
  "required": false,
  "inputType": "json",
  "description": "Phí phát sinh thêm do hàng hoá cồng kềnh hơn so với mức quy định",
  "validations": []
}
```

```Json
Key: ahamove.shipping.sgn_bike_d2d
Value: 
{
  "group": "ahamove",
  "label": "Giao hàng tận tay",
  "value": "{\n  \"_id\": \"SGN-BIKE-D2D\",\n  \"num\": 1\n}",
  "options": [],
  "required": false,
  "inputType": "json",
  "description": "Hàng hóa sẽ được trao tận tay người nhận ở những điểm giao hàng trên tầng cao như chung cư hoặc cơ quan, bệnh viện…",
  "validations": []
}
```

```Json
Key: ahamove.shipping.sgn_bike_sms
Value: {
  "group": "ahamove",
  "label": "Gửi SMS cho người nhận",
  "value": "{\n  \"_id\": \"SGN-BIKE-SMS\",\n  \"num\": 1\n}",
  "options": [],
  "required": false,
  "inputType": "json",
  "description": "Trạng thái đơn hàng sẽ được gửi qua tin nhắn SMS giúp người nhận dễ dàng theo dõi mọi lúc mọi nơi.",
  "validations": []
}
```

```Json
Key: ahamove.shipping.sgn_bike_fragile
Value: {
  "group": "ahamove",
  "label": "Giao hàng dễ vỡ",
  "value": "{\n  \"_id\": \"SGN-BIKE-FRAGILE\",\n  \"num\": 1\n}",
  "options": [],
  "required": false,
  "inputType": "json",
  "description": "Hàng hóa dễ vỡ, dễ hư hỏng sẽ được nâng niu và bảo vệ tối đa suốt quá trình vận chuyển.",
  "validations": []
}
```

```Json
Key: nhanhvn.order_sources
Value: {
  "group": "nhanhvn",
  "label": "Nguồn đơn hàng",
  "value": 1674,
  "options": [
    {
      "label": "TRẠM HOA",
      "value": 1682
    },
    {
      "label": "CRM",
      "value": 1681
    },
    {
      "label": "NGUỒN NEW 1",
      "value": 1680
    },
    {
      "label": "Nội bộ",
      "value": 1679
    },
    {
      "label": "MKT",
      "value": 1678
    },
    {
      "label": "Instagram HTM",
      "value": 1677
    },
    {
      "label": "Zalo Mini App",
      "value": 1676
    },
    {
      "label": "Partner",
      "value": 1675
    },
    {
      "label": "WEBSITE",
      "value": 1674
    },
    {
      "label": "CTV",
      "value": 1673
    },
    {
      "label": "Shopee",
      "value": 1672
    },
    {
      "label": "Zalo OA HTM",
      "value": 1671
    },
    {
      "label": "https://hoatheomua.com",
      "value": 1519
    },
    {
      "label": "B2B",
      "value": 1502
    },
    {
      "label": "Meta Shop",
      "value": 1403
    },
    {
      "label": "CSKH",
      "value": 1348
    },
    {
      "label": "Zalo (Hotline)",
      "value": 996
    },
    {
      "label": "Other (BNI)",
      "value": 995
    }
  ],
  "required": true,
  "inputType": "select",
  "validations": []
}
```

```Json
Key: nhanhvn.order_tags
Value: {
  "group": "nhanhvn",
  "label": "Giờ giao hàng",
  "value": [
    273,
    270,
    246,
    238,
    237,
    236,
    234,
    229
  ],
  "options": [
    {
      "label": "GIÀU",
      "value": 293
    },
    {
      "label": "Shipper Bảo",
      "value": 292
    },
    {
      "label": "Shipper Huy",
      "value": 291
    },
    {
      "label": "Shipper Hưng",
      "value": 290
    },
    {
      "label": "Ship Aha",
      "value": 287
    },
    {
      "label": "Ngọc Trân",
      "value": 274
    },
    {
      "label": "18h - 20h",
      "value": 273
    },
    {
      "label": "Purchasing",
      "value": 272
    },
    {
      "label": "6h - 7h",
      "value": 270
    },
    {
      "label": "ĐIỆN HOA",
      "value": 262
    },
    {
      "label": "Mỹ Linh",
      "value": 259
    },
    {
      "label": "Ngọc Hưởng",
      "value": 257
    },
    {
      "label": "Trà My",
      "value": 254
    },
    {
      "label": "VAT",
      "value": 253
    },
    {
      "label": "upsale",
      "value": 250
    },
    {
      "label": "DOANH NGHIỆP",
      "value": 248
    },
    {
      "label": "7h - 8h",
      "value": 246
    },
    {
      "label": "17h - 19h",
      "value": 238
    },
    {
      "label": "15h - 17h",
      "value": 237
    },
    {
      "label": "13h - 15h",
      "value": 236
    },
    {
      "label": "10h - 12h",
      "value": 234
    },
    {
      "label": "8h -10h",
      "value": 229
    }
  ],
  "required": true,
  "inputType": "multiSelect",
  "validations": []
}
```

```Json
Key: love.secret
Value: {
  "group": "love",
  "label": "Bí mật trong tim của Hùng",
  "value": "Hoàng Uyên",
  "options": [],
  "required": true,
  "inputType": "text",
  "validations": []
}
```
