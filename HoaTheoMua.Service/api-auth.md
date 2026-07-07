# Auth API — Endpoint nào cần xác thực, endpoint nào public

Tài liệu này liệt kê **toàn bộ endpoint backend mà frontend `hoatheomua-fe` đang gọi**, kèm yêu cầu xác thực cho từng endpoint. Dùng để backend team implement guard middleware server-side.

> **Nguyên tắc:** FE có thể bỏ qua bearer header vì đang dùng cookie HttpOnly, nhưng backend **không được tin FE**. Mọi endpoint đánh dấu `Auth: Có` phải bị chặn ở server nếu request không kèm access token hợp lệ.

---

## 1. Tổng quan

FE dùng **hai axios client** (`lib/axios.ts`) trỏ đến hai base URL khác nhau:

| Client | Env var | Dùng cho |
|---|---|---|
| `api` | `NEXT_PUBLIC_API_BASE_URL` | Product, article, order, address, config, contact, company |
| `apiTaskcoper` | `NEXT_PUBLIC_TASKCOPER_API_BASE_URL` | User / auth, presigned URL upload |

Tất cả request đi qua **same-origin proxy** tại `app/api/[...path]/route.ts` — proxy này strip trường `Domain` của cookie trước khi forward request lên upstream để cookie HttpOnly hoạt động cross-domain.

---

## 2. Cơ chế xác thực

- **Access token & refresh token**: cookie HttpOnly, backend set sau khi login / verify email.
- **Refresh flow** (`lib/axios.ts:240-261`):
  1. Bất kỳ response nào trả về `401` với body chứa `code` là `ExpiredAccessToken` hoặc `InvalidAccessToken` sẽ trigger refresh.
  2. FE gọi `POST /v1/users/refresh_token` (Taskcoper) — dựa vào refresh token trong cookie.
  3. Nếu refresh thành công, FE retry request gốc **một lần**. Nếu refresh fail, FE redirect về `/sign-in` (trừ khi đang ở public route).
- **Backend cần trả `code` rõ ràng ở body 401** để interceptor phân biệt được token hết hạn vs token sai — nếu thiếu, FE sẽ không retry và user bị đá về sign-in oan.
- **Route UI**:
  - Guest-only (redirect nếu đã đăng nhập): `/sign-in`, `/sign-up`, `/forgot-password`.
  - Protected (redirect về `/sign-in` nếu chưa đăng nhập): `/cart`, `/checkout`, `/orders/*`, `/users/*`.
  - Còn lại là public (landing, product listing, article, about, contact, terms, privacy, …).
- **Chưa có role model** — chưa có `/admin/*` route và không có phân quyền theo role. Nếu backend thêm role sau, cần sync lại với FE.

---

## 3. Bảng endpoint theo feature

Ký hiệu:
- **Auth = Không**: public, không yêu cầu token.
- **Auth = Có**: bắt buộc access token hợp lệ.
- **Auth = Cookie refresh**: chỉ dùng refresh token trong cookie HttpOnly, không cần access token.

### 3.1. User / Auth (`apiTaskcoper` — base `v1/users/`)

File FE: `features/users/services.ts`, refresh interceptor tại `lib/axios.ts`.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| POST | `/v1/users/register` | Không | Đăng ký tài khoản mới |
| POST | `/v1/users/login` | Không | Đăng nhập, set access + refresh cookie |
| POST | `/v1/users/verify_account` | Không | Xác thực email bằng mã 6 ký tự |
| POST | `/v1/users/resend_verify_account_code?email={email}` | Không | Gửi lại mã xác thực email |
| POST | `/v1/users/forgot_password` | Không | Yêu cầu reset mật khẩu qua email |
| POST | `/v1/users/verify_change_password_code` | Không | Xác thực mã reset mật khẩu |
| POST | `/v1/users/refresh_token` | Cookie refresh | Silent refresh access token, dựa vào refresh token cookie |
| POST | `/v1/users/logout` | Có | Đăng xuất, clear cookie |
| POST | `/v1/users/change_password` | Có | Đổi mật khẩu (cần current password) |
| GET | `/v1/users/me` | Có | Lấy profile user hiện tại |
| PUT | `/v1/users/me` | Có | Cập nhật profile user hiện tại |
| GET | `/v1/users/{slug}` | Có | Lấy profile user theo slug |

### 3.2. Products (`api` — base `v1/products/nhanh-vn/`)

File FE: `features/products/services.ts`. Cả hai endpoint đều nhận header `Accept-Language` để phân biệt locale.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| POST | `/v1/products/nhanh-vn/list` | Không | Danh sách sản phẩm với filter + paginator |
| POST | `/v1/products/nhanh-vn/details` | Không | Chi tiết sản phẩm theo `filters.id` |

### 3.3. Articles / News (`api` — base `v1/news/`)

