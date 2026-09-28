# Báo cáo tài khoản theo khoảng ngày

`GET /ecommerce-api/v1/post-gen/platform-accounts/{accountId}/insights`

Route backend trực tiếp: `/api/v1/post-gen/platform-accounts/{accountId}/insights`. Yêu cầu JWT Admin. `accountId` là UUID tài khoản đã kết nối, lấy từ API danh sách platform-accounts; không phải publicationId hoặc ID Meta.

| Query | Quy định |
|---|---|
| fromDate | Bắt buộc, YYYY-MM-DD, ngày đầu theo UTC |
| toDate | Bắt buộc, YYYY-MM-DD, bao gồm cả ngày cuối; không vượt ngày hiện tại UTC |
| metrics | Bắt buộc, 1–10 tên metric Meta dạng số, phân cách dấu phẩy; tên phụ thuộc nền tảng/API version |
| period | Mặc định day; cho phép day, week, days_28, month, total_over_range; Meta quyết định tổ hợp hợp lệ với metric |
| metricType | Chỉ Instagram: total_value (mặc định) hoặc time_series; Facebook không truyền |

Giới hạn ứng dụng: tối đa 30 ngày mỗi request. Backend gửi `since` = đầu fromDate UTC, `until` = đầu ngày sau toDate UTC, dưới dạng Unix seconds. Khoảng ngày được echo trong response. `values[].endTime` giữ mốc thời gian từ Meta; ranh giới các bucket của Meta có thể khác UTC. Không tự cộng các bucket hoặc reach thành tổng khoảng, không tự đổi metric giữa hai nền tảng. Chỉ hỗ trợ giá trị số nguyên không âm; không yêu cầu breakdown/demographic/object-valued metrics.

Ví dụ Instagram:

```http
GET /ecommerce-api/v1/post-gen/platform-accounts/{accountId}/insights?fromDate=2026-09-01&toDate=2026-09-17&metrics=views,reach&period=day&metricType=total_value
```

Để lấy chuỗi thời gian cho metric hỗ trợ, truyền `metricType=time_series`.

Ví dụ Facebook (tên metric cần khả dụng với tài khoản và API version đang cấu hình):

```http
GET /ecommerce-api/v1/post-gen/platform-accounts/{accountId}/insights?fromDate=2026-09-01&toDate=2026-09-17&metrics=page_views_total&period=day
```

`page_views_total` là lượt xem Page, không được gán thành lượt xem nội dung bài đăng.

Response dùng envelope chuẩn `value`, `isSuccess`, `traceId`. Ví dụ minh họa `value` cho Instagram, không phải số liệu live:

```json
{
  "platformAccountId": "00000000-0000-0000-0000-000000000001",
  "platform": "instagram",
  "externalAccountId": "123",
  "fromDate": "2026-09-01",
  "toDate": "2026-09-17",
  "timezone": "UTC",
  "period": "day",
  "metricType": "total_value",
  "apiVersion": "v25.0",
  "fetchedAt": "2026-09-17T10:00:00Z",
  "isPartial": false,
  "metrics": {
    "reach": { "status": "Available", "period": "day", "totalValue": 1200, "values": [], "reason": null }
  }
}
```

Chuỗi thời gian trả `totalValue: null`, `values: [{"endTime":"2026-09-02T00:00:00Z","value":100}]`. Đây là dữ liệu cấp tài khoản, có thể gồm nội dung đăng ngoài hệ thống. Không yêu cầu có publication trước đó.

Không có dữ liệu: `Unavailable`, `totalValue: null`, `values: []`, reason `META_NO_DATA`. Meta từ chối metric/tổ hợp ngày-period: `META_METRIC_OR_RANGE_UNAVAILABLE`. Những metric khác vẫn trả kết quả; `isPartial=true`. Không đổi dữ liệu thiếu thành số 0. `totalValue` chỉ lấy từ `total_value` do Meta trả về.

Lỗi: 400 query/nền tảng không hỗ trợ; 404 tài khoản không tồn tại hoặc đã xóa; 409 cần reconnect, thiếu quyền, refresh đang chạy hoặc token lỗi; 429 rate limit; 502 lỗi/kết quả Meta không hợp lệ; 504 timeout. Không trả token, raw provider error hoặc URL phân trang có token. Không theo link phân trang sang khoảng thời gian liền kề.

Quyền Facebook: pages_read_engagement, read_insights. Instagram qua Facebook Login: pages_read_engagement, instagram_basic, instagram_manage_insights. Dùng Graph API version và timeout trong PlatformAccountOptions hiện tại. Không thay đổi dữ liệu DB.

Nguồn contract Meta: [Page SDK](https://github.com/facebook/facebook-python-business-sdk/blob/main/facebook_business/adobjects/page.py), [IGUser SDK](https://github.com/facebook/facebook-python-business-sdk/blob/main/facebook_business/adobjects/iguser.py).

Kiểm chứng bằng HTTP giả lập ở `HoaTheoMua.Service.Test/PostPublishingTest/AccountInsights.cs`; chưa xác minh số liệu tài khoản live.
