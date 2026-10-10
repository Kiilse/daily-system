using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace Host.Core;

// Throwaway for #8, do not merge: user input concatenated into SQL, CodeQL must raise cs/sql-injection.
[ApiController]
[Route("throwaway-sqli-mvc")]
public sealed class ThrowawaySqlController(IDbCommand command) : ControllerBase
{
    [HttpGet]
    public string? Get(string name)
    {
#pragma warning disable CA2100
        command.CommandText = "SELECT id FROM users WHERE name = '" + name + "'";
#pragma warning restore CA2100
        return command.ExecuteScalar()?.ToString();
    }
}
