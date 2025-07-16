using Library.Domain.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace Library.Infrastructure.Persistence;

#nullable disable
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
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
        // Configure all entities
        ConfigureAuditableEntity(modelBuilder);
        ConfigureAuthor(modelBuilder);
        ConfigureBookDetails(modelBuilder);
        ConfigureBookCopy(modelBuilder);
        ConfigureMember(modelBuilder);
        ConfigureLoan(modelBuilder);
        ConfigureBookCopyLoan(modelBuilder);

        // Configure value converters for enums
        ConfigureEnumConverters(modelBuilder);

        // Add indexes for performance
        ConfigureIndexes(modelBuilder);

        // Seed database with updated data
        SeedDatabase(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    #region Entity Configurations

    private static void ConfigureAuditableEntity(ModelBuilder modelBuilder)
    {
        // List of entities that are required ends of relationships
        var requiredEndEntities = new[] { "BookDetails", "Loan", "BookCopy", "BookCopyLoan" };

        // Configure common auditable properties for all entities that inherit from AuditableEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.CreatedDate))
                    //.HasDefaultValueSql("GETUTCDATE()") // SQL Server
                    .HasDefaultValueSql("datetime('now')")  // SQLite format
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.CreatedBy))
                    .HasMaxLength(100);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.UpdatedBy))
                    .HasMaxLength(100);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.DeletedBy))
                    .HasMaxLength(100);

                // Global query filter for soft delete
                var entityName = entityType.ClrType.Name;
                if (!requiredEndEntities.Contains(entityName))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Property(parameter, nameof(AuditableEntity.IsDeleted)),
                        Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }

    private static void ConfigureAuthor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            // Primary key
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();

            // Properties configuration
            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Biography)
                .HasMaxLength(2000);

            entity.Property(a => a.Nationality)
                .HasMaxLength(50);

            entity.Property(a => a.Website)
                .HasMaxLength(200);

            entity.Property(a => a.BirthDate)
                .HasColumnType("date");

            entity.Property(a => a.DeathDate)
                .HasColumnType("date");

            // Relationships
            entity.HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if author has books

            // Table configuration
            entity.ToTable("Authors");
        });
    }

    private static void ConfigureBookDetails(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookDetails>(entity =>
        {
            // Primary key
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id)
                  .HasColumnName("ID")  // Maps Id property to ID column
                  .ValueGeneratedOnAdd();

            // Properties configuration
            entity.Property(b => b.ISBN)
                .IsRequired()
                .HasMaxLength(13);

            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(b => b.Subtitle)
                .HasMaxLength(200);

            entity.Property(b => b.Description)
                .HasMaxLength(2000);

            entity.Property(b => b.Publisher)
                .HasMaxLength(100);

            entity.Property(b => b.Language)
                .HasMaxLength(50)
                .HasDefaultValue("English");

            entity.Property(b => b.Genre)
                .HasMaxLength(100);

            entity.Property(b => b.Edition)
                .HasMaxLength(50);

            entity.Property(b => b.PublicationDate)
                .HasColumnType("date");

            entity.Property(b => b.Pages)
                .HasColumnType("int");

            // Foreign key
            entity.Property(b => b.AuthorId)
                .HasColumnName("AuthorID")
                .IsRequired();

            // Relationships
            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(b => b.Copies)
                .WithOne(c => c.Details)
                .HasForeignKey(c => c.DetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("BookDetails");
        });
    }

    private static void ConfigureBookCopy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookCopy>(entity =>
        {
            // Primary key
            entity.HasKey(bc => bc.Id);
            entity.Property(bc => bc.Id).ValueGeneratedOnAdd();

            // Properties configuration
            entity.Property(bc => bc.Location)
                .HasMaxLength(100);

            entity.Property(bc => bc.Barcode)
                .HasMaxLength(50);

            entity.Property(bc => bc.Notes)
                .HasMaxLength(500);

            entity.Property(bc => bc.PurchaseDate)
                .HasColumnType("date");

            entity.Property(bc => bc.PurchasePrice)
                .HasColumnType("decimal(10,2)");

            entity.Property(bc => bc.IsAvailable)
                .HasDefaultValue(true);

            // Foreign key
            entity.Property(bc => bc.DetailsId)
                .IsRequired();

            // Relationships
            entity.HasOne(bc => bc.Details)
                .WithMany(bd => bd.Copies)
                .HasForeignKey(bc => bc.DetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(bc => bc.BookCopyLoans)
                .WithOne(bcl => bcl.BookCopy)
                .HasForeignKey(bcl => bcl.BookCopyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("BookCopies");
        });
    }

    private static void ConfigureMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            // Primary key
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedOnAdd();

            // Properties configuration
            entity.Property(m => m.SSN)
                .IsRequired()
                .HasMaxLength(13);

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(m => m.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(m => m.Address)
                .HasMaxLength(200);

            entity.Property(m => m.DateOfBirth)
                .HasColumnType("date");

            entity.Property(m => m.MembershipStartDate)
                .HasColumnType("date")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(m => m.MaxLoans)
                .HasDefaultValue(3);

            // Relationships
            entity.HasMany(m => m.Loans)
                .WithOne(l => l.Member)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table configuration
            entity.ToTable("Members");
        });
    }

    private static void ConfigureLoan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            // Primary key
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedOnAdd();

            // Properties configuration
            entity.Property(l => l.StartDate)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(l => l.DueDate)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(l => l.ReturnDate)
                .HasColumnType("datetime2");

            entity.Property(l => l.Fee)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            entity.Property(l => l.Notes)
                .HasMaxLength(500);

            // Foreign key
            entity.Property(l => l.MemberId)
                .IsRequired();

            // Relationships
            entity.HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(l => l.BookCopyLoans)
                .WithOne(bcl => bcl.Loan)
                .HasForeignKey(bcl => bcl.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("Loans");
        });
    }

    private static void ConfigureBookCopyLoan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookCopyLoan>(entity =>
        {
            // Composite primary key
            entity.HasKey(bcl => new { bcl.BookCopyId, bcl.LoanId });

            // Relationships
            entity.HasOne(bcl => bcl.BookCopy)
                .WithMany(bc => bc.BookCopyLoans)
                .HasForeignKey(bcl => bcl.BookCopyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bcl => bcl.Loan)
                .WithMany(l => l.BookCopyLoans)
                .HasForeignKey(bcl => bcl.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table configuration
            entity.ToTable("BookCopyLoans");
        });
    }

    #endregion

    #region Enum Converters

    private static void ConfigureEnumConverters(ModelBuilder modelBuilder)
    {
        // BookCondition enum converter
        var bookConditionConverter = new EnumToStringConverter<BookCondition>();
        modelBuilder.Entity<BookCopy>()
            .Property(bc => bc.Condition)
            .HasConversion(bookConditionConverter)
            .HasMaxLength(20);

        // MembershipStatus enum converter
        var membershipStatusConverter = new EnumToStringConverter<MembershipStatus>();
        modelBuilder.Entity<Member>()
            .Property(m => m.Status)
            .HasConversion(membershipStatusConverter)
            .HasMaxLength(20)
            .HasDefaultValue(MembershipStatus.Active);

        // LoanStatus enum converter
        var loanStatusConverter = new EnumToStringConverter<LoanStatus>();
        modelBuilder.Entity<Loan>()
            .Property(l => l.Status)
            .HasConversion(loanStatusConverter)
            .HasMaxLength(20)
            .HasDefaultValue(LoanStatus.Active);
    }

    #endregion

    #region Indexes

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Author indexes
        modelBuilder.Entity<Author>()
            .HasIndex(a => a.Name)
            .HasDatabaseName("IX_Authors_Name");

        // BookDetails indexes
        modelBuilder.Entity<BookDetails>()
            .HasIndex(b => b.ISBN)
            .IsUnique()
            .HasDatabaseName("IX_BookDetails_ISBN");

        modelBuilder.Entity<BookDetails>()
            .HasIndex(b => b.Title)
            .HasDatabaseName("IX_BookDetails_Title");

        modelBuilder.Entity<BookDetails>()
            .HasIndex(b => b.AuthorId)
            .HasDatabaseName("IX_BookDetails_AuthorId");

        // Member indexes
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.SSN)
            .IsUnique()
            .HasDatabaseName("IX_Members_SSN");

        modelBuilder.Entity<Member>()
            .HasIndex(m => m.Email)
            .IsUnique()
            .HasDatabaseName("IX_Members_Email");

        // BookCopy indexes
        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.Barcode)
            .IsUnique()
            .HasDatabaseName("IX_BookCopies_Barcode")
            .HasFilter("[Barcode] IS NOT NULL");

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.DetailsId)
            .HasDatabaseName("IX_BookCopies_DetailsId");

        // Loan indexes
        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.MemberId)
            .HasDatabaseName("IX_Loans_MemberId");

        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.DueDate)
            .HasDatabaseName("IX_Loans_DueDate");

        modelBuilder.Entity<Loan>()
            .HasIndex(l => l.Status)
            .HasDatabaseName("IX_Loans_Status");
    }

    #endregion

    #region Seed Data

    private static void SeedDatabase(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;

        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author
            {
                Id = 1,
                Name = "William Shakespeare",
                Biography = "English playwright, poet, and actor, widely regarded as the greatest writer in the English language.",
                BirthDate = new DateTime(1564, 4, 23),
                DeathDate = new DateTime(1616, 4, 23),
                Nationality = "English",
                CreatedDate = now
            },
            new Author
            {
                Id = 2,
                Name = "Jordan B. Peterson",
                Biography = "Canadian clinical psychologist and professor of psychology.",
                BirthDate = new DateTime(1962, 6, 12),
                Nationality = "Canadian",
                CreatedDate = now
            },
            new Author
            {
                Id = 3,
                Name = "Dale Carnegie",
                Biography = "American writer and lecturer, developer of courses in self-improvement.",
                BirthDate = new DateTime(1888, 11, 24),
                DeathDate = new DateTime(1955, 11, 1),
                Nationality = "American",
                CreatedDate = now
            },
            new Author
            {
                Id = 4,
                Name = "Robert C. Martin",
                Biography = "American software engineer and instructor, known for his many books on software development.",
                BirthDate = new DateTime(1952, 12, 5),
                Nationality = "American",
                CreatedDate = now
            },
            new Author
            {
                Id = 5,
                Name = "Geoff Watts",
                Biography = "Agile coach and author specializing in Scrum and team dynamics.",
                Nationality = "British",
                CreatedDate = now
            }
        );

        // Seed BookDetails
        modelBuilder.Entity<BookDetails>().HasData(
            new BookDetails
            {
                Id = 1,
                AuthorId = 1,
                Title = "Hamlet",
                ISBN = "9781472518381",
                Description = "Arguably Shakespeare's greatest tragedy about the Prince of Denmark's quest for revenge.",
                Genre = "Tragedy",
                Language = "English",
                Pages = 342,
                PublicationDate = new DateTime(1603, 1, 1),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 2,
                AuthorId = 1,
                Title = "King Lear",
                ISBN = "9780141012292",
                Description = "King Lear is a tragedy written by William Shakespeare, depicting the gradual descent into madness of the title character.",
                Genre = "Tragedy",
                Language = "English",
                Pages = 256,
                PublicationDate = new DateTime(1606, 1, 1),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 3,
                AuthorId = 1,
                Title = "Othello",
                ISBN = "9781853260186",
                Description = "An intense drama of love, deception, jealousy and destruction.",
                Genre = "Tragedy",
                Language = "English",
                Pages = 178,
                PublicationDate = new DateTime(1603, 1, 1),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 4,
                AuthorId = 2,
                Title = "12 Rules for Life: An Antidote to Chaos",
                ISBN = "9780345816023",
                Description = "12 Rules for Life offers a deeply rewarding antidote to the chaos in our lives: eternal truths applied to our modern problems.",
                Genre = "Self-Help",
                Language = "English",
                Pages = 409,
                Publisher = "Random House Canada",
                PublicationDate = new DateTime(2018, 1, 23),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 5,
                AuthorId = 3,
                Title = "How to Win Friends and Influence People",
                ISBN = "9781439199190",
                Description = "Dale Carnegie had an understanding of human nature that will never be outdated.",
                Genre = "Self-Help",
                Language = "English",
                Pages = 291,
                Publisher = "Simon & Schuster",
                PublicationDate = new DateTime(1936, 10, 1),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 6,
                AuthorId = 4,
                Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                ISBN = "9780132350884",
                Description = "A handbook of agile software craftsmanship that presents a revolutionary paradigm with practical advice.",
                Genre = "Technology",
                Language = "English",
                Pages = 464,
                Publisher = "Prentice Hall",
                PublicationDate = new DateTime(2008, 8, 1),
                CreatedDate = now
            },
            new BookDetails
            {
                Id = 7,
                AuthorId = 5,
                Title = "Scrum Mastery: From Good To Great Servant-Leadership",
                ISBN = "9780957587403",
                Description = "The basics of being a ScrumMaster are fairly straightforward: Facilitate the Scrum process and remove impediments.",
                Genre = "Business",
                Language = "English",
                Pages = 152,
                Publisher = "Inspect & Adapt Ltd",
                PublicationDate = new DateTime(2013, 9, 15),
                CreatedDate = now
            }
        );

        // Seed Members
        modelBuilder.Entity<Member>().HasData(
            new Member
            {
                Id = 1,
                SSN = "19855666-0001",
                Name = "Daniel Graham",
                Email = "daniel.graham@email.com",
                PhoneNumber = "+46-70-123-4567",
                Address = "123 Library Street, Stockholm",
                MembershipStartDate = new DateTime(2023, 1, 15),
                Status = MembershipStatus.Active,
                CreatedDate = now
            },
            new Member
            {
                Id = 2,
                SSN = "19555666-0002",
                Name = "Eric Howell",
                Email = "eric.howell@email.com",
                PhoneNumber = "+46-70-234-5678",
                Address = "456 Book Avenue, Gothenburg",
                MembershipStartDate = new DateTime(2023, 2, 20),
                Status = MembershipStatus.Active,
                CreatedDate = now
            },
            new Member
            {
                Id = 3,
                SSN = "19555666-0003",
                Name = "Patricia Lebsack",
                Email = "patricia.lebsack@email.com",
                PhoneNumber = "+46-70-345-6789",
                Address = "789 Reading Road, Malmö",
                MembershipStartDate = new DateTime(2023, 3, 10),
                Status = MembershipStatus.Active,
                CreatedDate = now
            },
            new Member
            {
                Id = 4,
                SSN = "19555666-0004",
                Name = "Kalle Runolfsdottir",
                Email = "kalle.runolfsdottir@email.com",
                PhoneNumber = "+46-70-456-7890",
                Address = "321 Study Street, Uppsala",
                MembershipStartDate = new DateTime(2023, 4, 5),
                Status = MembershipStatus.Active,
                CreatedDate = now
            },
            new Member
            {
                Id = 5,
                SSN = "19555666-0005",
                Name = "Linus Reichert",
                Email = "linus.reichert@email.com",
                PhoneNumber = "+46-70-567-8901",
                Address = "654 Knowledge Lane, Linköping",
                MembershipStartDate = new DateTime(2023, 5, 1),
                Status = MembershipStatus.Active,
                CreatedDate = now
            }
        );

        // Seed BookCopies
        modelBuilder.Entity<BookCopy>().HasData(
            new BookCopy { Id = 1, DetailsId = 1, IsAvailable = true, Condition = BookCondition.Good, Location = "A-101", Barcode = "BC001", CreatedDate = now },
            new BookCopy { Id = 2, DetailsId = 1, IsAvailable = true, Condition = BookCondition.Excellent, Location = "A-102", Barcode = "BC002", CreatedDate = now },
            new BookCopy { Id = 3, DetailsId = 2, IsAvailable = false, Condition = BookCondition.Good, Location = "A-103", Barcode = "BC003", CreatedDate = now },
            new BookCopy { Id = 4, DetailsId = 3, IsAvailable = false, Condition = BookCondition.Fair, Location = "A-104", Barcode = "BC004", CreatedDate = now },
            new BookCopy { Id = 5, DetailsId = 4, IsAvailable = true, Condition = BookCondition.New, Location = "B-201", Barcode = "BC005", CreatedDate = now },
            new BookCopy { Id = 6, DetailsId = 5, IsAvailable = false, Condition = BookCondition.Good, Location = "B-202", Barcode = "BC006", CreatedDate = now },
            new BookCopy { Id = 7, DetailsId = 6, IsAvailable = true, Condition = BookCondition.Excellent, Location = "C-301", Barcode = "BC007", CreatedDate = now },
            new BookCopy { Id = 8, DetailsId = 7, IsAvailable = true, Condition = BookCondition.Good, Location = "C-302", Barcode = "BC008", CreatedDate = now }
        );

        // Seed Loans
        modelBuilder.Entity<Loan>().HasData(
            new Loan
            {
                Id = 1,
                MemberId = 3,
                StartDate = new DateTime(2024, 1, 5),
                DueDate = new DateTime(2024, 1, 19),
                ReturnDate = new DateTime(2024, 1, 19),
                Fee = 0,
                Status = LoanStatus.Returned,
                CreatedDate = new DateTime(2024, 1, 5)
            },
            new Loan
            {
                Id = 2,
                MemberId = 1,
                StartDate = new DateTime(2024, 1, 19),
                DueDate = new DateTime(2024, 2, 2),
                ReturnDate = new DateTime(2024, 2, 6),
                Fee = 24,
                Status = LoanStatus.Returned,
                CreatedDate = new DateTime(2024, 1, 19)
            },
            new Loan
            {
                Id = 3,
                MemberId = 2,
                StartDate = new DateTime(2024, 1, 3),
                DueDate = new DateTime(2024, 1, 17),
                ReturnDate = new DateTime(2024, 1, 16),
                Fee = 0,
                Status = LoanStatus.Returned,
                CreatedDate = new DateTime(2024, 1, 3)
            },
            new Loan
            {
                Id = 4,
                MemberId = 2,
                StartDate = new DateTime(2024, 6, 30),
                DueDate = new DateTime(2024, 7, 14),
                Status = LoanStatus.Active,
                CreatedDate = new DateTime(2024, 6, 30)
            },
            new Loan
            {
                Id = 5,
                MemberId = 4,
                StartDate = new DateTime(2024, 6, 29),
                DueDate = new DateTime(2024, 7, 13),
                Status = LoanStatus.Active,
                CreatedDate = new DateTime(2024, 6, 29)
            }
        );

        // Seed BookCopyLoans
        modelBuilder.Entity<BookCopyLoan>().HasData(
            new BookCopyLoan { BookCopyId = 3, LoanId = 1 },
            new BookCopyLoan { BookCopyId = 6, LoanId = 2 },
            new BookCopyLoan { BookCopyId = 4, LoanId = 3 },
            new BookCopyLoan { BookCopyId = 3, LoanId = 4 },
            new BookCopyLoan { BookCopyId = 6, LoanId = 5 }
        );
    }

    #endregion

    #region Additional Methods

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set audit fields
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    // We can set CreatedBy here if we have access to the current user
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                    // We can set UpdatedBy here if we have to the current user
                    entry.Property(x => x.CreatedDate).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }

    #endregion
}
