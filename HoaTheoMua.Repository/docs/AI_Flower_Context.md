# AI Flower Context - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize  
> Date: 2026-08-27  
> Last Updated: 2026-09-04

---

## 0. DECISION RECORD — 2026-08-28

| Quyết định | Trạng thái |
|---|---|
| Generate validate live đúng một lần trước AI, sau đó dùng immutable snapshot cho lần đầu và tối đa hai retry | Chốt |
| Admin đổi active/delete/stock/config sau snapshot không ảnh hưởng request đang chạy | Chốt |
| Generated Flower lịch sử vẫn tạo Card; Order mới validate Product gốc live | Chốt |
| Generated Flower legacy đủ ảnh/ownership nhưng thiếu provenance vẫn tạo Card, ghi unavailable; Order chặn 409 | Chốt — phương án A |
| Flower/Card history đều là record trong `client_histories`, không có bảng history riêng | Chốt |
| API Generate với Product có variants phải nhận đúng child/variant ID | Chốt |
| Parent inactive chỉ chặn ở Order sau khi child active; không dùng parent để chặn Generate | Chốt |
| Config cho phép nhiều key trong cùng group; tối đa một record active/chưa xóa cho exact `(group,key)` | Chốt |
| Snapshot lưu ảnh version/hash hoặc bytes và full system prompt; thêm `generated_flowers.input_snapshot` | Chốt |
| Regenerate từ Flower history giữ toàn bộ Product/user input nguồn; chỉ Mockup được phép chọn lại | Chốt |
| Regenerate bỏ trống/null/đúng ID Mockup nguồn dùng snapshot cũ, không kiểm tra live; chỉ ID khác nguồn được validate live | Chốt |
| Regenerate ưu tiên prompt hiện hành hợp lệ, nếu thiếu/rỗng/invalid thì fallback full prompt snapshot nguồn | Chốt |
| Order cần quantity live đủ tổng lượng đã cộng gộp và vẫn kiểm kho Nhanh.vn | Chốt, context-only |
| Contract truyền Card AI vào Order và trạng thái payment webhook khi hết hàng | Chưa chốt — phải hỏi lại |

Không thêm bảng mới và không sửa code C# trong phạm vi cập nhật document-first này.

---

## 1. TỔNG QUAN

### Mô tả

Hệ thống tạo mẫu hoa AI cho phép khách hàng tạo ảnh mẫu hoa từ Combo, Mockup và thông tin đã nhập. STORY-030 và STORY-033 cùng mô tả một lần gọi API tạo hoa; không có bước lưu yêu cầu nháp hay tạo AI Job riêng.

### Luồng chính

```text
Combo Card + Mockup + thông tin người dùng nhập
                    |
                    v
        POST /api/ai-flowers (STORY-030 + STORY-033)
                    |
                    v
Validate Product + Mockup + prompt + quota -> đóng băng input snapshot
                    |
                    v
  AI / tối đa 2 retry dùng cùng snapshot, không revalidate nguồn
                    |
                    v
  Thành công: generated_flowers + client_histories -> Card/Regenerate được phép
                    |
                    v
  Checkout/Order resolve Product gốc và kiểm tra dữ liệu live
  Thất bại: trả lỗi, không lưu request/job/history
```

### Stories liên quan

| Story | Vai trò trong flow chung | Sprint | Priority | Status |
|---|---|---:|---|---|
| **STORY-030** | Nhận dữ liệu để tạo mẫu hoa | 1 | Must | Draft |
| **STORY-033** | Gọi AI và trả mẫu hoa đã tạo | 1 | Must | Draft |
| **STORY-038** | Khởi tạo Checkout từ mẫu hoa | 1 | Must | Draft |
| **STORY-039** | Hoàn tất Checkout, tạo và thanh toán Order | 1 | Must | Draft |

---

## 2. LUỒNG NGHIỆP VỤ

### STORY-030 và STORY-033: Tạo mẫu hoa AI

#### Preconditions

- Khách hàng đã đăng nhập và tài khoản đang hoạt động.
- `product_id` là đúng ID Product/biến thể được chọn; Product tồn tại, chưa xóa, active và `sellableQuantity > 0`.
- Mockup được chọn có `is_active = true` và `is_deleted = false`.
- Khách hàng còn quota tạo hoa trong ngày.

