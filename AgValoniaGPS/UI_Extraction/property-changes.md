# Dynamic Property Changes

**Total Changes**: 2033

## Changes by Property Type

### Text (974 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| textBoxServer | `Settings.Default.AgShareServer` | `Unconditional` | FormAgShareSettings_Load |
| textBoxApiKey | `Settings.Default.AgShareApiKey` | `Unconditional` | FormAgShareSettings_Load |
| labelStatus | `Connecting...` | `Unconditional` | buttonTestConnection_Click |
| labelStatus | `✔ Connection successful` | `success` | buttonTestConnection_Click |
| labelStatus | `$"❌ {message}"` | `success` | buttonTestConnection_Click |
| labelStatus | `✔ Settings saved` | `Unconditional` | buttonSave_Click |
| btnToggleUpload | `Activated` | `Settings.Default.AgShareEna...` | UpdateAgShareToggleButton |
| btnToggleUpload | `Deactivated` | `Settings.Default.AgShareEna...` | UpdateAgShareToggleButton |
| textBoxApiKey | `Clipboard.GetText()` | `Clipboard.ContainsText()` | btnPaste_Click |
| btnAutoUpload | `Upload On` | `Settings.Default.AgShareUpl...` | UpdateAgShareUploadButton |
| btnAutoUpload | `Upload Off` | `Settings.Default.AgShareUpl...` | UpdateAgShareUploadButton |
| lblMessage | `message` | `Unconditional` | Unknown |
| lblTitle | `title` | `Unconditional` | Unknown |
| btnSection1Man | `1` | `Unconditional` | JobClose |
| lblFrameTime | `mf.frameTime.ToString("N1")` | `Unconditional` | timer1_Tick |

*... and 959 more changes*


### Enabled (450 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| btnPaste | `Clipboard.ContainsText()` | `Unconditional` | FormAgShareSettings_Load |
| textBoxServer | `false` | `Unconditional` | FormAgShareSettings_Load |
| btnPaste | `hasText` | `btnPaste.Enabled != hasText` | ClipboardCheckTimer_Tick |
| buttonSave | `true` | `success` | buttonTestConnection_Click |
| buttonSave | `true` | `Unconditional` | textBoxAnySetting_TextChanged |
| buttonSave | `true` | `Settings.Default.AgShareEna...` | UpdateAgShareToggleButton |
| btnPaste | `false` | `Clipboard.ContainsText()` | btnPaste_Click |
| textBoxServer | `true` | `Unconditional` | btnDevelop_Click |
| buttonSave | `true` | `Settings.Default.AgShareUpl...` | UpdateAgShareUploadButton |
| panelRight | `false` | `Unconditional` | FormGPS_Load |
| btnBrightnessDn | `false` | `displayBrightness.isWmiMonitor` | FormGPS_Load |
| btnBrightnessUp | `false` | `displayBrightness.isWmiMonitor` | FormGPS_Load |
| btnBrightnessDn | `false` | `displayBrightness.isWmiMonitor` | FormGPS_Load |
| btnBrightnessUp | `false` | `displayBrightness.isWmiMonitor` | FormGPS_Load |
| btnSectionMasterManual | `true` | `Unconditional` | JobNew |

*... and 435 more changes*


### BackColor (177 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| btnChargeStatus | `Color.YellowGreen` | `powerLineStatus == PowerLin...` | SystemEvents_PowerModeChanged |
| btnChargeStatus | `Color.LightCoral` | `powerLineStatus == PowerLin...` | SystemEvents_PowerModeChanged |
| btnSection1Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection2Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection3Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection4Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection5Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection6Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection7Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection8Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection9Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection10Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection11Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection12Man | `Color.Red` | `Unconditional` | JobNew |
| btnSection13Man | `Color.Red` | `Unconditional` | JobNew |

*... and 162 more changes*


### Checked (165 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| chkOffsetsOn | `mf.isKeepOffsetsOn` | `Unconditional` | FormShiftPos_Load |
| rbtnTractor | `true` | `_vehicleConfig.Type == case...` | Initialize |
| rbtnHarvester | `true` | `_vehicleConfig.Type == case...` | Initialize |
| rbtnArticulated | `true` | `_vehicleConfig.Type == case...` | Initialize |
| cboxIsImage | `!_vehicleConfig.IsImage` | `Unconditional` | UpdateImage |
| rbtnBrandTAgOpenGPS | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTCase | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTClaas | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTDeutz | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTFendt | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTJDeere | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTKubota | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTMassey | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTNH | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |
| rbtnBrandTSame | `true` | `TractorBrand == TractorBran...` | UpdateTractorBrand |

