namespace Architecture.Tests;

/// <summary>Guards the module list itself: a new module that is not listed would escape every rule.</summary>
public class ModuleListTests
{
    [Fact]
    public void ModuleList_MatchesTheModuleFolders()
    {
        var folders = ProductionCode.BackendRoot()
            .GetDirectories("Modules")
            .Single()
            .GetDirectories()
            .Select(directory => directory.Name)
            .Order(StringComparer.Ordinal);

        ProductionCode.Modules.Select(module => module.Name).Order(StringComparer.Ordinal).ShouldBe(
            folders,
            "every folder under src/backend/Modules must be listed in ProductionCode.Modules so the architecture rules cover it");
    }
}
