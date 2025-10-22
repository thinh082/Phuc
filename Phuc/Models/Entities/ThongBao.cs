using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class ThongBao
{
    public int Id { get; set; }

    public long? IdTaiKhoan { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
