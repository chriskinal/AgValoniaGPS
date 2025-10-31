﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AgOpenGPS.Classes.AgShare.Helpers;
using System.Threading.Tasks;
using System.IO;
using AgOpenGPS.Core.Translations;

namespace AgOpenGPS.Forms.Field
{
    /// <summary>
    /// Form that allows the user to preview and download their own AgShare fields,
    /// with OpenGL rendering of boundaries and AB lines.
    /// </summary>
    public partial class FormAgShareDownloader : System.Windows.Forms.Form
    {
        private readonly FormGPS gps;
        private readonly CAgShareDownloader downloader;


        public FormAgShareDownloader(FormGPS gpsContext)
        {
            InitializeComponent();
            gps = gpsContext;
            downloader = new CAgShareDownloader();
            progressBarDownloadAll.Visible = false;
            lblDownloading.Visible = false;
            chkForceOverwrite.Text = gStr.gsForceOverwrite;
            btnSaveAll.Text = gStr.gsDownloadAll;
            btnGetSelected.Text = gStr.gsGetSelected;
            lblDownloading.Text = gStr.gsDownloading;
            this.Text = gStr.gsAgShareDownloader;

        }

        // Called when the form loads: initialize OpenGL and load the list of available fields
        private async void FormAgShareDownloader_Load(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(Color.DarkSlateGray);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            glControl1.SwapBuffers();

            try
            {
                // Get user's own fields from the AgShare server
                var fields = await downloader.GetOwnFieldsAsync();

                lbFields.BeginUpdate();
                lbFields.Items.Clear();

                foreach (var field in fields)
                {
                    var item = new ListViewItem(field.Name) { Tag = field };
                    lbFields.Items.Add(item);
                }

                lbFields.EndUpdate();

                if (lbFields.Items.Count > 0)
                    lbFields.Items[0].Selected = true;
            }
            catch (Exception ex)
            {
                gps.TimedMessageBox(1000, "AgShare", "Failed to load field list.\n" + ex.Message);
            }
        }


        // Triggered when a user selects a field in the list; shows preview using OpenGL
        private async void lbFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbFields.SelectedItems.Count == 0) return;

            var dto = lbFields.SelectedItems[0].Tag as AgShareGetOwnFieldDto;
            if (dto == null) return;

            lblSelectedField.Text = "Selected Field: " + dto.Name;
            lblSelectedField.ForeColor = Color.Red;

            // Download and parse field for preview
            var previewDto = await downloader.DownloadFieldPreviewAsync(dto.Id);

            if (previewDto == null)
            {
                gps.TimedMessageBox(2000, "AgShare", "Failed to download field preview. Check logs for details.");
                return;
            }

            var localModel = AgShareFieldParser.Parse(previewDto); // Already converted to NE

