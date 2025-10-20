using System.Globalization;
using System.Xml.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Configuration;
using AgValoniaGPS.Models.Guidance;

namespace AgValoniaGPS.Services.Configuration;

/// <summary>
/// Converts between hierarchical JSON ApplicationSettings and flat XML format.
/// Handles property name mappings and value conversions for legacy compatibility.
/// </summary>
public class ConfigurationConverter
{
    /// <summary>
    /// Converts hierarchical ApplicationSettings to flat XML structure.
    /// </summary>
    /// <param name="settings">Application settings to convert</param>
    /// <returns>XML root element with flat structure</returns>
    public XElement JsonToXml(ApplicationSettings settings)
    {
        var root = new XElement("VehicleSettings");

        // Culture Settings (first in XML)
        AddElement(root, "Culture", settings.Culture.CultureCode);

        // Vehicle Settings
        AddElement(root, "Wheelbase", settings.Vehicle.Wheelbase);
        AddElement(root, "Track", settings.Vehicle.Track);
        AddElement(root, "MaxSteerDeg", settings.Vehicle.MaxSteerAngle);
        AddElement(root, "MaxAngularVel", settings.Vehicle.MaxAngularVelocity);
        AddElement(root, "AntennaPivot", settings.Vehicle.AntennaPivot);
        AddElement(root, "AntennaHeight", settings.Vehicle.AntennaHeight);
        AddElement(root, "AntennaOffset", settings.Vehicle.AntennaOffset);
        AddElement(root, "PivotBehindAnt", settings.Vehicle.PivotBehindAnt);
        AddElement(root, "SteerAxleAhead", settings.Vehicle.SteerAxleAhead);
        AddElement(root, "VehicleType", (int)settings.Vehicle.VehicleType);
        AddElement(root, "VehHitchLength", settings.Vehicle.VehicleHitchLength);
        AddElement(root, "MinUturnRadius", settings.Vehicle.MinUturnRadius);

        // Steering Settings
        AddElement(root, "CPD", settings.Steering.CountsPerDegree);
        AddElement(root, "Ackermann", settings.Steering.Ackermann);
        AddElement(root, "WASOffset", settings.Steering.WasOffset);
        AddElement(root, "HighPWM", settings.Steering.HighPwm);
        AddElement(root, "LowPWM", settings.Steering.LowPwm);
        AddElement(root, "MinPWM", settings.Steering.MinPwm);
        AddElement(root, "MaxPWM", settings.Steering.MaxPwm);
        AddElement(root, "PanicStop", settings.Steering.PanicStop);

        // Tool Settings
        // Tool width conversion: meters to code (approximate reverse of code*0.457)
        int toolWidthCode = (int)Math.Round(settings.Tool.ToolWidth / 0.457);
        AddElement(root, "ToolWidth", toolWidthCode);
        AddElement(root, "ToolFront", settings.Tool.ToolFront);
        AddElement(root, "ToolRearFixed", settings.Tool.ToolRearFixed);
        AddElement(root, "ToolTBT", settings.Tool.ToolTBT);
        AddElement(root, "ToolTrailing", settings.Tool.ToolTrailing);
        AddElement(root, "ToolToPivotLen", settings.Tool.ToolToPivotLength);
        AddElement(root, "ToolLookAndOn", settings.Tool.ToolLookAheadOn);
        AddElement(root, "ToolLookAndOff", settings.Tool.ToolLookAheadOff);
        AddElement(root, "ToolOffDelay", settings.Tool.ToolOffDelay);
        AddElement(root, "ToolOffset", settings.Tool.ToolOffset);
        AddElement(root, "ToolOverlap", settings.Tool.ToolOverlap);
        AddElement(root, "TrailingHitchLen", settings.Tool.TrailingHitchLength);
        AddElement(root, "TankHitchLength", settings.Tool.TankHitchLength);
        AddElement(root, "HydLiftLookAnd", settings.Tool.HydLiftLookAhead);

        // Section Control Settings
        AddElement(root, "NumberSections", settings.SectionControl.NumberSections);
        AddElement(root, "HeadlandSecControl", settings.SectionControl.HeadlandSecControl);
        AddElement(root, "FastSections", settings.SectionControl.FastSections);
        AddElement(root, "SectionOffOutBnds", settings.SectionControl.SectionOffOutBounds);
        AddElement(root, "SectionsNotZones", settings.SectionControl.SectionsNotZones);
        AddElement(root, "LowSpeedCutoff", settings.SectionControl.LowSpeedCutoff);

        // GPS Settings
        AddElement(root, "HDOP", settings.Gps.Hdop);
        AddElement(root, "RawHz", settings.Gps.RawHz);
        AddElement(root, "Hz", settings.Gps.Hz);
        AddElement(root, "GPSAgeAlarm", settings.Gps.GpsAgeAlarm);
        AddElement(root, "HeadingFrom", settings.Gps.HeadingFrom);
        AddElement(root, "AutoStartAgIO", settings.Gps.AutoStartAgIO);
        AddElement(root, "AutoOffAgIO", settings.Gps.AutoOffAgIO);
        AddElement(root, "RTK", settings.Gps.Rtk);
        AddElement(root, "RTKKillAutoSteer", settings.Gps.RtkKillAutoSteer);

        // IMU Settings
        AddElement(root, "DualAsIMU", settings.Imu.DualAsImu);
        AddElement(root, "DualHeadingOffset", settings.Imu.DualHeadingOffset);
        AddElement(root, "IMUFusionWeight", settings.Imu.ImuFusionWeight);
        AddElement(root, "MinHeadingStep", settings.Imu.MinHeadingStep);
        AddElement(root, "MinStepLimit", settings.Imu.MinStepLimit);
        AddElement(root, "RollZero", settings.Imu.RollZero);
        AddElement(root, "InvertRoll", settings.Imu.InvertRoll);
        AddElement(root, "RollFilter", settings.Imu.RollFilter);

        // Guidance Settings
        AddElement(root, "AcquireFactor", settings.Guidance.AcquireFactor);
        AddElement(root, "LookAhead", settings.Guidance.LookAhead);
        AddElement(root, "SpeedFactor", settings.Guidance.SpeedFactor);
        AddElement(root, "PPIntegral", settings.Guidance.PurePursuitIntegral);
        AddElement(root, "SnapDistance", settings.Guidance.SnapDistance);
        AddElement(root, "RefSnapDistance", settings.Guidance.RefSnapDistance);
        AddElement(root, "SideHillComp", settings.Guidance.SideHillComp);

        // Work Mode Settings
        AddElement(root, "RemoteWork", settings.WorkMode.RemoteWork);
        AddElement(root, "SteerWorkSw", settings.WorkMode.SteerWorkSwitch);
        AddElement(root, "SteerWorkManual", settings.WorkMode.SteerWorkManual);
        AddElement(root, "WorkActiveLow", settings.WorkMode.WorkActiveLow);
        AddElement(root, "WorkSwitch", settings.WorkMode.WorkSwitch);
        AddElement(root, "WorkManualSec", settings.WorkMode.WorkManualSection);

        // System State Settings
        AddElement(root, "StanleyUsed", settings.SystemState.StanleyUsed);
        AddElement(root, "SteerInReverse", settings.SystemState.SteerInReverse);
        AddElement(root, "ReverseOn", settings.SystemState.ReverseOn);
        AddElement(root, "Heading", settings.SystemState.Heading);
        AddElement(root, "IMU", settings.SystemState.ImuHeading);
        AddElement(root, "HeadingError", settings.SystemState.HeadingError);
        AddElement(root, "DistanceError", settings.SystemState.DistanceError);
        AddElement(root, "SteerIntegral", settings.SystemState.SteerIntegral);

        // Display Settings
        AddElement(root, "DeadHeadDelay", string.Join(",", settings.Display.DeadHeadDelay));
        AddElement(root, "North", settings.Display.North);
        AddElement(root, "East", settings.Display.East);
        AddElement(root, "Elev", settings.Display.Elevation);

        return root;
    }

