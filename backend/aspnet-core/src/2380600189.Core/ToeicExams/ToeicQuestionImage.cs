using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace TOEICReading.ToeicExams;

public class ToeicQuestionImage : Entity<long>
{
    [Required, StringLength(100)]
    public string ContentType { get; set; }

    [Required]
    public byte[] Data { get; set; }

    public int SortOrder { get; set; }

    public long ToeicQuestionId { get; set; }
}
