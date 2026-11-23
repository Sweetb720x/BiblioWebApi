using System.Data;
using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BiblioWebApi.Infrastructure.Persistence;

namespace BiblioWebApi.Infrastructure.Repositories;

public sealed class SqlBookBatchCreator(BiblioDbContext db) : IBookBatchCreator
{
    public async Task<IReadOnlyCollection<BookResponse>> CreateAsync(CreateBookRequest book, int quantity, CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection) await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.CrearLibrosLote";
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 60;
            command.Parameters.Add(new SqlParameter("@Isbn", SqlDbType.NVarChar, 20) { Value = book.Isbn.Trim() });
            command.Parameters.Add(new SqlParameter("@Titulo", SqlDbType.NVarChar, 250) { Value = book.Title.Trim() });
            command.Parameters.Add(new SqlParameter("@Autor", SqlDbType.NVarChar, 200) { Value = book.Author.Trim() });
            command.Parameters.Add(new SqlParameter("@IdCategoria", SqlDbType.Int) { Value = book.CategoryId });
            command.Parameters.Add(new SqlParameter("@Editorial", SqlDbType.NVarChar, 200) { Value = (object?)book.Publisher ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@AnioPublicacion", SqlDbType.Int) { Value = (object?)book.PublicationYear ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, -1) { Value = (object?)book.Description ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = quantity });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = new List<BookResponse>();
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new BookResponse(
                    reader.GetInt32(reader.GetOrdinal("id")),
                    reader.GetString(reader.GetOrdinal("isbn")),
                    reader.GetString(reader.GetOrdinal("titulo")),
                    reader.GetString(reader.GetOrdinal("autor")),
                    reader.IsDBNull(reader.GetOrdinal("editorial")) ? null : reader.GetString(reader.GetOrdinal("editorial")),
                    reader.IsDBNull(reader.GetOrdinal("anio_publicacion")) ? null : reader.GetInt32(reader.GetOrdinal("anio_publicacion")),
                    reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
                    reader.GetInt32(reader.GetOrdinal("id_categoria")),
                    reader.GetString(reader.GetOrdinal("categoria")),
                    0,
                    0));
            }

            return results;
        }
        finally
        {
            if (shouldCloseConnection) await connection.CloseAsync();
        }
    }
}
