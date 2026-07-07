# Form validation — Ràng buộc phía frontend cần backend mirror lại

Tài liệu này liệt kê **mọi rule validation** đang chạy trong frontend `hoatheomua-fe`. Backend team dùng để implement server-side validation.

> **Nguyên tắc**: FE validation chỉ để UX — user thao tác nhanh hơn, thấy lỗi tức thì. Backend **phải validate lại toàn bộ** vì client có thể bypass FE (tắt JS, gọi API trực tiếp bằng curl, sửa payload trong DevTools…).
>
> Stack: **Zod 4 + react-hook-form**. Tất cả string schema đều `.trim()` trước khi check length.

---

## 1. Helper dùng chung

File: `shared/validations.ts`. Các form gọi lại các helper này.

### 1.1. Số điện thoại VN (dùng cho profile, address, order)

- Regex: `VN_LOCAL_PHONE_REGEX = /^0\d{9,10}$/`
- Bắt đầu bằng `0`, tổng 10–11 chữ số.
- Bắt buộc (`createPhoneNumberSchema`): `trim()` → `min(1)` → khớp regex.
- Optional (`createOptionalPhoneNumberSchema`): `trim()` → chấp nhận chuỗi rỗng hoặc khớp regex.

### 1.2. Số điện thoại VN chặt hơn (chỉ contact form)

- Regex: `/^(?:\+84|0)[3-9]\d{8}$/`
- Chấp nhận `+84` hoặc `0`, tiếp theo `[3-9]` + 8 chữ số (đúng chuẩn nhà mạng VN mới).
- Trước khi test regex, **strip** whitespace, `-`, `.`, `(`, `)` khỏi input.

### 1.3. Email

- Chuẩn RFC (Zod `.email()`).
- Trim + lowercase trước khi lưu.
- Bắt buộc: `createEmailSchema` = `.email().toLowerCase()`.
- Optional: `createOptionalEmailSchema` = `.trim()` → chấp nhận chuỗi rỗng hoặc email hợp lệ.
- Với contact form thêm `.max(254)`.

### 1.4. Mật khẩu

Có **hai chính sách khác nhau** đang tồn tại trong code — backend cần xác nhận nên áp cái nào cho endpoint nào (xem ghi chú cuối tài liệu):

**Chính sách A** — dùng cho sign-up và change-password:
- Length: 6–128 ký tự.
- Phải chứa: ít nhất 1 chữ hoa `[A-Z]`, ít nhất 1 chữ số `\d`, ít nhất 1 ký tự đặc biệt `[^A-Za-z0-9]`.
- Check bằng 3 regex riêng lẻ (không phải 1 regex tổng hợp).

**Chính sách B** — dùng cho reset password sau forgot flow:
- Length: 8+ ký tự (không có max).
- Regex tổng hợp: `/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/`
- Phải chứa: 1+ chữ thường, 1+ chữ hoa, 1+ số, 1+ ký tự trong `[@$!%*?&]` (bị giới hạn chỉ 4 ký tự đặc biệt này).

---

## 4. Address form

File: `features/addresses/validations.ts` — `createAddressSchema`.

| Field          | Kiểu    | Bắt buộc | Ràng buộc                                            |
| -------------- | ------- | -------- | ---------------------------------------------------- |
| `receiverName` | string  | Có       | Length 1–100                                         |
| `phoneNumber`  | string  | Có       | Khớp `VN_LOCAL_PHONE_REGEX`                          |
| `addressLine`  | string  | Có       | Length 1–255                                         |
| `ward`         | string  | Có       | Length 1–100                                         |
| `city`         | string  | Có       | Length 1–100                                         |
| `country`      | string  | Có       | Length 1–100                                         |
| `mapId`        | string  | Có       | Length 1–255 (id từ map service)                     |
| `latitude`     | number  | Có       | Không ràng buộc range (nên clamp `[-90, 90]` ở BE)   |
| `longitude`    | number  | Có       | Không ràng buộc range (nên clamp `[-180, 180]` ở BE) |
| `isDefault`    | boolean | Có       | —                                                    |

---

## 5. Checkout / Order forms

File: `features/orders/validations.ts`.

### 5.1. Estimate — `createEstimateSchema` (dùng cho ước lượng phí ship)

