using Grand.Infrastructure.Migrations;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationUpdateResourceString : IMigration
{
    public int Priority => 0;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("C3D94E71-8B2A-4F67-AE15-72C06D94B830");
    public string Name => "Update resource string for english language 2.5";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        return serviceProvider.ImportLanguageResourcesFromXml("App_Data/Resources/Upgrade/en_250.xml");
    }
}
