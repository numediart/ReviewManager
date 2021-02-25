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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AddFloorPanelScript : MonoBehaviour
{
    public FloorsManagerScript FloorsManagerScriptRef;
    public InputField FloorNameInputField;
    public InputField ImageNameTxt;
    public RawImage PreviewRawImg;
    public InputField BlackWallsHeightInputField;
    public InputField RedWallsHeightInputField;
    public InputField BlueWallsHeightInputField;
    public InputField GreenWallsHeightInputField;
    public Text CreatePanelStatusText;

    private string imagePath;
    private List<InputField> requiredFields;
    private Vector2 initialRawImgSize;


    public void Awake()
    {
        requiredFields = new List<InputField>();
        requiredFields.Add(FloorNameInputField);
        requiredFields.Add(BlackWallsHeightInputField);
        initialRawImgSize = PreviewRawImg.rectTransform.sizeDelta;
    }



    public void LoadImageBtnOnClick()
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


    private bool CheckForm()
    {
        // check if image has been selected
        if(ImageNameTxt.text == "")
        {
            CreatePanelStatusText.text = "Please select an image";
            PreviewRawImg.color = new Color(1.0f, 0.6f, 0.6f);
            return false;
        }

        // check if required fields are filled in
        bool requiredFieldMissing = false;
        foreach(InputField field in requiredFields)
        {
            if(field.text == "")
            {
                field.image.color = new Color(1.0f, 0.6f, 0.6f);
                CreatePanelStatusText.text = "Please fill all the required fields before clicking 'Add' button";
                requiredFieldMissing = true;
            }
        }
        if (requiredFieldMissing)
            return false;

        // check if floor name and number are free
        /*if (ProjectManagerScript.Instance.SimulationSetup.IsFloorLevelFree(int.Parse(FloorNumberInputField.text)) == false)
        {
            FloorNumberInputField.image.color = new Color(1.0f, 0.6f, 0.6f);
            CreatePanelStatusText.text = "This Floor Number is already taken, please choose another value";
            return false;
        }*/
        if (ProjectManagerScript.Instance.SimulationSetup.IsFloorNameFree(FloorNameInputField.text) == false)
        {
            FloorNameInputField.image.color = new Color(1.0f, 0.6f, 0.6f);
            CreatePanelStatusText.text = "This Floor Name is already taken, please choose another";
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



    public void ResetValues()
    {
        CreatePanelStatusText.text = "";
        FloorNameInputField.text = "";
        ImageNameTxt.text = "";
        PreviewRawImg.texture = new Texture2D(300, 300);

        BlackWallsHeightInputField.text = "";
        RedWallsHeightInputField.text = "";
        BlueWallsHeightInputField.text = "";
        GreenWallsHeightInputField.text = "";
    }



    public void OKBtnOnClick()
    {
        if (CheckForm())
        {
            FloorCreationData data = new FloorCreationData
            {
                FloorName = FloorNameInputField.text,
                FloorId = ProjectManagerScript.Instance.SimulationSetup.GetNextFreeFloorId(),
                Texture = (Texture2D)PreviewRawImg.texture,
                BlackHeight = float.Parse(BlackWallsHeightInputField.text, CultureInfo.InvariantCulture),
                RedHeight = float.Parse(RedWallsHeightInputField.text, CultureInfo.InvariantCulture),
                BlueHeight = float.Parse(BlueWallsHeightInputField.text, CultureInfo.InvariantCulture),
                GreenHeight = float.Parse(GreenWallsHeightInputField.text, CultureInfo.InvariantCulture)
            };
            FloorsManagerScriptRef.CreateFloor(data);
            ResetValues();
            gameObject.SetActive(false);
        }
    }



    public void CancelBtnOnClick()
    {
        ResetValues();
        gameObject.SetActive(false);
    }
}
