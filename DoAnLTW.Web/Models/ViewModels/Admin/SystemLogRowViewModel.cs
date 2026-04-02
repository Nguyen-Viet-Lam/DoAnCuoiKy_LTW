namespace DoAnLTW.Web.Models.ViewModels;

public class SystemLogRowViewModel
{
    public string Level { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UserDisplayName { get; set; }
}
