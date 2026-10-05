using System.Xml.Linq;

namespace Architecture.Tests;

/// <summary>
/// Rules 1, 2 and 4 again, on the .csproj files. The compiler drops a project reference no code uses yet,
/// so ArchUnitNET cannot see it: these tests catch the reference as soon as it is added.
/// </summary>
public class ProjectReferenceTests
{
    private const string SharedKernel = "BuildingBlocks/SharedKernel/BuildingBlocks.SharedKernel.csproj";

    [Theory]
    [MemberData(nameof(ProductionCode.ModuleNames), MemberType = typeof(ProductionCode))]
    public void ModuleProject_ReferencesOtherModules_OnlyThroughTheirContracts(string moduleName)
    {
        var forbidden = ProjectReferences($"Modules/{moduleName}/{moduleName}/{moduleName}.csproj")
            .Where(reference => reference.StartsWith("Modules/", StringComparison.Ordinal)
                && !reference.EndsWith(".Contracts.csproj", StringComparison.Ordinal));

        forbidden.ShouldBeEmpty(
            $"{moduleName} may reference another module only through that module's *.Contracts project (ADR-001)");
    }

    [Theory]
    [MemberData(nameof(ProductionCode.ModuleNames), MemberType = typeof(ProductionCode))]
    public void ContractsProject_ReferencesOnlySharedKernel(string moduleName)
    {
        var forbidden = ProjectReferences($"Modules/{moduleName}/{moduleName}.Contracts/{moduleName}.Contracts.csproj")
            .Where(reference => reference != SharedKernel);

        forbidden.ShouldBeEmpty($"{moduleName}.Contracts may reference only BuildingBlocks.SharedKernel");
    }

    [Fact]
    public void GatewayProject_ReferencesNoModule()
    {
        var forbidden = ProjectReferences("Gateway.Bff/Gateway.Bff.csproj")
            .Where(reference => reference.StartsWith("Modules/", StringComparison.Ordinal));

        forbidden.ShouldBeEmpty("Gateway.Bff talks to the modules over HTTP through Host.Core, never in process (ADR-009)");
    }

    /// <summary>The project's references, as paths relative to src/backend with forward slashes.</summary>
    private static List<string> ProjectReferences(string projectPath)
    {
        var root = ProductionCode.BackendRoot().FullName;
        var project = Path.Combine(root, projectPath);
        var projectDirectory = Path.GetDirectoryName(project)!;

        return XDocument.Load(project)
            .Descendants("ProjectReference")
            .Select(reference => (string)reference.Attribute("Include")!)
            .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include.Replace('\\', Path.DirectorySeparatorChar))))
            .Select(fullPath => Path.GetRelativePath(root, fullPath).Replace(Path.DirectorySeparatorChar, '/'))
            .ToList();
    }
}
