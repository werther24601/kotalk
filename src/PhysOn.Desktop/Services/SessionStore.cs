using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PhysOn.Desktop.Models;

namespace PhysOn.Desktop.Services;

public sealed class SessionStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("PhysOn.Desktop.Session");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly byte[] WindowsHeader = "VSMW1"u8.ToArray();

    private readonly string _sessionDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PhysOn");
    private readonly string _sessionPath;
    private readonly string _legacySessionPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PhysOn",
        "session.json");

    public SessionStore()
    {
        _sessionPath = OperatingSystem.IsWindows()
            ? Path.Combine(_sessionDirectory, "session.dat")
            : Path.Combine(_sessionDirectory, "session.json");
    }

    public async Task<DesktopSession?> LoadAsync()
    {
        if (File.Exists(_sessionPath))
        {
            return await LoadFromPathAsync(_sessionPath);
        }

        if (File.Exists(_legacySessionPath))
        {
            return await LoadFromPathAsync(_legacySessionPath);
        }

        return null;
    }

    public async Task SaveAsync(DesktopSession session)
    {
        Directory.CreateDirectory(_sessionDirectory);
        ApplyDirectoryPermissions(_sessionDirectory);

        var payload = JsonSerializer.SerializeToUtf8Bytes(session, JsonOptions);
        if (OperatingSystem.IsWindows())
        {
            payload = WindowsHeader
                .Concat(ProtectedData.Protect(payload, Entropy, DataProtectionScope.CurrentUser))
                .ToArray();
        }

        await File.WriteAllBytesAsync(_sessionPath, payload);
        ApplyFilePermissions(_sessionPath);

        if (File.Exists(_legacySessionPath))
        {
            File.Delete(_legacySessionPath);
        }
    }

    public Task ClearAsync()
    {
        if (File.Exists(_sessionPath))
        {
            File.Delete(_sessionPath);
        }

        if (File.Exists(_legacySessionPath))
        {
            File.Delete(_legacySessionPath);
        }

        return Task.CompletedTask;
    }

    private async Task<DesktopSession?> LoadFromPathAsync(string path)
    {
        try
        {
            var payload = await File.ReadAllBytesAsync(path);
            if (payload.Length == 0)
            {
                return null;
            }

            if (OperatingSystem.IsWindows() && payload.AsSpan().StartsWith(WindowsHeader))
            {
                var encrypted = payload.AsSpan(WindowsHeader.Length).ToArray();
                var decrypted = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
                return JsonSerializer.Deserialize<DesktopSession>(decrypted, JsonOptions);
            }

            return JsonSerializer.Deserialize<DesktopSession>(payload, JsonOptions);
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void ApplyDirectoryPermissions(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            File.SetAttributes(path, FileAttributes.Directory | FileAttributes.Hidden);
            return;
        }

        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    private static void ApplyFilePermissions(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            File.SetAttributes(path, FileAttributes.Hidden);
            return;
        }

        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }
}
