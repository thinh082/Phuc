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

    public virtual DonDatBan DonDatBan { get; set; } = null!;
}
