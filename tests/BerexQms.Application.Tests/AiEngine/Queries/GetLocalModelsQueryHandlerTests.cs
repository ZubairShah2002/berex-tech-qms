using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.AiEngine.Queries.GetLocalModels;
using NSubstitute;

namespace BerexQms.Application.Tests.AiEngine.Queries;

public class GetLocalModelsQueryHandlerTests
{
    private readonly IAiOrchestrator _orchestrator;
    private readonly GetLocalModelsQueryHandler _handler;

    public GetLocalModelsQueryHandlerTests()
    {
        _orchestrator = Substitute.For<IAiOrchestrator>();
        _handler = new GetLocalModelsQueryHandler(_orchestrator);
    }

    [Fact]
    public async Task Handle_ReturnsModelsFromOrchestrator()
    {
        var models = new List<AiLocalModelDto>
        {
            new() { Name = "qwen3:latest", Available = true },
            new() { Name = "llama3.1:8b", Available = true },
        };
        _orchestrator.GetLocalModelsAsync(Arg.Any<CancellationToken>())
            .Returns(models);

        var result = await _handler.Handle(new GetLocalModelsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("qwen3:latest", result.Value[0].Name);
    }

    [Fact]
    public async Task Handle_NoModels_ReturnsEmptyList()
    {
        _orchestrator.GetLocalModelsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AiLocalModelDto>());

        var result = await _handler.Handle(new GetLocalModelsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_AlwaysReturnsSuccess()
    {
        _orchestrator.GetLocalModelsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AiLocalModelDto>());

        var result = await _handler.Handle(new GetLocalModelsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
