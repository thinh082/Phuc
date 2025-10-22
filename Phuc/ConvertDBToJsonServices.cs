using Microsoft.EntityFrameworkCore;
using Phuc.Models.Entities;
using Newtonsoft.Json;

public class ConvertDBToJsonServices
{
    private readonly PhucContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ConvertDBToJsonServices(PhucContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }

    // Convert Tỉnh
    public async Task ConvertBanAn()
    {
        var data = await _context.BanAns
            .Select(x => new
            {
                id = x.Id.ToString(),
                ten = x.TenBan,
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "BanAn.json");
        await File.WriteAllTextAsync(filePath, json);
    }

    // Convert Quận/Huyện
    public async Task ConvertThucDon(){
        var data = await _context.ThucDons
            .Select(x => new
            {
                id = x.Id.ToString(),
                ten = x.TenMon,
                monChinh = x.MonChinh
            })
            .OrderBy(x => x.ten)
            .ToListAsync();

        string json = JsonConvert.SerializeObject(data);
        string folderPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Models", "Config", "data");
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, "ThucDon.json");
        await File.WriteAllTextAsync(filePath, json);
    }

   
}
