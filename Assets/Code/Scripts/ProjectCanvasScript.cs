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

﻿using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleFileBrowser;
using System;
using System.Collections.Generic;

public class ProjectCanvasScript : MonoBehaviour
{
    public GameObject RecentPanel;
    public GameObject NewPanel;
    public Button StartNewProjectButton;
    public Text ErrorPanelText;
    public GameObject ProjectBtnContent;
    public GameObject ProjectBtnPrefab;
    public InputField ProjectNameInputField;
    public InputField SessionKeyInputField;
    public Text PreferedDirectoryProject;

    private FileBrowser fileBrowser;
    private ListProjectClass listProject;
    private string recentXmlFileName;

    void Start()
    {
        recentXmlFileName = Path.Combine(Application.dataPath, "Recent.xml");

        if (File.Exists(recentXmlFileName))
        {
            listProject = ListProjectClass.Load(recentXmlFileName);
        }
        else
        {
            listProject = new ListProjectClass();
            listProject.PreferedDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }

        PreferedDirectoryProject.text = listProject.PreferedDirectory;

        for (int index = 0; index < listProject.Projects.Count; ++index)
        {
            if (File.Exists(listProject.Projects[index]))
            {
                SimulationSetupClass simulationSetup = SimulationSetupClass.Load(listProject.Projects[index]);
                GameObject tmp = Instantiate(ProjectBtnPrefab, ProjectBtnContent.transform);
                tmp.GetComponent<ProjectBtnPrefabScript>().Init(simulationSetup.ProjectName, listProject.Projects[index], this);
            }
            else
            {
                listProject.Projects.RemoveAt(index);
                --index;
            }
        }

        listProject.Save(recentXmlFileName);
        ErrorPanelText.text = "";
    }

    public void RecentBtnOnClick()
    {
        if (!FileBrowser.IsOpen)
        {
            RecentPanel.SetActive(true);
            NewPanel.SetActive(false);
        }
    }

    public void NewBtnOnClick()
    {
        if (!FileBrowser.IsOpen)
        {
            RecentPanel.SetActive(false);
            NewPanel.SetActive(true);
            ErrorPanelText.text = "";
            SessionKeyInputField.text = RandomString.CreateRandomString(4).ToUpper();
        }
    }

    public void OpenBtnOnClick()
    {
        if (!FileBrowser.IsOpen)
        {
            FileBrowser.SetFilters(false, ".xml");
            FileBrowser.ShowLoadDialog(OpenBtnOnSuccess, null, false, PreferedDirectoryProject.text, "Select simulation.xml", "Select");
        }
    }

    public void OpenBtnOnSuccess(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                ProjectManagerScript.Instance.SimulationSetup = SimulationSetupClass.Load(path);

                if (listProject.Exist(path) == -1)
                {
                    listProject.Projects.Add(path);
                    listProject.Save(recentXmlFileName);
                }

                CheckAndRepairHierarchy(Path.GetDirectoryName(path));
                ProjectManagerScript.Instance.ProjectFullPath = Path.GetDirectoryName(path);
                SceneManager.LoadScene(1);

            }
            catch (Exception e)
            {
                Debug.Log(e.Source);
                Debug.Log(e.Message);
                ErrorPanelText.text = e.Message;
            }
        }
    }
    
    public void ExitBtnOnClick()
    {
        Application.Quit();
    }

    public void SetDirectoryOnClick()
    {
        if (!FileBrowser.IsOpen)
        {
            FileBrowser.ShowLoadDialog(SetDirectoryOnSuccess, null, true, PreferedDirectoryProject.text, "Select path", "Select");
        }
    }

    public void SetDirectoryOnSuccess(string path)
    {
        PreferedDirectoryProject.text = path;
    }

    public void ProjectNameInputonValueChanged()
    {
        string newName = ProjectNameInputField.text;
        bool projectNameTaken = false;
        foreach(string projectPath in listProject.Projects)
        {
            string projectName = new FileInfo(projectPath).Directory.Name;
            projectNameTaken |= projectName == newName;
        }

        if(projectNameTaken)
        {
            ErrorPanelText.text = "A project with this name already exists";
            ProjectNameInputField.image.color = new Color(1, 0.8f, 0.8f);
            StartNewProjectButton.interactable = false;
        }
        else
        {
            ErrorPanelText.text = "";
            ProjectNameInputField.image.color = Color.white;
            StartNewProjectButton.interactable = true;
        }
    }

    public void StartBtnOnClick()
    {
        string directory = PreferedDirectoryProject.text;
        string name = ProjectNameInputField.text;
        string session = SessionKeyInputField.text;

        if (name != "")
        {
            string projectFolderFullPath = Path.Combine(directory, name);
            string simulationXmlFullPath = Path.Combine(projectFolderFullPath, "simulation.xml");

            if(File.Exists(simulationXmlFullPath))
            {
                Debug.Log("A project with this name already exists");
                ErrorPanelText.text = "A project with this name already exists";
                return;
            }


            if (!Directory.Exists(projectFolderFullPath))
            {
                Directory.CreateDirectory(projectFolderFullPath);
            }

            CheckAndRepairHierarchy(projectFolderFullPath);

            SimulationSetupClass newSimulation = new SimulationSetupClass
            {
                ProjectName = name,
                SessionKey = session
            };

            listProject.Projects.Add(simulationXmlFullPath);
            listProject.Save(recentXmlFileName);

            ProjectManagerScript.Instance.ProjectFullPath = projectFolderFullPath;
            ProjectManagerScript.Instance.SimulationSetup = newSimulation;
            ProjectManagerScript.Instance.SaveSetup();

            SceneManager.LoadScene(1);
        }
    }

    public void CheckAndRepairHierarchy(string directory)
    {
        string floorsFolderFullPath = Path.Combine(directory, "Floors");
        //string notesFolderFullPath = Path.Combine(directory, "Notes");

        if (!Directory.Exists(floorsFolderFullPath))
        {
            Directory.CreateDirectory(floorsFolderFullPath);
        }

        /*if (!Directory.Exists(notesFolderFullPath))
        {
            Directory.CreateDirectory(notesFolderFullPath);
        }*/
    }
}
