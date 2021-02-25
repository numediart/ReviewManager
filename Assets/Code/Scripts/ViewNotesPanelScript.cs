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

public class ViewNotesPanelScript : MonoBehaviour
{
    public GameObject NoteItemListPrefab;
    public GameObject NoteItemListPrefabContent;
    public GameObject NoteDetailPanel;
    public GameObject NoteFilterEditPanel;
    public Toggle FilterNoteActiveToggle;
    public NoteFilter Filter;
    public bool filterActive = false;

    private List<GameObject> notesList = new List<GameObject>();

    public void Awake()
    {
        Filter = new NoteFilter();
    }

    public void UpdateNotesList()
    {
        ClearNotesList();
        List<int> floorIds = FloorsManagerScript.Instance.SelectedFloorsIds;
        NoteFilter tempFilter = new NoteFilter();
        if (filterActive)
        {
            tempFilter = Filter.Copy();
        }
        tempFilter.AddManyFloors(floorIds);

        List<NoteStruct> filteredNotes = ProjectManagerScript.Instance.GetNotesFiltered(tempFilter);


        foreach (NoteStruct n in ProjectManagerScript.Instance.NotesList)
        {
            bool showNoteIn3DView = filteredNotes.Contains(n) || !gameObject.activeInHierarchy;
            if (filteredNotes.Contains(n))
            {
                GameObject noteItem = GameObject.Instantiate(NoteItemListPrefab, NoteItemListPrefabContent.transform);
                noteItem.GetComponent<NoteItemListPrefabScript>().Init(n, NoteDetailPanel);

                notesList.Add(noteItem);
                n.ObjRef.SetActive(true);
            }
            n.ObjRef.SetActive(showNoteIn3DView);
        }
    }



    public void ClearNotesList()
    {
        foreach (GameObject note in notesList)
        {
            GameObject.Destroy(note);
        }

        notesList.Clear();
    }



    public void FilterNotesActiveOnChanged()
    {
        filterActive = FilterNoteActiveToggle.isOn;
        //Debug.Log("filter " + (filterActive ? "active" : "inactive"));
        UpdateNotesList();
    }



    public void EditFilterButtonOnClick()
    {
        List<string> authors = ProjectManagerScript.Instance.GetAllAuthors();
        NoteFilterEditPanel.GetComponent<NoteFilterPanelScript>().Init(Filter, authors);
        NoteFilterEditPanel.SetActive(true);
    }
}
