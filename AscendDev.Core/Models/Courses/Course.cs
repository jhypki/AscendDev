using MongoDB.Bson;

namespace AscendDev.Core.Models.Courses;

public class Course
{
    public ObjectId Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Language { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<Lesson> Lessons { get; set; } = [];
    public string Status { get; set; } = "Draft";
    public Guid CreatedBy { get; set; }
}