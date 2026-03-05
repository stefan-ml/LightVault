using System.IO.Pipelines;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace LightVault.Infrastructure.Services;

internal static class AuditAnchor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task WriteAsync(
        string path,
        long lastId,
        string lastHash,
        CancellationToken ct = default)
    {
        var anchor = new AnchorModel
        {
            LastId = lastId,
            LastHash = lastHash,
            WrittenAtUtc = DateTime.UtcNow
        };

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var tmp = path + ".tmp";

        await using (var fs = File.Create(tmp))
        {
            await JsonSerializer.SerializeAsync(fs, anchor, JsonOptions, ct);
            await fs.FlushAsync(ct);
        }

        if (File.Exists(path))
            File.Replace(tmp, path, null);
        else
            File.Move(tmp, path);
    }

    public static async Task<AnchorModel?> ReadAsync(
    string path,
    CancellationToken ct = default)
    {
        if (!File.Exists(path))
            return null;

        await using var fs = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<AnchorModel>(
            fs,
            options: null,
            cancellationToken: ct);
    }


    internal sealed class AnchorModel
    {
        public long LastId { get; set; }
        public string LastHash { get; set; } = default!;
        public DateTime WrittenAtUtc { get; set; }
    }
}
