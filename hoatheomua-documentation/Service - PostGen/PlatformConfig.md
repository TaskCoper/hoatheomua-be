# Cấu hình PostGen theo nền tảng

Cập nhật 2026-09-14: bỏ các key `postgen.platform.*`. Mỗi nền tảng dùng **một Config prompt** chứa cả bật/tắt và hướng dẫn biên tập. Bảng PlatformStandard và API chuyên biệt đã bỏ từ trước.

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
    "isActive": true,
    "content": "<system prompt hoàn chỉnh của Facebook>"
  }
}
```

`value.isActive=false` tắt sinh nội dung của nền tảng. `value.content` chứa đầy đủ vai trò, giọng văn, bố cục, độ dài và hashtag; gửi nguyên văn cho AI. Không có Config chuẩn thứ hai hoặc tham số maxLength lấy từ chuẩn cũ. `IsDeleted` là xóa mềm bản ghi Config, khác với isActive.

Ratio preset giữ riêng tại `postgen.ratio-preset`, vì phục vụ chọn tỷ lệ/ảnh. Payload prompt hoàn chỉnh ở [ParaphrasePromptData.md](ParaphrasePromptData.md).

## Thao tác của Admin

1. GET `/api/v1/configs/settings?groupName=postgen&key=postgen.system-prompt.facebook`.
2. Bộ lọc là tìm chuỗi: chọn bản ghi có group/key khớp chính xác, lấy id.
3. Chưa có thì POST `/api/v1/configs`; đã có thì PUT `/api/v1/configs/{id}` với body đầy đủ như trên. PUT thay toàn bộ value, phải giữ cả content và isActive.
4. GET/DELETE `/api/v1/configs/{id}` để đọc/xóa mềm. Muốn tắt tạm thời, PUT isActive=false.

Quản trị Config vẫn chỉ Admin. API Config dùng validation chung; generate kiểm content không rỗng và isActive là boolean trước khi gọi AI. Thiếu/sai prompt trả 400; isActive=false trả `Platform {platform} is disabled`. Không fallback về prompt Post cũ.

## Migration gộp cấu hình

`20260914073414_ConsolidatePostGenPlatformConfigs`:

- Giữ nguyên ID, nội dung prompt, tham chiếu GeneratedPost và PromptSnapshot.
- Prompt đã có isActive boolean thì giữ giá trị đó. Nếu chưa có, lấy từ standard active cũ; standard đã xóa thì false; hoàn toàn chưa có standard thì true.
- Thêm isActive vào các prompt còn hiệu lực. JSON đích hoặc trạng thái nguồn không hợp lệ làm migration dừng nguyên transaction trước khi xóa.
- Xóa toàn bộ Config có Group=postgen và Key bắt đầu bằng `postgen.platform.`, kể cả lịch sử xóa mềm; bỏ index `IX_Configs_PostGenPlatforms`. Không sửa Config nhóm khác hoặc ratio preset.
- Các description/giới hạn cũ được loại bỏ theo quyết định gộp; không tự chèn vào prompt tùy chỉnh. Prompt seed đã có hướng dẫn biên tập đầy đủ.

Migration cũ vẫn được giữ để tái tạo lịch sử schema. Migration mới chỉ chạy tiến; Down dừng và yêu cầu khôi phục backup trước migration, vì không thể phục hồi chính xác mô tả/giới hạn đã bỏ. Chưa áp dụng lên database triển khai.
