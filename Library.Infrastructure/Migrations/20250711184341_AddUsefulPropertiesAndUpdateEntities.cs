using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsefulPropertiesAndUpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 11, 18, 43, 40, 585, DateTimeKind.Utc).AddTicks(5378));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 20, 58, 4, 239, DateTimeKind.Utc).AddTicks(8072));
        }
    }
}
