using System.ComponentModel.DataAnnotations;

namespace FF.Models
{
    public class Area : BaseEntity
    {
        [Required(ErrorMessage = "Area Name is required.")]
        [StringLength(100, ErrorMessage = "Area Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; } // Optional description for the area

        // Navigation property for Tanks (if an Area can have multiple Tanks)
        public ICollection<Tank> Tanks { get; set; } = new List<Tank>();
    }
}
