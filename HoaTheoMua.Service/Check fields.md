Context, lúc trước khi tương tác với nhanh Vn, tôi sẽ gọi các api mà nhanhvn cung cấp, nhưng khi sử dụng được một thời gian,  thì bị một vấn đề đó là rate limit.

Vì vậy, mới sinh ra được giải pháp dùng webhook của nhanhvn bắn ngược lại về server của tôi.

Hiện tại, trong folder ProductImport, là một folder dùng để import file excel, tức là là bên nhanhvn xuất ra một file excel chứa các sản phẩm, tôi dùng hàm đó để import các sản phẩm vào bên trong entity product của tôi, để khi webhook bắn về sẽ cập nhật vào table product đó và có lúc sẽ query dữ liệu từ bảng product đó luôn

Cái khó ở đây là sự đồng nhất giữa các fields sao cho đúng,
- Bạn hãy đọc giúp tôi class Request trong folder ProductImport, kết hợp với entity Product, để biết và hiểu được hết các fields của nó.

Hiện tại trong folder NhanhVnService, đã có 7 cái webhooks mà tôi đã cấu hình rồi. Nhiệm vụ của bạn, khi tôi ra hiệu lệnh, thì bạn phải kiểm tra xem cái webhooks đó có thực hiện đúng các tương ứng với các fields nằm trong bảng product chưa. 