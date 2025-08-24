# Metrics Service Architecture and Implementation Plan

## Overview

The Metrics Service is a dedicated microservice designed to collect, process, and analyze user behavior and platform performance data from the AscendDev e-learning platform. It consumes events from Apache Kafka, processes them in real-time, and provides analytics APIs for the admin panel and reporting systems.

## Why a Separate Metrics Microservice?

### Benefits of Microservice Architecture
- **Performance Isolation**: Analytics processing doesn't impact main application performance
- **Scalability**: Independent scaling based on analytics workload
- **Technology Optimization**: Use specialized tools for data processing and analytics
- **Data Retention**: Separate data lifecycle management for metrics vs operational data
- **Real-time Processing**: Dedicated resources for stream processing and aggregations
- **Fault Tolerance**: Analytics failures don't affect core learning functionality

### Technical Justification
- **High Volume Data**: Potentially millions of events per day
- **Complex Aggregations**: Real-time and batch processing requirements
- **Different Storage Patterns**: Time-series data vs relational data
- **Specialized Infrastructure**: Kafka, ClickHouse, Redis for different data needs

## Architecture Overview

### System Components
```
┌─────────────────┐    ┌──────────────────────┐    ┌─────────────────┐
│   Main API      │    │     Apache Kafka     │    │  Metrics Service│
│   (AscendDev)   │───►│   (Event Stream)     │───►│   (Consumer)    │
│                 │    │                      │    │                 │
└─────────────────┘    └──────────────────────┘    └─────────────────┘
         │                        │                          │
         │                        │                          ▼
         │                        │                ┌─────────────────┐
         │                        │                │   ClickHouse    │
         │                        │                │  (Time Series)  │
         │                        │                └─────────────────┘
         │                        │                          │
         ▼                        │                          ▼
┌─────────────────┐               │                ┌─────────────────┐
│   PostgreSQL    │               │                │     Redis       │
│   (Main DB)     │               │                │   (Cache)       │
└─────────────────┘               │                └─────────────────┘
                                  │                          │
                                  │                          ▼
                                  │                ┌─────────────────┐
                                  │                │  Admin Panel    │
                                  │                │   (Analytics)   │
                                  │                └─────────────────┘
```

### Data Flow
1. **Event Generation**: Main API generates events for user actions
2. **Event Publishing**: Events published to Kafka topics
3. **Stream Processing**: Metrics service consumes and processes events
4. **Data Storage**: Processed data stored in ClickHouse and Redis
5. **API Access**: Admin panel queries metrics via REST APIs
6. **Real-time Updates**: WebSocket connections for live dashboards

## Event Schema and Types

### Core Event Types
```csharp
public enum EventType
{
    // User Events
    UserRegistered,
    UserLoggedIn,
    UserLoggedOut,
    UserProfileUpdated,
    
    // Learning Events
    LessonStarted,
    LessonCompleted,
    LessonAbandoned,
    CodeSubmitted,
    TestExecuted,
    TestPassed,
    TestFailed,
    
    // Social Events
    DiscussionCreated,
    DiscussionReplied,
    CodeReviewSubmitted,
    CodeReviewCompleted,
    AchievementEarned,
    
    // System Events
    ErrorOccurred,
    PerformanceMetric,
    ResourceUsage,
    
    // Admin Events
    CourseCreated,
    CoursePublished,
    UserSuspended,
    ContentModerated
}
```

### Event Schema
```csharp
public class BaseEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public EventType EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; }
    public string SessionId { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public Dictionary<string, string> Context { get; set; } = new();
}

// Specific Event Examples
public class LessonCompletedEvent : BaseEvent
{
    public string CourseId { get; set; }
    public string LessonId { get; set; }
    public TimeSpan Duration { get; set; }
    public int AttemptsCount { get; set; }
    public double CompletionScore { get; set; }
    public List<string> HintsUsed { get; set; }
}

public class CodeSubmittedEvent : BaseEvent
{
    public string LessonId { get; set; }
    public string Language { get; set; }
    public int CodeLength { get; set; }
    public int ExecutionTimeMs { get; set; }
    public bool TestsPassed { get; set; }
    public int TestsTotal { get; set; }
    public int TestsPassing { get; set; }
    public List<string> ErrorTypes { get; set; }
}

public class UserEngagementEvent : BaseEvent
{
    public string PageUrl { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public int ScrollDepth { get; set; }
    public List<string> ActionsPerformed { get; set; }
}
```

