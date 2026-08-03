namespace EduFlow.Domain.Abstractions;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOn { get; set; }
}
