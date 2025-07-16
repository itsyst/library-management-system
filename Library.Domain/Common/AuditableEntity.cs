using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    [Display(Name = "Created Date")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Updated Date")]
    [DataType(DataType.DateTime)]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Created By")]
    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [Display(Name = "Updated By")]
    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
