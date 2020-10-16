using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain
{
    public class Member
    {
        public int Id { get; set; }
        public string SSN { get; set; }
        public string Name { get; set; }
        public IList<Loan> Loans { get; set; }
    }
}
