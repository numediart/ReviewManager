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
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct FloorCreationData
{
    public string FloorName;
    public int FloorId;
    public Texture2D Texture;
    public float BlackHeight;
    public float RedHeight;
    public float BlueHeight;
    public float GreenHeight;
}



public class FloorsManagerScript : MonoBehaviour
{
    private static FloorsManagerScript _instance = null;
    public static FloorsManagerScript Instance { get { return _instance; } }

    public Button NotesPanelButton;
    public Button editBtn;
    public Button deleteBtn;

    public TabsPanelScript TabsPanelScriptRef;
    public GameObject EditBuildingPanel;
    public GameObject ViewNotesPanel;
    public GameObject AddFloorPanel;
    public GameObject CreatingFloorPanel;
    public GameObject ConfirmDeleteFloorPanel;
    public GameObject FloorItemListContent;
    public GameObject FloorItemListPrefab;
    public GameObject FloorPrefab;
    public GameObject FloorContent;
    public List<FloorItemListPrefabScript> FloorItemListPrefabScriptsList = new List<FloorItemListPrefabScript>();
    public bool ShowHeatmap = false;
    public List<int> SelectedFloorsIds;

    private FloorCreationData FloorCreationDataRef;
    private int CreatingFloorState = 0;
    private int SelectedFloorIndexInFloorList;



    public FloorItemListPrefabScript GetCurrentFloorItemListPrefabScript()
    {
        if (SelectedFloorIndexInFloorList >= 0 && SelectedFloorIndexInFloorList < FloorItemListPrefabScriptsList.Count)
        {
            return FloorItemListPrefabScriptsList[SelectedFloorIndexInFloorList];
        }
        return null;
    }

