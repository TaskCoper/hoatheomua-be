# Tách `Configs` thành 3 loại: content / setting / secret

> Tài liệu cho **backend team**: vì sao tách bảng `Configs` theo độ nhạy cảm, schema thay đổi, hợp đồng API mới, logic mask/reveal cho secret. **Không thay đổi shape JSON của `Value`** — FE tiếp tục đọc đúng `ConfigDefinition` cũ.

---

## 1. Bối cảnh & vấn đề

Bảng `Configs` hiện chứa **chung** 3 loại dữ liệu khác nhau về độ nhạy cảm:

| Loại                          | Ví dụ Key                                                                       | `IsPublic` | Ai được phép đọc                                 |
| ----------------------------- | ------------------------------------------------------------------------------- | ---------- | ------------------------------------------------ |
| Nội dung web (CMS)            | `contact.phone`, `company.info`, `occasions.collection`, `feedbacks.collection` | `true`     | Bất kỳ ai vào storefront                         |
| Cấu hình tích hợp             | `nhanhvn.default_depot`, `ahamove.base_url`, `ahamove.shipping.*`               | `false`    | Admin                                            |
| **Khoá bí mật (credentials)** | `nhanhvn.token`, `ahamove.api_key`                                              | `false`    | **Chỉ server** dùng gọi outbound — không browser |

Phân biệt hiện tại **chỉ dựa vào cờ `IsPublic`** trên từng dòng. Vấn đề:

1. **Endpoint `GET /v1/configs` trả tất cả dòng admin thấy được**, bao gồm `nhanhvn.token` plaintext. Token đó tự động vào: React Query cache, DevTools Network tab, Sentry breadcrumbs, HAR export, screen share — kể cả khi admin chỉ vào trang sửa text web không liên quan.
2. **Operator bấm nhầm `IsPublic = true`** trên 1 dòng credential → token public ngay. Không có safety net.
3. **Editor UI gộp chung trong cùng 1 trang** — copy text web nằm cạnh API key trên cùng một bảng.

**Outcome:** tách 3 loại bằng 1 cột discriminator + endpoint riêng cho từng loại. **Không reshape JSON, không đổi key.** Migration = 1 `ALTER` + 3 `UPDATE` idempotent.

---

## 2. Đề xuất tổng quan

Thêm cột `Kind`:

```
Kind: 'content' | 'setting' | 'secret'   -- NOT NULL VARCHAR(16), DEFAULT 'content'
```

Mapping cho dữ liệu hiện tại (dump 2026-06-30):

| `Kind`    | Group hiện tại                                                                                                                                         | Số dòng | Ghi chú                                                                                      |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- | -------------------------------------------------------------------------------------------- |
| `content` | `contact.*`, `company.*`, `occasions.*`, `feedbacks.*`                                                                                                 | 14      | Public website. `IsPublic` chuyển vai trò thành "draft / published" trong phạm vi `content`. |
| `setting` | `nhanhvn.app_id`, `nhanhvn.business_id`, `nhanhvn.default_depot`, `nhanhvn.order_tags`, `ahamove.base_url`, `ahamove.shipping.*`, `products.signature` | 10      | Cấu hình admin, không nhạy cảm.                                                              |
| `secret`  | `nhanhvn.token`, `ahamove.api_key`                                                                                                                     | 2       | **Credentials.** Mask khi đọc, reveal qua endpoint riêng.                                    |

Không thay đổi:

- Shape `Value` JSON (`ConfigDefinition` — xem `src/tenants/hoa-theo-mua/features/configs/types.ts`)
- Tên `Key` (vd `nhanhvn.token` vẫn là `nhanhvn.token`)
- Cột `Group` (giữ làm sub-grouping trong từng `Kind`)
- `IsPublic` (chỉ còn ý nghĩa trong `Kind='content'`)

---

## 3. Thay đổi schema

### 3.1 DDL

```sql
ALTER TABLE Configs
  ADD COLUMN Kind VARCHAR(16) NOT NULL DEFAULT 'content';

ALTER TABLE Configs
  ADD CONSTRAINT chk_configs_kind
  CHECK (Kind IN ('content', 'setting', 'secret'));

CREATE INDEX idx_configs_kind_group ON Configs (Kind, "Group") WHERE IsDeleted = false;
```

### 3.2 Backfill (idempotent — chạy lại nhiều lần vẫn ra cùng kết quả)

