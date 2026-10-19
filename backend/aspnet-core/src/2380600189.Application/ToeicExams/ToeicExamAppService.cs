using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using TOEICReading.ToeicExams.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace TOEICReading.ToeicExams;

public class ToeicExamAppService : ApplicationService, IToeicExamAppService
{
    private readonly IRepository<ToeicExam, long> _examRepository;
    private readonly IAbpSession _abpSession;

    public ToeicExamAppService(
        IRepository<ToeicExam, long> examRepository,
        IAbpSession abpSession)
    {
        _examRepository = examRepository;
        _abpSession = abpSession;
    }

    public async Task<SavedToeicExamDto> SaveImportedDraftAsync(SaveImportedToeicExamRequest input)
    {
        var code = input.Code.Trim();
        if (await _examRepository.GetAll().AnyAsync(exam => exam.Code == code))
        {
            throw new UserFriendlyException($"Mã đề '{code}' đã tồn tại.");
        }

        var exam = new ToeicExam
        {
            Title = input.Title.Trim(),
            Code = code,
            DurationMinutes = input.DurationMinutes,
            SourceFileName = input.SourceFileName.Trim(),
            TenantId = _abpSession.TenantId,
            IsPublished = false,
            Questions = input.Questions.Select(question => new ToeicQuestion
            {
                TenantId = _abpSession.TenantId,
                Part = question.Part,
                Number = question.Number,
                QuestionText = question.QuestionText.Trim(),
                Passage = string.IsNullOrWhiteSpace(question.Passage) ? null : question.Passage.Trim(),
                OptionA = question.OptionA.Trim(),
                OptionB = question.OptionB.Trim(),
                OptionC = question.OptionC.Trim(),
                OptionD = question.OptionD.Trim(),
                CorrectAnswer = question.CorrectAnswer?.Trim().ToUpperInvariant(),
                Images = question.Images.Select((image, index) => new ToeicQuestionImage
                {
                    ContentType = image.ContentType,
                    Data = image.Data,
                    SortOrder = index
                }).ToList()
            }).ToList()
        };

        await _examRepository.InsertAsync(exam);
        await CurrentUnitOfWork.SaveChangesAsync();

        return new SavedToeicExamDto(
            exam.Id,
            exam.Title,
            exam.Code,
            exam.DurationMinutes,
            exam.Questions.Count,
            "Draft");
    }

    public async Task<SavedToeicExamDto> PublishAsync(long id)
    {
        var exam = await _examRepository.GetAll()
            .Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (exam is null)
        {
            throw new Abp.Domain.Entities.EntityNotFoundException(typeof(ToeicExam), id);
        }

        if (exam.Questions.Count == 0)
        {
            throw new UserFriendlyException("Không thể xuất bản đề thi chưa có câu hỏi.");
        }

        exam.IsPublished = true;
        await CurrentUnitOfWork.SaveChangesAsync();

        return new SavedToeicExamDto(
            exam.Id,
            exam.Title,
            exam.Code,
            exam.DurationMinutes,
            exam.Questions.Count,
            "Published");
    }

    public async Task<ToeicExamDetailsDto> GetDraftAsync(long id)
    {
        var exam = await _examRepository.GetAll()
            .Include(item => item.Questions)
                .ThenInclude(question => question.Images)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (exam is null)
        {
            throw new Abp.Domain.Entities.EntityNotFoundException(typeof(ToeicExam), id);
        }

        return new ToeicExamDetailsDto(
            exam.Id,
            exam.Title,
            exam.Code,
            exam.DurationMinutes,
            exam.SourceFileName,
            exam.IsPublished ? "Published" : "Draft",
            exam.Questions
                .OrderBy(question => question.Part)
                .ThenBy(question => question.Number)
                .Select(question => new SavedToeicQuestionDto(
                    question.Part,
                    question.Number,
                    question.QuestionText,
                    question.Passage,
                    question.OptionA,
                    question.OptionB,
                    question.OptionC,
                    question.OptionD,
                    question.CorrectAnswer,
                    question.Images
                        .OrderBy(image => image.SortOrder)
                        .Select(image => new ToeicQuestionImageDto(image.ContentType, image.Data))
                        .ToList()))
                .ToList());
    }
}
