using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly PhucContext _context;

        public ThanhToanController(PhucContext phucContext)
        {
            _context = phucContext;
        }

        // POST: api/ThanhToan/DatBan
        [HttpPost("ThanhToan")]
        public IActionResult ThanhToan([FromBody] ThanhToanModel model)
        {
            if (model == null || model.DonDatBanId <= 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            if (model.TongTien <= 0)
            {
                return BadRequest(new { message = "Số tiền thanh toán phải lớn hơn 0." });
            }

            try
            {
                // Kiểm tra đơn đặt bàn có tồn tại không
                var donDatBan = _context.DonDatBans
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefault(d => d.Id == model.DonDatBanId);

                if (donDatBan == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đặt bàn." });
                }

                // Kiểm tra đơn có đang ở trạng thái có thể thanh toán không
                if (donDatBan.TrangThai == "Đã hủy" || donDatBan.TrangThai == "Đã hoàn thành")
                {
                    return BadRequest(new { message = $"Không thể thanh toán đơn có trạng thái '{donDatBan.TrangThai}'." });
                }

                // Kiểm tra xem đơn đã có thanh toán chưa
                var daThanhToan = _context.ThanhToans.Any(t => t.DonDatBanId == model.DonDatBanId && t.TrangThai == "Đã thanh toán");
                if (daThanhToan)
                {
                    return BadRequest(new { message = "Đơn đặt bàn này đã được thanh toán." });
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    // Tạo hoặc cập nhật thanh toán
                    var thanhToan = _context.ThanhToans.FirstOrDefault(t => t.DonDatBanId == model.DonDatBanId);
                    if (thanhToan == null)
                    {
                        thanhToan = new ThanhToan
                        {
                            DonDatBanId = model.DonDatBanId,
                            TongTien = model.TongTien,
                            PhuongThuc = model.PhuongThuc,
                            TrangThai = "Đã thanh toán",
                            NgayThanhToan = DateTime.Now
                        };
                        _context.ThanhToans.Add(thanhToan);
                    }
                    else
                    {
                        thanhToan.TongTien = model.TongTien;
                        thanhToan.PhuongThuc = model.PhuongThuc;
                        thanhToan.TrangThai = "Đã thanh toán";
                        thanhToan.NgayThanhToan = DateTime.Now;
                        _context.ThanhToans.Update(thanhToan);
                    }

                    // Cập nhật trạng thái đơn đặt bàn
                    donDatBan.TrangThai = "Đã thanh toán";
                    _context.DonDatBans.Update(donDatBan);

                    // Cập nhật trạng thái bàn về "Trống"
                    var chiTietDatBans = _context.ChiTietDatBans
                        .Where(ct => ct.DonDatBanId == model.DonDatBanId)
                        .ToList();

                    foreach (var chiTiet in chiTietDatBans)
                    {
                        var ban = _context.BanAns.FirstOrDefault(b => b.Id == chiTiet.BanAnId);
                        if (ban != null)
                        {
                            ban.TrangThai = 1; // 1 = Trống
                            _context.BanAns.Update(ban);
                        }
                    }

                    // Tạo thông báo
                    var thongBao = new ThongBao
                    {
                        IdTaiKhoan = donDatBan.TaiKhoanId,
                        TieuDe = "Thanh toán thành công",
                        NoiDung = $"Đơn đặt bàn #{donDatBan.Id} đã được thanh toán thành công với số tiền {model.TongTien:N0} VNĐ.",
                        NgayTao = DateTime.Now
                    };
                    _context.ThongBaos.Add(thongBao);

                    _context.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        success = true,
                        message = "Thanh toán thành công!",
                        thanhToan = new
                        {
                            thanhToan.Id,
                            thanhToan.DonDatBanId,
                            thanhToan.TongTien,
                            thanhToan.PhuongThuc,
                            thanhToan.TrangThai,
                            thanhToan.NgayThanhToan
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thanh toán: " + ex.Message
                });
            }
        }

        // POST: api/ThanhToan/HoanTien
        [HttpPost("HoanTien")]
        public IActionResult HoanTien([FromBody] HoanTienModel model)
        {
            if (model == null || model.DonDatBanId <= 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }

            if (model.SoTienHoan <= 0)
            {
                return BadRequest(new { message = "Số tiền hoàn phải lớn hơn 0." });
            }

            try
            {
                // Kiểm tra đơn đặt bàn có tồn tại không
                var donDatBan = _context.DonDatBans
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefault(d => d.Id == model.DonDatBanId);

                if (donDatBan == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn đặt bàn." });
                }

                // Tìm thanh toán của đơn
                var thanhToan = _context.ThanhToans.FirstOrDefault(t => t.DonDatBanId == model.DonDatBanId);

                if (thanhToan == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin thanh toán." });
                }

                if (thanhToan.TrangThai != "Đã thanh toán")
                {
                    return BadRequest(new { message = "Chỉ có thể hoàn tiền cho đơn đã thanh toán." });
                }

                // Kiểm tra số tiền hoàn không vượt quá số tiền đã thanh toán
                if (model.SoTienHoan > thanhToan.TongTien)
                {
                    return BadRequest(new { message = "Số tiền hoàn không được vượt quá số tiền đã thanh toán." });
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    // Cập nhật thông tin hoàn tiền
                    thanhToan.SoTienHoan = model.SoTienHoan;
                    thanhToan.PhuongThucHoan = model.PhuongThucHoan;
                    thanhToan.NgayHoanTien = DateTime.Now;
                    thanhToan.LyDoHoanTien = model.LyDoHoanTien;

                    // Cập nhật trạng thái thanh toán
                    if (model.SoTienHoan == thanhToan.TongTien)
                    {
                        thanhToan.TrangThai = "Đã hoàn tiền đầy đủ";
                    }
                    else
                    {
                        thanhToan.TrangThai = "Đã hoàn tiền một phần";
                    }

                    _context.ThanhToans.Update(thanhToan);

                    // Tạo thông báo
                    var thongBao = new ThongBao
                    {
                        IdTaiKhoan = donDatBan.TaiKhoanId,
                        TieuDe = "Hoàn tiền đơn đặt bàn",
                        NoiDung = $"Đơn đặt bàn #{donDatBan.Id} đã được hoàn {model.SoTienHoan:N0} VNĐ.\nLý do: {model.LyDoHoanTien}",
                        NgayTao = DateTime.Now
                    };
                    _context.ThongBaos.Add(thongBao);

                    _context.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        success = true,
                        message = "Hoàn tiền thành công!",
                        thanhToan = new
                        {
                            thanhToan.Id,
                            thanhToan.DonDatBanId,
                            thanhToan.TongTien,
                            thanhToan.SoTienHoan,
                            thanhToan.PhuongThucHoan,
                            thanhToan.TrangThai,
                            thanhToan.NgayHoanTien,
                            thanhToan.LyDoHoanTien
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi hoàn tiền: " + ex.Message
                });
            }
        }

        // GET: api/ThanhToan/ChiTiet/{donDatBanId}
        [HttpGet("ChiTiet/{donDatBanId}")]
        public IActionResult ChiTiet(long donDatBanId)
        {
            if (donDatBanId <= 0)
            {
                return BadRequest(new { message = "ID đơn đặt bàn không hợp lệ." });
            }

            var thanhToan = _context.ThanhToans
                .Include(t => t.DonDatBan)
                .ThenInclude(d => d.TaiKhoan)
                .FirstOrDefault(t => t.DonDatBanId == donDatBanId);

            if (thanhToan == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin thanh toán." });
            }

            return Ok(new
            {
                thanhToan.Id,
                thanhToan.DonDatBanId,
                thanhToan.TongTien,
                thanhToan.SoTienHoan,
                thanhToan.PhuongThuc,
                thanhToan.PhuongThucHoan,
                thanhToan.TrangThai,
                thanhToan.NgayThanhToan,
                thanhToan.NgayHoanTien,
                thanhToan.LyDoHoanTien,
                DonDatBan = new
                {
                    thanhToan.DonDatBan.Id,
                    thanhToan.DonDatBan.TrangThai,
                    thanhToan.DonDatBan.NgayDat,
                    TaiKhoan = new
                    {
                        thanhToan.DonDatBan.TaiKhoan.Id,
                        thanhToan.DonDatBan.TaiKhoan.HoTen,
                        thanhToan.DonDatBan.TaiKhoan.Email
                    }
                }
            });
        }

        // GET: api/ThanhToan/DanhSachTheoTrangThai
        [HttpGet("DanhSachTheoTrangThai")]
        public IActionResult DanhSachTheoTrangThai([FromQuery] string? trangThai)
        {
            try
            {
                var query = _context.ThanhToans
                    .Include(t => t.DonDatBan)
                    .ThenInclude(d => d.TaiKhoan)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    query = query.Where(t => t.TrangThai == trangThai);
                }

                var danhSachThanhToan = query
                    .Select(t => new
                    {
                        t.Id,
                        t.DonDatBanId,
                        t.TongTien,
                        t.SoTienHoan,
                        t.PhuongThuc,
                        t.PhuongThucHoan,
                        t.TrangThai,
                        t.NgayThanhToan,
                        t.NgayHoanTien,
                        t.LyDoHoanTien,
                        DonDatBan = new
                        {
                            t.DonDatBan.Id,
                            t.DonDatBan.TrangThai,
                            t.DonDatBan.NgayDat,
                            TaiKhoan = new
                            {
                                t.DonDatBan.TaiKhoan.Id,
                                t.DonDatBan.TaiKhoan.HoTen,
                                t.DonDatBan.TaiKhoan.Email
                            }
                        }
                    })
                    .ToList();

                return Ok(danhSachThanhToan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách thanh toán: " + ex.Message });
            }
        }
    }

    public class ThanhToanModel
    {
        public long DonDatBanId { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
    }

    public class HoanTienModel
    {
        public long DonDatBanId { get; set; }
        public decimal SoTienHoan { get; set; }
        public string PhuongThucHoan { get; set; } = string.Empty;
        public string LyDoHoanTien { get; set; } = string.Empty;
    }
}

