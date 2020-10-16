using System.Collections.Generic;

namespace Library.Domain
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<BookDetails> Books { get; set; }
    }
}