## Kafka Integration

### Topic Structure
```yaml
Topics:
  user-events:
    partitions: 6
    replication-factor: 3
    retention: 30 days
    
  learning-events:
    partitions: 12
    replication-factor: 3
    retention: 90 days
    
  social-events:
    partitions: 6
    replication-factor: 3
    retention: 60 days
    
  system-events:
    partitions: 3
    replication-factor: 3
    retention: 7 days
    
  admin-events:
    partitions: 3
    replication-factor: 3
    retention: 365 days
```

### Event Publishing (Main API)
```csharp
public interface IEventPublisher
{
    Task PublishAsync<T>(T eventData) where T : BaseEvent;
    Task PublishBatchAsync<T>(IEnumerable<T> events) where T : BaseEvent;
}

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public async Task PublishAsync<T>(T eventData) where T : BaseEvent
    {
        var topic = GetTopicForEventType(eventData.EventType);
        var key = eventData.UserId ?? eventData.SessionId;
        var value = JsonSerializer.Serialize(eventData);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = value,
            Headers = new Headers
            {
                { "event-type", Encoding.UTF8.GetBytes(eventData.EventType.ToString()) },
                { "timestamp", Encoding.UTF8.GetBytes(eventData.Timestamp.ToString("O")) }
            }
        });
    }

    private string GetTopicForEventType(EventType eventType)
    {
        return eventType switch
        {
            EventType.UserRegistered or EventType.UserLoggedIn => "user-events",
            EventType.LessonStarted or EventType.LessonCompleted => "learning-events",
            EventType.DiscussionCreated or EventType.CodeReviewSubmitted => "social-events",
            EventType.ErrorOccurred or EventType.PerformanceMetric => "system-events",
            EventType.CourseCreated or EventType.UserSuspended => "admin-events",
            _ => "general-events"
        };
    }
}
```

### Event Publishing Integration
```csharp
// In Main API Controllers
[HttpPost("api/lessons/{lessonId}/complete")]
public async Task<IActionResult> CompleteLesson(string lessonId, [FromBody] CompleteLessonRequest request)
{
    // Business logic
    var result = await _lessonService.CompleteLessonAsync(lessonId, userId);
    
    // Publish event
    await _eventPublisher.PublishAsync(new LessonCompletedEvent
    {
        UserId = userId,
        CourseId = result.CourseId,
        LessonId = lessonId,
        Duration = result.Duration,
        AttemptsCount = result.AttemptsCount,
        CompletionScore = result.Score,
        HintsUsed = result.HintsUsed
    });
    
    return Ok(result);
}
```

## Metrics Service Architecture

### Service Structure
```
AscendDev.Metrics.API/
├── Controllers/
│   ├── AnalyticsController.cs
│   ├── DashboardController.cs
│   └── ReportsController.cs
├── Hubs/
│   └── MetricsHub.cs (SignalR for real-time updates)
├── Services/
│   ├── EventProcessingService.cs
│   ├── AggregationService.cs
│   ├── ReportingService.cs
│   └── CacheService.cs
├── Consumers/
│   ├── UserEventConsumer.cs
│   ├── LearningEventConsumer.cs
│   ├── SocialEventConsumer.cs
│   └── SystemEventConsumer.cs
├── Models/
│   ├── Events/
│   ├── Metrics/
│   └── Reports/
└── Infrastructure/
    ├── Kafka/
    ├── ClickHouse/
    └── Redis/

AscendDev.Metrics.Core/
├── Interfaces/
├── Models/
├── Services/
└── Utilities/

AscendDev.Metrics.Infrastructure/
├── Kafka/
├── ClickHouse/
├── Redis/
└── Caching/
```

### Event Consumers
```csharp
public class LearningEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IEventProcessor _eventProcessor;
    private readonly ILogger<LearningEventConsumer> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(new[] { "learning-events" });

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                
                if (consumeResult?.Message?.Value != null)
                {
                    await ProcessEvent(consumeResult.Message);
                    _consumer.Commit(consumeResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing learning event");
            }
        }
    }

    private async Task ProcessEvent(Message<string, string> message)
    {
        var eventType = GetEventTypeFromHeaders(message.Headers);
        var eventData = JsonSerializer.Deserialize<BaseEvent>(message.Value);

        await _eventProcessor.ProcessAsync(eventData);
    }
}
```

