# Hướng dẫn Load Test 100 người dùng bằng k6

Test mô phỏng **100 người dùng truy cập cùng lúc** vào API, đo throughput (RPS),
độ trễ (latency) và tỉ lệ lỗi. Script: [`load_test_100_users.js`](./load_test_100_users.js).

---

## 1. Cài đặt k6 (chỉ làm 1 lần)

k6 là công cụ chạy độc lập (không phải thư viện npm), viết kịch bản bằng JavaScript.

**Windows:**
```bash
winget install k6 --source winget
# hoặc: choco install k6
```

**Kiểm tra đã cài xong:**
```bash
k6 version
```

> Không muốn cài? Có thể chạy bằng Docker:
> `docker run --rm -i grafana/k6 run - < load_test_100_users.js`
> (nhưng dùng Docker thì `localhost` phải đổi thành `host.docker.internal`).

---

## 2. Chạy API trước

Mở một terminal riêng, bật server rồi **để nguyên đó**:

```bash
dotnet run --project HoaTheoMua.API
```

- Chạy bằng `dotnet run` → API ở `http://localhost:5066`
- Chạy bằng **IIS Express** → API ở `http://localhost:26223`

Ghi nhớ cổng này để truyền vào bước sau.

---

## 3. Chạy load test

Mở **terminal thứ hai**, đứng ở thư mục gốc repo:

```bash
k6 run HoaTheoMua.Service/test/script/load_test_100_users.js
```

Mặc định: **100 VU (người dùng ảo), chạy 30 giây**, gọi endpoint
`/api/Test/bypass` trên `http://localhost:5066`.

### Đổi tham số (không cần sửa file)

Truyền biến môi trường bằng cờ `-e`:

```bash
# Đổi cổng sang IIS Express
k6 run -e BASE_URL=http://localhost:26223 HoaTheoMua.Service/test/script/load_test_100_users.js

# 200 người dùng, chạy 1 phút
k6 run -e VUS=200 -e DURATION=1m HoaTheoMua.Service/test/script/load_test_100_users.js

# Bắn tối đa, không nghỉ giữa các request (stress test thuần)
k6 run -e THINK_TIME=0 HoaTheoMua.Service/test/script/load_test_100_users.js

# Đổi endpoint muốn test
k6 run -e ENDPOINT=/api/Test/normal HoaTheoMua.Service/test/script/load_test_100_users.js
```

| Biến        | Mặc định                 | Ý nghĩa                                  |
|-------------|--------------------------|------------------------------------------|
| `BASE_URL`  | `http://localhost:5066`  | Địa chỉ gốc của API                      |
| `ENDPOINT`  | `/api/Test/bypass`       | Đường dẫn cần test                       |
| `VUS`       | `100`                    | Số người dùng ảo chạy đồng thời          |
| `DURATION`  | `30s`                    | Thời gian giữ tải (`30s`, `1m`, `5m`…)   |
| `THINK_TIME`| `1`                      | Số giây mỗi VU nghỉ giữa 2 request       |

---

## 4. ⚠️ Rất quan trọng: Rate limit ảnh hưởng kết quả

Server này có **giới hạn tốc độ** cấu hình trong `RateLimitExtensions.cs`, tính theo
**user đã đăng nhập** hoặc **IP đối với request ẩn danh**. Vì test từ 1 máy không
gửi access token = 1 IP, tất cả 100 VU dùng chung hạn mức:

| Endpoint            | Giới hạn                    | Dùng để                                   |
|---------------------|-----------------------------|-------------------------------------------|
| `/api/Test/bypass`  | **Không giới hạn**          | Đo sức chịu tải thật của server ✅         |
| *(mọi endpoint khác)* | **120 request/phút** (global) | Vượt ngưỡng sẽ trả `429`                 |
| `/api/Test/normal`  | **60 request/phút**         | Test riêng hành vi rate-limit             |
| `/api/Test/login`   | **5 request/phút**          | Test chặn brute-force login               |

👉 **Muốn đo hiệu năng server** → giữ endpoint mặc định `/api/Test/bypass`.
👉 **Thấy nhiều `429`** không phải server lỗi, mà là bị rate-limit — script đã tách
riêng chỉ số **"Tỉ lệ bị 429"** để bạn phân biệt.

---

## 5. Đọc kết quả

Sau khi chạy xong, phần cuối script in bảng tóm tắt tiếng Việt:

```
================ KẾT QUẢ LOAD TEST ================
  Tổng request:                  45230
  Request/giây (RPS):            1507.6
  Tỉ lệ lỗi (http_req_failed):   0.00 %
  Tỉ lệ bị 429 (rate limit):     0.00 %
  Latency trung bình:            18.4 ms
  Latency p95:                   42.1 ms
  Latency p99:                   88.7 ms
  Latency max:                   210.3 ms
==================================================
```

Ngoài ra k6 in bảng metric gốc — các dòng cần chú ý:

| Chỉ số              | Ý nghĩa                                                        |
|---------------------|---------------------------------------------------------------|
| `http_reqs`         | Tổng số request và RPS trung bình                             |
| `http_req_duration` | Tổng thời gian 1 request (`avg`, `p(95)`, `p(99)`, `max`)     |
| `http_req_failed`   | Tỉ lệ request lỗi — **muốn càng gần 0%**                      |
| `http_req_waiting`  | Thời gian server xử lý (TTFB) — phản ánh tốc độ backend       |
| `vus` / `vus_max`   | Số người dùng ảo đang chạy                                    |
| `checks`            | Tỉ lệ điều kiện `check(...)` pass                             |
| `iterations`        | Số lần vòng lặp hoàn tất                                      |

**`✓` / `✗` ở dòng THRESHOLDS** = đạt/không đạt ngưỡng đã đặt
(`p95 < 500ms`, lỗi `< 1%`, checks `> 99%`). Nếu có `✗`, k6 thoát với mã lỗi ≠ 0.

Dữ liệu đầy đủ được lưu ở `summary.json` (cùng thư mục chạy lệnh) để phân tích/vẽ biểu đồ sau.

---

## 6. Mẹo & lỗi thường gặp

- **`dial tcp ... connectex: No connection`** → API chưa chạy hoặc sai cổng
  (kiểm tra lại `BASE_URL`).
- **Toàn bộ là `429`** → đang test endpoint có rate-limit; đổi sang `/api/Test/bypass`
  hoặc giảm tải.
- **Muốn test endpoint cần đăng nhập** (vd `/api/v1/orders`) → phải thêm header
  `Authorization: Bearer <token>` trong `http.get(url, { headers: {...} })`.
  Token do service `taskcoper-be` cấp, không phải service này.
- **Test endpoint POST** (vd `/api/v1/ahamove/estimate`) → dùng `http.post(url, body, params)`
  với `Content-Type: application/json`. Lưu ý endpoint này gọi ra Ahamove thật nên
  chậm và có thể tốn phí — cân nhắc trước khi bắn tải.
