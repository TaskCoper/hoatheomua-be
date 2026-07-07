# Thống kê Controllers & phân quyền (Authorization)

> Tài liệu liệt kê toàn bộ endpoint theo từng Controller ở tầng `HoaTheoMua.API/Controllers`.
> Mỗi hàm ghi rõ **method + route + policy (ai được truy cập)**. Hàm nào **không có policy** thì cột "Quyền truy cập" để trống (public — ai cũng gọi được).
> Dùng để thống kê và làm việc với FE.

## Chú thích policy → vai trò

| Policy (trong code) | Ai được truy cập |
|---|---|
| `AdminPolicy` | Role **Admin** |
| `StaffPolicy` | Role **Staff** |
| `UserPolicy` | Role **User** |
| `StaffOrAdminPolicy` | Role **Staff** hoặc **Admin** |
| `UserVerifiedPolicy` | Role **User** + đã xác thực (`IsVerified=true`) |
| `CustomerPolicy` | Role **Customer** |
| `CustomerOrAdminPolicy` | Role **Customer** hoặc **Admin** |
| `[Authorize]` (không kèm policy) | **Default policy**: đã đăng nhập **và** `IsVerified=true` |
| _(để trống)_ | **Public** — không yêu cầu đăng nhập |

> Ghi chú: token do **taskcoper-be** cấp, service này chỉ *validate*. Role `Customer` chỉ dùng ở storefront này.

---

## AddressController
`[Route("api/v1/addresses")]` — **class-level** `[Authorize(Policy = CustomerPolicy)]` → **mọi endpoint yêu cầu Customer**.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/addresses` | GetMyAddresses | Customer |
| GET | `/api/v1/addresses/{id}` | GetAddress | Customer |
| POST | `/api/v1/addresses` | CreateAddress | Customer |
| PUT | `/api/v1/addresses/{id}` | UpdateAddress | Customer |
| PATCH | `/api/v1/addresses/{id}/default` | SetDefaultAddress | Customer |
| DELETE | `/api/v1/addresses/{id}` | DeleteAddress | Customer |

---

## OrderController
Không có route/authorize ở class — route khai báo tường minh trên từng hàm.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/orders` | GetOrderById (phân trang) | Customer |
| POST | `/api/v1/orders` | CreateOrder | Customer |
| PATCH | `/api/v1/orders/{id}/cancel` | CancelOrder | Customer |
| GET | `/api/v1/orders/{id}` | GetOrderById | Customer |
| GET | `/api/v1/orders/{id}/status` | GetStatusOrder | Customer |
| POST | `/api/v1/orders/nhanh-vn` | GetNhanhVnOrders | Admin |
| POST | `/api/v1/orders/shipping-location` | GetShippingLocation | Customer hoặc Admin |

---

## CompanyAddressController
`[Route("api/v1/company-addresses")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/company-addresses` | GetCompanyAddress | |
| GET | `/api/v1/company-addresses/{id}` | GetCompanyAddressById | |
| POST | `/api/v1/company-addresses` | CreateCompanyAddress | _(authorize Admin đang bị comment)_ |
| PUT | `/api/v1/company-addresses/{id}` | UpdateCompanyAddress | Admin |
| DELETE | `/api/v1/company-addresses/{id}` | DeleteCompanyAddress | Admin |

---

## RecruitmentRequestsController
`[Route("api/v1/recruitment-requests")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/recruitment-requests` | GetRecruitmentRequests | Admin |
| GET | `/api/v1/recruitment-requests/me` | GetMyRecruitmentRequests | Đã đăng nhập + IsVerified (default) |
| GET | `/api/v1/recruitment-requests/{id}` | GetRecruitmentRequestById | Admin |
| PATCH | `/api/v1/recruitment-requests/{id}/read` | MarkAsRead | Admin |
| POST | `/api/v1/recruitment-requests` | CreateRecruitmentRequest | Đã đăng nhập + IsVerified (default) |

---

## JodDescriptionsController
`[Route("api/v1/job-descriptions")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/job-descriptions` | CreateJobDescription | Admin |
| GET | `/api/v1/job-descriptions` | GetJobDescription | Admin |
| GET | `/api/v1/job-descriptions/highest-salary` | GetHighestSalary | _(authorize Admin đang bị comment)_ |
| GET | `/api/v1/job-descriptions/{key}` | GetJobDescriptionByIdOrSlug | Admin |
| PUT | `/api/v1/job-descriptions/{id}` | UpdateJobDescription | Admin |
| DELETE | `/api/v1/job-descriptions/{id}` | DeleteJobDescription | Admin |
| GET | `/api/v1/job-descriptions/locations` | GetAllLocation | |
| GET | `/api/v1/public/job-descriptions` | GetPublicJobDescriptions | |
| GET | `/api/v1/public/job-descriptions/{key}` | GetPublicJobDescriptionByIdOrSlug | |

