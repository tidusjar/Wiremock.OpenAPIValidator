using MediatR;
using System.Text.Json;
using System.Text.Json.Nodes;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Commands;

public class WiremockResponseReaderCommand : IRequest<WiremockResponseProperties>
{
    public string WiremockMappingPath { get; set; } = string.Empty;
    public string MockResponseFileName { get; set; } = string.Empty;
}

internal class WiremockResponseReaderCommandHandler : IRequestHandler<WiremockResponseReaderCommand, WiremockResponseProperties>
{
    public Task<WiremockResponseProperties> Handle(WiremockResponseReaderCommand request, CancellationToken cancellationToken)
    {
        var result = new WiremockResponseProperties();

        if (!Directory.Exists(request.WiremockMappingPath))
        {
            return Task.FromResult(result);
        }

        var parentWiremock = Directory.GetParent(request.WiremockMappingPath);

        if (parentWiremock == null)
        {
            return Task.FromResult(result);
        }
        using var responseStream = File.OpenRead(Path.Combine(parentWiremock.FullName, "__files", request.MockResponseFileName));
        var doc = JsonDocument.Parse(responseStream);
        if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            result.ObjectType = ObjectType.Object;
            TryAddProperty(result, doc.Deserialize<JsonObject>());
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var responseObjects = doc.Deserialize<JsonArray>();
            result.ObjectType = ObjectType.Array;
            if (responseObjects != null)
            {
                foreach (var item in responseObjects)
                {
                    TryAddProperty(result, item.Deserialize<JsonObject>());
                }
            }
        }

        return Task.FromResult(result);
    }

    private static void TryAddProperty(WiremockResponseProperties result, JsonObject? responseObjects)
    {
        if (responseObjects == null)
        {
            return;
        }
        foreach (var obj in responseObjects)
        {
            if (obj.Value == null)
            {
                // Currently do not support null values (we have no idea what the type should be from the mock)
                continue;
            }
            result.Properties.TryAdd(obj.Key, GetTypeFromValueKind(obj.Value.GetValue<JsonElement>().ValueKind));
        }
    }

    private static Type GetTypeFromValueKind(JsonValueKind kind) => kind switch
    {
        JsonValueKind.Undefined => throw new NotSupportedException(),
        JsonValueKind.Object => typeof(object),
        JsonValueKind.Array => typeof(Array),
        JsonValueKind.String => typeof(string),
        JsonValueKind.Number => typeof(int),
        JsonValueKind.True => typeof(bool),
        JsonValueKind.False => typeof(bool),
        JsonValueKind.Null => throw new NotSupportedException(),
        _ => throw new NotSupportedException(),
    };
}
