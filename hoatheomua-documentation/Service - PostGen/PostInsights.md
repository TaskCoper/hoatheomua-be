# Chỉ số bài đã đăng — Facebook và Instagram

## API

`GET /api/v1/post-gen/publications/{publicationId}/insights`

Quyền: Admin, cùng chính sách với API đăng bài. Dùng `publicationId` từ API publish hoặc danh sách publications của bài/nền tảng. Với môi trường có reverse proxy, thêm prefix `/ecommerce-api` như các API hiện có.

Chỉ đọc lần đăng `Succeeded` có `ExternalPostId`. Không nhận external ID hay access token từ client. Backend lấy tài khoản gắn với snapshot lần đăng, kiểm tra tài khoản hiện tại và giải mã credential phía server. Zalo chưa hỗ trợ. Không phụ thuộc `postgen.enabled`, image instruction hoặc trạng thái bật/tắt worker đăng bài.

Mỗi GET đọc trực tiếp từ Meta, dùng Graph API version hiện đang cấu hình và timeout `PlatformAccountOptions.RequestTimeoutSeconds`. Không tạo bài, cập nhật trạng thái lần đăng, tự refresh token, ghi snapshot chỉ số vào DB hoặc retry request. Response có `Cache-Control: no-store`. Không cần migration.

## Response

Envelope `ApiResponse` hiện có. Ví dụ `value` cho Instagram (số liệu minh họa):

```json
{
  "publicationId": "00000000-0000-0000-0000-000000000001",
  "basePostId": "00000000-0000-0000-0000-000000000002",
  "platformAccountId": "00000000-0000-0000-0000-000000000003",
  "platform": "instagram",
  "externalPostId": "18000000000000001",
  "externalPostUrl": "https://www.instagram.com/p/example/",
  "fetchedAt": "2026-09-17T10:00:00Z",
  "period": "lifetime",
  "apiVersion": "v25.0",
  "isPartial": false,
  "metrics": {
    "views": 1200,
    "reach": 800,
    "likes": 50,
    "comments": 7,
    "saved": 10,
    "shares": 3,
    "total_interactions": 70
  },
  "metricDetails": {
    "views": { "value": 1200, "status": "Available", "source": "views", "reason": null },
    "reach": { "value": 800, "status": "Available", "source": "reach", "reason": null },
    "likes": { "value": 50, "status": "Available", "source": "likes", "reason": null },
    "comments": { "value": 7, "status": "Available", "source": "comments", "reason": null },
    "saved": { "value": 10, "status": "Available", "source": "saved", "reason": null },
    "shares": { "value": 3, "status": "Available", "source": "shares", "reason": null },
    "total_interactions": { "value": 70, "status": "Available", "source": "total_interactions", "reason": null }
  }
}
```

Tất cả số đếm là `int64` nullable. `0` chỉ được trả khi Meta cung cấp giá trị 0. Dữ liệu thiếu, chưa có hoặc không hỗ trợ trả `null`; FE hiển thị “Chưa có dữ liệu/Không hỗ trợ”, không đổi thành 0. `isPartial=true` nếu có ít nhất một metric null (bao gồm Facebook saved).

`metricDetails` luôn có đủ bảy key tương ứng:

| Status | Ý nghĩa |
|---|---|
| Available | Số đếm do Meta cung cấp |
| Derived | Backend tính từ các số đếm đầy đủ; công thức nằm trong source |
| Unavailable | Meta không trả dữ liệu (`META_NO_DATA`) hoặc từ chối metric (`META_METRIC_UNAVAILABLE`) |
| Unsupported | Chỉ số không có mapping trong API nền tảng đang tích hợp |

## Mapping và định nghĩa

| Metric | Instagram | Facebook |
|---|---|---|
| views | insights `views` | insights `post_media_view` |
| reach | insights `reach` | insights `post_total_media_view_unique`: số người xem media duy nhất, mapping thay thế cho reach cũ |
| likes | insights `likes` | `reactions.type(LIKE).summary.total_count`, chỉ LIKE |
| comments | insights `comments` | `comments.summary.total_count` |
| saved | insights `saved` | null, `Unsupported`, `FACEBOOK_POST_SAVED_NOT_SUPPORTED` |
| shares | insights `shares` | `shares.count`; thiếu field trả null |
| total_interactions | insights `total_interactions`, giữ nguyên số Meta trả | Tổng **mọi reactions + comments + shares**, status `Derived`; không bao gồm saved hoặc clicks |

