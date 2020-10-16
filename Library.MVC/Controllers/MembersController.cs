using Library.Application.Interfaces;
using Library.Domain;
using Library.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace Library.MVC.Controllers
{
    public class MembersController : Controller
    {
        public readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public ActionResult Index()
        {
            var vm = new MemberVm();

            vm.Members = _memberService.GetAllmembers();

            return View(vm);
        }

        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new MemberVm
            {
                Member = _memberService.FindMember(id)
            };

            return View(vm);
        }

        public ActionResult Create()
        {
            var vm = new MemberVm();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MemberVm vm)
        {
            try
            {
                var newMember = new Member();

                newMember.SSN = vm.SSN;
                newMember.Name = vm.Name;
                _memberService.AddMember(newMember);
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new MemberVm
            {
                Member = _memberService.FindMember(id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MemberVm vm)
        {
            try
            {
                var memberToUpdate = new Member();

                memberToUpdate.Id = vm.Id;
                memberToUpdate.Name = vm.Name;
                memberToUpdate.SSN = vm.SSN;

                _memberService.UpdateMember(memberToUpdate);
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Delete(int id, bool blockDelete)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new MemberVm
            {
                Member = _memberService.FindMember(id)
            };

            if (blockDelete)
            {
                vm.BlockDelete = true;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(MemberVm vm)
        {
            try
            {
                // we first check if the member doesnt have any active loans
                // else the user needs to end these loans first
                var memberToCheckForLoans = _memberService.FindMember(vm.Id);
                var activeLoans = false;

                foreach (var loan in memberToCheckForLoans.Loans)
                {
                    if (loan.ReturnDate < loan.StartDate)
                    {
                        activeLoans = true;
                        break;
                    }
                }
                if (activeLoans == true)
                {
                    return RedirectToAction(nameof(Delete), new { id = vm.Id, blockDelete = true });
                }

                _memberService.DeleteMember(vm.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }
    }
}