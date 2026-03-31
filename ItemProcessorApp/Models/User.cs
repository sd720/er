using System.ComponentModel.DataAnnotations;

namespace ItemProcessorApp.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Password { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Item> CreatedItems { get; set; } = new List<Item>();
    public ICollection<ProcessedItem> ProcessedItems { get; set; } = new List<ProcessedItem>();
}
