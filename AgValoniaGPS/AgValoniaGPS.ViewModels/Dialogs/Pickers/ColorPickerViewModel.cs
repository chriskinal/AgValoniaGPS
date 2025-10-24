using System;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using Avalonia.Media;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Pickers;

/// <summary>
/// ViewModel for color picker dialog that allows selecting colors via palette, RGB sliders, or hex input.
/// Provides two-way binding between different color representations.
/// </summary>
public class ColorPickerViewModel : DialogViewModelBase
{
    private Color _selectedColor = Colors.White;
    private byte _red = 255;
    private byte _green = 255;
    private byte _blue = 255;
    private string _hexValue = "#FFFFFF";
    private bool _isUpdatingFromColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPickerViewModel"/> class.
    /// </summary>
    public ColorPickerViewModel()
    {
        SelectColorCommand = ReactiveCommand.Create<Color>(SelectColor);

        // Subscribe to RGB changes to update color
        this.WhenAnyValue(x => x.Red, x => x.Green, x => x.Blue)
            .Subscribe(_ => UpdateColorFromRGB());

        // Subscribe to hex input changes
        this.WhenAnyValue(x => x.HexValue)
            .Subscribe(_ => UpdateColorFromHex());
    }

    /// <summary>
    /// Initializes a new instance with a starting color.
    /// </summary>
    /// <param name="initialColor">The initial color to display.</param>
    public ColorPickerViewModel(Color initialColor) : this()
    {
        SelectedColor = initialColor;
    }

    /// <summary>
    /// Gets or sets the currently selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedColor, value);
            UpdateFromColor();
        }
    }

    /// <summary>
    /// Gets or sets the red component (0-255).
    /// </summary>
    public byte Red
    {
        get => _red;
        set => this.RaiseAndSetIfChanged(ref _red, value);
    }

    /// <summary>
    /// Gets or sets the green component (0-255).
    /// </summary>
    public byte Green
    {
        get => _green;
        set => this.RaiseAndSetIfChanged(ref _green, value);
    }

    /// <summary>
    /// Gets or sets the blue component (0-255).
    /// </summary>
    public byte Blue
    {
        get => _blue;
        set => this.RaiseAndSetIfChanged(ref _blue, value);
    }

    /// <summary>
    /// Gets or sets the hex color value (e.g., "#FFFFFF").
    /// </summary>
    public string HexValue
    {
        get => _hexValue;
        set => this.RaiseAndSetIfChanged(ref _hexValue, value?.ToUpperInvariant() ?? "#FFFFFF");
    }

    /// <summary>
    /// Gets the command to select a color from the palette.
    /// </summary>
    public ICommand SelectColorCommand { get; }

    /// <summary>
    /// Selects a color from the palette.
    /// </summary>
    /// <param name="color">The color to select.</param>
    private void SelectColor(Color color)
    {
        SelectedColor = color;
    }

    /// <summary>
    /// Updates RGB and Hex values when the color changes.
    /// </summary>
    private void UpdateFromColor()
    {
        _isUpdatingFromColor = true;

        Red = _selectedColor.R;
        Green = _selectedColor.G;
        Blue = _selectedColor.B;
        HexValue = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";

        _isUpdatingFromColor = false;
    }

    /// <summary>
    /// Updates the color when RGB values change.
    /// </summary>
    private void UpdateColorFromRGB()
    {
        if (_isUpdatingFromColor) return;

        _isUpdatingFromColor = true;
        SelectedColor = Color.FromRgb(Red, Green, Blue);
        HexValue = $"#{Red:X2}{Green:X2}{Blue:X2}";
        _isUpdatingFromColor = false;
    }

    /// <summary>
    /// Updates the color when hex value changes.
    /// </summary>
    private void UpdateColorFromHex()
    {
        if (_isUpdatingFromColor) return;
        if (string.IsNullOrWhiteSpace(HexValue)) return;

        try
        {
            // Try to parse hex color
            var hex = HexValue.TrimStart('#');
            if (hex.Length == 6 && uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var colorValue))
            {
                _isUpdatingFromColor = true;

                var r = (byte)((colorValue >> 16) & 0xFF);
                var g = (byte)((colorValue >> 8) & 0xFF);
                var b = (byte)(colorValue & 0xFF);

                Red = r;
                Green = g;
                Blue = b;
                SelectedColor = Color.FromRgb(r, g, b);

                _isUpdatingFromColor = false;
                ClearError();
            }
            else
            {
                SetError("Invalid hex color format. Use #RRGGBB");
            }
        }
        catch
        {
            SetError("Invalid hex color format. Use #RRGGBB");
        }
    }

    /// <summary>
    /// Validates and closes the dialog.
    /// </summary>
    protected override void OnOK()
    {
        ClearError();
        base.OnOK();
    }
}
