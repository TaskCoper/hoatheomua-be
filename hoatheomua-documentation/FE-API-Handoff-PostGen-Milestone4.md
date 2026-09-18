# Bàn giao FE — PostGen mốc 4: sinh ảnh

Cập nhật 2026-09-14. Mốc 4 và mode content/images/all đã triển khai, kiểm thử và được đưa vào nhánh `feature/post-gen`; chưa deploy. Dùng cùng API và quyền Staff/Admin của mốc 3; không thêm route. Mốc 5 (version nội dung) đã triển khai; xem [bàn giao mốc 5](FE-API-Handoff-PostGen-Milestone5.md). Mốc 6 (version/chốt ảnh) đã triển khai trên nhánh `feature/post-gen`: [bàn giao mốc 6](FE-API-Handoff-PostGen-Milestone6.md). Mốc 7 payload official riêng đã triển khai trên nhánh `feature/post-gen`: [bàn giao mốc 7](FE-API-Handoff-PostGen-Milestone7.md).

## Chọn phần cần sinh bằng `mode`

Cùng endpoint `POST /api/v1/posts/{id}/generate`, `mode` là chuỗi `content`, `images` hoặc `all` (viết thường). Bỏ field thì mặc định `all` để tương thích request cũ; gửi null hoặc giá trị khác trả 422.

| Mode | Xử lý |
| --- | --- |
| `content` | Chỉ gọi AI sinh nội dung, tạo version và chốt nội dung mới. Giữ nguyên ảnh/slot và thứ tự đã chốt. Không gửi ratios/usePreset=true; nếu gửi trả 422. Không cần Config chỉ dẫn ảnh/preset. |
| `images` | Chỉ gọi AI sinh ảnh. Không thêm GeneratedPost, không đổi nội dung, OfficialGenerationId hoặc SelectedAt. Mỗi nền tảng phải có ratios hoặc usePreset=true; thiếu trả 422. |
| `all` (mặc định) | Sinh nội dung trước, rồi ảnh theo lựa chọn. Không có lựa chọn ảnh thì chỉ sinh nội dung như request cũ. |

Hai lần gọi tách rời để tránh sinh nội dung hai lần:

```json
{
  "mode": "content",
  "platforms": [{ "platform": "facebook" }]
}
```

```json
{
  "mode": "images",
  "platforms": [{
    "platform": "facebook",
    "ratios": [{ "ratioCode": "Story9x16", "basePostImageIds": [] }]
  }]
}
```

Images mode vẫn kiểm postgen.enabled và isActive trong Config nền tảng, cùng chỉ dẫn ảnh/preset khi dùng; không yêu cầu hoặc gửi value.content của prompt nội dung. Có thể sinh ảnh trước khi có nội dung: platform.content vẫn null, chưa sẵn sàng đăng. Thành công/lỗi ảnh chỉ cập nhật trạng thái xử lý, ảnh và lastError; nội dung chốt giữ nguyên kể cả khi mọi ảnh lỗi. Hủy request hoặc ghi DB lỗi vẫn giải phóng claim và giữ dữ liệu đã chốt trước.

Mode áp dụng cho toàn bộ request. Các mode cùng dùng khóa/claim của nền tảng để chặn yêu cầu sinh trùng; không gọi content và images đồng thời cho cùng nền tảng. Không tự retry POST.

## Request

`POST /api/v1/posts/{basePostId}/generate`

```json
{
  "platforms": [
    {
      "platform": "facebook",
      "ratios": [
        {
          "ratioCode": "Portrait4x5",
          "basePostImageIds": ["22222222-2222-4222-8222-222222222222"]
        }
      ]
    },
    { "platform": "instagram", "usePreset": true }
  ]
}
```

Dùng ID ảnh nguồn từ `baseImages`. Khi mode mặc định/all, request chỉ có `platform` vẫn sinh nội dung, không sinh ảnh. Khi chọn ảnh, dùng một trong hai cách:

- `ratios`: 1–5 tỷ lệ không trùng. `basePostImageIds` bỏ trống, null hoặc `[]` nghĩa là tất cả ảnh nguồn còn hiệu lực; nếu có ID thì tối đa 20, không trùng, thuộc đúng bài và chưa xóa.
- `usePreset: true`: đọc Config `postgen.ratio-preset` của nền tảng đó. Không gửi `ratios` đồng thời. `all_images` lấy mọi ảnh nguồn; `first_n` lấy N ảnh đầu theo `sortOrder`.

Mã tỷ lệ: `Square1x1`, `Portrait4x5`, `Portrait3x4`, `Story9x16`, `Landscape16x9`. Tối đa **12 cặp ảnh nguồn × tỷ lệ khác nhau** trên toàn bộ request. Hai nền tảng cùng cặp dùng chung một lần sinh ảnh.