```sql
-- Bước 1: credentials trước (vì chúng nằm trong group nhanhvn/ahamove)
UPDATE Configs SET Kind = 'secret'
WHERE Kind <> 'secret'
  AND "Key" IN ('nhanhvn.token', 'ahamove.api_key');

-- Bước 2: phần còn lại của nhanhvn/ahamove = setting
UPDATE Configs SET Kind = 'setting'
WHERE Kind = 'content'
  AND "Group" IN ('nhanhvn', 'ahamove');

-- Bước 3: products.signature là internal mapping
UPDATE Configs SET Kind = 'setting'
WHERE Kind = 'content'
  AND "Key" = 'products.signature';

-- Mọi dòng còn lại giữ Kind = 'content' (default đã đúng)
```

### 3.3 Verify sau backfill

```sql
SELECT Kind, COUNT(*) AS rows, STRING_AGG(DISTINCT "Group", ', ') AS groups
FROM Configs
WHERE IsDeleted = false
GROUP BY Kind
ORDER BY Kind;
```

Kết quả mong đợi:

| Kind      | rows | groups                                 |
| --------- | ---- | -------------------------------------- |
| `content` | 14   | contact, company, occasions, feedbacks |
| `setting` | 10   | nhanhvn, ahamove, products             |
| `secret`  | 2    | nhanhvn, ahamove                       |

---

## 4. Hợp đồng API mới

Tất cả response dùng wrapper `BaseResponse<T>` hiện tại:

```ts
type BaseResponse<T> = {
  isSuccess: boolean
  isFailure: boolean
  value: T
  error: { message: string; messageCode: string } | null
}
```

`Config` payload giữ shape camelCase FE đang đọc, **thêm 1 field `kind`**:

```ts
type Config = {
  id: string
  key: string
  isPublic: boolean
  value: ConfigDefinition // ConfigDefinition.value: unknown — tuỳ inputType
  userId: string
  group: string
  kind: 'content' | 'setting' | 'secret' // ← MỚI
}
```

**Auth chung:** tất cả endpoints trừ `/content` yêu cầu cookie session admin. App hiện chỉ có 1 admin role nên không cần phân biệt sub-roles. Sai auth → trả `401` / `403` với `messageCode: "PermissionDenied"`.

### 4.1 `GET /v1/configs/content` — Public, cacheable

- Filter server: `WHERE Kind = 'content' AND IsPublic = true AND IsDeleted = false`
- Set header: `Cache-Control: public, max-age=60, stale-while-revalidate=300` (cache được ở CDN/edge)
- Admin xem cả bản draft đi qua `?includeDrafts=true` (cần auth admin, bỏ filter `IsPublic`)

**Response 200:**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": [
    {
      "id": "51f215c5-169a-4188-93a8-c0fb1c27770b",
      "key": "contact.phone",
      "isPublic": true,
      "value": {
        "group": "contact",
        "label": "Số điện thoại",
        "value": "0983024353",
        "options": [],
        "required": true,
        "inputType": "text",
        "validations": []
      },
      "userId": "9ea83b22-cd9f-45c0-b8dd-e1c4e25ada11",
      "group": "contact",
      "kind": "content"
    }
  ],
  "error": null
}
```

### 4.2 `GET /v1/configs/settings` — Admin

- Filter server: `WHERE Kind = 'setting' AND IsDeleted = false`
- Trả plaintext bình thường, **không mask**.
- Response shape giống 4.1, rows có `kind: "setting"`.

### 4.3 `GET /v1/configs/secrets` — Admin, **MASK BẮT BUỘC**

- Filter server: `WHERE Kind = 'secret' AND IsDeleted = false`
- **Mask `Value.value` server-side trước khi trả về** (thuật toán mục 5).
- Metadata khác (`label`, `description`, `inputType`, ...) trả về bình thường để FE render đúng form.

**Response 200 (mẫu — token đã mask):**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": [
    {
      "id": "c31886cd-7686-4787-bfe8-7deb2911b6c0",
      "key": "nhanhvn.token",
      "isPublic": false,
      "value": {
        "group": "nhanhvn",
        "label": "Token của NhanhVN",
        "value": "••••••••••••••••••••••••••kZPncG0QwyE7kEvO97vrc",
        "options": [],
        "required": true,
        "inputType": "text",
        "validations": []
      },
      "userId": "9ea83b22-cd9f-45c0-b8dd-e1c4e25ada11",
      "group": "nhanhvn",
      "kind": "secret"
    }
  ],
  "error": null
}
```

> ❌ **Tuyệt đối không** trả raw value của `Kind='secret'` qua bất kỳ list endpoint nào. Plaintext token chỉ được trả qua reveal endpoint (4.4).

