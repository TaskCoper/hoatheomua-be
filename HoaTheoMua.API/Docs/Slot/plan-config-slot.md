# I. Yêu cầu

-> Khách hàng muốn giới hạn số lượng đơn được đặt trong một khung giờ nhất định

# II. Cách triển khai, và những vấn đề gặp phải

## 1. Cách triển khai

Admin sẽ được cấu hình cấu hình số lượng đơn được đặt trong từng khung giờ cụ thể.

Khung giờ này được đồng bộ và đồng nhất với khung giờ giao hàng bên NhanhVn (gọi là **nhãn**) (từ giờ trở đi từ **nhãn**(lable): được hiểu là **khung giờ giao hàng**)

## 2. Vấn đề đang gặp

Hiện tại muốn quản lí được số lượng đơn hàng trong một khung giờ, thì mọi đơn hàng đều bắt buộc phải có nhãn. Ta sẽ dựa vào thời gian là : ngày + nhãn -> để tiến hành đếm được nhãn đó đã được đặt bao nhiêu lần trong một ngày.

Vấn đề, hiện tại trong hệ thống của BE có tới 2 bảng order: đó là OrderNhanhVn và Order

Chúng ta sẽ dựa vào **OrderNhanhVn** để đếm chính xác những đơn đã được đặt, nhưng hiện tại webhook về sự kiện Order của NhanhVn, không bắn giá trị là **nhãn** (đã viết mail, đang chờ phản hồi), hiện tại mặc định trong webhook sẽ bắn một trường là LabelIds (chứa giá trị các nhãn), vì vậy ta có thể đếm được đó là với một nhãn, đã được đặt thành công bao nhiêu lần trong một ngày ở table **OrderNhanhVn**. Sau đó, cộng với những đơn hàng đang ở trạng thái "Pending" của table Order, có trùng ngày và trùng nhãn với OrderNhanhVn (cơ chế giữ chỗ).

=> Sau khi đã có hai giá trị trên thì ta có thể quản lí được nhãn đó đã được được bao nhiêu lần trong một ngày nhất định

Field mới webhook bắt về có thể là labelIds: [253, 257] -> trong bảng OrderNhanhVn tạo một field tên là LabelIds luôn: để lưu nhãn từ webhook bắn xuống.

Thật ra trong bảng OrderNhanhVn cũng có một field tên là: "Nhan", nó không chứa LabelIds mà chứa string kiểu: "13h - 15h, DOANH NGHIỆP, VAT, Ngọc Hưởng, Mỹ Linh",

## 3. Cách triển khai chi tiết

### 3.1: Admin được cấu hình số đơn hàng có thể đặt đới với từng nhãn(từng khung giờ)

- Lúc trước, trong config đã có giá trị của các nhãn được đồng bộ bằng cách gọi API lên NhanhVn, nên hiện tại tôi sẽ chỉnh sửa cái config đó, để có thể cho admin chỉnh sửa được,
    
- Tại sao chúng ta lại phải đồng bộ chỉnh sửa config này, mà ko tạo một config mới. Vì không đồng bộ được các nhãn với NhanhVn
    
- Format của config mới
    

```
{
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
  "slotLimits": {
    "273": null,
    "270": null,
    "246": null,
    "238": null,
    "237": null,
    "236": null,
    "234": null,
    "229": null
  },
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

### 3.2: Cần một API trả ra số lượng có thể đặt của các tất cả các nhãn (có thể filter theo một nhãn) để người dùng thấy và có thể chọn.

Url:

```
GET /api/v1/delivery-slots/availability?date=2026-10-05
```

hoặc

```
GET /api/v1/delivery-slots/availability?date=2026-10-05&tagId=238
```

#### Flow BE:

- BE sẽ đọc config lấy ra được các slot limit của các nhãn
    
- Chuyển date mà FE truyền xuống thành [Đầu ngày, đầu ngày kế tiếp)
    
- Với từng LabelId mà nằm trong field: value của config, tiến hành đếm bên OrderNhanhVn, với điều kiện:
    
    - IsDeleted = false
        
    - ThoiGianDonHang nằm trong ngày cần kiểm tra
        
    - LabelIds trong config chứa LabelId của đơn hàng
        
    - Và loại đi: TrangThaiDonHang khác "63" và "Khách hủy"
        
- Đếm Order đang giữ chỗ:
    
    - Status = "Pending"
        
    - IsUrgent = false (cái này nói sau)
        
    - CreateAt nằm trong ngày cần kiểm tra
        
    - LabelIds trong config chứa LabelId của đơn hàng
        
- Tiến hành tính: `occupied(chiếm): orderNhanhvnCount + pendingOrderConut`
    
- Trả cho FE theo format này
    

```
{
  "date": "2026-10-05",
  "slots": [
    {
      "tagId": 238,
      "label": "17h - 19h",
      "limit": 20,
      "occupied": 12
    },
    {
      "tagId": 273,
      "label": "18h - 20h",
      "limit": 15,
      "occupied": 15
    }
  ]
}
```

- Lưu ý: LabelId với TagId là giống nhau. Trong config thì gọi là Label, trong Order thì gọi là Tag (theo giống với nhanhVn)
- Điều bắt buộc, đó là đơn hàng nào cũng đều phải có Tag (loại trừ tag của thuế). Có một vấn đề xuất hiện, ngoại trừ các tag là chứa thời gian. Ví dụ: 6h - 7h, 7h - 8h, thời còn có một tag tên là **GẤP**
    
    - Tag Gấp này có thể cho admin cấu hình được thời gian gọi Shipper
        
    - Nên trong request của order tiến hành bổ sung thêm một field tên là isUrgent với kiểu dữ liệu là boolean. Mục đích dùng để validate và phân biệt được tag gấp so với các tag có chứa thời gian.
        
    - Nếu chọn Tag gấp (IsUrgent = true) thì admin được config thời gian delay và không cần validate tag chứa giờ
        
    - Các trường hợp còn lại thì bắt buộc phải chứa Tag có giờ (chỉ 1 tag)
        
    - Hiện tại đã confirm với a Bình là Tag gấp thì sẽ ko giới hạn số lượng đơn
        

### 3.3

Trong hàm CreateOrder thì cũng sẽ có một hàm check lại số lượng có thể bans đối với tag đó, và 3.2 và 3.3 được dùng chung một hàm với nhau

### 3.4

Cần một config Backgroud job để tiến hành hủy đơn "Pending" quá thời gian về "Cancelled" -> để lúc tính thì tiến hành đếm pending cho nó chuẩn

### 3.5

Chỉnh sửa phần response của api lấy tag order nhanhvn một xíu, đợt trước thì chỉ trả ra tag, bây giờ trả ra thêm giới hạn của các tag đó nữa