using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class SetBookCopiesLoanedToFalse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 1,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 2,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 3,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 6,
                column: "IsAvailable",
                value: false);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 24.0, new DateTime(2022, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                keyValue: 2,
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 3,
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "IsAvailable",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 6,
                column: "IsAvailable",
                value: true);

            migrationBuilder.InsertData(
                table: "BookCopyLoans",
                columns: new[] { "BookCopyId", "LoanId" },
                values: new object[,]
                {
                    { 1, 5 },
                    { 5, 5 }
                });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), 128.0, new DateTime(2022, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
