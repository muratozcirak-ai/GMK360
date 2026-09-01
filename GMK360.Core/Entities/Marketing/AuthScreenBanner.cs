using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.Marketing;

public class AuthScreenBanner : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Subtitle { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string ImageUrl { get; set; } = null!;

    [MaxLength(100)]
    public string? ActionText { get; set; }

    [MaxLength(2000)]
    public string? ActionUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}
