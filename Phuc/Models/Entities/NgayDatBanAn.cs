using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class NgayDatBanAn
{
    public int Id { get; set; }

    public DateTime? NgayDat { get; set; }

    public bool? TinhTrang { get; set; }

    public long? IdBanAn { get; set; }

    public virtual BanAn? IdBanAnNavigation { get; set; }
}
