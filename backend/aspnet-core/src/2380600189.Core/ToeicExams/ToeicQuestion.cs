using Abp.Domain.Entities;
using Abp.MultiTenancy;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TOEICReading.ToeicExams;

public class ToeicQuestion : Entity<long>, IMayHaveTenant
{
    [Range(5, 7)]
    public int Part { get; set; }

    [Range(1, 300)]
    public int Number { get; set; }

    [Required]
    public string QuestionText { get; set; }

    public string Passage { get; set; }

    [Required]
    public string OptionA { get; set; }

    [Required]
    public string OptionB { get; set; }

    [Required]
    public string OptionC { get; set; }

    [Required]
    public string OptionD { get; set; }

    [StringLength(1)]
    public string CorrectAnswer { get; set; }

    public ICollection<ToeicQuestionImage> Images { get; set; } = new List<ToeicQuestionImage>();

    public long ToeicExamId { get; set; }

    public int? TenantId { get; set; }
}
