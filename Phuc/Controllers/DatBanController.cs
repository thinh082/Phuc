using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatBanController : ControllerBase
    {
        private readonly PhucContext _context;
        public DatBanController(PhucContext phucContext)
        {
            _context = phucContext;
        }
        [HttpPost("DatBan")]
        public IActionResult DatBan([FromBody] DatBanModel datBan)
        {
            if (datBan == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    // 1️⃣ Tạo mới đơn đặt bàn
                    var donDatBan = new DonDatBan
                    {
                        TaiKhoanId = datBan.TaiKhoanId,
                        SoNguoi = datBan.SoNguoi ?? 0,
                        TrangThai = datBan.TrangThai,
                        GhiChu = datBan.GhiChu,
                        NgayDat = DateTime.Now,
                        GioDat = TimeOnly.FromDateTime(DateTime.Now), // ✅ Đúng cú pháp
                        NgayTao = DateTime.Now
                    };


                    _context.DonDatBans.Add(donDatBan);
                    _context.SaveChanges();

                    // 2️⃣ Lưu danh sách chi tiết đặt bàn
                    if (datBan.ChiTietDatBans != null && datBan.ChiTietDatBans.Any())
                    {
                        foreach (var item in datBan.ChiTietDatBans)
                        {
                            var chiTiet = new ChiTietDatBan
                            {
                                DonDatBanId = donDatBan.Id,
                                BanAnId = item.BanAnId,
                                MonAnId = item.MonAnId,
                                SoLuong = item.SoLuong ?? 1,
                                GhiChu = item.GhiChu
                            };

                            _context.ChiTietDatBans.Add(chiTiet);

                            // 3️⃣ Cập nhật trạng thái bàn sang "Đang đặt"
                            var ban = _context.BanAns.FirstOrDefault(b => b.Id == item.BanAnId);
                            if (ban != null)
                            {
                                ban.TrangThai = 2;
                                _context.BanAns.Update(ban);
                            }
                        }
                    }
                    var thanhToan = new ThanhToan
                    {
                        DonDatBanId = donDatBan.Id,
                        PhuongThuc = datBan.PhuongThucThanhToan,
                        TongTien = datBan.TongTien,
                        TrangThai = datBan.PhuongThucThanhToan,
                        NgayThanhToan = DateTime.Now
                    };
                    _context.ThanhToans.Add(thanhToan);
                    var thongBao = new ThongBao
                    {
                        IdTaiKhoan = datBan.TaiKhoanId,
                        TieuDe = "Đặt bàn thành công",
                        NoiDung = $"Bạn đã đặt bàn thành công vào ngày {donDatBan.NgayDat?.ToString("dd/MM/yyyy")} lúc {donDatBan.GioDat}. Vui lòng đến đúng giờ!",
                        NgayTao = DateTime.Now
                    };
                    _context.ThongBaos.Add(thongBao);
                    _context.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        message = "Đặt bàn thành công!",
                        donDatBanId = donDatBan.Id
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi hệ thống: " + ex.Message,
                    inner = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace,
                    source = ex.Source,
                    targetSite = ex.TargetSite?.ToString()
                });
            }

        }
        [HttpPost("HuyDatBan")]
        public IActionResult HuyDatBan([FromBody] HuyDatBanModel huyDatBan)
        {
            if (huyDatBan == null || huyDatBan.DonDatBanId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    // 1️⃣ Tìm đơn đặt bàn
                    var donDatBan = _context.DonDatBans.FirstOrDefault(d => d.Id == huyDatBan.DonDatBanId);

                    if (donDatBan == null)
                        return NotFound("Không tìm thấy đơn đặt bàn.");

                    // Kiểm tra quyền hủy (chỉ người đặt mới được hủy)
                    if (donDatBan.TaiKhoanId != huyDatBan.TaiKhoanId)
                        return Forbid("Bạn không có quyền hủy đơn đặt bàn này.");

                    // Kiểm tra trạng thái đơn (không cho hủy nếu đã hoàn thành hoặc đã hủy)
                    if (donDatBan.TrangThai == "Đã hoàn thành" || donDatBan.TrangThai == "Đã hủy")
                        return BadRequest($"Không thể hủy đơn đặt bàn có trạng thái '{donDatBan.TrangThai}'.");

                    // 2️⃣ Cập nhật trạng thái đơn đặt bàn
                    donDatBan.TrangThai = "Đã hủy";
                    donDatBan.GhiChu = string.IsNullOrEmpty(donDatBan.GhiChu)
                        ? $"Lý do hủy: {huyDatBan.LyDoHuy}"
                        : $"{donDatBan.GhiChu}\nLý do hủy: {huyDatBan.LyDoHuy}";
                    _context.DonDatBans.Update(donDatBan);

                    // 3️⃣ Trả lại trạng thái bàn về "Trống"
                    var chiTietDatBans = _context.ChiTietDatBans
                        .Where(ct => ct.DonDatBanId == huyDatBan.DonDatBanId)
                        .ToList();

                    foreach (var chiTiet in chiTietDatBans)
                    {
                        var ban = _context.BanAns.FirstOrDefault(b => b.Id == chiTiet.BanAnId);
                        if (ban != null)
                        {
                            // Kiểm tra xem bàn có đang được đặt cho đơn khác không
                            var coDonKhac = _context.ChiTietDatBans
                                .Any(ct => ct.BanAnId == ban.Id
                                        && ct.DonDatBanId != huyDatBan.DonDatBanId
                                        && ct.DonDatBan.TrangThai != "Đã hủy"
                                        && ct.DonDatBan.TrangThai != "Đã hoàn thành");

                            // Chỉ trả về trạng thái trống nếu không có đơn nào khác đang đặt bàn này
                            if (!coDonKhac)
                            {
                                ban.TrangThai = 1; // 1 = Trống
                                _context.BanAns.Update(ban);
                            }
                        }
                    }

                    // 4️⃣ Cập nhật trạng thái thanh toán
                    var thanhToan = _context.ThanhToans.FirstOrDefault(tt => tt.DonDatBanId == huyDatBan.DonDatBanId);
                    if (thanhToan != null)
                    {
                        thanhToan.TrangThai = "Đã hủy";
                        _context.ThanhToans.Update(thanhToan);
                    }

                    // 5️⃣ Tạo thông báo hủy đặt bàn
                    var thongBao = new ThongBao
                    {
                        IdTaiKhoan = donDatBan.TaiKhoanId,
                        TieuDe = "Đơn đặt bàn đã được hủy",
                        NoiDung = $"Đơn đặt bàn #{donDatBan.Id} vào ngày {donDatBan.NgayDat?.ToString("dd/MM/yyyy")} lúc {donDatBan.GioDat} đã được hủy.\nLý do: {huyDatBan.LyDoHuy}",
                        NgayTao = DateTime.Now
                    };
                    _context.ThongBaos.Add(thongBao);

                    _context.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        message = "Hủy đặt bàn thành công!",
                        donDatBanId = donDatBan.Id
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi hệ thống: " + ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
        [HttpPost("LichSuDatBan")]
        public async Task<IActionResult> LichSuDatBan([FromBody] long taiKhoanId)
        {
            var lichSu = _context.DonDatBans
                .Where(d => d.TaiKhoanId == taiKhoanId)
                .Select(d => new
                {
                    d.Id,
                    d.NgayDat,
                    d.GioDat,
                    d.SoNguoi,
                    d.TrangThai,
                    d.GhiChu,
                    ChiTietDatBans = d.ChiTietDatBans.Select(ct => new
                    {
                        ct.BanAnId,
                        TenBan = ct.BanAn.TenBan,
                        ct.MonAnId,
                        TenMon = ct.MonAnId.HasValue ? ct.MonAn.TenMon : null,
                        ct.SoLuong,
                        ct.GhiChu
                    }).ToList()
                })
                .ToList();
            return Ok(lichSu);
        }

    }
    public class DatBanModel
    {
        public long TaiKhoanId { get; set; }

        public int? SoNguoi { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThucThanhToan { get; set; } = string.Empty;

        public string? TrangThai { get; set; }

        public string? GhiChu { get; set; }
        public List<ChiTietDatBanModel>? ChiTietDatBans { get; set; }
    }

    public class HuyDatBanModel
    {
        public long DonDatBanId { get; set; }
        public long TaiKhoanId { get; set; }
        public string LyDoHuy { get; set; } = string.Empty;
    }
    public class ChiTietDatBanModel
    {
        public long DonDatBanId { get; set; }

        public long BanAnId { get; set; }

        public long? MonAnId { get; set; }

        public int? SoLuong { get; set; }

        public string? GhiChu { get; set; }
    }
}
