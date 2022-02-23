using Library.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Models
{
    public class LoanCreateVm
    {

        public int LoanId { get; set; }

        [Display(Name = "Member ID")]
        public SelectList SelectListMemberId { get; set; }
        [Required]
        public int MemberId { get; set; }

        [Display(Name = "Id of book nr 1")]
        public SelectList SelectListBookCopy1 { get; set; }
        //[Required]
        public int? BookCopyID1 { get; set; }

        [Display(Name = "Id of book nr 2")]
        public SelectList SelectListBookCopy2 { get; set; }
        public int? BookCopyID2 { get; set; }

        [Display(Name = "Id of book nr 3")]
        public SelectList SelectListBookCopy3 { get; set; }
        public int? BookCopyID3 { get; set; }

    }
}
