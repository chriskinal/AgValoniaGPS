using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using AgValoniaGPS.Models.FieldOperations;

namespace AgValoniaGPS.Services.FieldOperations;

/// <summary>
/// Service for reading and writing field marker files.
/// Supports JSON format for saving and loading markers.
/// File name: Markers.json
/// </summary>
public class FieldMarkerFileService
{
    private const string MarkerFileName = "Markers.json";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Load all markers from Markers.json file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the field data.</param>
    /// <returns>List of markers if file exists and is valid, empty list otherwise.</returns>
    public List<FieldMarker> LoadMarkers(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, MarkerFileName);

        if (!File.Exists(filePath))
        {
            return new List<FieldMarker>();
        }

        try
        {
            using var reader = new StreamReader(filePath);
            var content = reader.ReadToEnd();

            var markers = JsonSerializer.Deserialize<List<FieldMarker>>(content, _jsonOptions);
            return markers ?? new List<FieldMarker>();
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error loading markers from {filePath}: {ex.Message}");
            return new List<FieldMarker>();
        }
    }

    /// <summary>
    /// Save markers to Markers.json file.
    /// </summary>
    /// <param name="markers">List of markers to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveMarkers(List<FieldMarker> markers, string fieldDirectory)
    {
        if (string.IsNullOrWhiteSpace(fieldDirectory))
        {
            throw new ArgumentException("Field directory must be specified", nameof(fieldDirectory));
        }

        if (!Directory.Exists(fieldDirectory))
        {
            Directory.CreateDirectory(fieldDirectory);
        }

        var filePath = Path.Combine(fieldDirectory, MarkerFileName);

        try
        {
            var json = JsonSerializer.Serialize(markers, _jsonOptions);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save markers to {filePath}", ex);
        }
    }

    /// <summary>
    /// Save a single marker to the markers file.
    /// Loads existing markers, adds or updates the marker, and saves back.
    /// </summary>
    /// <param name="marker">Marker to save.</param>
    /// <param name="fieldDirectory">Directory to save the file to.</param>
    public void SaveMarker(FieldMarker marker, string fieldDirectory)
    {
        var markers = LoadMarkers(fieldDirectory);

        // Find existing marker with same ID
        var existingIndex = markers.FindIndex(m => m.Id == marker.Id);

        if (existingIndex >= 0)
        {
            // Update existing marker
            markers[existingIndex] = marker;
        }
        else
        {
            // Add new marker
            markers.Add(marker);
        }

        SaveMarkers(markers, fieldDirectory);
    }

    /// <summary>
    /// Delete a specific marker from the markers file.
    /// </summary>
    /// <param name="markerId">ID of marker to delete.</param>
    /// <param name="fieldDirectory">Directory containing the file.</param>
    /// <returns>True if marker was deleted, false if marker wasn't found.</returns>
    public bool DeleteMarker(int markerId, string fieldDirectory)
    {
        var markers = LoadMarkers(fieldDirectory);
        var removed = markers.RemoveAll(m => m.Id == markerId);

        if (removed > 0)
        {
            SaveMarkers(markers, fieldDirectory);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Delete markers file from field directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file to delete.</param>
    /// <returns>True if file was deleted, false if file didn't exist.</returns>
    public bool DeleteAllMarkers(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, MarkerFileName);

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete markers file {filePath}", ex);
        }
    }

    /// <summary>
    /// Check if markers file exists in the specified directory.
    /// </summary>
    /// <param name="fieldDirectory">Directory to check.</param>
    /// <returns>True if file exists.</returns>
    public bool MarkersFileExists(string fieldDirectory)
    {
        var filePath = Path.Combine(fieldDirectory, MarkerFileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Get the full path to the markers file.
    /// </summary>
    /// <param name="fieldDirectory">Directory containing the file.</param>
    /// <returns>Full file path.</returns>
    public string GetMarkerFilePath(string fieldDirectory)
    {
        return Path.Combine(fieldDirectory, MarkerFileName);
    }

    /// <summary>
    /// Import markers from another field directory.
    /// </summary>
    /// <param name="sourceDirectory">Source field directory.</param>
    /// <param name="targetDirectory">Target field directory.</param>
    /// <param name="merge">If true, merge with existing markers; if false, replace.</param>
    /// <returns>Number of markers imported.</returns>
    public int ImportMarkers(string sourceDirectory, string targetDirectory, bool merge = true)
    {
        var sourceMarkers = LoadMarkers(sourceDirectory);

        if (sourceMarkers.Count == 0)
        {
            return 0;
        }

        if (merge)
        {
            var targetMarkers = LoadMarkers(targetDirectory);
            targetMarkers.AddRange(sourceMarkers);
            SaveMarkers(targetMarkers, targetDirectory);
        }
        else
        {
            SaveMarkers(sourceMarkers, targetDirectory);
        }

        return sourceMarkers.Count;
    }

    /// <summary>
    /// Export markers to another directory.
    /// </summary>
    /// <param name="sourceDirectory">Source field directory.</param>
    /// <param name="exportPath">Full path to export file.</param>
    /// <returns>Number of markers exported.</returns>
    public int ExportMarkers(string sourceDirectory, string exportPath)
    {
        var markers = LoadMarkers(sourceDirectory);

        if (markers.Count == 0)
        {
            return 0;
        }

        var exportDir = Path.GetDirectoryName(exportPath);
        if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        var json = JsonSerializer.Serialize(markers, _jsonOptions);
        File.WriteAllText(exportPath, json, Encoding.UTF8);

        return markers.Count;
    }
}
