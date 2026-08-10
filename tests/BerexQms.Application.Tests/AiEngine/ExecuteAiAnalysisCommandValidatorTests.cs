using BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;

namespace BerexQms.Application.Tests.AiEngine;

public class ExecuteAiAnalysisCommandValidatorTests
{
    private readonly ExecuteAiAnalysisCommandValidator _validator = new();

    [Theory]
    [InlineData("DocumentAnalysis")]
    [InlineData("QualityAnalysis")]
    [InlineData("RecommendationGeneration")]
    [InlineData("Summarization")]
    [InlineData("RiskAnalysis")]
    [InlineData("CAPAAnalysis")]
    [InlineData("SupplierAnalysis")]
    [InlineData("AuditAnalysis")]
    [InlineData("DefectTrendAnalysis")]
    [InlineData("StructuredDataExtraction")]
    public void Validate_ValidTaskType_Passes(string taskType)
    {
        var command = new ExecuteAiAnalysisCommand(taskType, "Inspection", "Analyze this content.", null, 10);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidTask")]
    [InlineData("documentanalysis")]
    [InlineData("DOCUMENTANALYSIS")]
    public void Validate_InvalidTaskType_Fails(string taskType)
    {
        var command = new ExecuteAiAnalysisCommand(taskType, "Inspection", "Content", null, 10);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TaskType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    [InlineData(100)]
    public void Validate_InvalidMaxContextDocuments_Fails(int maxDocs)
    {
        var command = new ExecuteAiAnalysisCommand("DocumentAnalysis", "Inspection", "Content", null, maxDocs);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MaxContextDocuments");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void Validate_ValidMaxContextDocuments_Passes(int maxDocs)
    {
        var command = new ExecuteAiAnalysisCommand("DocumentAnalysis", "Inspection", "Content", null, maxDocs);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NullModule_Passes()
    {
        var command = new ExecuteAiAnalysisCommand("RiskAnalysis", null, "Content", null, 10);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NullEntityId_Passes()
    {
        var command = new ExecuteAiAnalysisCommand("RiskAnalysis", "Module", "Content", null, 10);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
