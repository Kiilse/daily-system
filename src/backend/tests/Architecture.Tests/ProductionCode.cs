using System.Reflection;
using ArchUnitNET.Loader;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace Architecture.Tests;

internal sealed record Module(string Name, Assembly Implementation, Assembly Contracts);

/// <summary>The production assemblies the rules inspect, loaded once for the whole test run (loading is the slow part).</summary>
internal static class ProductionCode
{
    /// <summary>Every module under src/backend/Modules. <see cref="ModuleListTests"/> fails when a module folder is missing here.</summary>
    public static readonly IReadOnlyList<Module> Modules =
    [
        new("Account", typeof(Account.AccountModule).Assembly, typeof(Account.Contracts.AssemblyMarker).Assembly),
        new("Calendar", typeof(Calendar.CalendarModule).Assembly, typeof(Calendar.Contracts.AssemblyMarker).Assembly),
        new("Menu", typeof(Menu.MenuModule).Assembly, typeof(Menu.Contracts.AssemblyMarker).Assembly),
    ];

    public static readonly Assembly SharedKernel = typeof(BuildingBlocks.SharedKernel.Result).Assembly;
    public static readonly Assembly Web = typeof(BuildingBlocks.Web.ResultExtensions).Assembly;
    public static readonly Assembly Security = typeof(BuildingBlocks.Security.AssemblyMarker).Assembly;
    public static readonly Assembly Persistence = typeof(BuildingBlocks.Persistence.AssemblyMarker).Assembly;

    /// <summary>Top-level statements only, no type to point at: loaded by name.</summary>
    public static readonly Assembly Gateway = Assembly.Load(new AssemblyName("Gateway.Bff"));

    public static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssemblies(
        [
            .. Modules.SelectMany(module => new[] { module.Implementation, module.Contracts }),
            SharedKernel,
            Web,
            Security,
            Persistence,
            Gateway,
        ])
        .Build();

    public static TheoryData<string> ModuleNames => [.. Modules.Select(module => module.Name)];

    public static Module Get(string moduleName) => Modules.Single(module => module.Name == moduleName);

    /// <summary>src/backend, found by walking up from the test binaries to Ecosystem.slnx.</summary>
    public static DirectoryInfo BackendRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ecosystem.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new InvalidOperationException("Ecosystem.slnx not found above the test binaries.");
    }
}
