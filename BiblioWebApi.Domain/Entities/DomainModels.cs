namespace BiblioWebApi.Domain;

public enum UserRole { Administrator, Librarian, Reader, Student }
public enum UserStatus { Active, Inactive, Blocked }
public enum CopyStatus { Available, Reserved, OnLoan, Maintenance, Retired }
public enum LoanStatus { Active, Returned, Overdue, Cancelled }
public enum ReservationStatus { Pending, Approved, Rejected, Cancelled, PickedUp, Expired }
public enum FineStatus { Pending, Paid, Waived }

public sealed class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Reader;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Loan> Loans { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<Fine> Fines { get; set; } = [];
}

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<Book> Books { get; set; } = [];
}

public sealed class Book
{
    public int Id { get; set; }
    public string Isbn { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<BookCopy> Copies { get; set; } = [];
}

public sealed class BookCopy
{
    public int Id { get; set; }
    public string InventoryCode { get; set; } = "";
    public CopyStatus Status { get; set; } = CopyStatus.Available;
    public string? Notes { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}

public sealed class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CopyId { get; set; }
    public BookCopy Copy { get; set; } = null!;
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;
    public int Renewals { get; set; }
}

public sealed class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int? CopyId { get; set; }
    public BookCopy? Copy { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PickupExpiresAt { get; set; }
}

public sealed class Fine
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? LoanId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "";
    public FineStatus Status { get; set; } = FineStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SettledAt { get; set; }
}

