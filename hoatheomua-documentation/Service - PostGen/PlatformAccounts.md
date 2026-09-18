# PostGen — Tài khoản nền tảng (mốc 11)

> **Phạm vi sử dụng 2026-09-15:** chỉ cấu hình/kết nối Facebook và Instagram. Zalo tạm hoãn; giữ `PlatformAccountOptions:Zalo:Enabled=false`, chưa cần App ID/secret, callback hoặc OA Zalo. Contract Zalo bên dưới được giữ để dùng khi tiếp tục. FE chỉ hiển thị thao tác kết nối Facebook/Instagram trong đợt này.

Cập nhật 2026-09-15. Backend quản lý tài khoản dùng chung cho Admin, gọi trực tiếp API chính thức. Facebook dùng Page; Instagram dùng **Facebook Login**, yêu cầu tài khoản chuyên nghiệp liên kết với Page. Zalo dùng OAuth OA với PKCE. Chưa có API đăng bài/lập lịch trong mốc này; mốc 9–10 vẫn tạm hoãn.

## API

Prefix: `/api/v1/post-gen/platform-accounts`. Các thao tác quản trị yêu cầu JWT **Admin**, không có luồng Staff. Callback không yêu cầu JWT, nhưng phải có state và cookie trình duyệt hợp lệ của phiên do Admin khởi tạo.

| Method | Route sau prefix | Đầu vào | Kết quả HTTP 200 |
| --- | --- | --- | --- |
| GET | `/` | Query `platform` tùy chọn | `value`: danh sách account, gồm account Disabled |
| POST | `/{platform}/connect` | Không body | `value`: sessionId, authorizationUrl, expiresAt; Set-Cookie |
| GET | `/{platform}/callback` | Query state, code; error khi bị từ chối | `value`: các account vừa kết nối; xóa cookie phiên |
| POST | `/{id}/reconnect` | Không body | Phiên OAuth mới gắn với account và revision hiện tại |
| POST | `/{id}/refresh` | Không body | Metadata account sau xử lý refresh/kiểm tra token |
| DELETE | `/{id}` | Không body | Metadata account với status Disabled |

`platform` chỉ nhận `facebook`, `instagram`, `zalo` (không phân biệt hoa/thường). GET không lọc theo người kết nối và không gọi provider. Callback Meta lưu các Page/Instagram đủ điều kiện trong tập được cấp quyền; callback reconnect chỉ cập nhật đúng account ban đầu. Không gửi ID OA/Page từ FE làm bằng chứng sở hữu: identity được đọc lại từ provider.

Mẫu response account (callback/list trả mảng các object này trong `value`):

```json
{
  "value": {
    "id": "11111111-1111-4111-8111-111111111111",
    "platform": "facebook",
    "externalAccountId": "123456789",
    "pageId": "123456789",
    "displayName": "Hoa Theo Mùa",
    "status": "Connected",
    "grantedScopes": ["pages_manage_posts", "pages_read_engagement", "pages_show_list"],
    "accessTokenExpiresAt": "2026-11-14T00:00:00Z",
    "refreshTokenExpiresAt": null,
    "lastValidatedAt": "2026-09-15T00:00:00Z",
    "lastErrorCode": null,
    "connectedBy": "22222222-2222-4222-8222-222222222222",
    "createdAt": "2026-09-15T00:00:00Z",
    "updatedAt": "2026-09-15T00:00:00Z",
    "revision": 1
  },
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "example-trace-id",
  "timestampUtc": "2026-09-15T00:00:00Z"
}
```

## Luồng FE và callback

1. Admin gọi connect hoặc reconnect với `credentials: include` để nhận cookie. Backend tạo state ngẫu nhiên, chỉ lưu hash; cookie proof HttpOnly/SameSite=Lax gắn với đúng đường callback và từng phiên. Cookie có Secure khi request HTTPS; localhost HTTP phục vụ kiểm thử.
2. FE mở nguyên `authorizationUrl` trên **cùng trình duyệt**, người dùng cấp quyền tại provider. Không tự tạo callback hoặc chuyển URL sang trình duyệt khác. FE/API cần triển khai cùng site để cookie Lax hoạt động với lời gọi XHR; CORS phải cho phép origin FE và credentials.
3. Provider chuyển về callback backend. Meta gửi state thông thường; Zalo mang state trong query của `redirect_uri`, theo hướng dẫn tham số callback của OA. Backend kiểm hash, browser proof, platform, hạn và UsedAt; dùng state một lần trước khi đổi code.
4. Callback trả JSON envelope; hiện chưa redirect tự động về FE hoặc gửi `postMessage`. FE có thể dùng popup và tải lại danh sách khi popup đóng. Không lấy code/state để gọi lại callback bằng JavaScript; token không đi qua FE.
5. Sau khi bị từ chối, hết hạn, lỗi đổi code hoặc mất phản hồi, bắt đầu phiên connect mới. State đã dùng không được retry. Phiên hết hạn quá 7 ngày được dọn khi tạo phiên mới.

