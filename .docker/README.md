# Cấu hình PostGen trong Docker và GitHub Actions

Phạm vi hiện tại: kết nối Facebook Page và Instagram qua Facebook Login; Zalo tạm hoãn. Đây là cấu hình account/OAuth mốc 11, chưa bật API đăng bài/lập lịch.

`.env.sample` dùng tên `POSTGEN_*`. Compose ánh xạ sang `PlatformAccountOptions__*` trong container. Không dùng tên .NET `PlatformAccountOptions__*` làm tên GitHub Secret trong workflow này.

## Các biến cần điền

Tạo GitHub **Environment Secrets** trong environment `dev` hoặc `prod` (Settings → Environments → chọn environment → Environment secrets). Theo cơ chế hiện có, cả cấu hình thường và giá trị bí mật đều dùng secrets; workflow build không nhận các giá trị này.

| Secret / biến `.env` | Giá trị |
| --- | --- |
| `POSTGEN_META_ENABLED` | `true` khi đã cấu hình xong; thiếu/rỗng mặc định `false` |
| `POSTGEN_META_APP_ID` | App ID; app đang kiểm tra: `1076807665108799` |
| `POSTGEN_META_APP_SECRET` | App Secret của cùng app, nhập trực tiếp vào secrets |
| `POSTGEN_META_GRAPH_API_VERSION` | Phiên bản Graph API được app hỗ trợ, dạng `vN.N`; cần xác nhận, không có mặc định |
| `POSTGEN_META_CONFIG_ID` | Facebook Login for Business configuration ID, phải chọn loại **Mã truy cập dành cho người dùng** (user access token); kiểm tra loại thực tế trong chi tiết cấu hình, không suy ra từ tên |
| `POSTGEN_META_FACEBOOK_CALLBACK_URL` | URL callback Facebook công khai, khớp chính xác URL đã lưu trên Meta |
| `POSTGEN_META_INSTAGRAM_CALLBACK_URL` | URL callback Instagram công khai, khớp chính xác URL đã lưu trên Meta |
| `POSTGEN_ACTIVE_ENCRYPTION_KEY_ID` | `k1` (mặc định) hoặc `k2` khi xoay khóa |
| `POSTGEN_ENCRYPTION_KEY_K1` | Khóa ngẫu nhiên 32 byte mã hóa Base64; bắt buộc khi active ID là `k1` |
| `POSTGEN_ENCRYPTION_KEY_K2` | Khóa thứ hai, tùy chọn; bắt buộc khi active ID là `k2` |
| `POSTGEN_OAUTH_STATE_MINUTES` | Tùy chọn, mặc định `10`, khoảng 1–30 |
| `POSTGEN_REQUEST_TIMEOUT_SECONDS` | Tùy chọn, mặc định `30`, khoảng 1–60 |

Hai callback người dùng đã nhập trên Meta:

```text
https://hoatheomua-api.bentran1vn.com/ecommerce-api/v1/post-gen/platform-accounts/facebook/callback
https://hoatheomua-api.bentran1vn.com/ecommerce-api/v1/post-gen/platform-accounts/instagram/callback
```

Ingress phải ánh xạ `/ecommerce-api/v1/...` công khai tới route `/api/v1/...` của backend. Repo không cấu hình phép rewrite này; cần kiểm tra ingress khi kết nối thật. Cookie OAuth dùng path của callback công khai và Secure khi callback là HTTPS. FE/API cần cùng site để cookie SameSite=Lax hoạt động cho lời gọi connect/reconnect.

`POSTGEN_META_CONFIG_ID` được chuyển thành `config_id` trong URL đăng nhập, không gửi `scope` cùng lúc. Chỉ để trống khi dùng luồng Facebook Login cũ lấy quyền qua `scope`. Cấu hình dùng chung Facebook/Instagram cần đủ `pages_show_list`, `pages_read_engagement`, `pages_manage_posts`, `instagram_basic`, `instagram_content_publish`; backend vẫn kiểm quyền đã cấp sau khi đổi code. Không nhập Page ID, Instagram ID hoặc access token vào biến môi trường: backend lấy từ OAuth.

