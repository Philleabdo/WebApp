namespace grupp6WebApp.Models;

public class HomeIndexViewModel
{
    public List<Project> LatestProjects { get; set; } = new();
    public List<User> SuggestedUsers { get; set; } = new();
}