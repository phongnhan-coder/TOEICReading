using Abp.Zero.EntityFrameworkCore;
using TOEICReading.Authorization.Roles;
using TOEICReading.Authorization.Users;
using TOEICReading.MultiTenancy;
using TOEICReading.ToeicExams;
using Microsoft.EntityFrameworkCore;

namespace TOEICReading.EntityFrameworkCore;

public class TOEICReadingDbContext : AbpZeroDbContext<Tenant, Role, User, TOEICReadingDbContext>
{
    /* Define a DbSet for each entity of the application */
    public virtual DbSet<ToeicExam> ToeicExams { get; set; }
    public virtual DbSet<ToeicQuestion> ToeicQuestions { get; set; }
    public virtual DbSet<ToeicQuestionImage> ToeicQuestionImages { get; set; }

    public TOEICReadingDbContext(DbContextOptions<TOEICReadingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ToeicExam>(entity =>
        {
            entity.ToTable("ToeicExams");
            entity.HasIndex(exam => new { exam.TenantId, exam.Code }).IsUnique();
            entity.Property(exam => exam.Title).HasMaxLength(ToeicExam.MaxTitleLength).IsRequired();
            entity.Property(exam => exam.Code).HasMaxLength(ToeicExam.MaxCodeLength).IsRequired();
            entity.Property(exam => exam.SourceFileName).HasMaxLength(260);
            entity.HasMany(exam => exam.Questions)
                .WithOne()
                .HasForeignKey(question => question.ToeicExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ToeicQuestion>(entity =>
        {
            entity.ToTable("ToeicQuestions");
            entity.Property(question => question.QuestionText).IsRequired();
            entity.Property(question => question.OptionA).IsRequired();
            entity.Property(question => question.OptionB).IsRequired();
            entity.Property(question => question.OptionC).IsRequired();
            entity.Property(question => question.OptionD).IsRequired();
            entity.Property(question => question.CorrectAnswer).HasMaxLength(1);
            entity.HasIndex(question => new { question.TenantId, question.ToeicExamId, question.Part, question.Number })
                .IsUnique();
            entity.HasMany(question => question.Images)
                .WithOne()
                .HasForeignKey(image => image.ToeicQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ToeicQuestionImage>(entity =>
        {
            entity.ToTable("ToeicQuestionImages");
            entity.Property(image => image.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(image => image.Data).HasColumnType("varbinary(max)").IsRequired();
            entity.HasIndex(image => new { image.ToeicQuestionId, image.SortOrder }).IsUnique();
        });
    }
}
