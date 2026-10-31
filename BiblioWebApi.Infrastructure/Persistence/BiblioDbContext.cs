using BiblioWebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Infrastructure.Persistence;

public sealed class BiblioDbContext(DbContextOptions<BiblioDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Usuarios");
        modelBuilder.Entity<Category>().ToTable("Categorias");
        modelBuilder.Entity<Book>().ToTable("Libros");
        modelBuilder.Entity<BookCopy>().ToTable("Ejemplares");
        modelBuilder.Entity<Loan>().ToTable("Prestamos");
        modelBuilder.Entity<Reservation>().ToTable("Reservas");
        modelBuilder.Entity<Fine>().ToTable("Multas");

        modelBuilder.Entity<User>().Property(x => x.Id).HasColumnName("id_usuario");
        modelBuilder.Entity<User>().Property(x => x.FullName).HasColumnName("nombre_completo");
        modelBuilder.Entity<User>().Property(x => x.Email).HasColumnName("correo");
        modelBuilder.Entity<User>().Property(x => x.PasswordHash).HasColumnName("hash_contrasena");
        modelBuilder.Entity<User>().Property(x => x.Role).HasColumnName("rol");
        modelBuilder.Entity<User>().Property(x => x.Status).HasColumnName("estado");
        modelBuilder.Entity<User>().Property(x => x.CreatedAt).HasColumnName("fecha_creacion");
        modelBuilder.Entity<Category>().Property(x => x.Id).HasColumnName("id_categoria");
        modelBuilder.Entity<Category>().Property(x => x.Name).HasColumnName("nombre");
        modelBuilder.Entity<Book>().Property(x => x.Id).HasColumnName("id_libro");
        modelBuilder.Entity<Book>().Property(x => x.Isbn).HasColumnName("isbn");
        modelBuilder.Entity<Book>().Property(x => x.Title).HasColumnName("titulo");
        modelBuilder.Entity<Book>().Property(x => x.Author).HasColumnName("autor");
        modelBuilder.Entity<Book>().Property(x => x.Publisher).HasColumnName("editorial");
        modelBuilder.Entity<Book>().Property(x => x.PublicationYear).HasColumnName("anio_publicacion");
        modelBuilder.Entity<Book>().Property(x => x.Description).HasColumnName("descripcion");
        modelBuilder.Entity<Book>().Property(x => x.IsActive).HasColumnName("esta_activo");
        modelBuilder.Entity<Book>().Property(x => x.CategoryId).HasColumnName("id_categoria");
        modelBuilder.Entity<BookCopy>().Property(x => x.Id).HasColumnName("id_ejemplar");
        modelBuilder.Entity<BookCopy>().Property(x => x.InventoryCode).HasColumnName("codigo_inventario");
        modelBuilder.Entity<BookCopy>().Property(x => x.Status).HasColumnName("estado");
        modelBuilder.Entity<BookCopy>().Property(x => x.Notes).HasColumnName("notas");
        modelBuilder.Entity<BookCopy>().Property(x => x.BookId).HasColumnName("id_libro");
        modelBuilder.Entity<Loan>().Property(x => x.Id).HasColumnName("id_prestamo");
        modelBuilder.Entity<Loan>().Property(x => x.UserId).HasColumnName("id_usuario");
        modelBuilder.Entity<Loan>().Property(x => x.CopyId).HasColumnName("id_ejemplar");
        modelBuilder.Entity<Loan>().Property(x => x.BorrowedAt).HasColumnName("fecha_prestamo");
        modelBuilder.Entity<Loan>().Property(x => x.DueDate).HasColumnName("fecha_vencimiento");
        modelBuilder.Entity<Loan>().Property(x => x.ReturnedAt).HasColumnName("fecha_devolucion");
        modelBuilder.Entity<Loan>().Property(x => x.Status).HasColumnName("estado");
        modelBuilder.Entity<Loan>().Property(x => x.Renewals).HasColumnName("renovaciones");
        modelBuilder.Entity<Reservation>().Property(x => x.Id).HasColumnName("id_reserva");
        modelBuilder.Entity<Reservation>().Property(x => x.UserId).HasColumnName("id_usuario");
        modelBuilder.Entity<Reservation>().Property(x => x.BookId).HasColumnName("id_libro");
        modelBuilder.Entity<Reservation>().Property(x => x.CopyId).HasColumnName("id_ejemplar");
        modelBuilder.Entity<Reservation>().Property(x => x.Status).HasColumnName("estado");
        modelBuilder.Entity<Reservation>().Property(x => x.CreatedAt).HasColumnName("fecha_creacion");
        modelBuilder.Entity<Reservation>().Property(x => x.ApprovedAt).HasColumnName("fecha_aprobacion");
        modelBuilder.Entity<Reservation>().Property(x => x.PickupExpiresAt).HasColumnName("fecha_expiracion_retiro");
        modelBuilder.Entity<Fine>().Property(x => x.Id).HasColumnName("id_multa");
        modelBuilder.Entity<Fine>().Property(x => x.UserId).HasColumnName("id_usuario");
        modelBuilder.Entity<Fine>().Property(x => x.LoanId).HasColumnName("id_prestamo");
        modelBuilder.Entity<Fine>().Property(x => x.Amount).HasColumnName("monto");
        modelBuilder.Entity<Fine>().Property(x => x.Reason).HasColumnName("motivo");
        modelBuilder.Entity<Fine>().Property(x => x.Status).HasColumnName("estado");
        modelBuilder.Entity<Fine>().Property(x => x.CreatedAt).HasColumnName("fecha_creacion");
        modelBuilder.Entity<Fine>().Property(x => x.SettledAt).HasColumnName("fecha_regularizacion");

        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique().HasDatabaseName("UX_Usuarios_Correo");
        modelBuilder.Entity<Book>().HasIndex(x => x.Isbn).IsUnique().HasDatabaseName("UX_Libros_Isbn");
        modelBuilder.Entity<BookCopy>().HasIndex(x => x.InventoryCode).IsUnique().HasDatabaseName("UX_Ejemplares_Codigo");
        modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Categorias_Nombre");
        modelBuilder.Entity<User>().Property(x => x.Role).HasConversion(
            role => role == UserRole.Administrator ? "Administrador" : role == UserRole.Librarian ? "Bibliotecario" : role == UserRole.Reader ? "Lector" : "Estudiante",
            value => value == "Administrador" ? UserRole.Administrator : value == "Bibliotecario" ? UserRole.Librarian : value == "Lector" ? UserRole.Reader : UserRole.Student).HasMaxLength(30);
        modelBuilder.Entity<User>().Property(x => x.Status).HasConversion(
            status => status == UserStatus.Active ? "Activo" : status == UserStatus.Inactive ? "Inactivo" : "Bloqueado",
            value => value == "Activo" ? UserStatus.Active : value == "Inactivo" ? UserStatus.Inactive : UserStatus.Blocked).HasMaxLength(30);
        modelBuilder.Entity<BookCopy>().Property(x => x.Status).HasConversion(
            status => status == CopyStatus.Available ? "Disponible" : status == CopyStatus.Reserved ? "Reservado" : status == CopyStatus.OnLoan ? "Prestado" : status == CopyStatus.Maintenance ? "Mantenimiento" : "Retirado",
            value => value == "Disponible" ? CopyStatus.Available : value == "Reservado" ? CopyStatus.Reserved : value == "Prestado" ? CopyStatus.OnLoan : value == "Mantenimiento" ? CopyStatus.Maintenance : CopyStatus.Retired).HasMaxLength(30);
        modelBuilder.Entity<Loan>().Property(x => x.Status).HasConversion(
            status => status == LoanStatus.Active ? "Activo" : status == LoanStatus.Returned ? "Devuelto" : status == LoanStatus.Overdue ? "Vencido" : "Cancelado",
            value => value == "Activo" ? LoanStatus.Active : value == "Devuelto" ? LoanStatus.Returned : value == "Vencido" ? LoanStatus.Overdue : LoanStatus.Cancelled).HasMaxLength(30);
        modelBuilder.Entity<Reservation>().Property(x => x.Status).HasConversion(
            status => status == ReservationStatus.Pending ? "Pendiente" : status == ReservationStatus.Approved ? "Aprobada" : status == ReservationStatus.Rejected ? "Rechazada" : status == ReservationStatus.Cancelled ? "Cancelada" : status == ReservationStatus.PickedUp ? "Retirada" : "Expirada",
            value => value == "Pendiente" ? ReservationStatus.Pending : value == "Aprobada" ? ReservationStatus.Approved : value == "Rechazada" ? ReservationStatus.Rejected : value == "Cancelada" ? ReservationStatus.Cancelled : value == "Retirada" ? ReservationStatus.PickedUp : ReservationStatus.Expired).HasMaxLength(30);
        modelBuilder.Entity<Fine>().Property(x => x.Status).HasConversion(
            status => status == FineStatus.Pending ? "Pendiente" : status == FineStatus.Paid ? "Pagada" : "Condonada",
            value => value == "Pendiente" ? FineStatus.Pending : value == "Pagada" ? FineStatus.Paid : FineStatus.Waived).HasMaxLength(30);
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<Book>().Property(x => x.Isbn).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Book>().Property(x => x.Title).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<Book>().Property(x => x.Author).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Fine>().Property(x => x.Amount).HasPrecision(12, 2);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = "Admin",
            Email = "admin@bibliowebapi.local",
            PasswordHash = "pbkdf2$120000$sha256$QmlibGlvQWRtaW5TYWx0MDE=$O/zQ2Ro03Rd4WvQo/wCHQbDimPWRs1AFDnrL1Bw9W+k=",
            Role = UserRole.Administrator,
            Status = UserStatus.Active,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Book>().HasOne(x => x.Category).WithMany(x => x.Books).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<BookCopy>().HasOne(x => x.Book).WithMany(x => x.Copies).HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Loan>().HasOne(x => x.User).WithMany(x => x.Loans).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Loan>().HasOne(x => x.Copy).WithMany(x => x.Loans).HasForeignKey(x => x.CopyId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservation>().HasOne(x => x.User).WithMany(x => x.Reservations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservation>().HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Reservation>().HasOne(x => x.Copy).WithMany(x => x.Reservations).HasForeignKey(x => x.CopyId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Fine>().HasOne(x => x.User).WithMany(x => x.Fines).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}


