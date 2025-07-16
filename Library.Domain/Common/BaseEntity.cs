using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}
 