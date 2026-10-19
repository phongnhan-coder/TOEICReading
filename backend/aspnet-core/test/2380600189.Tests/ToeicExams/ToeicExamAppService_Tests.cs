using Abp.Domain.Entities;
using TOEICReading.ToeicExams;
using TOEICReading.ToeicExams.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TOEICReading.Tests.ToeicExams;

public class ToeicExamAppService_Tests : TOEICReadingTestBase
{
    private readonly IToeicExamAppService _examAppService;

    public ToeicExamAppService_Tests()
    {
        _examAppService = Resolve<IToeicExamAppService>();
    }

    [Fact]
    public async Task PublishAsync_ShouldPersistPublishedStatus()
    {
        var draft = await _examAppService.SaveImportedDraftAsync(new SaveImportedToeicExamRequest
        {
            Title = "Publish test",
            Code = "PUB-" + Guid.NewGuid().ToString("N"),
            SourceFileName = "publish-test.docx",
            Questions = new List<SaveImportedToeicQuestionRequest>
            {
                new()
                {
                    Part = 5,
                    Number = 1,
                    QuestionText = "Choose the correct answer.",
                    OptionA = "A",
                    OptionB = "B",
                    OptionC = "C",
                    OptionD = "D",
                    CorrectAnswer = "A"
                }
            }
        });

        var published = await _examAppService.PublishAsync(draft.Id);

        published.Status.ShouldBe("Published");
        published.QuestionCount.ShouldBe(1);
        await UsingDbContextAsync(async context =>
        {
            var exam = await context.ToeicExams.SingleAsync(item => item.Id == draft.Id);
            exam.IsPublished.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task PublishAsync_ShouldFailForUnknownExam()
    {
        await Should.ThrowAsync<EntityNotFoundException>(
            () => _examAppService.PublishAsync(long.MaxValue));
    }
}
