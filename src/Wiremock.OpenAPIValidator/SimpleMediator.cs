using System.Collections.Concurrent;

namespace Wiremock.OpenAPIValidator;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default);
}

public class SimpleMediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> _handlerCache = new();

    public SimpleMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = GetHandlerType(requestType, typeof(TResponse));

        if (handlerType == null)
        {
            throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
        }

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"Handler {handlerType.Name} not registered in service provider");
        }

        var handleMethod = handlerType.GetMethod("Handle");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handler {handlerType.Name} does not have a Handle method");
        }

        var result = handleMethod.Invoke(handler, new[] { request, cancellationToken });

        if (result is Task<TResponse> taskResult)
        {
            return await taskResult;
        }

        throw new InvalidOperationException($"Handler returned unexpected type");
    }

    private static Type? GetHandlerType(Type requestType, Type responseType)
    {
        var cacheKey = BuildCacheKey(requestType, responseType);

        return _handlerCache.GetOrAdd(cacheKey, _ =>
        {
            var assembly = requestType.Assembly;
            var requestName = requestType.Name;

            // Try convention: RequestName + Handler
            var handlerTypeName = requestName + "Handler";
            var handlerType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == handlerTypeName && t.IsClass && !t.IsAbstract);

            // If not found, try removing "Command" or "Query" suffix from request name
            if (handlerType == null)
            {
                if (requestName.EndsWith("Command"))
                {
                    handlerTypeName = requestName.Substring(0, requestName.Length - "Command".Length) + "Handler";
                }
                else if (requestName.EndsWith("Query"))
                {
                    handlerTypeName = requestName.Substring(0, requestName.Length - "Query".Length) + "Handler";
                }

                handlerType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == handlerTypeName && t.IsClass && !t.IsAbstract);
            }

            if (handlerType == null)
            {
                throw new InvalidOperationException($"No handler found with name {requestType.Name}Handler or matching convention");
            }

            return handlerType;
        });
    }

    private static Type BuildCacheKey(Type requestType, Type responseType)
    {
        // Use request type as cache key since we find handler by convention
        return requestType;
    }
}
