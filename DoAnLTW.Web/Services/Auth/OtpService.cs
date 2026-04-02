using System.Security.Cryptography;
using System.Text;
using DoAnLTW.Web.Data;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Services.Auth;

public class OtpService
{
    private readonly FinanceDbContext _db;
    private readonly EmailQueue _emailQueue;

    public OtpService(FinanceDbContext db, EmailQueue emailQueue)
    {
        _db = db;
        _emailQueue = emailQueue;
    }

    public async Task SendOtpAsync(AppUser? user, string email, string purpose, CancellationToken cancellationToken = default)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        _db.EmailOtps.Add(new EmailOtp
        {
            UserId = user?.Id,
            Email = email.Trim().ToLowerInvariant(),
            Purpose = purpose,
            CodeHash = ComputeHash(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);

        var purposeLabel = purpose switch
        {
            "Register" => "xác thực đăng ký",
            "ResetPassword" => "đổi mật khẩu",
            _ => "xác thực"
        };

        await _emailQueue.QueueAsync(new EmailMessage
        {
            To = email,
            Subject = $"Mã OTP {purposeLabel}",
            HtmlBody = $"""
                        <div style="font-family:Segoe UI,Arial,sans-serif;padding:20px;color:#1f2937">
                            <h2 style="margin:0 0 12px;color:#2563eb">Finance Flow</h2>
                            <p>Mã OTP của bạn là:</p>
                            <div style="font-size:32px;font-weight:700;letter-spacing:6px;background:#eff6ff;padding:16px 20px;border-radius:14px;display:inline-block;color:#1d4ed8">{code}</div>
                            <p style="margin-top:16px">Mã có hiệu lực trong 10 phút.</p>
                        </div>
                        """
        });
    }

    public async Task<OtpValidationResult> VerifyAsync(string email, string purpose, string code, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var otp = await _db.EmailOtps
            .Where(x => x.Email == normalizedEmail && x.Purpose == purpose && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
        {
            return new OtpValidationResult(false, "Không tìm thấy mã OTP.");
        }

        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            return new OtpValidationResult(false, "Mã OTP đã hết hạn.");
        }

        otp.Attempts += 1;

        if (otp.Attempts > 5)
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new OtpValidationResult(false, "Bạn đã nhập sai quá nhiều lần.");
        }

        if (!string.Equals(otp.CodeHash, ComputeHash(code.Trim()), StringComparison.Ordinal))
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new OtpValidationResult(false, "Mã OTP không đúng.");
        }

        otp.IsUsed = true;
        await _db.SaveChangesAsync(cancellationToken);
        return new OtpValidationResult(true, "Xác thực OTP thành công.");
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}

public record OtpValidationResult(bool Success, string Message);
