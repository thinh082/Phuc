using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanAnController : ControllerBase
    {
        private readonly PhucContext _context;

        public BanAnController(PhucContext phucContext)
        {
            _context = phucContext;
        }

        // GET: api/BanAn
        [HttpGet("GetDanhSachBanAn")]
        public IActionResult GetDanhSachBan()
        {
            var danhSachBan = _context.BanAns.Select(r => new
            {
                r.Id,
                r.TenBan,
                r.TrangThai,
                r.IdTang,
            }).ToList();
            return Ok(danhSachBan);
        }

        // POST: api/BanAn/SetTrangThai2/{id}
        [HttpPost("SetTrangThai2/{id}")]
        public IActionResult SetTrangThai2(long id)
        {
            var ban = _context.BanAns.FirstOrDefault(b => b.Id == id);
            if (ban == null)
            {
                return BadRequest(new { message = "Không tìm thấy bàn." });
            }

            ban.TrangThai = 2;
            _context.BanAns.Update(ban);
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật trạng thái thành công.", id = ban.Id, trangThai = ban.TrangThai });
        }

        // POST: api/BanAn/SetTrangThai1/{id}
        [HttpPost("SetTrangThai1/{id}")]
        public IActionResult SetTrangThai1(long id)
            {
            var ban = _context.BanAns.FirstOrDefault(b => b.Id == id);
            if (ban == null)
            {
                return BadRequest(new { message = "Không tìm thấy bàn." });
            }

            ban.TrangThai = 1;
            _context.BanAns.Update(ban);
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật trạng thái thành công.", id = ban.Id, trangThai = ban.TrangThai });
        }
    }
}


