using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class UpdateDatabaseLoanTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, new DateTime(2022, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 3,
                columns: new[] { "DueDate", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 2.0, new DateTime(2022, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "LoanId", "DueDate", "Fee", "MemberID", "ReturnDate", "StartDate" },
                values: new object[] { 5, new DateTime(2022, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "BookCopyLoans",
                columns: new[] { "BookCopyId", "LoanId" },
                values: new object[] { 5, 5 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 14, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4479), new DateTime(2020, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 2, 28, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4441) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2020, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 3,
                columns: new[] { "DueDate", "ReturnDate", "StartDate" },
                values: new object[] { new DateTime(2020, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 14, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4495), 0.0, new DateTime(2022, 2, 28, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4494) });
        }
    }
}
