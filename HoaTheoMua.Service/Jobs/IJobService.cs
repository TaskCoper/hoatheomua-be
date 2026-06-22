using System.Linq.Expressions;

namespace HoaTheoMua.Service.Jobs;

public interface IJobService
{
    // Đẩy một Job vào hàng đợi để xử lý ngay tức thì (Fire-and-forget).
    string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall);

    // Lên lịch chạy Job sau một khoảng thời gian trì hoãn (Delayed Job).
    string Schedule<TJob>(Expression<Func<TJob, Task>> methodCall, TimeSpan delay);

    // Đăng ký hoặc cập nhật Job chạy định kỳ theo thời gian Cron (Recurring Job).
    void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        Expression<Func<TJob, Task>> methodCall,
        string cronExpression,
        string queue = "default");

    // Xóa bỏ một Job định kỳ dựa vào ID.
    void RemoveRecurring(string recurringJobId);

    // Xóa/hủy một Job thông thường đang chờ thực thi dựa vào ID.
    bool Delete(string jobId);
}
