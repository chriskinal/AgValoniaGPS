using System;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for the first-run wizard that displays terms, welcome message, and initial setup.
/// Shown when the application is launched for the first time.
/// </summary>
public class FirstViewModel : DialogViewModelBase
{
    private bool _acceptedTerms;
    private bool _acceptedLicense;
    private int _currentPage;
    private const int TotalPages = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstViewModel"/> class.
    /// </summary>
    public FirstViewModel()
    {
        // Subscribe to property changes to validate
        this.WhenAnyValue(x => x.AcceptedTerms, x => x.AcceptedLicense)
            .Subscribe(_ => ValidateAcceptance());
    }

    /// <summary>
    /// Gets or sets whether the user has accepted the terms of service.
    /// </summary>
    public bool AcceptedTerms
    {
        get => _acceptedTerms;
        set => this.RaiseAndSetIfChanged(ref _acceptedTerms, value);
    }

    /// <summary>
    /// Gets or sets whether the user has accepted the license agreement.
    /// </summary>
    public bool AcceptedLicense
    {
        get => _acceptedLicense;
        set => this.RaiseAndSetIfChanged(ref _acceptedLicense, value);
    }

    /// <summary>
    /// Gets or sets the current page in the wizard (0-based).
    /// </summary>
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            this.RaisePropertyChanged(nameof(IsWelcomePage));
            this.RaisePropertyChanged(nameof(IsLicensePage));
            this.RaisePropertyChanged(nameof(IsTermsPage));
            this.RaisePropertyChanged(nameof(CanGoBack));
            this.RaisePropertyChanged(nameof(CanGoForward));
        }
    }

    /// <summary>
    /// Gets whether the current page is the welcome page.
    /// </summary>
    public bool IsWelcomePage => CurrentPage == 0;

    /// <summary>
    /// Gets whether the current page is the license page.
    /// </summary>
    public bool IsLicensePage => CurrentPage == 1;

    /// <summary>
    /// Gets whether the current page is the terms page.
    /// </summary>
    public bool IsTermsPage => CurrentPage == 2;

    /// <summary>
    /// Gets whether the user can go back to the previous page.
    /// </summary>
    public bool CanGoBack => CurrentPage > 0;

    /// <summary>
    /// Gets whether the user can go forward to the next page.
    /// </summary>
    public bool CanGoForward => CurrentPage < TotalPages - 1;

    /// <summary>
    /// Gets the welcome message.
    /// </summary>
    public string WelcomeMessage =>
        "Welcome to AgValoniaGPS!\n\n" +
        "AgValoniaGPS is a precision agriculture guidance software that helps farmers maintain accurate field operations with GPS guidance.\n\n" +
        "Key Features:\n" +
        "• GPS-based auto-steering\n" +
        "• AB line and curve guidance\n" +
        "• Automatic section control\n" +
        "• Field boundary management\n" +
        "• Real-time position tracking\n" +
        "• Cross-platform support (Windows, Linux, Android)\n\n" +
        "Before you begin, please review and accept the license agreement and terms of service.";

    /// <summary>
    /// Gets the license agreement text.
    /// </summary>
    public string LicenseText =>
        "GNU GENERAL PUBLIC LICENSE\n" +
        "Version 3, 29 June 2007\n\n" +
        "Copyright © 2025 AgValoniaGPS Contributors\n\n" +
        "This program is free software: you can redistribute it and/or modify " +
        "it under the terms of the GNU General Public License as published by " +
        "the Free Software Foundation, either version 3 of the License, or " +
        "(at your option) any later version.\n\n" +
        "This program is distributed in the hope that it will be useful, " +
        "but WITHOUT ANY WARRANTY; without even the implied warranty of " +
        "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the " +
        "GNU General Public License for more details.\n\n" +
        "You should have received a copy of the GNU General Public License " +
        "along with this program. If not, see <https://www.gnu.org/licenses/>.\n\n" +
        "This software is based on AgOpenGPS by Brian Tischler and the AgOpenGPS community.";

    /// <summary>
    /// Gets the terms of service text.
    /// </summary>
    public string TermsText =>
        "TERMS OF SERVICE\n\n" +
        "1. Acceptance of Terms\n" +
        "By using AgValoniaGPS, you agree to these terms of service.\n\n" +
        "2. Use of Software\n" +
        "This software is provided for agricultural guidance purposes. Users are responsible for:\n" +
        "• Proper installation and configuration\n" +
        "• Safe operation of equipment\n" +
        "• Compliance with local laws and regulations\n" +
        "• Maintaining appropriate insurance coverage\n\n" +
        "3. Disclaimer of Liability\n" +
        "The developers and contributors of AgValoniaGPS assume NO LIABILITY for:\n" +
        "• Equipment damage\n" +
        "• Crop damage or loss\n" +
        "• Property damage\n" +
        "• Personal injury\n" +
        "• Any other damages arising from use of this software\n\n" +
        "4. User Responsibility\n" +
        "Users must:\n" +
        "• Always maintain manual control of equipment\n" +
        "• Monitor GPS accuracy and system performance\n" +
        "• Use appropriate safety measures\n" +
        "• Not rely solely on automated guidance\n\n" +
        "5. Support\n" +
        "Community support is available through online forums and documentation. " +
        "No warranty or guaranteed support is provided.\n\n" +
        "6. Privacy\n" +
        "This software does not collect or transmit personal data without your explicit consent.";

    /// <summary>
    /// Goes to the next page in the wizard.
    /// </summary>
    public void NextPage()
    {
        if (CanGoForward)
        {
            CurrentPage++;
        }
    }

    /// <summary>
    /// Goes to the previous page in the wizard.
    /// </summary>
    public void PreviousPage()
    {
        if (CanGoBack)
        {
            CurrentPage--;
        }
    }

    /// <summary>
    /// Validates that the user has accepted all required terms.
    /// </summary>
    private void ValidateAcceptance()
    {
        if (AcceptedTerms && AcceptedLicense)
        {
            ClearError();
        }
    }

    /// <summary>
    /// Validates before closing the wizard.
    /// </summary>
    protected override bool OnOK()
    {
        if (!AcceptedTerms || !AcceptedLicense)
        {
            SetError("You must accept both the license agreement and terms of service to continue.");
            return false;
        }

        ClearError();
        return base.OnOK();
    }
}
