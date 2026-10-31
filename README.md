# BiblioWebApi

API REST de gestión bibliotecaria desarrollada con ASP.NET Core .NET 10, JWT Bearer, Entity Framework Core y SQL Server.

## Ejecutar la API

Revisa la cadena de conexión en `BiblioWebApi/appsettings.Development.json` y confirma que exista la base `BiblioWeb` en SQL Server. Ejecuta el proyecto desde Visual Studio o con:

```bash
dotnet run --project BiblioWebApi/BiblioWebApi.csproj
```

La URL depende de la configuración de lanzamiento, por ejemplo `https://localhost:44367`.

- Swagger UI: `https://localhost:44367/swagger`
- Documento OpenAPI: `https://localhost:44367/swagger/v1/swagger.json`

## Autenticación desde el frontend

La API utiliza tokens JWT. Primero inicia sesión:

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "correo": "admin@bibliowebapi.local",
  "contrasena": "Admin123"
}
```

La respuesta contiene `token` y `expira`. El usuario inicial es `admin@bibliowebapi.local` con contraseña `Admin123`. La contraseña se almacena como hash PBKDF2, no como texto plano.

Para llamar endpoints protegidos, envía el token en el encabezado:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Ejemplo con `fetch`:

```javascript
const token = sessionStorage.getItem("token");
const response = await fetch("https://localhost:44367/api/auth/me", {
  headers: { Authorization: `Bearer ${token}` }
});
const usuario = await response.json();
```

Para cerrar sesión, elimina el token localmente. El endpoint de logout también está disponible, pero JWT es stateless:

```javascript
sessionStorage.removeItem("token");
```

```http
POST /api/auth/logout
Authorization: Bearer {token}
```

## Autenticación y autorización

- `[AllowAnonymous]`: endpoint público; no necesita token.
- `[Authorize]`: requiere un JWT válido.
- `[Authorize(Roles = "Administrator,Librarian")]`: requiere un JWT válido y uno de esos roles.

Sin token o con token inválido: `401 Unauthorized`. Con token válido pero sin permisos: `403 Forbidden`.

Endpoints públicos principales:

```text
GET  /api/health
GET  /api/metadata
POST /api/auth/register
POST /api/auth/login
GET  /api/books
GET  /api/books/{id}
GET  /api/books/{id}/availability
GET  /api/books/search?q=texto
GET  /api/categories
```

Endpoints autenticados principales:

```text
GET   /api/auth/me
POST  /api/auth/refresh
GET   /api/loans
POST  /api/loans
POST  /api/reservations
GET   /api/books/{bookId}/copies
GET   /api/copies/{id}
```

Endpoints administrativos para `Administrator` o `Librarian`:

```text
POST   /api/books
PUT    /api/books/{id}
PATCH  /api/books/{id}
DELETE /api/books/{id}
POST   /api/categories
POST   /api/books/{bookId}/copies
PATCH  /api/copies/{id}/status
DELETE /api/copies/{id}
GET    /api/reservations
PATCH  /api/reservations/{id}/approve
PATCH  /api/reservations/{id}/reject
PATCH  /api/reservations/{id}/complete
GET    /api/users
POST   /api/users
PUT    /api/users/{id}
PATCH  /api/users/{id}
DELETE /api/users/{id}
GET    /api/fines
POST   /api/fines
GET    /api/dashboard/summary
```

## Ejemplo completo desde el frontend

```javascript
async function iniciarSesion(correo, contrasena) {
  const response = await fetch("https://localhost:44367/api/auth/login", {
	method: "POST",
	headers: { "Content-Type": "application/json" },
	body: JSON.stringify({ correo, contrasena })
  });

  if (!response.ok) throw new Error("Credenciales inválidas");
  const data = await response.json();
  sessionStorage.setItem("token", data.token);
  return data;
}

async function obtenerLibros() {
  const response = await fetch("https://localhost:44367/api/books");
  return response.json();
}

async function crearLibro(libro) {
  const token = sessionStorage.getItem("token");
  const response = await fetch("https://localhost:44367/api/books", {
	method: "POST",
	headers: {
	  "Content-Type": "application/json",
	  Authorization: `Bearer ${token}`
	},
	body: JSON.stringify(libro)
  });

  if (response.status === 401) throw new Error("Sesión expirada");
  if (response.status === 403) throw new Error("Sin permisos");
  return response.json();
}
```

## Guía de endpoints

En los ejemplos siguientes, `BASE_URL` representa la URL de la API, por ejemplo `https://localhost:44367`. Las respuestas de los DTO usan nombres JSON en español. Los endpoints que devuelven directamente entidades pueden conservar algunos nombres de propiedad del dominio.

### Formato recomendado para consumir la API