## Response và ảnh chốt

Response vẫn là `ApiResponse<PostCoreDetail>`:

- `baseImages`: ảnh nguồn.
- `platforms[].content`: một object nội dung chốt hoặc null.
- `platforms[].images`: chỉ ảnh thành công đã chốt, đúng `sortOrder`.
- Không có `value.images` hoặc `platforms[].versions`.

Một ảnh trong `platforms[].images`:

```json
{
  "sourceBasePostImageId": "22222222-2222-4222-8222-222222222222",
  "ratioCode": "Portrait4x5",
  "selectedGeneratedPostImageId": "33333333-3333-4333-8333-333333333333",
  "imageUrl": "https://example.com/generated-flower.png",
  "pixelWidth": 1080,
  "pixelHeight": 1350,
  "sortOrder": 1
}
```

`pixelWidth/pixelHeight` là kích thước mục tiêu đã yêu cầu AI theo bảng tỷ lệ; proxy chỉ trả URL, backend chưa tải ảnh để xác minh kích thước thực tế. URL được kiểm cú pháp HTTP/HTTPS; độ bền URL và chất lượng giữ sản phẩm chưa được xác minh bằng provider thật trong đợt này.

Ô ảnh chưa chọn sẽ tự chọn kết quả thành công. Nếu đã chọn ảnh từ lần trước thì giữ ID đó; lần sinh mới vẫn được lưu trong lịch sử. Các ô cũ và thứ tự của chúng giữ nguyên; cặp mới nối tiếp cuối danh sách. Muốn chọn bản khác dùng PUT platform images của mốc 6, đã triển khai trên nhánh `feature/post-gen`.

## Trạng thái và lỗi

| Trường hợp | HTTP / hành vi |
| --- | --- |
| Nội dung và ảnh yêu cầu thành công | 200, `Generated` (có thể `Partial` nếu còn ô cũ chưa chốt) |
| Một số ảnh lỗi | 200, `Partial`, `lastError`; ảnh thành công và nội dung vẫn lưu |
| Tất cả ảnh yêu cầu của một nền tảng lỗi | 200, nền tảng đó `Failed`, `lastError`; nội dung chỉ được sinh/chốt khi mode content/all; mode images giữ nội dung cũ, ảnh đã chốt trước vẫn giữ |
| Sai shape, tỷ lệ/ID trùng, dùng đồng thời preset và ratios, vượt 12 cặp | 422 trước khi gọi AI |
| ID ảnh sai bài/đã xóa, thiếu hoặc sai preset/chỉ dẫn Config | 400 trước khi gọi AI |
| Bài đang sinh hoặc trạng thái không cho phép | 409 |
| Một lời gọi nội dung lỗi (content/all) | 400/502; không sinh ảnh và không lưu version mới trong request |
| Heavy rate limit | 429, đọc `Retry-After` |

FE đọc trạng thái từng nền tảng kể cả khi HTTP 200; `isSuccess` không có nghĩa mọi ảnh đều thành công. Không tự retry POST vì có thể phát sinh chi phí và version mới. Không có SSE/job/background hoặc API hủy riêng.

Tổng ngân sách AI 180 giây, tối đa 3 lời gọi ảnh đồng thời; timeout mỗi lời gọi mặc định 120 giây. Hết ngân sách trong pha ảnh: lưu kết quả đã có, phần chưa xong đánh dấu lỗi; dành tối đa 10 giây riêng để lưu DB. Client timeout cần lớn hơn 190 giây cộng độ trễ mạng/DB ban đầu. Hủy request trước khi lưu làm bỏ kết quả mới và giải phóng trạng thái; nếu client mất kết nối sát lúc commit, GET lại bài để xác định kết quả.

## Config và kiểm tra FE

Chỉ dẫn ảnh chung nằm ở Config `postgen.image_instruction`, group `postgen`, kind `Setting`, private; `value` là **JSON string**, không phải object `{content}`. Ba prompt nền tảng vẫn chỉ dùng cho nội dung. Admin quản trị bằng API Config chung.

Các luồng cần kiểm: chọn cùng ảnh/tỷ lệ trên hai nền tảng; kết hợp preset và chọn tay; ảnh lỗi một phần; gọi lại giữ ảnh đã chốt; vượt 12 cặp; reload vẫn có nội dung, URL ảnh và trạng thái đúng. Lịch sử/sinh lại theo feedback/chọn version nội dung đã có ở mốc 5; lịch sử/chọn ảnh và đăng bài chưa triển khai.

Từ mốc 6, FE có thể đọc kho ảnh bằng GET images và PUT lựa chọn phiên bản đã có để dùng lại giữa các nền tảng, không gọi AI. POST images/regenerate chỉ thêm version vào kho, không tự thay ô chốt.
