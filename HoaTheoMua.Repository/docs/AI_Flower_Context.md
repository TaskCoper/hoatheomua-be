# AI Flower Context - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize  
> Date: 2026-08-27  
> Last Updated: 2026-09-09

---

## 0. DECISION RECORD — 2026-08-28

| Quyết định | Trạng thái |
|---|---|
| Generate kiểm tra dữ liệu hiện tại đúng một lần trước AI, sau đó hàm gọi AI dùng cùng bộ dữ liệu cho lần đầu và tối đa hai lần thử lại | Chốt |
| Admin đổi active/delete/stock/config sau khi dữ liệu đã được chuẩn bị không ảnh hưởng request đang chạy | Chốt |
| Generated Flower lịch sử vẫn tạo Card; Order mới validate Product gốc live | Chốt |
| Generated Flower thiếu Flower History/Product lineage không được dùng tạo Card; trả `SOURCE_PRODUCT_PROVENANCE_INVALID/409` | Chốt |
| Flower/Card history đều là record trong `client_histories`, không có bảng history riêng | Chốt |
| `client_histories.root` bắt buộc lưu Product ID gốc; Flower History luôn có `root=base_id` và cột không có FK | Chốt |
| API Generate với Product có variants phải nhận đúng child/variant ID | Chốt |
| Parent inactive chỉ chặn ở Order sau khi child active; không dùng parent để chặn Generate | Chốt |
| Config cho phép nhiều key trong cùng group; tối đa một record active/chưa xóa cho exact `(group,key)` | Chốt |
| Bỏ `generated_flowers.input_snapshot`; Flower History metadata lưu tên, thuộc tính, ảnh, trạng thái Product, Mockup và cấu hình thực tế cần dùng lại | Chốt |
| Bỏ `userInput` khỏi cả Create và Regenerate; API tạo hoa chỉ nhận Product và Mockup | Chốt |
| Regenerate lấy Product từ Flower History metadata; chỉ Mockup được phép chọn lại | Chốt |
| Regenerate bỏ trống/null/đúng ID Mockup nguồn dùng Mockup trong History metadata, không kiểm tra live; chỉ ID khác nguồn được validate live | Chốt |
| Regenerate ưu tiên prompt hiện hành hợp lệ, nếu thiếu/rỗng/invalid thì dùng nội dung trong `system_form` nguồn | Chốt |
| Order cần quantity live đủ tổng lượng đã cộng gộp và vẫn kiểm kho Nhanh.vn | Chốt, context-only |
| Contract truyền Card AI vào Order và trạng thái payment webhook khi hết hàng | Chưa chốt — phải hỏi lại |

Không thêm bảng mới và không sửa code C# trong phạm vi cập nhật document-first này.

---

## 1. TỔNG QUAN

### Mô tả

Hệ thống tạo mẫu hoa AI cho phép khách hàng tạo ảnh mẫu hoa từ Combo và Mockup đã chọn. STORY-030 và STORY-033 cùng mô tả một lần gọi API tạo hoa; không có bước lưu yêu cầu nháp hay tạo AI Job riêng.

### Luồng chính

