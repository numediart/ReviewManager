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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteItemListPrefabScript : MonoBehaviour
{
    public RawImage NoteImg;
    public Text NoteNameTxt;

    private GameObject detailPanelRef;
    private int noteIndex;
    private Vector2 initialNoteImgSize = new Vector2(50, 50);


    public void Init(NoteStruct note, GameObject detailPanel)
    {
        noteIndex = ProjectManagerScript.Instance.GetNoteIndex(note);
        FitTextureInRawImg(note.Image, initialNoteImgSize.x, initialNoteImgSize.y);
        var rot = NoteImg.gameObject.GetComponent<RectTransform>().eulerAngles;
        rot.z = note.Data.ImageOrientation;
        NoteImg.gameObject.GetComponent<RectTransform>().eulerAngles = rot;
        NoteNameTxt.text = GuiHelper.RemoveEmojiFromString(note.Data.Title) + " (" + note.Data.Author + ")";
        detailPanelRef = detailPanel;
    }

    public void NoteNameBtnOnClick()
    {
        detailPanelRef.SetActive(true);
        detailPanelRef.GetComponent<NoteDetailPanelScript>().SetData(noteIndex);
    }


    private void FitTextureInRawImg(Texture2D t, float maxW, float maxH)
    {
        NoteImg.texture = t;
        float originalImageAspectRatio = t.width / (float)t.height;
        Vector2 rawImgSize = NoteImg.rectTransform.sizeDelta;
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
        NoteImg.rectTransform.sizeDelta = new Vector2(w, h);
    }
}
