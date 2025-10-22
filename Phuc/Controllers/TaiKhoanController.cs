using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phuc.Models.Entities;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly PhucContext _context;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<string, OtpInfo> _otpStorage = new Dictionary<string, OtpInfo>();
        
        public TaiKhoanController(PhucContext phucContext, IConfiguration configuration)
        {
            _context = phucContext;
            _configuration = configuration;
        }
        [HttpPost("DangNhap")]
        public IActionResult DangNhap([FromBody] TaiKhoanModel taiKhoan)
        {
            var user = _context.TaiKhoans
                .FirstOrDefault(u => u.Email == taiKhoan.Email && u.MatKhau == taiKhoan.MatKhau);
            if (user != null)
            {
                return Ok(new { success = true, message = "Đăng nhập thành công", user,idTaiKhoan = user.Id });
            }
            return Unauthorized(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không đúng" });
        }
        [HttpPost("DangKy")]
        public IActionResult DangKy([FromBody] TaiKhoanModel taiKhoan)
        {
            if (taiKhoan.Email == null || taiKhoan.MatKhau == null)
            {
                return BadRequest(new { success = false, message = "Vui lòng cung cấp đầy đủ thông tin" });
            }
            var existingUser = _context.TaiKhoans
                .FirstOrDefault(u => u.Email == taiKhoan.Email);
            if (existingUser != null)
            {
                return Conflict(new { success = false, message = "Email đã được sử dụng" });
            }
            var newUser = new TaiKhoan
            {
                Email = taiKhoan.Email,
                SoDienThoai = taiKhoan.SoDienThoai,
                MatKhau = taiKhoan.MatKhau,
                NgayTao = DateTime.Now,
                TrangThai = true,
                LoaiTaiKhoanId = 1
            };
            _context.TaiKhoans.Add(newUser);
            _context.SaveChanges();
            return Ok(new { success = true, message = "Đăng ký thành công", user = newUser });
        }


        [HttpPost("GuiEmail")]
        public async Task<IActionResult> GuiEmail([FromBody] GuiEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.TieuDe) || string.IsNullOrWhiteSpace(request.NoiDung))
            {
                return BadRequest(new { message = "Vui lòng cung cấp đầy đủ Email, Tiêu đề và Nội dung." });
            }

            var emailSetting = _configuration.GetSection("EmailSetting").Get<EmailSetting>();
            if (emailSetting == null || string.IsNullOrWhiteSpace(emailSetting.SmtpServer) || string.IsNullOrWhiteSpace(emailSetting.SmtpUsername) || string.IsNullOrWhiteSpace(emailSetting.SmtpPassword) || string.IsNullOrWhiteSpace(emailSetting.SenderEmail))
            {
                return StatusCode(500, new { message = "Thiếu cấu hình EmailSetting trong appsettings." });
            }

            try
            {
                using var smtpClient = new SmtpClient(emailSetting.SmtpServer, emailSetting.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailSetting.SmtpUsername, emailSetting.SmtpPassword)
                };

                var fromAddress = string.IsNullOrWhiteSpace(emailSetting.SenderName)
                    ? new MailAddress(emailSetting.SenderEmail)
                    : new MailAddress(emailSetting.SenderEmail, emailSetting.SenderName);

                var message = new MailMessage(fromAddress, new MailAddress(request.Email))
                {
                    Subject = request.TieuDe,
                    Body = request.NoiDung,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);

                return Ok(new { message = "Gửi email thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Gửi email thất bại: {ex.Message}" });
            }
        }

        [HttpPost("GuiMaOTP")]
        public async Task<IActionResult> GuiMaOTP([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email không được để trống." });
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                return BadRequest(new { message = "Email không hợp lệ." });
            }

            // Kiểm tra email có tồn tại trong DB không
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return BadRequest(new { message = "Email không tồn tại trong hệ thống." });
            }

            // Tạo mã OTP 5 ký tự
            var otp = GenerateOTP(5);
            var otpInfo = new OtpInfo
            {
                Otp = otp,
                ExpiryTime = DateTime.Now.AddMinutes(5) // OTP hết hạn sau 5 phút
            };

            // Lưu OTP vào storage
            _otpStorage[email] = otpInfo;

            // Gửi email
            var tieuDe = "Mã OTP xác thực";
            var noiDung = $"<h2>Mã OTP của bạn là: <strong style='color: red; font-size: 24px;'>{otp}</strong></h2><p>Mã này có hiệu lực trong 5 phút.</p>";
            
            var emailRequest = new GuiEmailRequest
            {
                Email = email,
                TieuDe = tieuDe,
                NoiDung = noiDung
            };

            var result = await GuiEmailInternal(emailRequest);
            if (result is OkObjectResult)
            {
                return Ok(new { message = "Gửi mã OTP thành công." });
            }
            else
            {
                return BadRequest(new { message = "Gửi mã OTP thất bại." });
            }
        }

        [HttpPost("CheckOTP")]
        public IActionResult CheckOTP([FromBody] CheckOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.OTP))
            {
                return BadRequest(new { message = "Email và OTP không được để trống." });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Email không hợp lệ." });
            }

            if (!_otpStorage.ContainsKey(request.Email))
            {
                return Unauthorized(new { message = "Không tìm thấy mã OTP cho email này." });
            }

            var otpInfo = _otpStorage[request.Email];
            
            // Kiểm tra OTP hết hạn
            if (DateTime.Now > otpInfo.ExpiryTime)
            {
                _otpStorage.Remove(request.Email);
                return Unauthorized(new { message = "Mã OTP đã hết hạn." });
            }

            // Kiểm tra OTP đúng
            if (otpInfo.Otp != request.OTP)
            {
                return Unauthorized(new { message = "Mã OTP không đúng." });
            }

            // Xóa OTP sau khi xác thực thành công
            _otpStorage.Remove(request.Email);

            return Ok(new { message = "Xác thực OTP thành công." });
        }

        [HttpPost("DoiMatKhau")]
        public IActionResult DoiMatKhau([FromBody] DoiMatKhauRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.MatKhauMoi))
            {
                return BadRequest(new { message = "Email và mật khẩu mới không được để trống." });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Email không hợp lệ." });
            }

            // Kiểm tra email có tồn tại trong DB không
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Email không tồn tại trong hệ thống." });
            }

            // Cập nhật mật khẩu mới (không mã hóa theo yêu cầu)
            user.MatKhau = request.MatKhauMoi;
            _context.TaiKhoans.Update(user);
            _context.SaveChanges();

            return Ok(new { message = "Đổi mật khẩu thành công.",idTaiKhoan = user.Id });
        }

        [HttpPost("CapNhatThongTin")]
        public IActionResult CapNhatThongTin([FromBody] CapNhatThongTinRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new { message = "ID tài khoản không hợp lệ." });
            }

            // Kiểm tra tài khoản có tồn tại không
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == request.Id);
            if (user == null)
            {
                return BadRequest(new { message = "Không tìm thấy tài khoản." });
            }

            // Validate email nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Email không hợp lệ." });
                }

                // Kiểm tra email có bị trùng không
                var existingUser = _context.TaiKhoans.FirstOrDefault(u => u.Email == request.Email && u.Id != request.Id);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email đã được sử dụng bởi tài khoản khác." });
                }

                user.Email = request.Email;
            }

            // Cập nhật các trường khác nếu có
            if (!string.IsNullOrWhiteSpace(request.HoTen))
            {
                user.HoTen = request.HoTen;
            }

            if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
            {
                user.SoDienThoai = request.SoDienThoai;
            }

            if (!string.IsNullOrWhiteSpace(request.HinhAnh))
            {
                user.HinhAnh = request.HinhAnh;
            }

            if (request.TrangThai.HasValue)
            {
                user.TrangThai = request.TrangThai.Value;
            }

            if (request.LoaiTaiKhoanId.HasValue)
            {
                user.LoaiTaiKhoanId = request.LoaiTaiKhoanId.Value;
            }

            // Cập nhật vào database
            _context.TaiKhoans.Update(user);
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật thông tin thành công.", user = user });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateOTP(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<IActionResult> GuiEmailInternal(GuiEmailRequest request)
        {
            var emailSetting = _configuration.GetSection("EmailSetting").Get<EmailSetting>();
            if (emailSetting == null || string.IsNullOrWhiteSpace(emailSetting.SmtpServer) || string.IsNullOrWhiteSpace(emailSetting.SmtpUsername) || string.IsNullOrWhiteSpace(emailSetting.SmtpPassword) || string.IsNullOrWhiteSpace(emailSetting.SenderEmail))
            {
                return StatusCode(500, new { message = "Thiếu cấu hình EmailSetting trong appsettings." });
            }

            try
            {
                using var smtpClient = new SmtpClient(emailSetting.SmtpServer, emailSetting.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailSetting.SmtpUsername, emailSetting.SmtpPassword)
                };

                var fromAddress = string.IsNullOrWhiteSpace(emailSetting.SenderName)
                    ? new MailAddress(emailSetting.SenderEmail)
                    : new MailAddress(emailSetting.SenderEmail, emailSetting.SenderName);

                var message = new MailMessage(fromAddress, new MailAddress(request.Email))
                {
                    Subject = request.TieuDe,
                    Body = request.NoiDung,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);

                return Ok(new { message = "Gửi email thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Gửi email thất bại: {ex.Message}" });
            }
        }

        private class OtpInfo
        {
            public string Otp { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
        }

        private class EmailSetting
        {
            public string SmtpServer { get; set; } = string.Empty;
            public int SmtpPort { get; set; }
            public string SmtpUsername { get; set; } = string.Empty;
            public string SmtpPassword { get; set; } = string.Empty;
            public string SenderEmail { get; set; } = string.Empty;
            public string? SenderName { get; set; }
        }

        public class GuiEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public string TieuDe { get; set; } = string.Empty;
            public string NoiDung { get; set; } = string.Empty;
        }

        public class CheckOtpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string OTP { get; set; } = string.Empty;
        }

        public class DoiMatKhauRequest
        {
            public string Email { get; set; } = string.Empty;
            public string MatKhauMoi { get; set; } = string.Empty;
        }

        public class CapNhatThongTinRequest
        {
            public long Id { get; set; }
            public string? Email { get; set; }
            public string? HoTen { get; set; }
            public string? SoDienThoai { get; set; }
            public string? HinhAnh { get; set; }
            public bool? TrangThai { get; set; }
            public int? LoaiTaiKhoanId { get; set; }
        }
    }
    public partial class TaiKhoanModel
    {

        public string Email { get; set; } = null!;

        public string? SoDienThoai { get; set; }

        public string MatKhau { get; set; } = null!;

    }
}
