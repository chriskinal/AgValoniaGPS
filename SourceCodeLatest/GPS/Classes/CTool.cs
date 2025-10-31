﻿using AgOpenGPS.Core.Drawing;
using AgOpenGPS.Core.DrawLib;
using AgOpenGPS.Core.Models;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace AgOpenGPS
{
    public class CTool
    {
        private readonly FormGPS mf;

        public double width, halfWidth, contourWidth;
        public double farLeftPosition = 0;
        public double farLeftSpeed = 0;
        public double farRightPosition = 0;
        public double farRightSpeed = 0;

        public double overlap;
        public double trailingHitchLength, tankTrailingHitchLength, trailingToolToPivotLength;
        public double offset;

        public double lookAheadOffSetting, lookAheadOnSetting;
        public double turnOffDelay;

        public double lookAheadDistanceOnPixelsLeft, lookAheadDistanceOnPixelsRight;
        public double lookAheadDistanceOffPixelsLeft, lookAheadDistanceOffPixelsRight;

        public bool isToolTrailing, isToolTBT;
        public bool isToolRearFixed, isToolFrontFixed;

        public bool isMultiColoredSections, isSectionOffWhenOut;

        public double hitchLength;

        //how many individual sections
        public int numOfSections;

        //used for super section off on
        public int minCoverage;

        public bool isLeftSideInHeadland = true, isRightSideInHeadland = true, isSectionsNotZones;

        //read pixel values
        public int rpXPosition;

        public int rpWidth;

        private double textRotate;

        public Color[] secColors = new Color[16];

        public int zones;
        public int[] zoneRanges = new int[9];

        public bool isDisplayTramControl;

        //Constructor called by FormGPS
        public CTool(FormGPS _f)
        {
            mf = _f;

            //from settings grab the vehicle specifics

            trailingToolToPivotLength = Properties.Settings.Default.setTool_trailingToolToPivotLength;
            width = Properties.Settings.Default.setVehicle_toolWidth;
            overlap = Properties.Settings.Default.setVehicle_toolOverlap;

            offset = Properties.Settings.Default.setVehicle_toolOffset;

            trailingHitchLength = Properties.Settings.Default.setTool_toolTrailingHitchLength;
            tankTrailingHitchLength = Properties.Settings.Default.setVehicle_tankTrailingHitchLength;
            hitchLength = Properties.Settings.Default.setVehicle_hitchLength;

            isToolRearFixed = Properties.Settings.Default.setTool_isToolRearFixed;
            isToolTrailing = Properties.Settings.Default.setTool_isToolTrailing;
            isToolTBT = Properties.Settings.Default.setTool_isToolTBT;
            isToolFrontFixed = Properties.Settings.Default.setTool_isToolFront;

            lookAheadOnSetting = Properties.Settings.Default.setVehicle_toolLookAheadOn;
            lookAheadOffSetting = Properties.Settings.Default.setVehicle_toolLookAheadOff;
            turnOffDelay = Properties.Settings.Default.setVehicle_toolOffDelay;

            isSectionOffWhenOut = Properties.Settings.Default.setTool_isSectionOffWhenOut;

            isSectionsNotZones = Properties.Settings.Default.setTool_isSectionsNotZones;

            if (isSectionsNotZones)
                numOfSections = Properties.Settings.Default.setVehicle_numSections;
            else
                numOfSections = Properties.Settings.Default.setTool_numSectionsMulti;

            minCoverage = Properties.Settings.Default.setVehicle_minCoverage;
            isMultiColoredSections = Properties.Settings.Default.setColor_isMultiColorSections;

            secColors[0] = Properties.Settings.Default.setColor_sec01.CheckColorFor255();
            secColors[1] = Properties.Settings.Default.setColor_sec02.CheckColorFor255();
            secColors[2] = Properties.Settings.Default.setColor_sec03.CheckColorFor255();
            secColors[3] = Properties.Settings.Default.setColor_sec04.CheckColorFor255();
            secColors[4] = Properties.Settings.Default.setColor_sec05.CheckColorFor255();
            secColors[5] = Properties.Settings.Default.setColor_sec06.CheckColorFor255();
            secColors[6] = Properties.Settings.Default.setColor_sec07.CheckColorFor255();
            secColors[7] = Properties.Settings.Default.setColor_sec08.CheckColorFor255();
            secColors[8] = Properties.Settings.Default.setColor_sec09.CheckColorFor255();
            secColors[9] = Properties.Settings.Default.setColor_sec10.CheckColorFor255();
            secColors[10] = Properties.Settings.Default.setColor_sec11.CheckColorFor255();
            secColors[11] = Properties.Settings.Default.setColor_sec12.CheckColorFor255();
            secColors[12] = Properties.Settings.Default.setColor_sec13.CheckColorFor255();
            secColors[13] = Properties.Settings.Default.setColor_sec14.CheckColorFor255();
            secColors[14] = Properties.Settings.Default.setColor_sec15.CheckColorFor255();
            secColors[15] = Properties.Settings.Default.setColor_sec16.CheckColorFor255();

            string[] words = Properties.Settings.Default.setTool_zones.Split(',');
            zones = int.Parse(words[0]);

            for (int i = 0; i < words.Length; i++)
            {
                zoneRanges[i] = int.Parse(words[i]);
            }

            isDisplayTramControl = Properties.Settings.Default.setTool_isDisplayTramControl;
        }

        public double GetHitchLengthFromVehiclePivot()
        {
            double pivotToHitch = hitchLength;

            if (mf.vehicle.VehicleConfig.Type == VehicleType.Articulated && !glm.IsZero(pivotToHitch))
            {
                double halfWheelbase = 0.5 * mf.vehicle.VehicleConfig.Wheelbase;

                if (!glm.IsZero(halfWheelbase))
                {
                    pivotToHitch += Math.Sign(pivotToHitch) * halfWheelbase;
                }
            }

            return pivotToHitch;
        }

        public double GetHitchHeadingFromVehiclePivot(double pivotToHitchLength)
        {
            double hitchHeading = mf.fixHeading;

            if (mf.vehicle.VehicleConfig.Type == VehicleType.Articulated && !glm.IsZero(pivotToHitchLength))
            {
                double steerAngleDegrees = mf.timerSim.Enabled ? mf.sim.steerAngle : mf.mc.actualSteerAngleDegrees;
                double articulationRadians = glm.toRadians(steerAngleDegrees);

                // The hitch translation already starts from the averaged vehicle heading
                // (fixHeading). Applying the full rear-frame deflection on top of that
                // over-rotates the hitch, so scale the articulation once more to keep the
                // lateral movement aligned with the rear frame.
                double rearHeadingOffset = 0.25 * articulationRadians;

                if (pivotToHitchLength > 0)
                {
                    hitchHeading += rearHeadingOffset;
                }
                else
                {
                    hitchHeading -= rearHeadingOffset;
                }

                hitchHeading = NormalizeAngle(hitchHeading);
            }

            return hitchHeading;
        }

        private static double NormalizeAngle(double angle)
        {
            if (angle < 0)
            {
                angle = (angle % glm.twoPI) + glm.twoPI;
            }
            else if (angle >= glm.twoPI)
            {
                angle %= glm.twoPI;
            }

            return angle;
        }

        private void DrawHitch(double trailingTank)
        {
            XyCoord[] vertices = {
                new XyCoord(-0.57, trailingTank),
                new XyCoord(0.0, 0.0),
                new XyCoord(0.57, trailingTank)
            };
            LineStyle backgroundLineStyle = new LineStyle(6.0f, Colors.Black);
            LineStyle foregroundLineStyle = new LineStyle(1.0f, Colors.HitchColor);
            LineStyle[] lineStyles = { backgroundLineStyle, foregroundLineStyle };
            GLW.DrawLineLoopPrimitiveLayered(lineStyles, vertices);
        }

        private void DrawTrailingHitch(double trailingTool)
        {
            XyCoord[] vertices = {
                new XyCoord(-0.65 + mf.tool.offset, trailingTool),
                new XyCoord(0.0, 0.0),
                new XyCoord(0.65 + mf.tool.offset, trailingTool)
            };
            LineStyle backgroundLineStyle = new LineStyle(6.0f, Colors.Black);
            LineStyle foregroundLineStyle = new LineStyle(1.0f, Colors.HitchTrailingColor);
            LineStyle[] lineStyles = { backgroundLineStyle, foregroundLineStyle };
            GLW.DrawLineLoopPrimitiveLayered(lineStyles, vertices);
        }

        public void DrawTool()
        {
            //translate and rotate at pivot axle
            GL.Translate(mf.pivotAxlePos.easting, mf.pivotAxlePos.northing, 0);
            GL.PushMatrix();

            //translate down to the hitch pin
            double pivotToHitchLength = GetHitchLengthFromVehiclePivot();
            double hitchHeading = GetHitchHeadingFromVehiclePivot(pivotToHitchLength);
            GL.Translate(
                Math.Sin(hitchHeading) * pivotToHitchLength,
                Math.Cos(hitchHeading) * pivotToHitchLength,
                0);

            //settings doesn't change trailing hitch length if set to rigid, so do it here
            double trailingTank, trailingTool;
            if (isToolTrailing)
            {
                trailingTank = tankTrailingHitchLength;
                trailingTool = trailingHitchLength;
            }
            else { trailingTank = 0; trailingTool = 0; }

            // if there is a trailing tow between hitch
            if (isToolTBT && isToolTrailing)
            {
                //rotate to tank heading
                GL.Rotate(glm.toDegrees(-mf.tankPos.heading), 0.0, 0.0, 1.0);

                DrawHitch(trailingTank);

                GL.Color4(1, 1, 1, 0.75);
                XyCoord toolAxleCenter = new XyCoord(0.0, trailingTank);
                XyDelta deltaToU1V1 = new XyDelta(1.5, 1.0);
                mf.VehicleTextures.ToolAxle.DrawCentered(toolAxleCenter, deltaToU1V1);

                //move down the tank hitch, unwind, rotate to section heading
                GL.Translate(0.0, trailingTank, 0.0);
                GL.Rotate(glm.toDegrees(mf.tankPos.heading), 0.0, 0.0, 1.0);
            }
            GL.Rotate(glm.toDegrees(-mf.toolPivotPos.heading), 0.0, 0.0, 1.0);

            //draw the hitch if trailing
            if (isToolTrailing)
            {
                DrawTrailingHitch(trailingTool);

                if (Math.Abs(trailingToolToPivotLength) > 1 && mf.camera.camSetDistance > -100)
                {
                    textRotate += (mf.sim.stepDistance);
                    GL.Color4(1, 1, 1, 0.75);
                    XyCoord rightTire00 = new XyCoord(0.75 + offset, trailingTool + 0.51);
                    XyCoord rightTire11 = new XyCoord(1.4 + offset, trailingTool - 0.51);
                    XyCoord leftTire00 = new XyCoord(-0.75 + offset, trailingTool + 0.51);
                    XyCoord lefttTire11 = new XyCoord(-1.4 + offset, trailingTool - 0.51);
                    mf.VehicleTextures.Tire.Draw(rightTire00, rightTire11);
                    mf.VehicleTextures.Tire.Draw(leftTire00, lefttTire11);
                }
                trailingTool -= trailingToolToPivotLength;
            }

            if (mf.isJobStarted)
            {
                //look ahead lines
                GL.LineWidth(3);
                GL.Begin(PrimitiveType.Lines);

                //lookahead section on
                GL.Color3(0.20f, 0.7f, 0.2f);
                GL.Vertex3(mf.tool.farLeftPosition, (mf.tool.lookAheadDistanceOnPixelsLeft) * 0.1 + trailingTool, 0);
                GL.Vertex3(mf.tool.farRightPosition, (mf.tool.lookAheadDistanceOnPixelsRight) * 0.1 + trailingTool, 0);

                //lookahead section off
                GL.Color3(0.70f, 0.2f, 0.2f);
                GL.Vertex3(mf.tool.farLeftPosition, (mf.tool.lookAheadDistanceOffPixelsLeft) * 0.1 + trailingTool, 0);
                GL.Vertex3(mf.tool.farRightPosition, (mf.tool.lookAheadDistanceOffPixelsRight) * 0.1 + trailingTool, 0);

                if (mf.vehicle.isHydLiftOn)
                {
                    GL.Color3(0.70f, 0.2f, 0.72f);
                    GL.Vertex3(mf.section[0].positionLeft, (mf.vehicle.hydLiftLookAheadDistanceLeft * 0.1) + trailingTool, 0);
                    GL.Vertex3(mf.section[mf.tool.numOfSections - 1].positionRight, (mf.vehicle.hydLiftLookAheadDistanceRight * 0.1) + trailingTool, 0);
                }

                GL.End();
            }

            //draw the sections
            GL.LineWidth(2);

            double hite = mf.camera.camSetDistance / -250;
            if (hite > 4) hite = 4;
            if (hite < 1) hite = 1;

            //TooDoo
            //hite = 0.2;

            for (int j = 0; j < numOfSections; j++)
            {
                //if section is on, green, if off, red color
                if (mf.section[j].isSectionOn)
                {
                    if (mf.section[j].sectionBtnState == btnStates.Auto)
                    {
                        //GL.Color3(0.0f, 0.9f, 0.0f);
                        if (mf.section[j].isMappingOn) GL.Color3(0.0f, 0.95f, 0.0f);
                        else GL.Color3(0.970f, 0.30f, 0.970f);
                    }
                    else GL.Color3(0.97, 0.97, 0);
                }
                else
                {
                    if (!mf.section[j].isMappingOn) GL.Color3(0.950f, 0.2f, 0.2f);
                    else GL.Color3(0.00f, 0.250f, 0.97f);
                    //GL.Color3(0.7f, 0.2f, 0.2f);
                }

                double mid = (mf.section[j].positionRight - mf.section[j].positionLeft) / 2 + mf.section[j].positionLeft;
                XyCoord[] vertices = {
                    new XyCoord(mf.section[j].positionLeft, trailingTool),
                    new XyCoord(mf.section[j].positionLeft, trailingTool - hite),
                    new XyCoord(mid, trailingTool - hite * 1.5),
                    new XyCoord(mf.section[j].positionRight, trailingTool - hite),
                    new XyCoord(mf.section[j].positionRight, trailingTool),
                };
                GLW.DrawTriangleFanPrimitive(vertices);

                if (mf.camera.camSetDistance > -width * 200)
                {
                    GLW.SetColor(Colors.Black);
                    GLW.DrawLineLoopPrimitive(vertices);
                }
            }

            //zones
            if (!isSectionsNotZones && zones > 0 && mf.camera.camSetDistance > -150)
            {
                //GL.PointSize(8);

                GL.Begin(PrimitiveType.Lines);
                for (int i = 1; i < zones; i++)
                {
                    GL.Color3(0.5f, 0.80f, 0.950f);
                    GL.Vertex3(mf.section[zoneRanges[i]].positionLeft, trailingTool - 0.4, 0);
                    GL.Vertex3(mf.section[zoneRanges[i]].positionLeft, trailingTool + 0.2, 0);
                }

                GL.End();
            }

            //tram Dots
            if (isDisplayTramControl && mf.tram.displayMode != 0)
            {
                if (mf.camera.camSetDistance > -300)
                {
                    if (mf.camera.camSetDistance > -100)
                        GL.PointSize(12);
                    else GL.PointSize(8);

                    ColorRgb rightMarkerColor = ((mf.tram.controlByte) & 1) != 0 ? Colors.TramMarkerOnColor : Colors.Black;
                    ColorRgb leftMarkerColor = ((mf.tram.controlByte) & 2) != 0 ? Colors.TramMarkerOnColor : Colors.Black;
                    double rightX = mf.tram.isOuter ? farRightPosition - mf.tram.halfWheelTrack : mf.tram.halfWheelTrack;
                    double leftX = mf.tram.isOuter ? farLeftPosition + mf.tram.halfWheelTrack : -mf.tram.halfWheelTrack;
                    // section markers
                    GL.Begin(PrimitiveType.Points);
                    GLW.SetColor(rightMarkerColor);
                    GL.Vertex3(rightX, trailingTool, 0);
                    GLW.SetColor(leftMarkerColor);
                    GL.Vertex3(leftX, trailingTool, 0);
                    GL.End();
                }
            }

            GL.PopMatrix();
        }
    }
}