File FE: `features/articles/services.ts`.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/news?pageIndex=&pageSize=` | Không | Danh sách bài viết (paginated) |
| GET | `/v1/news/{identifier}` | Không | Chi tiết bài viết theo slug/id |

### 3.4. Orders (`api` — base `v1/orders/`)

File FE: `features/orders/services.ts`. Tất cả yêu cầu user đã đăng nhập.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/orders` | Có | Danh sách đơn hàng của user |
| GET | `/v1/orders/{orderId}` | Có | Chi tiết đơn hàng |
| GET | `/v1/orders/{orderId}/status` | Có | Trạng thái đơn hàng |
| POST | `/v1/orders` | Có | Tạo đơn hàng mới (checkout) |
| POST | `/v1/orders/shipping-location` | Có | Danh sách tỉnh / huyện / xã cho địa chỉ giao hàng |
| PATCH | `/v1/orders/{id}/cancel` | Có | Hủy đơn hàng |
| POST | `/v1/ahamove/estimate` | Có | Ước lượng phí ship + thời gian giao qua Ahamove |

### 3.5. Addresses (`api` — base `v1/addresses/`)

File FE: `features/addresses/services.ts`. Tất cả yêu cầu user đã đăng nhập; backend phải scope theo user id trong token — **không được cho phép query address của user khác**.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/addresses` | Có | Danh sách địa chỉ của user |
| GET | `/v1/addresses/{id}` | Có | Chi tiết địa chỉ |
| POST | `/v1/addresses` | Có | Tạo địa chỉ mới |
| PUT | `/v1/addresses/{id}` | Có | Cập nhật địa chỉ |
| DELETE | `/v1/addresses/{id}` | Có | Xóa địa chỉ |
| PATCH | `/v1/addresses/{id}/default` | Có | Đặt làm địa chỉ mặc định |

### 3.6. Configs (`api` — base `v1/configs/`)

File FE: `features/config/config.service.ts`. Dùng cho hotline, email, background music, occasion collection, featured product, mainline, order status tags…

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/configs/public?groupName={group}&key={key}` | Không | Lấy config public (company info, UI setting) |
| GET | `/v1/configs/order-tags` | Không | Lấy danh sách tag trạng thái đơn hàng cho UI |

### 3.7. Company (`api` — base `v1/company-addresses/`)

File FE: `features/company/company-address.service.ts`.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/company-addresses` | Không | Danh sách văn phòng / chi nhánh công ty |

### 3.8. Contact (`api` — base `v1/contact/`)

File FE: `features/contact/services.ts`.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| POST | `/v1/contact` | Không | Gửi form liên hệ từ trang public |

Backend nên rate-limit endpoint này (spam form).

### 3.9. Storage / Upload (`apiTaskcoper` — base `v1/storages/`)

File FE: `features/upload/hooks/use-get-presigned-url.ts`, `use-upload-file.ts`.

| Method | Path | Auth | Mục đích |
|---|---|---|---|
| GET | `/v1/storages/presigned-url?key={key}&contentType={type}` | Có | Lấy presigned URL để upload trực tiếp lên S3 |
| PUT / POST | `{presignedUrl}` (S3 trực tiếp) | Không (đã sign) | Upload file lên S3 |

**Yêu cầu backend**: khi tạo presigned URL, phải prefix `key` theo user id (vd. `user/{userId}/…`) để user không thể ghi đè file của user khác bằng cách gửi `key` tùy ý.

### 3.10. External (không phải backend team quản lý)

Ghi chú để đầy đủ — FE gọi thẳng, backend không cần xử lý:

| Service | Endpoint | Auth | Mục đích |
|---|---|---|---|
| Goong Maps | `GET https://rsapi.goong.io/geocode` | API key qua env | Reverse geocode địa chỉ → lat/lng |
| VietQR | `GET https://api.vietqr.io/v2/business/{taxCode}` | Không | Tra cứu tên & địa chỉ doanh nghiệp theo mã số thuế |

---

## 4. Checklist cho backend

- [ ] Middleware xác thực áp dụng cho toàn bộ endpoint đánh `Auth: Có`.
- [ ] 401 response luôn kèm `code` = `ExpiredAccessToken` hoặc `InvalidAccessToken` để FE refresh đúng cách.
- [ ] `refresh_token` chỉ tin refresh token cookie, không tin access token đã hết hạn.
- [ ] Address, Order, Storage: scope resource theo user id trong token, không cho query cross-user.
- [ ] Contact và các endpoint đăng ký / forgot-password: có rate-limit theo IP hoặc email.
- [ ] Presigned URL: server tự tạo `key` scope theo user, không nhận `key` từ client.

---

## 5. Tổng kết

- **Tổng endpoint FE gọi**: 34 (không tính S3 upload và external service).
- **Public**: 14 (auth flow trước login, product list/detail, article, config, company, contact).
- **Cần auth**: 20 (user profile, order, address, upload, một số auth endpoint sau khi login).
- **Cookie-refresh only**: 1 (`refresh_token`).
