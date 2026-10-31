IF DB_ID(N'BiblioWeb') IS NULL CREATE DATABASE BiblioWeb;
GO
USE BiblioWeb;
GO

IF OBJECT_ID(N'dbo.Multas', N'U') IS NOT NULL DROP TABLE dbo.Multas;
IF OBJECT_ID(N'dbo.Reservas', N'U') IS NOT NULL DROP TABLE dbo.Reservas;
IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NOT NULL DROP TABLE dbo.Prestamos;
IF OBJECT_ID(N'dbo.Ejemplares', N'U') IS NOT NULL DROP TABLE dbo.Ejemplares;
IF OBJECT_ID(N'dbo.Libros', N'U') IS NOT NULL DROP TABLE dbo.Libros;
IF OBJECT_ID(N'dbo.Categorias', N'U') IS NOT NULL DROP TABLE dbo.Categorias;
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL DROP TABLE dbo.Usuarios;
GO

CREATE TABLE dbo.Usuarios (
	id_usuario int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
	nombre_completo nvarchar(200) NOT NULL,
	correo nvarchar(320) NOT NULL CONSTRAINT UQ_Usuarios_Correo UNIQUE,
	hash_contrasena nvarchar(500) NOT NULL,
	rol nvarchar(30) NOT NULL CONSTRAINT CK_Usuarios_Rol CHECK (rol IN ('Administrador','Bibliotecario','Lector','Estudiante')),
	estado nvarchar(30) NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT 'Activo' CONSTRAINT CK_Usuarios_Estado CHECK (estado IN ('Activo','Inactivo','Bloqueado')),
	fecha_creacion datetime2(7) NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT SYSUTCDATETIME()
);
CREATE TABLE dbo.Categorias (
	id_categoria int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categorias PRIMARY KEY,
	nombre nvarchar(150) NOT NULL CONSTRAINT UQ_Categorias_Nombre UNIQUE
);
CREATE TABLE dbo.Libros (
	id_libro int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Libros PRIMARY KEY,
	isbn nvarchar(20) NOT NULL CONSTRAINT UQ_Libros_Isbn UNIQUE,
	titulo nvarchar(250) NOT NULL,
	autor nvarchar(200) NOT NULL,
	editorial nvarchar(200) NULL,
	anio_publicacion int NULL,
	descripcion nvarchar(max) NULL,
	esta_activo bit NOT NULL CONSTRAINT DF_Libros_EstaActivo DEFAULT 1,
	id_categoria int NOT NULL CONSTRAINT FK_Libros_Categorias REFERENCES dbo.Categorias(id_categoria)
);
CREATE TABLE dbo.Ejemplares (
	id_ejemplar int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ejemplares PRIMARY KEY,
	codigo_inventario nvarchar(100) NOT NULL CONSTRAINT UQ_Ejemplares_Codigo UNIQUE,
	estado nvarchar(30) NOT NULL CONSTRAINT DF_Ejemplares_Estado DEFAULT 'Disponible' CONSTRAINT CK_Ejemplares_Estado CHECK (estado IN ('Disponible','Reservado','Prestado','Mantenimiento','Retirado')),
	notas nvarchar(500) NULL,
	id_libro int NOT NULL CONSTRAINT FK_Ejemplares_Libros REFERENCES dbo.Libros(id_libro) ON DELETE CASCADE
);
CREATE TABLE dbo.Prestamos (
	id_prestamo int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prestamos PRIMARY KEY,
	id_usuario int NOT NULL CONSTRAINT FK_Prestamos_Usuarios REFERENCES dbo.Usuarios(id_usuario),
	id_ejemplar int NOT NULL CONSTRAINT FK_Prestamos_Ejemplares REFERENCES dbo.Ejemplares(id_ejemplar),
	fecha_prestamo datetime2(7) NOT NULL CONSTRAINT DF_Prestamos_FechaPrestamo DEFAULT SYSUTCDATETIME(),
	fecha_vencimiento datetime2(7) NOT NULL,
	fecha_devolucion datetime2(7) NULL,
	estado nvarchar(30) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT 'Activo' CONSTRAINT CK_Prestamos_Estado CHECK (estado IN ('Activo','Devuelto','Vencido','Cancelado')),
	renovaciones int NOT NULL CONSTRAINT DF_Prestamos_Renovaciones DEFAULT 0
);
CREATE TABLE dbo.Reservas (
	id_reserva int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reservas PRIMARY KEY,
	id_usuario int NOT NULL CONSTRAINT FK_Reservas_Usuarios REFERENCES dbo.Usuarios(id_usuario),
	id_libro int NOT NULL CONSTRAINT FK_Reservas_Libros REFERENCES dbo.Libros(id_libro),
	id_ejemplar int NULL CONSTRAINT FK_Reservas_Ejemplares REFERENCES dbo.Ejemplares(id_ejemplar),
	estado nvarchar(30) NOT NULL CONSTRAINT DF_Reservas_Estado DEFAULT 'Pendiente' CONSTRAINT CK_Reservas_Estado CHECK (estado IN ('Pendiente','Aprobada','Rechazada','Cancelada','Retirada','Expirada')),
	fecha_creacion datetime2(7) NOT NULL CONSTRAINT DF_Reservas_FechaCreacion DEFAULT SYSUTCDATETIME(),
	fecha_aprobacion datetime2(7) NULL,
	fecha_expiracion_retiro datetime2(7) NULL
);
CREATE TABLE dbo.Multas (
	id_multa int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Multas PRIMARY KEY,
	id_usuario int NOT NULL CONSTRAINT FK_Multas_Usuarios REFERENCES dbo.Usuarios(id_usuario),
	id_prestamo int NULL,
	monto decimal(12,2) NOT NULL CONSTRAINT CK_Multas_Monto CHECK (monto >= 0),
	motivo nvarchar(500) NOT NULL,
	estado nvarchar(30) NOT NULL CONSTRAINT DF_Multas_Estado DEFAULT 'Pendiente' CONSTRAINT CK_Multas_Estado CHECK (estado IN ('Pendiente','Pagada','Condonada')),
	fecha_creacion datetime2(7) NOT NULL CONSTRAINT DF_Multas_FechaCreacion DEFAULT SYSUTCDATETIME(),
	fecha_regularizacion datetime2(7) NULL
);
GO
CREATE INDEX IX_Libros_Titulo ON dbo.Libros(titulo);
CREATE INDEX IX_Libros_Autor ON dbo.Libros(autor);
CREATE INDEX IX_Ejemplares_Libro_Estado ON dbo.Ejemplares(id_libro, estado);
CREATE INDEX IX_Prestamos_Usuario_Estado ON dbo.Prestamos(id_usuario, estado);
CREATE INDEX IX_Reservas_Usuario_Estado ON dbo.Reservas(id_usuario, estado);
CREATE INDEX IX_Multas_Usuario_Estado ON dbo.Multas(id_usuario, estado);
GO
INSERT INTO dbo.Categorias(nombre) VALUES (N'Literatura'), (N'Ciencia'), (N'Tecnología'), (N'Historia'), (N'Administración');
SET IDENTITY_INSERT dbo.Usuarios ON;
INSERT INTO dbo.Usuarios (id_usuario, nombre_completo, correo, hash_contrasena, rol, estado, fecha_creacion)
VALUES (1, N'Admin', N'admin@bibliowebapi.local', N'pbkdf2$120000$sha256$QmlibGlvQWRtaW5TYWx0MDE=$O/zQ2Ro03Rd4WvQo/wCHQbDimPWRs1AFDnrL1Bw9W+k=', N'Administrador', N'Activo', '2026-01-01T00:00:00');
SET IDENTITY_INSERT dbo.Usuarios OFF;
GO
-- Cambie DefaultConnection en appsettings.json si no utiliza LocalDB.
