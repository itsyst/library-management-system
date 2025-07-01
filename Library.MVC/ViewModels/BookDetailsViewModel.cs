using Library.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels
{
    public class BookDetailsViewModel
    {
        public BookDetails? BookDetails { get; set; }

        [ValidateNever]
        [Display(Name = "Author")]
        public IEnumerable<SelectListItem>? Authors { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? BookCopies { get; set; }

        [ValidateNever]
        public int Copies { get; set; } = 0;

        [ValidateNever]
        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }
    }
}
