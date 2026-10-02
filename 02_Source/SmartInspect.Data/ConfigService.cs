using SmartInspect.Core;
using System;
using System.IO;
using System.Text.Json;

namespace SmartInspect.Data
{
    public class ConfigService
    {
        private readonly string configPath;

        public ConfigService()
        {
            configPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "appsettings.json");
        }

        public void SaveConfig(AppConfig config)
        {
            string json =
                JsonSerializer.Serialize(
                    config,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                configPath,
                json);
        }

        public AppConfig LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                return new AppConfig();
            }

            string json =
                File.ReadAllText(configPath);

            AppConfig? config =
                JsonSerializer.Deserialize<AppConfig>(
                    json);

            if (config == null)
            {
                return new AppConfig();
            }

            return config;
        }
    }
}