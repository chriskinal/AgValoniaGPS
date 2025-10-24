using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for the Help documentation viewer.
/// Displays help topics in a tree view with content panel.
/// </summary>
public class HelpViewModel : DialogViewModelBase
{
    private HelpTopic? _selectedTopic;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpViewModel"/> class.
    /// </summary>
    public HelpViewModel()
    {
        HelpTopics = new ObservableCollection<HelpTopic>();
        LoadHelpTopics();

        // Subscribe to search text changes (handled via property setter)
    }

    /// <summary>
    /// Gets the collection of help topics.
    /// </summary>
    public ObservableCollection<HelpTopic> HelpTopics { get; }

    /// <summary>
    /// Gets or sets the currently selected help topic.
    /// </summary>
    public HelpTopic? SelectedTopic
    {
        get => _selectedTopic;
        set => SetProperty(ref _selectedTopic, value);
    }

    /// <summary>
    /// Gets or sets the search text for filtering topics.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                FilterTopics();
        }
    }

    /// <summary>
    /// Gets the content to display for the selected topic.
    /// </summary>
    public string TopicContent => SelectedTopic?.Content ?? "Select a topic to view help content.";

    /// <summary>
    /// Loads the help topics from resources or configuration.
    /// </summary>
    private void LoadHelpTopics()
    {
        // Add default help topics
        HelpTopics.Add(new HelpTopic
        {
            Title = "Getting Started",
            Content = "Welcome to AgValoniaGPS!\n\nThis precision agriculture guidance software helps you maintain accurate field operations with GPS guidance.\n\nKey Features:\n- GPS-based auto-steering\n- AB line guidance\n- Section control\n- Field mapping\n- Real-time position tracking"
        });

        HelpTopics.Add(new HelpTopic
        {
            Title = "GPS Setup",
            Content = "GPS Configuration:\n\n1. Connect your GPS receiver\n2. Select the correct COM port\n3. Choose the appropriate NMEA sentence format\n4. Verify GPS signal quality\n5. Configure antenna offset if needed"
        });

        HelpTopics.Add(new HelpTopic
        {
            Title = "Field Operations",
            Content = "Working in Fields:\n\n1. Create or load a field\n2. Set up your AB line or guidance pattern\n3. Enable auto-steering if available\n4. Configure section control\n5. Start working\n6. Save your progress regularly"
        });

        HelpTopics.Add(new HelpTopic
        {
            Title = "Guidance Lines",
            Content = "Guidance Line Types:\n\n- AB Line: Straight line guidance\n- Curve Line: Curved path guidance\n- Contour: Follow terrain contours\n- Headland: Define field boundaries\n- Tram Lines: Create tramline patterns"
        });

        HelpTopics.Add(new HelpTopic
        {
            Title = "Keyboard Shortcuts",
            Content = "Common Shortcuts:\n\nCtrl+N - New field\nCtrl+O - Open field\nCtrl+S - Save field\nF1 - Help\nF5 - Reset view\nEsc - Cancel operation\n\nSee the Keys dialog for a complete list."
        });

        HelpTopics.Add(new HelpTopic
        {
            Title = "Troubleshooting",
            Content = "Common Issues:\n\n- No GPS signal: Check antenna connection and placement\n- Poor accuracy: Verify DGPS/RTK corrections\n- Steering issues: Calibrate auto-steer settings\n- Connection lost: Check hardware connections\n- Performance: Reduce displayed data complexity"
        });
    }

    /// <summary>
    /// Filters topics based on search text.
    /// </summary>
    private void FilterTopics()
    {
        // This is a simplified implementation
        // In a real application, you'd filter the topics based on search text
        // For now, we just keep all topics visible
        OnPropertyChanged(nameof(HelpTopics));
    }
}

/// <summary>
/// Represents a help topic with title and content.
/// </summary>
public class HelpTopic : ObservableObject
{
    private string _title = string.Empty;
    private string _content = string.Empty;

    /// <summary>
    /// Gets or sets the topic title.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the topic content.
    /// </summary>
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
