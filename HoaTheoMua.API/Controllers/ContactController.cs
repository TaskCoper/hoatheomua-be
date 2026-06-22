using HoaTheoMua.Service.MailService;
using HoaTheoMua.Service.User;
using Microsoft.AspNetCore.Mvc;
using IService = HoaTheoMua.Service.User.IService;

namespace HoaTheoMua.Controllers;


[ApiController]
public class ContactController: ControllerBase
{
    
    private readonly IService _userService;
    private readonly Service.MailService.IService _mailService;
    
    public ContactController(IService userService,  Service.MailService.IService mailService)
    {
        _userService = userService;
        _mailService = mailService;
    }
    
    [HttpPost("/api/v1/contact")]
    public async Task<IActionResult> SendContact([FromBody] Request.ContactRequest request)
    {
        // ----- Mail 1: gửi cho đối tác -----
        var partnerBody = $@"
<div style='font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;border:1px solid #eee;border-radius:8px;overflow:hidden;'>
    <div style='background:#2e7d32;padding:20px;text-align:center;'>
        <h2 style='color:#fff;margin:0;'>Hoa Theo Mùa</h2>
        <p style='color:#e8f5e9;margin:4px 0 0;font-size:14px;'>Thông báo liên hệ mới từ khách hàng</p>
    </div>
    <div style='padding:24px;background:#fff;'>
        <p style='font-size:15px;color:#333;'>Bạn vừa nhận được một yêu cầu liên hệ mới với thông tin chi tiết như sau:</p>
        <table style='width:100%;border-collapse:collapse;margin-top:12px;'>
            <tr>
                <td style='padding:8px 0;color:#666;width:140px;'>Họ và tên</td>
                <td style='padding:8px 0;color:#111;font-weight:600;'>{request.FullName}</td>
            </tr>
            <tr style='background:#fafafa;'>
                <td style='padding:8px 0;color:#666;'>Email</td>
                <td style='padding:8px 0;color:#111;'>{request.Email}</td>
            </tr>
            <tr>
                <td style='padding:8px 0;color:#666;'>Số điện thoại</td>
                <td style='padding:8px 0;color:#111;'>{request.Phone}</td>
            </tr>
            <tr style='background:#fafafa;'>
                <td style='padding:8px 0;color:#666;vertical-align:top;'>Chủ đề</td>
                <td style='padding:8px 0;color:#111;font-weight:600;'>{request.Topic}</td>
            </tr>
        </table>
        <div style='margin-top:16px;padding:14px;background:#f5f5f5;border-radius:6px;'>
            <p style='margin:0 0 6px;color:#666;font-size:13px;'>Nội dung:</p>
            <p style='margin:0;color:#111;white-space:pre-line;'>{request.Content}</p>
        </div>
        <p style='margin-top:20px;font-size:13px;color:#999;'>Vui lòng liên hệ lại khách hàng trong thời gian sớm nhất.</p>
    </div>
    <div style='background:#fafafa;padding:14px;text-align:center;font-size:12px;color:#999;'>
        Email này được gửi tự động từ hệ thống Hoa Theo Mùa.
    </div>
</div>";

        var partnerMail = new MailContent
        {
            To = request.Email,
            Subject = $"[Liên hệ mới] {request.Topic} - {request.FullName}",
            Body = partnerBody
        };

        // ----- Mail 2: gửi bản lưu nội bộ -----
        var internalBody = $@"
<div style='font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;'>
    <h3 style='color:#333;border-bottom:2px solid #2e7d32;padding-bottom:8px;'>[Internal] Liên hệ mới - {request.Topic}</h3>
    <ul style='list-style:none;padding:0;color:#333;font-size:14px;line-height:1.8;'>
        <li><strong>Họ và tên:</strong> {request.FullName}</li>
        <li><strong>Email:</strong> {request.Email}</li>
        <li><strong>Số điện thoại:</strong> {request.Phone}</li>
        <li><strong>Chủ đề:</strong> {request.Topic}</li>
    </ul>
    <p style='color:#333;font-size:14px;'><strong>Nội dung:</strong></p>
    <p style='color:#333;font-size:14px;white-space:pre-line;background:#f5f5f5;padding:12px;border-radius:6px;'>{request.Content}</p>
    <p style='color:#999;font-size:12px;margin-top:16px;'>Thời gian gửi: {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm} (GMT+7)</p>
</div>";

        var internalMail = new MailContent
        {
            To = "info@vnzdna.com",
            Subject = $"[Internal-Copy] {request.Topic}",
            Body = internalBody
        };

        await _mailService.SendMail(partnerMail);
        await _mailService.SendMail(internalMail);

        return Ok();
    }
}
