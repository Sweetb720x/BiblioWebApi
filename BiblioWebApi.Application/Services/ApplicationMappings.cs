using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Domain;

namespace BiblioWebApi.Application.Services;

public static class ApplicationMappings
{
    public static BookResponse ToBookResponse(Book book) => new(book.Id, book.Isbn, book.Title, book.Author, book.Publisher, book.PublicationYear, book.Description, book.CategoryId, book.Category?.Name ?? "", book.Copies.Count, book.Copies.Count(x => x.Status == CopyStatus.Available));
    public static CopyResponse ToCopyResponse(BookCopy copy) => new(copy.Id, copy.BookId, copy.InventoryCode, copy.Status, copy.Notes);
    public static LoanResponse ToLoanResponse(Loan loan) => new(loan.Id, loan.UserId, loan.CopyId, loan.Copy?.Book?.Title ?? "", loan.BorrowedAt, loan.DueDate, loan.ReturnedAt, loan.Status, loan.Renewals);
}

