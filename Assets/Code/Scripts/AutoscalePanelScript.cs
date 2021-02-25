// Copyright (C) 2020 - UMons
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoscalePanelScript : MonoBehaviour
{
    public Camera CanvasCamera;
    public Texture2D[] CrossCursors;

    public RawImage FloorImg;
    public RawImage PointADisplay;
    public RawImage PointBDisplay;
    public RawImage Line;
    public Text PointACoordsText;
    public Text PointBCoordsText;
    public InputField AutoscaleDistanceInputField;
    public Text AutoscaleStatusText;
    public InputField ScaleInputField;

    private int maxWidth = 1280;
    private int maxHeight = 854;

    private EditBuildingPanelScript editBuildingPanelScriptRef;

    private Vector2 PointAInPixels;
    private Vector2 PointAInRectSpace;
    private Vector2 PointBInPixels;
    private Vector2 PointBInRectSpace;
    private Vector2 OriginalFloorTextureResolution;
    private int CursorType;
    private float LineWidth = 6.0f;
    private float CrossSize;

    public void SetData(Texture2D floorPlanTexture, EditBuildingPanelScript script)
    {
        editBuildingPanelScriptRef = script;
        CursorType = -1;
        AutoscaleStatusText.text = "";

        CrossSize = 36.0f * Screen.height / 1080.0f;

        PointAInRectSpace = new Vector2(-100, 0);
        PointADisplay.texture = CrossCursors[0];  
        PointADisplay.rectTransform.sizeDelta = new Vector2(CrossSize, CrossSize);
        PointADisplay.rectTransform.localPosition = PointAInRectSpace;

        PointBInRectSpace = new Vector2(100, 0);
        PointBDisplay.texture = CrossCursors[1];
        PointBDisplay.rectTransform.sizeDelta = new Vector2(CrossSize, CrossSize);
        PointBDisplay.rectTransform.localPosition = PointBInRectSpace;

        Line.enabled = true;
        DrawLine(PointAInRectSpace, PointBInRectSpace, LineWidth);

        FloorImg.texture = floorPlanTexture;
        OriginalFloorTextureResolution = new Vector2(floorPlanTexture.width, floorPlanTexture.height);
        float originalImageAspectRatio = floorPlanTexture.width / (float)floorPlanTexture.height;
        float w = maxWidth;
        float h = maxHeight;
        float displayAspectRatio = w / h;
        if (originalImageAspectRatio > displayAspectRatio)
        {
            h = w / originalImageAspectRatio;
        }
        else
        {
            w = h * originalImageAspectRatio;
        }
        FloorImg.rectTransform.sizeDelta = new Vector2(w, h);

        PointAInPixels = (PointAInRectSpace / FloorImg.rectTransform.sizeDelta + new Vector2(0.5f, 0.5f)) * OriginalFloorTextureResolution;
        PointBInPixels = (PointBInRectSpace / FloorImg.rectTransform.sizeDelta + new Vector2(0.5f, 0.5f)) * OriginalFloorTextureResolution;
        PointACoordsText.text = "(" + PointAInPixels.x.ToString("N0", CultureInfo.InvariantCulture) + ", " +
                                    PointAInPixels.y.ToString("N0", CultureInfo.InvariantCulture) + ")";
        PointBCoordsText.text = "(" + PointBInPixels.x.ToString("N0", CultureInfo.InvariantCulture) + ", " +
                                    PointBInPixels.y.ToString("N0", CultureInfo.InvariantCulture) + ")";
    }



    public void GoBackBtnOnClick()
    {
        gameObject.SetActive(false);
    }



    public void AutoscaleBtnOnClick()
    {
        if(PointAInPixels.x < 0 || PointAInPixels.y < 0)
        {
            AutoscaleStatusText.text = "Please select point A";
            return;
        }
        if (PointBInPixels.x < 0 || PointBInPixels.y < 0)
        {
            AutoscaleStatusText.text = "Please select point B";
            return;
        }
        float distanceInRealWorld = -1.0f;
        try {
            distanceInRealWorld = float.Parse(AutoscaleDistanceInputField.text, CultureInfo.InvariantCulture);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
        if(distanceInRealWorld <= 0)
        {
            AutoscaleStatusText.text = "Please enter a distance\ngreater than 0,\nwith dot (.) as decimal separator";
            return;
        }
        AutoscaleStatusText.text = "";

        float scaleValue = distanceInRealWorld / Vector2.Distance(PointAInPixels, PointBInPixels) * 100.0f;
        // * 100 because the CreateBluidingPanlScript.CreateFloorMesh uses 0.01f as sizeBetweenPixels

        //Debug.Log("autoscale, value = " + scaleValue);
        ScaleInputField.text = scaleValue.ToString("N2", CultureInfo.InvariantCulture);
        editBuildingPanelScriptRef.ScaleInputFieldOnEndEdit();
        gameObject.SetActive(false);
    }



    public void PointASelectBtnOnClick()
    {
        //Debug.Log("Select Point A");
        CursorType = 0;
        PointADisplay.texture = null;
        Line.enabled = false;
    }



    public void PointBSelectBtnOnClick()
    {
        //Debug.Log("Select Point B");
        CursorType = 1;
        PointBDisplay.texture = null;
        Line.enabled = false;
    }



    public void FloorImgOnMouseEnter()
    {
        if (CursorType >= 0 && CursorType < CrossCursors.Length)
        {
            Texture2D cursorTexture = CrossCursors[CursorType];
            Vector2 hotspot = new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }



    public void FloorImgOnMouseDown()
    {
        if(CursorType>= 0)
        {
            return;
        }

        Vector2 pos = Input.mousePosition;
        Vector2 posInRectSpace;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(FloorImg.rectTransform, pos, CanvasCamera, out posInRectSpace);

        float distMouseToA = Vector2.Distance(PointAInRectSpace, posInRectSpace);
        float distMouseToB = Vector2.Distance(PointBInRectSpace, posInRectSpace);
        float distMin = Math.Min(distMouseToA, distMouseToB);

        if (distMin < CrossSize * 0.4)
        {
            if (distMouseToA <= distMouseToB)
            {
                CursorType = 0;
                PointADisplay.texture = null;
            }
            else
            {
                CursorType = 1;
                PointBDisplay.texture = null;
            }
            Texture2D cursorTexture = CrossCursors[CursorType];
            Vector2 hotspot = new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
        else
        {
            CursorType = -1;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }



    public void FloorImgOnMouseDrag()
    {
        if (CursorType < 0)
        {
            return;
        }

        Vector2 pos = Input.mousePosition;
        Vector2 posInRectSpace;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(FloorImg.rectTransform, pos, CanvasCamera, out posInRectSpace);

        Vector2 correctedPos = new Vector2();
        correctedPos = posInRectSpace / FloorImg.rectTransform.sizeDelta;
        correctedPos += new Vector2(0.5f, 0.5f);
        if (correctedPos.x < 0.0f || correctedPos.x > 1.0f ||
            correctedPos.y < 0.0f || correctedPos.y > 1.0f)
        {
            Line.enabled = false;
            return;
        }

        if (CursorType == 0)
        {
            DrawLine(PointBInRectSpace, posInRectSpace, LineWidth / 2);
        }
        if(CursorType == 1)
        {
            DrawLine(PointAInRectSpace, posInRectSpace, LineWidth / 2);
        }
    }



    public void FloorImgOnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }



    public void FloorImgOnMouseUp()
    {
        if(CursorType < 0)
        {
            return;
        }

        Vector2 pos = Input.mousePosition;
        Vector2 correctedPos = new Vector2() ;
        Vector2 posInRectSpace;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(FloorImg.rectTransform, pos, CanvasCamera, out posInRectSpace);
        correctedPos = posInRectSpace / FloorImg.rectTransform.sizeDelta;
        correctedPos += new Vector2(0.5f, 0.5f);
        if (correctedPos.x < 0.0f || correctedPos.x > 1.0f ||
            correctedPos.y < 0.0f || correctedPos.y > 1.0f)
        {
            return;
        }
        correctedPos *= OriginalFloorTextureResolution;

        Vector2 otherPointInRectSpace = new Vector2();
        if(CursorType == 0)
        {
            PointAInRectSpace = posInRectSpace;
            PointAInPixels = correctedPos;
            PointACoordsText.text = "(" + PointAInPixels.x.ToString("N0", CultureInfo.InvariantCulture) + ", " +
                                    PointAInPixels.y.ToString("N0", CultureInfo.InvariantCulture) + ")";
            PointADisplay.texture = CrossCursors[0];
            PointADisplay.transform.localPosition = posInRectSpace;
            otherPointInRectSpace = PointBInRectSpace;
        }
        if(CursorType == 1)
        {
            PointBInRectSpace = posInRectSpace;
            PointBInPixels = correctedPos;
            PointBCoordsText.text = "(" + PointBInPixels.x.ToString("N0", CultureInfo.InvariantCulture) + ", " +
                                    PointBInPixels.y.ToString("N0", CultureInfo.InvariantCulture) + ")";
            PointBDisplay.texture = CrossCursors[1];
            PointBDisplay.transform.localPosition = posInRectSpace;
            otherPointInRectSpace = PointAInRectSpace;
        }
        if(PointAInPixels.x >= 0 && PointBInPixels.x >= 0)
        {
            DrawLine(otherPointInRectSpace, posInRectSpace, LineWidth);
        }

        CursorType = -1;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }



    private void DrawLine(Vector2 a, Vector2 b, float lineWidth)
    {
        Vector3 differenceVector = a - b;
        Line.rectTransform.sizeDelta = new Vector2(differenceVector.magnitude, lineWidth);
        Line.rectTransform.pivot = new Vector2(0, 0.5f);
        Line.rectTransform.localPosition = a;
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg + 180.0f;
        Line.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        Line.enabled = true;
    }
}
