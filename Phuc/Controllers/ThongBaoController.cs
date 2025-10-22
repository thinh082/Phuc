using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongBaoController : ControllerBase
    {
        private readonly PhucContext _context;
        public ThongBaoController(PhucContext phucContext)
        {
            _context = phucContext;
        }
        [HttpGet("GetDanhSachThongBao")]
        public IActionResult GetDanhSachThongBao(int idTaiKhoan)
        {
            var danhSachThongBao = _context.ThongBaos
                .Where(t => t.IdTaiKhoan == idTaiKhoan)
                .Select(t => new
                {
                    t.Id,
                    t.TieuDe,
                    t.NoiDung,
                    t.NgayTao,
                })
                .ToList();
            return Ok(danhSachThongBao);
        }
        [HttpPost("XoaThongBao/{id}")]
        public IActionResult XoaThongBao(long id)
        {
            var thongBao = _context.ThongBaos.FirstOrDefault(t => t.Id == id);
            if (thongBao == null)
            {
                return BadRequest(new { message = "Không tìm thấy thông báo." });
            }
            _context.ThongBaos.Remove(thongBao);
            _context.SaveChanges();
            return Ok(new { message = "Xóa thông báo thành công." });
        }
    }
}
