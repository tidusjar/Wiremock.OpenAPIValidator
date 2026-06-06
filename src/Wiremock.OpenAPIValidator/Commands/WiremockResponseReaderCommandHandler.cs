using System.Text.Json;
using System.Text.Json.Nodes;
using Wiremock.OpenAPIValidator.Models;

namespace Wiremock.OpenAPIValidator.Commands;

public class WiremockResponseReaderCommand
{
    public string WiremockMappingPath { get; set; } = string.Empty;
    public WiremockResponse? WiremockResponse { get; set; }
}

public class WiremockResponseReaderCommandHandler
{
    public Task<WiremockResponseProperties> Handle(WiremockResponseReaderCommand request, CancellationToken cancellationToken)
    {
        var result = new WiremockResponseProperties();

        if (request.WiremockResponse == null)
        {
            return Task.FromResult(result);
        }

        var responseBody = ReadResponseBody(request);

        if (responseBody is null)
        {
            return Task.FromResult(result);
        }

        switch (responseBody.GetValueKind())
        {
            case JsonValueKind.Object:
                result.ObjectType = ObjectType.Object;
                TryAddProperty(result, responseBody.AsObject());
                break;
            case JsonValueKind.Array:
                result.ObjectType = ObjectType.Array;
                foreach (var item in responseBody.AsArray())
                {
                    TryAddProperty(result, item?.AsObject());
                }
                break;
        }

        return Task.FromResult(result);
    }

    private static JsonNode? ReadResponseBody(WiremockResponseReaderCommand request)
    {
        var response = request.WiremockResponse!;

        if (response.JsonBody is not null)
        {
            return response.JsonBody;
        }

        if (string.IsNullOrEmpty(response.FileName) || !Directory.Exists(request.WiremockMappingPath))
        {
            return null;
        }

        var parentWiremock = Directory.GetParent(request.WiremockMappingPath);

        if (parentWiremock == null)
        {
            return null;
        }

        var responseFilePath = Path.Combine(parentWiremock.FullName, "__files", response.FileName);

        if (!File.Exists(responseFilePath))
        {
            return null;
        }

        using var responseStream = File.OpenRead(responseFilePath);
        return JsonNode.Parse(responseStream);
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