### 4.4 `POST /v1/configs/secrets/{id}/reveal` — One-shot reveal

- Trả raw value của 1 dòng secret cho admin click "Hiện" trong UI.
- **Server không lưu state** — `expiresAt` chỉ là gợi ý cho FE biết khi nào tự ẩn.
- Mỗi click "Hiện" = một POST mới = một response mới.
- Set header: `Cache-Control: no-store, max-age=0, must-revalidate`

**Request:** không cần body (id nằm trong path).

**Response 200:**

```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": {
    "id": "c31886cd-7686-4787-bfe8-7deb2911b6c0",
    "key": "nhanhvn.token",
    "value": "qC6szW89FwrjJTiwHGT7FtJOjW4tNSRwrp080cT9klzcIq7IWW54nH4C4JZL3jU9yrBlQ71jZ952BBu1nswx1eLrIZoNRxDN4JxIgFdn6yUsAXbcTByGH1J8ue3FonfdL0Jc9OmGK4rDBA8f7tnhjUGVP3f8QPj4I8Jh5Ly9RkZPncG0QwyE7kEvO97vrc",
    "revealedAt": "2026-06-30T09:30:12.000Z",
    "expiresAt": "2026-06-30T09:35:12.000Z"
  },
  "error": null
}
```

**Logic `expiresAt`:** server tính `expiresAt = revealedAt + 5 phút`, trả về cùng response. Hết. **Không enforce server-side**, không track session, không track "user X đang xem secret Y". FE dùng `expiresAt` để setTimeout tự ẩn local state.

**Response 404 (id không tồn tại hoặc không phải secret):**

```json
{
  "isSuccess": false,
  "isFailure": true,
  "value": null,
  "error": { "message": "Config not found or not a secret", "messageCode": "SecretNotFound" }
}
```

### 4.5 `PUT /v1/configs/{id}` — Cập nhật

Body giữ shape hiện tại (`CreateConfigFormValues` — xem `validations.ts`), **thêm field `kind`**:

```json
{
  "key": "nhanhvn.token",
  "isPublic": false,
  "group": "nhanhvn",
  "kind": "secret",
  "value": {
    "group": "nhanhvn",
    "label": "Token của NhanhVN",
    "value": "<new_token_value>",
    "options": [],
    "required": true,
    "inputType": "text",
    "validations": []
  }
}
```

**Server validation BẮT BUỘC:**

> Nếu `body.kind !== row.kind` trong DB → return **409 Conflict** với `messageCode: "ConfigKindImmutable"`.

Lý do: không cho phép operator "ẩn" 1 secret bằng cách đổi thành `content` để qua mặt check. Cần đổi `Kind` thật sự → tạo dòng mới + soft delete dòng cũ.

### 4.6 `POST /v1/configs` — Tạo mới

Body bắt buộc có `kind`. Thiếu hoặc giá trị không hợp lệ → `400` với `messageCode: "ValidationFailed"`.

### 4.7 `DELETE /v1/configs/{id}` — Soft delete

Giữ behaviour hiện tại, không đổi gì.

### 4.8 `GET /v1/configs` (endpoint cũ)

**Giữ lại trong giai đoạn migration** (xem mục 6). Trong giai đoạn quá độ: endpoint cũ vẫn trả tất cả `kind`, **nhưng phải mask `secret` giống 4.3** ngay từ Phase 0. Không được trả token plaintext qua endpoint cũ.

---

## 5. Logic mask & reveal cho `secret`

### 5.1 Thuật toán mask (server-side)

Áp dụng cho mọi response chứa row `Kind='secret'`, **trừ** reveal endpoint (4.4).

Pseudo-code:

```
maskSecretValue(rawValue):
  if rawValue is null or undefined:
    return rawValue
  if typeof rawValue === 'string':
    if rawValue.length <= 4:
      return '•'.repeat(rawValue.length)
    return '•'.repeat(rawValue.length - 4) + rawValue.slice(-4)
  if typeof rawValue === 'number':
    return '••••'
  if typeof rawValue === 'object':
    return '••• (use reveal endpoint)'
  return '••••'
```

Mask **chỉ** áp lên `Value.value`. Metadata khác (`label`, `description`, `inputType`, ...) trả nguyên.

### 5.2 Reveal flow (BE side)

Đơn giản:

1. Nhận POST `/v1/configs/secrets/{id}/reveal`
2. Check auth admin + check `row.Kind === 'secret'`
3. Đọc raw `Value` từ DB
4. Tính `revealedAt = now()`, `expiresAt = revealedAt + 5min`
5. Trả `{ id, key, value: <raw>, revealedAt, expiresAt }`

