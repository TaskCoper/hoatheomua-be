# Cấu hình PostGen theo nền tảng

Cập nhật 2026-09-17: prompt hỗ trợ format form Config; text nằm trong `Config.Value.value`. Mỗi nền tảng dùng một Config prompt. Các key `postgen.platform.*`, bảng PlatformStandard và API chuyên biệt đã bỏ từ trước.

## Dữ liệu hiện tại

`Group = postgen`, `Kind = Setting`, `IsPublic = false`:

| Nền tảng | Key |
| --- | --- |
| Facebook | `postgen.system-prompt.facebook` |
| Instagram | `postgen.system-prompt.instagram` |
| Zalo | `postgen.system-prompt.zalo` |

```json
{
  "group": "postgen",
  "key": "postgen.system-prompt.facebook",
  "kind": "Setting",
  "isPublic": false,
  "value": {
    "group": "postgen",
    "label": "Facebook",
    "value": "<system prompt hoàn chỉnh của Facebook>",
    "options": [],
    "required": false,
    "inputType": "textarea",
    "validations": []
  }
}
```

Object form trên được lưu nguyên trong cột `Config.Value`. Backend chỉ lấy chuỗi `value` bên trong để gửi AI và lưu PromptSnapshot, giữ nguyên khoảng trắng/xuống dòng. Không gửi label/options/validations hoặc JSON form sang AI. Text phải là string không rỗng khi sinh nội dung; mode images cho phép prompt nền tảng là chuỗi rỗng vì dùng hướng dẫn ảnh riêng.

Format mới không bắt buộc `isActive`: thiếu cờ thì mặc định bật. Có thể đặt `isActive: false` trực tiếp trong object form để tắt nền tảng. Form đang dùng `description` là chuỗi JSON, ví dụ `"{\"logo\":\"https://example.com/icon.png\",\"isActive\":false}"`, cũng được đọc cờ bật/tắt. Cờ trực tiếp ưu tiên hơn description; cờ phải là boolean. Description văn bản thông thường được bỏ qua. JSON metadata bắt đầu bằng `{` nhưng không hợp lệ sẽ bị từ chối. `IsDeleted` vẫn là xóa mềm Config.

Tương thích dữ liệu cũ: `{ "isActive": true, "content": "..." }` vẫn đọc được, và vẫn yêu cầu isActive boolean. Nếu object có `value` thì luôn dùng value; không fallback sang content khi value rỗng/sai kiểu. Không fallback sang bảng SystemPrompt.

## Hướng dẫn sinh ảnh và bật/tắt chung

`postgen.image_instruction` dùng cùng format form:

```json
{
  "group": "postgen",
  "label": "Hướng dẫn tạo ảnh",
  "value": "Điều chỉnh bố cục hoặc mở rộng nền theo tỷ lệ đích. Giữ nguyên sản phẩm, chữ nhận diện thương hiệu và logo của ảnh gốc.",
  "options": [],
  "required": false,
  "inputType": "textarea",
  "validations": []
}
```

Đây là nội dung cột Config.Value, không phải toàn bộ body POST/PUT Config. Khi gọi API quản trị, đặt object này vào trường `value` của body cùng key/group/kind/isPublic bên ngoài. Generate và regenerate ảnh đều đọc chuỗi bên trong. Giá trị string thuần của dữ liệu cũ vẫn được hỗ trợ. Text null/rỗng/sai kiểu bị từ chối trước khi gọi AI.

`postgen.enabled` hỗ trợ cả boolean thuần và object form có `value: true/false`. Boolean false chặn sinh nội dung/ảnh; chuỗi `"true"` không hợp lệ. Không có bản ghi enabled thì giữ hành vi mặc định cho phép sinh. Không đổi cách đọc ratio preset.

Ratio preset giữ riêng tại `postgen.ratio-preset`, vì phục vụ chọn tỷ lệ/ảnh. Payload prompt hoàn chỉnh ở [ParaphrasePromptData.md](ParaphrasePromptData.md).

## Thao tác của Admin

1. GET `/api/v1/configs/settings?groupName=postgen&key=postgen.system-prompt.facebook`.
2. Bộ lọc là tìm chuỗi: chọn bản ghi có group/key khớp chính xác, lấy id.
3. Chưa có thì POST `/api/v1/configs`; đã có thì PUT `/api/v1/configs/{id}` với body đầy đủ như trên. PUT thay toàn bộ object value, cần giữ metadata và cờ bật/tắt nếu đang dùng.
4. GET/DELETE `/api/v1/configs/{id}` để đọc/xóa mềm. Muốn tắt tạm thời, đặt isActive=false trong object form hoặc metadata description.

Quản trị Config vẫn chỉ Admin. Thiếu/sai prompt trả 400; isActive=false trả `Platform {platform} is disabled`. Các kiểm tra đọc form mới áp dụng cho cả generate và regenerate. Không cần migration hoặc sửa dữ liệu Config hiện có.

## Migration gộp cấu hình

`20260914073414_ConsolidatePostGenPlatformConfigs`:

- Giữ nguyên ID, nội dung prompt, tham chiếu GeneratedPost và PromptSnapshot.
- Prompt đã có isActive boolean thì giữ giá trị đó. Nếu chưa có, lấy từ standard active cũ; standard đã xóa thì false; hoàn toàn chưa có standard thì true.
- Thêm isActive vào các prompt còn hiệu lực. JSON đích hoặc trạng thái nguồn không hợp lệ làm migration dừng nguyên transaction trước khi xóa.
- Xóa toàn bộ Config có Group=postgen và Key bắt đầu bằng `postgen.platform.`, kể cả lịch sử xóa mềm; bỏ index `IX_Configs_PostGenPlatforms`. Không sửa Config nhóm khác hoặc ratio preset.
- Các description/giới hạn cũ được loại bỏ theo quyết định gộp; không tự chèn vào prompt tùy chỉnh. Prompt seed đã có hướng dẫn biên tập đầy đủ.

Migration cũ vẫn được giữ để tái tạo lịch sử schema. Migration mới chỉ chạy tiến; Down dừng và yêu cầu khôi phục backup trước migration, vì không thể phục hồi chính xác mô tả/giới hạn đã bỏ. Chưa áp dụng lên database triển khai.