#### Trigger

Khách hàng chọn Combo Card, chọn Mockup, nhập thông tin trên form và nhấn tạo mẫu hoa.

#### Input và xử lý

- API nhận toàn bộ dữ liệu form cùng `product_id` chính xác của Combo/biến thể và `mockup_id`.
- Service load dữ liệu live theo thứ tự Product → Mockup → system prompt → quota, sau đó tạo `FlowerGenerationSnapshot` bất biến trước lần gọi AI đầu tiên.
- Mỗi `system_prompts.type` có đúng một prompt hiện hành. Create yêu cầu prompt type `flower` tồn tại và content hợp lệ; Create không có source snapshot để fallback.
- Snapshot giữ dữ liệu Product/biến thể, Mockup, bytes hoặc object version/hash của ảnh và toàn bộ nội dung system prompt. Dữ liệu đó được truyền cho AI Module; không tạo `flower_request` hay `flower_ai_job`.
- Một lần gọi ban đầu và tối đa hai retry đều dùng cùng snapshot; không tải lại ảnh hoặc query lại trạng thái khả dụng của nguồn.
- API trả ảnh tạo thành công trong cùng flow. Khi tạo lại từ lịch sử, client gọi `POST /api/ai-flowers/{source_flower_id}/regenerate`, chỉ được chọn lại Mockup; không có endpoint theo `flower_request_id`.

#### Lưu kết quả

- Chỉ khi AI trả ảnh hợp lệ, tạo một dòng `generated_flowers` và một dòng `client_histories` có `type = "flower"`.
- `client_histories.input` lưu JSON request gốc của lần thành công để phục vụ history; không phải trạng thái tạm của yêu cầu tạo hoa.
- `client_histories.base_id = products.id`; `output_id = generated_flowers.id`.
- `generated_flowers.input_snapshot` và `client_histories.metadata` lưu snapshot Product, Mockup, ảnh, full system prompt và provenance đã dùng.
- Khi AI thất bại sau retry, không tạo `generated_flowers`, `client_histories`, `flower_requests` hoặc `flower_ai_jobs`.

### Tạo lại mẫu hoa từ lịch sử

- Flower nguồn phải thuộc user hiện tại và có `client_histories(type="flower")`, `base_id` Product gốc cùng snapshot đủ dùng. Thiếu lineage/Product provenance hoặc snapshot bắt buộc trả `FLOWER_SOURCE_HISTORY_INVALID/409`.
- Không dùng ảnh kết quả `image_url` của Flower nguồn làm input AI. Hệ thống dựng input mới từ Product/biến thể snapshot, ảnh Product đã đóng băng, toàn bộ user input nguồn và Effective Mockup.
- Client chỉ được truyền `mockup_id` tùy chọn; mọi field còn lại không được phép thay đổi. Bỏ trống/null hoặc truyền đúng ID Mockup nguồn thì dùng nguyên Mockup snapshot nguồn, không query live và không kiểm tra active/soft-delete. Chỉ ID khác nguồn mới được validate theo not-found → deleted → inactive.
- Mỗi `system_prompts.type` có đúng một prompt hiện hành. Regenerate ưu tiên prompt hiện hành type `flower`; nếu record không tồn tại hoặc content rỗng/không hợp lệ thì dùng full prompt snapshot nguồn, không kiểm tra trạng thái record nguồn.
- Logo dùng cấu hình hiện hành. `generated_flowers.input_snapshot` được dựng mới từ dữ liệu nguồn + Effective Mockup + prompt thực tế + logo/config hiện hành.
- History mới giữ `base_id` của history nguồn, đặt `output_id` bằng ID Flower mới và `metadata.regenerated_from_flower_id` bằng ID Flower trực tiếp được chọn. Chuỗi F1 → F2 → F3 vì vậy có cùng Product `base_id`, còn quan hệ trực tiếp lần lượt là F2→F1 và F3→F2.
- `client_histories.input` chỉ ghi hành động thực tế `{operation,source_flower_id,mockup_id}`; metadata giữ snapshot/provenance thực tế và `system_prompt_source=current|source_flower_fallback`.

### STORY-038: Khởi tạo Checkout

