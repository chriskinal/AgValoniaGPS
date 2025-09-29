using ReactiveUI;
using System;

namespace AgOpenGPS.Avalonia.ViewModels
{
    /// <summary>
    /// Main view model for the AgOpenGPS Avalonia application.
    /// This serves as the root view model during Phase 1 of the migration.
    /// </summary>
    public class MainViewModel : ReactiveObject
    {
        private string _title = "AgOpenGPS - Avalonia (Phase 1)";
        private string _statusText = "OpenGL rendering initialized";

        /// <summary>
        /// Gets or sets the main window title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <summary>
        /// Gets or sets the status bar text.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public MainViewModel()
        {
            // Phase 1: Basic initialization
            // Future phases will integrate with AgOpenGPS.Core
        }

        /// <summary>
        /// Updates the status text with current information.
        /// </summary>
        /// <param name="status">The status message to display</param>
        public void UpdateStatus(string status)
        {
            StatusText = $"{DateTime.Now:HH:mm:ss} - {status}";
        }
    }
}