```text
Combo Card + Mockup
                    |
                    v
        POST /api/ai-flowers (STORY-030 + STORY-033)
                    |
                    v
Validate Product + Mockup + prompt -> chuẩn bị bộ dữ liệu cố định
                    |
                    v
  Hàm gọi AI tự quản lý quota/retry và dùng cùng bộ dữ liệu
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
- `productId` là đúng ID Product/biến thể được chọn; Product tồn tại, chưa xóa, active và `sellableQuantity > 0`.
- Mockup được chọn có `isActive = true` và `isDeleted = false`.
- Hàm gọi AI chấp nhận request theo quy tắc quota của chính hàm đó.

#### Trigger

Khách hàng chọn Combo Card, chọn Mockup và nhấn tạo mẫu hoa.

#### Input và xử lý

- API chỉ nhận `productId` chính xác của Combo/biến thể và `mockupId`; không nhận `userInput`.
- Service đọc dữ liệu hiện tại theo thứ tự Product → Mockup → System Prompt, sau đó chuẩn bị một bộ dữ liệu cố định trong bộ nhớ trước khi gọi hàm AI.
- Mỗi `system_prompts.type` có đúng một prompt hiện hành. Create yêu cầu prompt type `flower` tồn tại và content hợp lệ; Create không có History nguồn để dùng dự phòng.
- Bộ dữ liệu trong bộ nhớ giữ Product/biến thể, Mockup, bytes hoặc object version/hash của ảnh và toàn bộ nội dung System Prompt. Dữ liệu đó được truyền một lần cho hàm gọi AI; không tạo `flower_request` hay `flower_ai_job`.
- Việc đếm lượt và thử lại do hàm gọi AI tự quản lý. Khi thử lại, hàm này phải dùng cùng bộ dữ liệu và không tải lại ảnh hoặc đọc lại trạng thái nguồn.
- API trả ảnh tạo thành công trong cùng flow. Khi tạo lại từ lịch sử, client gọi `POST /api/ai-flowers/{sourceFlowerId}/regenerate`, chỉ được chọn lại Mockup; không có endpoint theo `flowerRequestId`.

#### Lưu kết quả

- Chỉ khi AI trả ảnh hợp lệ, tạo một dòng `generated_flowers` và một dòng `client_histories` có `type = "flower"`.
- API không nhận hoặc lưu `userInput`. Dữ liệu Product và Mockup cần dùng lại nằm trong `client_histories.metadata`.
- `client_histories.base_id = products.id`; `root = base_id`; `output_id = generated_flowers.id`. Nếu Product được chọn là biến thể thì cả `base_id` và `root` lưu đúng ID biến thể đó.
- `generated_flowers` chỉ giữ kết quả ảnh. `client_histories.metadata` giữ tên, thuộc tính, ảnh và trạng thái Product tại thời điểm tạo; Mockup và cấu hình logo thực tế; loại thao tác và nguồn tạo lại. `client_histories.system_form` giữ System Prompt đã dùng.
- Khi AI thất bại sau retry, không tạo `generated_flowers`, `client_histories`, `flower_requests` hoặc `flower_ai_jobs`.

### Tạo lại mẫu hoa từ lịch sử

- Flower nguồn phải thuộc user hiện tại và có đúng một `client_histories(type="flower")`, `base_id` Product gốc, `root=base_id` khác `Guid.Empty` cùng metadata đủ dùng. Thiếu hoặc không nhất quán nguồn gốc Product, `root` hay metadata bắt buộc trả `FLOWER_SOURCE_HISTORY_INVALID/409`.
- Không dùng ảnh kết quả `imageUrl` của Flower nguồn làm đầu vào AI. Hệ thống dựng dữ liệu mới từ tên, thuộc tính và ảnh Product trong History metadata cùng Mockup thực tế.
- Client chỉ được truyền `mockupId` tùy chọn; mọi field còn lại không được phép thay đổi. Bỏ trống/null hoặc truyền đúng ID Mockup nguồn thì dùng nguyên Mockup trong History metadata, không đọc dữ liệu hiện tại và không kiểm tra active/soft-delete. Chỉ ID khác nguồn mới được kiểm tra theo not-found → deleted → inactive.
- Mỗi `system_prompts.type` có đúng một prompt hiện hành. Regenerate ưu tiên prompt hiện hành type `flower`; nếu record không tồn tại hoặc content rỗng/không hợp lệ thì dùng nội dung trong `system_form` nguồn, không kiểm tra trạng thái record prompt cũ.
- Logo dùng cấu hình hiện hành. History mới lưu dữ liệu Product kế thừa, Mockup thực tế, nguồn prompt và cấu hình logo thực tế đã dùng.
- History mới giữ cột `base_id` của History nguồn, gán `root=base_id`, đặt `output_id` bằng ID Flower mới và `metadata.regeneratedFromFlowerId` bằng ID Flower trực tiếp được chọn. Chuỗi F1 → F2 → F3 vì vậy có cùng Product `base_id/root`, còn quan hệ trực tiếp lần lượt là F2→F1 và F3→F2.
- History Regenerate không lưu input; metadata giữ provenance thực tế, `regeneratedFromFlowerId` và `systemPromptSource=current|source_flower_fallback`.

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

Chỉ lưu kết quả AI thành công; không có request/job trung gian. Dữ liệu cần dùng lại nằm trong Flower History, không nằm trong bảng này.

```sql
Table generated_flowers {
  id uuid [pk]
  image_url varchar
  user_id uuid
  created_at timestamp
}
```

#### client_histories

```sql
Table client_histories {
  id uuid [pk]
  user_id uuid
  type varchar                  -- flower | card | handmade_card; post hiện chưa dùng
  system_form json [null]       -- Snapshot {type, content}; null với handmade_card
  metadata json                 -- Snapshot/provenance; không chứa System Prompt
  created_at timestamp
  root uuid [not null]          -- Product ID gốc; type=flower luôn bằng base_id; không có FK

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
products.id --(root, logical only)--> client_histories(type=flower)
client_histories.output_id --> generated_flowers.id
```

Các mũi tên trên mô tả lineage nghiệp vụ. Database không tạo FK cho `base_id`, `output_id` hoặc `root`; service chịu trách nhiệm gán và kiểm tra. `root` không nằm trong response API công khai.

`client_histories.metadata` khi `type="flower"` dùng `schemaVersion = 3`:

```json
{
  "schemaVersion": 3,
  "operation": "create",
  "product": {
    "name": "Combo A - biến thể đỏ",
    "attributes": {},
    "image": { "url": "https://...", "objectKey": "products/a.jpg", "versionId": "v15", "sha256": "..." },
    "observedStatus": { "isActive": true, "isDeleted": false, "sellableQuantity": 3 }
  },
  "mockup": {
    "id": "uuid",
    "name": "Tên mockup",
    "description": "Mô tả mockup",
    "image": { "url": "https://...", "objectKey": "mockups/m.jpg", "versionId": "v8", "sha256": "..." },
    "observedStatus": { "isActive": true, "isDeleted": false }
  },
  "logoConfig": {},
  "regeneratedFromFlowerId": null,
  "systemPromptSource": "current",
  "validatedAt": "2026-08-28T08:00:00Z"
}
```

Product ID không lặp trong metadata vì đã có ở `base_id/root`. Flower ID không lặp vì đã có ở `output_id`. `userInput` không được lưu. Với Regenerate, `operation="regenerate"`, `regeneratedFromFlowerId` là Flower trực tiếp được chọn và `systemPromptSource` là `current` hoặc `source_flower_fallback`.

`client_histories.system_form` không có FK và lưu mọi field của `system_prompts` ngoại trừ `id`:

```json
{
  "type": "flower",
  "content": "Full prompt đã dùng"
}
```

---

## 4. QUY TẮC NGHIỆP VỤ

### 4.1 Nguồn tạo hoa

- `base_id` là `products.id` chính xác của Combo/biến thể user chọn. `root` bắt buộc bằng `base_id`. Nếu Product cha có biến thể, API không chấp nhận ID cha thay cho ID biến thể.
- Product được kiểm tra theo thứ tự cố định: không có → `PRODUCT_NOT_FOUND/404`; đã xóa → `PRODUCT_DELETED/410`; inactive → `PRODUCT_INACTIVE/409`; không còn lượng bán → `PRODUCT_OUT_OF_STOCK/409`.
- `no_formula` hoặc `no_core` → `PRODUCT_NOT_SELLABLE/422`; lỗi tính tồn kho → `PRODUCT_AVAILABILITY_UNAVAILABLE/503`. Lượng bán của đúng biến thể được chọn phải được tính bằng `CalculateAvailableQty`, không dùng lượng tối đa của Product cha.
- Mockup được kiểm tra: không có → `MOCKUP_NOT_FOUND/404`; đã xóa → `MOCKUP_DELETED/410`; inactive → `MOCKUP_INACTIVE/409`.
- Flower Create không có prompt hiện hành type `flower` hoặc content rỗng/không hợp lệ → `SYSTEM_PROMPT_INVALID/409`.
- Việc kiểm tra Product và Mockup chỉ diễn ra một lần trước khi gọi AI. Admin đổi Product/Mockup sau thời điểm này không được hủy request, bước thử lại bên trong hàm AI, bước gắn logo hoặc bước lưu kết quả.

### 4.2 Ranh giới kiểm tra và dữ liệu cố định

Thứ tự của dịch vụ nghiệp vụ là `auth/request → Product → Mockup → System Prompt → logo/config → chuẩn bị dữ liệu trong bộ nhớ → gọi hàm AI`.

- Nếu một bước kiểm tra thất bại: không gọi AI và không tạo `generated_flowers`/`client_histories`.
- Sau khi chuẩn bị dữ liệu, dịch vụ nghiệp vụ không đọc lại nguồn trước khi lưu.
- Hàm gọi AI tự quản lý quota và retry. Mọi lần thử dùng cùng bytes/version ảnh và toàn bộ prompt; URL hiện tại không được tải lại.
- Generated Flower thuộc user chỉ là nguồn hợp lệ để tạo Card khi có Flower History nhất quán và xác định được Product ID gốc. Product/Mockup gốc về sau inactive, soft-delete hoặc hết hàng không làm thay đổi `root` và không chặn Card.
- Generated Flower không bảo đảm Order hợp lệ; Order phải lần theo `client_histories` đến Product gốc và kiểm tra live.

### 4.2A Regenerate Flower

- Thứ tự là `auth/request → ownership → source Flower + Flower History → kiểm tra base_id/root Product gốc → lấy Product từ metadata → xác định Mockup thực tế → prompt hiện hành hoặc dự phòng → logo/config hiện hành → chuẩn bị dữ liệu → gọi hàm AI`.
- Tên, thuộc tính và ảnh Product lấy nguyên từ History metadata nguồn; không đọc hoặc kiểm tra Product hiện tại. Regenerate không dùng `userInput` và không lấy ảnh kết quả Flower cũ làm đầu vào AI.
- Nếu `mockupId` null/bỏ trống hoặc bằng ID Mockup nguồn, hệ thống dùng Mockup trong History metadata và không kiểm tra bản ghi hiện tại. Chỉ ID khác nguồn mới được kiểm tra: `MOCKUP_NOT_FOUND/404` → `MOCKUP_DELETED/410` → `MOCKUP_INACTIVE/409`.
- Prompt hiện hành type `flower` chỉ dùng khi content hợp lệ; nếu không có/rỗng/invalid thì dùng nội dung trong `system_form` nguồn. Nếu cả hai không đủ dùng, trả `FLOWER_SOURCE_HISTORY_INVALID/409`.
- Regenerate kế thừa `base_id` của Flower History nguồn và gán `root=base_id`; không gán `sourceFlowerId` vào hai cột này. `base_id/root` thiếu, bằng `Guid.Empty` hoặc không nhất quán trả `FLOWER_SOURCE_HISTORY_INVALID/409` trước AI và persistence.
- Flower mới có ID, AI `imageUrl`, `createdAt` mới và `userId` hiện tại. History mới sao chép Product từ metadata nguồn, lưu Mockup thực tế và cấu hình logo hiện hành. Thiếu tên, thuộc tính, ảnh Product hoặc Mockup cần dùng thì trả `FLOWER_SOURCE_HISTORY_INVALID/409`.

### 4.3 Thứ tự ưu tiên Prompt

`Combo > Mockup`

### 4.4 Input cho AI Module

| Input | Nguồn |
|---|---|
| Thông tin và ảnh Combo | `products` / Core Database |
| Mockup | `mockup` được chọn |
| System prompt | `system_prompts` với `type = "flower"` |

Với Regenerate, Product lấy từ Flower History metadata, Mockup lấy từ History hoặc Mockup mới hợp lệ, System Prompt dùng hiện hành/dự phòng như mục 4.2A và logo dùng cấu hình hiện hành.

### 4.5 Kết quả ảnh

- Ảnh được gắn logo theo cấu hình trước khi lưu.
- Chỉ ảnh kết quả hợp lệ mới được lưu trong `generated_flowers`.

---

## 5. QUOTA RULES

| Loại | Giới hạn | Đơn vị |
|---|---:|---|
| Tạo mẫu hoa | 3 lượt | /ngày/khách hàng |

Các quy tắc dưới đây thuộc hàm gọi AI, không thuộc dịch vụ nghiệp vụ trong TDD-016:

1. Hàm gọi AI giữ một lượt theo cách không cho hai request cùng chiếm lượt cuối.
2. Thành công dùng đúng một lượt; thất bại cuối cùng phải hoàn lại lượt.
3. Một lần gọi ban đầu và tối đa hai lần thử lại dùng cùng bộ dữ liệu và cùng một lượt.
4. Nếu vẫn thất bại, hàm trả lỗi cuối cùng cho dịch vụ nghiệp vụ. Dịch vụ không lưu Flower hoặc History.

---

## 6. API ENDPOINTS

### FLOWER – Tạo mẫu hoa AI

| Stories | Chức năng | Method | Route | Ghi chú |
|---|---|---|---|---|
| STORY-030, STORY-033 | Tạo mẫu hoa AI | POST | `/api/ai-flowers` | Nhận input, gọi AI và trả kết quả trong một flow |
| Regenerate Flower | Tạo lại mẫu hoa từ lịch sử | POST | `/api/ai-flowers/{sourceFlowerId}/regenerate` | Chỉ `mockupId` tùy chọn |
| STORY-030 | Lấy danh sách Mockup | GET | `/api/mockups` | Dùng Mockup active, chưa soft-delete |

Không có endpoint `/api/flower-requests`, endpoint generate/regenerate theo `flowerRequestId`, hoặc endpoint kết quả theo request ID. Regenerate dùng ID Flower kết quả trong lịch sử, không dùng request ID.

### FLOWER – Checkout & Order (context only)

- Endpoint code hiện hữu: `POST /api/v1/orders`; thanh toán hoàn tất qua webhook SePay/Bảo Kim.
- Order phải resolve Product nguồn live, yêu cầu `sellableQuantity >= tổng Number` sau khi cộng gộp các dòng cùng nguồn, rồi vẫn chạy kiểm kho depot Nhanh.vn hiện hữu.
- Với ID biến thể con, Order kiểm tra child active trước, sau đó kiểm tra Product cha; cha inactive thì chặn Order. Quy tắc cha này không dùng để chặn Flower/Card generation đã nhận đúng child ID.
- Chưa có TDD Order trong phạm vi này.

#### OPEN DECISION bắt buộc hỏi lại khi tiếp tục Order

1. `CreateOrderRequest` hiện chỉ có `Items(ItemId, ProductId, Number)`, chưa có `generatedCardId`. Cần quyết định Card AI được truyền vào Order bằng contract nào. Phương án đang chờ duyệt: thêm `generatedCardId` tùy chọn ở cấp request, kiểm tra ownership, resolve lineage, yêu cầu Product gốc khớp một `Items.ProductId`, và lưu provenance snapshot vào `OrderDetails.Metadata` mà không thêm bảng/cột.
2. Chưa reserve inventory. Nếu webhook nhận tiền nhưng Product không còn bán được, cần quyết định trạng thái cuối (`CANCELLED`, `REFUNDED` hoặc trạng thái mới) và quy trình hoàn tiền. Không được tự suy diễn trước khi được xác nhận.

---

## GHI CHÚ QUAN TRỌNG

- AI Module tách riêng; context này chỉ mô tả contract và lưu kết quả sau khi AI thành công.
- `generated_flowers` không chứa `flower_request_id` hoặc `input_snapshot`; bảng này chỉ lưu kết quả ảnh.
- `userInput` không thuộc hợp đồng API Create hoặc Regenerate và không được lưu vào bảng nào. Flower History metadata chỉ giữ Product, Mockup, cấu hình và nguồn gốc cần cho tạo lại.
- `flower_requests` và `flower_ai_jobs` đã được loại bỏ hoàn toàn.
- Flower History lưu ảnh theo `url + objectKey/versionId/sha256`, không chỉ URL. System Prompt thực tế nằm trong `client_histories.system_form` dưới dạng `{type, content}` và không lưu ID.