- Checkout là phiên tạm, được mở từ mẫu hoa đã tạo hoặc sản phẩm thường.
- Refresh giữ Checkout; rời luồng, đóng tab, đăng xuất hoặc crash thì Checkout bị hủy.
- Tối đa một thiệp được chọn trong Checkout.

### STORY-039: Hoàn tất Checkout & Order

Phạm vi lượt document-first này chỉ ghi nhận nguyên tắc Order, không tạo TDD Order và không sửa code. Runtime hiện có `POST /api/v1/orders`; thanh toán hoàn tất qua webhook SePay/Bảo Kim, chưa có `/pay`, Checkout endpoint, inventory reservation hoặc cơ chế tự hết hạn PENDING sau 24 giờ.

---

## 3. DATABASE SCHEMA

### 3.1 Enums

```sql
Enum system_prompt_types {
  flower
  card
  post
}
```

### 3.2 Tables liên quan

#### products

```sql
Table products {
  id uuid [pk]
  is_active boolean
  is_deleted boolean
  -- sellableQuantity là derived value từ CalculateAvailableQty, không phải column
  -- Các field khác thuộc Core Database
}
```

#### mockup

Mockup là nguồn tham chiếu cho kiểu dáng, bố cục. Cấu trúc và quy tắc quản lý được giữ theo AI Mockup Context.

```sql
Table mockup {
  id uuid [pk]
  name varchar
  description text
  image_url varchar
  is_active boolean
  is_deleted boolean
  created_at timestamp
  updated_at timestamp
}
```

#### system_prompts

```sql
Table system_prompts {
  id uuid [pk]
  type system_prompt_types [unique] -- mỗi type có đúng một prompt hiện hành
  content text
}
```

#### generated_flowers

Chỉ lưu kết quả AI thành công; không có request/job trung gian. `input_snapshot` là dữ liệu bất biến đã validate và thực tế được gửi cho AI/retry.

```sql
Table generated_flowers {
  id uuid [pk]
  image_url varchar
  user_id uuid
  input_snapshot json
  created_at timestamp
}
```

#### client_histories

```sql
Table client_histories {
  id uuid [pk]
  user_id uuid
  input text                    -- JSON request tạo mới hoặc hành động regenerate thực tế
  type varchar                  -- flower | card | handmade_card | post
  system_prompt_id uuid [null]  -- null với handmade_card
  metadata json                 -- Snapshot/provenance Product, Mockup, ảnh và full prompt
  created_at timestamp

  -- Polymorphic references:
  -- type = flower: base_id -> products.id, output_id -> generated_flowers.id
  -- type = card|handmade_card: base_id -> products.id hoặc generated_flowers.id,
  --                            output_id -> generated_cards.id
  base_id uuid
  output_id uuid
}
```

### 3.3 Bảng không tồn tại trong flow này

`flower_requests` và `flower_ai_jobs` không thuộc schema. Chúng không cần thiết vì API không có trạng thái request nháp, queue hay job cần lưu lâu dài.

### 3.4 Quan hệ và JSON

```text
products.id --(base_id)--> client_histories(type=flower)
client_histories.output_id --> generated_flowers.id
client_histories.system_prompt_id --> system_prompts.id
```

`generated_flowers.input_snapshot` và `client_histories.metadata` khi tạo mới hoặc tạo lại hoa dùng `schema_version = 2`:

```json
{
  "schema_version": 2,
  "source_product_id": "uuid-selected-variant",
  "product": {
    "name": "Combo A - biến thể đỏ",
    "attributes": {},
    "image": { "url": "https://...", "object_key": "products/a.jpg", "version_id": "v15", "sha256": "..." },
    "observed_status": { "is_active": true, "is_deleted": false, "sellable_quantity": 3 }
  },
  "user_input": {
    "name": "Bó hoa sinh nhật mẹ",
    "occasion": "sinh_nhat",
    "style": "dang_cap",
    "budget": 500000,
    "note": "Ưa thích hoa hồng và hoa lan"
  },
  "mockup": {
    "id": "uuid",
    "name": "Tên mockup",
    "metadata": {},
    "image": { "url": "https://...", "object_key": "mockups/m.jpg", "version_id": "v8", "sha256": "..." },
    "observed_status": { "is_active": true, "is_deleted": false }
  },
  "system_prompt": { "id": "uuid", "content": "Full prompt đã dùng", "sha256": "..." },
  "operation": "create-or-regenerate",
  "regenerated_from_flower_id": "uuid-or-null",
  "system_prompt_source": "current-or-source_flower_fallback",
  "validated_at": "2026-08-28T08:00:00Z"
}
```

