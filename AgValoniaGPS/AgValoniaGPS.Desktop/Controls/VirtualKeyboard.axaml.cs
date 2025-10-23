using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AgValoniaGPS.Desktop.Controls
{
    /// <summary>
    /// Touch-friendly virtual QWERTY keyboard control for text input.
    /// Features full keyboard layout with shift/caps lock support and special characters.
    /// </summary>
    public partial class VirtualKeyboard : UserControl
    {
        #region Styled Properties

        /// <summary>
        /// The current text value.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Text), defaultValue: string.Empty);

        /// <summary>
        /// Whether shift is currently pressed (temporary uppercase).
        /// </summary>
        public static readonly StyledProperty<bool> IsShiftPressedProperty =
            AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(IsShiftPressed), defaultValue: false);

        /// <summary>
        /// Whether caps lock is active (permanent uppercase).
        /// </summary>
        public static readonly StyledProperty<bool> IsCapsLockActiveProperty =
            AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(IsCapsLockActive), defaultValue: false);

        /// <summary>
        /// Whether to show special characters (symbols) instead of numbers.
        /// </summary>
        public static readonly StyledProperty<bool> ShowSpecialCharsProperty =
            AvaloniaProperty.Register<VirtualKeyboard, bool>(nameof(ShowSpecialChars), defaultValue: false);

        #endregion

        #region Display Properties (for key labels)

        // Row 1 number/symbol keys
        public static readonly StyledProperty<string> Row1Key0Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key0), "`");
        public static readonly StyledProperty<string> Row1Key1Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key1), "1");
        public static readonly StyledProperty<string> Row1Key2Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key2), "2");
        public static readonly StyledProperty<string> Row1Key3Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key3), "3");
        public static readonly StyledProperty<string> Row1Key4Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key4), "4");
        public static readonly StyledProperty<string> Row1Key5Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key5), "5");
        public static readonly StyledProperty<string> Row1Key6Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key6), "6");
        public static readonly StyledProperty<string> Row1Key7Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key7), "7");
        public static readonly StyledProperty<string> Row1Key8Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key8), "8");
        public static readonly StyledProperty<string> Row1Key9Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key9), "9");
        public static readonly StyledProperty<string> Row1Key10Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key10), "0");
        public static readonly StyledProperty<string> Row1Key11Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key11), "-");
        public static readonly StyledProperty<string> Row1Key12Property = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(Row1Key12), "=");

        // Letter keys
        public static readonly StyledProperty<string> QKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(QKey), "q");
        public static readonly StyledProperty<string> WKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(WKey), "w");
        public static readonly StyledProperty<string> EKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(EKey), "e");
        public static readonly StyledProperty<string> RKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(RKey), "r");
        public static readonly StyledProperty<string> TKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(TKey), "t");
        public static readonly StyledProperty<string> YKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(YKey), "y");
        public static readonly StyledProperty<string> UKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(UKey), "u");
        public static readonly StyledProperty<string> IKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(IKey), "i");
        public static readonly StyledProperty<string> OKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(OKey), "o");
        public static readonly StyledProperty<string> PKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(PKey), "p");
        public static readonly StyledProperty<string> AKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(AKey), "a");
        public static readonly StyledProperty<string> SKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(SKey), "s");
        public static readonly StyledProperty<string> DKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(DKey), "d");
        public static readonly StyledProperty<string> FKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(FKey), "f");
        public static readonly StyledProperty<string> GKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(GKey), "g");
        public static readonly StyledProperty<string> HKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(HKey), "h");
        public static readonly StyledProperty<string> JKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(JKey), "j");
        public static readonly StyledProperty<string> KKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(KKey), "k");
        public static readonly StyledProperty<string> LKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(LKey), "l");
        public static readonly StyledProperty<string> ZKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(ZKey), "z");
        public static readonly StyledProperty<string> XKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(XKey), "x");
        public static readonly StyledProperty<string> CKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(CKey), "c");
        public static readonly StyledProperty<string> VKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(VKey), "v");
        public static readonly StyledProperty<string> BKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(BKey), "b");
        public static readonly StyledProperty<string> NKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(NKey), "n");
        public static readonly StyledProperty<string> MKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(MKey), "m");

        // Punctuation keys
        public static readonly StyledProperty<string> BracketLeftKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(BracketLeftKey), "[");
        public static readonly StyledProperty<string> BracketRightKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(BracketRightKey), "]");
        public static readonly StyledProperty<string> SemicolonKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(SemicolonKey), ";");
        public static readonly StyledProperty<string> QuoteKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(QuoteKey), "'");
        public static readonly StyledProperty<string> CommaKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(CommaKey), ",");
        public static readonly StyledProperty<string> PeriodKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(PeriodKey), ".");
        public static readonly StyledProperty<string> SlashKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(SlashKey), "/");
        public static readonly StyledProperty<string> BackslashKeyProperty = AvaloniaProperty.Register<VirtualKeyboard, string>(nameof(BackslashKey), "\\");

        #endregion

        #region Public Properties (Accessors for display keys)

        public string Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public bool IsShiftPressed { get => GetValue(IsShiftPressedProperty); set => SetValue(IsShiftPressedProperty, value); }
        public bool IsCapsLockActive { get => GetValue(IsCapsLockActiveProperty); set => SetValue(IsCapsLockActiveProperty, value); }
        public bool ShowSpecialChars { get => GetValue(ShowSpecialCharsProperty); set => SetValue(ShowSpecialCharsProperty, value); }

        public string Row1Key0 { get => GetValue(Row1Key0Property); set => SetValue(Row1Key0Property, value); }
        public string Row1Key1 { get => GetValue(Row1Key1Property); set => SetValue(Row1Key1Property, value); }
        public string Row1Key2 { get => GetValue(Row1Key2Property); set => SetValue(Row1Key2Property, value); }
        public string Row1Key3 { get => GetValue(Row1Key3Property); set => SetValue(Row1Key3Property, value); }
        public string Row1Key4 { get => GetValue(Row1Key4Property); set => SetValue(Row1Key4Property, value); }
        public string Row1Key5 { get => GetValue(Row1Key5Property); set => SetValue(Row1Key5Property, value); }
        public string Row1Key6 { get => GetValue(Row1Key6Property); set => SetValue(Row1Key6Property, value); }
        public string Row1Key7 { get => GetValue(Row1Key7Property); set => SetValue(Row1Key7Property, value); }
        public string Row1Key8 { get => GetValue(Row1Key8Property); set => SetValue(Row1Key8Property, value); }
        public string Row1Key9 { get => GetValue(Row1Key9Property); set => SetValue(Row1Key9Property, value); }
        public string Row1Key10 { get => GetValue(Row1Key10Property); set => SetValue(Row1Key10Property, value); }
        public string Row1Key11 { get => GetValue(Row1Key11Property); set => SetValue(Row1Key11Property, value); }
        public string Row1Key12 { get => GetValue(Row1Key12Property); set => SetValue(Row1Key12Property, value); }

        public string QKey { get => GetValue(QKeyProperty); set => SetValue(QKeyProperty, value); }
        public string WKey { get => GetValue(WKeyProperty); set => SetValue(WKeyProperty, value); }
        public string EKey { get => GetValue(EKeyProperty); set => SetValue(EKeyProperty, value); }
        public string RKey { get => GetValue(RKeyProperty); set => SetValue(RKeyProperty, value); }
        public string TKey { get => GetValue(TKeyProperty); set => SetValue(TKeyProperty, value); }
        public string YKey { get => GetValue(YKeyProperty); set => SetValue(YKeyProperty, value); }
        public string UKey { get => GetValue(UKeyProperty); set => SetValue(UKeyProperty, value); }
        public string IKey { get => GetValue(IKeyProperty); set => SetValue(IKeyProperty, value); }
        public string OKey { get => GetValue(OKeyProperty); set => SetValue(OKeyProperty, value); }
        public string PKey { get => GetValue(PKeyProperty); set => SetValue(PKeyProperty, value); }
        public string AKey { get => GetValue(AKeyProperty); set => SetValue(AKeyProperty, value); }
        public string SKey { get => GetValue(SKeyProperty); set => SetValue(SKeyProperty, value); }
        public string DKey { get => GetValue(DKeyProperty); set => SetValue(DKeyProperty, value); }
        public string FKey { get => GetValue(FKeyProperty); set => SetValue(FKeyProperty, value); }
        public string GKey { get => GetValue(GKeyProperty); set => SetValue(GKeyProperty, value); }
        public string HKey { get => GetValue(HKeyProperty); set => SetValue(HKeyProperty, value); }
        public string JKey { get => GetValue(JKeyProperty); set => SetValue(JKeyProperty, value); }
        public string KKey { get => GetValue(KKeyProperty); set => SetValue(KKeyProperty, value); }
        public string LKey { get => GetValue(LKeyProperty); set => SetValue(LKeyProperty, value); }
        public string ZKey { get => GetValue(ZKeyProperty); set => SetValue(ZKeyProperty, value); }
        public string XKey { get => GetValue(XKeyProperty); set => SetValue(XKeyProperty, value); }
        public string CKey { get => GetValue(CKeyProperty); set => SetValue(CKeyProperty, value); }
        public string VKey { get => GetValue(VKeyProperty); set => SetValue(VKeyProperty, value); }
        public string BKey { get => GetValue(BKeyProperty); set => SetValue(BKeyProperty, value); }
        public string NKey { get => GetValue(NKeyProperty); set => SetValue(NKeyProperty, value); }
        public string MKey { get => GetValue(MKeyProperty); set => SetValue(MKeyProperty, value); }

        public string BracketLeftKey { get => GetValue(BracketLeftKeyProperty); set => SetValue(BracketLeftKeyProperty, value); }
        public string BracketRightKey { get => GetValue(BracketRightKeyProperty); set => SetValue(BracketRightKeyProperty, value); }
        public string SemicolonKey { get => GetValue(SemicolonKeyProperty); set => SetValue(SemicolonKeyProperty, value); }
        public string QuoteKey { get => GetValue(QuoteKeyProperty); set => SetValue(QuoteKeyProperty, value); }
        public string CommaKey { get => GetValue(CommaKeyProperty); set => SetValue(CommaKeyProperty, value); }
        public string PeriodKey { get => GetValue(PeriodKeyProperty); set => SetValue(PeriodKeyProperty, value); }
        public string SlashKey { get => GetValue(SlashKeyProperty); set => SetValue(SlashKeyProperty, value); }
        public string BackslashKey { get => GetValue(BackslashKeyProperty); set => SetValue(BackslashKeyProperty, value); }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the user clicks the Enter button.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? EnterPressed;

        #endregion

        #region Constructor

        public VirtualKeyboard()
        {
            InitializeComponent();

            // Update key labels when shift/caps changes
            PropertyChanged += (s, e) =>
            {
                if (e.Property == IsShiftPressedProperty || e.Property == IsCapsLockActiveProperty || e.Property == ShowSpecialCharsProperty)
                {
                    UpdateKeyLabels();
                    UpdateShiftButtonState();
                }
            };

            UpdateKeyLabels();
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// Handles key button clicks.
        /// </summary>
        private void OnKeyClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string tag)
                return;

            var parts = tag.Split('|');
            string keyChar;

            if (parts.Length == 3) // Symbol key: "1|normal|shift"
            {
                if (ShowSpecialChars)
                    keyChar = parts[2]; // Symbol
                else if (IsShiftPressed || IsCapsLockActive)
                    keyChar = parts[2]; // Shifted
                else
                    keyChar = parts[1]; // Normal
            }
            else // Letter key: just the letter
            {
                keyChar = parts[0];
                if (IsShiftPressed || IsCapsLockActive)
                    keyChar = keyChar.ToUpper();
                else
                    keyChar = keyChar.ToLower();
            }

            Text += keyChar;

            // Auto-release shift after key press (but not caps)
            if (IsShiftPressed)
            {
                IsShiftPressed = false;
            }
        }

        /// <summary>
        /// Handles shift button click.
        /// </summary>
        private void OnShiftClick(object? sender, RoutedEventArgs e)
        {
            IsShiftPressed = !IsShiftPressed;
        }

        /// <summary>
        /// Handles caps lock button click.
        /// </summary>
        private void OnCapsClick(object? sender, RoutedEventArgs e)
        {
            IsCapsLockActive = !IsCapsLockActive;
            if (IsCapsLockActive)
            {
                IsShiftPressed = false; // Caps overrides shift
            }
        }

        /// <summary>
        /// Handles backspace button click.
        /// </summary>
        private void OnBackspaceClick(object? sender, RoutedEventArgs e)
        {
            if (Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
        }

        /// <summary>
        /// Handles enter button click.
        /// </summary>
        private void OnEnterClick(object? sender, RoutedEventArgs e)
        {
            EnterPressed?.Invoke(this, e);
        }

        /// <summary>
        /// Handles spacebar click.
        /// </summary>
        private void OnSpaceClick(object? sender, RoutedEventArgs e)
        {
            Text += " ";
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates all key labels based on shift/caps state.
        /// </summary>
        private void UpdateKeyLabels()
        {
            bool isUppercase = IsShiftPressed || IsCapsLockActive;

            // Update letter keys
            QKey = isUppercase ? "Q" : "q";
            WKey = isUppercase ? "W" : "w";
            EKey = isUppercase ? "E" : "e";
            RKey = isUppercase ? "R" : "r";
            TKey = isUppercase ? "T" : "t";
            YKey = isUppercase ? "Y" : "y";
            UKey = isUppercase ? "U" : "u";
            IKey = isUppercase ? "I" : "i";
            OKey = isUppercase ? "O" : "o";
            PKey = isUppercase ? "P" : "p";
            AKey = isUppercase ? "A" : "a";
            SKey = isUppercase ? "S" : "s";
            DKey = isUppercase ? "D" : "d";
            FKey = isUppercase ? "F" : "f";
            GKey = isUppercase ? "G" : "g";
            HKey = isUppercase ? "H" : "h";
            JKey = isUppercase ? "J" : "j";
            KKey = isUppercase ? "K" : "k";
            LKey = isUppercase ? "L" : "l";
            ZKey = isUppercase ? "Z" : "z";
            XKey = isUppercase ? "X" : "x";
            CKey = isUppercase ? "C" : "c";
            VKey = isUppercase ? "V" : "v";
            BKey = isUppercase ? "B" : "b";
            NKey = isUppercase ? "N" : "n";
            MKey = isUppercase ? "M" : "m";

            // Update number/symbol row
            if (ShowSpecialChars || IsShiftPressed)
            {
                Row1Key0 = "~";
                Row1Key1 = "!";
                Row1Key2 = "@";
                Row1Key3 = "#";
                Row1Key4 = "$";
                Row1Key5 = "%";
                Row1Key6 = "^";
                Row1Key7 = "&";
                Row1Key8 = "*";
                Row1Key9 = "(";
                Row1Key10 = ")";
                Row1Key11 = "_";
                Row1Key12 = "+";
            }
            else
            {
                Row1Key0 = "`";
                Row1Key1 = "1";
                Row1Key2 = "2";
                Row1Key3 = "3";
                Row1Key4 = "4";
                Row1Key5 = "5";
                Row1Key6 = "6";
                Row1Key7 = "7";
                Row1Key8 = "8";
                Row1Key9 = "9";
                Row1Key10 = "0";
                Row1Key11 = "-";
                Row1Key12 = "=";
            }

            // Update punctuation
            BracketLeftKey = (ShowSpecialChars || IsShiftPressed) ? "{" : "[";
            BracketRightKey = (ShowSpecialChars || IsShiftPressed) ? "}" : "]";
            SemicolonKey = (ShowSpecialChars || IsShiftPressed) ? ":" : ";";
            QuoteKey = (ShowSpecialChars || IsShiftPressed) ? "\"" : "'";
            CommaKey = (ShowSpecialChars || IsShiftPressed) ? "<" : ",";
            PeriodKey = (ShowSpecialChars || IsShiftPressed) ? ">" : ".";
            SlashKey = (ShowSpecialChars || IsShiftPressed) ? "?" : "/";
            BackslashKey = (ShowSpecialChars || IsShiftPressed) ? "|" : "\\";
        }

        /// <summary>
        /// Updates the visual state of shift/caps buttons.
        /// </summary>
        private void UpdateShiftButtonState()
        {
            if (ShiftButton != null)
            {
                if (IsShiftPressed)
                    ShiftButton.Classes.Add("Active");
                else
                    ShiftButton.Classes.Remove("Active");
            }

            if (CapsButton != null)
            {
                if (IsCapsLockActive)
                    CapsButton.Classes.Add("Active");
                else
                    CapsButton.Classes.Remove("Active");
            }
        }

        #endregion
    }
}
