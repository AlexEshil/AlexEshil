using System;
using System.ComponentModel.DataAnnotations;

namespace BiogenomAPI.Models
{
	public class NutritionAssessmentResult
	{
		[Key]
        public int Id { get; set; }

		public DateTime DatePassed { get; set; } = DateTime.UtcNow;

		[Required]
		public string? UserFullName { get; set; }

		[Required]
		public string? ResultJson { get; set; }
        
	}
}

