using Microsoft.EntityFrameworkCore;
using BiogenomAPI.Models;

namespace BiogenomAPI.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options) { }
        public DbSet<NutritionAssessmentResult> NutritionAssessmentResults { get; set; }
    }
}

