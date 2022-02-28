using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    public partial class ChangeFeeTypeToDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Fee",
                table: "Loans",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 14, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4479), 0.0, new DateTime(2022, 2, 28, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4441) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                column: "Fee",
                value: 0.0);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 3,
                column: "Fee",
                value: 0.0);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 14, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4495), 0.0, new DateTime(2022, 2, 28, 12, 41, 12, 876, DateTimeKind.Local).AddTicks(4494) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Fee",
                table: "Loans",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 1,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 11, 17, 22, 22, 998, DateTimeKind.Local).AddTicks(3054), 0, new DateTime(2022, 2, 25, 17, 22, 22, 998, DateTimeKind.Local).AddTicks(3003) });

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 2,
                column: "Fee",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 3,
                column: "Fee",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Loans",
                keyColumn: "LoanId",
                keyValue: 4,
                columns: new[] { "DueDate", "Fee", "StartDate" },
                values: new object[] { new DateTime(2022, 3, 11, 17, 22, 22, 998, DateTimeKind.Local).AddTicks(3070), 0, new DateTime(2022, 2, 25, 17, 22, 22, 998, DateTimeKind.Local).AddTicks(3068) });
        }
    }
}
