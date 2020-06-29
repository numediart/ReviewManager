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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TabButtonsScript : MonoBehaviour
{

    public Text ProjectNameText;
    public Button NotesPanelButton;
    public Button EditFloorButton;
    public TabsPanelScript TabsPanelScriptRef;
    public GameObject EditBuildingPanel;
    public GameObject ViewNotesPanel;
    public GameObject ConfirmExitPanel;
    public WebsocketServerScript WebsocketServerScriptRef;
    public Text StartServerTxt;
    public Image ServerStateImg;
    public Text ServerStatusTxt;


    public void Start()
    {
        ProjectNameText.text = "Project : <size=20>" + 
            ProjectManagerScript.Instance.SimulationSetup.ProjectName +
            "</size>";
    }


    public void ViewNoteBtnOnClick()
    {
        if(ViewNotesPanel.activeInHierarchy)
        {
            ViewNotesPanel.SetActive(false);
            ViewNotesPanel.GetComponent<ViewNotesPanelScript>().UpdateNotesList(); // update 3D view too
            TabsPanelScriptRef.Fold();
            NotesPanelButton.image.color = Color.white;
        }
        else
        {
            EditBuildingPanel.SetActive(false);
            EditFloorButton.image.color = Color.white;
            ViewNotesPanel.SetActive(true);
            if(!TabsPanelScriptRef.IsShown())
                TabsPanelScriptRef.Deploy();
            FloorsManagerScript.Instance.UpdateViewNotesPanel();
            Color notesPanelDownColor;
            ColorUtility.TryParseHtmlString("#A9D9D7", out notesPanelDownColor);
            NotesPanelButton.image.color = notesPanelDownColor;
        }
    }



    public void StartServerBtnOnClick()
    {
        if (WebsocketServerScriptRef.IsServerStarted())
        {
            WebsocketServerScriptRef.StopServer();
            ServerStateImg.color = Color.red;
            StartServerTxt.text = "Start Server";
            ServerStatusTxt.text = "";
        }
        else
        {
            WebsocketServerScriptRef.StartServer(ProjectManagerScript.Instance.SimulationSetup.SessionKey);
            ServerStateImg.color = Color.green;
            StartServerTxt.text = "Stop Server";
            ServerStatusTxt.text = "Server IP is <size=20>" + WebsocketServerScriptRef.getIPAddress() +
                                    "</size>, port : <size=20>" + WebsocketServerScriptRef.WebsocketPort +
                                    "</size>, session id : <size=20>" + WebsocketServerScriptRef.SessionKey +
                                    "</size>";
        }

    }



        /*public void StartServerBtnOnClick()
        {
            if (tcpFileServerScriptRef.isServerStarted)
            {
                tcpFileServerScriptRef.StopServer();
                ServerStateImg.color = Color.red;
                StartServerTxt.text = "Start Server";
                ServerStatusTxt.text = "";
            }
            else
            {
                tcpFileServerScriptRef.StartServer();
                ServerStateImg.color = Color.green;
                StartServerTxt.text = "Stop Server";
                ServerStatusTxt.text = "Server IP is " + tcpFileServerScriptRef.getIPAddress() + ", port " + tcpFileServerScriptRef.RcvPort;
            }
        }*/



    public void TitleBtnOnClick()
    {
        if(WebsocketServerScriptRef.IsServerStarted())
        {
            WebsocketServerScriptRef.StopServer();
        }
        ProjectManagerScript.Instance.Clear();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void ExitBtnOnClick()
    {
        ConfirmExitPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ConfirmExitPanel.SetActive(true);
        }
    }
}
