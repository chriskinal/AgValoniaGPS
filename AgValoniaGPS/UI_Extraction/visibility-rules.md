# Dynamic Visibility Rules

**Total Rules**: 312

## Top 10 Forms by Visibility Rules

### FormBuildTracks.cs (78 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| btnRefSideCurve | `Unconditional` | False | btnLatLonPivot2_Click |
| panelABLine | `Unconditional` | False | FormBuildTracks_Load |
| panelABLine | `Unconditional` | False | btnNewTrack_Click |
| panelABLine | `Unconditional` | True | btnzABLine_Click |
| panelABLine | `Unconditional` | False | btnEnter_AB_Click |
| panelABLine | `Unconditional` | False | btnCancelCurve_Click |
| panelAPlus | `Unconditional` | False | FormBuildTracks_Load |
| panelAPlus | `Unconditional` | False | btnNewTrack_Click |
| panelAPlus | `Unconditional` | True | btnzAPlus_Click |
| panelAPlus | `Unconditional` | False | btnEnter_APlus_Click |
| panelAPlus | `Unconditional` | False | btnCancelCurve_Click |
| panelChoose | `Unconditional` | False | FormBuildTracks_Load |
| panelChoose | `Unconditional` | False | btnNewTrack_Click |
| panelChoose | `Unconditional` | True | btnNewTrack_Click |
| panelChoose | `Unconditional` | False | btnzABCurve_Click |
| panelChoose | `Unconditional` | False | btnzAPlus_Click |
| panelChoose | `Unconditional` | False | btnzABLine_Click |
| panelChoose | `Unconditional` | False | btnzLatLonPlusHeading_Click |
| panelChoose | `Unconditional` | False | btnzLatLon_Click |
| panelChoose | `Unconditional` | False | btnLatLonPivot_Click |

*... and 58 more rules*


### FormSteer.cs (54 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| hsbarSensor | `cboxEncoder.Checked` | False | FormSteer_Load |
| hsbarSensor | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| hsbarSensor | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| hsbarSensor | `cboxCurrentSensor.Checked` | False | FormSteer_Load |
| hsbarSensor | `!checkbox.Checked` | False | EnableAlert_Click |
| hsbarSensor | `checkbox == cboxPressureSensor` | True | EnableAlert_Click |
| hsbarSensor | `checkbox == cboxCurrentSensor` | True | EnableAlert_Click |
| hsbarSensor | `checkbox == cboxEncoder` | False | EnableAlert_Click |
| labelTurnSensor | `cboxEncoder.Checked` | True | FormSteer_Load |
| labelTurnSensor | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| labelTurnSensor | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| labelTurnSensor | `cboxCurrentSensor.Checked` | False | FormSteer_Load |
| labelTurnSensor | `!checkbox.Checked` | False | EnableAlert_Click |
| labelTurnSensor | `checkbox == cboxPressureSensor` | True | EnableAlert_Click |
| labelTurnSensor | `checkbox == cboxCurrentSensor` | True | EnableAlert_Click |
| labelTurnSensor | `checkbox == cboxEncoder` | True | EnableAlert_Click |
| lblhsbarSensor | `cboxEncoder.Checked` | False | FormSteer_Load |
| lblhsbarSensor | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| lblhsbarSensor | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| lblhsbarSensor | `cboxCurrentSensor.Checked` | False | FormSteer_Load |

*... and 34 more rules*


