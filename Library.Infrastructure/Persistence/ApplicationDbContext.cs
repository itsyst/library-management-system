using Library.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<BookDetails> BookDetails { get; set; }
        public DbSet<BookCopyLoan> BookCopyLoans { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureBookDetails(modelBuilder);
            ConfigureAuthor(modelBuilder);
            ConfigureBookCopyLoan(modelBuilder);

            SeedDatabase(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SeedDatabase(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, Name = "Laurelli Rolf" },
                new Author { Id = 2, Name = "Jordan B Peterson" },
                new Author { Id = 3, Name = "Annmarie Palm" },
                new Author { Id = 4, Name = "Dale Carnegie" },
                new Author { Id = 5, Name = "Bo Gustafsson" },
                new Author { Id = 6, Name = "Brian Tracy " },
                new Author { Id = 7, Name = "Stephen Denning" },
                new Author { Id = 8, Name = "Geoff Watts" },
                new Author { Id = 9, Name = "David J Anderson" },
                new Author { Id = 10, Name = "Rashina Hoda" },
                new Author { Id = 11, Name = "William Shakespeare" },
                new Author { Id = 12, Name = "Villiam Skakspjut" },
                new Author { Id = 13, Name = "Robert C. Martin" }
                );
            modelBuilder.Entity<BookDetails>().HasData(
                new BookDetails
                {
                    ID = 1,
                    AuthorID = 1,
                    Title = "Hamlet",
                    ISBN = "1472518381",
                    Description = "Arguably Shakespeare's greatest tragedy"
                },
                new BookDetails
                {
                    ID = 2,
                    AuthorID = 1,
                    Title = "King Lear",
                    ISBN = "9780141012292",
                    Description = "King Lear is a tragedy written by William Shakespeare. It depicts the gradual descent into madness of the title character, after he disposes of his kingdom by giving bequests to two of his three daughters egged on by their continual flattery, bringing tragic consequences for all."
                },
                new BookDetails
                {
                    ID = 3,
                    AuthorID = 2,
                    Title = "Othello",
                    ISBN = "1853260185",
                    Description = "An intense drama of love, deception, jealousy and destruction."
                },
                new BookDetails
                {
                    ID = 4,
                    ISBN = "9789147107483",
                    Title = "Affärsmannaskap för ingenjörer, jurister och alla andra specialister",
                    Description = "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.",
                    AuthorID = 4,
                },
                new BookDetails
                {
                    ID = 5,
                    ISBN = "9780345816023",
                    Title = "12 Rules For Life ",
                    Description = "12 Rules for Life offers a deeply rewarding antidote to the chaos in our lives: eternal truths applied to our modern problems. ",
                    AuthorID = 5,
                },
                    new BookDetails
                    {
                        ID = 6,
                        ISBN = "9789147122103",
                        Title = "Business behavior",
                        Description = "Denna eminenta bok handlar om hur man ska behandla sina affärskontakter för att de ska känna sig trygga med dig som affärspartner. ",
                        AuthorID = 6,
                    },
                    new BookDetails
                    {
                        ID = 7,
                        ISBN = "9781439199190",
                        Title = "How to Win Friends and Influence People",
                        Description = "Dale Carnegie had an understanding of human nature that will never be outdated. Financial success, Carnegie believed, is due 15 percent to professional knowledge and 85 percent to the ability to express ideas, to assume leadership, and to arouse enthusiasm among people.",
                        AuthorID = 7,
                    },
                    new BookDetails
                    {
                        ID = 8,
                        ISBN = "9789186293321",
                        Title = "Förhandla : från strikta regler till dirty tricks",
                        Description = "I Affärsmannaskap har Rolf Laurelli summerat sin långa erfarenhet av konsten att göra affärer. Med boken hoppas han kunna locka fram dina affärsinstinkter.",
                        AuthorID = 8,
                    },
                    new BookDetails
                    {
                        ID = 9,
                        ISBN = "9780814433195",
                        Title = "Negotiation ",
                        Description = "Tracy teaches readers how to utilize the six key negotiating styles ",
                        AuthorID = 9,
                    },
                    new BookDetails
                    {
                        ID = 13,
                        ISBN = "9780814439098",
                        Title = "THE AGE OF AGILE ",
                        Description = "The Age of Agile helps readers master the three laws of Agile Management (team, customer, network)",
                        AuthorID = 13,
                    },
                    new BookDetails
                    {
                        ID = 10,
                        ISBN = "9780957587403",
                        Title = "Scrum Mastery ",
                        Description = "The basics of being a ScrumMaster are fairly straightforward: Facilitate the Scrum process and remove impediments. ",
                        AuthorID = 10,
                    },
                    new BookDetails
                    {
                        ID = 11,
                        ISBN = "9780984521401",
                        Title = "Kanban ",
                        Description = "Optimize the effectiveness of your business, to produce fit-for-purpose products and services that delight your customers, making them loyal to your brand and increasing your share, revenues and margins.",
                        AuthorID = 11,
                    },
                    new BookDetails
                    {
                        ID = 12,
                        ISBN = "9783030301255",
                        Title = " Agile Processes in Software Engineering and Extreme Programming",
                        Description = "This  book constitutes the research workshops, doctoral symposium and panel summaries presented at the 20th International Conference on Agile Software Development",
                        AuthorID = 12,
                    }

                );

            modelBuilder.Entity<Member>().HasData(
                new Member { Id = 1, SSN = "555666-0001", Name = "Daniel Graham" },
                new Member { Id = 2, SSN = "555666-0002", Name = "Eric Howell" },
                new Member { Id = 3, SSN = "555666-0003", Name = "Patricia Lebsack" },
                new Member { Id = 4, SSN = "555666-0004", Name = "Kalle Runolfsdottir" },
                new Member { Id = 5, SSN = "555666-0005", Name = "Linus Reichert" }
            );
            modelBuilder.Entity<Loan>().HasData(
             new Loan { LoanId = 1, MemberID = 3, ReturnDate = new DateTime(2020, 5, 4) },
             new Loan { LoanId = 2, StartDate = new DateTime(2020, 1, 5), DueDate = new DateTime(2020, 1, 19), MemberID = 1 },
             new Loan { LoanId = 3, StartDate = new DateTime(2020, 1, 3), DueDate = new DateTime(2020, 1, 17), MemberID = 2 },
             new Loan { LoanId = 4, MemberID = 2 }
         );
            modelBuilder.Entity<BookCopy>().HasData(
                new BookCopy { BookCopyId = 1, DetailsId = 1 },
                new BookCopy { BookCopyId = 2, DetailsId = 1 },
                new BookCopy { BookCopyId = 3, DetailsId = 1 },
                new BookCopy { BookCopyId = 4, DetailsId = 3 },
                new BookCopy { BookCopyId = 5, DetailsId = 2 },
                new BookCopy { BookCopyId = 6, DetailsId = 3 },
                new BookCopy { BookCopyId = 7, DetailsId = 3 },
                new BookCopy { BookCopyId = 8, DetailsId = 3 }
            );
        }

        private static void ConfigureAuthor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().Property(x => x.Name).HasMaxLength(55);
        }

        private static void ConfigureBookDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookDetails>().HasKey(x => x.ID);
            modelBuilder.Entity<BookDetails>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorID);
        }

        private static void ConfigureBookCopyLoan(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookCopyLoan>()
                .HasKey(x => new { x.BookCopyId, x.LoanId });

            modelBuilder.Entity<BookCopyLoan>()
                .HasOne(pt => pt.BookCopy)
                .WithMany(p => p.BookCopyLoans)
                .HasForeignKey(pt => pt.BookCopyId);

            modelBuilder.Entity<BookCopyLoan>()
                .HasOne(pt => pt.Loan)
                .WithMany(t => t.BookCopyLoans)
                .HasForeignKey(pt => pt.LoanId);
        }
    }
}