    public FloorItemListPrefabScript GetFloorItemListPrefabScriptFromId(int id)
    {
        foreach(FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
        {
            if(f.GetFloorStruct().Id == id)
            {
                return f;
            }
        }
        return null;
    }

    public int CurrentFloorIndex()
    {
        return SelectedFloorIndexInFloorList;
    }


    private void Awake()
    {
        if (_instance == null)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
            editBtn.interactable = false;
            deleteBtn.interactable = false;
            SelectedFloorIndexInFloorList = -1;
            SelectedFloorsIds = new List<int>();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

    }



    public void Clear()
    {
        foreach(FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
        {
            GameObject.Destroy(f.mesh);
            GameObject.Destroy(f.gameObject);
        }
        FloorItemListPrefabScriptsList.Clear();
        CreatingFloorState = 0;
        SelectedFloorIndexInFloorList = -1;
        SelectedFloorsIds.Clear();
        editBtn.interactable = false;
        deleteBtn.interactable = false;
    }



    public void AddFloorBtnOnClick()
    {
        AddFloorPanel.SetActive(true);
    }



    public void EditFloorBtnOnClick()
    {
        if(EditBuildingPanel.activeInHierarchy)
        {
            EditBuildingPanel.SetActive(false);
            TabsPanelScriptRef.Fold();
            editBtn.image.color = Color.white;
            if(SelectedFloorsIds.Count == 0)
            {
                editBtn.interactable = false;
            }
        }
        else
        {
            if(!TabsPanelScriptRef.IsShown())
            {
                TabsPanelScriptRef.Deploy();
            }
            if(ViewNotesPanel.activeInHierarchy)
            {
                ViewNotesPanel.SetActive(false);
                NotesPanelButton.image.color = Color.white;
            }
            EditBuildingPanel.SetActive(true);

            if (SelectedFloorIndexInFloorList >= 0)
            {
                SetSelectedFloorItem(SelectedFloorIndexInFloorList); // EditBuildingPanelScript will take values from GetCurrentFloorItemListPrefabScript() 
            }
            else
                EditBuildingPanel.GetComponent<EditBuildingPanelScript>().ResetValues();
            Color editBtnDownColor;
            ColorUtility.TryParseHtmlString("#A9D9D7", out editBtnDownColor);
            editBtn.image.color = editBtnDownColor;
        }
    }



    public void SetSelectedFloorItem(int indexInFloorList)
    {
        /*if (GetCurrentFloorItemListPrefabScript() != null)
        {
            GetCurrentFloorItemListPrefabScript().Deselect();
        }*/

        foreach(FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
        {
            f.Deselect();
        }

        editBtn.interactable = true;
        deleteBtn.interactable = true;

        SelectedFloorIndexInFloorList = indexInFloorList;
        SelectedFloorsIds.Clear();
        SelectedFloorsIds.Add(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Id);
        GetCurrentFloorItemListPrefabScript().Select();

        // update display on the tab panel
        if (TabsPanelScriptRef.IsShown())
        {
            if (EditBuildingPanel.activeInHierarchy)
            {
                EditBuildingPanel.GetComponent<EditBuildingPanelScript>().SetValues();
            }
            if (ViewNotesPanel.activeInHierarchy)
            {
                ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList();
            }
        }
    }



    public void UpdateViewNotesPanel()
    {
        if (ViewNotesPanel.activeInHierarchy && SelectedFloorIndexInFloorList >= 0)
        {
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList();
        }
    }



    public void ResetSelectedFloorItem()
    {
        foreach (FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
        {
            f.Deselect();
        }
        SelectedFloorIndexInFloorList = -1;
        if(!EditBuildingPanel.activeInHierarchy)
            editBtn.interactable = false;
        deleteBtn.interactable = false;
        SelectedFloorsIds.Clear();

        if (TabsPanelScriptRef.IsShown())
        {
            EditBuildingPanel.GetComponent<EditBuildingPanelScript>().ResetValues();
            //EditBuildingPanel.SetActive(false);
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().ClearNotesList();
            //ViewNotesPanel.SetActive(false);
            //TabsPanelScriptRef.Fold();
        }
    }



    public bool AddSelectedFloorId(int id)
    {
        if (ViewNotesPanel.activeInHierarchy)
        {
            SelectedFloorsIds.Add(id);
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList();
            return true;
        }
        return false;
    }



    public void RemoveSelectedFloorId(int id)
    {
        if (!ViewNotesPanel.activeInHierarchy)
        {
            ResetSelectedFloorItem();
        }
        else
        {
            SelectedFloorsIds.Remove(id);
            if (SelectedFloorsIds.Count == 0)
            {
                ResetSelectedFloorItem();
            }
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList();
        }
    }



    public void DeleteFloorBtnOnClick()
    {
        string floorString = "the floor named '" + GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name + "' and  all the associated notes";
        ConfirmDeleteFloorPanel.GetComponent<ConfirmDeleteFloorPanelScript>().Init(floorString);
        ConfirmDeleteFloorPanel.SetActive(true);
    }


    public void DeleteCurrentFloor()
    {
        if (SelectedFloorIndexInFloorList >= 0 && SelectedFloorIndexInFloorList < FloorItemListPrefabScriptsList.Count)
        {
            ProjectManagerScript.Instance.DeleteAllNotesFromFloor(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Id);
            ProjectManagerScript.Instance.DeleteFloorFolder(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name);
            ProjectManagerScript.Instance.SimulationSetup.FloorList.RemoveAt(SelectedFloorIndexInFloorList);
            ProjectManagerScript.Instance.SaveSetup();

            GameObject.Destroy(GetCurrentFloorItemListPrefabScript().mesh);
            GameObject.Destroy(GetCurrentFloorItemListPrefabScript().gameObject);

            FloorItemListPrefabScriptsList.RemoveAt(SelectedFloorIndexInFloorList);
            ResetSelectedFloorItem();
        }
    }



    public void ToogleHeatmapButtonOnClick()
    {
        ShowHeatmap = !ShowHeatmap;
        foreach(FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
        {
            f.UpdateHeatmapImage();
        }
    }



    public void CreateFloor(FloorCreationData data)
    {
        FloorCreationDataRef = data;
        CreatingFloorState = 1;
    }



    public void UpdateFloor(FloorCreationData data)
    {
        FloorCreationDataRef = data;
        CreatingFloorState = 3;
    }



    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.P))
        {
            foreach (FloorItemListPrefabScript f in FloorItemListPrefabScriptsList)
            {
                Debug.Log(f.Floor.Name + " (" + f.GetIndex() +")");
                Debug.Log(Convert.ToBase64String(f.texture2D.EncodeToPNG()));
            }
        }*/
        if (CreatingFloorState == 1 || CreatingFloorState == 3)
        {
            CreatingFloorPanel.SetActive(true);
            CreatingFloorState++;
        }
        else if (CreatingFloorState == 2) // create floor
        {
            if (ProjectManagerScript.Instance.CreateFloorFolder(FloorCreationDataRef.FloorName))
            {
                if (ProjectManagerScript.Instance.SaveImage(FloorCreationDataRef.FloorName, "FloorPlan.png", FloorCreationDataRef.Texture))
                {
                    GameObject floor = null;
                    CreateFloorMesh(FloorCreationDataRef, ref floor);

                    FloorStruct floorStruct = new FloorStruct
                    {
                        Name = FloorCreationDataRef.FloorName,
                        Id = FloorCreationDataRef.FloorId,
                        BlackHeight = FloorCreationDataRef.BlackHeight,
                        RedHeight = FloorCreationDataRef.RedHeight,
                        BlueHeight = FloorCreationDataRef.BlueHeight,
                        GreenHeight = FloorCreationDataRef.GreenHeight,
                        PositionX = 0.0f,
                        PositionY = 0.0f,
                        PositionZ = 0.0f,
                        Rotation = 0.0f,
                        scaleX = 1.0f,
                        scaleZ = 1.0f
                    };

                    // put floorStruct in the projectManager list
                    ProjectManagerScript.Instance.SimulationSetup.FloorList.Add(floorStruct);
                    // THEN create floor item list with the newly added FloorStruct (needs the floorStruct to be the simulation steup)
                    CreateFloorItemList(floor, FloorCreationDataRef.Texture, floorStruct.Id);

                    //Save in simulationSetup
                    ProjectManagerScript.Instance.SaveSetup();
                }
            }
            CreatingFloorPanel.SetActive(false);
            CreatingFloorState = 0;
        }
        else if(CreatingFloorState == 4) // update floor
        {
            // check what's new
            if(GetCurrentFloorItemListPrefabScript() != null)
            {
                //Debug.Log("updating floor " + GetCurrentFloorItemListPrefabScript().Floor.Name);
                int indexInProjectManager = ProjectManagerScript.Instance.SimulationSetup.GetFloorIndex(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name);
                FloorStruct modified = ProjectManagerScript.Instance.SimulationSetup.FloorList[SelectedFloorIndexInFloorList];

                bool nameChanged = FloorCreationDataRef.FloorName != GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name;
                if(nameChanged)
                {
                    //Debug.Log("floor name has changed to " + FloorCreationDataRef.FloorName);
                    modified.Name = FloorCreationDataRef.FloorName;
                    // rename floor folder
                    ProjectManagerScript.Instance.RenameFloorFolder(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Name, FloorCreationDataRef.FloorName);
                }

                bool levelChanged = FloorCreationDataRef.FloorId != GetCurrentFloorItemListPrefabScript().GetFloorStruct().Id;
                if(levelChanged)
                {
                    //Debug.Log("Level changed to " + FloorCreationDataRef.FloorLevel);
                    modified.Id = FloorCreationDataRef.FloorId;
                    ProjectManagerScript.Instance.ChangeNotesFloorId(GetCurrentFloorItemListPrefabScript().GetFloorStruct().Id, modified.Id);
                }


                bool needToUpdateMesh = FloorCreationDataRef.BlackHeight != GetCurrentFloorItemListPrefabScript().GetFloorStruct().BlackHeight ||
                    FloorCreationDataRef.RedHeight != GetCurrentFloorItemListPrefabScript().GetFloorStruct().RedHeight ||
                    FloorCreationDataRef.BlueHeight != GetCurrentFloorItemListPrefabScript().GetFloorStruct().BlueHeight ||
                    FloorCreationDataRef.GreenHeight != GetCurrentFloorItemListPrefabScript().GetFloorStruct().GreenHeight ||
                    CompareTextures(FloorCreationDataRef.Texture, GetCurrentFloorItemListPrefabScript().texture2D) == false;

                if (needToUpdateMesh)
                {
                    //Debug.Log("update mesh");
                    modified.BlackHeight = FloorCreationDataRef.BlackHeight;
                    modified.RedHeight = FloorCreationDataRef.RedHeight;
                    modified.BlueHeight = FloorCreationDataRef.BlueHeight;
                    modified.GreenHeight = FloorCreationDataRef.GreenHeight;
                    GetCurrentFloorItemListPrefabScript().SetFloorTexture(FloorCreationDataRef.Texture);

                    CreateFloorMesh(FloorCreationDataRef, ref GetCurrentFloorItemListPrefabScript().mesh);
                    GetCurrentFloorItemListPrefabScript().mesh.transform.position = new Vector3(modified.PositionX, modified.PositionY, modified.PositionZ);
                    GetCurrentFloorItemListPrefabScript().mesh.transform.eulerAngles = new Vector3(0.0f, modified.Rotation, 0.0f);
                    GetCurrentFloorItemListPrefabScript().mesh.transform.localScale = new Vector3(modified.scaleX, 1.0f, modified.scaleZ);
                }

                GetCurrentFloorItemListPrefabScript().SetFloorStruct(modified);
                GetCurrentFloorItemListPrefabScript().UpdateButtonName();
                ProjectManagerScript.Instance.SaveSetup();
                if (FloorCreationDataRef.FloorName != "") {
                    ProjectManagerScript.Instance.SaveImage(FloorCreationDataRef.FloorName, 
                            "FloorPlan.png", 
                            GetCurrentFloorItemListPrefabScript().texture2D);
                    ProjectManagerScript.Instance.SaveImage(FloorCreationDataRef.FloorName, 
                            "Heatmap.png", 
                            GetCurrentFloorItemListPrefabScript().Heatmap);
                }

                EditBuildingPanel.GetComponent<EditBuildingPanelScript>().SetValues();
            }

            CreatingFloorPanel.SetActive(false);
            CreatingFloorState = 0;
        }
    }



    public void CreateFloorMesh(FloorCreationData data, ref GameObject floor)
    {
        if (floor == null)
        {
            floor = GameObject.Instantiate(FloorPrefab, FloorContent.transform);
        }

        ImageExtruder imageExtruder = new ImageExtruder();
        FloorPrefabScript script = floor.GetComponent<FloorPrefabScript>();

        Texture2D texture = imageExtruder.BlackOrWhite(data.Texture);
        texture = imageExtruder.CorrectColor(texture);

        imageExtruder.Extrude(
            texture,
            Color.black,
            0.01f,
            script.BlackCeilMeshFilter,
            script.BlackWallsMeshFilter,
            data.BlackHeight,
            true,
            script.GroundMeshFilter);

        script.GroundMeshFilter.GetComponent<Renderer>().material.mainTexture = data.Texture;

        imageExtruder.Extrude(
            texture,
            Color.red,
            0.01f,
            script.RedCeilMeshFilter,
            script.RedWallsMeshFilter,
            data.RedHeight
            );

        imageExtruder.Extrude(
            texture,
            Color.blue,
            0.01f,
            script.BlueCeilMeshFilter,
            script.BlueWallsMeshFilter,
            data.BlueHeight
            );

        imageExtruder.Extrude(
            texture,
            Color.green,
            0.01f,
            script.GreenCeilMeshFilter,
            script.GreenWallsMeshFilter,
            data.GreenHeight
            );
    }


    public void CreateFloorItemList(GameObject floorMesh, Texture2D floorTexture, int floorId)
    {
        GameObject floorItemList = GameObject.Instantiate(FloorItemListPrefab, FloorItemListContent.transform);

        floorItemList.GetComponent<FloorItemListPrefabScript>().Init( 
                                    floorMesh, 
                                    floorTexture, 
                                    this, 
                                    floorId);

        FloorItemListPrefabScriptsList.Add(floorItemList.GetComponent<FloorItemListPrefabScript>());
    }


    public bool CompareTextures(Texture2D t1, Texture2D t2)
    {
        return t1.EncodeToPNG().SequenceEqual(t2.EncodeToPNG());
    }
}
