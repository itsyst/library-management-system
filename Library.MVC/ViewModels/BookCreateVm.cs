using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models
{
    public class BookCreateVm
    {

        [Display(Name = "ISBN")]
        [MaxLength(13)]
        [Required]
        public string ISBN { get; set; }

        [Display(Name = "Title")]
        [MaxLength(55)]
        [Required]
        public string Title { get; set; }
        
        [Display(Name = "Author")]
        public SelectList AuthorList { get; set; }
        [Required]
        public int AuthorId { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }

        [Display(Name = "Number of copies")]
        [Range(0,20, ErrorMessage ="The maximum amount of copies is 20")]
        public int NumberOfCopies { get; set; }
 


    }
}
