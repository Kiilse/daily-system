using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

/// <summary>Rule 5 of ticket #3: a module exposes its registration class and nothing else (ADR-001, ADR-007).</summary>
public class VisibilityTests
{
    [Theory]
    [MemberData(nameof(ProductionCode.ModuleNames), MemberType = typeof(ProductionCode))]
    public void ModuleImplementation_ExposesOnlyItsModuleClass(string moduleName)
    {
        var module = ProductionCode.Get(moduleName);

        Types().That().ResideInAssembly(module.Implementation)
            .And().DoNotHaveFullName($"{moduleName}.{moduleName}Module")
            .Should().NotBePublic()
            .Because($"only {moduleName}Module (registration and endpoint mapping) is public, handlers and the rest stay internal; other modules go through {moduleName}.Contracts")
            .Check(ProductionCode.Architecture);
    }
}