*... and 150 more changes*


### Image (110 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| btnToggleUpload | `Properties.Resources.UploadOn` | `Settings.Default.AgShareEna...` | UpdateAgShareToggleButton |
| btnToggleUpload | `Properties.Resources.UploadOff` | `Settings.Default.AgShareEna...` | UpdateAgShareToggleButton |
| btnAutoUpload | `Resources.AutoUploadOn` | `Settings.Default.AgShareUpl...` | UpdateAgShareUploadButton |
| btnAutoUpload | `Resources.AutoUploadOff` | `Settings.Default.AgShareUpl...` | UpdateAgShareUploadButton |
| btnSectionMasterManual | `Properties.Resources.ManualOff` | `Unconditional` | JobNew |
| btnSectionMasterAuto | `Properties.Resources.Sectio...` | `Unconditional` | JobNew |
| btnCycleLines | `Properties.Resources.ABLine...` | `Unconditional` | JobNew |
| btnCycleLinesBk | `Properties.Resources.ABLine...` | `Unconditional` | JobNew |
| btnAutoTrack | `Resources.AutoTrackOff` | `Unconditional` | JobNew |
| btnResumePath | `Properties.Resources.pathRe...` | `Unconditional` | JobClose |
| btnHydLift | `Properties.Resources.Hydrau...` | `Unconditional` | JobClose |
| btnSectionMasterManual | `Properties.Resources.ManualOff` | `Unconditional` | JobClose |
| btnSectionMasterAuto | `Properties.Resources.Sectio...` | `Unconditional` | JobClose |
| btnContour | `Properties.Resources.Contou...` | `Unconditional` | JobClose |
| btnCycleLines | `Properties.Resources.ABLine...` | `Unconditional` | JobClose |

*... and 95 more changes*


### Size (61 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| lblMessage2 | `new System.Drawing.Size(831...` | `Unconditional` | InitializeComponent |
| btnSerialOK | `new System.Drawing.Size(105...` | `Unconditional` | InitializeComponent |
| Size | `Properties.Settings.Default...` | `Unconditional` | FormBndTool_Load |
| this | `new Size(600, 300)` | `Unconditional` | FormBoundary_Load |
| Size | `new Size(180, 35)` | `Unconditional` | UpdateChart |
| Size | `new System.Drawing.Size(180...` | `Unconditional` | UpdateChart |
| Size | `new System.Drawing.Size(110...` | `Unconditional` | UpdateChart |
| this | `new System.Drawing.Size(600...` | `Unconditional` | btnReturn_Click |
| this | `new Size(245, 350)` | `Unconditional` | btnAdd_Click |
| this | `new Size(600, 300)` | `Unconditional` | btnLoadBoundaryFromGE_Click |
| Size | `Properties.Settings.Default...` | `Unconditional` | FormJob_Load |
| Size | `Properties.Settings.Default...` | `Unconditional` | FormMap_Load |
| Size | `Properties.Settings.Default...` | `Unconditional` | FormABDraw_Load |
| this | `new System.Drawing.Size(650...` | `Unconditional` | FormBuildTracks_Load |
| Size | `new Size(40, 25)` | `Unconditional` | UpdateTable |

*... and 46 more changes*


### ForeColor (35 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| labelStatus | `Color.Gray` | `Unconditional` | buttonTestConnection_Click |
| labelStatus | `Color.Green` | `success` | buttonTestConnection_Click |
| labelStatus | `Color.Red` | `success` | buttonTestConnection_Click |
| labelStatus | `Color.Blue` | `Unconditional` | buttonSave_Click |
| ForeColor | `GetStepColor(SavingStepStat...` | `Unconditional` | AddStep |
| Items[key] | `GetStepColor(state)` | `Unconditional` | UpdateStep |
| btnSerialOK | `System.Drawing.SystemColors...` | `Unconditional` | InitializeComponent |
| lblSpeed | `System.Drawing.Color.Red` | `Properties.Settings.Default...` | UpdateFixPosition |
| lblSpeed | `System.Drawing.Color.Green` | `Properties.Settings.Default...` | UpdateFixPosition |
| lblSelectedField | `Color.Red` | `Unconditional` | lbFields_SelectedIndexChanged |
| a | `Color.OrangeRed` | `i == fenceSelected` | UpdateChart |
| b | `Color.OrangeRed` | `i == fenceSelected` | UpdateChart |
| a | `System.Drawing.SystemColors...` | `i == fenceSelected` | UpdateChart |
| b | `System.Drawing.SystemColors...` | `i == fenceSelected` | UpdateChart |
| ForeColor | `Color.Black` | `Unconditional` | CreateTrackCheckbox |

