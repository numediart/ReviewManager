// Copyright (C) 2020 - UMons
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
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
using UnityEngine;
using UnityEngine.UI;

public class NoteDetailPanelScript : MonoBehaviour
{
    public Camera CanvasCamera;
    public Text NoteNameText;
    public RawImage NoteImg;
    public RawImage LocationFloorImage;
    public RawImage LocationCursor;
    public RawImage EmotionGraph;
    public RawImage EmotionCursor;
    public InputField AuthorInputField;
    public Text DateField;
    public InputField TitleInputField;
    public InputField ContentInputField;
    public GameObject ViewNotesPanel;
    public GameObject ConfirmDeleteNotePanel;

    private Vector2 initialNoteRawImgSize;
    private Vector2 initialFloorRawImgSize;
    private Vector2 initialLocationCursorPos;
    private Vector2 initialEmotionCursorPos;

    private int noteIndex;



    public void Awake()
    {
        initialNoteRawImgSize = NoteImg.rectTransform.sizeDelta;
        initialFloorRawImgSize = LocationFloorImage.rectTransform.sizeDelta;
    }

    public void SetData(int index)
    {
        noteIndex = index;
        NoteStruct note = ProjectManagerScript.Instance.NotesList[index];
        NoteNameText.text = note.Data.Image; // contains image file name
        NoteImg.texture = note.Image;
        AuthorInputField.text = note.Data.Author;
        DateField.text = "Date : " + note.Data.Date;
        TitleInputField.text = GuiHelper.RemoveEmojiFromString(note.Data.Title);
        ContentInputField.text = GuiHelper.RemoveEmojiFromString(note.Data.Content);
        // set floor texture
        FloorItemListPrefabScript floorItemListPrefabScript = FloorsManagerScript.Instance.GetFloorItemListPrefabScriptFromId(note.Data.Location.Id);
        FitTextureInLocationFloorRawImg(floorItemListPrefabScript.texture2D, initialFloorRawImgSize.x, initialFloorRawImgSize.y);
        // position cursors
        Vector2 emotionGraphSize = EmotionGraph.rectTransform.sizeDelta;
        Vector2 emotion = new Vector2(note.Data.Emotion.Intensity * emotionGraphSize.x * 0.5f, 
                                        note.Data.Emotion.Valence * emotionGraphSize.y * 0.5f);
        EmotionCursor.rectTransform.localPosition = emotion;
        EmotionCursor.color = Color.white;
        initialEmotionCursorPos = emotion;

        Vector2 locationFloorImageSize = LocationFloorImage.rectTransform.sizeDelta;
        Vector2 location = new Vector2((note.Data.Location.XCoordinate - 0.5f) * locationFloorImageSize.x, 
                                        -(note.Data.Location.YCoordinate - 0.5f) * locationFloorImageSize.y  //invert y axis as origin is top left for coordinates
                                            + LocationCursor.rectTransform.sizeDelta.y / 2); // we want the tip of the cursor to be at the image location
        LocationCursor.rectTransform.localPosition = location;
        LocationCursor.color = Color.white;
        initialLocationCursorPos = location;

        TurnNoteImage(note.Data.ImageOrientation, true);
    }

    public void LocationCursorOnPointerEnter()
    {
        LocationCursor.color = new Color(0.24f, 1f, 1f);
    }

    public void LocationCursorOnDrag()
    {
        Vector2 mouseCurrentPositionOnGraphImage;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(LocationFloorImage.rectTransform,
            Input.mousePosition,
            CanvasCamera,
            out mouseCurrentPositionOnGraphImage);
        Vector2 imageSize = LocationFloorImage.rectTransform.sizeDelta;
        if (mouseCurrentPositionOnGraphImage.x > -imageSize.x / 2 &&
            mouseCurrentPositionOnGraphImage.x < imageSize.x / 2 &&
            mouseCurrentPositionOnGraphImage.y > (-imageSize.y / 2 - LocationCursor.rectTransform.sizeDelta.y / 2) &&
            mouseCurrentPositionOnGraphImage.y < (imageSize.y / 2 - LocationCursor.rectTransform.sizeDelta.y / 2))
        {
            LocationCursor.rectTransform.localPosition = mouseCurrentPositionOnGraphImage;
        }
    }

    public void LocationCursorOnPointerExit()
    {
        LocationCursor.color = Color.white;
    }

    public void EmotionCursorOnPointerEnter()
    {
        EmotionCursor.color = new Color(0.24f, 1f, 1f);
    }

