using System.Collections.Generic;
using System.IO;
using CryptoBudged.Models;
using Newtonsoft.Json;

namespace CryptoBudged.Services
{
    public class GridConfigurationService
    {
        private const string GridConfigurationFilePath = "gridConfiguration.json";

        private readonly Dictionary<string, GridConfigurationModel> _configurations;

        private GridConfigurationService()
        {
            if (File.Exists(GridConfigurationFilePath))
            {
                var content = File.ReadAllText(GridConfigurationFilePath);
                _configurations = JsonConvert.DeserializeObject<Dictionary<string, GridConfigurationModel>>(content);
            }
            else
            {
                _configurations = new Dictionary<string, GridConfigurationModel>();
            }
        }

        public GridConfigurationModel LoadConfiguration(string gridName)
        {
            if (_configurations.ContainsKey(gridName))
                return _configurations[gridName];

            return null;
        }

        public void SaveConfiguration(string gridName, GridConfigurationModel model)
        {
            if (_configurations.ContainsKey(gridName))
                _configurations[gridName] = model;
            else
                _configurations.Add(gridName, model);

            File.WriteAllText(GridConfigurationFilePath, JsonConvert.SerializeObject(_configurations));
        }

        public static GridConfigurationService Instance { get; } = new GridConfigurationService();
    }
}
