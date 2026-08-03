using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public abstract class BaseEntity : ISoftDelete
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }
}