| Field | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `fullName` | string | Có | Length 1–100 |
| `phone` | string | Có | Khớp `VN_LOCAL_PHONE_REGEX` |
| `remarks` | string | Không | Length ≤ 500 |
| `addressLine` | string | Có | Length 1–255 |
| `ward` | string | Có | Length ≥ 1 |
| `city` | string | Có | Length ≥ 1 |
| `cityCode` | number | Có | **Phải ≠ `-1`** (`-1` là giá trị sentinel = "chưa chọn") |
| `wardCode` | number | Có | **Phải ≠ `-1`** |
| `latitude` | number | Có | — |
| `longitude` | number | Có | — |
| `mapId` | string | Có | — (không kiểm length) |
| `orderTime` | number | Có | Unix timestamp — BE nên check ≥ now |
| `tagIds` | number[] | Có | Array of number, **max 1 phần tử** |

### 5.2. Checkout — `createCheckoutSchema`

Extend từ Estimate + thêm invoice fields:

| Field | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `taxCode` | string \| null | Không | Length ≤ 20 |
| `identityNumber` | string \| null | Không | Length ≤ 20 |
| `receiptFullName` | string \| null | Không | Length ≤ 100 |
| `receiptPhoneNumber` | string \| null | Không | Rỗng hoặc `VN_LOCAL_PHONE_REGEX` |
| `receiptAddress` | string \| null | Không | Length ≤ 255 |
| `receiptEmail` | string \| null | Không | Rỗng hoặc email hợp lệ |
| `isExportInvoice` | boolean | Có | — |
| `invoiceType` | enum \| null | Không | `Business` hoặc `Personal` |

**Cross-field rule (`superRefine`)**:

Chỉ áp dụng khi `isExportInvoice === true`:

| `invoiceType` | Field bắt buộc phải có (trim ≠ rỗng) |
|---|---|
| `Business` | `taxCode`, `receiptEmail`, `receiptPhoneNumber` |
| `Personal` | `identityNumber`, `receiptFullName`, `receiptAddress`, `receiptEmail`, `receiptPhoneNumber` |

Nếu `isExportInvoice === false`, invoice fields được bỏ qua.

---

## 6. Recruitment form

File: `app/[locale]/(public)/recruitment/validation.ts` — `createRecruitmentRequestSchema`.

| Field | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `jobDescriptionId` | string | Có | Length ≥ 1 (id job đang apply) |
| `title` | string | Có | Length ≥ 1 (job title snapshot) |
| `coverLetter` | string | Có | Là rich text HTML — sau khi strip tag `<[^>]*>` và thay `&nbsp;` bằng space thì trim length > 0 |
| `attachments` | `{value: string}[]` | Có | Mảng object, mỗi object có field `value` là string (file id / URL) |

Schema `.strict()`.

**Note cho backend**: `coverLetter` là HTML từ rich-text editor. Cần sanitize để tránh XSS trước khi lưu / render lại.

---

## 7. Ghi chú & câu hỏi cần xác nhận

1. **Hai chính sách mật khẩu**:
   - Sign-up (`change_password`, `register`) đang dùng chính sách A (6–128 ký tự, cần A-Z + digit + special).
   - Reset password sau forgot flow đang dùng chính sách B (8+ ký tự, chỉ chấp nhận 4 ký tự đặc biệt `@$!%*?&`).
   - **Backend nên chọn 1 chính sách áp dụng cho tất cả endpoint** — hoặc thông báo cho FE để đồng bộ.

2. **Hai regex số điện thoại**:
   - Address, order, profile: `/^0\d{9,10}$/` (đơn giản, chấp nhận cả đầu số cũ).
   - Contact: `/^(?:\+84|0)[3-9]\d{8}$/` (chặt hơn theo chuẩn nhà mạng mới).
   - Backend nên áp regex chặt cho contact (để lọc spam) và regex đơn giản cho profile / order (để không loại nhầm user cũ).

3. **Sentinel `-1`** trong `cityCode` / `wardCode`: là quy ước "chưa chọn" ở FE. BE có thể yêu cầu số > 0 để chắc chắn.

4. **Error response format**: FE dùng `react-hook-form` — cần error trả về **field-level** để hiển thị cạnh input. Backend không cần trả i18n key, chỉ cần `{ field: 'email', message: '...' }` (hoặc tương đương). Nếu backend trả kèm key trung tính (vd. `EMAIL_INVALID`), FE có thể map sang message i18n phía FE.

5. **Trim ở backend**: FE trim tất cả string trước khi validate. Backend nên trim đầu vào để tránh trường hợp FE bypass gửi chuỗi có whitespace.

6. **Rate limit**: các form public (contact, forgot-password, resend verify code, register) nên có rate limit ở backend (FE không kiểm).
