using System.ComponentModel.DataAnnotations;

namespace ItemProcessorApp.Models;

public class Item
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Item name is required.")]
    [MaxLength(200)]
    [Display(Name = "Item Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Weight is required.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
    [Display(Name = "Weight (kg)")]
    public decimal Weight { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int? CreatedBy { get; set; }
    public User? Creator { get; set; }

    // Navigation
    public ICollection<ProcessedItem> ProcessedItems { get; set; } = new List<ProcessedItem>();
}
