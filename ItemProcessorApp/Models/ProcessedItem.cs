using System.ComponentModel.DataAnnotations;

namespace ItemProcessorApp.Models;

public class ProcessedItem
{
    public int Id { get; set; }

    [Required]
    public int ItemId { get; set; }
    public Item? Item { get; set; }

    // Self-referencing parent-child
    public int? ParentId { get; set; }
    public ProcessedItem? Parent { get; set; }
    public ICollection<ProcessedItem> Children { get; set; } = new List<ProcessedItem>();

    [Required(ErrorMessage = "Output weight is required.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Output weight must be greater than 0.")]
    [Display(Name = "Output Weight (kg)")]
    public decimal OutputWeight { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.Now;

    public int? ProcessedBy { get; set; }
    public User? Processor { get; set; }
}