**Không cần:**

- Lưu state "đang reveal cho ai"
- Background job xoá session
- Track expiry server-side (FE tự handle)

### 5.3 Cách FE tiêu thụ `expiresAt` (tham khảo, BE không cần implement)

FE dùng `useMutation` (không phải `useQuery` để tránh raw value vào React Query cache), giữ local state, setTimeout dựa trên `expiresAt`:

```tsx
const reveal = useMutation({ mutationFn: (id) => configService.revealSecret(id) })

const handleReveal = async () => {
  const r = await reveal.mutateAsync(secret.id)
  setRevealedValue(r.value)
  setExpiresAt(new Date(r.expiresAt).getTime())
}

// useEffect setTimeout(expiresAt - Date.now()) → clear local state khi hết hạn
// → component re-render với mask cũ từ list endpoint
```

→ BE chỉ cần trả `expiresAt` đúng. FE tự handle UI ẩn/hiện.

---

## 6. Migration plan (rollout)

3 phase, không downtime, không break FE đang chạy.

### Phase 0 — Schema + backfill + hot-fix mask

1. Deploy migration `ALTER` + `CREATE INDEX` + 3 `UPDATE` (mục 3).
2. Verify count theo bảng 3.3.
3. **Hot-fix bắt buộc cùng phase này:** mask `Kind='secret'` trong response của endpoint cũ `GET /v1/configs` ngay lập tức (mục 4.8). Đây là vá lỗ leak chính, không chờ FE.
4. FE chạy như cũ — chỉ thấy secret bị mask.

### Phase 1 — Endpoint mới chạy song song

1. Deploy 4 endpoint mới: `/configs/content`, `/configs/settings`, `/configs/secrets`, `/configs/secrets/{id}/reveal`.
2. PUT/POST validate `kind` (mục 4.5, 4.6).
3. Endpoint cũ `GET /v1/configs` vẫn sống (đã mask từ Phase 0).
4. Báo FE team có thể bắt đầu split UI.

### Phase 2 — FE cut over → deprecate endpoint cũ

1. FE merge PR tách `configs.page.tsx` → gọi 3 endpoint mới.
2. BE log warning cho mọi request vào endpoint cũ: `[deprecated] GET /v1/configs called by <userAgent>`.
3. Sau 2 sprint (~1 tháng) không còn caller → BE xoá endpoint cũ.

---

## 7. Cảnh báo TRƯỚC khi triển khai

### 7.1 Schema

- ❌ **KHÔNG** đổi shape của cột `Value` JSON. FE đang parse theo `ConfigDefinition` interface. Thêm field optional bên trong được, nhưng đừng đổi tên field cũ.
- ❌ **KHÔNG** đổi tên `Key` của các config hiện tại. FE hardcode một số keys trong `locked-config-keys.ts` và `*-palette.ts` files — đổi key = vỡ editor.
- ✅ Backfill SQL **phải idempotent**. Chạy lại lần thứ 2 phải ra cùng kết quả.
- ✅ `CHECK constraint` phải có **trước** khi mở write API mới.

### 7.2 API

- ❌ **KHÔNG** trả raw value của `Kind='secret'` qua bất kỳ list endpoint nào. Mask phải áp ở serializer chứ không phải ở caller — caller nào quên gọi mask = leak.
- ❌ **KHÔNG** cho phép `PUT` đổi `Kind`. Validate `body.kind === row.kind` trước khi update. Hành vi sai = 409, không phải 200.
- ❌ **KHÔNG** trả token / api_key trong error message hoặc stack trace. Logger phải redact `Kind='secret'` row values.
- ✅ `GET /v1/configs/content` **phải cache được** ở CDN/edge — endpoint này bị gọi nhiều nhất.
- ✅ Reveal endpoint phải `Cache-Control: no-store, max-age=0, must-revalidate`.
- ✅ Middleware check permission đọc `Kind` từ row trong DB, **không** từ body (tránh client gửi `kind: 'content'` để bypass check).

### 7.3 Quy trình test

- Snapshot DB → chạy migration → verify count → revert. Đừng để DB prod ở trạng thái "đã ALTER nhưng chưa deploy code mới".
- Test mask: `GET /v1/configs/secrets` phải trả dots, không phải raw.
- Test 409: PUT với `body.kind = 'content'` cho row đang là `'secret'` → 409 `ConfigKindImmutable`.
- Test reveal: POST `/secrets/{id}/reveal` → response có `value` raw + `expiresAt` đúng `revealedAt + 5min`.

---