### FormSteerWiz.cs (50 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| hsbarSensor | `cboxEncoder.Checked` | False | FormSteer_Load |
| hsbarSensor | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| hsbarSensor | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| hsbarSensor | `cboxCurrentSensor.Checked` | False | FormSteer_Load |
| hsbarSensor | `!checkbox.Checked` | False | cboxCancelGuidance_Click |
| hsbarSensor | `checkbox == cboxPressureSensor` | True | cboxCancelGuidance_Click |
| hsbarSensor | `checkbox == cboxCurrentSensor` | True | cboxCancelGuidance_Click |
| hsbarSensor | `checkbox == cboxEncoder` | False | cboxCancelGuidance_Click |
| label61 | `cboxEncoder.Checked` | True | FormSteer_Load |
| label61 | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| label61 | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| label61 | `cboxCurrentSensor.Checked` | False | FormSteer_Load |
| label61 | `!checkbox.Checked` | False | cboxCancelGuidance_Click |
| label61 | `checkbox == cboxPressureSensor` | True | cboxCancelGuidance_Click |
| label61 | `checkbox == cboxCurrentSensor` | True | cboxCancelGuidance_Click |
| label61 | `checkbox == cboxEncoder` | True | cboxCancelGuidance_Click |
| lblhsbarSensor | `cboxEncoder.Checked` | False | FormSteer_Load |
| lblhsbarSensor | `cboxPressureSensor.Checked` | True | FormSteer_Load |
| lblhsbarSensor | `cboxCurrentSensor.Checked` | True | FormSteer_Load |
| lblhsbarSensor | `cboxCurrentSensor.Checked` | False | FormSteer_Load |

*... and 30 more rules*


### FormBndTool.cs (30 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| btnCancelTouch | `Unconditional` | True | btnMakeBoundary_Click |
| btnCenterOGL | `Unconditional` | False | btnResetReduce_Click |
| btnCenterOGL | `Unconditional` | True | btnMakeBoundary_Click |
| btnMoveDn | `Unconditional` | False | btnResetReduce_Click |
| btnMoveDn | `Unconditional` | False | btnMakeBoundary_Click |
| btnMoveDn | `zoom == 0.1` | True | timer1_Tick |
| btnMoveDn | `zoom == 0.1` | False | timer1_Tick |
| btnMoveLeft | `Unconditional` | False | btnResetReduce_Click |
| btnMoveLeft | `Unconditional` | False | btnMakeBoundary_Click |
| btnMoveLeft | `zoom == 0.1` | True | timer1_Tick |
| btnMoveLeft | `zoom == 0.1` | False | timer1_Tick |
| btnMoveRight | `Unconditional` | False | btnResetReduce_Click |
| btnMoveRight | `Unconditional` | False | btnMakeBoundary_Click |
| btnMoveRight | `zoom == 0.1` | True | timer1_Tick |
| btnMoveRight | `zoom == 0.1` | False | timer1_Tick |
| btnMoveUp | `Unconditional` | False | btnResetReduce_Click |
| btnMoveUp | `Unconditional` | False | btnMakeBoundary_Click |
| btnMoveUp | `zoom == 0.1` | True | timer1_Tick |
| btnMoveUp | `zoom == 0.1` | False | timer1_Tick |
| btnSlice | `Unconditional` | False | btnResetReduce_Click |

*... and 10 more rules*


### FormBoundary.cs (24 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| btnABDraw | `numberSets.Length > 2` | True | btnLoadBoundaryFromGE_Click |
| btnABDraw | `sender is Button button` | True | btnLoadBoundaryFromGE_Click |
| d | `Unconditional` | True | UpdateChart |
| panelChoose | `Unconditional` | False | FormBoundary_Load |
| panelChoose | `Unconditional` | False | btnReturn_Click |
| panelChoose | `Unconditional` | True | btnAdd_Click |
| panelChoose | `Unconditional` | False | btnLoadBoundaryFromGE_Click |
| panelChoose | `Unconditional` | False | btnDriveOrExt_Click |
| panelChoose | `Unconditional` | False | btnGetKML_Click |
| panelChoose | `Unconditional` | False | btnBingMaps_Click |
| panelKML | `Unconditional` | False | FormBoundary_Load |
| panelKML | `Unconditional` | False | btnReturn_Click |
| panelKML | `Unconditional` | False | btnAdd_Click |
| panelKML | `Unconditional` | False | btnLoadBoundaryFromGE_Click |
| panelKML | `Unconditional` | False | btnDriveOrExt_Click |
| panelKML | `Unconditional` | True | btnGetKML_Click |
| panelKML | `Unconditional` | False | btnBingMaps_Click |
| panelMain | `Unconditional` | True | FormBoundary_Load |
| panelMain | `Unconditional` | True | btnReturn_Click |
| panelMain | `Unconditional` | False | btnAdd_Click |

