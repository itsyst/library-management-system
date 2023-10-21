using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_ImageBinary_Values : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "Description",
                value: "King Lear is a tragedy written by William Shakespeare. It depicts the gradual descent into madness of the title character.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "Description",
                value: "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "Description",
                value: "Dale Carnegie had an understanding of human nature that will never be outdated.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 12,
                column: "Description",
                value: "This book constitutes the research workshops, doctoral symposium and panel summaries presented at the 20th.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 2,
                column: "Description",
                value: "King Lear is a tragedy written by William Shakespeare. It depicts the gradual descent into madness of the title character, after he disposes of his kingdom by giving bequests to two of his three daughters egged on by their continual flattery, bringing tragic consequences for all.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 4,
                column: "Description",
                value: "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 7,
                column: "Description",
                value: "Dale Carnegie had an understanding of human nature that will never be outdated. Financial success, Carnegie believed, is due 15 percent to professional knowledge and 85 percent to the ability to express ideas, to assume leadership, and to arouse enthusiasm among people.");

            migrationBuilder.UpdateData(
                table: "BookDetails",
                keyColumn: "ID",
                keyValue: 12,
                column: "Description",
                value: "This  book constitutes the research workshops, doctoral symposium and panel summaries presented at the 20th International Conference on Agile Software Development");
        }
    }
}
