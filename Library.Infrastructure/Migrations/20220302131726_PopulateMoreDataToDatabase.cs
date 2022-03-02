using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class PopulateMoreDataToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 2,
                column: "DetailsId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 3,
                column: "DetailsId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 4,
                column: "DetailsId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "DetailsId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 6,
                column: "DetailsId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 7,
                column: "DetailsId",
                value: 7);

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "DetailsId", "IsAvailable" },
                values: new object[,]
                {
                    { 9, 12, true },
                    { 10, 12, true },
                    { 11, 5, true },
                    { 12, 4, true },
                    { 13, 8, true },
                    { 14, 1, true },
                    { 15, 7, true },
                    { 16, 11, true },
                    { 17, 11, true },
                    { 18, 2, true },
                    { 19, 9, true },
                    { 20, 9, true },
                    { 21, 13, true },
                    { 22, 5, true },
                    { 24, 10, true },
                    { 25, 10, true },
                    { 26, 13, true },
                    { 27, 13, true }
                });

            migrationBuilder.InsertData(
                table: "BookCopyLoans",
                columns: new[] { "BookCopyId", "LoanId" },
                values: new object[,]
                {
                    { 1, 5 },
                    { 2, 5 },
                    { 3, 1 },
                    { 7, 2 }
                });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee" },
                values: new object[] { new DateTime(2022, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), 12.0 });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee" },
                values: new object[] { new DateTime(2022, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0 });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "LoanId", "DueDate", "Fee", "MemberID", "ReturnDate", "StartDate" },
                values: new object[] { 6, new DateTime(2022, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "BookCopyLoans",
                columns: new[] { "BookCopyId", "LoanId" },
                values: new object[] { 6, 6 });

            migrationBuilder.InsertData(
                table: "BookCopyLoans",
                columns: new[] { "BookCopyId", "LoanId" },
                values: new object[] { 12, 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 6, 6 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "BookCopyLoans",
                keyColumns: new[] { "BookCopyId", "LoanId" },
                keyValues: new object[] { 12, 1 });

            migrationBuilder.DeleteData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 2,
                column: "DetailsId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 3,
                column: "DetailsId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 4,
                column: "DetailsId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 5,
                column: "DetailsId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 6,
                column: "DetailsId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "BookCopyId",
                keyValue: 7,
                column: "DetailsId",
                value: 3);

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "DetailsId", "IsAvailable" },
                values: new object[] { 8, 3, true });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                columns: new[] { "DueDate", "Fee" },
                values: new object[] { new DateTime(2022, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0 });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee" },
                values: new object[] { new DateTime(2022, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 2.0 });
        }
    }
}
