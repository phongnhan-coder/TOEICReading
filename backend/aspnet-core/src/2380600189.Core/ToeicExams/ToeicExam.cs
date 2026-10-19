using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.MultiTenancy;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TOEICReading.ToeicExams;

public class ToeicExam : CreationAuditedEntity<long>, IMayHaveTenant
{
    public const int MaxTitleLength = 200;
    public const int MaxCodeLength = 64;

    [Required]
    [StringLength(MaxTitleLength)]
    public string Title { get; set; }

    [Required]
    [StringLength(MaxCodeLength)]
    public string Code { get; set; }

    [Range(1, 300)]
    public int DurationMinutes { get; set; }

    public bool IsPublished { get; set; }

    [StringLength(260)]
    public string SourceFileName { get; set; }

    public int? TenantId { get; set; }

    public virtual ICollection<ToeicQuestion> Questions { get; set; } = new List<ToeicQuestion>();
}
