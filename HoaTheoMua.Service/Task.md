Hiện tại ở field DeliveryTime ở folder Webhook của tôi đang lỗi định dạng, 

định dạng đúng của  field "DeliveryTime" ở dòng 421, phải là  kiểu tring với định dạng như thế này:  " 15:30:00". 

Nhưng fields đó không nhận từ người dùng được lấy từ tagIds, nếu không có tagIds thì để "", để bên nhanhvn học lấy thời gian đặt hàng là làm DeliveryTime

Và tagIds cho FE truyền xuống, ở dạng mãng , với định dạng là một cái List<long>, nhưng chữ là các số. Nên không thể truyền trực tiếp vào fiell DeliveryTime được

Tôi nghĩ luồng đó DeliveryTime nên xử lí như thế nào
 DeliveryTime = (order.OrderTime == 0 && order.TagIds!.Count == 0)  ?  " "  :  "chỗ nào phải tìm cách nào đó giúp convert từ tagIds được truyền xuống trở thành định như ở trên (15:30:00)",

Tôi có một hàm nằm trong class Service ở folder Config giấy order tags 
và nó trả về như thế này 

{ "value": { "group": "nhanhvn", "label": "Giờ giao hàng", "value": [ 273, 270, 246, 238, 237, 236, 234, 229 ], "options": [ { "label": "18h - 20h", "value": 273 }, { "label": "6h - 7h", "value": 270 }, { "label": "7h - 8h", "value": 246 }, { "label": "17h - 19h", "value": 238 }, { "label": "15h - 17h", "value": 237 }, { "label": "13h - 15h", "value": 236 }, { "label": "10h - 12h", "value": 234 }, { "label": "8h -10h", "value": 229 } ], "required": true, "inputType": "multiSelect", "validations": [] }, "isSuccess": true, "isFailed": false, "error": null, "traceId": "0HNMU6NPKAJJH:00000001", "timestampUtc": "2026-07-10T03:55:33.5944853Z" }

thì lấy các giờ đầu, sau đó convert thành thành định dạng như thế này:
15:30:00, hoặc có phương án nào dễ hơn không, tôi chưa hình dung được hãy hướng dẫn giúp tôi

