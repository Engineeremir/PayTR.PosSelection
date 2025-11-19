using Microsoft.Extensions.Configuration;

namespace PayTR.PosSelection.Shared.Utils;

public static class ConfigurationExtensions
{
    // Section ve key ile erişim
    public static T GetSecretValue<T>(this IConfiguration configuration, string section, string key)
    {
        var value = configuration.GetSection($"VaultSecrets:{section}")[key];
        if (value == null)
            throw new ArgumentException($"Secret key '{section}:{key}' not found in VaultSecrets");

        return (T)Convert.ChangeType(value, typeof(T));
    }

    // Path ile erişim (örn: "DatabaseSettings:ProductDatabase")
    public static T GetSecretValue<T>(this IConfiguration configuration, string path)
    {
        var value = configuration.GetSection($"VaultSecrets:{path}").Value;
        if (value == null)
            throw new ArgumentException($"Secret path 'VaultSecrets:{path}' not found");

        return (T)Convert.ChangeType(value, typeof(T));
    }

    // Section'ın tamamını almak için
    public static IConfigurationSection GetSecretSection(this IConfiguration configuration, string section)
    {
        return configuration.GetSection($"VaultSecrets:{section}");
    }

    // Section'ı bind etmek için
    public static T GetSecretSection<T>(this IConfiguration configuration, string section) where T : new()
    {
        var configSection = configuration.GetSection($"VaultSecrets:{section}");
        var instance = new T();
        configSection.Bind(instance);
        return instance;
    }
}
