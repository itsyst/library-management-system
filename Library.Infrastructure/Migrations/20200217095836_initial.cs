using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Library.Infrastructure.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 55, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SSN = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookDetails",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ISBN = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    AuthorID = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BookDetails_Authors_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    LoanId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(nullable: false),
                    ReturnDate = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    MemberID = table.Column<int>(nullable: false),
                    Fee = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.LoanId);
                    table.ForeignKey(
                        name: "FK_Loans_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCopies",
                columns: table => new
                {
                    BookCopyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailsId = table.Column<int>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopies", x => x.BookCopyId);
                    table.ForeignKey(
                        name: "FK_BookCopies_BookDetails_DetailsId",
                        column: x => x.DetailsId,
                        principalTable: "BookDetails",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCopyLoans",
                columns: table => new
                {
                    BookCopyId = table.Column<int>(nullable: false),
                    LoanId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopyLoans", x => new { x.BookCopyId, x.LoanId });
                    table.ForeignKey(
                        name: "FK_BookCopyLoans_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "BookCopyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCopyLoans_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "LoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "William Shakespeare" },
                    { 2, "Villiam Skakspjut" },
                    { 3, "Robert C. Martin" }
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Name", "SSN" },
                values: new object[,]
                {
                    { 1, "Daniel Graham", "555666-0001" },
                    { 2, "Eric Howell", "555666-0002" },
                    { 3, "Patricia Lebsack", "555666-0003" },
                    { 4, "Kalle Runolfsdottir", "555666-0004" },
                    { 5, "Linus Reichert", "555666-0005" }
                });

            migrationBuilder.InsertData(
                table: "BookDetails",
                columns: new[] { "ID", "AuthorID", "Description", "ISBN", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Arguably Shakespeare's greatest tragedy", "1472518381", "Hamlet" },
                    { 2, 1, "King Lear is a tragedy written by William Shakespeare. It depicts the gradual descent into madness of the title character, after he disposes of his kingdom by giving bequests to two of his three daughters egged on by their continual flattery, bringing tragic consequences for all.", "9780141012292", "King Lear" },
                    { 3, 2, "An intense drama of love, deception, jealousy and destruction.", "1853260185", "Othello" }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "LoanId", "DueDate", "Fee", "MemberID", "ReturnDate", "StartDate" },
                values: new object[,]
                {
                    { 2, new DateTime(2020, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2020, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2020, 3, 2, 10, 58, 36, 75, DateTimeKind.Local).AddTicks(842), 0, 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 2, 17, 10, 58, 36, 75, DateTimeKind.Local).AddTicks(840) },
                    { 1, new DateTime(2020, 3, 2, 10, 58, 36, 74, DateTimeKind.Local).AddTicks(9050), 0, 3, new DateTime(2020, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 2, 17, 10, 58, 36, 72, DateTimeKind.Local).AddTicks(5014) }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "DetailsId", "IsAvailable" },
                values: new object[,]
                {
                    { 1, 1, true },
                    { 2, 1, true },
                    { 3, 1, true },
                    { 5, 2, true },
                    { 4, 3, true },
                    { 6, 3, true },
                    { 7, 3, true },
                    { 8, 3, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_DetailsId",
                table: "BookCopies",
                column: "DetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopyLoans_LoanId",
                table: "BookCopyLoans",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_BookDetails_AuthorID",
                table: "BookDetails",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_MemberID",
                table: "Loans",
                column: "MemberID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCopyLoans");

            migrationBuilder.DropTable(
                name: "BookCopies");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "BookDetails");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
