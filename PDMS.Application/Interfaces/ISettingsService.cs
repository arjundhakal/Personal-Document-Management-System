
namespace PDMS.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<string> GetSettingValueByKey(string key);
    }
}
