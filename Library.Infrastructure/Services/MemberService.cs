using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library.Infrastructure.Services
{
    public class MemberService : IMemberService
    {

        private readonly ApplicationDbContext _context;


        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Member FindMember(int id)
        {
            var member = _context.Members.Where(m => m.Id == id).Include(m => m.Loans).ThenInclude(l => l.BookCopyLoans).ThenInclude(b => b.BookCopy).ThenInclude(b => b.Details).ToList().Last();

            return member;
        }

        public void AddMember(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
        }

        public void DeleteMember(int id)
        {
            var member = FindMember(id);

            _context.Remove(member);
            _context.SaveChanges();
        }

        public void UpdateMember(Member memberToUpdate)
        {
            _context.Update(memberToUpdate);
            _context.SaveChanges();
        }

        public IList<Member> GetAllmembers()
        {
            return _context.Members.ToList();
        }
    }
}
