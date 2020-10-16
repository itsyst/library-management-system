using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Library.Infrastructure.Migrations
{
    public partial class addAuthors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Laurelli Rolf");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Jordan B Peterson");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Annmarie Palm");

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 4, "Dale Carnegie" },
                    { 5, "Bo Gustafsson" },
                    { 6, "Brian Tracy " },
                    { 7, "Stephen Denning" },
                    { 8, "Geoff Watts" },
                    { 9, "David J Anderson" },
                    { 10, "Rashina Hoda" },
                    { 11, "William Shakespeare" },
                    { 12, "Villiam Skakspjut" },
                    { 13, "Robert C. Martin" }
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "William Shakespeare");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Villiam Skakspjut");

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Robert C. Martin");

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 2, 10, 58, 36, 74, DateTimeKind.Local).AddTicks(9050), new DateTime(2020, 2, 17, 10, 58, 36, 72, DateTimeKind.Local).AddTicks(5014) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { new DateTime(2020, 3, 2, 10, 58, 36, 75, DateTimeKind.Local).AddTicks(842), new DateTime(2020, 2, 17, 10, 58, 36, 75, DateTimeKind.Local).AddTicks(840) });
        }
    }
}
