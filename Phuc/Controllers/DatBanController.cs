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

        public string? TrangThai { get; set; }

        public string? GhiChu { get; set; }
        public List<ChiTietDatBanModel>? ChiTietDatBans { get; set; }
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
