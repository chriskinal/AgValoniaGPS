using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.Services.FieldOperations;
using PositionModel = AgValoniaGPS.Models.Position;

namespace AgValoniaGPS.Services;

/// <summary>
/// Implementation of field management service.
/// Coordinates file I/O services to provide complete field management
/// including guidance line persistence (AB lines, curve lines, contours).
/// </summary>
public class FieldService : IFieldService
{
    private readonly FieldPlaneFileService _fieldPlaneService;
    private readonly IBoundaryFileService _boundaryService;
    private readonly BackgroundImageFileService _backgroundImageService;
    private readonly ABLineFileService _abLineService;
    private readonly CurveLineFileService _curveLineService;
    private readonly ContourLineFileService _contourService;

    public event EventHandler<Field?>? ActiveFieldChanged;
    public Field? ActiveField { get; private set; }

    public FieldService()
    {
        _fieldPlaneService = new FieldPlaneFileService();
        _boundaryService = new BoundaryFileService();
        _backgroundImageService = new BackgroundImageFileService();
        _abLineService = new ABLineFileService();
        _curveLineService = new CurveLineFileService();
        _contourService = new ContourLineFileService();
    }

    /// <summary>
    /// Get list of available field names in the Fields directory
    /// </summary>
    public List<string> GetAvailableFields(string fieldsRootDirectory)
    {
        if (!Directory.Exists(fieldsRootDirectory))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(fieldsRootDirectory)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()
            .OrderBy(name => name)
            .ToList();
    }

    /// <summary>
    /// Load a complete field (Field.txt, Boundary.txt, BackPic.Txt)
    /// </summary>
    public Field LoadField(string fieldDirectory)
    {
        var field = _fieldPlaneService.LoadField(fieldDirectory);

        // Load boundary from AgOpenGPS format
        var boundaryPath = Path.Combine(fieldDirectory, "Boundary.txt");
        var boundaryPositions = _boundaryService.LoadFromAgOpenGPS(boundaryPath);
        if (boundaryPositions.Length > 0)
        {
            // Convert Position[] to BoundaryPolygon
            var outerBoundary = new BoundaryPolygon
            {
                Points = boundaryPositions.Select(p => new BoundaryPoint(p.Easting, p.Northing, p.Heading)).ToList()
            };

            field.Boundary = new Boundary
            {
                OuterBoundary = outerBoundary,
                IsActive = true
            };
        }

        field.BackgroundImage = _backgroundImageService.LoadBackgroundImage(fieldDirectory);
        return field;
    }

    /// <summary>
    /// Save a complete field (Field.txt, Boundary.txt, BackPic.Txt)
    /// </summary>
    public void SaveField(Field field)
    {
        if (string.IsNullOrWhiteSpace(field.DirectoryPath))
        {
            throw new ArgumentException("Field.DirectoryPath must be set", nameof(field));
        }

        _fieldPlaneService.SaveField(field, field.DirectoryPath);

        if (field.Boundary != null && field.Boundary.OuterBoundary != null && field.Boundary.OuterBoundary.Points.Count > 0)
        {
            // Convert BoundaryPolygon to Position[]
            var positions = field.Boundary.OuterBoundary.Points.Select(p => new Position
            {
                Easting = p.Easting,
                Northing = p.Northing,
                Heading = p.Heading
            }).ToArray();

            var boundaryPath = Path.Combine(field.DirectoryPath, "Boundary.txt");
            _boundaryService.SaveToAgOpenGPS(positions, boundaryPath);
        }

        if (field.BackgroundImage != null)
        {
            _backgroundImageService.SaveBackgroundImage(field.BackgroundImage, field.DirectoryPath);
        }
    }

    /// <summary>
    /// Create a new empty field
    /// </summary>
    public Field CreateField(string fieldsRootDirectory, string fieldName, PositionModel originPosition)
    {
        var fieldDirectory = Path.Combine(fieldsRootDirectory, fieldName);

        if (Directory.Exists(fieldDirectory))
        {
            throw new InvalidOperationException($"Field '{fieldName}' already exists");
        }

        Directory.CreateDirectory(fieldDirectory);

        var field = new Field
        {
            Name = fieldName,
            DirectoryPath = fieldDirectory,
            Origin = originPosition,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        // Create empty boundary file
        var boundaryPath = Path.Combine(fieldDirectory, "Boundary.txt");
        _boundaryService.SaveToAgOpenGPS(Array.Empty<Position>(), boundaryPath);

        // Save field metadata
        _fieldPlaneService.SaveField(field, fieldDirectory);

        return field;
    }

    /// <summary>
    /// Delete a field (removes entire directory)
    /// </summary>
    public void DeleteField(string fieldDirectory)
    {
        if (Directory.Exists(fieldDirectory))
        {
            Directory.Delete(fieldDirectory, true);
        }
    }

    /// <summary>
    /// Check if a field exists
    /// </summary>
    public bool FieldExists(string fieldDirectory)
    {
        return Directory.Exists(fieldDirectory) &&
               File.Exists(Path.Combine(fieldDirectory, "Field.txt"));
    }

    /// <summary>
    /// Set the active field
    /// </summary>
    public void SetActiveField(Field? field)
    {
        if (ActiveField != field)
        {
            ActiveField = field;
            ActiveFieldChanged?.Invoke(this, field);
        }
    }

    /// <summary>
    /// Save AB line to field directory. Persists to ABLine.txt in JSON format.
    /// </summary>
    public void SaveABLine(ABLine abLine, string fieldDirectory)
    {
        _abLineService.SaveABLine(abLine, fieldDirectory);
    }

    /// <summary>
    /// Load AB line from field directory. Supports both JSON and legacy AgOpenGPS text format.
    /// </summary>
    public ABLine? LoadABLine(string fieldDirectory)
    {
        return _abLineService.LoadABLine(fieldDirectory);
    }

    /// <summary>
    /// Save curve line to field directory. Persists to CurveLine.txt in JSON format.
    /// </summary>
    public void SaveCurveLine(CurveLine curveLine, string fieldDirectory)
    {
        _curveLineService.SaveCurveLine(curveLine, fieldDirectory);
    }

    /// <summary>
    /// Load curve line from field directory. Supports both JSON and legacy AgOpenGPS text format.
    /// </summary>
    public CurveLine? LoadCurveLine(string fieldDirectory)
    {
        return _curveLineService.LoadCurveLine(fieldDirectory);
    }

    /// <summary>
    /// Save contour line to field directory. Persists to Contour.txt in JSON format.
    /// </summary>
    public void SaveContour(ContourLine contour, string fieldDirectory)
    {
        _contourService.SaveContour(contour, fieldDirectory);
    }

    /// <summary>
    /// Load contour line from field directory. Supports both JSON and legacy AgOpenGPS text format.
    /// </summary>
    public ContourLine? LoadContour(string fieldDirectory)
    {
        return _contourService.LoadContour(fieldDirectory);
    }

    /// <summary>
    /// Delete a guidance line file from field directory.
    /// </summary>
    public bool DeleteGuidanceLine(string fieldDirectory, GuidanceLineType lineType)
    {
        return lineType switch
        {
            GuidanceLineType.ABLine => _abLineService.DeleteABLine(fieldDirectory),
            GuidanceLineType.CurveLine => _curveLineService.DeleteCurveLine(fieldDirectory),
            GuidanceLineType.Contour => _contourService.DeleteContour(fieldDirectory),
            _ => throw new ArgumentException($"Unknown guidance line type: {lineType}", nameof(lineType))
        };
    }
}
