using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Library.Infrastructure.Migrations
{
    public partial class addBooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BookDetails",
                columns: new[] { "ID", "AuthorID", "Description", "ISBN", "Title" },
                values: new object[,]
                {
                    { 4, 4, "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.", "9789147107483", "Affärsmannaskap för ingenjörer, jurister och alla andra specialister" },
                    { 5, 5, "12 Rules for Life offers a deeply rewarding antidote to the chaos in our lives: eternal truths applied to our modern problems. ", "9780345816023", "12 Rules For Life " },
                    { 6, 6, "Denna eminenta bok handlar om hur man ska behandla sina affärskontakter för att de ska känna sig trygga med dig som affärspartner. ", "9789147122103", "Business behavior" },
                    { 7, 7, "Dale Carnegie had an understanding of human nature that will never be outdated. Financial success, Carnegie believed, is due 15 percent to professional knowledge and 85 percent to the ability to express ideas, to assume leadership, and to arouse enthusiasm among people.", "9781439199190", "How to Win Friends and Influence People" },
                    { 8, 8, "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.", "9789186293321", "Förhandla : från strikta regler till dirty tricks" },
                    { 9, 9, "Tracy teaches readers how to utilize the six key negotiating styles ", "9780814433195", "Negotiation " },
                    { 13, 13, "The Age of Agile helps readers master the three laws of Agile Management (team, customer, network)", "9780814439098", "THE AGE OF AGILE " },
                    { 10, 10, "The basics of being a ScrumMaster are fairly straightforward: Facilitate the Scrum process and remove impediments. ", "9780957587403", "Scrum Mastery " },
                    { 11, 11, "Optimize the effectiveness of your business, to produce fit-for-purpose products and services that delight your customers, making them loyal to your brand and increasing your share, revenues and margins.", "9780984521401", "Kanban " },
                    { 12, 12, "This  book constitutes the research workshops, doctoral symposium and panel summaries presented at the 20th International Conference on Agile Software Development", "9783030301255", " Agile Processes in Software Engineering and Extreme Programming" }
                });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 3, 17, 38, 51, 528, DateTimeKind.Local).AddTicks(4542), new DateTime(2020, 2, 18, 17, 38, 51, 524, DateTimeKind.Local).AddTicks(9934) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 3, 17, 38, 51, 528, DateTimeKind.Local).AddTicks(7257), new DateTime(2020, 2, 18, 17, 38, 51, 528, DateTimeKind.Local).AddTicks(7253) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 3, 15, 29, 18, 25, DateTimeKind.Local).AddTicks(2222), new DateTime(2020, 2, 18, 15, 29, 18, 22, DateTimeKind.Local).AddTicks(1250) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 3, 15, 29, 18, 25, DateTimeKind.Local).AddTicks(4823), new DateTime(2020, 2, 18, 15, 29, 18, 25, DateTimeKind.Local).AddTicks(4820) });
        }
    }
}
