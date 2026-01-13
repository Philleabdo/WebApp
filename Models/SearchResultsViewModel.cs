namespace grupp6WebApp.Models;

public class SearchResultsViewModel
{
    public string? Query { get; set; }
    public List<Project> Projects { get; set; } = new();
    public List<User> Users { get; set; } = new();
}