using Library.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class BookDeleteVm
    {
        public BookDetails Book = new BookDetails();

        public int BookDetailsID { get; set; }

        public bool DeleteAll { get; set; }

        [Display(Name = "Book Copy")]
        public SelectList BookCopyList { get; set; } 
        public int BookCopyId { get; set; }

        public bool BookCopiesBlocked { get; set; }
        public bool BookCopyBlocked { get; set; }
    }
}
