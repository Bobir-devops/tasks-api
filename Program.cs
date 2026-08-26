using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var tasks = new ConcurrentDictionary<int, TaskItem>();
var nextId = 0;

tasks[Interlocked.Increment(ref nextId)] = new TaskItem(1, "Birinchi vazifa", false);
tasks[Interlocked.Increment(ref nextId)] = new TaskItem(2, "Ikkinchi vazifa", false);

app.MapGet("/", () => "Tasks API ishlayapti");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/tasks", () =>
    Results.Ok(tasks.Values.OrderBy(t => t.Id)));

app.MapGet("/tasks/{id:int}", (int id) =>
    tasks.TryGetValue(id, out var task)
        ? Results.Ok(task)
        : Results.NotFound(new { error = "Topilmadi" }));

app.MapPost("/tasks", (CreateTaskRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { error = "title majburiy" });

    var id = Interlocked.Increment(ref nextId);
    var task = new TaskItem(id, req.Title, false);
    tasks[id] = task;
    return Results.Created($"/tasks/{id}", task);
});

app.MapPut("/tasks/{id:int}", (int id, UpdateTaskRequest req) =>
{
    if (!tasks.TryGetValue(id, out var existing))
        return Results.NotFound(new { error = "Topilmadi" });

    var updated = existing with
    {
        Title = string.IsNullOrWhiteSpace(req.Title) ? existing.Title : req.Title,
        Done = req.Done ?? existing.Done
    };
    tasks[id] = updated;
    return Results.Ok(updated);
});

app.MapDelete("/tasks/{id:int}", (int id) =>
    tasks.TryRemove(id, out _)
        ? Results.NoContent()
        : Results.NotFound(new { error = "Topilmadi" }));

app.Run();

record TaskItem(int Id, string Title, bool Done);
record CreateTaskRequest(string? Title);
record UpdateTaskRequest(string? Title, bool? Done);
