using Library.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class BookEditVm
    {
        public BookDetails Book = new BookDetails();

        public int Id { get; set; }

        [Display(Name = "Title")]
        [MaxLength(55)]
        public string Title { get; set; }
        
        [Display(Name = "Author")]
        public SelectList AuthorList { get; set; }
        public int AuthorId { get; set; }
       
        [MaxLength(300)]
        public string Description { get; set; }
        
        [MaxLength(13)]
        [Required]
        public string ISBN { get; set; }
    }
}
