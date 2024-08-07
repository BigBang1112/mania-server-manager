using System.Text.Json.Serialization;

namespace ManiaServerManager;

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

