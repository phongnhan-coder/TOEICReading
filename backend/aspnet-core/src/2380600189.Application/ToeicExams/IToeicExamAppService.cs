using Abp.Application.Services;
using TOEICReading.ToeicExams.Dto;
using System.Threading.Tasks;

namespace TOEICReading.ToeicExams;

public interface IToeicExamAppService : IApplicationService
{
    Task<SavedToeicExamDto> SaveImportedDraftAsync(SaveImportedToeicExamRequest input);
    Task<SavedToeicExamDto> PublishAsync(long id);
    Task<ToeicExamDetailsDto> GetDraftAsync(long id);
}
