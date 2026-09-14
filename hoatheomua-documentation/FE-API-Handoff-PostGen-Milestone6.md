# Bàn giao FE — PostGen mốc 6: version ảnh và ảnh chốt

Cập nhật 2026-09-14. Đã triển khai trên nhánh `feature/post-gen`, chưa deploy. Dùng JWT Staff/Admin, bài dùng chung. Chỉ POST regenerate có heavy rate limit và gọi AI. Không thêm migration.

## API

| Method | Route | Kết quả |
| --- | --- | --- |
| POST | `/api/v1/posts/{basePostId}/images/regenerate` | Một phiên bản ảnh mới, không thay ảnh chốt |
| GET | `/api/v1/posts/{basePostId}/images` | Kho ảnh, gom theo ảnh nguồn × tỷ lệ, gồm cả lần lỗi |
| PUT | `/api/v1/posts/{basePostId}/platforms/{platform}/images` | Thay toàn bộ tập ô ảnh/phiên bản/thứ tự của một nền tảng |

Platform: `facebook`, `instagram`, `zalo`. Ratio: `Square1x1`, `Portrait4x5`, `Portrait3x4`, `Story9x16`, `Landscape16x9` (xem enum/preset hiện có). POST/GET kho ảnh không yêu cầu đã có PlatformPost. PUT yêu cầu nền tảng đã được tạo qua generate, nếu chưa có trả 404.

## Sinh lại một ảnh

```json
{
  "sourceBasePostImageId": "11111111-1111-4111-8111-111111111111",
  "ratioCode": "Square1x1",
  "sourceGeneratedPostImageId": "22222222-2222-4222-8222-222222222222",
  "feedback": "Nền sáng hơn, giữ nguyên bó hoa"
}
```

- SourceBasePostImageId bắt buộc là ảnh nguồn còn hiệu lực của bài. Ratio bắt buộc là giá trị enum hợp lệ.
- Bỏ/null `sourceGeneratedPostImageId`: dựng lại từ ảnh gốc. Gửi ID: sửa tiếp trên đúng version thành công của cùng cặp ảnh nguồn × tỷ lệ; có thể rẽ nhánh từ bản cũ. ID rỗng không hợp lệ.
- Feedback tùy chọn, tối đa 2000 ký tự; chỉ feedback hiện tại được gửi, không cộng dồn. Shared instruction đọc từ Config `postgen.image_instruction`; không dùng prompt nội dung của nền tảng. Cờ `postgen.enabled` vẫn có hiệu lực.
- Khi có parent, AI nhận theo thứ tự: nhãn ảnh gốc → ảnh gốc đối chiếu → nhãn bản được chọn → ảnh phiên bản → chỉ dẫn chung → feedback hiện tại (nếu có) → yêu cầu tạo ảnh. SourceImageUrl trong lịch sử là ảnh thực sự được chỉnh sửa; parentGeneratedPostImageId trỏ đúng bản nguồn.
- Mỗi POST hợp lệ chủ động sinh một phiên bản mới; không tự retry. Không đổi PlatformPost.Status, content, SelectedAt hoặc bất kỳ ô ảnh chốt nào, kể cả khi AI lỗi.

Response HTTP 200 dùng envelope hiện tại, `value` là một ImageVersion (shape dưới đây). **FE phải kiểm tra value.status**: Succeeded có URL; Failed có errorMessage và URL null. Lỗi provider/timeout trong lúc tạo ảnh được lưu thành một lần Failed và trả 200, để FE xem lại trong kho. Cấu hình/đầu vào lỗi trước khi gọi AI vẫn trả 4xx/5xx bình thường. Không dùng ảnh Failed làm parent hoặc chốt.

Request đồng bộ; AI timeout mặc định 120 giây, budget 180 giây và tối đa 10 giây lưu kết quả. Client hủy trước persistence không ghi version. Nếu bài hoặc nguồn bị xóa trong lúc AI chạy, backend kiểm lại khi lưu và từ chối 404/400, không gắn kết quả vào nguồn đã xóa. Client mất kết nối sát commit nên GET lịch sử để đối chiếu trước khi bấm lại.

## Xem lịch sử

GET trả `ApiResponse<ImageHistory>`. Ví dụ phần `value`:

```json
{
  "basePostId": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa",
  "groups": [
    {
      "sourceBasePostImageId": "11111111-1111-4111-8111-111111111111",
      "originalImageUrl": "https://example.com/source.jpg",
      "sourceIsDeleted": false,
      "ratioCode": "Square1x1",
      "versions": [
        {
          "generatedPostImageId": "33333333-3333-4333-8333-333333333333",
          "sourceBasePostImageId": "11111111-1111-4111-8111-111111111111",
          "ratioCode": "Square1x1",
          "parentGeneratedPostImageId": "22222222-2222-4222-8222-222222222222",
          "versionNo": 2,
          "sourceImageUrl": "https://example.com/v1.png",
          "imageUrl": "https://example.com/v2.png",
          "pixelWidth": 1080,
          "pixelHeight": 1080,
          "status": "Succeeded",
          "errorMessage": null,
          "feedback": "Nền sáng hơn",
          "createdAt": "2026-09-14T10:00:00Z",
          "selectedPlatforms": ["facebook"]
        }
      ]
    }
  ]
}
```

