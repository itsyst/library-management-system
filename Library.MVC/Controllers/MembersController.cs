using Library.Application.Interfaces;
using Library.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Library.MVC.Controllers
{
#nullable disable
    public class MembersController : Controller
    {
        public readonly IMemberService _memberService;
        public readonly ILoanService _loanService;

        public MembersController(IMemberService memberService, ILoanService loanService)
        {
            _memberService = memberService;
            _loanService = loanService;
        }

        public async Task<IActionResult> Index()
        {
            var members = await _memberService.GetAllMembersAsync(orderBy: m => m.OrderBy(m => m.Name), includeProperties: m => m.Loans);
            return View(members);
        }

        public async Task<IActionResult> Create()
        {
            await Task.CompletedTask;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Member member)
        {
            var memberInDb = await _memberService.GetAllAsync();
            foreach (var item in memberInDb)
            {
                if (member.SSN.ToLower().Trim() != item.SSN.ToLower().Trim() &&
                    member.Name.ToLower().Trim() != item.Name.ToLower().Trim())
                {
                    try
                    {
                        await _memberService.AddAsync(member);
                        TempData["Success"] = "Member created successfully.";

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbException)
                    {
                        TempData["Error"] = "Something went wrong.";
                        return View();
                    }
                }
                else
                {
                    TempData["Error"] = "Member already exists.";
                    return View(nameof(Create));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
                return NotFound();

            var member = await _memberService.GetMemberByIdAsync(id, includeProperties: true);

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, [Bind("Id, SSN, Name")] Member member)
        {
            if (id != member.Id)
                return NotFound();

            try
            {
                await _memberService.UpdateAsync(member);
                TempData["Success"] = "Author updated successfully.";

                return RedirectToAction(nameof(Index));

            }
            catch (DbUpdateConcurrencyException)
            {
                if (await MemberExists(member.Id))
                {
                    return NotFound();
                }
                else
                {
                    TempData["Error"] = "An Unexpected Error Occurred!";
                }
            }
            return View(member);
        }
        private async Task<bool> MemberExists(int id)
        {
            return await _memberService.GetByIdAsync(id) != null;
        }

        #region API CALLS
        // DELETE: Members/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var memberInDb = await _memberService.GetByIdAsync(id);

            var loanInDb = await _loanService.GetLoanOrDefaultAsync(filter: b => b.Member.Id == memberInDb.Id);

            if (loanInDb != null)
                return Json(new { error = true, message = "You can not delete this member as long it has loans refering to it (check loans)!" });

            await _memberService.DeleteAsync(id);
            return Json(new { success = true, message = "Member deleted successfully." });

        }
        #endregion
    }
}