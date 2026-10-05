using System.Reflection;

namespace Architecture.Tests;

/// <summary>
/// Placeholder until ticket #3 adds the ArchUnitNET boundary rules: checks that every module
/// assembly the rules will inspect is referenced and loads.
/// </summary>
public class ModuleAssembliesTests
{
    public static TheoryData<Assembly> ModuleAssemblies =>
    [
        typeof(Account.AccountModule).Assembly,
        typeof(Menu.MenuModule).Assembly,
        typeof(Calendar.CalendarModule).Assembly,
    ];

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void ModuleAssembly_Loads_AndExposesItsModuleEntryPoint(Assembly assembly)
    {
        assembly.ExportedTypes.ShouldContain(type => type.Name.EndsWith("Module", StringComparison.Ordinal));
    }
}
