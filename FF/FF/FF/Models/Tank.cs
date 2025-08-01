using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Required for ICollection

namespace FF.Models
{
    public enum WaterType // New enum for water types
    {
        Drinking,
        NonDrinking,
        Industrial // Example of another type
    }

  

    public class Tank : BaseEntity
    {
        // تم إزالة '?' لأن [Required] يجعلها غير قابلة للتصفير من الناحية المنطقية
        [Required(ErrorMessage = "Tank Name is required.")]
        [StringLength(100, ErrorMessage = "Tank Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int Capacity { get; set; } // تم إزالة '?'

        [Required(ErrorMessage = "Price Per Barrel is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal PricePerBarrel { get; set; }

        // تم إزالة '?'
        [Required(ErrorMessage = "Location is required.")] // New: Location property
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; }

        // تم إزالة '?'
        [Required(ErrorMessage = "Water Type is required.")] // New: WaterType property
        public WaterType TypeOfWater { get; set; } // Using the new enum

        // --- Relationship with Tank Owner (User) ---
        // تم إزالة [Required] من هنا مؤقتا للاختبار. إذا استمر الخطأ، أعدها.
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; } // يمكن أن يكون null

        // --- Relationship with Areas it serves (Many-to-Many) ---
        public ICollection<Area> Areas { get; set; } = new List<Area>();

        // مُنشئ لتهيئة الخصائص غير القابلة للتصفير (non-nullable)
        public Tank()
        {
            // تهيئة الخصائص إلى قيم افتراضية غير null
            Name = string.Empty;
            Location = string.Empty;
            // يجب تهيئة OwnerId إذا لم يكن [Required] هنا.
            // سيتولى الكنترولر تعيينه أو يجب توفير قيمة افتراضية له.
            OwnerId = string.Empty;
            // يتم تهيئة Capacity و PricePerBarrel بواسطة الـ Model Binder عندما يتم إدخال قيم.
            // إذا لم يتم إدخالها، ستظهر رسائل التحقق من الصحة (Validation).
            // TypeOfWater يتم تعيينها افتراضيا لأول قيمة في enum إذا لم يتم اختيارها.
        }
    }
}