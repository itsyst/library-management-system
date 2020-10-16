using Library.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class BookDetailsVm
    {
        public BookDetails Book = new BookDetails();
        public List<BookCopy> BookCopies = new List<BookCopy>();
    }
}