## Khóa mã hóa và triển khai

Tạo khóa **một lần** trên máy quản trị:

```bash
openssl rand -base64 32
```

Lưu kết quả vào secret khóa tương ứng, giữ cùng giá trị giữa các instance và qua restart/deploy. Khi xoay khóa: thêm K2, giữ nguyên K1, đổi active ID sang `k2`. Không ghi đè/xóa K1 khi còn token/session được mã hóa bằng K1. Muốn thêm slot khóa ngoài k1/k2 cần mở rộng Compose, workflow và validator cùng nhau.

Có thể điền file local `.docker/.env.dev` hoặc `.docker/.env.prod` từ `.env.sample` rồi dùng script có sẵn để đồng bộ GitHub Environment Secrets:

```bash
ENVIRONMENTS=dev ENV_FILE=.docker/.env.dev bash .docker/sync-env-secrets.sh
```

Lệnh trên ghi secrets lên GitHub; thay `dev` bằng `prod` cho môi trường production. Không commit file chứa giá trị thật. Script đồng bộ đã hỗ trợ mọi dòng KEY=VALUE nên không cần sửa danh sách khóa trong script.

Workflow deploy chạy `validate-postgen-env.sh` trước khi pull/deploy: Meta tắt thì không bắt cung cấp secrets; Meta bật thì kiểm các trường bắt buộc, callback HTTPS, key slot/định dạng khóa và giới hạn thời gian. Script nhận biến đã export, không tự đọc/source file `.env` và không in giá trị secrets.

- CI/CD dùng `.docker/compose.yaml` với profile dev/prod.
- Chạy local dùng `.docker/docker-compose.yml`; `compose.dev.yml` / `compose.prod.yml` kế thừa các biến PostGen từ base, không khai báo lặp.
- `PlatformAccountOptions__Zalo__Enabled` được giữ `false` trong cả hai Compose base.
- Thay đổi env cần tạo lại container/deploy; chỉ restart container cũ không cập nhật env.
- Startup tự áp dụng migration mốc 11. Nếu dữ liệu cũ có `PlatformPost.PlatformAccountId` chưa được đối soát thì migration chủ động dừng; không tự xóa các giá trị này.

Việc chỉnh file không tạo GitHub Secrets hoặc tự triển khai. Sau khi điền secrets và deploy code mốc 11, thử connect/callback bằng tài khoản Admin trong cùng trình duyệt; kiểm tra backend trả account đúng trước khi làm mốc đăng bài.


### PostGen publishing — mốc 13

`POSTGEN_PUBLISH_FACEBOOK_ENABLED` và `POSTGEN_PUBLISH_INSTAGRAM_ENABLED` được ánh xạ sang `PublishingOptions` trong cả hai compose/workflow, mặc định `false`. `POSTGEN_META_ENABLED` chỉ bật kết nối tài khoản. Bật từng flag sau khi cấu hình mốc 11 và smoke được chỉ định trên account/bài thử phù hợp; không bật chung với OAuth một cách tự động. Chưa thay secret hoặc deploy trong lần triển khai mốc 13. Xem [PublishingAdapters.md](../hoatheomua-documentation/Service%20-%20PostGen/PublishingAdapters.md).


### PostGen scheduling — mốc 14

Ba biến tùy chọn: `POSTGEN_SCHEDULE_MIN_LEAD_SECONDS=60`, `POSTGEN_SCHEDULE_MAX_DAYS=30`, `POSTGEN_SCHEDULE_LATENESS_SECONDS=900`; đã ánh xạ trong compose/workflow. Ngưỡng trễ tính đến lần worker bắt đầu xử lý, không phải cam kết thời điểm Meta hiển thị bài. Không đổi secrets/môi trường đang chạy. [Contract lịch đăng](../hoatheomua-documentation/Service%20-%20PostGen/Scheduling.md).
