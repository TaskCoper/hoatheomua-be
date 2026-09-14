# Chuẩn nền tảng qua API Config

Cập nhật 2026-09-14: bỏ bảng `PlatformStandard`, `GET /api/v1/platform-standards` và `PUT /api/v1/platform-standards/{id}`. PostGen còn 5 API Post Core; chuẩn nền tảng và ratio preset dùng API Config chung.

## Dữ liệu

Mỗi nền tảng một bản ghi `Configs`, `Group = postgen`, `Kind = Setting`, `IsPublic = false`:

| Nền tảng | Key |
| --- | --- |
| Facebook | `postgen.platform.facebook` |
| Instagram | `postgen.platform.instagram` |
| Zalo | `postgen.platform.zalo` |

Ratio giữ riêng trong `postgen.ratio-preset`. Key xác định nền tảng; `Value` không chứa id hay platform lặp lại. `IsDeleted` là metadata Config, khác với `value.isActive` bật/tắt nền tảng.

Ví dụ POST `/api/v1/configs` (dữ liệu minh họa):

```json
{
  "group": "postgen",
  "key": "postgen.platform.instagram",
  "kind": "Setting",
  "value": {
    "description": "Caption ngắn gọn, giàu cảm xúc, hashtag phù hợp.",
    "maxContentLength": 2200,
    "maxHashtagCount": 30,
    "maxImageCount": 10,
    "isActive": true
  }
}
```

`description` là chuỗi dùng ghép vào prompt sinh nội dung; `isActive` là boolean. Ba giới hạn là số nguyên hoặc null (chưa cấu hình giới hạn). Các giá trị ví dụ không phải cam kết giới hạn hiện tại của nền tảng.

## Thao tác của Admin

Tất cả API quản trị giữ `AdminPolicy`; không thêm quyền Staff hoặc mở đọc công khai.

1. Đọc `GET /api/v1/configs/settings?groupName=postgen&key=postgen.platform.instagram`.
2. Response envelope `value` chứa danh sách Config. Chọn bản ghi có `group`/`key` khớp chính xác vì bộ lọc hiện dùng phép chứa chuỗi; lấy `id` và JSON trong trường `value` của bản ghi.
3. Chưa có thì POST với body như trên. Đã có thì PUT `/api/v1/configs/{id}`, dùng cùng body và thêm `"isPublic": false` ở cấp ngoài cùng.
4. Đọc theo id qua GET `/api/v1/configs/{id}`; xóa mềm qua DELETE cùng đường dẫn.

PUT thay toàn bộ `Value` của một nền tảng, không ảnh hưởng nền tảng khác hoặc ratio preset. Config.Service hiện không validate riêng shape chuẩn nền tảng; validator của API cũ đã bỏ. Khi triển khai generate, cần kiểm cấu hình hợp lệ trước khi sử dụng. API vẫn có contract, response và cơ chế revalidation chung của Config.

## Migration và rollback

`20260914055741_MovePlatformStandardsToConfig` chạy sau `AddPostGenCore`. Lịch sử migration cũ được giữ nguyên; database mới được seed bảng cũ rồi chuyển thành Config trong chuỗi migration.

- Up khóa bảng nguồn và đích, kiểm xung đột ID hoặc key (kể cả bản ghi Config đã xóa), chuyển toàn bộ dòng rồi drop bảng trong transaction. Không ghi đè Config đã có.
- Giữ nguyên ID, mô tả, giới hạn nullable, trạng thái active/deleted, CreatedAt và UpdatedAt. Config được chuyển có `UserId = Guid.Empty`, `Kind = Setting`, `IsPublic = false`; bản ghi tạo mới bằng API dùng UserId của Admin.
- Unique index `IX_Configs_PostGenPlatforms` bảo đảm mỗi key trong ba key trên chỉ có một dòng chưa xóa trong group `postgen`. Không thay index ratio preset hoặc card_config.
- Down dựng lại bảng từ ba key này, giữ dữ liệu hiện tại rồi xóa các Config đã chuyển ngược. Nếu JSON thiếu trường, sai kiểu, có trường bổ sung, giới hạn không chuyển được sang integer hoặc có nhiều bản ghi cùng nền tảng (kể cả lịch sử xóa mềm), Down dừng và rollback. Cần đối soát dữ liệu trước khi thử lại; không tự chọn bản ghi hay thay bằng mặc định.

Không thay native enum PlatformType, schema bài viết hoặc ratio preset. Cần cập nhật client sử dụng API Config trước khi triển khai phiên bản bỏ route; thực hiện migration trong cửa sổ triển khai vì có khóa bảng và phiên bản API cũ còn đọc bảng đã bỏ.

## Kiểm thử

- `PlatformConfigMigration.cs`: chuyển dữ liệu đã chỉnh, nullable, inactive, deleted, metadata; xung đột ID/key; unique index; Up/Down; JSON sai và lịch sử trùng làm rollback dừng nguyên tử.
- `PostGenPostgres.cs`: toàn bộ chuỗi migration từ database mới, model snapshot, seed Config và các ràng buộc Post Core.
- `Verification/Program.cs`: HTTP Admin CRUD Config, đọc từ Admin khác, độc lập giữa nền tảng, key trùng, không công khai và route cũ 404. Chỉ dependency revalidation bên ngoài được stub.

Probe có tùy chọn `--migrate` để áp dụng migration trước khi chạy trên database PostgreSQL cục bộ dành riêng cho test. Không dùng tùy chọn này với dữ liệu thật.
