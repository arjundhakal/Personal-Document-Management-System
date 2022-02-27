using PDMS.Application.Interfaces;
using PDMS.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMS.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IDatabaseRepository<Setting> _settingRepository;
        public SettingsService(IDatabaseRepository<Setting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<string> GetSettingValueByKey(string key)
        {
            var setting = await _settingRepository.SingleOrDefaultAsync(x => x.Key == key);
            if (setting == default)
                return "";

            return setting.Value;
        }
    }
}