### Kết nối lại và kiểm thử FE local

- Sau DELETE, giữ account ID và gọi `POST /platform-accounts/{id}/reconnect` để tạo phiên mới; mở `authorizationUrl` vừa nhận. Không cache URL connect/reconnect, mở lại callback cũ hoặc gọi callback thêm lần nữa từ FE. Account Disabled không được khôi phục bằng endpoint connect chung.
- FE `http://localhost:<port>` gọi API HTTPS trên domain server là khác site. `credentials: "include"` (Axios: `withCredentials: true`) là cần thiết nhưng không tự vượt qua chính sách cookie `SameSite=Lax`; cookie proof có thể không được lưu, khiến callback báo `PLATFORM_OAUTH_STATE_INVALID`.
- Để kiểm thử với API đã deploy, phục vụ FE qua HTTPS trên subdomain cùng site với API, đồng thời cấu hình CORS đúng origin và cho phép credentials. Hoặc chạy FE/backend local với callback trỏ đúng backend local và được Meta chấp nhận. Chỉ proxy request connect qua localhost trong khi callback vẫn về domain server không giải quyết được cookie khác host.
- Trong DevTools, kiểm tra response connect/reconnect có `Set-Cookie`, cảnh báo cookie bị chặn, và request callback có cookie `postgen_oauth_*` tương ứng. Không chia sẻ giá trị cookie/code/state. Callback là single-use: refresh trang callback sau một lần xử lý cũng sẽ bị từ chối.
- Kiểm thử live ngày 15/09/2026: từ cùng origin với API, reconnect Page Demo HTM đang Disabled trả callback 200, giữ nguyên account ID và chuyển về Connected. Báo lỗi từ FE local cần kiểm tra Network của FE để phân biệt cookie bị chặn, thiếu credentials và URL/phiên đã dùng.

`Connected` xác nhận thông tin kết nối ở lần kiểm tra gần nhất, **không đồng nghĩa sẵn sàng xuất bản**. Zalo không trả danh sách scopes trong token/getoa nên `grantedScopes` rỗng nghĩa là chưa có dữ liệu scope để liệt kê; không suy ra đã có quyền quản lý bài viết. Mốc 13 vẫn phải kiểm quyền xuất bản thực tế.

## Token và trạng thái

- `Connected`: có credentials mã hóa. `ReauthRequired`: cần chạy OAuth lại. `Disabled`: Admin đã ngắt kết nối tại backend.
- Zalo refresh đổi cặp access/refresh token và lưu cả hai trước mọi lời gọi profile bổ sung. Hạn access lấy từ `expires_in` (chấp nhận số hoặc chuỗi). Hạn refresh không được suy đoán khi response không cung cấp.
- Meta không có refresh_token theo luồng này: endpoint refresh **kiểm tra token Page hiện có** bằng debug_token và xác minh Page/Instagram liên kết; không hứa gia hạn token bằng refresh_token. Token không hợp lệ/thiếu quyền yêu cầu reconnect. Lúc connect, hạn token user dùng làm mốc kiểm tra bảo thủ cho Page; khi debug trả `expires_at = 0`, lưu null thay vì tuyên bố token vĩnh viễn.
- Làm mới **theo yêu cầu qua POST refresh**, chưa có cron tự refresh. Cột expiresAt phục vụ hiển thị và tích hợp worker sau này; GET list không ghi trạng thái hoặc refresh ngầm.
- Mỗi account có lease refresh và Revision. Hai request đồng thời: request sau nhận 409. Không giữ transaction qua HTTP. Disconnect/reconnect đổi revision/lease, kết quả refresh cũ không được ghi đè.
- Lease hết hạn sau crash, hoặc Zalo gặp lỗi/timeout/cancel khi refresh: chuyển ReauthRequired và bỏ credentials có thể đã mất hiệu lực; không gửi lại refresh token một lần. Đây là lựa chọn bảo thủ, có thể cần reconnect cả khi lỗi tạm thời. Meta gặp lỗi tạm thời giữ thông tin cũ và ghi lastErrorCode.
- Refresh có thể trả HTTP 200 nhưng status ReauthRequired hoặc lastErrorCode khác null: FE phải đọc trạng thái, không chỉ dựa vào isSuccess. Lỗi cấu hình/đang xử lý/tài khoản không tồn tại vẫn trả HTTP lỗi.
- DELETE xóa credentials local, giữ account ID và lịch sử tham chiếu; không xóa tài khoản trên nền tảng và không thu hồi quyền ứng dụng Meta cho tất cả Page khác đang dùng chung app. Muốn thu hồi quyền bên provider, Admin thao tác trong quản lý ứng dụng của provider. Nếu có PlatformPost Scheduled/Publishing tham chiếu account thì trả 409, cần hủy lịch/chờ hoàn tất.

