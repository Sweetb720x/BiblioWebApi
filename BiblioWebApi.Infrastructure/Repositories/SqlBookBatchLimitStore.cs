using System.Data;
using BiblioWebApi.Application.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BiblioWebApi.Infrastructure.Persistence;

namespace BiblioWebApi.Infrastructure.Repositories;

public sealed class SqlBookBatchLimitStore(BiblioDbContext db) : IBookBatchLimitStore
{
    public async Task<int> GetAsync(CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection) await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT limite_maximo FROM dbo.Configuracion WHERE clave = N'LimiteLoteLibros'";
            var value = await command.ExecuteScalarAsync(cancellationToken);
            return value is null or DBNull ? 500 : Convert.ToInt32(value);
        }
        finally
        {
            if (shouldCloseConnection) await connection.CloseAsync();
        }
    }

    public async Task<int> SetAsync(int maximum, CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection) await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE dbo.Configuracion SET limite_maximo = @Maximum WHERE clave = N'LimiteLoteLibros'; IF @@ROWCOUNT = 0 INSERT INTO dbo.Configuracion (clave, limite_maximo) VALUES (N'LimiteLoteLibros', @Maximum); SELECT @Maximum;";
            command.Parameters.Add(new SqlParameter("@Maximum", SqlDbType.Int) { Value = maximum });
            var value = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(value);
        }
        finally
        {
            if (shouldCloseConnection) await connection.CloseAsync();
        }
    }
}
