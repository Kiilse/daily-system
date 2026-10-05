using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

/// <summary>Rules 1, 2 and 4 of ticket #3, checked on the compiled code (ADR-001).</summary>
public class ModuleBoundaryTests
{
    [Theory]
    [MemberData(nameof(ProductionCode.ModuleNames), MemberType = typeof(ProductionCode))]
    public void Module_UsesOtherModules_OnlyThroughTheirContracts(string moduleName)
    {
        var module = ProductionCode.Get(moduleName);
        var otherImplementations = ProductionCode.Modules
            .Where(other => other != module)
            .Select(other => other.Implementation)
            .ToArray();

        Types().That().ResideInAssembly(module.Implementation)
            .Should().NotDependOnAnyTypesThat().ResideInAssembly(otherImplementations[0], otherImplementations[1..])
            .Because($"{moduleName} may use another module only through that module's *.Contracts project (ADR-001)")
            .Check(ProductionCode.Architecture);
    }

    [Theory]
    [MemberData(nameof(ProductionCode.ModuleNames), MemberType = typeof(ProductionCode))]
    public void Contracts_DependOnlyOnSharedKernel(string moduleName)
    {
        var module = ProductionCode.Get(moduleName);
        var allowed = Types(includeReferenced: true).That()
            .ResideInAssembly(module.Contracts, ProductionCode.SharedKernel)
            .Or().ResideInNamespaceMatching(@"^System(\..+)?$");

        Types().That().ResideInAssembly(module.Contracts)
            .Should().OnlyDependOn(allowed)
            .Because($"{moduleName}.Contracts is shared with other modules and may depend only on BuildingBlocks.SharedKernel")
            .Check(ProductionCode.Architecture);
    }

    [Fact]
    public void Gateway_DoesNotUseAnyModule()
    {
        var moduleAssemblies = ProductionCode.Modules
            .SelectMany(module => new[] { module.Implementation, module.Contracts })
            .ToArray();

        Types().That().ResideInAssembly(ProductionCode.Gateway)
            .Should().NotDependOnAnyTypesThat().ResideInAssembly(moduleAssemblies[0], moduleAssemblies[1..])
            .Because("Gateway.Bff talks to the modules over HTTP through Host.Core, never in process (ADR-009)")
            .Check(ProductionCode.Architecture);
    }
}
