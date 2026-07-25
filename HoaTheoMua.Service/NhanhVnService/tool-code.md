Hiện tại bạn các hàm nằm trong folder NhanhVnService: WebhookNhanhVnOrderUpdate, WebhookNhanhVnOrderDelete, WebhookNhanhVnOrderAdd. Lúc trước chúng sẽ tương tác với bảng Order. Nhưng bây giờ có sự thay đổi nên chúng sẽ tương tác với bảng OrderNhanhVn.cs

Trước hết, bạn hãy đọc các fields nằm trong class OrderNhanhVn.cs ở folder Entity, để hiểu được ý nghĩ của các fields , cộng với đọc thêm file excel nằm trong folder Entity để hiểu được các fields nằm trong table OrderNhanhVn, vì tôi muốn webhook liên quan đến order là: WebhookNhanhVnOrderUpdate, WebhookNhanhVnOrderDelete, WebhookNhanhVnOrderAdd sẽ tương tác với bảng OrderNhanhVn, và điều khá là sự đồng nhất các fields, nên tôi cần bạn đọc kĩ.

Sau khi bạn đã hiểu được các feilds nằm trong bảng OrderNhanhVns, bước tiếp theo là bạn vào lần lượt các hàm WebhookNhanhVnOrderAdd, WebhookNhanhVnOrderUpdate, 
WebhookNhanhVnOrderDelete và tiến hành sửa code chuyển qua tương tác có thể là xóa, cập nhật, update tới bảng OrderNhanhVns chớ không còn bảng Orders như trước nữa.

Nhưng có một điều quan trọng là tôi cần bạn update hoặc tạo đúng các fields giữa phần payload của order (gồm có 3 file .md đều nằm trong folder nhanhVnService: webhooks order add, webhooks order update, và webhooks order delete). Hãy giúp tôi map đúng các fields.

Lưu ý, trong code của webhook liên quan đến order phía trên khi bạn sửa, hãy bỏ hết tất cả các chỗ nào throw lỗi, nếu có lỗi thì cứ return string giúp tôi để lúc này status cũng phải 200
