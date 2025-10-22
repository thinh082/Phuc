using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class DonDatBan
{
    public long Id { get; set; }

    public long TaiKhoanId { get; set; }

    public DateTime? NgayDat { get; set; }

    public TimeOnly GioDat { get; set; }

    public int? SoNguoi { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietDatBan> ChiTietDatBans { get; set; } = new List<ChiTietDatBan>();

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
