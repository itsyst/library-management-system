using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageBinarycolumntobookdetailstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageBinary",
                table: "BookDetails",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 1,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 3,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 5,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 6,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 8,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 9,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 10,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 11,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 12,
                column: "ImageBinary",
                value: "");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 13,
                column: "ImageBinary",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBinary",
                table: "BookDetails");
        }
    }
}
