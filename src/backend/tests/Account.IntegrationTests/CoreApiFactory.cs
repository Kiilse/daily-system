using Microsoft.AspNetCore.Mvc.Testing;

namespace Account.IntegrationTests;

/// <summary>Runs Host.Core in memory. Shared by every test class through <see cref="IClassFixture{TFixture}"/>.</summary>
public sealed class CoreApiFactory : WebApplicationFactory<Program>;
