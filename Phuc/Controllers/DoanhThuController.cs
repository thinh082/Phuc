using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoanhThuController : ControllerBase
    {
        private readonly PhucContext _context;

        public DoanhThuController(PhucContext phucContext)
        {
            _context = phucContext;
        }

        // GET: api/DoanhThu/TheoNgay?ngay=2024-01-01
        [HttpGet("TheoNgay")]
        public IActionResult TheoNgay([FromQuery] string ngay)
        {
            if (string.IsNullOrWhiteSpace(ngay))
            {
                return BadRequest(new { message = "Vui lòng nhập ngày." });
            }

            try
            {
                var ngayFilter = DateTime.ParseExact(ngay, "dd-MM-yyyy", null);

                var doanhThu = _context.ThanhToans
                    .Where(t => t.NgayThanhToan.HasValue && 
                                t.NgayThanhToan.Value.Date == ngayFilter.Date &&
                                t.TrangThai == "Đã thanh toán")
                    .Select(t => new
                    {
                        t.Id,
                        t.DonDatBanId,
                        t.TongTien,
                        t.SoTienHoan,
                        t.PhuongThuc,
                        t.NgayThanhToan,
                        DonDatBan = new
                        {
                            t.DonDatBan.Id,
                            t.DonDatBan.NgayDat,
                            t.DonDatBan.SoNguoi,
                            TaiKhoan = new
                            {
                                t.DonDatBan.TaiKhoan.Id,
                                t.DonDatBan.TaiKhoan.HoTen,
                                t.DonDatBan.TaiKhoan.Email
                            }
                        }
                    })
                    .ToList();

                var tongDoanhThu = doanhThu.Sum(d => d.TongTien);
                var tongHoanTien = doanhThu.Where(d => d.SoTienHoan.HasValue).Sum(d => d.SoTienHoan.Value);
                var doanhThuThucTe = tongDoanhThu - tongHoanTien;
                var soDon = doanhThu.Count;

                return Ok(new
                {
                    ngay = ngayFilter.ToString("dd/MM/yyyy"),
                    tongDoanhThu,
                    tongHoanTien,
                    doanhThuThucTe,
                    soDon,
                    chiTiet = doanhThu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy doanh thu theo ngày: " + ex.Message });
            }
        }

        // GET: api/DoanhThu/TheoThang?thang=01&nam=2024
        [HttpGet("TheoThang")]
        public IActionResult TheoThang([FromQuery] int thang, [FromQuery] int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return BadRequest(new { message = "Tháng không hợp lệ. Vui lòng nhập từ 1 đến 12." });
            }

            if (nam < 2000 || nam > DateTime.Now.Year + 1)
            {
                return BadRequest(new { message = "Năm không hợp lệ." });
            }

            try
            {
                var doanhThu = _context.ThanhToans
                    .Where(t => t.NgayThanhToan.HasValue &&
                                t.NgayThanhToan.Value.Month == thang &&
                                t.NgayThanhToan.Value.Year == nam &&
                                t.TrangThai == "Đã thanh toán")
                    .Select(t => new
                    {
                        t.Id,
                        t.DonDatBanId,
                        t.TongTien,
                        t.SoTienHoan,
                        t.PhuongThuc,
                        t.NgayThanhToan,
                        DonDatBan = new
                        {
                            t.DonDatBan.Id,
                            t.DonDatBan.NgayDat,
                            t.DonDatBan.SoNguoi,
                            TaiKhoan = new
                            {
                                t.DonDatBan.TaiKhoan.Id,
                                t.DonDatBan.TaiKhoan.HoTen,
                                t.DonDatBan.TaiKhoan.Email
                            }
                        }
                    })
                    .ToList();

                var tongDoanhThu = doanhThu.Sum(d => d.TongTien);
                var tongHoanTien = doanhThu.Where(d => d.SoTienHoan.HasValue).Sum(d => d.SoTienHoan!.Value);
                var doanhThuThucTe = tongDoanhThu - tongHoanTien;
                var soDon = doanhThu.Count;

                // Tổng hợp theo từng ngày trong tháng
                var chiTietTheoNgay = doanhThu
                    .GroupBy(d => d.NgayThanhToan!.Value.Date)
                    .Select(g => new
                    {
                        ngay = g.Key.ToString("dd/MM/yyyy"),
                        tongTien = g.Sum(x => x.TongTien),
                        hoanTien = g.Where(x => x.SoTienHoan.HasValue).Sum(x => x.SoTienHoan!.Value),
                        doanhThuThucTe = g.Sum(x => x.TongTien) - g.Where(x => x.SoTienHoan.HasValue).Sum(x => x.SoTienHoan!.Value),
                        soDon = g.Count()
                    })
                    .OrderBy(x => x.ngay)
                    .ToList();

                return Ok(new
                {
                    thang = thang.ToString("00"),
                    nam,
                    tongDoanhThu,
                    tongHoanTien,
                    doanhThuThucTe,
                    soDon,
                    chiTietTheoNgay
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy doanh thu theo tháng: " + ex.Message });
            }
        }

        // GET: api/DoanhThu/TheoNam?nam=2024
        [HttpGet("TheoNam")]
        public IActionResult TheoNam([FromQuery] int nam)
        {
            if (nam < 2000 || nam > DateTime.Now.Year + 1)
            {
                return BadRequest(new { message = "Năm không hợp lệ." });
            }

            try
            {
                var doanhThu = _context.ThanhToans
                    .Where(t => t.NgayThanhToan.HasValue &&
                                t.NgayThanhToan.Value.Year == nam &&
                                t.TrangThai == "Đã thanh toán")
                    .Select(t => new
                    {
                        t.Id,
                        t.DonDatBanId,
                        t.TongTien,
                        t.SoTienHoan,
                        t.PhuongThuc,
                        t.NgayThanhToan,
                        DonDatBan = new
                        {
                            t.DonDatBan.Id,
                            t.DonDatBan.NgayDat,
                            t.DonDatBan.SoNguoi,
                            TaiKhoan = new
                            {
                                t.DonDatBan.TaiKhoan.Id,
                                t.DonDatBan.TaiKhoan.HoTen,
                                t.DonDatBan.TaiKhoan.Email
                            }
                        }
                    })
                    .ToList();

                var tongDoanhThu = doanhThu.Sum(d => d.TongTien);
                var tongHoanTien = doanhThu.Where(d => d.SoTienHoan.HasValue).Sum(d => d.SoTienHoan!.Value);
                var doanhThuThucTe = tongDoanhThu - tongHoanTien;
                var soDon = doanhThu.Count;

                // Tổng hợp theo từng tháng trong năm
                var chiTietTheoThang = doanhThu
                    .GroupBy(d => d.NgayThanhToan!.Value.Month)
                    .Select(g => new
                    {
                        thang = g.Key,
                        tongTien = g.Sum(x => x.TongTien),
                        hoanTien = g.Where(x => x.SoTienHoan.HasValue).Sum(x => x.SoTienHoan!.Value),
                        doanhThuThucTe = g.Sum(x => x.TongTien) - g.Where(x => x.SoTienHoan.HasValue).Sum(x => x.SoTienHoan!.Value),
                        soDon = g.Count()
                    })
                    .OrderBy(x => x.thang)
                    .ToList();

                return Ok(new
                {
                    nam,
                    tongDoanhThu,
                    tongHoanTien,
                    doanhThuThucTe,
                    soDon,
                    chiTietTheoThang
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy doanh thu theo năm: " + ex.Message });
            }
        }

        // GET: api/DoanhThu/ThongKeChung
        [HttpGet("ThongKeChung")]
        public IActionResult ThongKeChung()
        {
            try
            {
                var tatCaThanhToan = _context.ThanhToans
                    .Where(t => t.NgayThanhToan.HasValue && t.TrangThai == "Đã thanh toán")
                    .ToList();

                var tongDoanhThuTatCa = tatCaThanhToan.Sum(t => t.TongTien);
                var tongHoanTienTatCa = tatCaThanhToan.Where(t => t.SoTienHoan.HasValue).Sum(t => t.SoTienHoan!.Value);
                var doanhThuThucTeTatCa = tongDoanhThuTatCa - tongHoanTienTatCa;
                var tongSoDon = tatCaThanhToan.Count;

                // Doanh thu hôm nay
                var homNay = DateTime.Today;
                var doanhThuHomNay = tatCaThanhToan
                    .Where(t => t.NgayThanhToan!.Value.Date == homNay)
                    .ToList();
                var tongDoanhThuHomNay = doanhThuHomNay.Sum(t => t.TongTien);
                var tongHoanTienHomNay = doanhThuHomNay.Where(t => t.SoTienHoan.HasValue).Sum(t => t.SoTienHoan!.Value);
                var doanhThuThucTeHomNay = tongDoanhThuHomNay - tongHoanTienHomNay;
                var soDonHomNay = doanhThuHomNay.Count;

                // Doanh thu tháng này
                var thangHienTai = DateTime.Now.Month;
                var namHienTai = DateTime.Now.Year;
                var doanhThuThangNay = tatCaThanhToan
                    .Where(t => t.NgayThanhToan!.Value.Month == thangHienTai && t.NgayThanhToan.Value.Year == namHienTai)
                    .ToList();
                var tongDoanhThuThangNay = doanhThuThangNay.Sum(t => t.TongTien);
                var tongHoanTienThangNay = doanhThuThangNay.Where(t => t.SoTienHoan.HasValue).Sum(t => t.SoTienHoan!.Value);
                var doanhThuThucTeThangNay = tongDoanhThuThangNay - tongHoanTienThangNay;
                var soDonThangNay = doanhThuThangNay.Count;

                // Top 5 khách hàng
                var topKhachHang = _context.ThanhToans
                    .Include(t => t.DonDatBan)
                    .ThenInclude(d => d.TaiKhoan)
                    .Where(t => t.NgayThanhToan.HasValue && t.TrangThai == "Đã thanh toán")
                    .GroupBy(t => new
                    {
                        t.DonDatBan.TaiKhoan.Id,
                        t.DonDatBan.TaiKhoan.HoTen,
                        t.DonDatBan.TaiKhoan.Email
                    })
                    .Select(g => new
                    {
                        khachHangId = g.Key.Id,
                        hoTen = g.Key.HoTen,
                        email = g.Key.Email,
                        tongChi = g.Sum(t => t.TongTien),
                        soDon = g.Count()
                    })
                    .OrderByDescending(x => x.tongChi)
                    .Take(5)
                    .ToList();

                // Phương thức thanh toán
                var thongKePhuongThuc = _context.ThanhToans
                    .Where(t => t.NgayThanhToan.HasValue && t.TrangThai == "Đã thanh toán")
                    .GroupBy(t => t.PhuongThuc)
                    .Select(g => new
                    {
                        phuongThuc = g.Key ?? "Không xác định",
                        soLuong = g.Count(),
                        tongTien = g.Sum(t => t.TongTien)
                    })
                    .ToList();

                return Ok(new
                {
                    tongQuat = new
                    {
                        tongDoanhThuTatCa,
                        tongHoanTienTatCa,
                        doanhThuThucTeTatCa,
                        tongSoDon
                    },
                    homNay = new
                    {
                        ngay = homNay.ToString("dd/MM/yyyy"),
                        tongDoanhThu = tongDoanhThuHomNay,
                        tongHoanTien = tongHoanTienHomNay,
                        doanhThuThucTe = doanhThuThucTeHomNay,
                        soDon = soDonHomNay
                    },
                    thangNay = new
                    {
                        thang = thangHienTai,
                        nam = namHienTai,
                        tongDoanhThu = tongDoanhThuThangNay,
                        tongHoanTien = tongHoanTienThangNay,
                        doanhThuThucTe = doanhThuThucTeThangNay,
                        soDon = soDonThangNay
                    },
                    topKhachHang,
                    thongKePhuongThuc
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thống kê chung: " + ex.Message });
            }
        }
    }
}

