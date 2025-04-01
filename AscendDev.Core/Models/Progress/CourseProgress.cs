using MongoDB.Bson;

namespace AscendDev.Core.Models.Progress;

public class CourseProgress
{
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId CourseId { get; set; }
}