            RenderField(localModel);
        }


        // Called when the user clicks the "Open" button to download the field
        private async void btnOpen_Click(object sender, EventArgs e)
        {
            if (lbFields.SelectedItems.Count == 0)
            {
                gps.TimedMessageBox(1000, "AgShare", "No field selected.");
                return;
            }

            var selected = lbFields.SelectedItems[0].Tag as AgShareGetOwnFieldDto;
            if (selected == null)
            {
                gps.TimedMessageBox(1000, "AgShare", "Invalid selection.");
                return;
            }

            // Attempt to download and save field locally
            bool success = await downloader.DownloadAndSaveAsync(selected.Id);

            if (success)
            {
                gps.TimedMessageBox(2000, "AgShare", "Field downloaded and saved.");

                // Build full path to Field.txt
                string fieldDir = Path.Combine(RegistrySettings.fieldsDirectory, selected.Name);
                string fieldFile = Path.Combine(fieldDir, "Field.txt");

                if (!File.Exists(fieldFile))
                {
                    gps.TimedMessageBox(2000, "AgShare", "Field saved but could not be opened (missing Field.txt).");
                    return;
                }

                // Close Current Field if necessary
                if (gps.isJobStarted)
                {
                    await gps.FileSaveEverythingBeforeClosingField();
                }

                gps.FileOpenField(fieldFile);
                Close();
            }
        }

        // Called when the "Download All" button is clicked
        private async void BtnDownloadAll_Click(object sender, EventArgs e)
        {
            // Disable UI during download
            lblDownloading.Visible = true;
            btnSaveAll.Enabled = false;
            btnClose.Enabled = false;
            btnGetSelected.Enabled = false;
            btnSaveAll.Enabled = false;
            chkForceOverwrite.Enabled = false;
            progressBarDownloadAll.Visible = true;
            progressBarDownloadAll.Value = 0;

            // Get list of fields to determine max for progress bar
            var fields = await downloader.GetOwnFieldsAsync();
            progressBarDownloadAll.Maximum = fields.Count;

            // Prepare progress reporting
            var progress = new Progress<int>(v =>
            {
                progressBarDownloadAll.Value = v;
                progressBarDownloadAll.Refresh();
            });

            // Use checkbox value to determine overwrite behavior
            bool forceOverwrite = chkForceOverwrite.Checked;

            // Start download
            var result = await downloader.DownloadAllAsync(forceOverwrite, progress);

            // Restore UI
            progressBarDownloadAll.Visible = false;
            lblDownloading.Visible = false;
            btnClose.Enabled = true;
            btnGetSelected.Enabled = true;
            btnSaveAll.Enabled = true;
            chkForceOverwrite.Enabled = true;

            // Show result
            string message = $"Downloaded {result.Downloaded} new field(s).";
            if (result.Skipped > 0)
            {
                message += $"\nSkipped {result.Skipped} existing.";
            }
            if (result.Failed > 0)
            {
                message += $"\nFailed {result.Failed} field(s).";
            }
            gps.TimedMessageBox(3000, "AgShare", message);
        }



        // Called when the user clicks the "Close" button
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region OpenGL Rendering

        // Draws the field boundaries and AB lines in the OpenGL context
        private void RenderField(LocalFieldModel field)
        {
            glControl1.MakeCurrent();

            // Set OpenGL background
            GL.ClearColor(0.12f, 0.12f, 0.12f, 1f); // Anthracite gray
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Enable alpha blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Determine scaling based on boundary extents, or AB lines if no boundary
            double minX, minY, maxX, maxY;
            GetBounds(field.Boundaries, field.AbLines, out minX, out minY, out maxX, out maxY);

            // Ensure non-zero margins even for vertical/horizontal lines or single points
            double marginX = Math.Max((maxX - minX) * 0.05, 50);
            double marginY = Math.Max((maxY - minY) * 0.05, 50);

            // Configure orthographic projection
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(minX - marginX, maxX + marginX, minY - marginY, maxY + marginY, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw field boundaries in lime green
            GL.Color4(0f, 1f, 0f, 0.8f);
            foreach (var ring in field.Boundaries)
            {
                GL.Begin(PrimitiveType.LineLoop);
                foreach (var pt in ring)
                    GL.Vertex2(pt.Easting, pt.Northing);
                GL.End();
            }

            // Draw AB lines and curves (dashed)
            foreach (var ab in field.AbLines)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0x0F0F);
                GL.LineWidth(3.5f);

                if (ab.CurvePoints != null && ab.CurvePoints.Count > 0)
                {
                    // Render curve (red dashed line)
                    GL.Color4(1f, 0f, 0f, 0.9f);
                    GL.Begin(PrimitiveType.LineStrip);
                    foreach (var pt in ab.CurvePoints)
                        GL.Vertex2(pt.Easting, pt.Northing);
                    GL.End();
                }
                else
                {
                    // Render AB line (orange dashed line)
                    GL.Color4(1f, 0.65f, 0f, 0.9f);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex2(ab.PtA.Easting, ab.PtA.Northing);
                    GL.Vertex2(ab.PtB.Easting, ab.PtB.Northing);
                    GL.End();
                }

                GL.Disable(EnableCap.LineStipple);
            }

            // Swap OpenGL buffers to show result
            glControl1.SwapBuffers();
        }

        // Calculates min and max coordinate extents for given field boundaries and AB lines
        private void GetBounds(List<List<LocalPoint>> boundaries, List<AbLineLocal> abLines,
            out double minX, out double minY, out double maxX, out double maxY)
        {
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;

            bool hasPoints = false;

            // Check boundaries first
            if (boundaries != null)
            {
                foreach (var ring in boundaries)
                {
                    foreach (var pt in ring)
                    {
                        if (pt.Easting < minX) minX = pt.Easting;
                        if (pt.Easting > maxX) maxX = pt.Easting;
                        if (pt.Northing < minY) minY = pt.Northing;
                        if (pt.Northing > maxY) maxY = pt.Northing;
                        hasPoints = true;
                    }
                }
            }

            // If no boundary, use AB lines to calculate bounds
            if (!hasPoints && abLines != null)
            {
                foreach (var ab in abLines)
                {
                    if (ab.CurvePoints != null && ab.CurvePoints.Count > 0)
                    {
                        foreach (var pt in ab.CurvePoints)
                        {
                            if (pt.Easting < minX) minX = pt.Easting;
                            if (pt.Easting > maxX) maxX = pt.Easting;
                            if (pt.Northing < minY) minY = pt.Northing;
                            if (pt.Northing > maxY) maxY = pt.Northing;
                            hasPoints = true;
                        }
                    }
                    else
                    {
                        // AB line (two points)
                        if (ab.PtA.Easting < minX) minX = ab.PtA.Easting;
                        if (ab.PtA.Easting > maxX) maxX = ab.PtA.Easting;
                        if (ab.PtA.Northing < minY) minY = ab.PtA.Northing;
                        if (ab.PtA.Northing > maxY) maxY = ab.PtA.Northing;

                        if (ab.PtB.Easting < minX) minX = ab.PtB.Easting;
                        if (ab.PtB.Easting > maxX) maxX = ab.PtB.Easting;
                        if (ab.PtB.Northing < minY) minY = ab.PtB.Northing;
                        if (ab.PtB.Northing > maxY) maxY = ab.PtB.Northing;
                        hasPoints = true;
                    }
                }
            }

            // Fallback to default bounds if no data
            if (!hasPoints)
            {
                minX = minY = -100;
                maxX = maxY = 100;
            }
        }

        #endregion

        private void glControl1_Load(object sender, EventArgs e)
        {

        }
    }
}
