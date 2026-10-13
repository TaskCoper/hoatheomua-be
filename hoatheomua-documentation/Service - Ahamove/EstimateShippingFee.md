#### Request  
```json  
// POST /api/v1/ahamove/estimate  
{
  "latitude": 10.762622,
  "longitude": 106.660172,
  "ward": 10376,
  "city": 255,
  "address": "108/3 Đào Duy Anh, Phường 9, Phú Nhuận",
  "shortAddress": "108/3 Đào Duy Anh",
  "fullName": "Nguyễn Văn A",
  "phone": "0988364353",
  "remarks": "Giao giờ hành chính",
  "orderTime": 0,
  "items": [
    {
      "itemId": 101,
      "number": 2
    }
  ]
} 
```  
#### Response  
```json  
{
  "value": {
    "ahamoveResult": [
      {
        "service_id": "SGN-BIKE",
        "data": {
          "distance": 9,
          "duration": 900,
          "distance_fee": 25000,
          "request_fee": 0,
          "stop_fee": 0,
          "vat_fee": 0,
          "discount": 0,
          "total_fee": 25000,
          "total_price": 25000,
          "requests": []
        }
      }
    ],
    "products": [],
    "shippingFee": 20000
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMLOJUN580N:00000004",
  "timestampUtc": "2026-06-29T10:48:17.589154Z"
}  
```
`shippingFee` là phí người mua trả, FE hiển thị field này (không dùng `total_fee` của Ahamove). Mọi khoảng cách tính theo km, `free_ship` lấy từ config `ahamove.free_ship`:
- `distance ≤ free_ship` → `shippingFee = 0`
- `distance > free_ship` → `shippingFee = (distance − free_ship) × 5.000đ`, làm tròn tới đồng. Ví dụ `free_ship = 5`, `distance = 9` → 20.000đ.
- Không có config (hoặc ≤ 0) hoặc `distance` không hợp lệ → `shippingFee = total_fee` của Ahamove.

`ahamoveResult` giữ nguyên báo giá gốc của hãng (phí shop trả Ahamove).
#### Flow diagram
![[../Store-Image/EstimateShippingFee.png]]
#### Test case
**Test case 01**: Company address không tồn tại trong DB  
**Điều kiện:** Bảng CompanyAddresses rỗng (không có địa chỉ công ty nào), gọi EstimateShippingFee.  
**Kết quả:** Hệ thống ném Exception với message "Company address not found".  
  
**Test case 02**: NhanhVn trả về Code == 0  
**Điều kiện:** Có company address hợp lệ, request có Items, nhưng NhanhVn trả về response với Code = 0.  
**Kết quả:** Hệ thống ném BadRequestException với message "ERR_INVALID_FORM_FIELDS".  
  
**Test case 03**: ItemId trong request không tồn tại trong dữ liệu NhanhVn trả về  
**Điều kiện:** Company address hợp lệ, NhanhVn trả về Code = 1 với sản phẩm có Id = 999, nhưng request truyền ItemId = 1 (không khớp).  
**Kết quả:** Hệ thống ném InvalidOperationException (do không tìm thấy sản phẩm khớp ItemId).  
  
**Test case 04**: Logic OrderTime và IdleUntil khi gọi AhaMove  
**Điều kiện:** Company address hợp lệ, NhanhVn trả về sản phẩm khớp ItemId, request có OrderTime = 1680000000 (> 0).  
**Kết quả:** Request gửi sang AhaMove có OrderTime = 1680000000 và IdleUntil = 1680000000 (IdleUntil được map đúng bằng OrderTime).  
  
**Test case 05**: AhaMove trả về danh sách null hoặc rỗng  
**Điều kiện:** Company address hợp lệ, NhanhVn trả về hợp lệ, nhưng AhaMove trả về danh sách ServiceResponse rỗng.  
**Kết quả:** Hệ thống ném BadRequestException với message "Ahamove returned null or empty data".  
  
**Test case 06**: Request hợp lệ (Happy case)  
**Điều kiện:** Company address hợp lệ, NhanhVn trả về Code = 1 với sản phẩm khớp ItemId, AhaMove trả về danh sách service hợp lệ.  
**Kết quả:** Trả về object kết quả khác null (ước tính phí ship thành công).  
  
**Test case 07**: Tính `shippingFee` theo bán kính miễn phí  
**Điều kiện:** Config `ahamove.free_ship = 5`, Ahamove trả `total_fee = 30000` với `distance` lần lượt 3 / 5 / 5,4 / 9 / 12,5 km; thêm trường hợp `free_ship = 5,5` với 7 km và trường hợp không có config với 9 km.  
**Kết quả:** `shippingFee` lần lượt 0 / 0 / 2.000 / 20.000 / 37.500đ; 7.500đ; không có config thì bằng phí hãng 30.000đ.