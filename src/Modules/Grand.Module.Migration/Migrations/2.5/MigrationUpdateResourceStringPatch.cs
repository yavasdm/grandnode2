using Grand.Infrastructure.Migrations;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationUpdateResourceStringPatch : IMigration
{
    public int Priority => 1;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("E7F8A9B0-C1D2-4E3F-5A6B-7C8D9E0F1A2B");
    public string Name => "Update resource strings patch for english language 2.5";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        return serviceProvider.ImportLanguageResourcesFromXml("App_Data/Resources/Upgrade/en_250_patch.xml");
    }
}
