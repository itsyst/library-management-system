using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels;

public class BookDetailsViewModel
{
    public BookDetails? BookDetails { get; set; }

    [ValidateNever]
    [Display(Name = "Författare")]
    public IEnumerable<SelectListItem>? Authors { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem>? BookCopies { get; set; }

    [ValidateNever]
    public int Copies { get; set; } = 0;

    [ValidateNever]
    [Display(Name = "Omslagsbild")]
    public IFormFile? CoverImage { get; set; }
    public string SearchTerm { get; internal set; }
    public int? AuthorFilter { get; internal set; }
    public string GenreFilter { get; internal set; }
    public bool? AvailabilityFilter { get; internal set; }
    public int CurrentPage { get; internal set; }
    public int PageSize { get; internal set; }
    public int TotalPages { get; internal set; }
    public int TotalRecords { get; internal set; }
    public BookStatistics Statistics { get; internal set; }
    public IEnumerable<SelectListItem> Genres { get; internal set; }
    public int CopiesToAdd { get; internal set; }
    public BookCondition InitialCondition { get; internal set; }
    public IEnumerable<SelectListItem> BookConditions { get; internal set; }
    public IEnumerable<SelectListItem> Publishers { get; internal set; }
    public IEnumerable<SelectListItem> Languages { get; internal set; }
    public DateTime PurchaseDate { get; internal set; }
    public string Notes { get; internal set; }
    public decimal? PurchasePrice { get; internal set; }
    public string Location { get; internal set; }
    public object BarcodePrefix { get; internal set; }
}

public class BookStatistics
{
    public int TotalBooks { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int BorrowedCopies { get; set; }
    public int TotalAuthors { get; set; }
    public int UniqueGenres { get; set; }
    public int UniquePublishers { get; set; }
    public int UniqueLanguages { get; set; }
    public decimal AveragePages { get; set; }
    public string? MostPopularGenre { get; set; }
    public string? MostPopularAuthor { get; set; }
    public string? MostBorrowedBook { get; set; }
    public int BooksAddedThisMonth { get; set; }
    public int BooksAddedThisYear { get; set; }
    public int OverdueBooks { get; set; }
    public int LowInventoryBooks { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}