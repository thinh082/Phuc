using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class ThanhToan
{
    public long Id { get; set; }

    public long DonDatBanId { get; set; }

    public decimal TongTien { get; set; }

    public string? PhuongThuc { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public decimal? SoTienHoan { get; set; }

    public string? PhuongThucHoan { get; set; }

    public DateTime? NgayHoanTien { get; set; }

    public string? LyDoHoanTien { get; set; }

    public virtual DonDatBan DonDatBan { get; set; } = null!;
}