```javascript
const BASE_URL = "https://localhost:44367";

async function apiFetch(ruta, opciones = {}) {
  const token = sessionStorage.getItem("token");
  const response = await fetch(`${BASE_URL}${ruta}`, {
	...opciones,
	headers: {
	  "Content-Type": "application/json",
	  ...(token ? { Authorization: `Bearer ${token}` } : {}),
	  ...(opciones.headers ?? {})
	}
  });

  if (response.status === 204) return null;
  const body = await response.json().catch(() => null);
  if (!response.ok) {
	throw new Error(body?.mensaje ?? body?.message ?? `Error HTTP ${response.status}`);
  }
  return body;
}
```

### Sistema y autenticación

#### `GET /api/health`

No requiere autenticación. Comprueba que la API esté disponible.

Respuesta `200 OK`:

```json
{
  "status": "ok",
  "timestamp": "2026-01-01T12:00:00Z"
}
```

#### `GET /api/metadata`

No requiere autenticación. Devuelve los valores de roles y estados que puede utilizar el frontend. Responde `200 OK`.

#### `POST /api/auth/register`

No requiere autenticación.

```json
{
  "nombreCompleto": "Ana Pérez",
  "correo": "ana@example.com",
  "contrasena": "ClaveSegura123",
  "rol": "Student"
}
```

Respuesta `201 Created`: devuelve un objeto con `token` y `expira`.

#### `POST /api/auth/login`

No requiere autenticación.

```javascript
const sesion = await apiFetch("/api/auth/login", {
  method: "POST",
  body: JSON.stringify({
	correo: "admin@bibliowebapi.local",
	contrasena: "Admin123"
  })
});
sessionStorage.setItem("token", sesion.token);
```

Respuesta `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expira": "2026-01-01T13:00:00Z"
}
```

#### `GET /api/auth/me`

Requiere autenticación. Respuesta `200 OK`:

```json
{
  "id": 1,
  "nombreCompleto": "Admin",
  "correo": "admin@bibliowebapi.local",
  "rol": "Administrator",
  "estado": "Active",
  "fechaCreacion": "2026-01-01T00:00:00Z"
}
```

#### `POST /api/auth/refresh` y `POST /api/auth/logout`

Requieren autenticación. `refresh` responde `200 OK` con un nuevo objeto `token` y `expira`. `logout` responde `204 No Content`; el frontend debe eliminar el token localmente.

### Libros y categorías

#### `GET /api/books`

No requiere autenticación. Permite filtros opcionales:

```text
/api/books?title=Clean&author=Martin&category=Software&isbn=9780000000000
```

Respuesta `200 OK`, una colección de libros:

```json
[
  {
	"id": 1,
	"isbn": "9780000000000",
	"titulo": "Clean Architecture",
	"autor": "Robert C. Martin",
	"editorial": "Prentice Hall",
	"anioPublicacion": 2017,
	"descripcion": "Arquitectura de software",
	"idCategoria": 1,
	"categoria": "Tecnología",
	"totalEjemplares": 3,
	"ejemplaresDisponibles": 2
  }
]
```

También están disponibles `GET /api/books/{id}`, `GET /api/books/{id}/availability` y `GET /api/books/search?q=texto`. Todos responden `200 OK`; si el recurso no existe, la API responde `404 Not Found`.

#### `POST /api/books`

Requiere rol `Administrator` o `Librarian`.

```json
{
  "isbn": "9780000000000",
  "titulo": "Clean Architecture",
  "autor": "Robert C. Martin",
  "idCategoria": 1,
  "editorial": "Prentice Hall",
  "anioPublicacion": 2017,
  "descripcion": "Arquitectura de software"
}
```

Responde `201 Created` con el libro creado. `PUT` o `PATCH /api/books/{id}` actualiza el libro y responde `204 No Content`. `DELETE /api/books/{id}` lo elimina y responde `204 No Content`.

#### Categorías

- `GET /api/categories`: público; responde `200 OK` con una colección.
- `POST /api/categories`: requiere `Administrator` o `Librarian`.

Body del `POST`:

```json
{ "nombre": "Tecnología" }
```

Responde `201 Created` con la categoría creada.

### Ejemplares

- `GET /api/books/{bookId}/copies`: requiere autenticación; responde `200 OK` con los ejemplares.
- `GET /api/copies/{id}`: requiere autenticación; responde `200 OK` con el ejemplar.
- `POST /api/books/{bookId}/copies`: requiere `Administrator` o `Librarian`.
- `PATCH /api/copies/{id}/status`: requiere `Administrator` o `Librarian`.
- `DELETE /api/copies/{id}`: requiere `Administrator` o `Librarian`; responde `204 No Content`.

Body para crear un ejemplar:

```json
{
  "codigoInventario": "LIB-0001-001",
  "notas": "Ejemplar en buen estado"
}
```

Body para cambiar el estado:

```json
{ "estado": "Available" }
```

Las operaciones de creación responden `201 Created`; una operación correcta de actualización responde `204 No Content`.

### Préstamos