*... and 20 more changes*


### Location (27 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| lblMessage2 | `new System.Drawing.Point(24...` | `Unconditional` | InitializeComponent |
| btnSerialOK | `new System.Drawing.Point(74...` | `Unconditional` | InitializeComponent |
| Location | `Properties.Settings.Default...` | `Unconditional` | FormJob_Load |
| Location | `Properties.Settings.Default...` | `Unconditional` | FormBuildTracks_Load |
| Location | `Properties.Settings.Default...` | `Unconditional` | FormABDraw_Load |
| Location | `Properties.Settings.Default...` | `Unconditional` | FormEditTrack_Load |
| btnSnapToPivot | `new System.Drawing.Point(90...` | `Unconditional` | InitializeComponent |
| bthOK | `new System.Drawing.Point(58...` | `Unconditional` | InitializeComponent |
| btnAdjLeft | `new System.Drawing.Point(7,...` | `Unconditional` | InitializeComponent |
| btnAdjRight | `new System.Drawing.Point(90...` | `Unconditional` | InitializeComponent |
| tableLayoutPanel1 | `new System.Drawing.Point(7,...` | `Unconditional` | InitializeComponent |
| btnZeroMove | `new System.Drawing.Point(7,...` | `Unconditional` | InitializeComponent |
| btnHalfToolRight | `new System.Drawing.Point(90...` | `Unconditional` | InitializeComponent |
| btnHalfToolLeft | `new System.Drawing.Point(7,...` | `Unconditional` | InitializeComponent |
| lblOffset | `new System.Drawing.Point(4,...` | `Unconditional` | InitializeComponent |

*... and 12 more changes*


### Font (22 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| lblMessage2 | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnSerialOK | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| this | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| Font | `new Font("Segoe UI", 16F, F...` | `Unconditional` | CreateTrackCheckbox |
| Font | `backupfont` | `Unconditional` | UpdateTable |
| btnSnapToPivot | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| bthOK | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnAdjLeft | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnAdjRight | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnZeroMove | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnHalfToolRight | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnHalfToolLeft | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| lblOffset | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| nudSnapDistance | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |
| btnAdjLeft | `new System.Drawing.Font("Ta...` | `Unconditional` | InitializeComponent |

*... and 7 more changes*


### BackgroundImage (12 changes)

| Control | New Value | Condition | Context |
|---------|-----------|-----------|---------|
| pboxAlpha | `TractorBitmaps.GetBitmap(_t...` | `Unconditional` | Unknown |
| pboxAlpha | `HarvesterBitmaps.GetBitmap(...` | `Unconditional` | Unknown |
| pboxAlpha | `ArticulatedBitmaps.GetFront...` | `Unconditional` | Unknown |
| pboxAlpha | `BrandImages.BrandTriangleVe...` | `_vehicleConfig.IsImage` | UpdateImage |
| pboxAlpha | `SetAlpha((Bitmap)_original,...` | `Unconditional` | UpdateOpacity |
| bthOK | `global::AgOpenGPS.Propertie...` | `Unconditional` | InitializeComponent |
| btnMode | `Properties.Resources.TramAll` | `mf.tram.generateMode == cas...` | FormTram_Load |
| btnMode | `Properties.Resources.TramLines` | `mf.tram.generateMode == cas...` | FormTram_Load |
| btnMode | `Properties.Resources.TramOuter` | `mf.tram.generateMode == cas...` | FormTram_Load |
| btnMode | `Properties.Resources.TramAll` | `mf.tram.generateMode == cas...` | btnMode_Click |
| btnMode | `Properties.Resources.TramLines` | `mf.tram.generateMode == cas...` | btnMode_Click |
| btnMode | `Properties.Resources.TramOuter` | `mf.tram.generateMode == cas...` | btnMode_Click |

