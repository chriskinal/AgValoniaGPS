using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Globalization;

namespace AgValoniaGPS.Desktop.Controls
{
    /// <summary>
    /// Touch-friendly numeric keypad control for entering numeric values.
    /// Features large buttons (60x60), decimal support, and validation.
    /// </summary>
    public partial class NumericKeypad : UserControl
    {
        #region Styled Properties

        /// <summary>
        /// The current numeric value entered on the keypad.
        /// </summary>
        public static readonly StyledProperty<decimal> ValueProperty =
            AvaloniaProperty.Register<NumericKeypad, decimal>(nameof(Value), defaultValue: 0m);

        /// <summary>
        /// The number of decimal places to allow (0 for integers).
        /// </summary>
        public static readonly StyledProperty<int> DecimalPlacesProperty =
            AvaloniaProperty.Register<NumericKeypad, int>(nameof(DecimalPlaces), defaultValue: 2);

        /// <summary>
        /// The minimum allowed value (for validation).
        /// </summary>
        public static readonly StyledProperty<decimal> MinValueProperty =
            AvaloniaProperty.Register<NumericKeypad, decimal>(nameof(MinValue), defaultValue: decimal.MinValue);

        /// <summary>
        /// The maximum allowed value (for validation).
        /// </summary>
        public static readonly StyledProperty<decimal> MaxValueProperty =
            AvaloniaProperty.Register<NumericKeypad, decimal>(nameof(MaxValue), defaultValue: decimal.MaxValue);

        /// <summary>
        /// Whether to show the decimal point button.
        /// </summary>
        public static readonly StyledProperty<bool> ShowDecimalButtonProperty =
            AvaloniaProperty.Register<NumericKeypad, bool>(nameof(ShowDecimalButton), defaultValue: true);

        /// <summary>
        /// The display value (internal string representation for editing).
        /// </summary>
        public static readonly StyledProperty<string> DisplayValueProperty =
            AvaloniaProperty.Register<NumericKeypad, string>(nameof(DisplayValue), defaultValue: "0");

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current numeric value.
        /// </summary>
        public decimal Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of decimal places allowed.
        /// </summary>
        public int DecimalPlaces
        {
            get => GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// </summary>
        public decimal MinValue
        {
            get => GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// </summary>
        public decimal MaxValue
        {
            get => GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the decimal button is shown.
        /// </summary>
        public bool ShowDecimalButton
        {
            get => GetValue(ShowDecimalButtonProperty);
            set => SetValue(ShowDecimalButtonProperty, value);
        }

        /// <summary>
        /// Gets or sets the display value (for internal editing).
        /// </summary>
        public string DisplayValue
        {
            get => GetValue(DisplayValueProperty);
            set => SetValue(DisplayValueProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the user clicks the Enter/OK button.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? EnterPressed;

        #endregion

        #region Constructor

        public NumericKeypad()
        {
            InitializeComponent();

            // Initialize display with current value
            PropertyChanged += (s, e) =>
            {
                if (e.Property == ValueProperty)
                {
                    UpdateDisplayFromValue();
                }
                else if (e.Property == DecimalPlacesProperty)
                {
                    // Update ShowDecimalButton based on DecimalPlaces
                    ShowDecimalButton = DecimalPlaces > 0;
                }
            };

            UpdateDisplayFromValue();
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// Handles numeric button clicks (0-9, 00).
        /// </summary>
        private void OnNumberClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string digit)
            {
                AppendDigit(digit);
            }
        }

        /// <summary>
        /// Handles decimal point button click.
        /// </summary>
        private void OnDecimalClick(object? sender, RoutedEventArgs e)
        {
            if (DecimalPlaces == 0) return; // No decimals allowed

            // Only add decimal if not already present
            if (!DisplayValue.Contains("."))
            {
                DisplayValue += ".";
            }
        }

        /// <summary>
        /// Handles backspace button click.
        /// </summary>
        private void OnBackspaceClick(object? sender, RoutedEventArgs e)
        {
            if (DisplayValue.Length > 1)
            {
                DisplayValue = DisplayValue.Substring(0, DisplayValue.Length - 1);
            }
            else
            {
                DisplayValue = "0";
            }

            UpdateValueFromDisplay();
        }

        /// <summary>
        /// Handles clear button click.
        /// </summary>
        private void OnClearClick(object? sender, RoutedEventArgs e)
        {
            DisplayValue = "0";
            Value = 0m;
        }

        /// <summary>
        /// Handles enter/OK button click.
        /// </summary>
        private void OnEnterClick(object? sender, RoutedEventArgs e)
        {
            UpdateValueFromDisplay();
            ValidateValue();
            EnterPressed?.Invoke(this, e);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Appends a digit to the display value.
        /// </summary>
        private void AppendDigit(string digit)
        {
            // Handle initial zero
            if (DisplayValue == "0" && digit != "00")
            {
                DisplayValue = digit;
            }
            else
            {
                // Check decimal places constraint
                if (DisplayValue.Contains("."))
                {
                    var decimalPart = DisplayValue.Split('.')[1];
                    if (decimalPart.Length >= DecimalPlaces)
                    {
                        return; // Max decimal places reached
                    }
                }

                DisplayValue += digit;
            }

            UpdateValueFromDisplay();
        }

        /// <summary>
        /// Updates the Value property from the DisplayValue string.
        /// </summary>
        private void UpdateValueFromDisplay()
        {
            if (decimal.TryParse(DisplayValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedValue))
            {
                Value = parsedValue;
            }
        }

        /// <summary>
        /// Updates the DisplayValue string from the Value property.
        /// </summary>
        private void UpdateDisplayFromValue()
        {
            DisplayValue = Value.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Validates the value is within min/max range.
        /// </summary>
        private void ValidateValue()
        {
            if (Value < MinValue)
            {
                Value = MinValue;
                UpdateDisplayFromValue();
            }
            else if (Value > MaxValue)
            {
                Value = MaxValue;
                UpdateDisplayFromValue();
            }
        }

        #endregion
    }
}
