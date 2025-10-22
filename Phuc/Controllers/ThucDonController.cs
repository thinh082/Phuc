using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThucDonController : ControllerBase
    {
        private readonly PhucContext _context;

        public ThucDonController(PhucContext phucContext)
        {
            _context = phucContext;
        }

        // GET: api/BanAn
        [HttpGet("GetDanhSachThucDon")]
        public IActionResult GetDanhSachThucDon(int? idTaiKhoan)
        {
            var danhSachBan = _context.ThucDons.Select(r => new
            {
                r.Id,
                r.Gia,
                r.TenMon,
                r.MonChinh,
                r.HinhAnh,
                r.DoUong,
                r.TrangMien
            }).ToList();
           var danhSachYeuThich = idTaiKhoan.HasValue?_context.MonAnYeuThiches
                .Where(m => m.IdTaiKhoan == idTaiKhoan.Value)
                .Select(m => new
                {
                    m.IdThucDonNavigation.Id,
                    m.IdThucDonNavigation.TenMon,
                    m.IdThucDonNavigation.Gia,
                    m.IdThucDonNavigation.HinhAnh,
                })
                .ToList() : null;
            return Ok(new
            {
                danhSachBan,
                danhSachYeuThich
            });
        }
        [HttpGet("GetMonAnYeuThich")]
        public IActionResult GetMonAnYeuThich(int idTaiKhoan)
        {
            var danhSachYeuThich = _context.MonAnYeuThiches
                .Where(m => m.IdTaiKhoan == idTaiKhoan)
                .Select(m => new
                {
                    m.IdThucDonNavigation.Id,
                    m.IdThucDonNavigation.TenMon,
                    m.IdThucDonNavigation.Gia,
                })
                .ToList();
            return Ok(danhSachYeuThich);
        }
        [HttpPost("ThemMonAnYeuThich")]
        public IActionResult ThemMonAnYeuThich(int idTaiKhoan, int idThucDon)
        {
            var monAnYeuThich = new MonAnYeuThich
            {
                IdTaiKhoan = idTaiKhoan,
                IdThucDon = idThucDon
            };
            _context.MonAnYeuThiches.Add(monAnYeuThich);
            _context.SaveChanges();
            return Ok();
        }
        [HttpPost("XoaMonAnYeuThich")]
        public IActionResult XoaMonAnYeuThich(int idTaiKhoan, int idThucDon)
        {
            var monAnYeuThich = _context.MonAnYeuThiches
                .FirstOrDefault(m => m.IdTaiKhoan == idTaiKhoan && m.IdThucDon == idThucDon);
            if (monAnYeuThich == null)
            {
                return NotFound();
            }
            _context.MonAnYeuThiches.Remove(monAnYeuThich);
            _context.SaveChanges();
            return Ok();
        }
    }
}