Todos los endpoints de préstamos requieren autenticación.

- `GET /api/loans`: lista préstamos; `200 OK`.
- `GET /api/loans/overdue`: lista préstamos vencidos; `200 OK`.
- `GET /api/loans/{id}`: obtiene un préstamo; `200 OK` o `404 Not Found`.
- `GET /api/users/{userId}/loans`: lista los préstamos de un usuario; `200 OK`.
- `POST /api/loans`: crea un préstamo; `201 Created`.
- `PATCH /api/loans/{id}/return`: registra devolución; `204 No Content`.
- `PATCH /api/loans/{id}/renew`: renueva; `200 OK` con el préstamo actualizado.
- `PATCH /api/loans/{id}/cancel`: cancela; `204 No Content`.

Body para crear un préstamo:

```json
{
  "idUsuario": 2,
  "idEjemplar": 1,
  "fechaVencimiento": "2026-02-15T23:59:59Z"
}
```

### Reservas

Requieren autenticación, excepto que las operaciones administrativas requieren además `Administrator` o `Librarian`.

- `POST /api/reservations`: crea una reserva; `201 Created`.
- `GET /api/reservations/{id}`: consulta una reserva; `200 OK`.
- `PATCH /api/reservations/{id}/cancel`: cancela una reserva; `204 No Content`.
- `GET /api/users/{userId}/reservations`: consulta reservas del usuario; `200 OK`.
- `GET /api/reservations` y `GET /api/reservations/pending`: administración; `200 OK`.
- `PATCH /api/reservations/{id}/approve`, `/reject` y `/complete`: administración; `200 OK` o `204 No Content` según la operación.

Body para crear una reserva:

```json
{
  "idUsuario": 2,
  "idLibro": 1
}
```

### Usuarios

Todos requieren rol `Administrator` o `Librarian`:

- `GET /api/users`: lista usuarios; `200 OK`.
- `GET /api/users/{id}`: obtiene un usuario; `200 OK`.
- `POST /api/users`: crea un usuario; `201 Created`.
- `PUT` o `PATCH /api/users/{id}`: actualiza; `200 OK`.
- `PATCH /api/users/{id}/status`: cambia el estado; `204 No Content`.
- `DELETE /api/users/{id}`: elimina; `204 No Content`.

Body para crear un usuario:

```json
{
  "nombreCompleto": "Carlos López",
  "correo": "carlos@example.com",
  "contrasena": "ClaveSegura123",
  "rol": "Student"
}
```

### Multas

- `GET /api/users/{userId}/fines`: requiere autenticación; lista multas del usuario.
- `GET /api/users/{userId}/account-status`: requiere autenticación; consulta el estado de cuenta.
- `GET /api/fines/{id}`: requiere autenticación; consulta una multa.
- `GET /api/fines`: requiere `Administrator` o `Librarian`; lista todas las multas.
- `POST /api/fines`: requiere `Administrator` o `Librarian`; crea una multa y responde `201 Created`.
- `PATCH /api/fines/{id}/pay` y `/waive`: requieren `Administrator` o `Librarian`; responden `204 No Content`.

Body para crear una multa:

```json
{
  "idUsuario": 2,
  "monto": 25.5,
  "motivo": "Entrega tardía",
  "idPrestamo": 10
}
```

### Dashboard

Todos requieren rol `Administrator` o `Librarian` y responden `200 OK` con el objeto o colección correspondiente:

```text
GET /api/dashboard/summary
GET /api/dashboard/recent-activity
GET /api/dashboard/alerts
GET /api/dashboard/loans-by-status
GET /api/dashboard/books-availability
GET /api/dashboard/overdue
```

## Respuestas y errores

Respuestas habituales:

- `200 OK`: consulta o actualización con contenido.
- `201 Created`: recurso creado; incluye el recurso en el cuerpo.
- `204 No Content`: operación correcta sin contenido de respuesta.
- `400 Bad Request`: JSON inválido o validación fallida.
- `401 Unauthorized`: falta el JWT, está vencido o es inválido.
- `403 Forbidden`: el rol del usuario no tiene permisos.
- `404 Not Found`: no existe el recurso solicitado.
- `409 Conflict`: conflicto, por ejemplo correo, ISBN o código de inventario duplicado.
- `500 Internal Server Error`: error inesperado del servidor.

Los errores de validación y excepciones se devuelven como `ProblemDetails`. El frontend debe revisar siempre `response.ok` antes de utilizar la respuesta como datos correctos.

## JSON y CORS

Los DTOs utilizan nombres JSON en español, como `correo`, `contrasena`, `titulo`, `idCategoria` y `fechaVencimiento`. Consulta Swagger para ver el modelo exacto de cada operación.

Si el frontend usa otro dominio o puerto, configura CORS en ASP.NET Core para permitir ese origen. En desarrollo, acepta el certificado HTTPS local cuando el navegador lo solicite.
