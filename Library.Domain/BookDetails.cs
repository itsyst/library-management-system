﻿using Library.Domain.Utilities;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain
{
    public class BookDetails
    {
        public int ID { get; set; }
        public string ISBN { get; set; } = IsbnGenerator.GenerateRandomIsbn13();
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Author")]
        public int AuthorID { get; set; }
        public Author? Author { get; set; }
        public string Description { get; set; } = string.Empty;

        public string? ImageBinary { get; set; }

        [Range(0, 1000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public ICollection<BookCopy> Copies { get; set; }
    }
}
