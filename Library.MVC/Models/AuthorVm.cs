using Library.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class AuthorVm
    {
        // Needed for the index page to list all members
        public IList<Author> Authors { get; set; } = new List<Author>();

        // needed for the details page 
        public Author Author = new Author();

        // Needed for the edit page
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Name")]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }

       // For delete - to block if there are active loans
        public bool BlockDelete { get; set; }
    }
}
