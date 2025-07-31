using CsvHelper.Configuration;
using Library.Domain.Entities;
using Library.Domain.Enums;

public sealed class MemberCsvMap : ClassMap<Member>
{
    public MemberCsvMap()
    {
        Map(m => m.Name).Name("Namn");
        Map(m => m.Email).Name("E-post");
        Map(m => m.SSN).Name("Personnummer");
        Map(m => m.PhoneNumber).Name("Telefon");
        Map(m => m.Address).Name("Adress");
        Map(m => m.DateOfBirth).Name("Födelsedatum");
        Map(m => m.MembershipStartDate)
            .Name("Medlemskap Startdatum")
            .TypeConverterOption.Format("yyyy-MM-dd");
 
        Map(m => m.Status).Name("Status")
            .Convert(args => args.Row.GetField("Status") switch
            {
                "Aktiv" => MembershipStatus.Active,
                "Inaktiv" => MembershipStatus.Inactive,
                _ => MembershipStatus.Active
            });

        Map(m => m.CreatedDate).Name("Skapelsedatum");
        Map(m => m.MaxLoans).Name("MaxLån");
        Map(m => m.CreatedBy).Name("Skapad av");
        Map(m => m.UpdatedBy).Name("Uppdaterad av");
        Map(m => m.IsDeleted).Name("Raderad");
        Map(m => m.DeletedDate).Name("Raderingsdatum");
        Map(m => m.DeletedBy).Name("Raderad av");
        Map(m => m.Notes).Name("Anteckningar");
 
    }
}