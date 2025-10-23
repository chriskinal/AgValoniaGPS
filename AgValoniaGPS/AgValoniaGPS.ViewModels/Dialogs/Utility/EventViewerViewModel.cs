using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for event log viewer with filtering and scrolling.
/// Displays system events with timestamp, level, source, and message.
/// </summary>
public class EventViewerViewModel : DialogViewModelBase
{
    private EventLevel _filterLevel = EventLevel.All;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventViewerViewModel"/> class.
    /// </summary>
    public EventViewerViewModel()
    {
        Events = new ObservableCollection<EventLogEntry>();
        FilteredEvents = new ObservableCollection<EventLogEntry>();

        ClearCommand = ReactiveCommand.Create(OnClear);
        RefreshCommand = ReactiveCommand.Create(OnRefresh);

        // Subscribe to filter changes
        this.WhenAnyValue(x => x.FilterLevel, x => x.SearchText)
            .Subscribe(_ => ApplyFilters());

        // Load initial events
        LoadEvents();
    }

    /// <summary>
    /// Gets the collection of all events.
    /// </summary>
    public ObservableCollection<EventLogEntry> Events { get; }

    /// <summary>
    /// Gets the collection of filtered events.
    /// </summary>
    public ObservableCollection<EventLogEntry> FilteredEvents { get; }

    /// <summary>
    /// Gets or sets the event level filter.
    /// </summary>
    public EventLevel FilterLevel
    {
        get => _filterLevel;
        set => this.RaiseAndSetIfChanged(ref _filterLevel, value);
    }

    /// <summary>
    /// Gets or sets the search text for filtering events.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    /// <summary>
    /// Gets the command to clear all events.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

    /// <summary>
    /// Gets the command to refresh the event list.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// Gets the total event count.
    /// </summary>
    public int TotalEventCount => Events.Count;

    /// <summary>
    /// Gets the filtered event count.
    /// </summary>
    public int FilteredEventCount => FilteredEvents.Count;

    /// <summary>
    /// Loads events from the event log service.
    /// </summary>
    private void LoadEvents()
    {
        // TODO: In a real implementation, inject IEventLogService and load from there
        // For now, add some sample events for demonstration
        AddSampleEvents();
        ApplyFilters();
    }

    /// <summary>
    /// Adds sample events for testing.
    /// </summary>
    private void AddSampleEvents()
    {
        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-30),
            Level = EventLevel.Info,
            Source = "GPS",
            Message = "GPS connected successfully"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-25),
            Level = EventLevel.Info,
            Source = "Field",
            Message = "Field 'North40' loaded"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-20),
            Level = EventLevel.Warning,
            Source = "GPS",
            Message = "GPS signal quality degraded (HDOP: 2.5)"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-15),
            Level = EventLevel.Info,
            Source = "Guidance",
            Message = "AB line created"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-10),
            Level = EventLevel.Info,
            Source = "AutoSteer",
            Message = "Auto-steer enabled"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-5),
            Level = EventLevel.Error,
            Source = "Section",
            Message = "Section 3 communication timeout"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now.AddMinutes(-2),
            Level = EventLevel.Warning,
            Source = "Section",
            Message = "Section 3 reconnected"
        });

        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now,
            Level = EventLevel.Info,
            Source = "System",
            Message = "Event viewer opened"
        });
    }

    /// <summary>
    /// Applies the current filters to the event list.
    /// </summary>
    private void ApplyFilters()
    {
        FilteredEvents.Clear();

        var filtered = Events.AsEnumerable();

        // Filter by level
        if (FilterLevel != EventLevel.All)
        {
            filtered = filtered.Where(e => e.Level == FilterLevel);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.Source.ToLowerInvariant().Contains(searchLower) ||
                e.Message.ToLowerInvariant().Contains(searchLower));
        }

        foreach (var evt in filtered.OrderByDescending(e => e.Timestamp))
        {
            FilteredEvents.Add(evt);
        }

        this.RaisePropertyChanged(nameof(TotalEventCount));
        this.RaisePropertyChanged(nameof(FilteredEventCount));
    }

    /// <summary>
    /// Clears all events from the log.
    /// </summary>
    private void OnClear()
    {
        Events.Clear();
        ApplyFilters();
    }

    /// <summary>
    /// Refreshes the event list.
    /// </summary>
    private void OnRefresh()
    {
        LoadEvents();
    }

    /// <summary>
    /// Adds a new event to the log.
    /// </summary>
    /// <param name="level">The event level.</param>
    /// <param name="source">The event source.</param>
    /// <param name="message">The event message.</param>
    public void AddEvent(EventLevel level, string source, string message)
    {
        Events.Add(new EventLogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Source = source,
            Message = message
        });

        ApplyFilters();
    }
}

/// <summary>
/// Represents an entry in the event log.
/// </summary>
public class EventLogEntry : ReactiveObject
{
    private DateTime _timestamp;
    private EventLevel _level;
    private string _source = string.Empty;
    private string _message = string.Empty;

    /// <summary>
    /// Gets or sets the event timestamp.
    /// </summary>
    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            this.RaiseAndSetIfChanged(ref _timestamp, value);
            this.RaisePropertyChanged(nameof(FormattedTimestamp));
        }
    }

    /// <summary>
    /// Gets the formatted timestamp string.
    /// </summary>
    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Gets or sets the event level.
    /// </summary>
    public EventLevel Level
    {
        get => _level;
        set => this.RaiseAndSetIfChanged(ref _level, value);
    }

    /// <summary>
    /// Gets or sets the event source.
    /// </summary>
    public string Source
    {
        get => _source;
        set => this.RaiseAndSetIfChanged(ref _source, value);
    }

    /// <summary>
    /// Gets or sets the event message.
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }
}

/// <summary>
/// Represents the severity level of an event.
/// </summary>
public enum EventLevel
{
    /// <summary>
    /// Show all events.
    /// </summary>
    All,

    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error
}
