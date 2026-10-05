using System.Security.Cryptography;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

/// <summary>Rule 6 of ticket #3: encryption lives in one audited place (ADR-010).</summary>
public class CryptographyTests
{
    [Fact]
    public void AesGcm_IsUsedOnlyInBuildingBlocksSecurity()
    {
        Types().That().DoNotResideInAssembly(ProductionCode.Security)
            .Should().NotDependOnAnyTypesThat().HaveFullName(typeof(AesGcm).FullName!)
            .Because("field encryption goes through BuildingBlocks.Security, never ad hoc AesGcm calls (ADR-010)")
            .Check(ProductionCode.Architecture);
    }
}
