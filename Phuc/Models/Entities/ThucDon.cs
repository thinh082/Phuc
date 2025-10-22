using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class ThucDon
{
    public long Id { get; set; }

    public string TenMon { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal Gia { get; set; }

    public string? HinhAnh { get; set; }

    public bool? MonChinh { get; set; }

    public bool? DoUong { get; set; }

    public bool? TrangMien { get; set; }

    public virtual ICollection<ChiTietDatBan> ChiTietDatBans { get; set; } = new List<ChiTietDatBan>();

    public virtual ICollection<MonAnYeuThich> MonAnYeuThiches { get; set; } = new List<MonAnYeuThich>();
}