*... and 4 more rules*


### FormQuickAB.cs (24 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| btnPausePlay | `mf.curve.isMakingCurve` | True | btnACurve_Click |
| panelABLine | `Unconditional` | False | FormQuickAB_Load |
| panelABLine | `Unconditional` | True | btnzABLine_Click |
| panelABLine | `Unconditional` | False | btnEnter_AB_Click |
| panelAPlus | `Unconditional` | False | FormQuickAB_Load |
| panelAPlus | `Unconditional` | True | btnzAPlus_Click |
| panelAPlus | `Unconditional` | False | btnEnter_APlus_Click |
| panelChoose | `Unconditional` | True | FormQuickAB_Load |
| panelChoose | `Unconditional` | False | btnzABCurve_Click |
| panelChoose | `Unconditional` | False | btnzAPlus_Click |
| panelChoose | `Unconditional` | False | btnzABLine_Click |
| panelChoose | `cnt > 3` | False | btnBCurve_Click |
| panelCurve | `Unconditional` | False | FormQuickAB_Load |
| panelCurve | `Unconditional` | True | btnzABCurve_Click |
| panelCurve | `Unconditional` | False | btnBCurve_Click |
| panelCurve | `cnt > 3` | False | btnBCurve_Click |
| panelCurve | `cnt > 3` | False | btnBCurve_Click |
| panelName | `Unconditional` | False | FormQuickAB_Load |
| panelName | `Unconditional` | True | btnBCurve_Click |
| panelName | `cnt > 3` | True | btnBCurve_Click |

*... and 4 more rules*


### FormFieldData.cs (14 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| labelRemain | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| labelRemain | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| labelRemain2 | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| labelRemain2 | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| lblActualRemain | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| lblActualRemain | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| lblAreaRemain | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| lblAreaRemain | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| lblRemainPercent | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| lblRemainPercent | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| lblTimeRemaining | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| lblTimeRemaining | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |
| lblTotalArea | `mf.bnd.bndList.Count > 0` | True | timer1_Tick |
| lblTotalArea | `mf.bnd.bndList.Count > 0` | False | timer1_Tick |

### FormGPS.cs (10 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| btnABDraw | `Unconditional` | False | JobClose |
| btnFieldStats | `Unconditional` | True | JobNew |
| btnFieldStats | `Unconditional` | False | JobClose |
| btnHydLift | `Unconditional` | False | JobClose |
| lblGuidanceLine | `Unconditional` | False | JobNew |
| lblGuidanceLine | `Unconditional` | False | JobClose |
| lblHardwareMessage | `Unconditional` | False | JobNew |
| lblHardwareMessage | `Unconditional` | False | JobClose |
| lblHardwareMessage | `Unconditional` | False | JobClose |
| panelDrag | `Unconditional` | False | JobClose |

### ConfigVehicleControl.cs (6 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| panelArticulatedBrands | `Unconditional` | False | UpdateImage |
| panelArticulatedBrands | `_vehicleConfig.Type == VehicleType.Ar...` | True | UpdateImage |
| panelHarvesterBrands | `Unconditional` | False | UpdateImage |
| panelHarvesterBrands | `_vehicleConfig.Type == VehicleType.Ha...` | True | UpdateImage |
| panelTractorBrands | `Unconditional` | False | UpdateImage |
| panelTractorBrands | `_vehicleConfig.Type == VehicleType.Tr...` | True | UpdateImage |

### FormAgShareDownloader.cs (6 rules)

| Control | Condition | Visible | Context |
|---------|-----------|---------|---------|
| lblDownloading | `Unconditional` | False | Unknown |
| lblDownloading | `Unconditional` | True | BtnDownloadAll_Click |
| lblDownloading | `Unconditional` | False | BtnDownloadAll_Click |
| progressBarDownloadAll | `Unconditional` | False | Unknown |
| progressBarDownloadAll | `Unconditional` | True | BtnDownloadAll_Click |
| progressBarDownloadAll | `Unconditional` | False | BtnDownloadAll_Click |

