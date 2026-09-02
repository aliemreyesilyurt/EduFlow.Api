using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Constants;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduFlow.Infrastructure.Database.Seed;

/// <summary>
/// Seeds one demo tenant (TenantAdmin + Instructor + 2 Students, a published course with steps,
/// and a published exam with questions) so a fresh dev database has something to click through
/// beyond the bare SysAdmin account from <see cref="IdentitySeeder"/>. Only called for the
/// Development environment (see Program.cs) — this is throwaway demo data with a known password,
/// not something that belongs in a real tenant's database.
/// </summary>
public static class DemoDataSeeder
{
    private const string TenantSlug = "demo";
    private const string DemoPassword = "Demo12345.";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DemoDataSeeder));

        // IgnoreQueryFilters: Tenant.Slug has a DB-level unique index that isn't scoped by the
        // soft-delete filter, so the existence check has to see soft-deleted rows too, or a retry
        // after a soft-deleted "demo" tenant would crash on the unique constraint instead of no-op'ing.
        if (await dbContext.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == TenantSlug))
        {
            return;
        }

        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = "Demo Akademi",
            Slug = TenantSlug,
            IsActive = true,
            AllowSelfRegistration = true
        };

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var adminId = await CreateUserAsync(identityService, logger, "admin@demo.eduflow.local", "Tenant", "Yönetici", tenant.Id, Roles.TenantAdmin);
        var instructorId = await CreateUserAsync(identityService, logger, "egitmen@demo.eduflow.local", "Elif", "Kaya", tenant.Id, Roles.Instructor);
        var student1Id = await CreateUserAsync(identityService, logger, "ogrenci1@demo.eduflow.local", "Ayşe", "Yılmaz", tenant.Id, Roles.Student);
        var student2Id = await CreateUserAsync(identityService, logger, "ogrenci2@demo.eduflow.local", "Mehmet", "Demir", tenant.Id, Roles.Student);

        if (adminId is null || instructorId is null || student1Id is null || student2Id is null)
        {
            logger.LogError("Demo data seeding aborted: one or more demo users failed to be created");
            return;
        }

        var course = new Course
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenant.Id,
            Title = "Web Geliştirmeye Giriş",
            Description = "HTML, CSS ve JavaScript temellerini kapsayan giriş seviyesi bir kurs.",
            InstructorId = instructorId.Value,
            Status = CourseStatus.Published,
            PublishedOn = DateTime.UtcNow
        };

        dbContext.Courses.Add(course);

        var steps = new[]
        {
            new Step
            {
                Id = Guid.CreateVersion7(), TenantId = tenant.Id, CourseId = course.Id,
                Title = "Kursa Giriş", Order = 1, ContentType = StepContentType.Text,
                TextContent = "Bu kursta web geliştirmenin temellerini öğreneceksiniz: HTML ile yapı, CSS ile görünüm, JavaScript ile davranış."
            },
            new Step
            {
                Id = Guid.CreateVersion7(), TenantId = tenant.Id, CourseId = course.Id,
                Title = "HTML Temelleri", Order = 2, ContentType = StepContentType.Text,
                TextContent = "HTML (HyperText Markup Language), web sayfalarının iskeletini oluşturan işaretleme dilidir. İçerik, etiketler (tag) ile yapılandırılır."
            },
            new Step
            {
                Id = Guid.CreateVersion7(), TenantId = tenant.Id, CourseId = course.Id,
                Title = "CSS ile Stil Verme", Order = 3, ContentType = StepContentType.Text,
                TextContent = "CSS (Cascading Style Sheets), HTML ile oluşturulan yapının görünümünü (renk, boşluk, düzen) tanımlamak için kullanılır."
            }
        };

        dbContext.Steps.AddRange(steps);

        // Student 1 has finished the course (so the seeded exam is immediately takeable);
        // Student 2 is only enrolled, to show the in-progress state too.
        var enrollment1 = new Enrollment
        {
            Id = Guid.CreateVersion7(), TenantId = tenant.Id, CourseId = course.Id,
            StudentId = student1Id.Value, EnrolledOn = DateTime.UtcNow, CompletedOn = DateTime.UtcNow
        };
        var enrollment2 = new Enrollment
        {
            Id = Guid.CreateVersion7(), TenantId = tenant.Id, CourseId = course.Id,
            StudentId = student2Id.Value, EnrolledOn = DateTime.UtcNow, CompletedOn = null
        };

        dbContext.Enrollments.AddRange(enrollment1, enrollment2);

        dbContext.StepProgresses.AddRange(steps.Select(s => new StepProgress
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenant.Id,
            EnrollmentId = enrollment1.Id,
            StepId = s.Id,
            CompletedOn = DateTime.UtcNow
        }));

        var exam = new Exam
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenant.Id,
            CourseId = course.Id,
            Title = "Web Geliştirme Sınavı",
            PassScorePercentage = 60,
            TimeLimitMinutes = 30,
            MaxAttempts = 3,
            IsPublished = true,
            ProctoringEnabled = true,
            RequireCamera = true,
            SnapshotIntervalSeconds = 20,
            ViolationWarningThreshold = 3
        };

        dbContext.Exams.Add(exam);

        var questions = new (string Text, (string Text, bool IsCorrect)[] Options)[]
        {
            ("HTML ne anlama gelir?",
            [
                ("HyperText Markup Language", true),
                ("High Tech Modern Language", false),
                ("Home Tool Markup Language", false),
                ("Hyperlink and Text Markup Language", false)
            ]),
            ("CSS öncelikli olarak neyi kontrol eder?",
            [
                ("Sayfanın görünümünü ve düzenini", true),
                ("Sunucu tarafı iş mantığını", false),
                ("Veritabanı sorgularını", false),
                ("Ağ protokollerini", false)
            ]),
            ("JavaScript öncelikli olarak nerede çalışır?",
            [
                ("Tarayıcıda (client-side)", true),
                ("Yalnızca sunucuda", false),
                ("Yalnızca veritabanında", false),
                ("Yalnızca işletim sisteminde", false)
            ])
        };

        var questionOrder = 1;

        foreach (var (text, options) in questions)
        {
            var question = new Question
            {
                Id = Guid.CreateVersion7(), TenantId = tenant.Id, ExamId = exam.Id,
                Text = text, Order = questionOrder++, Points = 1
            };
            dbContext.Questions.Add(question);

            var optionOrder = 1;

            foreach (var (optionText, isCorrect) in options)
            {
                dbContext.QuestionOptions.Add(new QuestionOption
                {
                    Id = Guid.CreateVersion7(),
                    TenantId = tenant.Id,
                    QuestionId = question.Id,
                    Text = optionText,
                    IsCorrect = isCorrect,
                    Order = optionOrder++
                });
            }
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded demo tenant '{Slug}' — TenantAdmin/Instructor/2 Students, 1 published course ({StepCount} steps), 1 published exam ({QuestionCount} questions). Demo password for every seeded user: {Password}",
            TenantSlug, steps.Length, questions.Length, DemoPassword);
    }

    private static async Task<Guid?> CreateUserAsync(
        IIdentityService identityService,
        ILogger logger,
        string email,
        string firstName,
        string lastName,
        Guid tenantId,
        string role)
    {
        var result = await identityService.CreateUserAsync(
            new CreateUserRequest(
                email, DemoPassword, firstName, lastName, NationalId: null,
                TenantId: tenantId, Role: role, EmailConfirmed: true, MustChangePassword: false),
            CancellationToken.None);

        if (result.IsFailure)
        {
            logger.LogError("Failed to seed demo user {Email}: {Error}", email, result.Error.Description);
            return null;
        }

        return result.Value;
    }
}