## 8. Files FE liên quan (tham chiếu)

Backend không cần đụng vào, chỉ cần biết FE đang dựa vào shape gì:

| File                                                              | Vai trò                                                                                            |
| ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `src/tenants/hoa-theo-mua/features/configs/types.ts`              | Định nghĩa `Config`, `ConfigDefinition`, `InputType`. Shape `Value` JSON phải match interface này. |
| `src/tenants/hoa-theo-mua/features/configs/validations.ts`        | Zod schema `configExportSchema` validate request body.                                             |
| `src/tenants/hoa-theo-mua/features/configs/services.ts`           | Axios calls. Sau Phase 1 sẽ thêm `getContent`, `getSettings`, `getSecrets`, `revealSecret`.        |
| `src/tenants/hoa-theo-mua/features/configs/locked-config-keys.ts` | Keys FE hardcode (`nhanhvn.default_depot`, `nhanhvn.order_tags`). **Không được đổi tên** ở BE.     |
| `src/tenants/hoa-theo-mua/features/configs/*-palette.ts`          | Hardcode các key `occasions.collection`, `feedbacks.collection`, `ahamove.*`.                      |
| `src/lib/axios.ts`                                                | `BaseResponse<T>` wrapper. Mọi response BE phải match shape này.                                   |

---

## 9. FAQ ngắn

**Q: Tại sao không tách thẳng 2 bảng (`ContentConfigs` + `SecretConfigs`)?**
A: Tách 2 bảng là phương án sạch nhất về bảo mật, nhưng (a) gấp đôi scaffolding cho mọi feature mới, (b) FE cần 2 service class riêng. Cột `Kind` đạt 95% lợi ích bảo mật (endpoint riêng + mask + 409 immutable) với chi phí 1 `ALTER` + backfill. Nếu sau này muốn promote `secret` lên bảng riêng + Vault → vẫn làm được mà không phá content path.

**Q: Tại sao cần endpoint reveal riêng mà không trả raw luôn trong `/secrets`?**
A: Mask không phải để chống admin — mà để giới hạn khoảng thời gian token plaintext tồn tại trong browser. Endpoint list được gọi mỗi lần load trang admin (mỗi reload, mỗi tab, mỗi RQ refetch). Endpoint reveal chỉ được gọi khi admin chủ ý click "Hiện". Khác biệt giữa "token nằm trong browser memory 24/7" và "chỉ 5 phút sau khi tự bấm" → chống leak qua DevTools / Sentry breadcrumbs / HAR export / screen share. Mỗi lần gọi reveal = code path tách biệt, không thể quên mask.

**Q: `Kind` có mở rộng giá trị mới được không?**
A: Được. Update `CHECK constraint` + thêm endpoint nếu cần. Ví dụ tương lai: `policy`, `experiment`, `featureFlag`, `webhookSecret`.

**Q: Endpoint cũ `GET /v1/configs` khi nào tắt?**
A: Sau khi FE cut over (Phase 2) + 2 sprint không có caller. BE bật warning log để đếm caller.

**Q: Có cần audit log cho reveal không?**
A: V1 không cần (app hiện chỉ có 1 admin). Nếu sau này có nhiều admin → thêm bảng `ConfigAuditLog(Id, ConfigId, UserId, Action, AtUtc)` ghi mỗi lần reveal/update/delete cho `Kind='secret'`. Vì reveal đã là endpoint riêng, instrumentation chỉ cần ở 1 chỗ.

**Q: Có cần encrypt at-rest cho `secret` không?**
A: V1 không bắt buộc — mask + endpoint tách + 409 immutable đã đóng được lỗ hổng chính (leak qua API + qua screen share). V2 nếu hạ tầng có KMS sẵn thì encrypt thêm 1 lớp ở cột `Value` cho `Kind='secret'`.

**Q: Nếu operator sửa nhầm `IsPublic = true` trên 1 row `Kind='content'` đang draft?**
A: Storefront thấy ngay (endpoint `/configs/content` filter `IsPublic = true`). Behaviour này không đổi — operator vẫn cần cẩn thận với `IsPublic` trong phạm vi content. Nhưng secrets không còn bị ảnh hưởng bởi `IsPublic` nữa (filter của `/configs/secrets` chỉ check `Kind`).

**Q: Có cần migration version control (Flyway / EF migrations / etc.)?**
A: Tuỳ stack BE. Tất cả SQL ở mục 3 idempotent → đặt vào 1 migration file. Nhớ kèm script rollback (`DROP COLUMN Kind` + `DROP INDEX`) để có đường lùi.