    public void EmotionCursorOnDrag()
    {
        Vector2 mouseCurrentPositionOnGraphImage;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(EmotionGraph.rectTransform,
            Input.mousePosition,
            CanvasCamera,
            out mouseCurrentPositionOnGraphImage);
        Vector2 imageSize = EmotionGraph.rectTransform.sizeDelta;
        if (mouseCurrentPositionOnGraphImage.x > -imageSize.x / 2 &&
            mouseCurrentPositionOnGraphImage.x < imageSize.x / 2 &&
            mouseCurrentPositionOnGraphImage.y > -imageSize.y / 2 &&
            mouseCurrentPositionOnGraphImage.y < imageSize.y / 2)
        { 
            EmotionCursor.rectTransform.localPosition = mouseCurrentPositionOnGraphImage;
        }
    }

    public void EmotionCursorOnPointerExit()
    {
            EmotionCursor.color = Color.white;
    }

    public void SaveBtnOnClick()
    {
        NoteStruct note = ProjectManagerScript.Instance.NotesList[noteIndex];
        note.Data.ImageOrientation = (float)Math.Round(NoteImg.gameObject.GetComponent<RectTransform>().eulerAngles.z, 2);
        // location and emotion
        if ((Vector2)LocationCursor.rectTransform.localPosition != initialLocationCursorPos)
        {
            Vector2 locationFloorImageSize = LocationFloorImage.rectTransform.sizeDelta;
            Vector2 locationVector = LocationCursor.rectTransform.localPosition;
            locationVector.y -= LocationCursor.rectTransform.sizeDelta.y / 2;
            locationVector /= locationFloorImageSize;
            note.Data.Location.XCoordinate = locationVector.x + 0.5f;
            note.Data.Location.YCoordinate = -locationVector.y + 0.5f;
        }
        if ((Vector2)EmotionCursor.rectTransform.localPosition != initialEmotionCursorPos)
        {
            Vector2 emotionGraphSize = EmotionGraph.rectTransform.sizeDelta;
            Vector2 emotionVector = EmotionCursor.rectTransform.localPosition;
            emotionVector /= emotionGraphSize;
            emotionVector *= 2;
            note.Data.Emotion.Intensity = emotionVector.x;
            note.Data.Emotion.Valence = emotionVector.y;
        }
        // text info
        note.Data.Content = ContentInputField.text;
        note.Data.Title = TitleInputField.text;
        note.Data.Author = AuthorInputField.text;
        // update and save in project manager
        ProjectManagerScript.Instance.NotesList[noteIndex] = note;
        ProjectManagerScript.Instance.UpdateNotePositionAndRotation(noteIndex);
        ProjectManagerScript.Instance.SaveNote(note.Data);
        if(ViewNotesPanel.activeInHierarchy)
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList();
        gameObject.SetActive(false);
    }

    public void CancelBtnOnClick()
    {
        gameObject.SetActive(false);
    }

    public void DeleteNoteBtnOnCLick()
    {
        ConfirmDeleteNotePanel.GetComponent<ConfirmDeleteNotePanelScript>().Init(NoteNameText.text, noteIndex);
        ConfirmDeleteNotePanel.SetActive(true);
    }

    public void TurnLeftBtnOnClick()
    {
        TurnNoteImage(90, false);
    }

    public void TurnRightBtnOnClick()
    {
        TurnNoteImage(-90, false);
    }

    private void TurnNoteImage(float angle, bool absolute = false)
    {
        var rot = NoteImg.gameObject.GetComponent<RectTransform>().eulerAngles;
        if (absolute)
        {
            rot.z = angle;
        }
        else
        {
            rot.z += angle;
        }
        NoteImg.gameObject.GetComponent<RectTransform>().eulerAngles = rot;

        float originalImageAspectRatio = NoteImg.texture.width / (float)NoteImg.texture.height;
        float w = initialNoteRawImgSize.x;
        float h = initialNoteRawImgSize.y;

        float displayAspectRatio = w / h;

        if (((int)rot.z % 180) != 0)
        {
            h = initialNoteRawImgSize.x;
            w = initialNoteRawImgSize.y;
            displayAspectRatio = w / h;
        }

        if (originalImageAspectRatio > displayAspectRatio)
        {
            h = w / originalImageAspectRatio;
        }
        else
        {
            w = h * originalImageAspectRatio;
        }
        NoteImg.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
    }


    private void FitTextureInLocationFloorRawImg(Texture2D t, float maxW, float maxH)
    {
        LocationFloorImage.texture = t;
        float originalImageAspectRatio = t.width / (float)t.height;
        Vector2 rawImgSize = LocationFloorImage.rectTransform.sizeDelta;
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
        LocationFloorImage.rectTransform.sizeDelta = new Vector2(w, h);
    }
}