### Event Processing Service
```csharp
public interface IEventProcessor
{
    Task ProcessAsync(BaseEvent eventData);
    Task ProcessBatchAsync(IEnumerable<BaseEvent> events);
}

public class EventProcessor : IEventProcessor
{
    private readonly IClickHouseRepository _clickHouseRepo;
    private readonly IAggregationService _aggregationService;
    private readonly ICacheService _cacheService;

    public async Task ProcessAsync(BaseEvent eventData)
    {
        // Store raw event
        await _clickHouseRepo.InsertEventAsync(eventData);

        // Update real-time aggregations
        await _aggregationService.UpdateAggregationsAsync(eventData);

        // Update cache for frequently accessed metrics
        await _cacheService.UpdateMetricsCacheAsync(eventData);

        // Trigger real-time notifications if needed
        await NotifyRealTimeSubscribers(eventData);
    }

    private async Task NotifyRealTimeSubscribers(BaseEvent eventData)
    {
        // Send real-time updates via SignalR
        if (IsRealTimeMetric(eventData))
        {
            await _metricsHub.Clients.Group("dashboard")
                .SendAsync("MetricUpdated", CreateMetricUpdate(eventData));
        }
    }
}
```

## Data Storage Strategy

### ClickHouse Schema
```sql
-- Raw Events Table
CREATE TABLE events (
    event_id String,
    event_type String,
    timestamp DateTime64(3),
    user_id String,
    session_id String,
    properties String, -- JSON
    context String     -- JSON
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (event_type, timestamp, user_id);

-- User Activity Aggregations
CREATE TABLE user_activity_daily (
    date Date,
    user_id String,
    lessons_started UInt32,
    lessons_completed UInt32,
    code_submissions UInt32,
    tests_passed UInt32,
    time_spent_minutes UInt32,
    achievements_earned UInt32
) ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(date)
ORDER BY (date, user_id);

-- Course Performance Metrics
CREATE TABLE course_metrics_hourly (
    datetime DateTime,
    course_id String,
    lesson_id String,
    completion_rate Float32,
    average_attempts Float32,
    average_duration_minutes Float32,
    success_rate Float32
) ENGINE = ReplacingMergeTree()
PARTITION BY toYYYYMM(datetime)
ORDER BY (datetime, course_id, lesson_id);

-- System Performance Metrics
CREATE TABLE system_metrics (
    timestamp DateTime64(3),
    metric_name String,
    metric_value Float64,
    tags Map(String, String)
) ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (timestamp, metric_name);
```

### Redis Cache Structure
```redis
# Real-time counters
metrics:realtime:active_users -> Set (user IDs)
metrics:realtime:lessons_completed_today -> Counter
metrics:realtime:code_executions_per_minute -> Time series

# Aggregated metrics cache (TTL: 5 minutes)
metrics:cache:user_stats:{user_id} -> Hash
metrics:cache:course_stats:{course_id} -> Hash
metrics:cache:system_health -> Hash

# Dashboard data cache (TTL: 1 minute)
dashboard:active_users_count -> String
dashboard:popular_courses -> List
dashboard:recent_activities -> List
```

## Analytics APIs

### Core Analytics Endpoints
```csharp
[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<DashboardMetrics> GetDashboardMetrics([FromQuery] TimeRange timeRange)
    {
        return await _analyticsService.GetDashboardMetricsAsync(timeRange);
    }

    [HttpGet("users/{userId}/activity")]
    public async Task<UserActivityMetrics> GetUserActivity(string userId, [FromQuery] TimeRange timeRange)
    {
        return await _analyticsService.GetUserActivityAsync(userId, timeRange);
    }

    [HttpGet("courses/{courseId}/performance")]
    public async Task<CoursePerformanceMetrics> GetCoursePerformance(string courseId, [FromQuery] TimeRange timeRange)
    {
        return await _analyticsService.GetCoursePerformanceAsync(courseId, timeRange);
    }

    [HttpGet("system/health")]
    public async Task<SystemHealthMetrics> GetSystemHealth()
    {
        return await _analyticsService.GetSystemHealthAsync();
    }

    [HttpPost("reports/generate")]
    public async Task<ReportGenerationResult> GenerateReport([FromBody] ReportRequest request)
    {
        return await _reportingService.GenerateReportAsync(request);
    }
}
```

