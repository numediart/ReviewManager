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
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProjectManagerScript : MonoBehaviour
{
    private static ProjectManagerScript _instance = null;
    public static ProjectManagerScript Instance { get { return _instance; } }
    public string ProjectFullPath;
    public SimulationSetupClass SimulationSetup;
    public bool IsLoaded;
    public GameObject NotePrefab;
    public List<NoteStruct> NotesList = new List<NoteStruct>();
    public Vector2 RescaledImageSize;

    private FloorsManagerScript floorsManagerScriptRef;
    private GameObject loadingPanel;
    private Text loadingTxt;

    private volatile bool busyCreatingNote = false;
    private volatile string newNoteString = "";
    private volatile string noteCreatedAnswerString = "";



    private void Awake()
    {
        if (_instance == null)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            IsLoaded = false;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }



    public void Clear()
    {
        ProjectFullPath = "";
        IsLoaded = false;
        NotesList.Clear();
        FloorsManagerScript.Instance.Clear();
        busyCreatingNote = false;
        newNoteString = "";
        noteCreatedAnswerString = "";
    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        switch (scene.buildIndex)
        {
            case 0:
                break;

            case 1:
                floorsManagerScriptRef = GameObject.Find("FloorListPanel").GetComponent<FloorsManagerScript>();
                floorsManagerScriptRef.ResetSelectedFloorItem();
                loadingPanel = GameObject.Find("LoadingPanel");
                loadingTxt = GameObject.Find("LoadingTxt").GetComponent<Text>();

                StartCoroutine(LoadFloors());
                break;

            default:
                break;
        }
    }



    private bool isNoteMessage(string mes)
    {
        bool result = true;
        string[] expectedValues = { "Title", "Content" , "Author", "Session", "Date", "Emotion", "Location", "Image"};
        foreach(string val in expectedValues)
        {
            result &= mes.Contains(val);
        }
        result &= mes.StartsWith("{");
        result &= mes.EndsWith("}");
        return result;
    }



    public void HandleNoteMessage(string m)
    {
        //Debug.Log(m);
        if (isNoteMessage(m))
        {   // json containing note data
            busyCreatingNote = true;
            newNoteString = m;
        }
        else
        {
            noteCreatedAnswerString = "INCORRECT_NOTE_FORMAT";
        }
    }



    public bool IsBusyCreatingNote()
    {
        return busyCreatingNote;
    }



    public string GetCreateNoteAnswer()
    {
        string temp = noteCreatedAnswerString;
        noteCreatedAnswerString = "";
        return temp;
    }



    public void Update()
    {
        if (busyCreatingNote)
        {
            DateTime now = DateTime.Now;
            string filename = now.ToString("yyyyMMdd_HHmmssfff");

            ReceivedData rcv;
            try
            {
               rcv = ReceivedData.CreateFromJSON(newNoteString);
            }
            catch(ArgumentException e)
            {
                Debug.LogError(e);
                noteCreatedAnswerString = "JSON_PARSING_ERROR";
                busyCreatingNote = false;
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                noteCreatedAnswerString = "ERROR";
                busyCreatingNote = false;
                return;
            }

            string newFloorNotesFolderFullPath = GetNotesFolder(rcv.Location.Id);

            if (newFloorNotesFolderFullPath != "")
            {
                newFloorNotesFolderFullPath = Path.Combine(newFloorNotesFolderFullPath, filename);

                try
                {
                    // save image as jpg
                    byte[] picBytes = Convert.FromBase64String(rcv.Image);
                    File.WriteAllBytes(newFloorNotesFolderFullPath + ".jpg", picBytes);
                    //Debug.Log("Original image saved as " + newFloorNotesFolderFullPath + ".jpg");
                    Texture2D pic = new Texture2D(1, 1);
                    pic.LoadImage(picBytes);
                    pic.Apply();
                    ResizeTexture(ref pic, (int)RescaledImageSize.x, (int)RescaledImageSize.y);
                    File.WriteAllBytes(newFloorNotesFolderFullPath + "_light.jpg", pic.EncodeToJPG(75));
                    //Debug.Log("Resized image saved as " + newFloorNotesFolderFullPath + "_light.jpg");

                    // save other data as json
                    rcv.Image = filename; // replace huge string representing the image with its filename
                    rcv.ImageOrientation = 270; // force image to portrait mode (application will force that anyway)
                    File.WriteAllText(newFloorNotesFolderFullPath + ".json", JsonUtility.ToJson(rcv, true));
                    Debug.Log("Metadata saved as " + newFloorNotesFolderFullPath + ".json");

                    bool noteCreated = CreateNote(rcv, pic);
                    if (noteCreated)
                    {
                        NoteStruct n = NotesList[NotesList.Count - 1];
                        int floorIndex = SimulationSetup.GetFloorIndex(rcv.Location.Id);
                        FloorItemListPrefabScript floorScript = floorsManagerScriptRef.FloorItemListPrefabScriptsList[floorIndex];
                        floorScript.UpdateHeatmap(n);
                        SaveImage(floorScript.GetFloorStruct().Name, "Heatmap.png", floorScript.Heatmap);
                    }
                }
                catch (FormatException e)
                {
                    Debug.LogError(e);
                    noteCreatedAnswerString = "TEXTURE_CONVERSION_ERROR";
                    busyCreatingNote = false;
                    return;
                }
                catch (IOException e)
                {
                    Debug.LogError(e);
                    noteCreatedAnswerString = "SAVE_DATA_ERROR";
                    busyCreatingNote = false;
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    noteCreatedAnswerString = "ERROR";
                    busyCreatingNote = false;
                    return;
                }
            }
            else
            {
                noteCreatedAnswerString = "UNKNOWN_FLOOR_ID";
                busyCreatingNote = false;
                return;
            }
            noteCreatedAnswerString = "OK";
            newNoteString = "";
            busyCreatingNote = false;
            floorsManagerScriptRef.UpdateViewNotesPanel();
        }
    }



    public void SaveNote(ReceivedData rcv)
    {
        string newFloorNotesFolderFullPath = GetNotesFolder(rcv.Location.Id);

        if (newFloorNotesFolderFullPath != "")
        {
            newFloorNotesFolderFullPath = Path.Combine(newFloorNotesFolderFullPath, rcv.Image);
            File.WriteAllText(newFloorNotesFolderFullPath + ".json", JsonUtility.ToJson(rcv, true));
        }
    }


    public void UpdateNotePositionAndRotation(int noteIndex)
    {
        NoteStruct n = NotesList[noteIndex];
        // move gameobject to position
        int floorIndex = SimulationSetup.GetFloorIndex(NotesList[noteIndex].Data.Location.Id);
        if (floorIndex >= 0)
        {
            FloorItemListPrefabScript floorScript = floorsManagerScriptRef.FloorItemListPrefabScriptsList[floorIndex];
            Mesh tmp = floorScript.mesh.GetComponent<FloorPrefabScript>().GroundMeshFilter.mesh;
            Vector3 pos00 = tmp.vertices[0];
            Vector3 pos11 = tmp.vertices[3];
            float x = pos00.x - NotesList[noteIndex].Data.Location.XCoordinate * (pos00.x - pos11.x);
            float z = pos11.z - NotesList[noteIndex].Data.Location.YCoordinate * (pos11.z - pos00.z);
            n.ObjRef.transform.localPosition = new Vector3(x, 1.0f, z);
            n.ObjRef.GetComponent<NotePrefabScript>().Init(n.Image, n.Data.ImageOrientation);
            floorScript.CreateHeatmap();
            SaveImage(floorScript.GetFloorStruct().Name, "Heatmap.png", floorScript.Heatmap);
        }
    }



    private void ResizeTexture(ref Texture2D texture, int targetW, int targetH)
    {
        float targetAR = (1.0f * targetW) / targetH;
        float textureAR = (1.0f * texture.width) / texture.height;
        if (targetAR > textureAR)
        {
            targetW = (int)Math.Round(targetH * textureAR);
        }
        else
        {
            targetH = (int)Math.Round(targetW / textureAR);
        }
        TextureScale.Bilinear(texture, targetW, targetH);
    }



    private bool CreateNote(ReceivedData rcv, Texture2D texture)
    {
        float x = 0, z = 0;
        GameObject NotePrefabContent;

        int floorIndex = SimulationSetup.GetFloorIndex(rcv.Location.Id);
        if (floorIndex >= 0)
        {
            FloorItemListPrefabScript floorScript = floorsManagerScriptRef.FloorItemListPrefabScriptsList[floorIndex];

            Mesh tmp = floorScript.mesh.GetComponent<FloorPrefabScript>().GroundMeshFilter.mesh;
            Vector3 pos00 = tmp.vertices[0];
            Vector3 pos11 = tmp.vertices[3];

            x = pos00.x - rcv.Location.XCoordinate * (pos00.x - pos11.x);
            z = pos11.z - rcv.Location.YCoordinate * (pos11.z - pos00.z);
            //Debug.Log("note xz :" + x + ", " + z);

            NotePrefabContent = floorScript.mesh.GetComponent<FloorPrefabScript>().NoteContent;

            GameObject noteInstance = GameObject.Instantiate(NotePrefab, NotePrefabContent.transform);
            noteInstance.GetComponent<NotePrefabScript>().Init(texture, rcv.ImageOrientation);

            noteInstance.transform.localPosition = new Vector3(x, 1.0f, z);
            Vector3 floorScale = floorScript.mesh.transform.localScale;
            noteInstance.transform.localScale = new Vector3(1.0f / floorScale.x, 1.0f, 1.0f / floorScale.z);

            NoteStruct newNote = new NoteStruct
            {
                Data = rcv,
                ObjRef = noteInstance,
                Image = texture
            };

            NotesList.Add(newNote);
            return true;
        }
        return false;
    }



    IEnumerator LoadFloors()
    {
        int i = 1;

        foreach (FloorStruct floor in SimulationSetup.FloorList)
        {
            loadingTxt.text = "Loading floor " + i.ToString() + " on " + SimulationSetup.FloorList.Count.ToString() + "...";
            string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
            string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, floor.Name);

            if (Directory.Exists(newFloorFolderFullPath))
            {
                WWW www = new WWW(Path.Combine(newFloorFolderFullPath, "FloorPlan.png"));

                while (!www.isDone)
                {
                    yield return null;
                }
                FloorCreationData data = new FloorCreationData
                {
                    FloorName = floor.Name,
                    FloorId = floor.Id,
                    Texture = www.texture,
                    BlackHeight = floor.BlackHeight,
                    RedHeight = floor.RedHeight,
                    BlueHeight = floor.BlueHeight,
                    GreenHeight = floor.GreenHeight
                };
                GameObject floorMesh = null;
                floorsManagerScriptRef.CreateFloorMesh(data, ref floorMesh);
                floorMesh.transform.position = new Vector3(floor.PositionX, floor.PositionY, floor.PositionZ);
                floorMesh.transform.eulerAngles = new Vector3(0.0f, floor.Rotation, 0.0f);
                floorMesh.transform.localScale = new Vector3(floor.scaleX, 1.0f, floor.scaleZ);
                floorsManagerScriptRef.CreateFloorItemList(floorMesh, www.texture, floor.Id);
                yield return null;
            }

            ++i;
        }

        StartCoroutine(LoadNotes());
    }



    IEnumerator LoadNotes()
    {
        foreach (FloorStruct floor in SimulationSetup.FloorList)
        {
            string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
            string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, floor.Name);
            string newFloorNotesFolderFullPath = Path.Combine(newFloorFolderFullPath, "Notes");

            List<string> files = new List<string>(Directory.GetFiles(newFloorNotesFolderFullPath, "*.json"));

            int i = 1;
            foreach (string file in files)
            {
                loadingTxt.text = "Loading notes " + i.ToString() + " on " + files.Count + " for floor " + floor.Name + "...";
                StreamReader reader = new StreamReader(file);
                string json = reader.ReadToEnd();
                reader.Close();
                ReceivedData rcv = null;
                try
                {
                    rcv = JsonUtility.FromJson<ReceivedData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (rcv != null)
                {
                    string lightImagePath = Path.Combine(newFloorNotesFolderFullPath, rcv.Image + "_light.jpg");
                    string origImagePath = Path.Combine(newFloorNotesFolderFullPath, rcv.Image + ".jpg");
                    bool needToResize = false;
                    WWW www;
                    if (File.Exists(lightImagePath))
                    { 
                         www = new WWW(lightImagePath);
                    }
                    else
                    {
                        www = new WWW(origImagePath);
                        needToResize = true;
                    }

                    while (!www.isDone)
                    {
                        yield return null;
                    }

                    Texture2D texture = www.texture;
                    if(needToResize)
                    {
                        ResizeTexture(ref texture, (int)RescaledImageSize.x, (int)RescaledImageSize.y);
                        File.WriteAllBytes(lightImagePath, texture.EncodeToJPG(100));
                        Debug.Log("Resized image created and saved : " + lightImagePath);
                    }
                    CreateNote(rcv, texture);
                    yield return null;
                }
                ++i;
            }
        }

        foreach (FloorItemListPrefabScript f in floorsManagerScriptRef.FloorItemListPrefabScriptsList)
        {
            f.CreateHeatmap();
            SaveImage(f.GetFloorStruct().Name, "Heatmap.png", f.Heatmap);
        }
        IsLoaded = true;
        loadingPanel.SetActive(false);
    }



    public bool CreateFloorFolder(string FloorName)
    {
        try
        {
            string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
            string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, FloorName);

            if (!Directory.Exists(newFloorFolderFullPath))
            {
                Directory.CreateDirectory(newFloorFolderFullPath);
            }

            string newFloorNotesFolderFullPath = Path.Combine(newFloorFolderFullPath, "Notes");

            if (!Directory.Exists(newFloorNotesFolderFullPath))
            {
                Directory.CreateDirectory(newFloorNotesFolderFullPath);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }


    public bool RenameFloorFolder(string currentName, string newName)
    {
        try
        {
            string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
            string currentFloorFolderFullPath = Path.Combine(floorsFolderFullPath, currentName);
            string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, newName);

            if (!Directory.Exists(currentFloorFolderFullPath))
            {
                throw new FileNotFoundException("No floor folder named '" + currentName + "' found");
            }

            Debug.Log("Rename " + currentFloorFolderFullPath + " to " + newFloorFolderFullPath);
            Directory.Move(currentFloorFolderFullPath, newFloorFolderFullPath);
            if (!Directory.Exists(newFloorFolderFullPath))
            {
                throw new IOException("Impossible to rename " + currentFloorFolderFullPath + " to " + newFloorFolderFullPath);
            }

            string newFloorNotesFolderFullPath = Path.Combine(newFloorFolderFullPath, "Notes");
            if (!Directory.Exists(newFloorNotesFolderFullPath))
            {
                Directory.CreateDirectory(newFloorNotesFolderFullPath);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }



    public bool DeleteFloorFolder(string FloorName)
    {
        try
        {
            string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
            string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, FloorName);

            if (Directory.Exists(newFloorFolderFullPath))
            {
                Directory.Delete(newFloorFolderFullPath, true);
                return true;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }



    public List<int> GetAllFloorIds()
    {
        List<int> floorIds = new List<int>();
        foreach(FloorStruct f in SimulationSetup.FloorList)
        {
            floorIds.Add(f.Id);
        }
        return floorIds;
    }



    public string GetNotesFolder(int floorId)
    {
        foreach (FloorStruct info in SimulationSetup.FloorList)
        {
            if (info.Id == floorId)
            {
                string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
                string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, info.Name);
                string newFloorNotesFolderFullPath = Path.Combine(newFloorFolderFullPath, "Notes");

                if (Directory.Exists(newFloorNotesFolderFullPath))
                {
                    return newFloorNotesFolderFullPath;
                }
            }
        }

        return "";
    }



    public void ChangeNotesFloorId(int currentId, int newId)
    {
        foreach (NoteStruct n in NotesList)
        {
            if (n.Data.Location.Id == currentId)
            {
                n.Data.Location.Id = newId;
                SaveNote(n.Data);
            }
        }
    }



    public void DeleteAllNotesFromFloor(int floorId)
    {
        NoteFilter floorFilter = new NoteFilter();
        floorFilter.AddFloor(floorId);
        foreach(NoteStruct n in GetNotesFiltered(floorFilter))
        {
                DeleteNote(n);
        }
    }



    public List<NoteStruct> GetNotesFiltered(NoteFilter filter)
    {
        Vector2 filterEmotionVector = new Vector2(filter.EmotionCloseTo.Valence, filter.EmotionCloseTo.Intensity);
        bool filterByTags = filter.Tags != "";
        List<string> filterTags = filter.Tags.Split(',').ToList();

        List<NoteStruct> filteredNotes = new List<NoteStruct>();
        foreach (NoteStruct n in NotesList)
        {
            Vector2 noteEmotionVector = new Vector2(n.Data.Emotion.Valence, n.Data.Emotion.Intensity);
            List<string> noteTags = n.Data.Tags.Split(',').ToList();
            bool tagsMatch = false;
            foreach(string t in noteTags)
            {
                if(filterTags.Contains(t))
                {
                    tagsMatch = true;
                    break;
                }
            }

            string auth = n.Data.Author;
            if (auth == "")
                auth = "(Empty)";

            bool addToList = filter.FloorsIds.IndexOf(n.Data.Location.Id) >= 0 &&
                (!filter.WithMessageOnly || n.Data.Content != "") &&
                (filterEmotionVector - noteEmotionVector).magnitude <= filter.EmotionDistance &&
                (!filterByTags || tagsMatch) &&
                (filter.Authors.Count == 0 || filter.Authors.Contains(auth));

            if(addToList)
            {
                filteredNotes.Add(n);
            }
        }
        return filteredNotes;
    }



    public List<string> GetAllAuthors()
    {
        List<string> authors = new List<string>();
        foreach(NoteStruct n in NotesList)
        {
            string auth = n.Data.Author;
            if (auth == "")
                auth = "(Empty)";
            if (!authors.Contains(auth))
            {
                authors.Add(auth);
            }
        }
        return authors;
    }



    public List<string> GetAllAuthorsFromLevels(List<int> levels)
    {
        List<string> authors = new List<string>();
        foreach (NoteStruct n in NotesList)
        {
            string auth = n.Data.Author;
            if (auth == "")
                auth = "(Empty)";
            if (!authors.Contains(auth) && levels.Contains(n.Data.Location.Id))
            {
                authors.Add(auth);
            }
        }
        return authors;
    }



    public NoteStruct GetNoteByPosition3D(Vector3 pos)
    {
        foreach (NoteStruct n in NotesList)
        {
            //Debug.Log("ref = " + pos + ", note is at " + n.ObjRef.transform.position);
            if (Vector3.Distance(pos, n.ObjRef.transform.position) < 0.05f)
            {
                return n;
            }
        }
        return new NoteStruct();
    }



    public int GetNoteIndexByPosition3D(Vector3 pos)
    {
        int i = 0;
        foreach (NoteStruct n in NotesList)
        {
            if (Vector3.Distance(pos, n.ObjRef.transform.position) < 0.05f)
            {
                return i;
            }
            i++;
        }
        return -1;
    }



    public int GetNoteIndex(NoteStruct note)
    {
        int i = 0;
        foreach (NoteStruct n in NotesList)
        {
            if (GameObject.ReferenceEquals(n.ObjRef, note.ObjRef))
            {
                return i;
            }
            i++;
        }
        return -1;
    }



    public void SetHighlightOnNote(int noteIndex)
    {
        if (noteIndex >= 0 && noteIndex < NotesList.Count)
        {
            NotesList[noteIndex].ObjRef.GetComponent<NotePrefabScript>().SetHighlight(true);
        }
    }



    public void DisableHighlightOnAllNotes()
    {
        foreach(NoteStruct n in NotesList)
        {
            n.ObjRef.GetComponent<NotePrefabScript>().SetHighlight(false);
        }
    }



    public void DeleteNote(NoteStruct note)
    {
        int index = GetNoteIndex(note);
        if(index >= 0)
        {
            DeleteNote(index);
        }
    }



    public void DeleteNote(int noteIndex)
    {
        Debug.Log("Delete note " + NotesList[noteIndex]);
        GameObject.Destroy(NotesList[noteIndex].ObjRef);
        string floorNotesFolderFullPath = GetNotesFolder(NotesList[noteIndex].Data.Location.Id);
        if (floorNotesFolderFullPath != "")
        {
            string noteFileFullPath = Path.Combine(floorNotesFolderFullPath, NotesList[noteIndex].Data.Image);
            if(File.Exists(noteFileFullPath + ".json"))
            {
                File.Delete(noteFileFullPath + ".json");
            }
            if (File.Exists(noteFileFullPath + ".jpg"))
            {
                File.Delete(noteFileFullPath + ".jpg");
            }
            if (File.Exists(noteFileFullPath + "_light.jpg"))
            {
                File.Delete(noteFileFullPath + "_light.jpg");
            }
        }
        NotesList.RemoveAt(noteIndex);
    }



    public void SaveSetup()
    {
        SimulationSetup.Save(Path.Combine(ProjectFullPath, "simulation.xml"));
    }



    public bool SaveImage(string FloorName, string imageName,  Texture2D texture)
    {
        string floorsFolderFullPath = Path.Combine(ProjectFullPath, "Floors");
        string newFloorFolderFullPath = Path.Combine(floorsFolderFullPath, FloorName);
        //Debug.Log("save image at " + newFloorFolderFullPath + "\\" + imageName);
        if (Directory.Exists(newFloorFolderFullPath))
        {
            //Debug.Log("overwrite image");
            File.WriteAllBytes(Path.Combine(newFloorFolderFullPath, imageName), texture.EncodeToPNG());
        }

        return true;
    }
}
