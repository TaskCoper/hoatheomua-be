# Quản lý ratio preset qua API Config

Cập nhật 2026-09-14: đã bỏ `GET` và `PUT /api/v1/post-gen/presets`. Dùng API Config chung để quản lý cấu hình tỷ lệ theo nền tảng.

## Bản ghi và quyền

- Bảng `Configs`: `Group = postgen`, `Key = postgen.ratio-preset`, `Kind = Setting`, `IsPublic = false`.
- `Value` là JSON object theo tên nền tảng, dùng chung toàn hệ thống. `UserId` do Config.Service gán theo người tạo, không dùng để lọc preset; bản ghi cũ có `Guid.Empty` vẫn dùng được.
- Các API quản trị dưới đây dùng `AdminPolicy`. Staff không có quyền đọc/sửa preset qua các API này. Chuẩn nền tảng cũng dùng [API Config chung](PlatformConfig.md); hai API chuẩn riêng đã bỏ.
- Partial unique index bảo đảm tối đa một bản ghi preset chưa xóa theo Group/Key. Không cần đổi schema hoặc chuyển dữ liệu cũ.

## Luồng thao tác

| Thao tác | API | Hành vi |
| --- | --- | --- |
| Tìm preset | `GET /api/v1/configs/settings?groupName=postgen&key=postgen.ratio-preset` | Trả danh sách Config trong `value`; mỗi phần tử có `id` và `value` chứa preset. Chưa có trả danh sách rỗng. |
| Đọc theo id | `GET /api/v1/configs/{id}` | Trả bản ghi Config trong `value`. |
| Tạo | `POST /api/v1/configs` | Dùng khi chưa có; trả bản ghi Config và `id`. Tạo trùng key bị từ chối. |
| Cập nhật | `PUT /api/v1/configs/{id}` | Thay toàn bộ `value`, không merge theo nền tảng; trả thông báo thành công. |
| Xóa | `DELETE /api/v1/configs/{id}` | Xóa mềm bản ghi. |

GET settings lọc group/key bằng phép chứa chuỗi, nên phía gọi chọn bản ghi có `group` và `key` khớp chính xác. Lấy `id` từ bản ghi này để cập nhật hoặc xóa. Các response dùng envelope Config hiện có.

## Payload

Ví dụ body cho POST (dữ liệu minh họa):

```json
{
  "group": "postgen",
  "key": "postgen.ratio-preset",
  "kind": "Setting",
  "value": {
    "facebook": {
      "ratios": ["Square1x1", "Landscape16x9"],
      "imageSelectionMode": "all_images"
    },
    "instagram": {
      "ratios": ["Portrait4x5", "Story9x16"],
      "imageSelectionMode": "first_n",
      "imageCount": 3
    }
  }
}
```

PUT dùng cùng các trường trên và thêm `"isPublic": false` ở cấp ngoài cùng. POST tự đặt `IsPublic = false`. Gửi đủ các nền tảng muốn giữ vì PUT thay toàn bộ JSON. `value: {}` làm rỗng lựa chọn nhưng giữ bản ghi; DELETE xóa mềm bản ghi.

## Quy ước nội dung và giới hạn hiện tại

Shape dành cho PostGen: key nền tảng `facebook`, `instagram`, `zalo`; mỗi nền tảng có 1–5 mã tỷ lệ không trùng trong `Square1x1`, `Portrait4x5`, `Portrait3x4`, `Story9x16`, `Landscape16x9`. `imageSelectionMode` là `all_images` (bỏ `imageCount`) hoặc `first_n` (`imageCount` nguyên dương).

API Config chung hiện lưu JSON theo contract Config, **không validate shape, mã tỷ lệ, trùng tỷ lệ hoặc điều kiện imageCount của PostGen**. Validator riêng của hai API đã bỏ cũng được xóa; không còn cam kết trả 422 cho các lỗi ratio đó. Luồng generate mốc 4 đã kiểm preset khi dùng: thiếu/sai shape, mã hoặc imageCount trả 400 trước khi gọi AI; vượt 12 cặp sau khai triển trả 422.

POST tạo và PUT sửa là hai thao tác riêng, không còn upsert nguyên tử của API preset cũ. Nếu tạo trùng, đọc lại Config để lấy id rồi cập nhật. Unique index vẫn ngăn hai bản ghi còn hiệu lực.

## Kiểm thử

`HoaTheoMua.Service.Test/ConfigTest/PostGenRatioConfig.cs` kiểm luồng tạo → tài khoản khác đọc → cập nhật thay toàn bộ JSON → xóa mềm bằng Config.Service. Test PostgreSQL kiểm unique index; HTTP probe PostGen kiểm hai route cũ trả 404.
