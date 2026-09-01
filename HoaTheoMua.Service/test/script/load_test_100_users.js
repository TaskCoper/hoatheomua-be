import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// ---------------------------------------------------------------------------
// Load test: mô phỏng 100 người dùng truy cập cùng lúc.
// Chạy:
//   k6 run HoaTheoMua.Service/test/script/load_test_100_users.js
// Đổi tham số qua biến môi trường (-e KEY=VALUE), ví dụ:
//   k6 run -e BASE_URL=http://localhost:5066 -e VUS=100 -e DURATION=30s load_test_100_users.js
// ---------------------------------------------------------------------------

// dotnet run -> 5066 ; IIS Express -> 26223. Đổi cho khớp cách bạn chạy API.
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5066';

// Mặc định /api/Test/bypass vì có [DisableRateLimiting] -> đo đúng sức chịu tải
// của server, không bị 429 làm nhiễu. Muốn xem hành vi rate-limit thì đổi sang
// /api/Test/normal (giới hạn 60 request/phút).
const ENDPOINT = __ENV.ENDPOINT || '/api/Test/bypass';

const VUS = parseInt(__ENV.VUS || '100', 10);   // số người dùng ảo (đồng thời)
const DURATION = __ENV.DURATION || '30s';       // thời gian giữ tải

// Tách riêng tỉ lệ 429 (bị rate-limit) để không nhầm với lỗi thật của server.
const rateLimited = new Rate('rate_limited_429');
const waitTrend = new Trend('waiting_time', true);

export const options = {
  // Cần dòng này nếu test qua HTTPS của localhost .NET (chứng chỉ dev tự ký).
  insecureSkipTLSVerify: true,

  // 100 VU chạy song song suốt DURATION = "100 người truy cập cùng lúc".
  scenarios: {
    constant_load: {
      executor: 'constant-vus',
      vus: VUS,
      duration: DURATION,
    },
    // --- Tuỳ chọn: kịch bản spike tăng dần rồi giảm. Bỏ comment để dùng ---
    // ramping_load: {
    //   executor: 'ramping-vus',
    //   startVUs: 0,
    //   stages: [
    //     { duration: '10s', target: VUS }, // tăng dần lên 100 VU
    //     { duration: '30s', target: VUS }, // giữ 100 VU
    //     { duration: '5s',  target: 0 },   // hạ tải
    //   ],
    // },
  },

  // Ngưỡng đạt/không đạt: k6 trả exit code != 0 nếu vi phạm (dùng được cho CI).
  thresholds: {
    http_req_failed: ['rate<0.01'],    // < 1% request lỗi
    http_req_duration: ['p(95)<500'],  // 95% request nhanh hơn 500ms
    checks: ['rate>0.99'],             // > 99% check pass
  },
};

export default function () {
  const res = http.get(`${BASE_URL}${ENDPOINT}`);

  rateLimited.add(res.status === 429);
  waitTrend.add(res.timings.waiting);

  check(res, {
    'status là 200': (r) => r.status === 200,
    'không phải lỗi 5xx': (r) => r.status < 500,
    'phản hồi < 800ms': (r) => r.timings.duration < 800,
  });

  // Nghỉ ngắn giữa các request cho giống người dùng thật.
  // Đặt THINK_TIME=0 để bắn tối đa (stress test thuần).
  sleep(parseFloat(__ENV.THINK_TIME || '1'));
}

// In tóm tắt gọn ra console + lưu dữ liệu đầy đủ ra summary.json.
export function handleSummary(data) {
  const m = data.metrics;
  const get = (name, field, d = 0) =>
    m[name] && m[name].values[field] != null ? m[name].values[field] : d;

  const line = (label, value) => `  ${label.padEnd(30)} ${value}`;
  const ms = (v) => `${v.toFixed(1)} ms`;

  const summary = [
    '',
    '================ KẾT QUẢ LOAD TEST ================',
    line('Tổng request:', get('http_reqs', 'count')),
    line('Request/giây (RPS):', get('http_reqs', 'rate').toFixed(1)),
    line('Tỉ lệ lỗi (http_req_failed):', `${(get('http_req_failed', 'rate') * 100).toFixed(2)} %`),
    line('Tỉ lệ bị 429 (rate limit):', `${(get('rate_limited_429', 'rate') * 100).toFixed(2)} %`),
    line('Latency trung bình:', ms(get('http_req_duration', 'avg'))),
    line('Latency p95:', ms(get('http_req_duration', 'p(95)'))),
    line('Latency p99:', ms(get('http_req_duration', 'p(99)'))),
    line('Latency max:', ms(get('http_req_duration', 'max'))),
    '==================================================',
    '',
  ].join('\n');

  return {
    stdout: summary,
    'summary.json': JSON.stringify(data, null, 2),
  };
}
