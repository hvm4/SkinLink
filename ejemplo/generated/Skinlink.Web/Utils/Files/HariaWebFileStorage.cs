// HARIA-WEB GENERATED sha256:cf7ecf03c7062eabdc5ffdd81964d0f022ba2191b29286bf53e838eb80a27eaf
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;

namespace Skinlink.Web.Utils.Files;

public static class HariaWebUploadApplicationExtensions
{
    public static IApplicationBuilder UseHariaWebUploads(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var options = app.ApplicationServices.GetRequiredService<IOptions<HariaWebFileOptions>>().Value;
        var webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
        var directory = Path.GetFullPath(Path.Combine(webRoot, options.UploadRoot));
        if (options.UploadRoot.Contains('\\') || options.UploadRoot.Split('/').Any(segment => !Regex.IsMatch(segment, @"^[A-Za-z0-9_-]+$"))
            || !directory.StartsWith(webRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("HariaWeb:Files:UploadRoot debe ser una ruta relativa segura dentro de wwwroot.");
        Directory.CreateDirectory(directory);
        return app.UseStaticFiles(new StaticFileOptions
        {
        FileProvider = new PhysicalFileProvider(directory),
        RequestPath = "/" + options.UploadRoot,
        OnPrepareResponse = context =>
        {
            context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Context.Response.Headers["Content-Security-Policy"] = "sandbox; default-src 'none'";
        }
        });
    }
}

public sealed class HariaWebFileOptions
{
    public static readonly IReadOnlyDictionary<string, string> SupportedFormats = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };
    public string UploadRoot { get; set; } = "uploads";
    public long MaxBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = SupportedFormats.Keys.ToArray();
    public string[] AllowedContentTypes { get; set; } = SupportedFormats.Values.Distinct().ToArray();
    public bool DeleteReplacedFiles { get; set; } = true;
    public bool DeleteFilesOnEntityDelete { get; set; }
}

public interface IWebFileStorage
{
    Task ValidateAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<string> SaveAsync(IFormFile file, string domainClass, string attribute, object id, CancellationToken cancellationToken = default);
    Task DeleteNewAsync(string? relativePath);
    Task DeleteReplacedAsync(string? relativePath);
    Task DeleteEntityFileAsync(string? relativePath);
}

// Files in wwwroot are PUBLIC. This service is not an authorization boundary.
// Disk writes are compensated explicitly; they do not join SQL transactions.
public sealed class HariaWebFileStorage : IWebFileStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly HariaWebFileOptions _options;
    private readonly ILogger<HariaWebFileStorage> _logger;
    public HariaWebFileStorage(IWebHostEnvironment environment, IOptions<HariaWebFileOptions> options, ILogger<HariaWebFileStorage> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
        if (_options.MaxBytes <= 0) throw new InvalidOperationException("HariaWeb:Files:MaxBytes must be positive.");
        if (_options.AllowedExtensions.Any(extension => !HariaWebFileOptions.SupportedFormats.TryGetValue(extension, out var mime) || !_options.AllowedContentTypes.Contains(mime, StringComparer.OrdinalIgnoreCase))
            || _options.AllowedContentTypes.Any(contentType => !HariaWebFileOptions.SupportedFormats.Values.Contains(contentType, StringComparer.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Solo están implementados PNG y JPEG; ampliar la configuración requiere implementar el formato.");
        foreach (var segment in _options.UploadRoot.Replace('\\', '/').Split('/')) ValidateSegment(segment);
    }

    public async Task ValidateAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0 || file.Length > _options.MaxBytes) throw new InvalidDataException($"Selecciona un archivo no vacío de un máximo de {_options.MaxBytes} bytes.");
        if (file.FileName.Length > 240 || file.FileName.IndexOfAny(['/', '\\', ':']) >= 0 || file.FileName.Any(char.IsControl)
            || Regex.IsMatch(file.FileName, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)", RegexOptions.IgnoreCase))
            throw new InvalidDataException("El nombre del archivo no es válido.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) throw new InvalidDataException("La extensión del archivo no está permitida.");
        if (!HariaWebFileOptions.SupportedFormats.TryGetValue(extension, out var expectedMime) || !_options.AllowedContentTypes.Contains(expectedMime, StringComparer.OrdinalIgnoreCase)
            || !string.Equals(file.ContentType, expectedMime, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("El tipo de contenido no está permitido.");
        await using var stream = file.OpenReadStream();
        var header = new byte[8];
        var count = await stream.ReadAtLeastAsync(header, 8, false, cancellationToken);
        var valid = extension == ".png" ? count >= 8 && header.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            : count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255;
        if (!valid) throw new InvalidDataException("El contenido del archivo no coincide con su extensión.");
    }

    public async Task<string> SaveAsync(IFormFile file, string domainClass, string attribute, object id, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(file, cancellationToken);
        ValidateSegment(domainClass);
        ValidateSegment(attribute);
        var idText = Convert.ToString(id, CultureInfo.InvariantCulture) ?? "";
        if (idText.Length == 0 || idText == "0" || idText.Length > 100) throw new InvalidDataException("A persistent entity identifier is required before saving an upload.");
        var idSegment = Regex.IsMatch(idText, @"^-?[0-9]+$") ? idText : "k-" + Convert.ToHexString(Encoding.UTF8.GetBytes(idText)).ToLowerInvariant();
        var relative = string.Join('/', _options.UploadRoot.Replace('\\', '/'), domainClass, attribute, idSegment,
            Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant());
        var physical = Resolve(relative);
        Directory.CreateDirectory(Path.GetDirectoryName(physical)!);
        RejectReparsePoints(physical);
        try
        {
            await using var input = file.OpenReadStream();
            await using var output = new FileStream(physical, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            var buffer = new byte[81920];
            long total = 0;
            int count;
            while ((count = await input.ReadAsync(buffer, cancellationToken)) != 0)
            {
                total += count;
                if (total > _options.MaxBytes) throw new InvalidDataException("El archivo supera el tamaño máximo permitido.");
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            }
            if (total == 0 || total != file.Length) throw new InvalidDataException("La longitud del archivo no es coherente.");
        }
        catch
        {
            try { File.Delete(physical); } catch (IOException error) { _logger.LogWarning(error, "Could not compensate a partial upload."); }
            throw;
        }
        return relative;
    }

    public Task DeleteNewAsync(string? relativePath) => DeleteAsync(relativePath, true);
    public Task DeleteReplacedAsync(string? relativePath) => DeleteAsync(relativePath, _options.DeleteReplacedFiles);
    public Task DeleteEntityFileAsync(string? relativePath) => DeleteAsync(relativePath, _options.DeleteFilesOnEntityDelete);

    private Task DeleteAsync(string? relativePath, bool enabled)
    {
        if (!enabled || string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;
        try
        {
            var prefix = _options.UploadRoot.Replace('\\', '/') + "/";
            if (!relativePath.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException("Only managed uploads can be deleted.");
            var suffix = relativePath[prefix.Length..].Split('/');
            if (suffix.Length != 4 || !Regex.IsMatch(suffix[3], @"^[a-f0-9]{32}\.[a-z0-9]+$") || !HariaWebFileOptions.SupportedFormats.ContainsKey(Path.GetExtension(suffix[3]))) throw new InvalidDataException("The path is not a managed upload.");
            var physical = Resolve(relativePath);
            RejectReparsePoints(physical);
            File.Delete(physical);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
        {
            _logger.LogWarning(error, "Upload cleanup failed; the stored path was retained for investigation.");
        }
        return Task.CompletedTask;
    }

    private string Resolve(string relative)
    {
        if (Path.IsPathRooted(relative) || relative.Contains('\\')) throw new InvalidDataException("An upload path must be relative.");
        foreach (var segment in relative.Split('/')) ValidateSegment(segment);
        var webRoot = Path.GetFullPath(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"));
        var root = Path.GetFullPath(Path.Combine(webRoot, _options.UploadRoot.Replace('/', Path.DirectorySeparatorChar)));
        var physical = Path.GetFullPath(Path.Combine(webRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!root.StartsWith(webRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison)
            || !physical.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison))
            throw new InvalidDataException("An upload path escaped wwwroot/uploads.");
        RejectReparsePoints(physical);
        return physical;
    }

    private void RejectReparsePoints(string physical)
    {
        var webRoot = Path.GetFullPath(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"));
        for (var directory = new DirectoryInfo(Path.GetDirectoryName(physical)!); directory is not null; directory = directory.Parent)
        {
            if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Upload paths cannot contain symbolic links.");
            if (string.Equals(directory.FullName, webRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) break;
        }
    }

    private static void ValidateSegment(string value)
    {
        if (value.Length == 0 || value.Length > 240 || value is "." or ".." || value.EndsWith('.') || value.EndsWith(' ')
            || !Regex.IsMatch(value, @"^[A-Za-z0-9_.-]+$")
            || Regex.IsMatch(value, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)", RegexOptions.IgnoreCase))
            throw new InvalidDataException("An upload path segment is invalid.");
    }
}
