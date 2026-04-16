using System.Text.Json;
using PhysOn.Desktop.Models;

namespace PhysOn.Desktop.Services;

public sealed class WorkspaceLayoutStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _directoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PhysOn");
    private readonly string _layoutPath;

    public WorkspaceLayoutStore()
    {
        _layoutPath = Path.Combine(_directoryPath, "workspace-layout.json");
    }

    public async Task<DesktopWorkspaceLayout?> LoadAsync()
    {
        if (!File.Exists(_layoutPath))
        {
            return null;
        }

        try
        {
            var payload = await File.ReadAllTextAsync(_layoutPath);
            return JsonSerializer.Deserialize<DesktopWorkspaceLayout>(payload, JsonOptions);
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveAsync(DesktopWorkspaceLayout layout)
    {
        Directory.CreateDirectory(_directoryPath);
        var payload = JsonSerializer.Serialize(layout, JsonOptions);
        await File.WriteAllTextAsync(_layoutPath, payload);
    }
}
