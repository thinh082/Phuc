using Microsoft.AspNetCore.Mvc;
using Phuc.Models.Entities;
using System;
using System.Linq;
using System.Globalization;

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
        public IActionResult GetDanhSachBan([FromQuery] string? ngayDat)
        {
            try
            {
                // Parse ngày từ định dạng dd-MM-yyyy
                DateTime? ngayDatFilter = null;
                if (!string.IsNullOrEmpty(ngayDat))
                {
                    if (DateTime.TryParseExact(ngayDat, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        ngayDatFilter = parsedDate;
                    }
                    else
                    {
                        return BadRequest(new { message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng định dạng dd-MM-yyyy" });
                    }
                }

                var query = _context.BanAns.AsQueryable();

                if (ngayDatFilter.HasValue)
                {
                    // Lấy tất cả bàn ăn và join với NgayDatBanAn để kiểm tra bàn đã được đặt trong ngày
                    var danhSachBan = from banAn in _context.BanAns
                                      let ngayDatBan = _context.NgayDatBanAns
                                          .Where(n => n.IdBanAn == banAn.Id && 
                                                 n.NgayDat.HasValue && 
                                                 n.NgayDat.Value.Date == ngayDatFilter.Value.Date)
                                          .FirstOrDefault()
                                      select new
                                      {
                                          banAn.Id,
                                          banAn.TenBan,
                                          banAn.TrangThai,
                                          banAn.IdTang,
                                          banAn.SoChoNgoi,
                                          DaDat = ngayDatBan != null,
                                          TinhTrangDatBan = ngayDatBan != null ? ngayDatBan.TinhTrang : false,
                                          NgayDatBan = ngayDatBan != null ? ngayDatBan.NgayDat : (DateTime?)null
                                      };

                    return Ok(danhSachBan.ToList());
                }
                else
                {
                    // Nếu không có tham số ngày, trả về danh sách tất cả bàn ăn
                    var danhSachBan = _context.BanAns.Select(r => new
                    {
                        r.Id,
                        r.TenBan,
                        r.TrangThai,
                        r.IdTang,
                        r.SoChoNgoi,
                        DaDat = false,
                        TinhTrangDatBan = false,
                        NgayDatBan = (DateTime?)null
                    }).ToList();
                    return Ok(danhSachBan);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách bàn ăn", error = ex.Message });
            }
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

        // POST: api/BanAn/TaoBanAn
        [HttpPost("TaoBanAn")]
        public IActionResult TaoBanAn([FromBody] TaoBanAnRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Body không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(request.TenBan))
            {
                return BadRequest(new { message = "Vui lòng nhập tên bàn." });
            }

            if (request.SoChoNgoi.HasValue && request.SoChoNgoi.Value < 0)
            {
                return BadRequest(new { message = "Số chỗ ngồi không hợp lệ." });
            }

            var banAn = new BanAn
            {
                TenBan = request.TenBan.Trim(),
                IdTang = request.IdTang,
                SoChoNgoi = request.SoChoNgoi ?? 0,
                TrangThai = request.TrangThai ?? 0
            };

            _context.BanAns.Add(banAn);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Tạo bàn ăn thành công.",
                data = new
                {
                    banAn.Id,
                    banAn.TenBan,
                    banAn.TrangThai,
                    banAn.IdTang,
                    banAn.SoChoNgoi
                }
            });
        }

        // POST: api/BanAn/HuyTrangThaiBanAn
        [HttpPost("HuyTrangThaiBanAn")]
        public IActionResult HuyTrangThaiBanAn([FromBody] HuyTrangThaiBanAnRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Body không hợp lệ." });
            }

            if (request.BanAnId <= 0)
            {
                return BadRequest(new { message = "BanAnId không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(request.NgayDat))
            {
                return BadRequest(new { message = "Vui lòng truyền NgayDat theo định dạng dd-MM-yyyy." });
            }

            if (!DateTime.TryParseExact(request.NgayDat, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
            {
                return BadRequest(new { message = "Định dạng ngày không hợp lệ. Vui lòng sử dụng dd-MM-yyyy." });
            }

            var ban = _context.BanAns.FirstOrDefault(b => b.Id == request.BanAnId);
            if (ban == null)
            {
                return NotFound(new { message = "Không tìm thấy bàn." });
            }

            var ngayDatBan = _context.NgayDatBanAns.FirstOrDefault(n => n.IdBanAn == request.BanAnId && n.NgayDat.HasValue && n.NgayDat.Value.Date == ngay.Date);
            if (ngayDatBan == null)
            {
                return NotFound(new { message = "Không có trạng thái đặt bàn cho ngày này." });
            }

            // Hủy trạng thái đặt bàn: đặt lại về khả dụng
            ngayDatBan.TinhTrang = true;
            _context.NgayDatBanAns.Update(ngayDatBan);
            _context.SaveChanges();

            return Ok(new { message = "Hủy trạng thái bàn ăn thành công.", banAnId = request.BanAnId, ngayDat = ngay.ToString("dd-MM-yyyy"), tinhTrang = ngayDatBan.TinhTrang });
        }

        public class TaoBanAnRequest
        {
            public string TenBan { get; set; }
            public int? TrangThai { get; set; }
            public int? IdTang { get; set; }
            public int? SoChoNgoi { get; set; }
        }

        public class HuyTrangThaiBanAnRequest
        {
            public long BanAnId { get; set; }
            public string NgayDat { get; set; }
        }
    }
}