## Cấu hình môi trường

Docker/GitHub Actions: xem [hướng dẫn môi trường](../../.docker/README.md), dùng tên `POSTGEN_*` ở secrets và Compose ánh xạ sang options .NET. Cookie OAuth lấy path từ callback công khai, hỗ trợ ingress có tiền tố `/ecommerce-api/...`.

Không seed secret vào Config/appsettings. Mặc định Meta/Zalo Disabled; chỉ bật sau khi cấp cấu hình cho môi trường.

| Tên cấu hình (env dùng `__` thay `:`) | Nội dung |
| --- | --- |
| `PlatformAccountOptions:ActiveEncryptionKeyId` | Tên khóa đang dùng, không chứa dấu chấm |
| `PlatformAccountOptions:EncryptionKeys:<keyId>` | Khóa ngẫu nhiên 32 byte, Base64, dùng AES-256-GCM |
| `PlatformAccountOptions:OAuthStateMinutes` | Mặc định 10, trong 1–30 phút |
| `PlatformAccountOptions:RequestTimeoutSeconds` | Mặc định 30, trong 1–60 giây; tổng một luồng provider tối đa 3 lần giá trị này |
| `PlatformAccountOptions:Meta:Enabled` | Bật cả Facebook và Instagram qua Facebook Login |
| `PlatformAccountOptions:Meta:AppId` / `AppSecret` | Ứng dụng Meta của môi trường |
| `PlatformAccountOptions:Meta:GraphApiVersion` | Bắt buộc pin phiên bản được app hỗ trợ, dạng vN.N; không mặc định phiên bản suy đoán |
| `PlatformAccountOptions:Meta:ConfigId` | ID cấu hình Facebook Login for Business loại user token; khi có thì gửi `config_id` thay `scope`; trống giữ luồng scope cũ |
| `PlatformAccountOptions:Meta:FacebookCallbackUrl` | URL backend cố định kết thúc `/platform-accounts/facebook/callback` |
| `PlatformAccountOptions:Meta:InstagramCallbackUrl` | URL backend cố định kết thúc `/platform-accounts/instagram/callback` |
| `PlatformAccountOptions:Zalo:Enabled` | Bật kết nối Zalo OA |
| `PlatformAccountOptions:Zalo:AppId` / `AppSecret` | Ứng dụng Zalo OA |
| `PlatformAccountOptions:Zalo:CallbackUrl` | URL backend cố định kết thúc `/platform-accounts/zalo/callback` |

Callback trong cấu hình không chứa query/fragment/userinfo, yêu cầu HTTPS ngoài localhost; đăng ký đúng URL/cơ chế callback ở dashboard provider. Không nhận redirect URL tùy ý từ FE. Zalo thêm state vào query callback cho mỗi phiên.

Meta Dashboard → Đăng nhập bằng Facebook cho doanh nghiệp → Cài đặt: bật cả **Đăng nhập OAuth ứng dụng** và **Đăng nhập OAuth trên web**, thêm hai callback vào **URI chuyển hướng OAuth hợp lệ**, rồi Save Changes. Ô **Chuyển hướng URI để kiểm tra** chỉ dùng kiểm tra, không lưu callback. Hai công tắc bị tắt vẫn gây lỗi “URL bị chặn” dù callback đã đăng ký đúng; đây là cài đặt trên Meta, khác với `POSTGEN_META_ENABLED` của backend.

Khi tạo cấu hình cho `POSTGEN_META_CONFIG_ID`, chọn **Mã truy cập dành cho người dùng** để khớp luồng backend đổi user token và lấy Page qua `/me/accounts`. Nếu đã chọn **Mã truy cập dành cho người dùng hệ thống**, tạo cấu hình mới đúng loại và cập nhật ID khi deploy. Trong kiểm thử thực tế ngày 15/09/2026, cấu hình system user chặn chọn business sở hữu app với thông báo “This Meta Business Account owns the app”; không tạo business mới để vượt qua bước này.

