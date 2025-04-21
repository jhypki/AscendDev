using AscendDev.Core.Models.Courses;
using Dapper;

namespace AscendDev.Data;

public static class DapperConfig
{
    public static void SetupTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<string>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<LessonSummary>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<AdditionalResource>>());
        // SqlMapper.AddTypeHandler(new JsonTypeHandler<List<TestCase>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<TestCase>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<TestConfig>());
    }
}