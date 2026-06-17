using System.Diagnostics;
using BaseballApi.Observability;

namespace BaseballApiTests;

/// <summary>
/// Verifies the background-job <see cref="ActivitySource"/> contract that every job
/// call site relies on. Telemetry exports nothing locally without OTLP secrets, so
/// these tests attach an in-process <see cref="ActivityListener"/> instead — that is
/// enough to catch the real wiring failures (wrong source name, missing tags, error
/// status not recorded) without a collector or a live database.
/// </summary>
public class TelemetryTests
{
    /// <summary>
    /// Collects every activity emitted by the background-jobs source for the duration
    /// of the returned subscription, sampling all spans as the real exporter would.
    /// </summary>
    private static ActivityListener ListenToBackgroundJobs(List<Activity> recorded)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == Telemetry.BackgroundJobsSourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = recorded.Add,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    // Unique per call so a test can pick its own span out of the process-global listener,
    // which could also observe spans from other tests/services running in parallel.
    private static string UniqueSpanName() => $"job.test-{Guid.NewGuid():N}";

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void StartActivity_WithListener_EmitsNamedSpanWithTag()
    {
        var recorded = new List<Activity>();
        using var listener = ListenToBackgroundJobs(recorded);

        var spanName = UniqueSpanName();
        var importId = Guid.NewGuid();
        using (var activity = Telemetry.BackgroundJobs.StartActivity(spanName))
        {
            // A listener is attached, so the source must produce a live activity; if this
            // is null the source name is wrong or no listener matched.
            Assert.NotNull(activity);
            activity.SetTag("import.id", importId);
        }

        var span = Assert.Single(recorded, a => a.DisplayName == spanName);
        Assert.Equal(importId, Assert.IsType<Guid>(span.GetTagItem("import.id")));
        Assert.Equal(ActivityStatusCode.Unset, span.Status);
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordException_SetsErrorStatusAndExceptionEvent()
    {
        var recorded = new List<Activity>();
        using var listener = ListenToBackgroundJobs(recorded);

        var spanName = UniqueSpanName();
        using (var activity = Telemetry.BackgroundJobs.StartActivity(spanName))
        {
            Assert.NotNull(activity);
            try
            {
                throw new InvalidOperationException("boom");
            }
            catch (Exception ex)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddException(ex);
            }
        }

        var span = Assert.Single(recorded, a => a.DisplayName == spanName);
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal("boom", span.StatusDescription);
        var exceptionEvent = Assert.Single(span.Events);
        Assert.Equal("exception", exceptionEvent.Name);
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordJobException_MarksErrorForOrdinaryException()
    {
        var recorded = new List<Activity>();
        using var listener = ListenToBackgroundJobs(recorded);

        var spanName = UniqueSpanName();
        using (var activity = Telemetry.BackgroundJobs.StartActivity(spanName))
        {
            Telemetry.RecordJobException(activity, new InvalidOperationException("boom"));
        }

        var span = Assert.Single(recorded, a => a.DisplayName == spanName);
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal("boom", span.StatusDescription);
        Assert.Single(span.Events, e => e.Name == "exception");
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordJobException_IgnoresCancellation()
    {
        // Shutdown cancellation isn't a job failure: the span must stay unset so it
        // doesn't show up as an error in span-metrics.
        var recorded = new List<Activity>();
        using var listener = ListenToBackgroundJobs(recorded);

        var spanName = UniqueSpanName();
        using (var activity = Telemetry.BackgroundJobs.StartActivity(spanName))
        {
            Telemetry.RecordJobException(activity, new OperationCanceledException());
        }

        var span = Assert.Single(recorded, a => a.DisplayName == spanName);
        Assert.Equal(ActivityStatusCode.Unset, span.Status);
        Assert.Empty(span.Events);
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void RecordJobException_NullActivity_DoesNotThrow()
    {
        // No listener => StartActivity returns null; the helper must tolerate it.
        Telemetry.RecordJobException(null, new InvalidOperationException("boom"));
    }

    [Fact]
    [Trait(TestCategory.Category, TestCategory.Observability)]
    public void StartActivity_WithoutListener_ReturnsNull()
    {
        // No listener attached: StartActivity must return null, which is why every call
        // site null-conditions the activity rather than assuming a live span.
        using var activity = Telemetry.BackgroundJobs.StartActivity("job.media-import");
        Assert.Null(activity);
    }
}
