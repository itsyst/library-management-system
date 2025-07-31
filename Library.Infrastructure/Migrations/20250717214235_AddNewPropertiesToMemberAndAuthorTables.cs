using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropertiesToMemberAndAuthorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Members",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookCopies",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 6,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "CreatedDate",
                value: new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Notes" },
                values: new object[] { new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013), null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Notes" },
                values: new object[] { new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013), null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Notes" },
                values: new object[] { new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013), null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedDate", "Notes" },
                values: new object[] { new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013), null });

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedDate", "Notes" },
                values: new object[] { new DateTime(2025, 7, 17, 21, 42, 35, 206, DateTimeKind.Utc).AddTicks(1013), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Members");

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
    }
}
