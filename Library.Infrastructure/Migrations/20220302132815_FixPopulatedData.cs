using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class FixPopulatedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                column: "Fee",
                value: 24.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                column: "Fee",
                value: 12.0);
        }
    }
}