Ví dụ rút gọn chỉ hiển thị v2; response thực tế gồm mọi version của nhóm. `versionNo` tính riêng cho mỗi cặp, từ 1, thứ tự CreatedAt/Id tăng dần; bao gồm Failed. Dùng generatedPostImageId làm định danh khi thao tác. CreatedAt của ảnh mới được gán khi lưu dưới khóa bài, để lượt AI bắt đầu sớm nhưng kết thúc muộn không chen trước các số version đã trả.

`selectedPlatforms` liệt kê các nền tảng đang chốt version đó, không gồm nền tảng đã xóa. Các version chưa chọn có mảng rỗng. Nhóm chỉ xuất hiện khi đã có ít nhất một lần tạo ảnh; chưa có lịch sử thì groups=[]. Không phân trang ở mốc này. Nhóm của ảnh nguồn đã xóa vẫn được trả với sourceIsDeleted=true để xem lịch sử; FE không cho chọn/sinh lại trên nguồn đã xóa.

## Chọn phiên bản và sắp xếp ảnh

```json
{
  "images": [
    {
      "sourceBasePostImageId": "11111111-1111-4111-8111-111111111111",
      "ratioCode": "Square1x1",
      "selectedGeneratedPostImageId": "33333333-3333-4333-8333-333333333333",
      "sortOrder": 1
    }
  ]
}
```

- Đây là PUT toàn bộ tập ô: phải gửi `images` (0–100 ô), không có phần tử null. `[]` bỏ hết ô ảnh của nền tảng; lịch sử ảnh vẫn giữ.
- Một cặp ảnh nguồn × ratio chỉ xuất hiện một lần. SortOrder >0, không trùng, không bắt buộc liên tiếp. FE gửi sortOrder quyết định thứ tự, không dựa vào thứ tự phần tử trong JSON.
- SelectedGeneratedPostImageId phải là ảnh Succeeded, có URL, thuộc đúng bài và đúng cặp của ô. Cho phép bỏ/null để giữ ô nhưng chưa chốt version; Guid.Empty không hợp lệ. Ô chưa chốt không xuất hiện trong detail.platforms[].images.
- Đổi version, bỏ ô, thêm ô và đảo thứ tự được lưu trong một transaction có khóa bài. Lỗi validation/persistence không lưu một phần. PUT không gọi AI, không đọc Config prompt và không tạo version nội dung/ảnh mới.
- Chỉ nền tảng được chỉ định thay đổi; OfficialGenerationId và content giữ nguyên. Nếu có nội dung chốt và ít nhất một ảnh chốt → Ready; chỉ có nội dung → Generated; chưa có nội dung → Draft. LastError được xóa khi chốt thành công. Detail có thể hiển thị Partial nếu tập vẫn có ô chưa chốt.
- Chặn PUT khi nền tảng Generating/Scheduled/Publishing/Published/PublishFailed (409). Lượt regenerate ảnh chạy độc lập, không chiếm trạng thái Generating của nền tảng.
- Các PUT đồng thời được khóa tuần tự; tập của lần lưu sau có hiệu lực. Chưa có ETag phát hiện form cũ.

Response 200: `ApiResponse<PostCoreDetail>`, cùng shape GET detail. Không thêm value.images hoặc platforms[].versions; chỉ platforms[].images chốt, có URL/pixelWidth/pixelHeight/sortOrder. Sau PUT, cập nhật detail và GET lịch sử nếu UI đang mở để tải lại selectedPlatforms.

## Kịch bản FE

Facebook và Instagram đang dùng ảnh A v1. Người dùng tạo lại từ v1 → v2 xuất hiện trong kho, cả hai nền tảng vẫn dùng v1. Người dùng chọn v2 cho Facebook rồi PUT → Facebook dùng v2, Instagram vẫn v1. Buổi chiều muốn Instagram dùng lại v2 thì chọn trong kho và PUT, không gọi generate.

## Lỗi và giới hạn

| HTTP | Trường hợp |
| --- | --- |
| 401 / 403 | Chưa đăng nhập / không đủ quyền |
| 404 | Bài bị xóa/không tồn tại; PUT nền tảng chưa tồn tại |
| 400 | Nguồn/parent/ảnh chọn không thuộc bài hoặc sai cặp; Failed; nguồn đã xóa; trùng ô/thứ tự; Config ảnh thiếu hoặc PostGen tắt |
| 422 | Shape sai, thiếu field, GUID rỗng, enum không hợp lệ, feedback quá dài hoặc quá 100 ô |
| 409 | PUT vào nền tảng đang sinh hoặc thuộc trạng thái publish |
| 429 | Heavy rate limit trên POST regenerate; đọc Retry-After |
| 500 | Cấu hình AI server lỗi hoặc lỗi lưu ngoài dự kiến |

Kích thước pixel hiện vẫn là mục tiêu; backend chưa tải/đo/chuẩn hóa ảnh thực, cũng chưa chuyển URL provider sang storage riêng. Generate mốc 4 vẫn gom cặp trong cùng request; không tự tái sử dụng giữa các request. Dùng GET kho + PUT để tái sử dụng ảnh đã có.

Mốc 7 đã có API đọc payload đang chốt: [bàn giao FE mốc 7](FE-API-Handoff-PostGen-Milestone7.md).
