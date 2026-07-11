namespace Application.Common;

public interface ICurrentUserAccessor
{
    Guid? CurrentUserId { get; }
}
