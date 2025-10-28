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
        // POST: api/ThucDon/ThemMoi
        [HttpPost("ThemMoi")]
        public IActionResult ThemMoi([FromBody] ThemMoiThucDonModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.TenMon))
            {
                return BadRequest(new { message = "Vui lòng cung cấp đầy đủ thông tin món ăn." });
            }

            if (model.Gia <= 0)
            {
                return BadRequest(new { message = "Giá món ăn phải lớn hơn 0." });
            }

            try
            {
                var thucDon = new ThucDon
                {
                    TenMon = model.TenMon,
                    MoTa = model.MoTa,
                    Gia = model.Gia,
                    HinhAnh = model.HinhAnh,
                    MonChinh = model.MonChinh ?? false,
                    DoUong = model.DoUong ?? false,
                    TrangMien = model.TrangMien ?? false
                };

                _context.ThucDons.Add(thucDon);
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Thêm món ăn thành công!",
                    thucDon = new
                    {
                        thucDon.Id,
                        thucDon.TenMon,
                        thucDon.MoTa,
                        thucDon.Gia,
                        thucDon.HinhAnh,
                        thucDon.MonChinh,
                        thucDon.DoUong,
                        thucDon.TrangMien
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm món ăn: " + ex.Message
                });
            }
        }

        // PUT: api/ThucDon/CapNhat
        [HttpPut("CapNhat")]
        public IActionResult CapNhat([FromBody] CapNhatThucDonModel model)
        {
            if (model == null || model.Id <= 0)
            {
                return BadRequest(new { message = "ID món ăn không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(model.TenMon))
            {
                return BadRequest(new { message = "Tên món ăn không được để trống." });
            }

            if (model.Gia <= 0)
            {
                return BadRequest(new { message = "Giá món ăn phải lớn hơn 0." });
            }

            try
            {
                var thucDon = _context.ThucDons.FirstOrDefault(t => t.Id == model.Id);
                if (thucDon == null)
                {
                    return NotFound(new { message = "Không tìm thấy món ăn." });
                }

                // Cập nhật thông tin
                thucDon.TenMon = model.TenMon;
                thucDon.MoTa = model.MoTa;
                thucDon.Gia = model.Gia;
                thucDon.HinhAnh = model.HinhAnh;
                if (model.MonChinh.HasValue) thucDon.MonChinh = model.MonChinh.Value;
                if (model.DoUong.HasValue) thucDon.DoUong = model.DoUong.Value;
                if (model.TrangMien.HasValue) thucDon.TrangMien = model.TrangMien.Value;

                _context.ThucDons.Update(thucDon);
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật món ăn thành công!",
                    thucDon = new
                    {
                        thucDon.Id,
                        thucDon.TenMon,
                        thucDon.MoTa,
                        thucDon.Gia,
                        thucDon.HinhAnh,
                        thucDon.MonChinh,
                        thucDon.DoUong,
                        thucDon.TrangMien
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật món ăn: " + ex.Message
                });
            }
        }

        // DELETE: api/ThucDon/Xoa/{id}
        [HttpDelete("Xoa/{id}")]
        public IActionResult Xoa(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID món ăn không hợp lệ." });
            }

            try
            {
                var thucDon = _context.ThucDons.FirstOrDefault(t => t.Id == id);
                if (thucDon == null)
                {
                    return NotFound(new { message = "Không tìm thấy món ăn." });
                }

                // Kiểm tra xem món ăn có đang được sử dụng trong chi tiết đặt bàn không
                var coSuDung = _context.ChiTietDatBans.Any(c => c.MonAnId == id);
                if (coSuDung)
                {
                    return BadRequest(new { message = "Không thể xóa món ăn vì đang được sử dụng trong đơn đặt bàn." });
                }

                _context.ThucDons.Remove(thucDon);
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Xóa món ăn thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa món ăn: " + ex.Message
                });
            }
        }

        // GET: api/ThucDon/ChiTiet/{id}
        [HttpGet("ChiTiet/{id}")]
        public IActionResult ChiTiet(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID món ăn không hợp lệ." });
            }

            var thucDon = _context.ThucDons.FirstOrDefault(t => t.Id == id);
            if (thucDon == null)
            {
                return NotFound(new { message = "Không tìm thấy món ăn." });
            }

            return Ok(new
            {
                thucDon.Id,
                thucDon.TenMon,
                thucDon.MoTa,
                thucDon.Gia,
                thucDon.HinhAnh,
                thucDon.MonChinh,
                thucDon.DoUong,
                thucDon.TrangMien
            });
        }
    }

    public class ThemMoiThucDonModel
    {
        public string TenMon { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public string? HinhAnh { get; set; }
        public bool? MonChinh { get; set; }
        public bool? DoUong { get; set; }
        public bool? TrangMien { get; set; }
    }

    public class CapNhatThucDonModel
    {
        public long Id { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public string? HinhAnh { get; set; }
        public bool? MonChinh { get; set; }
        public bool? DoUong { get; set; }
        public bool? TrangMien { get; set; }
    }
}
