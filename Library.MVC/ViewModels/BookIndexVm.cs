using Library.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class BookIndexVm
    {
        public ICollection<BookDetails> Books { get; set; } = new List<BookDetails>();
        public bool ShowOnlyAvailableCopies = false;
        public bool SelectedOnAuthors = false;
        public string SearchString = "";
        public bool BookCopyAdded { get; set; }
    }
}