---

## ConfigController
`[Route("api/v1/configs")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/configs` | GetConfigList | Admin |
| GET | `/api/v1/configs/content` | GetConfigContentList | Admin |
| GET | `/api/v1/configs/settings` | GetConfigSettingList | Admin |
| GET | `/api/v1/configs/secrets` | GetConfigSecretList | Admin |
| POST | `/api/v1/configs/secrets/reveal/id` | GetConfigReveal | Admin |
| GET | `/api/v1/configs/{id}` | GetConfig | Admin |
| POST | `/api/v1/configs` | CreateConfig | Admin |
| PUT | `/api/v1/configs/{id}` | UpdateConfig | Admin |
| DELETE | `/api/v1/configs/{id}` | DeleteConfig | Admin |
| GET | `/api/v1/configs/public` | GetPublicConfig | |
| GET | `/api/v1/configs/group` | GetConfigGroups | Admin |
| GET | `/api/v1/configs/order-tags` | GetNhanhVnOrderTag | Customer hoặc Admin |

---

## CategoryNewController
`[Route("/api/v1/category-news")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/category-news` | CreateCate | Admin |
| GET | `/api/v1/category-news` | GetCategories | |
| GET | `/api/v1/category-news/{id}` | GetCategory | |
| PUT | `/api/v1/category-news/{id}` | UpdateCate | Admin |
| DELETE | `/api/v1/category-news/{id}` | DeleteCate | Admin |

---

## NewController
`[Route("api/v1/news")]` — không authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| GET | `/api/v1/news` | GetNews | |
| GET | `/api/v1/news/{key}` | GetNewById | |
| POST | `/api/v1/news` | CreateNew | Admin |
| PUT | `/api/v1/news/{id}` | UpDateNew | Admin |
| DELETE | `/api/v1/news/{id}` | DeleteNew | Admin |

---

## ProductController
Không có route/authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/products/nhanh-vn/list` | SearchNhanhVnProducts (list) | |
| POST | `/api/v1/products/nhanh-vn/details` | SearchNhanhVnProducts (details) | |

---

## AhamoveService (Controller)
Không có route/authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/ahamove/estimate` | EstimateShippingFee | |
| GET | `/api/v1/ahamove/service-details` | GetNhanhVnOrders | |

---

## NhanhVnController (file `NhanhVnService.cs`)
Không có route/authorize ở class.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/nhanh-vn/orders-tags` | GetOrderLabel | |
| POST | `/api/v1/nhanh-vn/orders-sources` | GetOrderSource | |
| POST | `/api/v1/nhanh-vn/depot` | GetNhanhVnDepot | |

---

## SepayController
Không có route/authorize ở class — webhook nhận từ Sepay.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/sepay` | SepayWebhookHandler | |

---

## TestController
`[Route("api/[controller]")]` → prefix `api/Test`. Không có authorization — chỉ gắn **rate limiting** (không phải phân quyền).

| Method | Route | Hàm | Quyền truy cập | Rate limit |
|---|---|---|---|---|
| GET | `/api/Test/normal` | TestNormalApi | | `api` (100 req/phút) |
| GET | `/api/Test/login` | TestLoginApi | | `login` (5 req/phút) |
| GET | `/api/Test/heavy` | TestHeavyApi | | `heavy` (token bucket) |
| GET | `/api/Test/bypass` | TestBypassApi | | _(bỏ qua rate limit)_ |

---

## ContactController
**Toàn bộ file đang bị comment** — chưa có endpoint nào hoạt động. Ghi lại để theo dõi.

| Method | Route | Hàm | Quyền truy cập |
|---|---|---|---|
| POST | `/api/v1/contact` | SendContact | _(đang bị comment — chưa bật)_ |

---

## Tổng kết nhanh

- **Chỉ Admin**: hầu hết endpoint quản trị của `Config`, `CategoryNew`, `New`, `JobDescriptions` (trừ các route `public`/`locations`/`highest-salary`), `CompanyAddress` (PUT/DELETE), Recruitment (GET danh sách / {id} / read), `Order` nhanh-vn.
- **Customer**: toàn bộ `Address`, phần lớn `Order`.
- **Customer hoặc Admin**: `Order` shipping-location, `Config` order-tags.
- **Đã đăng nhập + IsVerified**: Recruitment `me` và `POST` (tạo đơn ứng tuyển).
- **Public (để trống)**: các API tra cứu/tích hợp ngoài — `Product`, `Ahamove`, `NhanhVn`, `Sepay` webhook, `Test`, các route `public` của Config/JobDescriptions, GET danh sách News & CategoryNews.
- **Cần rà soát** (authorize đang bị comment): `CompanyAddress` POST, `JobDescriptions` GET highest-salary, toàn bộ `Contact`.