    /// <summary>
    /// Converts flat XML structure to hierarchical ApplicationSettings.
    /// </summary>
    /// <param name="root">XML root element</param>
    /// <returns>Application settings</returns>
    public ApplicationSettings XmlToJson(XElement root)
    {
        var settings = new ApplicationSettings();

        // Vehicle Settings
        settings.Vehicle.Wheelbase = GetDouble(root, "Wheelbase", 180.0);
        settings.Vehicle.Track = GetDouble(root, "Track", 30.0);
        settings.Vehicle.MaxSteerAngle = GetDouble(root, "MaxSteerDeg", 45.0);
        settings.Vehicle.MaxAngularVelocity = GetDouble(root, "MaxAngularVel", 100.0);
        settings.Vehicle.AntennaPivot = GetDouble(root, "AntennaPivot", 25.0);
        settings.Vehicle.AntennaHeight = GetDouble(root, "AntennaHeight", 50.0);
        settings.Vehicle.AntennaOffset = GetDouble(root, "AntennaOffset", 0.0);
        settings.Vehicle.PivotBehindAnt = GetDouble(root, "PivotBehindAnt", 30.0);
        settings.Vehicle.SteerAxleAhead = GetDouble(root, "SteerAxleAhead", 110.0);
        settings.Vehicle.VehicleType = (AgValoniaGPS.Models.Configuration.VehicleType)GetInt(root, "VehicleType", 0);
        settings.Vehicle.VehicleHitchLength = GetDouble(root, "VehHitchLength", 0.0);
        settings.Vehicle.MinUturnRadius = GetDouble(root, "MinUturnRadius", 3.0);

        // Steering Settings
        settings.Steering.CountsPerDegree = GetInt(root, "CPD", 100);
        settings.Steering.Ackermann = GetInt(root, "Ackermann", 100);
        settings.Steering.WasOffset = GetDouble(root, "WASOffset", 0.0);
        settings.Steering.HighPwm = GetInt(root, "HighPWM", 235);
        settings.Steering.LowPwm = GetInt(root, "LowPWM", 78);
        settings.Steering.MinPwm = GetInt(root, "MinPWM", 5);
        settings.Steering.MaxPwm = GetInt(root, "MaxPWM", 10);
        settings.Steering.PanicStop = GetInt(root, "PanicStop", 0);

        // Tool Settings
        // Tool width conversion: code to meters (code * 0.457)
        int toolWidthCode = GetInt(root, "ToolWidth", 4);
        settings.Tool.ToolWidth = toolWidthCode * 0.457;
        settings.Tool.ToolFront = GetBool(root, "ToolFront", false);
        settings.Tool.ToolRearFixed = GetBool(root, "ToolRearFixed", true);
        settings.Tool.ToolTBT = GetBool(root, "ToolTBT", false);
        settings.Tool.ToolTrailing = GetBool(root, "ToolTrailing", false);
        settings.Tool.ToolToPivotLength = GetDouble(root, "ToolToPivotLen", 0.0);
        settings.Tool.ToolLookAheadOn = GetDouble(root, "ToolLookAndOn", 1.0);
        settings.Tool.ToolLookAheadOff = GetDouble(root, "ToolLookAndOff", 0.5);
        settings.Tool.ToolOffDelay = GetDouble(root, "ToolOffDelay", 0.0);
        settings.Tool.ToolOffset = GetDouble(root, "ToolOffset", 0.0);
        settings.Tool.ToolOverlap = GetDouble(root, "ToolOverlap", 0.0);
        settings.Tool.TrailingHitchLength = GetDouble(root, "TrailingHitchLen", -2.5);
        settings.Tool.TankHitchLength = GetDouble(root, "TankHitchLength", 3.0);
        settings.Tool.HydLiftLookAhead = GetDouble(root, "HydLiftLookAnd", 2.0);

        // Section Control Settings
        settings.SectionControl.NumberSections = GetInt(root, "NumberSections", 3);
        settings.SectionControl.HeadlandSecControl = GetBool(root, "HeadlandSecControl", false);
        settings.SectionControl.FastSections = GetBool(root, "FastSections", true);
        settings.SectionControl.SectionOffOutBounds = GetBool(root, "SectionOffOutBnds", true);
        settings.SectionControl.SectionsNotZones = GetBool(root, "SectionsNotZones", true);
        settings.SectionControl.LowSpeedCutoff = GetDouble(root, "LowSpeedCutoff", 1.0);
        settings.SectionControl.SectionPositions = Array.Empty<double>();

        // GPS Settings
        settings.Gps.Hdop = GetDouble(root, "HDOP", 0.69);
        settings.Gps.RawHz = GetDouble(root, "RawHz", 9.890);
        settings.Gps.Hz = GetDouble(root, "Hz", 10.0);
        settings.Gps.GpsAgeAlarm = GetInt(root, "GPSAgeAlarm", 20);
        settings.Gps.HeadingFrom = GetString(root, "HeadingFrom", "Dual");
        settings.Gps.AutoStartAgIO = GetBool(root, "AutoStartAgIO", true);
        settings.Gps.AutoOffAgIO = GetBool(root, "AutoOffAgIO", false);
        settings.Gps.Rtk = GetBool(root, "RTK", false);
        settings.Gps.RtkKillAutoSteer = GetBool(root, "RTKKillAutoSteer", false);

        // IMU Settings
        settings.Imu.DualAsImu = GetBool(root, "DualAsIMU", false);
        settings.Imu.DualHeadingOffset = GetInt(root, "DualHeadingOffset", 90);
        settings.Imu.ImuFusionWeight = GetDouble(root, "IMUFusionWeight", 0.06);
        settings.Imu.MinHeadingStep = GetDouble(root, "MinHeadingStep", 0.5);
        settings.Imu.MinStepLimit = GetDouble(root, "MinStepLimit", 0.05);
        settings.Imu.RollZero = GetDouble(root, "RollZero", 0.0);
        settings.Imu.InvertRoll = GetBool(root, "InvertRoll", false);
        settings.Imu.RollFilter = GetDouble(root, "RollFilter", 0.15);

        // Guidance Settings
        settings.Guidance.AcquireFactor = GetDouble(root, "AcquireFactor", 0.90);
        settings.Guidance.LookAhead = GetDouble(root, "LookAhead", 3.0);
        settings.Guidance.SpeedFactor = GetDouble(root, "SpeedFactor", 1.0);
        settings.Guidance.PurePursuitIntegral = GetDouble(root, "PPIntegral", 0.0);
        settings.Guidance.SnapDistance = GetDouble(root, "SnapDistance", 20.0);
        settings.Guidance.RefSnapDistance = GetDouble(root, "RefSnapDistance", 5.0);
        settings.Guidance.SideHillComp = GetDouble(root, "SideHillComp", 0.0);

        // Work Mode Settings
        settings.WorkMode.RemoteWork = GetBool(root, "RemoteWork", false);
        settings.WorkMode.SteerWorkSwitch = GetBool(root, "SteerWorkSw", false);
        settings.WorkMode.SteerWorkManual = GetBool(root, "SteerWorkManual", true);
        settings.WorkMode.WorkActiveLow = GetBool(root, "WorkActiveLow", false);
        settings.WorkMode.WorkSwitch = GetBool(root, "WorkSwitch", false);
        settings.WorkMode.WorkManualSection = GetBool(root, "WorkManualSec", true);

        // Culture Settings
        settings.Culture.CultureCode = GetString(root, "Culture", "en");
        settings.Culture.LanguageName = "English";

        // System State Settings
        settings.SystemState.StanleyUsed = GetBool(root, "StanleyUsed", false);
        settings.SystemState.SteerInReverse = GetBool(root, "SteerInReverse", false);
        settings.SystemState.ReverseOn = GetBool(root, "ReverseOn", true);
        settings.SystemState.Heading = GetDouble(root, "Heading", 0.0);
        settings.SystemState.ImuHeading = GetDouble(root, "IMU", 0.0);
        settings.SystemState.HeadingError = GetInt(root, "HeadingError", 0);
        settings.SystemState.DistanceError = GetInt(root, "DistanceError", 0);
        settings.SystemState.SteerIntegral = GetInt(root, "SteerIntegral", 0);

        // Display Settings
        var deadHeadDelayStr = GetString(root, "DeadHeadDelay", "10,10");
        var deadHeadParts = deadHeadDelayStr.Split(',');
        settings.Display.DeadHeadDelay = deadHeadParts.Length >= 2
            ? new[] { int.Parse(deadHeadParts[0].Trim()), int.Parse(deadHeadParts[1].Trim()) }
            : new[] { 10, 10 };
        settings.Display.North = GetDouble(root, "North", 0.0);
        settings.Display.East = GetDouble(root, "East", 0.0);
        settings.Display.Elevation = GetDouble(root, "Elev", 0.0);
        settings.Display.UnitSystem = UnitSystem.Metric;
        settings.Display.SpeedSource = "GPS";

        return settings;
    }

    private static void AddElement(XElement parent, string name, object value)
    {
        string strValue = value switch
        {
            bool b => b ? "True" : "False",
            double d => d.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
        parent.Add(new XElement(name, strValue));
    }

    private static string GetString(XElement root, string elementName, string defaultValue)
    {
        var element = root.Element(elementName);
        return element?.Value ?? defaultValue;
    }

    private static int GetInt(XElement root, string elementName, int defaultValue)
    {
        var element = root.Element(elementName);
        return element != null && int.TryParse(element.Value, out var value) ? value : defaultValue;
    }

    private static double GetDouble(XElement root, string elementName, double defaultValue)
    {
        var element = root.Element(elementName);
        return element != null && double.TryParse(element.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : defaultValue;
    }

    private static bool GetBool(XElement root, string elementName, bool defaultValue)
    {
        var element = root.Element(elementName);
        if (element == null) return defaultValue;

        var value = element.Value;
        return value.Equals("True", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("1", StringComparison.Ordinal);
    }
}
