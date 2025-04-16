using BaseballApi.Import;
using BaseballApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConstantsController : ControllerBase
    {
        private readonly BaseballContext _context;

        public ConstantsController(BaseballContext context)
        {
            _context = context;
        }

        [HttpPost("refresh")]
        public async Task<int> RefreshFangraphsConstants([FromForm] IFormFile file)
        {
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var loader = new CsvLoader(filePath);

            foreach (var yearConstants in loader.GetFangraphsConstants())
            {
                var existing = await _context.Constants.FirstOrDefaultAsync(c => c.Year == yearConstants.Year);
                if (existing == null)
                {
                    _context.Constants.Add(yearConstants);
                }
                else
                {
                    existing.WOBA = yearConstants.WOBA;
                    existing.WOBAScale = yearConstants.WOBAScale;
                    existing.WBB = yearConstants.WBB;
                    existing.WHBP = yearConstants.WHBP;
                    existing.W1B = yearConstants.W1B;
                    existing.W2B = yearConstants.W2B;
                    existing.W3B = yearConstants.W3B;
                    existing.WHR = yearConstants.WHR;
                    existing.RunSB = yearConstants.RunSB;
                    existing.RunCS = yearConstants.RunCS;
                    existing.RPA = yearConstants.RPA;
                    existing.RW = yearConstants.RW;
                    existing.CFIP = yearConstants.CFIP;
                }
            }

            await _context.SaveChangesAsync();

            int maxYear = await _context.Constants.MaxAsync(c => c.Year);
            return maxYear;
        }
    }
}
