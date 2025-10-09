using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class DanhGium
{
    public long Id { get; set; }

    public long TaiKhoanId { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
