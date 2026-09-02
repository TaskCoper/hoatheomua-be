import http from 'k6/http';
import { check } from 'k6';
import { Rate } from 'k6/metrics';

export let rateLimitedRequests = new Rate('rate_limited_requests');

export const options = {
    // TRƯỜNG HỢP ĐẶC BIỆT: Bắt buộc phải có dòng này khi test với HTTPS của localhost .NET
    insecureSkipTLSVerify: true,

    scenarios: {
        rate_limit_test: {
            executor: 'constant-arrival-rate',
            // Sử dụng biến môi trường (Environment Variable) để thay đổi tốc độ bắn linh hoạt
            rate: __ENV.RATE || 20,
            timeUnit: '1s',
            duration: '10s',
            preAllocatedVUs: 50,
            maxVUs: 200,
        },
    },
    thresholds: {
        rate_limited_requests: ['rate>0.50'],
    },
};

export default function () {
    // Nhận tên endpoint từ câu lệnh gõ bên ngoài. Mặc định là 'normal'
    const endpoint = __ENV.ENDPOINT || 'normal';

    // ĐIỀN PORT HTTPS CỦA DỰ ÁN .NET VÀO ĐÂY
    const url = `http://localhost:26223/api/test/${endpoint}`;

    const params = {
        headers: {
            // Có thể điền token để kiểm tra phân vùng theo user; nếu bỏ trống sẽ phân vùng theo IP.
         
        },
    };

    const res = http.get(url, params);

    const isRateLimited = res.status === 429;

    check(res, {
        'is status 200 (Thành công)': (r) => r.status === 200,
        'is status 429 (Bị chặn do Rate Limit)': (r) => r.status === 429,
    });

    rateLimitedRequests.add(isRateLimited);
}
