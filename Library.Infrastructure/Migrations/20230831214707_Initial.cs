using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Library.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 55, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SSN = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookDetails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ISBN = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorID = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
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
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fee = table.Column<double>(type: "REAL", nullable: false),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
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
                    BookCopyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DetailsId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    BookCopyId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    { 1, "Laurelli Rolf" },
                    { 2, "Jordan B Peterson" },
                    { 3, "Annmarie Palm" },
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

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Name", "SSN" },
                values: new object[,]
                {
                    { 1, "Daniel Graham", "19855666-0001" },
                    { 2, "Eric Howell", "19555666-0002" },
                    { 3, "Patricia Lebsack", "19555666-0003" },
                    { 4, "Kalle Runolfsdottir", "19555666-0004" },
                    { 5, "Linus Reichert", "19555666-0005" }
                });

            migrationBuilder.InsertData(
                table: "BookDetails",
                columns: new[] { "ID", "AuthorID", "Description", "ISBN", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Arguably Shakespeare's greatest tragedy", "1472518381", "Hamlet" },
                    { 2, 1, "King Lear is a tragedy written by William Shakespeare. It depicts the gradual descent into madness of the title character, after he disposes of his kingdom by giving bequests to two of his three daughters egged on by their continual flattery, bringing tragic consequences for all.", "9780141012292", "King Lear" },
                    { 3, 2, "An intense drama of love, deception, jealousy and destruction.", "1853260185", "Othello" },
                    { 4, 4, "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.", "9789147107483", "Affärsmannaskap för ingenjörer, jurister och alla andra specialister" },
                    { 5, 5, "12 Rules for Life offers a deeply rewarding antidote to the chaos in our lives: eternal truths applied to our modern problems. ", "9780345816023", "12 Rules For Life " },
                    { 6, 6, "Denna eminenta bok handlar om hur man ska behandla sina affärskontakter för att de ska känna sig trygga med dig som affärspartner. ", "9789147122103", "Business behavior" },
                    { 7, 7, "Dale Carnegie had an understanding of human nature that will never be outdated. Financial success, Carnegie believed, is due 15 percent to professional knowledge and 85 percent to the ability to express ideas, to assume leadership, and to arouse enthusiasm among people.", "9781439199190", "How to Win Friends and Influence People" },
                    { 8, 8, "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.", "9789186293321", "Förhandla : från strikta regler till dirty tricks" },
                    { 9, 9, "Tracy teaches readers how to utilize the six key negotiating styles ", "9780814433195", "Negotiation " },
                    { 10, 10, "The basics of being a ScrumMaster are fairly straightforward: Facilitate the Scrum process and remove impediments. ", "9780957587403", "Scrum Mastery " },
                    { 11, 11, "Optimize the effectiveness of your business, to produce fit-for-purpose products and services that delight your customers, making them loyal to your brand and increasing your share, revenues and margins.", "9780984521401", "Kanban " },
                    { 12, 12, "This  book constitutes the research workshops, doctoral symposium and panel summaries presented at the 20th International Conference on Agile Software Development", "9783030301255", " Agile Processes in Software Engineering and Extreme Programming" },
                    { 13, 13, "The Age of Agile helps readers master the three laws of Agile Management (team, customer, network)", "9780814439098", "THE AGE OF AGILE " }
                });

            migrationBuilder.InsertData(
                table: "Loans",
                columns: new[] { "LoanId", "DueDate", "Fee", "MemberID", "ReturnDate", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 3, new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2022, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 24.0, 1, new DateTime(2022, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2022, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 2, new DateTime(2022, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2022, 2, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2022, 2, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2022, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.0, 5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 3, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "BookCopies",
                columns: new[] { "BookCopyId", "DetailsId", "IsAvailable" },
                values: new object[,]
                {
                    { 1, 1, true },
                    { 2, 2, false },
                    { 3, 3, false },
                    { 4, 4, true },
                    { 5, 5, true },
                    { 6, 6, false },
                    { 7, 7, true },
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
                    { 1, 2 },
                    { 2, 3 },
                    { 2, 5 },
                    { 3, 1 },
                    { 3, 4 },
                    { 4, 1 },
                    { 6, 6 },
                    { 7, 2 },
                    { 12, 1 }
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

        /// <inheritdoc />
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
