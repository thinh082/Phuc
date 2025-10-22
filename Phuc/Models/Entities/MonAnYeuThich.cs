using System;
using System.Collections.Generic;

namespace Phuc.Models.Entities;

public partial class MonAnYeuThich
{
    public int Id { get; set; }

    public int? IdTaiKhoan { get; set; }

    public long? IdThucDon { get; set; }

    public virtual ThucDon? IdThucDonNavigation { get; set; }
}
