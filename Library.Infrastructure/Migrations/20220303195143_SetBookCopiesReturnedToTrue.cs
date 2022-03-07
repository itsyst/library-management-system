using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class SetBookCopiesReturnedToTrue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 1,
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "IsAvailable",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 1,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "IsAvailable",
                value: false);
        }
    }
}
