using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class LoanDeleteVm
    {
        [Display(Name = "LoanId")]
        public SelectList LoanIdList { get; set; }
        [Required]
        public int LoanId { get; set; }
    }
}
