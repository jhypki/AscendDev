using MongoDB.Bson;

namespace AscendDev.Core.Models.Courses;

public class Lesson
{
    public ObjectId Id { get; set; }
    public ObjectId CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string CodeTemplate { get; set; } = null!;
    public string MainFunction { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Order { get; set; }
    public string Status { get; set; } = "Draft";
    public TestConfig TestConfig { get; set; } = null!;
    public List<AdditionalResource> AdditionalResources { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}