### Real-time Metrics Hub
```csharp
[Authorize(Roles = "SuperAdmin,Admin")]
public class MetricsHub : Hub
{
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        
        // Send current metrics
        var currentMetrics = await _metricsService.GetRealTimeMetricsAsync();
        await Clients.Caller.SendAsync("CurrentMetrics", currentMetrics);
    }

    public async Task LeaveDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
    }

    public async Task SubscribeToMetric(string metricName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"metric-{metricName}");
    }
}
```

### Data Models
```csharp
public class DashboardMetrics
{
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
    public int LessonsCompletedToday { get; set; }
    public int CodeExecutionsToday { get; set; }
    public double AverageSessionDuration { get; set; }
    public List<PopularCourse> PopularCourses { get; set; }
    public List<RecentActivity> RecentActivities { get; set; }
    public SystemHealthStatus SystemHealth { get; set; }
}

public class UserActivityMetrics
{
    public string UserId { get; set; }
    public TimeRange Period { get; set; }
    public int LessonsStarted { get; set; }
    public int LessonsCompleted { get; set; }
    public int CodeSubmissions { get; set; }
    public int TestsPassed { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public List<DailyActivity> DailyBreakdown { get; set; }
    public List<CourseProgress> CourseProgress { get; set; }
}

public class CoursePerformanceMetrics
{
    public string CourseId { get; set; }
    public TimeRange Period { get; set; }
    public int TotalEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public List<LessonPerformance> LessonPerformance { get; set; }
    public List<CommonError> CommonErrors { get; set; }
    public UserSatisfactionMetrics Satisfaction { get; set; }
}
```

## Aggregation Service

### Real-time Aggregations
```csharp
public class AggregationService : IAggregationService
{
    private readonly IClickHouseRepository _clickHouse;
    private readonly ICacheService _cache;

    public async Task UpdateAggregationsAsync(BaseEvent eventData)
    {
        await Task.WhenAll(
            UpdateUserAggregations(eventData),
            UpdateCourseAggregations(eventData),
            UpdateSystemAggregations(eventData),
            UpdateRealTimeCounters(eventData)
        );
    }

    private async Task UpdateUserAggregations(BaseEvent eventData)
    {
        if (eventData is LessonCompletedEvent lessonEvent)
        {
            var key = $"user_daily:{lessonEvent.UserId}:{DateTime.UtcNow:yyyy-MM-dd}";
            await _cache.HashIncrementAsync(key, "lessons_completed", 1);
            await _cache.HashIncrementAsync(key, "time_spent_minutes", (int)lessonEvent.Duration.TotalMinutes);
        }
    }

    private async Task UpdateRealTimeCounters(BaseEvent eventData)
    {
        // Update active users set
        if (!string.IsNullOrEmpty(eventData.UserId))
        {
            await _cache.SetAddAsync("metrics:realtime:active_users", eventData.UserId);
            await _cache.ExpireAsync("metrics:realtime:active_users", TimeSpan.FromMinutes(30));
        }

        // Update per-minute counters
        var minuteKey = $"metrics:realtime:events_per_minute:{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
        await _cache.IncrementAsync(minuteKey);
        await _cache.ExpireAsync(minuteKey, TimeSpan.FromHours(1));
    }
}
```

### Batch Processing Jobs
```csharp
public class MetricsBatchProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessHourlyAggregations();
                await ProcessDailyAggregations();
                await CleanupOldData();
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch processing");
            }
        }
    }

    private async Task ProcessHourlyAggregations()
    {
        var sql = @"
            INSERT INTO course_metrics_hourly
            SELECT 
                toStartOfHour(timestamp) as datetime,
                JSONExtractString(properties, 'courseId') as course_id,
                JSONExtractString(properties, 'lessonId') as lesson_id,
                countIf(event_type = 'LessonCompleted') / countIf(event_type = 'LessonStarted') as completion_rate,
                avgIf(JSONExtractFloat(properties, 'attemptsCount'), event_type = 'LessonCompleted') as average_attempts,
                avgIf(JSONExtractFloat(properties, 'duration'), event_type = 'LessonCompleted') / 60 as average_duration_minutes,
                countIf(event_type = 'TestPassed') / countIf(event_type = 'TestExecuted') as success_rate
            FROM events 
            WHERE timestamp >= now() - INTERVAL 2 HOUR 
              AND timestamp < now() - INTERVAL 1 HOUR
              AND event_type IN ('LessonStarted', 'LessonCompleted', 'TestExecuted', 'TestPassed')
            GROUP BY datetime, course_id, lesson_id";

        await _clickHouse.ExecuteAsync(sql);
    }
}
```

