using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FF.Models
{
    public enum OrderStatus
    {
        New,
        InProgress,
        Delivered,
        Canceled
    }

    public class Order : BaseEntity
    {
        // --- Relationship with Customer ---
        [Required(ErrorMessage = "Customer is required.")]
        public string? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser? Customer { get; set; }

        // --- Relationship with the requested Tank ---
        [Required(ErrorMessage = "Tank is required.")]
        public int TankId { get; set; }
        [ForeignKey("TankId")]
        public Tank? Tank { get; set; }

        [Required(ErrorMessage = "Quantity in Barrels is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int QuantityInBarrels { get; set; }

        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.New;

        [Required(ErrorMessage = "Delivery Address is required.")]
        [StringLength(500, ErrorMessage = "Delivery Address cannot exceed 500 characters.")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Contact Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters.")]
        public string ContactPhoneNumber { get; set; }
    }
}