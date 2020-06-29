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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteFilterPanelScript : MonoBehaviour
{
    public Camera CanvasCamera;
    public ViewNotesPanelScript ViewNotesPanelScriptRef;
    public GameObject AuthorsSelectionPanel;
    public RawImage EmotionGraphImage;
    public RawImage EmotionFilterCircle;
    public Button ResetEmotionFilterButton;
    public GameObject NoteFilterAuthorTogglePrefab;
    //public Toggle NoteContentToggle;

    private List<GameObject> AllAuthorsPrefabList = new List<GameObject>();
    private Vector2 emotionFilterCenterOnGraphImage;


    public void Init(NoteFilter lastFilter, List<string> authors)
    {
        ResetValues();
        // set authors with checkbox on ui
        foreach (string author in authors)
        {
            GameObject authorSelectGameObject = GameObject.Instantiate(NoteFilterAuthorTogglePrefab, AuthorsSelectionPanel.transform);
            authorSelectGameObject.GetComponentInChildren<Text>().text = author;
            authorSelectGameObject.GetComponent<Toggle>().isOn = lastFilter.Authors.Contains(author);
            AllAuthorsPrefabList.Add(authorSelectGameObject);
        }
        int nbRows = (int)Mathf.Ceil(authors.Count / 4.0f);
        Vector2 panelSize = AuthorsSelectionPanel.GetComponent<RectTransform>().sizeDelta;
        panelSize.y = 48 * nbRows;
        AuthorsSelectionPanel.GetComponent<RectTransform>().sizeDelta = panelSize;
        
        // set emotion graph and circle (if emotion filter is set)
        if(lastFilter.EmotionCloseTo.Intensity !=0 || 
                lastFilter.EmotionCloseTo.Valence != 0 || 
                lastFilter.EmotionDistance != 2)
        {
            EmotionFilterCircle.gameObject.SetActive(true);
            Vector2 emotionCloseTo = new Vector2(lastFilter.EmotionCloseTo.Intensity, lastFilter.EmotionCloseTo.Valence);
            emotionFilterCenterOnGraphImage = emotionCloseTo * EmotionGraphImage.rectTransform.sizeDelta / 2;
            float diameter = lastFilter.EmotionDistance * EmotionGraphImage.rectTransform.sizeDelta.x;
            Vector2 circleSize = new Vector2(diameter, diameter);
            EmotionFilterCircle.rectTransform.localPosition = emotionFilterCenterOnGraphImage;
            EmotionFilterCircle.rectTransform.sizeDelta = circleSize;
            ResetEmotionFilterButton.interactable = true;
        }

        // set WithMessageOnly toggle
        //NoteContentToggle.isOn = lastFilter.WithMessageOnly;
    }



    public void ResetValues()
    {
        foreach(GameObject go in AllAuthorsPrefabList)
        {
            GameObject.Destroy(go);
        }
        AllAuthorsPrefabList = new List<GameObject>();
        EmotionFilterCircle.gameObject.SetActive(false);
        ResetEmotionFilterButton.interactable = false;
        //NoteContentToggle.isOn = false;
    }



    public void SelectAllAuthorsButtonOnClick()
    {
        foreach (GameObject go in AllAuthorsPrefabList)
        {
            go.GetComponent<Toggle>().isOn = true;
        }
    }



    public void DeselectAllAuthorsButtonOnClick()
    {
        foreach(GameObject go in AllAuthorsPrefabList)
        {
            go.GetComponent<Toggle>().isOn = false;
        }
    }



    public void ResetEmotionFilterButtonOnClick()
    {
        EmotionFilterCircle.gameObject.SetActive(false);
        ResetEmotionFilterButton.interactable = false;
    }


    public void EmotionGraphOnPointerDown()
    {
        EmotionFilterCircle.gameObject.SetActive(true);
        EmotionFilterCircle.rectTransform.sizeDelta = new Vector2(20, 20);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(EmotionGraphImage.rectTransform, 
            Input.mousePosition, 
            CanvasCamera, 
            out emotionFilterCenterOnGraphImage);
        EmotionFilterCircle.rectTransform.localPosition = emotionFilterCenterOnGraphImage;
        ResetEmotionFilterButton.interactable = true;
    }



    public void EmotionGraphOnDrag()
    {
        Vector2 mouseCurrentPositionOnGraphImage;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(EmotionGraphImage.rectTransform,
            Input.mousePosition,
            CanvasCamera,
            out mouseCurrentPositionOnGraphImage);
        float radius = (emotionFilterCenterOnGraphImage - mouseCurrentPositionOnGraphImage).magnitude;
        Vector2 circleSize = new Vector2(radius * 2, radius * 2);
        EmotionFilterCircle.rectTransform.sizeDelta = circleSize;
    }



    public void OKBtnOnClick()
    {
        NoteFilter filter = new NoteFilter();
        // set authors
        foreach (GameObject go in AllAuthorsPrefabList)
        {
            if (go.GetComponent<Toggle>().isOn)
                filter.Authors.Add(go.GetComponentInChildren<Text>().text);
        }

        // Set Emotion
        if(EmotionFilterCircle.IsActive())
        {
            Vector2 emotionFilterVector = emotionFilterCenterOnGraphImage / EmotionGraphImage.rectTransform.sizeDelta * 2;
            float emotionFilterRadius = EmotionFilterCircle.rectTransform.sizeDelta.x / EmotionGraphImage.rectTransform.sizeDelta.x;
            EmotionData emotionfilterCenter = new EmotionData()
            {
                Intensity = emotionFilterVector.x,
                Valence = emotionFilterVector.y
            };
            filter.EmotionCloseTo = emotionfilterCenter;
            filter.EmotionDistance = emotionFilterRadius;
        }

        // set WithMessageOnly
        //filter.WithMessageOnly = NoteContentToggle.isOn;

        ViewNotesPanelScriptRef.Filter = filter;
        ViewNotesPanelScriptRef.UpdateNotesList();
        ResetValues();
        gameObject.SetActive(false);
    }



    public void CancelBtnOnClick()
    {
        ResetValues();
        gameObject.SetActive(false);
    }
}
