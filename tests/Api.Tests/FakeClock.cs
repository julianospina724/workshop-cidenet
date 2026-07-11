using Application.Common;

namespace Api.Tests;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
