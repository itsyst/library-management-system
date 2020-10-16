using Library.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class MemberVm
    {
        // Needed for the index page to list all members
        public IList <Member> Members { get; set; } = new List<Member>();

        // needed for the details page 
        public Member Member = new Member();

        // Needed for the edit/create page
        [Display(Name = "Id")]
        public int Id { get; set; }
        
        [Display(Name = "Name")]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "SSN")]
        [MaxLength(12)]
        [Required]
        public string SSN { get; set; }

        // For delete - to block if there are active loans
        public bool BlockDelete { get; set; }
        
    }
}
