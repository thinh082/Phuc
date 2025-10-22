using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class TaiKhoan
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string MatKhau { get; set; } = null!;

    public int? LoaiTaiKhoanId { get; set; }

    public string? HoTen { get; set; }

    public DateTime? NgayTao { get; set; }

    public int? SoLanNhapSaiMatKhau { get; set; }

    public DateTime? ThoiGianKhoaMatKhau { get; set; }

    public string? HinhAnh { get; set; }

    public bool? TrangThai { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DonDatBan> DonDatBans { get; set; } = new List<DonDatBan>();

    public virtual LoaiTaiKhoan? LoaiTaiKhoan { get; set; }

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
}
