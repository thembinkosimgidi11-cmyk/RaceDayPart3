using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using RaceDayMVC.Models;

namespace RaceDayMVC.Services;

public sealed class RaceDayAuthenticationService
{
    private readonly string _connectionString;
    private readonly PasswordHasher<PasswordIdentity> _hasher = new();

    public RaceDayAuthenticationService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("RaceDayDB")
            ?? throw new InvalidOperationException("ConnectionStrings:RaceDayDB is missing from appsettings.json.");
    }

    public async Task<AuthenticatedRaceDayUser?> AuthenticateAsync(string email, string password)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT TOP (1) organiser_id AS UserID, first_name AS FirstName,
                   last_name AS LastName, email AS Email, password_hash AS PasswordHash,
                   CAST('Organiser' AS NVARCHAR(20)) AS Role
            FROM Organiser WHERE email = @Email
            UNION ALL
            SELECT TOP (1) participant_id AS UserID, first_name AS FirstName,
                   last_name AS LastName, email AS Email, password_hash AS PasswordHash,
                   CAST('Participant' AS NVARCHAR(20)) AS Role
            FROM Participant WHERE email = @Email;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var user = new AuthenticatedRaceDayUser
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };

            // Existing sample rows in the supplied SQL use placeholder hashes.
            // New accounts use ASP.NET Core's supported PBKDF2 password hasher.
            var verification = _hasher.VerifyHashedPassword(
                new PasswordIdentity { Email = user.Email },
                user.PasswordHash,
                password);

            if (verification != PasswordVerificationResult.Failed)
                return user;
        }

        return null;
    }

    public async Task RegisterAsync(RegisterVM model)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            const string duplicateSql = """
                SELECT
                    CASE WHEN EXISTS (SELECT 1 FROM Organiser WHERE email = @Email)
                           OR EXISTS (SELECT 1 FROM Participant WHERE email = @Email)
                    THEN 1 ELSE 0 END;
                """;
            await using (var duplicateCommand = new SqlCommand(duplicateSql, connection, transaction))
            {
                duplicateCommand.Parameters.AddWithValue("@Email", model.Email.Trim());
                var exists = Convert.ToInt32(await duplicateCommand.ExecuteScalarAsync());
                if (exists == 1)
                    throw new DuplicateAccountException();
            }

            var identity = new PasswordIdentity { Email = model.Email.Trim() };
            var passwordHash = _hasher.HashPassword(identity, model.Password);

            if (model.Role == "Organiser")
            {
                const string insertOrganiser = """
                    INSERT INTO Organiser
                        (first_name, last_name, email, phone, organisation, password_hash)
                    VALUES
                        (@FirstName, @LastName, @Email, @Phone, @Organisation, @PasswordHash);
                    """;
                await using var command = new SqlCommand(insertOrganiser, connection, transaction);
                AddCommonParameters(command, model, passwordHash);
                command.Parameters.AddWithValue("@Phone", model.Phone.Trim());
                command.Parameters.AddWithValue("@Organisation", model.Organisation.Trim());
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                const string insertParticipant = """
                    INSERT INTO Participant
                        (first_name, last_name, email, phone, date_of_birth, gender,
                         emergency_contact, emergency_phone, password_hash)
                    VALUES
                        (@FirstName, @LastName, @Email, @Phone, @DateOfBirth, @Gender,
                         @EmergencyContact, @EmergencyPhone, @PasswordHash);
                    """;
                await using var command = new SqlCommand(insertParticipant, connection, transaction);
                AddCommonParameters(command, model, passwordHash);
                command.Parameters.AddWithValue("@Phone", model.Phone.Trim());
                command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth!.Value.Date);
                command.Parameters.AddWithValue("@Gender", model.Gender);
                command.Parameters.AddWithValue("@EmergencyContact", model.EmergencyContact.Trim());
                command.Parameters.AddWithValue("@EmergencyPhone", model.EmergencyPhone.Trim());
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void AddCommonParameters(SqlCommand command, RegisterVM model, string passwordHash)
    {
        command.Parameters.AddWithValue("@FirstName", model.FirstName.Trim());
        command.Parameters.AddWithValue("@LastName", model.LastName.Trim());
        command.Parameters.AddWithValue("@Email", model.Email.Trim());
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
    }

    private sealed class PasswordIdentity
    {
        public string Email { get; init; } = "";
    }
}

public sealed class AuthenticatedRaceDayUser
{
    public int UserID { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public string Role { get; init; } = "";
}

public sealed class DuplicateAccountException : Exception
{
}
