using System.ComponentModel.DataAnnotations;

namespace ItemProcessorApp.ViewModels;

public class ChildItemInput
{
    [Required(ErrorMessage = "Item is required.")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Output weight is required.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
    public decimal OutputWeight { get; set; }

    public string? Notes { get; set; }
}

public class ProcessItemViewModel
{
    [Required(ErrorMessage = "Please select a parent item.")]
    [Display(Name = "Parent Item")]
    public int ParentItemId { get; set; }

    [Required(ErrorMessage = "Parent output weight is required.")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
    [Display(Name = "Parent Output Weight (kg)")]
    public decimal ParentOutputWeight { get; set; }

    public string? ParentNotes { get; set; }

    public List<ChildItemInput> Children { get; set; } = new() { new ChildItemInput() };
}
