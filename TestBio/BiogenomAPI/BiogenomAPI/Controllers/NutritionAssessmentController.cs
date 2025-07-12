using Microsoft.AspNetCore.Mvc;
using BiogenomAPI.Data;
using BiogenomAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BiogenomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NutritionAssessmentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public NutritionAssessmentController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Получить последний результат (GET api/NutritionAssessment/last)
        [HttpGet("last")]
        public async Task<ActionResult<NutritionAssessmentResult>> GetLastResult()
        {
            var result = await _db.NutritionAssessmentResults
                .OrderByDescending(x => x.DatePassed) // убедись, что в модели DatePassed
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // Сохранить новый результат (POST api/NutritionAssessment)
        [HttpPost]
        public async Task<ActionResult> SaveResult([FromBody] NutritionAssessmentResult result)
        {
            var oldResults = await _db.NutritionAssessmentResults.ToListAsync();
            _db.NutritionAssessmentResults.RemoveRange(oldResults);

            await _db.NutritionAssessmentResults.AddAsync(result);
            await _db.SaveChangesAsync();

            return Ok(result);
        }
    }
}
