using System.Diagnostics.Metrics;

namespace BaseballApiTests;

/// <summary>
/// Minimal <see cref="IMeterFactory"/> for tests that construct metrics classes directly (the test
/// project doesn't pull in the ASP.NET hosting stack that registers a real one). Each created
/// <see cref="Meter"/> is tracked and disposed with the factory.
/// </summary>
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = [];

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }
        _meters.Clear();
    }
}
