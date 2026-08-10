using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.AiEngine;

public class AiPromptTemplateTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    private static AiPromptTemplate CreateTestTemplate(
        string taskType = "DocumentAnalysis",
        string name = "Default Document Analysis",
        int version = 1)
    {
        return AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            taskType, name,
            "Analyzes document content for quality compliance.",
            "You are a quality management AI assistant.",
            "Analyze the following document: {{content}}",
            "{\"type\":\"object\"}",
            version);
    }

    [Fact]
    public void Create_ValidParameters_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var template = AiPromptTemplate.Create(
            id, TestTenantId,
            "RiskAnalysis", "Risk Assessment Prompt",
            "Template for risk analysis tasks.",
            "You are a risk analysis expert.",
            "Analyze the following risk data: {{content}}",
            "{\"type\":\"array\"}",
            2);

        Assert.Equal(id, template.Id);
        Assert.Equal(TestTenantId, template.TenantId);
        Assert.Equal("RiskAnalysis", template.TaskType);
        Assert.Equal("Risk Assessment Prompt", template.Name);
        Assert.Equal("Template for risk analysis tasks.", template.Description);
        Assert.Equal("You are a risk analysis expert.", template.SystemPrompt);
        Assert.Equal("Analyze the following risk data: {{content}}", template.UserPromptTemplate);
        Assert.Equal("{\"type\":\"array\"}", template.OutputSchema);
        Assert.True(template.IsActive);
        Assert.Equal(2, template.Version);
    }

    [Fact]
    public void Create_DefaultVersion_IsOne()
    {
        var template = AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "Summarization", "Summary",
            null,
            "System prompt.",
            "User prompt.", null);

        Assert.Equal(1, template.Version);
    }

    [Fact]
    public void Create_NewTemplate_IsActiveByDefault()
    {
        var template = CreateTestTemplate();

        Assert.True(template.IsActive);
    }

    [Fact]
    public void Create_NullDescription_Allowed()
    {
        var template = AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "Summarization", "Summary", null,
            "System prompt.", "User prompt.", null);

        Assert.Null(template.Description);
    }

    [Fact]
    public void Create_NullOutputSchema_Allowed()
    {
        var template = AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "Summarization", "Summary", null,
            "System prompt.", "User prompt.", null);

        Assert.Null(template.OutputSchema);
    }

    [Fact]
    public void Create_EmptyTaskType_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "", "Name", null,
            "System prompt.", "User prompt.", null));
    }

    [Fact]
    public void Create_WhitespaceTaskType_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "   ", "Name", null,
            "System prompt.", "User prompt.", null));
    }

    [Fact]
    public void Create_EmptyName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "TaskType", "", null,
            "System prompt.", "User prompt.", null));
    }

    [Fact]
    public void Create_EmptySystemPrompt_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "TaskType", "Name", null,
            "", "User prompt.", null));
    }

    [Fact]
    public void Create_EmptyUserPromptTemplate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => AiPromptTemplate.Create(
            Guid.NewGuid(), TestTenantId,
            "TaskType", "Name", null,
            "System prompt.", "", null));
    }

    [Fact]
    public void Deactivate_ActiveTemplate_SetsIsActiveFalse()
    {
        var template = CreateTestTemplate();
        Assert.True(template.IsActive);

        template.Deactivate();

        Assert.False(template.IsActive);
    }

    [Fact]
    public void Activate_InactiveTemplate_SetsIsActiveTrue()
    {
        var template = CreateTestTemplate();
        template.Deactivate();
        Assert.False(template.IsActive);

        template.Activate();

        Assert.True(template.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActiveTemplate_RemainsActive()
    {
        var template = CreateTestTemplate();

        template.Activate();

        Assert.True(template.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveTemplate_RemainsInactive()
    {
        var template = CreateTestTemplate();
        template.Deactivate();

        template.Deactivate();

        Assert.False(template.IsActive);
    }
}
