using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain
{
    public class Member
    {
        public int Id { get; set; }

        [MaxLength(12)]
        [Required]
        public string SSN { get; set; } = "00000000-0000";

        [Display(Name="Full Name")]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; } = string.Empty;
        [ValidateNever]
        public IList<Loan> Loans { get; set; }
    }
}
