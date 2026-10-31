using System.Text.Json.Serialization;
using BiblioWebApi.Domain;

namespace BiblioWebApi.Application.Contracts;

public record RegisterRequest([property: JsonPropertyName("nombreCompleto")] string FullName, [property: JsonPropertyName("correo")] string Email, [property: JsonPropertyName("contrasena")] string Password, [property: JsonPropertyName("rol")] UserRole Role = UserRole.Student);
public record LoginRequestDto([property: JsonPropertyName("correo")] string Email, [property: JsonPropertyName("contrasena")] string Password);
public record UserResponse([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("nombreCompleto")] string FullName, [property: JsonPropertyName("correo")] string Email, [property: JsonPropertyName("rol")] UserRole Role, [property: JsonPropertyName("estado")] UserStatus Status, [property: JsonPropertyName("fechaCreacion")] DateTime CreatedAt);
public record CreateUserRequest([property: JsonPropertyName("nombreCompleto")] string FullName, [property: JsonPropertyName("correo")] string Email, [property: JsonPropertyName("contrasena")] string Password, [property: JsonPropertyName("rol")] UserRole Role);
public record UpdateUserRequest([property: JsonPropertyName("nombreCompleto")] string? FullName, [property: JsonPropertyName("correo")] string? Email, [property: JsonPropertyName("rol")] UserRole? Role, [property: JsonPropertyName("estado")] UserStatus? Status);
public record StatusRequest([property: JsonPropertyName("estado")] UserStatus Status);
public record TokenResponse([property: JsonPropertyName("token")] string Token, [property: JsonPropertyName("expira")] DateTime Expires);
public record CreateBookRequest([property: JsonPropertyName("isbn")] string Isbn, [property: JsonPropertyName("titulo")] string Title, [property: JsonPropertyName("autor")] string Author, [property: JsonPropertyName("idCategoria")] int CategoryId, [property: JsonPropertyName("editorial")] string? Publisher, [property: JsonPropertyName("anioPublicacion")] int? PublicationYear, [property: JsonPropertyName("descripcion")] string? Description);
public record UpdateBookRequest([property: JsonPropertyName("isbn")] string? Isbn, [property: JsonPropertyName("titulo")] string? Title, [property: JsonPropertyName("autor")] string? Author, [property: JsonPropertyName("idCategoria")] int? CategoryId, [property: JsonPropertyName("editorial")] string? Publisher, [property: JsonPropertyName("anioPublicacion")] int? PublicationYear, [property: JsonPropertyName("descripcion")] string? Description, [property: JsonPropertyName("estaActivo")] bool? IsActive);
public record BookResponse([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("isbn")] string Isbn, [property: JsonPropertyName("titulo")] string Title, [property: JsonPropertyName("autor")] string Author, [property: JsonPropertyName("editorial")] string? Publisher, [property: JsonPropertyName("anioPublicacion")] int? PublicationYear, [property: JsonPropertyName("descripcion")] string? Description, [property: JsonPropertyName("idCategoria")] int CategoryId, [property: JsonPropertyName("categoria")] string Category, [property: JsonPropertyName("totalEjemplares")] int TotalCopies, [property: JsonPropertyName("ejemplaresDisponibles")] int AvailableCopies);
public record CreateCategoryRequest([property: JsonPropertyName("nombre")] string Name);
public record CreateCopyRequest([property: JsonPropertyName("codigoInventario")] string InventoryCode, [property: JsonPropertyName("notas")] string? Notes);
public record CopyStatusRequest([property: JsonPropertyName("estado")] CopyStatus Status);
public record CopyResponse([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("idLibro")] int BookId, [property: JsonPropertyName("codigoInventario")] string InventoryCode, [property: JsonPropertyName("estado")] CopyStatus Status, [property: JsonPropertyName("notas")] string? Notes);
public record CreateLoanRequest([property: JsonPropertyName("idUsuario")] int UserId, [property: JsonPropertyName("idEjemplar")] int CopyId, [property: JsonPropertyName("fechaVencimiento")] DateTime DueDate);
public record LoanResponse([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("idUsuario")] int UserId, [property: JsonPropertyName("idEjemplar")] int CopyId, [property: JsonPropertyName("tituloLibro")] string BookTitle, [property: JsonPropertyName("fechaPrestamo")] DateTime BorrowedAt, [property: JsonPropertyName("fechaVencimiento")] DateTime DueDate, [property: JsonPropertyName("fechaDevolucion")] DateTime? ReturnedAt, [property: JsonPropertyName("estado")] LoanStatus Status, [property: JsonPropertyName("renovaciones")] int Renewals);
public record CreateReservationRequest([property: JsonPropertyName("idUsuario")] int UserId, [property: JsonPropertyName("idLibro")] int BookId);
public record FineRequest([property: JsonPropertyName("idUsuario")] int UserId, [property: JsonPropertyName("monto")] decimal Amount, [property: JsonPropertyName("motivo")] string Reason, [property: JsonPropertyName("idPrestamo")] int? LoanId);
public record ErrorResponse([property: JsonPropertyName("estado")] int Status, [property: JsonPropertyName("codigo")] string Code, [property: JsonPropertyName("mensaje")] string Message, [property: JsonPropertyName("detalles")] object[] Details);



