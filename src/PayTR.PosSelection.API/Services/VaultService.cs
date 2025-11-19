using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace PayTR.PosSelection.API.Services;

public class VaultService
{
    private readonly IVaultClient vaultClient;
    private readonly string secretPath;

    public VaultService(IConfiguration configuration)
    {
        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(configuration["Vault:Token"]);
        var vaultClientSettings = new VaultClientSettings(configuration["Vault:Url"], authMethod);
        vaultClient = new VaultClient(vaultClientSettings);
        secretPath = configuration["Vault:Path"]!;
    }

    public void SetSecrets()
    {
        try
        {
            var secret = vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(secretPath, mountPoint: "secret")
                .GetAwaiter().GetResult();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configFilePath = $"appsettings.{environment}.json";

            if (!File.Exists(configFilePath))
            {
                File.WriteAllText(configFilePath, "{}");
            }

            var json = File.ReadAllText(configFilePath);
            var jsonObj = JsonNode.Parse(json) ?? new JsonObject();

            var vaultSecretsNode = ConvertVaultDataToJsonNode(secret.Data.Data);
            jsonObj["VaultSecrets"] = vaultSecretsNode;

            var updatedJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(configFilePath, updatedJson);
            Console.WriteLine($"Vault secrets written to {configFilePath}");
        }
        catch (Exception exception)
        {
            throw new Exception("Application failed to start due to secret configuration error", exception);
        }
    }

    private JsonNode ConvertVaultDataToJsonNode(IDictionary<string, object> vaultData)
    {
        var jsonObject = new JsonObject();

        foreach (var kvp in vaultData)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (value is IDictionary<string, object> nestedDict)
            {
                jsonObject[key] = ConvertVaultDataToJsonNode(nestedDict);
            }
            else if (value is System.Text.Json.JsonElement jsonElement)
            {
                jsonObject[key] = ConvertJsonElementToJsonNode(jsonElement);
            }
            else
            {
                jsonObject[key] = JsonValue.Create(value);
            }
        }

        return jsonObject;
    }

    private JsonNode? ConvertJsonElementToJsonNode(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new JsonObject();
                foreach (var property in element.EnumerateObject())
                {
                    obj[property.Name] = ConvertJsonElementToJsonNode(property.Value);
                }
                return obj;

            case JsonValueKind.Array:
                var array = new JsonArray();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(ConvertJsonElementToJsonNode(item));
                }
                return array;

            case JsonValueKind.String:
                return JsonValue.Create(element.GetString());

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return JsonValue.Create(intValue);
                if (element.TryGetInt64(out long longValue))
                    return JsonValue.Create(longValue);
                if (element.TryGetDouble(out double doubleValue))
                    return JsonValue.Create(doubleValue);
                return JsonValue.Create(element.GetDecimal());

            case JsonValueKind.True:
                return JsonValue.Create(true);

            case JsonValueKind.False:
                return JsonValue.Create(false);

            case JsonValueKind.Null:
                return null;

            default:
                return JsonValue.Create(element.ToString());
        }
    }
}