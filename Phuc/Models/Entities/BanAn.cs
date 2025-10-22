using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class BanAn
{
    public long Id { get; set; }

    public string? TenBan { get; set; }

    public int? TrangThai { get; set; }

    public int? IdTang { get; set; }

    public int? SoChoNgoi { get; set; }

    public virtual ICollection<ChiTietDatBan> ChiTietDatBans { get; set; } = new List<ChiTietDatBan>();
}