## Deployment and Infrastructure

### Docker Configuration
```dockerfile
# Dockerfile for Metrics Service
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AscendDev.Metrics.API/AscendDev.Metrics.API.csproj", "AscendDev.Metrics.API/"]
RUN dotnet restore "AscendDev.Metrics.API/AscendDev.Metrics.API.csproj"
COPY . .
WORKDIR "/src/AscendDev.Metrics.API"
RUN dotnet build "AscendDev.Metrics.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AscendDev.Metrics.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AscendDev.Metrics.API.dll"]
```

### Docker Compose Integration
```yaml
version: '3.8'
services:
  # Existing services...
  
  zookeeper:
    image: confluentinc/cp-zookeeper:latest
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000

  kafka:
    image: confluentinc/cp-kafka:latest
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1

  clickhouse:
    image: clickhouse/clickhouse-server:latest
    ports:
      - "8123:8123"
      - "9000:9000"
    environment:
      CLICKHOUSE_DB: metrics
      CLICKHOUSE_USER: admin
      CLICKHOUSE_PASSWORD: password
    volumes:
      - clickhouse_data:/var/lib/clickhouse

  metrics-service:
    build: ./AscendDev.Metrics.API
    ports:
      - "5002:80"
    environment:
      - ConnectionStrings__ClickHouse=Host=clickhouse;Port=9000;Database=metrics;Username=admin;Password=password
      - ConnectionStrings__Redis=redis:6379
      - Kafka__BootstrapServers=kafka:9092
      - Authentication__MainApiUrl=http://ascenddev-api
    depends_on:
      - kafka
      - clickhouse
      - redis
      - ascenddev-api

volumes:
  clickhouse_data:
```

## Performance and Scalability

### Scaling Strategies
- **Horizontal Scaling**: Multiple consumer instances with Kafka partitioning
- **Data Partitioning**: ClickHouse partitioning by time and event type
- **Caching**: Redis for frequently accessed metrics
- **Batch Processing**: Scheduled aggregation jobs for heavy computations

### Performance Targets
- **Event Processing**: <100ms per event
- **API Response Time**: <500ms for dashboard queries
- **Real-time Updates**: <1 second latency
- **Data Retention**: 2 years for detailed events, 5 years for aggregated data

## Implementation Timeline

### Phase 1: Core Infrastructure (Week 1-2)
- [ ] Set up Kafka cluster and topics
- [ ] Set up ClickHouse database
- [ ] Create basic event schema and publishing
- [ ] Implement basic event consumers

### Phase 2: Basic Analytics (Week 3-4)
- [ ] Implement core aggregation service
- [ ] Create basic analytics APIs
- [ ] Set up Redis caching
- [ ] Create dashboard metrics endpoints

### Phase 3: Advanced Features (Week 5-6)
- [ ] Implement real-time SignalR hub
- [ ] Add batch processing jobs
- [ ] Create reporting system
- [ ] Add advanced analytics queries

### Phase 4: Integration and Optimization (Week 7-8)
- [ ] Integrate with admin panel
- [ ] Performance optimization
- [ ] Monitoring and alerting
- [ ] Documentation and testing

## Security and Privacy

### Data Protection
- **Event Anonymization**: Remove PII from events where possible
- **Data Encryption**: Encrypt sensitive data in transit and at rest
- **Access Control**: Role-based access to analytics APIs
- **Data Retention**: Automatic cleanup of old detailed events

### Compliance Considerations
- **GDPR Compliance**: User data deletion capabilities
- **Data Minimization**: Only collect necessary metrics
- **Audit Logging**: Track access to analytics data
- **Privacy Controls**: User opt-out mechanisms

## Monitoring and Observability

### Metrics to Monitor
- **Event Processing Rate**: Events per second
- **Consumer Lag**: Kafka consumer lag
- **Query Performance**: ClickHouse query times
- **Cache Hit Rate**: Redis cache effectiveness
- **API Response Times**: Analytics API performance

### Alerting Rules
- Consumer lag > 1000 messages
- Event processing errors > 1%
- API response time > 2 seconds
- ClickHouse disk usage > 80%
- Redis memory usage > 90%

This comprehensive metrics service will provide powerful analytics capabilities for the AscendDev platform while maintaining high performance and scalability through event-driven architecture and specialized data storage solutions.