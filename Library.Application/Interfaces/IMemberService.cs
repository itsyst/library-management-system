using Library.Domain;
using System.Collections.Generic;

namespace Library.Application.Interfaces
{
    public interface IMemberService
    {
 
        Member FindMember(int id);

        void AddMember(Member member);

        void UpdateMember (Member member);

        void DeleteMember(int id);

        IList<Member> GetAllmembers();
    }
}