Khóa mã hóa phải dùng chung giữa các instance và tồn tại qua deploy/restart. Rotation: giữ các keyId cũ để giải mã, thêm key mới và đổi ActiveEncryptionKeyId; các lần kết nối/refresh mới dùng khóa mới. Chưa có tác vụ re-encrypt hàng loạt, nên không bỏ khóa cũ khi còn account/session dùng nó. Khóa không có fallback hardcode và không dùng JWT signing key.

Named HttpClient `PostGenOAuth` tắt HTTP request logging và tự redirect. Các lỗi không kèm response/token gốc. Không cấu hình reverse proxy/APM ghi query OAuth, Authorization, secret_key, access_token hoặc body đổi token. Giới hạn response provider 1 MiB; phân trang đi theo cursor trên host cố định, không theo URL next từ response. Không tự retry trao đổi/refresh token.

## Lỗi

| HTTP | messageCode | Xử lý |
| --- | --- | --- |
| 400 | PLATFORM_OAUTH_STATE_INVALID | Bắt đầu phiên mới trong đúng trình duyệt |
| 400 | PLATFORM_OAUTH_DENIED | Người dùng cấp quyền lại nếu muốn kết nối |
| 400 | PLATFORM_REAUTH_REQUIRED | Cấp lại quyền/token hợp lệ |
| 400 | PLATFORM_NO_ELIGIBLE_ACCOUNT | Kiểm tra Page/tài khoản chuyên nghiệp và quyền tạo nội dung |
| 400 | PLATFORM_ACCOUNT_MISMATCH | Reconnect đúng account ban đầu |
| 404 | PLATFORM_ACCOUNT_NOT_FOUND | Tải lại danh sách |
| 409 | PLATFORM_ACCOUNT_CHANGED | Có thay đổi đồng thời; tải lại rồi reconnect |
| 409 | PLATFORM_ACCOUNT_DISABLED | Dùng reconnect cho account Disabled |
| 409 | PLATFORM_REFRESH_IN_PROGRESS | Chờ thao tác đang chạy |
| 409 | PLATFORM_REAUTH_REQUIRED | Account không còn Connected; chạy reconnect |
| 409 | PLATFORM_ACCOUNT_IN_USE | Hủy lịch/chờ đăng hoàn tất trước khi disconnect |
| 429 | PLATFORM_RATE_LIMITED | Provider giới hạn yêu cầu |
| 502 | PLATFORM_PROVIDER_UNAVAILABLE / PLATFORM_INVALID_RESPONSE | Kiểm tra provider, tạo phiên mới nếu code đã được dùng |
| 503 | PLATFORM_NOT_CONFIGURED / PLATFORM_ENCRYPTION_NOT_CONFIGURED / PLATFORM_TOKEN_UNAVAILABLE | Kiểm tra app/callback/khóa mã hóa của môi trường |
| 504 | PLATFORM_PROVIDER_TIMEOUT | Không tự gửi lại code hoặc refresh token |

401/403 của API quản trị dùng envelope xác thực chung hiện có. Code/platform đầu vào không hợp lệ trả BAD_REQUEST. Lỗi provider trong refresh được ghi lastErrorCode và trả metadata như mục trạng thái.

## Nguồn contract

- [Meta Instagram API — Facebook Login, Page token và account liên kết](https://www.postman.com/meta/instagram/documentation/6yqw8pt/instagram-api).
- [Meta manual OAuth flow](https://developers.facebook.com/docs/facebook-login/guides/advanced/manual-flow/) và [long-lived token](https://developers.facebook.com/docs/facebook-login/guides/access-tokens/get-long-lived/): truy cập trang bị giới hạn trong lượt kiểm tra này; cần smoke với app thực tế để xác nhận cấu hình/quyền được Meta cấp.
- [Zalo OAuth OA](https://developers.zalo.me/docs/official-account/bat-dau/xac-thuc-va-uy-quyen-cho-ung-dung-new) và [tra cứu thông tin OA](https://developers.zalo.me/docs/official-account/quan-ly/quan-ly-thong-tin-oa/lay-thong-tin-zalo-official-account): đã đọc nội dung bundle chính thức do trang nhúng từ stc-developers.zdn.vn. Token endpoint hiện tại trong tài liệu là `https://oauth.zaloapp.com/v4/oa/access_token`, form-urlencoded và header secret_key; profile dùng `https://openapi.zalo.me/v2.0/oa/getoa`.

Bằng chứng mock/integration không thay cho App Review, quyền cấp thực tế hoặc smoke tài khoản thật. Mốc này không gửi bài hay tin nhắn ra nền tảng.
