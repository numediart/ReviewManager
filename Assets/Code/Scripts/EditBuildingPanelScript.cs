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

﻿using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EditBuildingPanelScript : MonoBehaviour
{
    public FloorsManagerScript FloorsManagerScriptRef;

    public GameObject ConfirmChangeFloorImagePanel;
    public Text ImageNameTxt;
    public InputField FloorNameInputField;
    public InputField FloorIdInputField;
    public InputField BlackWallsHeightInputField;
    public InputField RedWallsHeightInputField;
    public InputField BlueWallsHeightInputField;
    public InputField GreenWallsHeightInputField;
    public Text UpdatePanelStatusText;
    public RawImage PreviewRawImg;

    public InputField PositionXInputField;
    public InputField PositionZInputField;
    public InputField PositionYInputField;
    public InputField RotationInputField;
    public InputField ScaleInputField;
    public Button AutoscaleButton;
    public GameObject AutoscalePanelRef;

    private string imagePath;

    private List<InputField> requiredFields;
    private Vector2 initialRawImgSize;



    public void Awake()
    {
        requiredFields = new List<InputField>();
        requiredFields.Add(FloorNameInputField);
        //requiredFields.Add(FloorIdInputField);
        requiredFields.Add(BlackWallsHeightInputField);
        initialRawImgSize = PreviewRawImg.rectTransform.sizeDelta;
    }



    public void RequiredFieldOnEndEdit()
    {
        foreach (InputField field in requiredFields)
        {
            if (field.text != "")
            {
                field.image.color = Color.white;
            }
        }
    }



    public void LoadImageBtnOnClick()
    {
        ConfirmChangeFloorImagePanel.SetActive(true);
    }



    public void ShowImageFileDialog()
    {
        if (!FileBrowser.IsOpen)
        {
            FileBrowser.SetFilters(false, new FileBrowser.Filter("image", new string[] { ".png", ".jpg", ".jpeg" }));
            FileBrowser.ShowLoadDialog(SetImagePathOnSuccess, null, false, System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures), "Select Floor Plan", "Select");
        }
    }



    private void SetImagePathOnSuccess(string myPath)
    {
        imagePath = myPath;
        ImageNameTxt.text = Path.GetFileName(myPath);

        StartCoroutine(LoadImage());
    }

    IEnumerator LoadImage()
    {
        WWW www = new WWW(imagePath);

        while (!www.isDone)
            yield return null;

        FitTextureInPreviewRawImg(www.texture, initialRawImgSize.x, initialRawImgSize.y);
    }

    private void FitTextureInPreviewRawImg(Texture2D t, float maxW, float maxH)
    {
        PreviewRawImg.texture = t;
        float originalImageAspectRatio = t.width / (float)t.height;
        Vector2 rawImgSize = PreviewRawImg.rectTransform.sizeDelta;
        float w = maxW;
        float h = maxH;

        float displayAspectRatio = w / h;

        if (originalImageAspectRatio > displayAspectRatio)
        {
            h = w / originalImageAspectRatio;
        }
        else
        {
            w = h * originalImageAspectRatio;
        }
        PreviewRawImg.rectTransform.sizeDelta = new Vector2(w, h);
        //Debug.Log("raw : " + rawImgSize + ", texture : " + t.width + ", " + t.height + ", final : " + PreviewRawImg.rectTransform.sizeDelta);
    }



    private bool CheckForm()
    {
        // check if required fields are filled in
        bool requiredFieldMissing = false;
        foreach (InputField field in requiredFields)
        {
            if (field.text == "")
            {
                field.image.color = new Color(1.0f, 0.6f, 0.6f);
                UpdatePanelStatusText.text = "Please fill all the required fields before clicking 'Add' button";
                requiredFieldMissing = true;
            }
        }
        if (requiredFieldMissing)
            return false;

        // check if floor name and number are free
        /*int newFloorLevel = int.Parse(FloorIdInputField.text);
        if (ProjectManagerScript.Instance.SimulationSetup.IsFloorIdFree(newFloorLevel) == false &&
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct().Id != newFloorLevel)
        {
            FloorIdInputField.image.color = new Color(1.0f, 0.6f, 0.6f);
            UpdatePanelStatusText.text = "This Floor Number is already taken, please choose another value";
            return false;
        }*/
        if (ProjectManagerScript.Instance.SimulationSetup.IsFloorNameFree(FloorNameInputField.text) == false &&
             FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name != FloorNameInputField.text)
        {
            FloorNameInputField.image.color = new Color(1.0f, 0.6f, 0.6f);
            UpdatePanelStatusText.text = "This Floor Name is already taken, please choose another";
            return false;
        }

        // if no value has been put in optional fields, fill them with "0"
        if (RedWallsHeightInputField.text == "")
            RedWallsHeightInputField.text = "0";
        if (BlueWallsHeightInputField.text == "")
            BlueWallsHeightInputField.text = "0";
        if (GreenWallsHeightInputField.text == "")
            GreenWallsHeightInputField.text = "0";

        return true;
    }



    public void Update3DModelBtnOnClick()
    {
        if (CheckForm())
        {
            FloorCreationData data = new FloorCreationData
            {
                FloorName = FloorNameInputField.text,
                FloorId = int.Parse(FloorIdInputField.text),
                Texture = (Texture2D)PreviewRawImg.texture,
                BlackHeight = float.Parse(BlackWallsHeightInputField.text, CultureInfo.InvariantCulture),
                RedHeight = float.Parse(RedWallsHeightInputField.text, CultureInfo.InvariantCulture),
                BlueHeight = float.Parse(BlueWallsHeightInputField.text, CultureInfo.InvariantCulture),
                GreenHeight = float.Parse(GreenWallsHeightInputField.text, CultureInfo.InvariantCulture)
            };
            FloorsManagerScriptRef.UpdateFloor(data);
        }
    }

    public void SetValues()
    {
        FloorItemListPrefabScript floorItemListPrefabScript = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript();
        // update CreatePanel with loaded values and set controls as not interactable
        FloorNameInputField.text = floorItemListPrefabScript.GetFloorStruct().Name;
        FloorIdInputField.text = floorItemListPrefabScript.GetFloorStruct().Id.ToString();
        BlackWallsHeightInputField.text = floorItemListPrefabScript.GetFloorStruct().BlackHeight.ToString(CultureInfo.InvariantCulture);
        RedWallsHeightInputField.text = floorItemListPrefabScript.GetFloorStruct().RedHeight.ToString(CultureInfo.InvariantCulture);
        BlueWallsHeightInputField.text = floorItemListPrefabScript.GetFloorStruct().BlueHeight.ToString(CultureInfo.InvariantCulture);
        GreenWallsHeightInputField.text = floorItemListPrefabScript.GetFloorStruct().GreenHeight.ToString(CultureInfo.InvariantCulture);
        FitTextureInPreviewRawImg(floorItemListPrefabScript.texture2D, initialRawImgSize.x, initialRawImgSize.y);

        ImageNameTxt.text = "";

        // fill in and enable controls on EditPanel
        PositionXInputField.text = floorItemListPrefabScript.mesh.transform.position.x.ToString(CultureInfo.InvariantCulture);
        PositionZInputField.text = floorItemListPrefabScript.mesh.transform.position.z.ToString(CultureInfo.InvariantCulture);
        PositionYInputField.text = floorItemListPrefabScript.mesh.transform.position.y.ToString(CultureInfo.InvariantCulture);
        RotationInputField.text = floorItemListPrefabScript.mesh.transform.eulerAngles.y.ToString(CultureInfo.InvariantCulture);
        ScaleInputField.text = floorItemListPrefabScript.mesh.transform.localScale.x.ToString(CultureInfo.InvariantCulture);
    }



    public void ResetValues()
    {
        UpdatePanelStatusText.text = "";

        // clear values on CreatePanel and set controls as interactable
        ImageNameTxt.text = "";
        FloorNameInputField.text = "";
        FloorIdInputField.text = "";
        BlackWallsHeightInputField.text = "";
        RedWallsHeightInputField.text = "";
        BlueWallsHeightInputField.text = "";
        GreenWallsHeightInputField.text = "";
        PreviewRawImg.texture = new Texture2D(300, 300);


        // clear values on EditPanel and disable controls
        PositionXInputField.text = "";
        PositionZInputField.text = "";
        PositionYInputField.text = "";
        RotationInputField.text = "";
        ScaleInputField.text = "";
    }

    public void PositionXInputFieldOnEndEdit()
    {
        if (FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh != null)
        {
            Vector3 pos = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position;
            pos.x = float.Parse(PositionXInputField.text, CultureInfo.InvariantCulture);
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position = pos;
            FloorStruct floor = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct();
            floor.PositionX = pos.x;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().SetFloorStruct(floor);
            ProjectManagerScript.Instance.SaveSetup();
        }
    }

    public void PositionZInputFieldOnEndEdit()
    {
        if (FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh != null)
        {
            Vector3 pos = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position;
            pos.z = float.Parse(PositionZInputField.text, CultureInfo.InvariantCulture);
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position = pos;
            FloorStruct floor = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct();
            floor.PositionZ = pos.z;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().SetFloorStruct(floor);
            ProjectManagerScript.Instance.SaveSetup();
        }
    }

    public void PositionYInputFieldOnEdit()
    {
        if (FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh != null)
        {
            Vector3 pos = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position;
            pos.y = float.Parse(PositionYInputField.text, CultureInfo.InvariantCulture);
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.position = pos;
            FloorStruct floor = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct();
            floor.PositionY = pos.y;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().SetFloorStruct(floor);
            ProjectManagerScript.Instance.SaveSetup();
        }
    }

    public void RotationInputFieldOnEndEdit()
    {
        if (FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh != null)
        {
            Vector3 rot = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.eulerAngles;
            rot.y = float.Parse(RotationInputField.text, CultureInfo.InvariantCulture);
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.eulerAngles = rot;
            FloorStruct floor = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct();
            floor.Rotation = rot.y;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().SetFloorStruct(floor);
            ProjectManagerScript.Instance.SaveSetup();
        }
    }

    public void ScaleInputFieldOnEndEdit()
    {
        if (FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh != null)
        {
            Vector3 sca = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.localScale;
            sca.x = float.Parse(ScaleInputField.text, CultureInfo.InvariantCulture);
            sca.z = sca.x;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().mesh.transform.localScale = sca;

            FloorStruct floor = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().GetFloorStruct();
            floor.scaleX = sca.x;
            floor.scaleZ = sca.z;
            FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript().SetFloorStruct(floor);
            ProjectManagerScript.Instance.SaveSetup();
        }
    }

    public void AutoscaleButtonOnClick()
    {
        FloorItemListPrefabScript floorItemListPrefabScriptRef = FloorsManagerScriptRef.GetCurrentFloorItemListPrefabScript();
        if (floorItemListPrefabScriptRef != null)
        {
            string floorName = floorItemListPrefabScriptRef.FloorNameTxt.text;
            //Debug.Log("autoscale pressed on floor " + floorName);
            AutoscalePanelRef.SetActive(true);
            AutoscalePanelRef.GetComponent<AutoscalePanelScript>().SetData(floorItemListPrefabScriptRef.texture2D, this);
        }
    }
}
