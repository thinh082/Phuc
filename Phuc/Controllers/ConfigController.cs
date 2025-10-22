using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Phuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ConvertDBToJsonServices _convertDBToJsonServices;
        public ConfigController(ConvertDBToJsonServices convertDBToJsonServices)
        {
            _convertDBToJsonServices = convertDBToJsonServices;
        }
        [HttpGet("ConvertThucDon")]
        public async Task<IActionResult> ConvertThucDon()
        {
            await _convertDBToJsonServices.ConvertThucDon();
            return Ok(new { message = "Convert Tỉnh to JSON thành công!" });
        }
        [HttpGet("ConvertBanAn")]
        public async Task<IActionResult> ConvertBanAn()
        {
            await _convertDBToJsonServices.ConvertBanAn();
            return Ok(new { message = "Convert Tỉnh to JSON thành công!" });
        }
    }
}
