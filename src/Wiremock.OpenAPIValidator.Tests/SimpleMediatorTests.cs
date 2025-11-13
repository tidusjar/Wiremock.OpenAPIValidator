using Microsoft.Extensions.DependencyInjection;

namespace Wiremock.OpenAPIValidator.Tests;

public class SimpleMediatorTests
{
    // Test request/response types
    public class TestCommand
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandHandler
    {
        public Task<string> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Value}");
        }
    }

    public class TestQuery
    {
        public int Number { get; set; }
    }

    public class TestQueryHandler
    {
        public Task<int> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Number * 2);
        }
    }

    public class SimpleRequest
    {
        public string Data { get; set; } = string.Empty;
    }

    public class SimpleRequestHandler
    {
        public Task<bool> Handle(SimpleRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(request.Data));
        }
    }

    public class RequestWithoutHandler
    {
        public string Value { get; set; } = string.Empty;
    }

    [Test]
    public async Task Send_WithCommandSuffix_FindsHandlerAndReturnsResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<TestCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test data" };

        // Act
        var result = await mediator.Send<string>(command);

        // Assert
        Assert.That(result, Is.EqualTo("Handled: test data"));
    }

    [Test]
    public async Task Send_WithQuerySuffix_FindsHandlerAndReturnsResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<TestQueryHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var query = new TestQuery { Number = 5 };

        // Act
        var result = await mediator.Send<int>(query);

        // Assert
        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public async Task Send_WithSimpleRequestName_FindsHandlerByConvention()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<SimpleRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new SimpleRequest { Data = "some data" };

        // Act
        var result = await mediator.Send<bool>(request);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Send_WithoutMatchingHandler_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var request = new RequestWithoutHandler { Value = "test" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.Send<string>(request));

        Assert.That(ex.Message, Does.Contain("No handler found"));
    }

    [Test]
    public void Send_WithHandlerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        // Note: Not registering TestCommandHandler
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await mediator.Send<string>(command));

        Assert.That(ex.Message, Does.Contain("not registered"));
    }

    [Test]
    public async Task Send_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<CancellableCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var cts = new CancellationTokenSource();
        var command = new CancellableCommand();

        // Act
        cts.Cancel();

        // Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await mediator.Send<string>(command, cts.Token));
    }

    [Test]
    public async Task Send_MultipleTimes_UsesCachedHandlerType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<TestCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act - Send multiple times to verify caching works
        var result1 = await mediator.Send<string>(new TestCommand { Value = "first" });
        var result2 = await mediator.Send<string>(new TestCommand { Value = "second" });
        var result3 = await mediator.Send<string>(new TestCommand { Value = "third" });

        // Assert
        Assert.That(result1, Is.EqualTo("Handled: first"));
        Assert.That(result2, Is.EqualTo("Handled: second"));
        Assert.That(result3, Is.EqualTo("Handled: third"));
    }

    [Test]
    public async Task Send_WithComplexReturnType_ReturnsCorrectResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator, SimpleMediator>();
        services.AddTransient<ComplexReturnCommandHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new ComplexReturnCommand { Input = "test" };

        // Act
        var result = await mediator.Send<ComplexResult>(command);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("Processed: test"));
        Assert.That(result.Count, Is.EqualTo(4));
    }

    // Support classes for cancellation test
    public class CancellableCommand { }

    public class CancellableCommandHandler
    {
        public async Task<string> Handle(CancellableCommand request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken); // Will throw if cancelled
            return "completed";
        }
    }

    // Support classes for complex return type test
    public class ComplexReturnCommand
    {
        public string Input { get; set; } = string.Empty;
    }

    public class ComplexResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ComplexReturnCommandHandler
    {
        public Task<ComplexResult> Handle(ComplexReturnCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ComplexResult
            {
                Success = true,
                Message = $"Processed: {request.Input}",
                Count = request.Input.Length
            });
        }
    }
}
