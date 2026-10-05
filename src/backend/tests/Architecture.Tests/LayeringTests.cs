using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

/// <summary>Rule 3 of ticket #3: the domain stays free of infrastructure (ADR-007).</summary>
public class LayeringTests
{
    [Fact]
    public void Domain_DoesNotDependOnInfrastructure()
    {
        Types().That().ResideInNamespaceMatching(@"\.Domain(\..+)?$")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^(Microsoft\.EntityFrameworkCore|Microsoft\.AspNetCore|System\.Net\.Http)(\..+)?$")
            .Because("domain code holds business rules only, persistence and HTTP belong to the outer layers (ADR-007)")
            // No *.Domain namespace exists yet: without this, ArchUnitNET fails a rule that matches no type.
            .WithoutRequiringPositiveResults()
            .Check(ProductionCode.Architecture);
    }
}
