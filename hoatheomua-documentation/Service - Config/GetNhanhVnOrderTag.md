  
#### Request  
```CSharp  
// GET /api/v1/configs/order-tags  
// Không có tham số  
```  
#### Response  
```CSharp  
{
  "value": {
    "group": "nhanhvn",
    "label": "Order Tags",
    "value": [
      1
    ],
    "options": [
      {
        "label": "Tag 1",
        "value": 1
      }
    ],
    "required": true,
    "inputType": "multiSelect",
    "validations": []
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "0HNMH0HLC67KU:00000005",
  "timestampUtc": "2026-06-23T08:51:30.37969Z"
} 
```

#### Flow diagram
![[../Store-Image/GetNhanhVnOrderTag.png]]


#### Test case

**Test case 01: Config hợp lệ trả về options đã lọc**
- **Điều kiện:** Config `nhanhvn.order_tags` có `Value` là JSON hợp lệ với `value = [1, 2]` và `options` chứa các tag có `value` 1 và 3.
- **Kết quả:** Hệ thống trả về kết quả chỉ chứa option có `value = 1` (option khớp với danh sách `value`).

**Test case 02: Không tìm thấy config**
- **Điều kiện:** Không có config nào có key `nhanhvn.order_tags`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `NhanhVnOrderTag Not Found`.

**Test case 03: Value sai định dạng JSON**
- **Điều kiện:** Config tồn tại nhưng `Value = "invalid-json"`.
- **Kết quả:** Hệ thống ném lỗi `JsonException` khi parse.

**Test case 04: Các thuộc tính trong config bị null**
- **Điều kiện:** Config có `Value` là JSON hợp lệ nhưng `value` và `options` đều `null`.
- **Kết quả:** Hệ thống ném lỗi `BadRequestException` với message `Cannot convert value`.