Không dùng impressions thay views, không dùng engaged users làm số lần tương tác, không cộng reach theo ngày. Tổng Facebook có thể lớn hơn likes + comments + shares vì reactions bao gồm LOVE, CARE, v.v. Tổng Instagram không tự tính lại vì định nghĩa/provider có thể khác phép cộng trên UI.

Instagram: xác minh media tồn tại bằng `GET /{mediaId}?fields=id`, rồi đọc bảy metric lifetime riêng biệt. Facebook: đọc summary của post và hai metric lifetime riêng biệt. Metric không hợp lệ không làm mất các metric khác; lỗi quyền/token/rate limit/transport/malformed response trả lỗi toàn request. Không đi theo paging URL do Meta trả về.

## Quyền và vận hành

- Facebook: `pages_read_engagement`, `read_insights`; Page token của người có task ANALYZE trên Page.
- Instagram qua Facebook Login: `pages_read_engagement`, `instagram_basic`, `instagram_manage_insights` và quyền truy cập media của chính tài khoản đã đăng.
- OAuth không có `ConfigId`: yêu cầu thêm scope Insights khi connect/reconnect. Scope này không trở thành điều kiện bắt buộc để kết nối hoặc đăng bài.
- Facebook Login for Business dùng `ConfigId`: phải thêm quyền tương ứng trong cấu hình ứng dụng Meta, hoàn tất App Review/Advanced Access nếu cần, rồi reconnect tài khoản. Backend không tự sửa cấu hình Meta.
- Tài khoản kết nối trước đây có thể chưa có các scope mới; API trả `409 INSIGHTS_PERMISSION_REQUIRED` với danh sách cần cấp.
- Các metric phụ thuộc media type, Graph version và khả năng cung cấp của Meta. `fetchedAt` là thời gian backend nhận dữ liệu, không phải cam kết dữ liệu Meta cập nhật tức thì.

## Lỗi

| HTTP | Code/trường hợp |
|---|---|
| 401/403 | Chưa đăng nhập / không có quyền Admin |
| 404 | Publication không tồn tại |
| 400 | `INSIGHTS_PLATFORM_UNSUPPORTED` (Zalo) |
| 409 | `INSIGHTS_PUBLICATION_NOT_PUBLISHED`, `INSIGHTS_PUBLICATION_INVALID` |
| 409 | `INSIGHTS_PERMISSION_REQUIRED`, `PLATFORM_REAUTH_REQUIRED`, `PLATFORM_TOKEN_UNAVAILABLE`, `PLATFORM_ACCOUNT_MISMATCH`, `PLATFORM_REFRESH_IN_PROGRESS` |
| 409 | Meta từ chối token/quyền: `META_TOKEN_INVALID`, `META_PERMISSION_DENIED` |
| 429 | `META_RATE_LIMITED`; không tự retry |
| 502 | Meta trả dữ liệu sai, media bị xóa/không truy cập được hoặc lỗi upstream |
| 503 | `INSIGHTS_NOT_CONFIGURED` |
| 504 | `META_TIMEOUT` |

Không trả token, URL chứa token, nội dung lỗi thô của Meta cho client.

## Nguồn và kiểm chứng

- [Meta Page/Post Insights](https://developers.facebook.com/docs/graph-api/reference/insights): `post_media_view`, `post_total_media_view_unique`, lifetime và quyền Insights. Đọc được tài liệu chính thức ngày 17/09/2026.
- [Meta Instagram insights SDK](https://github.com/facebook/facebook-python-business-sdk/blob/main/facebook_business/adobjects/instagraminsightsresult.py): tên metric và hai dạng `values` / `total_value`.
- [Meta Post SDK](https://github.com/facebook/facebook-python-business-sdk/blob/main/facebook_business/adobjects/post.py): post fields/edges.
- [Instagram media insights reference](https://developers.facebook.com/docs/instagram-platform/instagram-graph-api/reference/ig-media/insights/).

Test HTTP provider giả lập nằm tại `HoaTheoMua.Service.Test/PostPublishingTest/Insights.cs`: mapping, lifetime, 0/null, partial metrics, permissions, snapshot/tài khoản sai, ID sai, lỗi Meta và bảo vệ credential. Chưa xác nhận số liệu thật trên Meta; cần publication đã đăng thành công và tài khoản đã cấp quyền Insights để smoke sau deploy.

Kiểm chứng local ngày 17/09/2026: build `HoaTheoMua.API` thành công; **179/179 test** của `PostPublishingTest` và `PlatformAccountTest` (loại các test PostgreSQL) pass. Trong đó có 33 ca Insights mới. Không gọi Meta thật, không publish hoặc sửa cấu hình production trong lần triển khai API này.
