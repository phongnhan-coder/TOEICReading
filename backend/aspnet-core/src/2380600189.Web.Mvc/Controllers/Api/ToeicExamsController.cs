using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using TOEICReading.Controllers;
using TOEICReading.ToeicExams;
using TOEICReading.ToeicExams.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TOEICReading.Web.Controllers.Api;

[ApiController]
[AbpMvcAuthorize]
[Route("api/toeic-exams")]
public class ToeicExamsController : TOEICReadingControllerBase
{
    private readonly IDocxToeicExamParser _parser;
    private readonly IToeicExamAppService _examAppService;
    private readonly ILogger<ToeicExamsController> _logger;
    private static readonly HashSet<string> SupportedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp", "image/tiff"
    };

    private const long MaximumSaveRequestBytes = 80 * 1024 * 1024;

    public ToeicExamsController(
        IDocxToeicExamParser parser,
        IToeicExamAppService examAppService,
        ILogger<ToeicExamsController> logger)
    {
        _parser = parser;
        _examAppService = examAppService;
        _logger = logger;
    }

    /// <summary>Uploads and parses a Word document without saving an exam.</summary>
    [HttpPost("import-preview")]
    [RequestSizeLimit(DocxUploadValidator.MaximumUploadBytes + 64 * 1024)]
    [ProducesResponseType(typeof(ToeicExamImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ToeicExamImportPreviewDto>> ImportPreview(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return Problem("Vui lòng chọn một tệp Word .docx.", statusCode: StatusCodes.Status400BadRequest);
        }

        var fileName = Path.GetFileName(file.FileName);
        await using var stream = file.OpenReadStream();
        var validationError = await DocxUploadValidator.ValidateAsync(stream, fileName, file.Length, cancellationToken);
        if (validationError is not null)
        {
            var statusCode = file.Length > DocxUploadValidator.MaximumUploadBytes
                ? StatusCodes.Status413PayloadTooLarge
                : StatusCodes.Status400BadRequest;
            return Problem(validationError, statusCode: statusCode);
        }

        try
        {
            var preview = _parser.Parse(stream, fileName);
            return Ok(preview);
        }
        catch (DocxImportException exception)
        {
            _logger.LogWarning(
                "DOCX preview rejected by parser validation. Error type: {ErrorType}.",
                exception.GetType().Name);
            return Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error parsing uploaded TOEIC DOCX file {FileName}.", fileName);
            return CreateLoggedServerProblem();
        }
    }

    /// <summary>Saves a user-reviewed Word import as an unpublished exam draft.</summary>
    [HttpPost]
    [RequestSizeLimit(MaximumSaveRequestBytes)]
    [ProducesResponseType(typeof(SavedToeicExamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SavedToeicExamDto>> SaveDraft(
        [FromBody] SaveImportedToeicExamRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request?.Questions is null ||
            request.Questions.Count == 0 ||
            request.Questions.Any(question => question is null))
        {
            return Problem("Đề thi cần có ít nhất một câu hỏi.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.Questions
            .Select(question => new { question.Part, question.Number })
            .Distinct()
            .Count() != request.Questions.Count)
        {
            return Problem("Số câu trong cùng một Part không được trùng lặp.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (!string.Equals(Path.GetFileName(request.SourceFileName), request.SourceFileName, StringComparison.Ordinal))
        {
            return Problem("Tên tệp nguồn không hợp lệ.", statusCode: StatusCodes.Status400BadRequest);
        }

        var totalImageBytes = 0L;
        foreach (var image in request.Questions.SelectMany(question => question.Images ?? Array.Empty<ToeicQuestionImageDto>()))
        {
            if (image?.Data is null ||
                !SupportedImageContentTypes.Contains(image.ContentType ?? string.Empty) ||
                !ToeicQuestionImageValidator.IsValid(image.ContentType, image.Data))
            {
                return Problem("Tệp ảnh trong câu hỏi không hợp lệ hoặc không được hỗ trợ.", statusCode: StatusCodes.Status400BadRequest);
            }

            totalImageBytes += image.Data.LongLength;
            if (totalImageBytes > DocxUploadValidator.MaximumExpandedBytes)
            {
                return Problem("Tổng dung lượng hình ảnh vượt giới hạn xử lý 50 MB.", statusCode: StatusCodes.Status400BadRequest);
            }
        }

        try
        {
            var saved = await _examAppService.SaveImportedDraftAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = saved.Id }, saved);
        }
        catch (UserFriendlyException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error saving TOEIC exam draft with code {ExamCode}.", request.Code);
            return CreateLoggedServerProblem();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error saving TOEIC exam draft with code {ExamCode}.", request.Code);
            return CreateLoggedServerProblem();
        }
    }

    /// <summary>Publishes a saved TOEIC exam draft.</summary>
    [HttpPost("{id:long}/publish")]
    [ProducesResponseType(typeof(SavedToeicExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedToeicExamDto>> Publish(
        long id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Ok(await _examAppService.PublishAsync(id));
        }
        catch (Abp.Domain.Entities.EntityNotFoundException)
        {
            return Problem("Không tìm thấy đề thi.", statusCode: StatusCodes.Status404NotFound);
        }
        catch (UserFriendlyException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database error publishing TOEIC exam {ExamId}.", id);
            return CreateLoggedServerProblem();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error publishing TOEIC exam {ExamId}.", id);
            return CreateLoggedServerProblem();
        }
    }

    /// <summary>Gets a saved exam draft and its persisted questions.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ToeicExamDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToeicExamDetailsDto>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return Ok(await _examAppService.GetDraftAsync(id));
        }
        catch (Abp.Domain.Entities.EntityNotFoundException)
        {
            return Problem("Không tìm thấy đề thi.", statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database error reading TOEIC exam draft {ExamId}.", id);
            return CreateLoggedServerProblem();
        }
    }

    private ObjectResult CreateLoggedServerProblem()
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Không thể xử lý đề thi do lỗi phía máy chủ.",
            Detail = $"Mã tham chiếu: {HttpContext.TraceIdentifier}"
        };
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

}
