using PDMS.Domain.Entity;

namespace PDMS.Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedDefaultSettingsAsync(ApplicationDbContext context)
        {
            foreach (var setting in DefaultListOfSettings())
            {
                var listOfSettings = DefaultListOfSettings();
                var existingSetting = context.Settings.SingleOrDefault(x => x.Key == setting.Key);
                if (existingSetting == default)
                    await context.AddAsync(setting);
            }
            await context.SaveChangesAsync();
        }


        private static List<Setting> DefaultListOfSettings()
        {
            return new List<Setting>()
            {
                new Setting()
                {
                    Key = "SessionTimeoutInMins",
                    Value = "15",
                    CreatedAt = DateTime.Now,
                    LastModified = DateTime.Now,
                    LastModifiedBy = ""
                }
            };
        }
    }
}