---

## 4. QUY TẮC NGHIỆP VỤ

### 4.1 Nguồn tạo hoa

- `base_id` là `products.id` chính xác của Combo/biến thể user chọn. Nếu Product cha có biến thể, API không chấp nhận ID cha thay cho ID biến thể.
- Product được kiểm tra theo thứ tự cố định: không có → `PRODUCT_NOT_FOUND/404`; đã xóa → `PRODUCT_DELETED/410`; inactive → `PRODUCT_INACTIVE/409`; không còn lượng bán → `PRODUCT_OUT_OF_STOCK/409`.
- `no_formula` hoặc `no_core` → `PRODUCT_NOT_SELLABLE/422`; lỗi tính tồn kho → `PRODUCT_AVAILABILITY_UNAVAILABLE/503`. Lượng bán của đúng biến thể được chọn phải được tính bằng `CalculateAvailableQty`, không dùng lượng tối đa của Product cha.
- Mockup được kiểm tra: không có → `MOCKUP_NOT_FOUND/404`; đã xóa → `MOCKUP_DELETED/410`; inactive → `MOCKUP_INACTIVE/409`.
- Flower Create không có prompt hiện hành type `flower` hoặc content rỗng/không hợp lệ → `SYSTEM_PROMPT_INVALID/409`.
- Validation chỉ diễn ra một lần tại admission, trước AI. Admin đổi Product/Mockup sau snapshot không được hủy request, retry, gắn logo, persist kết quả hoặc history.

### 4.2 Validation boundary và snapshot semantics

Thứ tự admission là `auth/request → Product → Mockup → system prompt → reserve quota → snapshot → AI`.

- Nếu admission fail: không gọi AI, không tạo `generated_flowers`/`client_histories`, không consume quota.
- Sau snapshot không revalidate nguồn trong AI, retry, trước persist hoặc sau khi gắn logo.
- Retry dùng cùng bytes/version ảnh và full prompt snapshot; URL live không được tải lại.
- Generated Flower thuộc user là kết quả lịch sử hợp lệ để tạo Card dù Product/Mockup gốc về sau inactive, soft-delete hoặc hết hàng.
- Generated Flower không bảo đảm Order hợp lệ; Order phải lần theo `client_histories` đến Product gốc và kiểm tra live.

### 4.2A Regenerate Flower

- Admission: `auth/request → ownership → source Flower + history/snapshot → resolve Effective Mockup → prompt current/fallback → current logo/config → reserve quota → snapshot → AI`.
- Product, toàn bộ user input và ảnh Product lấy nguyên từ snapshot nguồn; không query hay validate Product live, không lấy ảnh output Flower cũ làm input AI.
- Nếu `mockup_id` null/bỏ trống hoặc bằng ID Mockup nguồn, Effective Mockup là snapshot nguồn và không kiểm tra live. Chỉ ID khác nguồn mới được validate: `MOCKUP_NOT_FOUND/404` → `MOCKUP_DELETED/410` → `MOCKUP_INACTIVE/409`.
- Prompt hiện hành type `flower` chỉ dùng khi content hợp lệ; nếu không có/rỗng/invalid thì fallback full prompt snapshot nguồn. Nếu cả hai không đủ dùng, trả `FLOWER_SOURCE_HISTORY_INVALID/409`.
- Flower mới có ID, AI `image_url`, `created_at` mới và `user_id` hiện tại. Logo/config lấy hiện hành; các dữ liệu nghiệp vụ còn lại lấy từ nguồn.

### 4.3 Thứ tự ưu tiên Prompt

`Combo > Mockup > Ghi chú/Yêu cầu thêm > Phong cách`

### 4.4 Input cho AI Module

| Input | Nguồn |
|---|---|
| Dữ liệu người dùng nhập | Request API; chỉ dùng trong lần gọi AI |
| Thông tin và ảnh Combo | `products` / Core Database |
| Mockup | `mockup` được chọn |
| System prompt | `system_prompts` với `type = "flower"` |

