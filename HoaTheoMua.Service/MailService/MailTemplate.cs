namespace HoaTheoMua.Service.MailService;

public static class MailTemplate
{
    public static string EmailContainToken(string token)
    {
        var verificationLink = $"http://localhost:3000/verify?token={token}";
        
        var htmlBody = """
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Xác thực tài khoản - Hoa Theo Mùa</title>
            <style>
                @media screen and (max-width: 600px) {
                    .email-container { width: 100% !important; padding: 15px !important; }
                    .content-wrapper { padding: 30px 20px !important; }
                    .btn-verify { display: block !important; text-align: center !important; padding: 16px 20px !important; }
                }
            </style>
        </head>
        <body style="margin: 0; padding: 0; background-color: #eef3f0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; color: #3f3f3f; -webkit-font-smoothing: antialiased;">
        
            <table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color: #eef3f0; padding: 50px 0;">
                <tr>
                    <td align="center">
                        
                        <table class="email-container" border="0" cellpadding="0" cellspacing="0" width="580" style="background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #d6e2dc; box-shadow: 0 10px 25px -5px rgba(81, 108, 108, 0.05), 0 8px 10px -6px rgba(81, 108, 108, 0.05);">
                            
                            <tr>
                                <td align="center" style="padding: 40px 40px 10px 40px;">
                                    
                                    <h1 style="margin: 0; color: #7a9c59; font-size: 22px; font-weight: 640; letter-spacing: 2px; text-transform: uppercase;">
                                        Hoa Theo Mùa
                                    </h1>
                                    <div style="height: 2px; width: 30px; background-color: #7a9c59; margin: 15px auto 0 auto; border-radius: 2px;"></div>
                                </td>
                            </tr>
        
                            <tr>
                                <td class="content-wrapper" style="padding: 30px 45px 40px 45px;">
                                    <p style="margin-top: 0; margin-bottom: 16px; color: #516c6c; font-size: 18px; font-weight: 600; text-align: center;">
                                        Xác thực tài khoản của bạn
                                    </p>
                                    
                                    <p style="margin: 0 0 24px 0; font-size: 15px; line-height: 1.6; color: #516c6c; text-align: center;">
                                        Chào bạn, cảm ơn bạn đã ghé chơi khu vườn <strong>Hoa Theo Mùa</strong>. Để bắt đầu hành trình khám phá những hương sắc ngọt ngào nhất, bạn vui lòng xác nhận tài khoản bằng cách nhấn vào nút bên dưới nhé.
                                    </p>
        
                                    <table border="0" cellpadding="0" cellspacing="0" width="100%" style="margin: 32px 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{{verification_link}}" class="btn-verify" style="background-color: #516c6c; color: #ffffff; display: inline-block; padding: 14px 36px; font-size: 15px; font-weight: 500; text-decoration: none; border-radius: 8px; box-shadow: 0 4px 14px rgba(81, 108, 108, 0.25); letter-spacing: 0.5px; transition: all 0.2s ease;">
                                                    Kích Hoạt Tài Khoản
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        
                                    <table border="0" cellpadding="0" cellspacing="0" width="100%" style="background-color: #e3ece7; border-radius: 8px; border-left: 4px solid #7a9c59;">
                                        <tr>
                                            <td style="padding: 16px 20px;">
                                                <p style="margin: 0 0 6px 0; font-size: 13px; font-weight: 600; color: #2f4646;">
                                                    Bạn gặp sự cố với nút bấm?
                                                </p>
                                                <p style="margin: 0; font-size: 12px; color: #6b8079; line-height: 1.5; word-break: break-all;">
                                                    Sao chép liên kết này dán vào trình duyệt: <br>
                                                    <a href="{{verification_link}}" style="color: #516c6c; text-decoration: underline; font-weight: 500;">{{verification_link}}</a>
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
        
                            <tr>
                                <td style="background-color: #fafbfa; padding: 30px 45px; text-align: center; border-top: 1px solid #d6e2dc;">
                                    <p style="margin: 0 0 12px 0; font-size: 12px; color: #6b8079; line-height: 1.5;">
                                        Bạn nhận được email này vì đã đăng ký thành viên tại hoatheomua.com.<br>Nếu không phải bạn thực hiện, bạn có thể an tâm bỏ qua email này.
                                    </p>
                                    <p style="margin: 0; font-size: 11px; color: #bcd0c7; letter-spacing: 0.5px;">
                                        &copy; 2026 Hoa Theo Mùa. All right reserved.
                                    </p>
                                </td>
                            </tr>
        
                        </table>
                        </td>
                </tr>f
            </table>
        
        </body>
        </html>
        """;


        return htmlBody
            .Replace("{{verification_link}}", verificationLink);
    }
}