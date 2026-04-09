namespace DoAnLTW.Web.Services.Finance;

/// <summary>Service lưu, thay thế và trả về đường dẫn ảnh đại diện của người dùng.</summary>
public class AvatarStorageService
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Khởi tạo lớp AvatarStorageService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public AvatarStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Lưu ảnh đại diện mới lên ổ đĩa và trả về đường dẫn đã lưu.
    /// </summary>
    public async Task<string?> SaveAsync(IFormFile? file, string? currentRelativePath, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return currentRelativePath;
        }

        var extension = Path.GetExtension(file.FileName);
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return currentRelativePath;
        }

        var root = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(root);

        if (!string.IsNullOrWhiteSpace(currentRelativePath))
        {
            var currentPath = Path.Combine(_environment.WebRootPath, currentRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(currentPath))
            {
                File.Delete(currentPath);
            }
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(root, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/avatars/{fileName}";
    }
}