Với Regenerate, Product/user input lấy từ source snapshot, Mockup lấy từ Effective Mockup, System Prompt dùng current/fallback như mục 4.2A và logo dùng cấu hình hiện hành.

### 4.5 Kết quả ảnh

- Ảnh được gắn logo theo cấu hình trước khi lưu.
- Chỉ ảnh kết quả hợp lệ mới được lưu trong `generated_flowers`.

---

## 5. QUOTA RULES

| Loại | Giới hạn | Đơn vị |
|---|---:|---|
| Tạo mẫu hoa | 3 lượt | /ngày/khách hàng |

1. Sau khi dependency validation thành công, reserve quota slot atomically trước snapshot/AI để hai request không cùng chiếm lượt cuối.
2. Thành công thì consume đúng một slot; AI hoặc persistence thất bại thì release slot.
3. Một lần gọi ban đầu + tối đa 2 retry = tối đa 3 lần gọi AI, dùng cùng snapshot và cùng slot.
4. Nếu vẫn thất bại, không lưu kết quả/history và quota không bị consume.

---

## 6. API ENDPOINTS

### FLOWER – Tạo mẫu hoa AI

| Stories | Chức năng | Method | Route | Ghi chú |
|---|---|---|---|---|
| STORY-030, STORY-033 | Tạo mẫu hoa AI | POST | `/api/ai-flowers` | Nhận input, gọi AI và trả kết quả trong một flow |
| Regenerate Flower | Tạo lại mẫu hoa từ lịch sử | POST | `/api/ai-flowers/{source_flower_id}/regenerate` | Chỉ `mockup_id` tùy chọn |
| STORY-030 | Lấy danh sách Mockup | GET | `/api/mockups` | Dùng Mockup active, chưa soft-delete |

Không có endpoint `/api/flower-requests`, endpoint generate/regenerate theo `flower_request_id`, hoặc endpoint kết quả theo request id. Regenerate dùng ID Flower kết quả trong lịch sử, không dùng request id.

### FLOWER – Checkout & Order (context only)

- Endpoint code hiện hữu: `POST /api/v1/orders`; thanh toán hoàn tất qua webhook SePay/Bảo Kim.
- Order phải resolve Product nguồn live, yêu cầu `sellableQuantity >= tổng Number` sau khi cộng gộp các dòng cùng nguồn, rồi vẫn chạy kiểm kho depot Nhanh.vn hiện hữu.
- Với ID biến thể con, Order kiểm tra child active trước, sau đó kiểm tra Product cha; cha inactive thì chặn Order. Quy tắc cha này không dùng để chặn Flower/Card generation đã nhận đúng child ID.
- Chưa có TDD Order trong phạm vi này.

#### OPEN DECISION bắt buộc hỏi lại khi tiếp tục Order

1. `CreateOrderRequest` hiện chỉ có `Items(ItemId, ProductId, Number)`, chưa có `generated_card_id`. Cần quyết định Card AI được truyền vào Order bằng contract nào. Phương án đang chờ duyệt: thêm `generated_card_id` tùy chọn ở cấp request, kiểm tra ownership, resolve lineage, yêu cầu Product gốc khớp một `Items.ProductId`, và lưu provenance snapshot vào `OrderDetails.Metadata` mà không thêm bảng/cột.
2. Chưa reserve inventory. Nếu webhook nhận tiền nhưng Product không còn bán được, cần quyết định trạng thái cuối (`CANCELLED`, `REFUNDED` hoặc trạng thái mới) và quy trình hoàn tiền. Không được tự suy diễn trước khi được xác nhận.

---

## GHI CHÚ QUAN TRỌNG

- AI Module tách riêng; context này chỉ mô tả contract và lưu kết quả sau khi AI thành công.
- `generated_flowers` không chứa `flower_request_id`; toàn bộ input đã dùng được đóng băng trong `input_snapshot` thay vì các column rời `occasion`, `style`, `budget`, `note`.
- `flower_requests` và `flower_ai_jobs` đã được loại bỏ hoàn toàn.
- Snapshot lưu full system prompt (`id`, `content`, `sha256`) và ảnh theo bytes đã tải hoặc `object_key/version_id/sha256`, không chỉ URL.
