//Please, if you use this, share the improvements

using AgLibrary.Logging;
using AgOpenGPS.Core.Models;
using AgOpenGPS.Core.Translations;
using AgOpenGPS.Forms;
using AgOpenGPS.Forms.Pickers;
using AgOpenGPS.Forms.Profiles;
using AgOpenGPS.IO;
using AgOpenGPS.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormGPS
    {
        #region Right Menu
        public bool isABCyled = false;
        private void btnContour_Click(object sender, EventArgs e)
        {
            trk.isAutoTrack = false;
            btnAutoTrack.Image = Resources.AutoTrackOff;

            ct.isContourBtnOn = !ct.isContourBtnOn;
            btnContour.Image = ct.isContourBtnOn ? Properties.Resources.ContourOn : Properties.Resources.ContourOff;

            if (ct.isContourBtnOn)
            {
                DisableYouTurnButtons();
                guidanceLookAheadTime = 0.5;
                btnContourLock.Image = Resources.ColorUnlocked;
                ct.isLocked = false;
            }

            else
            {
                EnableYouTurnButtons();
                ABLine.isABValid = false;
                curve.isCurveValid = false;
                ct.isLocked = false;
                guidanceLookAheadTime = Properties.Settings.Default.setAS_guidanceLookAheadTime;
                btnContourLock.Image = Resources.ColorUnlocked;
                if (isBtnAutoSteerOn)
                {
                    btnAutoSteer.PerformClick();
                    TimedMessageBox(2000, gStr.gsGuidanceStopped, gStr.gsContourOn);
                }

            }

            PanelUpdateRightAndBottom();
        }

        private void btnContourLock_Click(object sender, EventArgs e)
        {
            if (ct.isContourBtnOn)
            {
                ct.SetLockToLine();
            }
        }
        public void SetContourLockImage(bool isOn)
        {
            btnContourLock.Image = isOn ? Resources.ColorLocked : Resources.ColorUnlocked;
        }
        private void btnTrack_Click(object sender, EventArgs e)
        {
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            if (trk.gArr.Count > 0)
            {
                if (trk.idx == -1)
                {
                    //find index of first visible track
                    trk.idx = trk.gArr.FindIndex(track => track.isVisible);

                    //otherwise default to index 0
                    if (trk.idx == -1) trk.idx = 0;

                    EnableYouTurnButtons();
                    PanelUpdateRightAndBottom();
                    twoSecondCounter = 100;
                    return;
                }

                EnableYouTurnButtons();
                twoSecondCounter = 100;
            }

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }
            else
            {
                flp1.Visible = true;

                //build the flyout based on properties of program
                int tracksTotal = 0, tracksVisible = 0;
                bool isBnd = bnd.bndList.Count > 0;

                for (int i = 0; i < trk.gArr.Count; i++)
                {
                    tracksTotal++;
                    if (trk.gArr[i].isVisible)
                    {
                        tracksVisible++;
                    }
                }

                int btnCount = 0;
                //nudge closest
                flp1.Controls[0].Visible = tracksVisible > 0;

                //always these 3 - Build and if a bnd then ABDraw
                flp1.Controls[1].Visible = isBnd;

                flp1.Controls[2].Visible = true;
                flp1.Controls[3].Visible = true;

                //auto snap to pivot
                flp1.Controls[4].Visible = tracksVisible > 0;

                //off button
                flp1.Controls[5].Visible = tracksVisible > 0;

                //ref nudge
                flp1.Controls[6].Visible = tracksVisible > 0;

                for (int i = 0; i < flp1.Controls.Count; i++)
                {
                    if (flp1.Controls[i].Visible) btnCount++;
                }

                //position of panel
                flp1.Top = this.Height - 120 - (btnCount * 75);
                flp1.Left = this.Width - 120 - flp1.Width;
                trackMethodPanelCounter = 4;
            }

            PanelUpdateRightAndBottom();
        }
        private void btnAutoSteer_Click(object sender, EventArgs e)
        {
            longAvgPivDistance = 0;

            if (!timerSim.Enabled)
            {
                if (avgSpeed > vehicle.maxSteerSpeed)
                {
                    if (isBtnAutoSteerOn)
                    {
                        isBtnAutoSteerOn = false;
                        btnAutoSteer.Image = trk.isAutoSnapToPivot ? Properties.Resources.AutoSteerOffSnapToPivot : Properties.Resources.AutoSteerOff;
                        //if (yt.isYouTurnBtnOn) btnAutoYouTurn.PerformClick();
                        if (sounds.isSteerSoundOn) sounds.sndAutoSteerOff.Play();
                    }

                    Log.EventWriter("Steer Off, Above Max Safe Speed for Autosteer");

                    if (isMetric)
                        TimedMessageBox(3000, "AutoSteer Disabled", "Above Maximum Safe Steering Speed: " + vehicle.maxSteerSpeed.ToString("N0") + " Kmh");
                    else
                        TimedMessageBox(3000, "AutoSteer Disabled", "Above Maximum Safe Steering Speed: " + Speed.KmhToMph(vehicle.maxSteerSpeed).ToString("N1") + " MPH");

                    return;
                }
            }

            if (isBtnAutoSteerOn)
            {
                isBtnAutoSteerOn = false;
                btnAutoSteer.Image = trk.isAutoSnapToPivot ? Properties.Resources.AutoSteerOffSnapToPivot : Properties.Resources.AutoSteerOff;
                //if (yt.isYouTurnBtnOn) btnAutoYouTurn.PerformClick();
                if (sounds.isSteerSoundOn) sounds.sndAutoSteerOff.Play();
            }
            else
            {
                if (ct.isContourBtnOn | trk.idx > -1)
                {
                    isBtnAutoSteerOn = true;
                    btnAutoSteer.Image = trk.isAutoSnapToPivot ? Properties.Resources.AutoSteerOnSnapToPivot : Properties.Resources.AutoSteerOn;
                    if (sounds.isSteerSoundOn) sounds.sndAutoSteerOn.Play();

                    //redraw uturn if btn enabled.
                    if (yt.isYouTurnBtnOn)
                    {
                        yt.ResetYouTurn();
                    }
                }
                else
                {
                    TimedMessageBox(2000, (gStr.gsNoGuidanceLines), (gStr.gsTurnOnContourOrMakeABLine));
                }
            }
        }

        private void btnAutoYouTurn_Click(object sender, EventArgs e)
        {
            yt.isTurnCreationTooClose = false;

            if (bnd.bndList.Count == 0)
            {
                TimedMessageBox(2000, gStr.gsNoBoundary, gStr.gsCreateABoundaryFirst);
                Log.EventWriter("Uturn attempted without boundary");
                return;
            }

            yt.turnTooCloseTrigger = false;

            if (!yt.isYouTurnBtnOn)
            {
                //new direction so reset where to put turn diagnostic
                yt.ResetCreatedYouTurn();

                if (trk.idx == -1) return;

                yt.isYouTurnBtnOn = true;
                yt.isTurnCreationTooClose = false;
                yt.isTurnCreationNotCrossingError = false;
                yt.ResetYouTurn();
                btnAutoYouTurn.Image = Properties.Resources.Youturn80;
            }
            else
            {
                yt.isYouTurnBtnOn = false;
                //yt.rowSkipsWidth = Properties.Settings.Default.set_youSkipWidth;
                //yt.Set_Alternate_skips();

                btnAutoYouTurn.Image = Properties.Resources.YouTurnNo;
                yt.ResetYouTurn();

                //new direction so reset where to put turn diagnostic
                yt.ResetCreatedYouTurn();
            }
        }
        private void btnCycleLines_Click(object sender, EventArgs e)
        {
            trk.isAutoTrack = false;
            btnAutoTrack.Image = Resources.AutoTrackOff;

            if (trk.gArr.Count > 1)
            {
                while (true)
                {
                    trk.idx++;
                    if (trk.idx == trk.gArr.Count) trk.idx = 0;

                    if (trk.gArr[trk.idx].isVisible)
                    {
                        guideLineCounter = 20;
                        lblGuidanceLine.Visible = true;
                        lblGuidanceLine.Text = trk.gArr[trk.idx].name;
                        break;
                    }
                }

                if (isBtnAutoSteerOn)
                {
                    btnAutoSteer.PerformClick();
                    TimedMessageBox(2000, gStr.gsGuidanceStopped, "Track Changed");
                }

                if (yt.isYouTurnBtnOn) btnAutoYouTurn.PerformClick();

                lblNumCu.Text = (trk.idx + 1).ToString() + "/" + trk.gArr.Count.ToString();
            }

            twoSecondCounter = 100;

            ABLine.isABValid = false;
            curve.isCurveValid = false;
        }

        private void btnCycleLinesBk_Click(object sender, EventArgs e)
        {
            if (ct.isContourBtnOn)
            {
                ct.SetLockToLine();
                return;
            }

            trk.isAutoTrack = false;
            btnAutoTrack.Image = Resources.AutoTrackOff;

            if (trk.gArr.Count > 1)
            {
                while (true)
                {
                    trk.idx--;
                    if (trk.idx == -1) trk.idx = trk.gArr.Count - 1;

                    if (trk.gArr[trk.idx].isVisible)
                    {
                        guideLineCounter = 20;
                        lblGuidanceLine.Visible = true;
                        lblGuidanceLine.Text = trk.gArr[trk.idx].name;
                        break;
                    }
                }

                if (isBtnAutoSteerOn)
                {
                    btnAutoSteer.PerformClick();
                    TimedMessageBox(2000, gStr.gsGuidanceStopped, "Track Changed");
                }

                lblNumCu.Text = (trk.idx + 1).ToString() + "/" + trk.gArr.Count.ToString();
            }

            ABLine.isABValid = false;
            curve.isCurveValid = false;

            twoSecondCounter = 100;
        }

        #endregion

        #region Track Flyout

        private void btnRefNudge_Click(object sender, EventArgs e)
        {
            Form fcc = Application.OpenForms["FormNudge"];

            if (fcc != null)
            {
                fcc.Focus();
                TimedMessageBox(2000, "Nudge Window Open", "Close Nudge Window");
                return;
            }


            if (trk.idx > -1)
            {
                Form form = new FormRefNudge(this);
                form.Show(this);
            }
            else
            {
                TimedMessageBox(1500, gStr.gsNoABLineActive, gStr.gsPleaseEnterABLine);
                return;
            }
            if (flp1.Visible)
            {
                flp1.Visible = false;
            }

            panelRight.Visible = false;

            this.Activate();
        }
        private void btnTracksOff_Click(object sender, EventArgs e)
        {
            trk.idx = -1;

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }
            PanelUpdateRightAndBottom();
        }
        private void btnNudge_Click(object sender, EventArgs e)
        {
            Form fcc = Application.OpenForms["FormNudge"];

            if (fcc != null)
            {
                fcc.Focus();
                return;
            }

            if (trk.idx > -1)
            {
                Form form = new FormNudge(this);
                form.Show(this);
            }
            else
            {
                TimedMessageBox(1500, gStr.gsNoABLineActive, gStr.gsPleaseEnterABLine);
                return;
            }

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }

            this.Activate();

        }
        private void btnBuildTracks_Click(object sender, EventArgs e)
        {
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            //check if window already exists
            Form fc = Application.OpenForms["FormBuildTracks"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            Form form = new FormBuildTracks(this);
            form.Show(this);

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }
        }

        private void btnPlusAB_Click(object sender, EventArgs e)
        {
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            //check if window already exists
            Form fc = Application.OpenForms["FormQuickAB"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            Form form = new FormQuickAB(this);
            form.Show(this);

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }
            this.Activate();
        }

        private void btnABDraw_Click(object sender, EventArgs e)
        {
            if (bnd.bndList.Count == 0)
            {
                TimedMessageBox(2000, gStr.gsNoBoundary, gStr.gsCreateABoundaryFirst);
                return;
            }

            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }

            if (flp1.Visible)
            {
                flp1.Visible = false;
            }

            using (var form = new FormABDraw(this))
            {
                form.ShowDialog(this);
            }

            PanelUpdateRightAndBottom();
        }
        private void cboxAutoSnapToPivot_Click(object sender, EventArgs e)
        {
            trk.isAutoSnapToPivot = cboxAutoSnapToPivot.Checked;
            trackMethodPanelCounter = 1;

            //show the correct icon variant on the AutoSteer button
            if (isBtnAutoSteerOn)
                btnAutoSteer.Image = trk.isAutoSnapToPivot ? Properties.Resources.AutoSteerOnSnapToPivot : Properties.Resources.AutoSteerOn;
            else
                btnAutoSteer.Image = trk.isAutoSnapToPivot ? Properties.Resources.AutoSteerOffSnapToPivot : Properties.Resources.AutoSteerOff;
        }
        #endregion

        #region Field Menu
        private void toolStripBtnFieldTools_Click(object sender, EventArgs e)
        {
            headlandToolStripMenuItem.Enabled = (bnd.bndList.Count > 0);
            headlandBuildToolStripMenuItem.Enabled = (bnd.bndList.Count > 0);

            tramLinesMenuField.Enabled = tramsMultiMenuField.Enabled = (trk.gArr.Count > 0 && trk.idx > -1);
        }

        public bool isCancelJobMenu;

        private void btnJobMenu_Click(object sender, EventArgs e)
        {
            // Remember current state before opening the Job dialog
            var wasJobStarted = isJobStarted;
            var prevFieldDir = currentFieldDirectory;

            Form f = Application.OpenForms["FormGPSData"];
            if (f != null)

            {
                f.Focus();
                f.Close();
            }

            f = Application.OpenForms["FormFieldData"];
            if (f != null)

            {
                f.Focus();
                f.Close();
            }

            f = Application.OpenForms["FormEventViewer"];
            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            f = Application.OpenForms["FormPan"];
            if (f != null)
            {
                isPanFormVisible = false;
                f.Focus();
                f.Close();
            }

            if (this.OwnedForms.Any())
            {
                TimedMessageBox(2000, gStr.gsWindowsStillOpen, gStr.gsCloseAllWindowsFirst);
                return;
            }

            using (var form = new FormJob(this))
            {
                var result = form.ShowDialog(this);

                if (isCancelJobMenu)
                {
                    isCancelJobMenu = false;
                    return;
                }

                if (isJobStarted)
                {
                    if (autoBtnState == btnStates.Auto) btnSectionMasterAuto.PerformClick();
                    if (manualBtnState == btnStates.On) btnSectionMasterManual.PerformClick();
                }

                if (result == DialogResult.Yes)
                {
                    using (var form2 = new FormFieldDir(this)) { form2.ShowDialog(this); }
                }
                else if (result == DialogResult.No)
                {
                    using (var form2 = new FormFieldKML(this)) { form2.ShowDialog(this); }
                }
                else if (result == DialogResult.Retry)
                {
                    using (var form2 = new FormFieldExisting(this)) { form2.ShowDialog(this); }
                }
                else if (result == DialogResult.Abort)
                {
                    using (var form2 = new FormFieldIsoXml(this)) { form2.ShowDialog(this); }
                }

                // ---- Only log "Opened" if a field was newly opened or changed ----
                bool openedNewOrChanged =
                    isJobStarted &&
                    (!wasJobStarted ||
                     !string.Equals(currentFieldDirectory, prevFieldDir, StringComparison.OrdinalIgnoreCase));

                if (openedNewOrChanged)
                {
                    double distance = AppModel.CurrentLatLon.DistanceInKiloMeters(AppModel.LocalPlane.Origin);
                    if (distance > 10)
                    {
                        TimedMessageBox(2500, "High Field Start Distance Warning",
                            "Field Start is " + distance.ToString("N1") + " km From current position");
                        Log.EventWriter("High Field Start Distance Warning");
                    }

                    Log.EventWriter("** Opened **  " + currentFieldDirectory + "   " +
                        DateTime.Now.ToString("f", CultureInfo.InvariantCulture));

                    Settings.Default.setF_CurrentDir = currentFieldDirectory;
                    Settings.Default.Save();
                }
            }

            FieldMenuButtonEnableDisable(isJobStarted);
            toolStripBtnFieldTools.Enabled = isJobStarted;
            bnd.isHeadlandOn = (bnd.bndList.Count > 0 && bnd.bndList[0].hdLine.Count > 0);
            trk.idx = -1;
            PanelUpdateRightAndBottom();
        }

        public async Task FileSaveEverythingBeforeClosingField()
        {
            // Save the current field data before closing
            if (ct.isContourOn) ct.StopContourLine();

            if (autoBtnState == btnStates.Auto)
                btnSectionMasterAuto.PerformClick();

            if (manualBtnState == btnStates.On)
                btnSectionMasterManual.PerformClick();

            for (int j = 0; j < tool.numOfSections; j++)
            {
                section[j].sectionOnOffCycle = false;
                section[j].sectionOffRequest = false;
            }

            for (int j = 0; j < triStrip.Count; j++)
            {
                if (triStrip[j].isDrawing) triStrip[j].TurnMappingOff();
            }

            // Start AgShare upload (if enabled)
            agShareUploadTask = Task.CompletedTask;
            if (!isAgShareUploadStarted &&
                Settings.Default.AgShareEnabled &&
                Settings.Default.AgShareUploadActive)
            {
                try
                {
                    isAgShareUploadStarted = true;
                    agShareUploadTask = CAgShareUploader.UploadAsync(snapshot, agShareClient, this);
                }
                catch (Exception ex)
                {
                    Log.EventWriter("AgShare upload start error: " + ex.Message);
                    TimedMessageBox(4000, "AgShare upload failed", "An error occurred while starting upload to AgShare.");
                }
            }

            // Save field data with individual exception handling for each operation
            await Task.Run(() =>
            {
                try { FileSaveBoundary(); }
                catch (Exception ex) { Log.EventWriter($"CRITICAL: Boundary save failed: {ex}"); throw; }

                try { FileSaveSections(); }
                catch (Exception ex) { Log.EventWriter($"CRITICAL: Sections save failed: {ex}"); throw; }

                try { FileSaveContour(); }
                catch (Exception ex) { Log.EventWriter($"CRITICAL: Contour save failed: {ex}"); throw; }

                try { FileSaveTracks(); }
                catch (Exception ex) { Log.EventWriter($"CRITICAL: Tracks save failed: {ex}"); throw; }

                try { ExportFieldAs_KML(); }
                catch (Exception ex) { Log.EventWriter($"WARNING: KML export failed: {ex}"); }

                //ExportFieldAs_ISOXMLv3(); NOTE: This is very very slow, commented out until we have a field exporter

                try { ExportFieldAs_ISOXMLv4(); }
                catch (Exception ex) { Log.EventWriter($"WARNING: ISOXML export failed: {ex}"); }
            });

            if (Settings.Default.AgShareEnabled && Settings.Default.AgShareUploadActive)
            {
                try
                {
                    await agShareUploadTask;
                }
                catch (Exception ex)
                {
                    Log.EventWriter("AgShare upload error: " + ex.Message);
                    TimedMessageBox(4000, "AgShare upload failed", "An error occurred during upload to AgShare.");
                }
            }

            Log.EventWriter("** Field closed **   " + currentFieldDirectory + "   " +
                DateTime.Now.ToString("f", CultureInfo.InvariantCulture));

            this.Invoke((MethodInvoker)(() =>
            {
                panelRight.Enabled = false;
                FieldMenuButtonEnableDisable(false);
                JobClose();
                Text = "AgOpenGPS";
            }));
        }

        #region AgShare Snapshot

        private bool isAgShareUploadStarted = false;
        private FieldSnapshot snapshot;

        //this method is called to create a snapshot of the field for AgShare so we can close the field to speed up close and re-open
        public void AgShareSnapshot()
        {
            if (!isJobStarted) return;

            snapshot = CAgShareUploader.CreateSnapshot(this);
        }



        public void AgShareUpload()
        {
            //check if we're already uploading by closing a field or are we shutting down
            if (!isJobStarted || snapshot == null || isAgShareUploadStarted || isShuttingDown)
                return;

            //set bool to true so we don't start another upload by double clicking or something.
            isAgShareUploadStarted = true;
            agShareUploadTask = CAgShareUploader.UploadAsync(snapshot, agShareClient, this);
        }
        #endregion
        private void tramLinesMenuField_Click(object sender, EventArgs e)
        {
            if (ct.isContourBtnOn) btnContour.PerformClick();

            if (trk.idx == -1)
            {
                TimedMessageBox(1500, gStr.gsNoABLineActive, gStr.gsPleaseEnterABLine);
                panelRight.Enabled = true;
                return;
            }

            Form form99 = new FormTram(this, trk.gArr[trk.idx].mode != TrackMode.AB);
            form99.Show(this);

        }

        private void tramLinesMenuMulti_Click(object sender, EventArgs e)
        {
            if (ct.isContourBtnOn) btnContour.PerformClick();

            if (trk.gArr.Count < 1)
            {
                TimedMessageBox(1500, gStr.gsNoGuidanceLines, gStr.gsNoGuidanceLines);
                panelRight.Enabled = true;
                return;
            }
            if (bnd.bndList.Count < 1)
            {
                TimedMessageBox(1500, gStr.gsNoBoundary, gStr.gsCreateABoundaryFirst);
                panelRight.Enabled = true;
                return;
            }

            Form form99 = new FormTramLine(this);
            form99.ShowDialog(this);
        }

        public void GetHeadland()
        {
            using (var form = new FormHeadLine(this))
            {
                form.ShowDialog(this);
            }

            bnd.isHeadlandOn = (bnd.bndList.Count > 0 && bnd.bndList[0].hdLine.Count > 0);

            PanelsAndOGLSize();
            PanelUpdateRightAndBottom();
            SetZoom();
        }
        private void headlandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bnd.bndList.Count == 0)
            {
                TimedMessageBox(2000, gStr.gsNoBoundary, gStr.gsCreateABoundaryFirst);
                return;
            }

            GetHeadland();
        }
        private void headlandBuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bnd.bndList.Count == 0)
            {
                TimedMessageBox(2000, gStr.gsNoBoundary, gStr.gsCreateABoundaryFirst);
                return;
            }

            using (var form = new FormHeadAche(this))
            {
                form.ShowDialog(this);
            }

            bnd.isHeadlandOn = (bnd.bndList.Count > 0 && bnd.bndList[0].hdLine.Count > 0);

            PanelsAndOGLSize();
            PanelUpdateRightAndBottom();
            SetZoom();
        }
        private void boundariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isJobStarted) return;

            using (var boundaryForm = new FormBoundary(this))
            {
                var result = boundaryForm.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    var boundaryPlayer = new FormBoundaryPlayer(this);
                    boundaryPlayer.FormClosed += (s, args) => toolStripBtnFieldTools.Enabled = true;
                    toolStripBtnFieldTools.Enabled = false;
                    boundaryPlayer.Show(this);
                }
                else if (result == DialogResult.Yes)
                {
                    new FormMap(this).Show(this);
                }
            }

            PanelUpdateRightAndBottom();
        }

        #endregion

        #region Recorded Path
        private void btnPathGoStop_Click(object sender, EventArgs e)
        {
            #region Turn off Guidance
            //if contour is on, turn it off
            if (ct.isContourBtnOn) { if (ct.isContourBtnOn) btnContour.PerformClick(); }
            //btnContourPriority.Enabled = true;

            if (yt.isYouTurnBtnOn) btnAutoYouTurn.PerformClick();
            if (isBtnAutoSteerOn)
            {
                btnAutoSteer.PerformClick();
                TimedMessageBox(2000, gStr.gsGuidanceStopped, "Paths Enabled");
                Log.EventWriter("Autosteer On While Enable Paths");
            }

            DisableYouTurnButtons();

            if (trk.idx > -1)
            {
                trk.idx = -1;
            }

            PanelUpdateRightAndBottom();

            #endregion

            //already running?
            if (recPath.isDrivingRecordedPath)
            {
                recPath.StopDrivingRecordedPath();
                btnPathGoStop.Image = Properties.Resources.boundaryPlay;
                btnPathRecordStop.Enabled = true;
                btnPickPath.Enabled = true;
                btnResumePath.Enabled = true;
                return;
            }

            //start the recorded path driving process
            if (!recPath.StartDrivingRecordedPath())
            {
                //Cancel the recPath - something went seriously wrong
                recPath.StopDrivingRecordedPath();
                TimedMessageBox(1500, gStr.gsProblemMakingPath, gStr.gsCouldntGenerateValidPath);
                btnPathGoStop.Image = Properties.Resources.boundaryPlay;
                btnPathRecordStop.Enabled = true;
                btnPickPath.Enabled = true;
                btnResumePath.Enabled = true;
                return;
            }
            else
            {
                btnPathGoStop.Image = Properties.Resources.boundaryStop;
                btnPathRecordStop.Enabled = false;
                btnPickPath.Enabled = false;
                btnResumePath.Enabled = false;
            }
        }
        private void btnPathRecordStop_Click(object sender, EventArgs e)
        {
            if (recPath.isRecordOn)
            {
                recPath.isRecordOn = false;
                btnPathRecordStop.Image = Properties.Resources.BoundaryRecord;
                btnPathGoStop.Enabled = true;
                btnPickPath.Enabled = true;
                btnResumePath.Enabled = true;

                using (var form = new FormRecordName(this))
                {
                    form.ShowDialog(this);
                    if (form.DialogResult == DialogResult.OK)
                    {
                        String filename = form.filename + ".rec";
                        FileSaveRecPath();
                        FileSaveRecPath(filename);
                    }
                    else
                    {
                        recPath.recList.Clear();
                    }
                }
            }
            else if (isJobStarted)
            {
                recPath.recList.Clear();
                recPath.isRecordOn = true;
                btnPathRecordStop.Image = Properties.Resources.boundaryStop;
                btnPathGoStop.Enabled = false;
                btnPickPath.Enabled = false;
                btnResumePath.Enabled = false;
            }
        }
        private void btnResumePath_Click(object sender, EventArgs e)
        {
            if (recPath.resumeState == 0)
            {
                recPath.resumeState++;
                btnResumePath.Image = Properties.Resources.pathResumeLast;
                TimedMessageBox(1500, "Resume Style", "Last Stopped Position");
            }

            else if (recPath.resumeState == 1)
            {
                recPath.resumeState++;
                btnResumePath.Image = Properties.Resources.pathResumeClose;
                TimedMessageBox(1500, "Resume Style", "Closest Point");
            }
            else
            {
                recPath.resumeState = 0;
                btnResumePath.Image = Properties.Resources.pathResumeStart;
                TimedMessageBox(1500, "Resume Style", "Start At Beginning");
            }
        }
        private void btnSwapABRecordedPath_Click(object sender, EventArgs e)
        {
            int cnt = recPath.recList.Count;
            List<CRecPathPt> _recList = new List<CRecPathPt>();

            for (int i = cnt - 1; i > -1; i--)
            {
                recPath.recList[i].heading += (glm.PIBy2) + (glm.PIBy2);
                if (recPath.recList[i].heading < -glm.twoPI) recPath.recList[i].heading += glm.twoPI;

                _recList.Add(recPath.recList[i]);
            }
            recPath.recList.Clear();
            for (int i = 0; i < cnt; i++)
            {
                recPath.recList.Add(_recList[i]);
            }
        }
        private void btnPickPath_Click(object sender, EventArgs e)
        {
            recPath.resumeState = 0;
            btnResumePath.Image = Properties.Resources.pathResumeStart;
            recPath.currentPositonIndex = 0;

            using (FormRecordPicker form = new FormRecordPicker(this))
            {
                //returns full field.txt file dir name
                if (form.ShowDialog(this) == DialogResult.Yes)
                {
                }
            }
        }
        private void recordedPathStripMenu_Click(object sender, EventArgs e)
        {
            recPath.resumeState = 0;
            btnResumePath.Image = Properties.Resources.pathResumeStart;
            recPath.currentPositonIndex = 0;

            if (isJobStarted)
            {
                if (panelDrag.Visible)
                {
                    panelDrag.Visible = false;
                    recPath.recList.Clear();
                    recPath.StopDrivingRecordedPath();
                }
                else
                {
                    FileLoadRecPath();
                    panelDrag.Visible = true;
                }
            }
            else
            {
                TimedMessageBox(3000, gStr.gsFieldNotOpen, gStr.gsStartNewField);
            }
        }

        #endregion

        #region Left Panel Menu
        private void steerWizardMenuItem_Click(object sender, EventArgs e)
        {
            Form fcs = Application.OpenForms["FormSteer"];

            if (fcs != null)
            {
                fcs.Focus();
                fcs.Close();
            }

            //check if window already exists
            Form fc = Application.OpenForms["FormSteerWiz"];

            if (fc != null)
            {
                fc.Focus();
                //fc.Close();
                return;
            }

            //
            Form form = new FormSteerWiz(this);
            form.Show(this);

        }
        private void toolStripDropDownButtonDistance_Click(object sender, EventArgs e)
        {
            fd.distanceUser = 0;
        }
        private void btnNavigationSettings_Click(object sender, EventArgs e)
        {
            //buttonPanelCounter = 0;
            Form f = Application.OpenForms["FormGPSData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            Form f1 = Application.OpenForms["FormFieldData"];

            if (f1 != null)
            {
                f1.Focus();
                f1.Close();
            }

            panelNavigation.Location = new System.Drawing.Point(90, 100);

            if (panelNavigation.Visible)
            {
                panelNavigation.Visible = false;
            }
            else
            {
                panelNavigation.Visible = true;
                navPanelCounter = 2;
                if (displayBrightness.isWmiMonitor) btnBrightnessDn.Text = (displayBrightness.GetBrightness().ToString()) + "%";
                else btnBrightnessDn.Text = "??";
            }

            if (isJobStarted) btnGrid.Enabled = true;
            else btnGrid.Enabled = false;

        }
        private void btnStartAgIO_Click(object sender, EventArgs e)
        {
            Log.EventWriter("AgIO Manually Started");

            Process[] processName = Process.GetProcessesByName("AgIO");
            if (processName.Length == 0)
            {
                //Start application here
                string strPath = Path.Combine(Application.StartupPath, "AgIO.exe");

                try
                {
                    //TimedMessageBox(2000, "Please Wait", "Starting AgIO");
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = strPath;
                    //processInfo.ErrorDialog = true;
                    //processInfo.UseShellExecute = false;
                    processInfo.WorkingDirectory = Path.GetDirectoryName(strPath);
                    Process proc = Process.Start(processInfo);
                }
                catch
                {
                    TimedMessageBox(2000, "No File Found", "Can't Find AgIO");
                    Log.EventWriter("AgIO Not Found");

                }
            }
            else
            {
                //Set foreground window
                ShowWindow(processName[0].MainWindowHandle, 9);
                SetForegroundWindow(processName[0].MainWindowHandle);
            }
        }
        private void btnAutoSteerConfig_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fc = Application.OpenForms["FormSteer"];

            if (fc != null)
            {
                fc.Focus();
                fc.Close();

                return;
            }

            //
            Form form = new FormSteer(this);
            //form.Top = 0;
            //form.Left = 0;
            form.Show(this);
            this.Activate();

        }
        private void btnConfig_Click(object sender, EventArgs e)
        {
            using (FormConfig form = new FormConfig(this))
            {
                form.ShowDialog(this);
            }
        }

        #endregion

        #region Flags
        private void toolStripMenuItemFlagRed_Click(object sender, EventArgs e)
        {
            flagColor = 0;
            btnFlag.Image = Properties.Resources.FlagRed;
        }
        private void toolStripMenuGrn_Click(object sender, EventArgs e)
        {
            flagColor = 1;
            btnFlag.Image = Properties.Resources.FlagGrn;
        }
        private void toolStripMenuYel_Click(object sender, EventArgs e)
        {
            flagColor = 2;
            btnFlag.Image = Properties.Resources.FlagYel;
        }
        private void toolStripMenuFlagForm_Click(object sender, EventArgs e)
        {
            Form fc = Application.OpenForms["FormFlags"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            if (flagPts.Count > 0)
            {
                flagNumberPicked = 1;
                Form form = new FormFlags(this);
                form.Show(this);
            }
        }
        private void btnFlag_Click(object sender, EventArgs e)
        {
            int nextflag = flagPts.Count + 1;
            CFlag flagPt = new CFlag(
                AppModel.CurrentLatLon.Latitude,
                AppModel.CurrentLatLon.Longitude,
                pn.fix.easting, pn.fix.northing,
                fixHeading, flagColor, nextflag, nextflag.ToString());
            flagPts.Add(flagPt);
            flagPts = FlagsFiles.DeduplicateFlags(flagPts);
            FileSaveFlags();

            Form fc = Application.OpenForms["FormFlags"];

            if (fc != null)
            {
                fc.Focus();
                return;
            }

            if (flagPts.Count > 0)
            {
                flagNumberPicked = nextflag;
                Form form = new FormFlags(this);
                form.Show(this);
            }
        }

        private void btnSnapToPivot_Click(object sender, EventArgs e)
        {
            trk.SnapToPivot();
        }

        private void btnAdjRight_Click(object sender, EventArgs e)
        {
            trk.NudgeTrack(Properties.Settings.Default.setAS_snapDistance * 0.01);
        }

        private void btnAdjLeft_Click(object sender, EventArgs e)
        {
            trk.NudgeTrack(-Properties.Settings.Default.setAS_snapDistance * 0.01);
        }

        #endregion

        #region Top Panel
        private void btnFieldStats_Click(object sender, EventArgs e)
        {
            Form f = Application.OpenForms["FormGPSData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            f = null;
            f = Application.OpenForms["FormFieldData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
                return;
            }

            if (!isJobStarted) return;

            Form form = new FormFieldData(this);
            form.Show(this);

            form.Top = this.Top + this.Height / 2 - GPSDataWindowTopOffset;
            if (isPanelBottomHidden)
                form.Left = this.Left + 5;
            else
                form.Left = this.Left + GPSDataWindowLeft + 5;


            Form ff = Application.OpenForms["FormGPS"];
            ff.Focus();

            btnAutoSteerConfig.Focus();
        }

        private void btnGPSData_Click(object sender, EventArgs e)
        {
            Form f = Application.OpenForms["FormGPSData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
                return;
            }

            f = null;
            f = Application.OpenForms["FormFieldData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            Form form = new FormGPSData(this);
            form.Show(this);

            form.Top = this.Top + this.Height / 2 - GPSDataWindowTopOffset;
            if (isPanelBottomHidden)
                form.Left = this.Left + 5;
            else
                form.Left = this.Left + GPSDataWindowLeft + 5;

            Form ff = Application.OpenForms["FormGPS"];
            ff.Focus();
        }
        private void btnShutdown_Click(object sender, EventArgs e)
        {
            Form f = Application.OpenForms["FormGPSData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            f = null;
            f = Application.OpenForms["FormFieldData"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            f = null;
            f = Application.OpenForms["FormEventViewer"];

            if (f != null)
            {
                f.Focus();
                f.Close();
            }

            f = null;
            f = Application.OpenForms["FormPan"];

            if (f != null)
            {
                isPanFormVisible = false;
                f.Focus();
                f.Close();
            }

            Close();
        }
        private void btnMinimizeMainForm_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void btnMaximizeMainForm_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else this.WindowState = FormWindowState.Maximized;

            FormGPS_ResizeEnd(this, e);
        }
        private void lblCurrentField_Click(object sender, EventArgs e)
        {
            isPauseFieldTextCounter = !isPauseFieldTextCounter;
            if (isPauseFieldTextCounter)
            {
                //lblCurrentField.Text = "\u23F8";
                fourSecondCounter = 4;
            }
            else
            {
                fourSecondCounter = 4;
            }
        }

        #endregion

        #region File Menu

        //File drop down items

        private void flagByLatLonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormEnterFlag(this))
            {
                form.ShowDialog(this);
                this.Activate();
            }
        }
        private void setWorkingDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                // Show timed message if a job is still open
                TimedMessageBox(2000, gStr.gsFieldIsOpen, gStr.gsCloseFieldFirst);
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Currently: " + RegistrySettings.workingDirectory
            };

            if (RegistrySettings.workingDirectory == RegistrySettings.defaultString)
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                fbd.SelectedPath = RegistrySettings.workingDirectory;

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                // Save new working directory to registry
                RegistrySettings.Save(RegKeys.workingDirectory, fbd.SelectedPath);

                // Inform user that app needs to restart
                FormDialog.Show("Restart Required",
                    gStr.gsProgramWillExitPleaseRestart,
                    MessageBoxButtons.OK);

                // Close the app
                Close();
            }
        }

        private void enterSimCoordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormSimCoords(this))
            {
                form.ShowDialog(this);
            }
        }

        private void AgShareApiMenuItem_Click(object sender, EventArgs e)
        {
            var server = Properties.Settings.Default.AgShareServer;
            var apiKey = Properties.Settings.Default.AgShareApiKey;
            var client = new AgShareClient(server, apiKey);

            var form = new FormAgShareSettings();
            form.ShowDialog(this);
        }


        private void hotKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new Form_Keys(this))
            {
                form.ShowDialog(this);
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new Form_About())
            {
                form.ShowDialog(this);
            }
        }
        private void kioskModeToolStrip_Click(object sender, EventArgs e)
        {
            isKioskMode = !isKioskMode;

            if (isKioskMode)
            {
                kioskModeToolStrip.Checked = true;
                this.WindowState = FormWindowState.Maximized;
                isFullScreen = true;
                btnMaximizeMainForm.Visible = false;
                btnMinimizeMainForm.Visible = false;
                Settings.Default.setWindow_isKioskMode = true;
            }
            else
            {
                kioskModeToolStrip.Checked = false;
                btnMaximizeMainForm.Visible = true;
                btnMinimizeMainForm.Visible = true;
                Settings.Default.setWindow_isKioskMode = false;
            }

            Settings.Default.Save();
        }
        private void resetALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                // Show message if field is still open
                FormDialog.Show("Warning", gStr.gsCloseFieldFirst, MessageBoxButtons.OK);
            }
            else
            {
                // Ask user for confirmation before resetting everything
                DialogResult result2 = FormDialog.Show(gStr.gsResetAll, gStr.gsReallyResetEverything, MessageBoxButtons.YesNoCancel);

                if (result2 == DialogResult.OK)
                {
                    // Reset registry settings
                    RegistrySettings.Reset();

                    // Notify user and close app
                    FormDialog.Show("Restart Required", gStr.gsProgramWillExitPleaseRestart, MessageBoxButtons.OK);
                    Close();
                }
            }
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new Form_Help(this))
            {
                form.ShowDialog(this);
            }
        }
        private void simulatorOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                TimedMessageBox(2000, gStr.gsFieldIsOpen, gStr.gsCloseFieldFirst);
                return;
            }
            if (simulatorOnToolStripMenuItem.Checked)
            {
                if (sentenceCounter < 299)
                {
                    TimedMessageBox(2000, "Connected", "GPS");
                    simulatorOnToolStripMenuItem.Checked = false;
                    return;
                }
            }

            timerSim.Enabled = panelSim.Visible = simulatorOnToolStripMenuItem.Checked;
            isFirstFixPositionSet = false;
            isGPSPositionInitialized = false;
            isFirstHeadingSet = false;
            startCounter = 0;

            Settings.Default.setMenu_isSimulatorOn = simulatorOnToolStripMenuItem.Checked;
            Settings.Default.Save();
        }
        private void colorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormColor(this))
            {
                form.ShowDialog(this);
            }
            Settings.Default.Save();
        }
        private void colorsSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tool.isSectionsNotZones)
            {
                using (var form = new FormColorSection(this))
                {
                    form.ShowDialog(this);
                }
                Settings.Default.Save();
            }
            else
            {
                TimedMessageBox(2000, "Cannot use with zones", "Only for Sections");
            }
        }

        //Profiles
        private void newProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormNewProfile(this))
            {
                form.ShowDialog(this);
            }
        }

        private void loadProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormLoadProfile(this))
            {
                form.ShowDialog(this);
            }
        }

        #endregion

        #region Languages
        private void InitializeLanguages()
        {
            menustripLanguage.DropDownItems.Clear();
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Čeština (Czech)", "cs"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Dansk (Denmark)", "da"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Deutsch (Germany)", "de"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("English (Canada)", "en"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Eesti (Estonia)", "et"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Español (Spanish)", "es"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Français (France)", "fr"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Italiano (Italy)", "it"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Latviski (Latvia)", "lv"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Lietuvių (Lithuania)", "lt"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Magyar (Hungary)", "hu"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Nederlands (Holland)", "nl"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Norsk (Norway)", "no"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Polski (Poland)", "pl"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Português (Portuguese)", "pt"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Română (Romanian)", "ro"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("русский (Russia)", "ru"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Suomalainen (Finland)", "fi"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Slovenčina (Slovakia)", "sk"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Serbia (Servië)", "sr"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Türkçe (Turkey)", "tr"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("Yкраїнська (Ukraine)", "uk"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("中国人 (Chinese)", "zh-CHS"));
            menustripLanguage.DropDownItems.Add(CreateLanguageMenuItem("한국인 (Korean)", "ko"));
        }

        private ToolStripMenuItem CreateLanguageMenuItem(string text, string lang)
        {
            var menuItem = new ToolStripMenuItem()
            {
                CheckOnClick = true,
                Text = text,
                Tag = lang
            };
            menuItem.Click += languageMenuItem_Click;
            return menuItem;
        }

        private void languageMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            SetLanguage((string)menuItem.Tag);
        }

        private void SetLanguage(string lang)
        {
            foreach (var menuItem in menustripLanguage.DropDownItems.OfType<ToolStripMenuItem>())
            {
                menuItem.Checked = (string)menuItem.Tag == lang;
            }

            RegistrySettings.Save(RegKeys.language, lang);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

            LoadText();
        }

        #endregion

        #region Bottom Menu
        public void CloseTopMosts()
        {
            Form fc = Application.OpenForms["FormSteer"];

            if (fc != null)
            {
                fc.Focus();
                fc.Close();
            }

            fc = Application.OpenForms["FormSteerGraph"];

            if (fc != null)
            {
                fc.Focus();
                fc.Close();
            }

            fc = Application.OpenForms["FormGPSData"];

            if (fc != null)
            {
                fc.Focus();
                fc.Close();
            }

        }
        private void btnAutoTrack_Click(object sender, EventArgs e)
        {
            trk.isAutoTrack = !trk.isAutoTrack;
            btnAutoTrack.Image = trk.isAutoTrack ? Resources.AutoTrack : Resources.AutoTrackOff;
        }
        private void btnResetToolHeading_Click(object sender, EventArgs e)
        {
            tankPos.heading = fixHeading;
            tankPos.easting = hitchPos.easting + (Math.Sin(tankPos.heading) * (tool.tankTrailingHitchLength));
            tankPos.northing = hitchPos.northing + (Math.Cos(tankPos.heading) * (tool.tankTrailingHitchLength));

            toolPivotPos.heading = tankPos.heading;
            toolPivotPos.easting = tankPos.easting + (Math.Sin(toolPivotPos.heading) * (tool.trailingHitchLength));
            toolPivotPos.northing = tankPos.northing + (Math.Cos(toolPivotPos.heading) * (tool.trailingHitchLength));
        }
        private void btnTramDisplayMode_Click(object sender, EventArgs e)
        {
            tram.isLeftManualOn = false;
            tram.isRightManualOn = false;

            //if only lines cycle on off
            if (tram.tramList.Count > 0 && tram.tramBndOuterArr.Count == 0)
            {
                if (tram.displayMode != 0) tram.displayMode = 0;
                else tram.displayMode = 2;
            }
            else
            {
                tram.displayMode++;
                if (tram.displayMode > 3) tram.displayMode = 0;
            }

            switch (tram.displayMode)
            {
                case 0:
                    btnTramDisplayMode.Image = Properties.Resources.TramOff;
                    break;
                case 1:
                    btnTramDisplayMode.Image = Properties.Resources.TramAll;
                    break;
                case 2:
                    btnTramDisplayMode.Image = Properties.Resources.TramLines;
                    break;
                case 3:
                    btnTramDisplayMode.Image = Properties.Resources.TramOuter;
                    break;

                default:
                    break;
            }
        }
        public bool isPatchesChangingColor = false;
        private void btnChangeMappingColor_Click(object sender, EventArgs e)
        {
            using (var form = new FormColorPicker(this, sectionColorDay))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    sectionColorDay = form.useThisColor;
                }
            }

            Settings.Default.setDisplay_colorSectionsDay = sectionColorDay;
            Settings.Default.Save();

            isPatchesChangingColor = true;
        }
        private void btnYouSkipEnable_Click(object sender, EventArgs e)
        {
            yt.rowSkipsWidth = Properties.Settings.Default.set_youSkipWidth;
            switch (yt.skipMode)
            {
                case SkipMode.Normal:
                    btnYouSkipEnable.Image = Resources.YouSkipOn;
                    yt.skipMode = SkipMode.Alternative;
                    //make sure at least 1
                    if (yt.rowSkipsWidth < 2)
                    {
                        yt.rowSkipsWidth = 2;
                        cboxpRowWidth.Text = "1";
                    }
                    yt.Set_Alternate_skips();
                    break;
                case SkipMode.Alternative:
                    btnYouSkipEnable.Image = Resources.YouSkipWorkedTracks;
                    yt.skipMode = SkipMode.IgnoreWorkedTracks;
                    //make sure at least 1
                    if (yt.rowSkipsWidth < 2)
                    {
                        yt.rowSkipsWidth = 2;
                        cboxpRowWidth.Text = "1";
                    }
                    break;
                case SkipMode.IgnoreWorkedTracks:
                    btnYouSkipEnable.Image = Resources.YouSkipOff;
                    yt.skipMode = SkipMode.Normal;
                    break;
            }

            yt.ResetCreatedYouTurn();

        }


        private void cboxpRowWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            yt.rowSkipsWidth = cboxpRowWidth.SelectedIndex + 1;
            yt.Set_Alternate_skips();
            if (!yt.isYouTurnTriggered) yt.ResetCreatedYouTurn();
            Properties.Settings.Default.set_youSkipWidth = yt.rowSkipsWidth;
            Properties.Settings.Default.Save();
        }
        private void btnHeadlandOnOff_Click(object sender, EventArgs e)
        {
            bnd.isHeadlandOn = !bnd.isHeadlandOn;
            if (bnd.isHeadlandOn)
            {
                btnHeadlandOnOff.Image = Properties.Resources.HeadlandOn;
            }
            else
            {
                btnHeadlandOnOff.Image = Properties.Resources.HeadlandOff;
            }

            if (vehicle.isHydLiftOn && !bnd.isHeadlandOn) vehicle.isHydLiftOn = false;

            if (!bnd.isHeadlandOn)
            {
                p_239.pgn[p_239.hydLift] = 0;
                btnHydLift.Image = Properties.Resources.HydraulicLiftOff;
            }

            PanelUpdateRightAndBottom();
        }
        private void cboxIsSectionControlled_Click(object sender, EventArgs e)
        {
            if (cboxIsSectionControlled.Checked) cboxIsSectionControlled.Image = Properties.Resources.HeadlandSectionOn;
            else cboxIsSectionControlled.Image = Properties.Resources.HeadlandSectionOff;
            bnd.isSectionControlledByHeadland = cboxIsSectionControlled.Checked;
            Properties.Settings.Default.setHeadland_isSectionControlled = cboxIsSectionControlled.Checked;
            Properties.Settings.Default.Save();
        }
        private void btnHydLift_Click(object sender, EventArgs e)
        {
            if (bnd.isHeadlandOn)
            {
                vehicle.isHydLiftOn = !vehicle.isHydLiftOn;
                if (vehicle.isHydLiftOn)
                {
                    btnHydLift.Image = Properties.Resources.HydraulicLiftOn;
                }
                else
                {
                    btnHydLift.Image = Properties.Resources.HydraulicLiftOff;
                    p_239.pgn[p_239.hydLift] = 0;
                }
            }
            else
            {
                p_239.pgn[p_239.hydLift] = 0;
                vehicle.isHydLiftOn = false;
                btnHydLift.Image = Properties.Resources.HydraulicLiftOff;
            }
        }

        #endregion

        #region Tools Menu

        private void allSettingsMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new FormAllSettings(this);
            form.Show(this);
        }
        private void boundaryToolToolStripMenu_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                using (var form = new FormBndTool(this))
                {
                    form.ShowDialog(this);
                }
            }
        }
        private void SmoothABtoolStripMenu_Click(object sender, EventArgs e)
        {
            if (isJobStarted && trk.idx > -1)
            {
                using (var form = new FormSmoothAB(this))
                {
                    form.ShowDialog(this);
                }
            }
            else
            {
                if (!isJobStarted) TimedMessageBox(2000, gStr.gsFieldNotOpen, gStr.gsStartNewField);
                else TimedMessageBox(2000, gStr.gsCurveNotOn, gStr.gsTurnABCurveOn);
            }
        }
        private void deleteContourPathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FileCreateContour();
            ct.stripList?.Clear();
            ct.ptList?.Clear();
            ct.ctList?.Clear();
            contourSaveList?.Clear();
        }
        private void toolStripAreYouSure_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                if (autoBtnState == btnStates.Off && manualBtnState == btnStates.Off)
                {
                    DialogResult result3 = FormDialog.Show(
                        gStr.gsDeleteAllContoursAndSections,
                        gStr.gsDeleteForSure,
                        MessageBoxButtons.YesNo
                    );

                    if (result3 == DialogResult.OK)
                    {
                        //FileCreateElevation();

                        if (tool.isSectionsNotZones)
                        {
                            //Update the button colors and text
                            AllSectionsAndButtonsToState(btnStates.Off);

                            //enable disable manual buttons
                            LineUpIndividualSectionBtns();
                        }
                        else
                        {
                            AllZonesAndButtonsToState(btnStates.Off);
                            LineUpAllZoneButtons();
                        }

                        //turn manual button off
                        manualBtnState = btnStates.Off;
                        btnSectionMasterManual.Image = Properties.Resources.ManualOff;

                        //turn auto button off
                        autoBtnState = btnStates.Off;
                        btnSectionMasterAuto.Image = Properties.Resources.SectionMasterOff;


                        //clear out the contour Lists
                        ct.StopContourLine();
                        ct.ResetContour();
                        fd.workedAreaTotal = 0;
                        fd.workedAreaTotalUser = 0;
                        fd.distanceUser = 0;

                        //clear the section lists
                        for (int j = 0; j < triStrip.Count; j++)
                        {
                            //clean out the lists
                            triStrip[j].patchList?.Clear();
                            triStrip[j].triangleList?.Clear();
                        }
                        patchSaveList?.Clear();

                        //delete all worked Lanes too
                        foreach (CTrk TrackItem in trk.gArr)
                        {
                            TrackItem.workedTracks.Clear();
                        }

                        FileCreateContour();
                        FileCreateSections();

                        Log.EventWriter("All Section Mapping Deleted");
                    }
                    else
                    {
                        TimedMessageBox(1500, gStr.gsNothingDeleted, gStr.gsActionHasBeenCancelled);
                    }
                }
                else
                {
                    TimedMessageBox(1500, "Sections are on", "Turn Auto or Manual Off First");
                }
            }
        }
        private void headingChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fh = Application.OpenForms["FormHeadingGraph"];

            if (fh != null)
            {
                fh.Focus();
                return;
            }

            //
            Form formH = new FormGraphHeading(this);
            formH.Show(this);
        }
        private void toolStripAutoSteerChart_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fcg = Application.OpenForms["FormSteerGraph"];

            if (fcg != null)
            {
                fcg.Focus();
                return;
            }

            //
            Form formG = new FormGraphSteer(this);
            formG.Show(this);
        }
        private void xTEChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fx = Application.OpenForms["FormXTEGraph"];

            if (fx != null)
            {
                fx.Focus();
                return;
            }

            //
            Form formX = new FormGraphXTE(this);
            formX.Show(this);
        }
        private void eventViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new FormEventViewer(Path.Combine(RegistrySettings.logsDirectory, "AgOpenGPS_Events_Log.txt"));
            form.Show(this);
            this.Activate();
        }
        private void webcamToolStrip_Click(object sender, EventArgs e)
        {
            Form form = new FormWebCam();
            form.Show(this);
            this.Activate();
        }
        private void offsetFixToolStrip_Click(object sender, EventArgs e)
        {
            using (var form = new FormShiftPos(this))
            {
                form.ShowDialog(this);
            }
        }
        private void correctionToolStrip_Click(object sender, EventArgs e)
        {
            //check if window already exists
            Form fcc = Application.OpenForms["FormCorrection"];

            if (fcc != null)
            {
                fcc.Focus();
                return;
            }

            //
            Form formC = new FormCorrection(this);
            formC.Show(this);
        }

        #endregion

        #region Nav Panel

        private void btnTiltUp_Click(object sender, EventArgs e)
        {
            camera.PitchInDegrees -= ((camera.PitchInDegrees * 0.012) - 1);
            if (camera.PitchInDegrees > -58) camera.PitchInDegrees = 0;
            navPanelCounter = 2;
        }

        private void btnTiltDn_Click(object sender, EventArgs e)
        {
            if (camera.PitchInDegrees > -59) camera.PitchInDegrees = -60;
            camera.PitchInDegrees += ((camera.PitchInDegrees * 0.012) - 1);
            if (camera.PitchInDegrees < -70) camera.PitchInDegrees = -70;
            navPanelCounter = 2;
        }

        private void btnN2D_Click(object sender, EventArgs e)
        {
            camera.FollowDirectionHint = false;
            camera.PitchInDegrees = 0;
            navPanelCounter = 0;
        }
        private void btn2D_Click(object sender, EventArgs e)
        {
            camera.FollowDirectionHint = true;
            camera.PitchInDegrees = 0;
            navPanelCounter = 0;
        }

        private void btn3D_Click(object sender, EventArgs e)
        {
            camera.FollowDirectionHint = true;
            camera.PitchInDegrees = -65;
            navPanelCounter = 0;
        }

        private void btnGrid_Click(object sender, EventArgs e)
        {
            var form = new FormGrid(this);
            form.Show(this);
            navPanelCounter = 0;
        }
        private void btnBrightnessUp_Click(object sender, EventArgs e)
        {
            if (displayBrightness.isWmiMonitor)
            {
                displayBrightness.BrightnessIncrease();
                btnBrightnessDn.Text = displayBrightness.GetBrightness().ToString() + "%";
                Settings.Default.setDisplay_brightness = displayBrightness.GetBrightness();
                Settings.Default.Save();
            }
            navPanelCounter = 3;
        }
        private void btnBrightnessDn_Click(object sender, EventArgs e)
        {
            if (displayBrightness.isWmiMonitor)
            {
                displayBrightness.BrightnessDecrease();
                btnBrightnessDn.Text = displayBrightness.GetBrightness().ToString() + "%";
                Settings.Default.setDisplay_brightness = displayBrightness.GetBrightness();
                Settings.Default.Save();
            }
            navPanelCounter = 3;
        }
        private void btnDayNightMode_Click(object sender, EventArgs e)
        {
            SwapDayNightMode();
            navPanelCounter = 0;
        }

        #endregion

        #region OpenGL Window context Menu and functions
        private void contextMenuStripOpenGL_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //dont bring up menu if no flag selected
            if (flagNumberPicked == 0) e.Cancel = true;
        }
        private void googleEarthOpenGLContextMenu_Click(object sender, EventArgs e)
        {
            if (isJobStarted)
            {
                //save new copy of kml with selected flag and view in GoogleEarth
                FileSaveSingleFlagKML(flagNumberPicked);

                //Process.Start(@"C:\Program Files (x86)\Google\Google Earth\client\googleearth", workingDirectory + currentFieldDirectory + "\\Flags.KML");
                Process.Start(Path.Combine(RegistrySettings.fieldsDirectory, currentFieldDirectory, "Flag.KML"));
            }
        }

        private void lblHardwareMessage_Click(object sender, EventArgs e)
        {
            hardwareLineCounter = 1;
        }

        #endregion

        #region Sim controls

        private void btnSimSpeedUp_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sim.stepDistance < 0)
            {
                sim.stepDistance = 0;
                return;
            }
            if (sim.stepDistance < 0.2) sim.stepDistance += 0.02;
            else sim.stepDistance *= 1.15;

            if (sim.stepDistance > 7.5) sim.stepDistance = 7.5;
        }
        private void btnSpeedDn_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sim.stepDistance < 0.2 && sim.stepDistance > -0.51) sim.stepDistance -= 0.02;
            else sim.stepDistance *= 0.8;
            if (sim.stepDistance < -0.5) sim.stepDistance = -0.5;
        }

        double lastSimGuidanceAngle = 0;
        private void timerSim_Tick(object sender, EventArgs e)
        {
            if (recPath.isDrivingRecordedPath || isBtnAutoSteerOn && (guidanceLineDistanceOff != 32000))
            {
                if (vehicle.isInDeadZone)
                {
                    sim.DoSimTick((double)lastSimGuidanceAngle);
                }
                else
                {
                    lastSimGuidanceAngle = (double)guidanceLineSteerAngle * 0.01 * 0.9;
                    sim.DoSimTick(lastSimGuidanceAngle);
                }
            }
            else sim.DoSimTick(sim.steerAngleScrollBar);
        }
        private void btnSimReverseDirection_Click(object sender, EventArgs e)
        {
            sim.headingTrue += Math.PI;
            ABLine.isABValid = false;
            curve.isCurveValid = false;
            if (isBtnAutoSteerOn)
            {
                btnAutoSteer.PerformClick();
                TimedMessageBox(2000, gStr.gsGuidanceStopped, "Sim Reverse Touched");
                Log.EventWriter("Steer Off, Sim Reverse Activated");
            }
        }
        private void hsbarSteerAngle_Scroll(object sender, ScrollEventArgs e)
        {
            sim.steerAngleScrollBar = (hsbarSteerAngle.Value - 400) * 0.1;
            btnResetSteerAngle.Text = sim.steerAngleScrollBar.ToString("N1");
        }
        private void btnResetSteerAngle_Click(object sender, EventArgs e)
        {
            sim.steerAngleScrollBar = 0;
            hsbarSteerAngle.Value = 400;
            btnResetSteerAngle.Text = sim.steerAngleScrollBar.ToString("N1");
        }
        private void btnResetSim_Click(object sender, EventArgs e)
        {
            sim.CurrentLatLon = new Wgs84(
                Properties.Settings.Default.setGPS_SimLatitude,
                Properties.Settings.Default.setGPS_SimLongitude);
        }
        private void btnSimSetSpeedToZero_Click(object sender, EventArgs e)
        {
            sim.stepDistance = 0;
        }
        private void btnSimReverse_Click(object sender, EventArgs e)
        {
            sim.stepDistance = 0;
            sim.isAccelBack = true;
        }
        private void btnSimForward_Click(object sender, EventArgs e)
        {
            sim.stepDistance = 0;
            sim.isAccelForward = true;
        }

        #endregion

        public void FixTramModeButton()
        {
            if (tram.tramList.Count > 0 && tram.tramBndOuterArr.Count > 0)
            {
                tram.displayMode = 1;
            }
            else if (tram.tramList.Count == 0 && tram.tramBndOuterArr.Count > 0)
            {
                tram.displayMode = 3;
            }
            else if (tram.tramList.Count > 0 && tram.tramBndOuterArr.Count == 0)
            {
                tram.displayMode = 2;
            }

            switch (tram.displayMode)
            {
                case 1:
                    btnTramDisplayMode.Image = Properties.Resources.TramAll;
                    break;
                case 2:
                    btnTramDisplayMode.Image = Properties.Resources.TramLines;
                    break;
                case 3:
                    btnTramDisplayMode.Image = Properties.Resources.TramOuter;
                    break;

                default:
                    break;
            }
        }

        private ToolStripMenuItem steerChartToolStripMenuItem;
        private ToolStripMenuItem headingChartToolStripMenuItem;
        private ToolStripMenuItem xTEChartToolStripMenuItem;
    }//end class

}//end namespace