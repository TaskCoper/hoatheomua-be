# Bàn giao FE — PostGen mốc 11

> **Phạm vi chốt 2026-09-15:** chỉ hiển thị cấu hình/kết nối tài khoản và luồng đăng tải Facebook/Instagram. Zalo tạm hoãn: chưa hiển thị thao tác kết nối/đăng/lập lịch Zalo và không gọi OAuth Zalo. Backend giữ `PlatformAccountOptions:Zalo:Enabled=false`. Phạm vi này không thay đổi tính năng sinh nội dung/ảnh Zalo hiện có; contract account Zalo được giữ để tiếp tục sau.

Ngày 2026-09-15. Tài khoản Facebook Page, Instagram chuyên nghiệp qua Facebook Login, Zalo OA dùng chung cho Admin. Contract đầy đủ: [PlatformAccounts.md](Service%20-%20PostGen/PlatformAccounts.md).

| Thao tác | API |
| --- | --- |
| Danh sách account | GET `/api/v1/post-gen/platform-accounts?platform=facebook` (platform tùy chọn) |
| Bắt đầu kết nối | POST `/api/v1/post-gen/platform-accounts/{platform}/connect` |
| Kết nối lại | POST `/api/v1/post-gen/platform-accounts/{id}/reconnect` |
| Làm mới/kiểm tra token | POST `/api/v1/post-gen/platform-accounts/{id}/refresh` |
| Ngắt kết nối | DELETE `/api/v1/post-gen/platform-accounts/{id}` |

Các POST không cần body. Dùng JWT Admin và credentials include. Backend trả envelope hiện có. Connect/reconnect trả `value = { sessionId, authorizationUrl, expiresAt }`; mở nguyên URL trên cùng trình duyệt. Cookie OAuth HttpOnly/SameSite=Lax yêu cầu FE/API cùng site; không chuyển sang trình duyệt khác hoặc tự gọi callback bằng code/state.

Callback do provider điều hướng đến backend, không cần JWT và trả JSON envelope. FE dùng popup, tải lại danh sách sau khi popup đóng; chưa có callback redirect về FE/postMessage. Khi flow thất bại, gọi connect/reconnect mới, không retry URL callback.

List trả `value: Account[]`; refresh/disconnect trả `value: Account`. Account gồm id, platform, externalAccountId, pageId, displayName, status, grantedScopes, accessTokenExpiresAt, refreshTokenExpiresAt, lastValidatedAt, lastErrorCode, connectedBy, createdAt, updatedAt, revision. Không có token trong response.

- Connected: hiển thị account và lần kiểm tra gần nhất. Chưa đồng nghĩa đã đủ điều kiện đăng bài.
- ReauthRequired: hiển thị nút kết nối lại. Refresh có thể trả HTTP 200 với trạng thái này; luôn đọc status/lastErrorCode.
- Disabled: giữ trong danh sách, hiển thị nút kết nối lại; ID không đổi khi reconnect thành công.
- 409 PLATFORM_REFRESH_IN_PROGRESS/PLATFORM_ACCOUNT_CHANGED: chờ hoặc tải lại; không gửi refresh liên tục.
- 409 PLATFORM_ACCOUNT_IN_USE: cần hủy lịch/chờ tác vụ đăng trước khi ngắt kết nối.
- 503 PLATFORM_NOT_CONFIGURED: chưa bật/cấu hình app cho môi trường; đây không phải lỗi JWT người dùng.

Cấu hình Docker/GitHub và callback công khai: [hướng dẫn môi trường](../.docker/README.md). Cookie OAuth dùng path của callback cấu hình, kể cả khi ingress ánh xạ `/ecommerce-api/...` sang `/api/...`; FE mở nguyên authorizationUrl backend trả về, không tự thêm config_id hoặc scope.

Backend tự xác minh tài khoản từ provider. Callback Meta có thể kết nối nhiều Page/Instagram đã được cấp quyền; FE không phải nhập Page ID. Reconnect chỉ chấp nhận đúng account cần nối lại. Zalo grantedScopes rỗng nghĩa là provider chưa trả danh sách quyền cho contract này.

Mốc 9–10 tạm hoãn. Mốc 11 chưa có API publish/schedule, không gọi AI hoặc upload ảnh. Danh sách bài vẫn dùng platforms.content và platforms.images hiện có.
