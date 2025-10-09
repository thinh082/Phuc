using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class ChiTietDatBan
{
    public long Id { get; set; }

    public long DonDatBanId { get; set; }

    public long BanAnId { get; set; }

    public long? MonAnId { get; set; }

    public int? SoLuong { get; set; }

    public string? GhiChu { get; set; }

    public virtual BanAn BanAn { get; set; } = null!;

    public virtual DonDatBan DonDatBan { get; set; } = null!;

    public virtual ThucDon? MonAn { get; set; }
}
