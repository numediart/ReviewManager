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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraManipulationScript : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Camera[] Cams;
    public RenderTexture[] CamRenderTextures;

    public Button ChangeCameraButton;
    public Button NoteSelectableButton;
    public GameObject NoteDetailPanelRef;
    public GameObject MainCanvas;
    private List<GameObject> foregroundPanels;

    private float visitorCamSpeed = 3.0f;
    private float visitorCamRotationSpeed = 60.0f;

    private int currentCam; // 0 : OmniscientCam, 1 : VisitorCam
    private float CameraAspect;
    private bool hasFocus = false;
    private bool notesSelectableFrom3DView = false;
    private int lastNoteIndex;

    public delegate void ClickAction(Vector3 position);
    public event ClickAction OnClicked;



    private void Awake()
    {
        currentCam = 0;
        CameraAspect = Cams[0].aspect;
        lastNoteIndex = -1;
        foregroundPanels = new List<GameObject>();
        foreach (Transform panel in MainCanvas.transform)
        {
            if(panel.name != "MainPanel")
            {
                foregroundPanels.Add(panel.gameObject);
            }
        }
    }



    public void ChangeCameraButtonOnClick()
    {
        if (currentCam == 0)
        {
            ChangeCameraButton.GetComponentInChildren<Text>().text = "Omniscient Camera";
            currentCam = 1;
        }
        else if (currentCam == 1)
        {
            ChangeCameraButton.GetComponentInChildren<Text>().text = "Visitor Camera";
            currentCam = 0;
        }
        CameraAspect = Cams[currentCam].aspect;
        GetComponent<RawImage>().texture = CamRenderTextures[currentCam];
    }



    public void NoteSelectableButtonOnClick()
    {
        notesSelectableFrom3DView = !notesSelectableFrom3DView;
        if(notesSelectableFrom3DView)
        {
            NoteSelectableButton.GetComponentInChildren<Text>().text = "Notes selectable";
        }
        else
        {
            NoteSelectableButton.GetComponentInChildren<Text>().text = "Notes unselectable";
            ProjectManagerScript.Instance.DisableHighlightOnAllNotes();
        }
    }


    private void Update()
    {
        Vector2 displaySize = GetComponent<RectTransform>().sizeDelta;
        float displayAspect = displaySize.x / displaySize.y;
        if (Math.Abs(displayAspect - CameraAspect) > 0.001f)
        {
            CameraAspect = displayAspect;
            Cams[currentCam].aspect = CameraAspect;
        }
        if (hasFocus && !NoteDetailPanelRef.activeInHierarchy)
        {
            if (notesSelectableFrom3DView)
            {
                int noteIndex = GetPointedNoteIndex(Input.mousePosition, Camera.current);
                if (noteIndex != lastNoteIndex)
                {
                    ProjectManagerScript.Instance.DisableHighlightOnAllNotes();
                    if (noteIndex >= 0)
                    {
                        ProjectManagerScript.Instance.SetHighlightOnNote(noteIndex);
                    }
                }
                lastNoteIndex = noteIndex;
            }

            if (currentCam == 0)
            {
                float scrollVal = Input.GetAxis("Mouse ScrollWheel");
                //Debug.Log(scrollVal);

                if (Cams[currentCam].orthographic)
                {
                    Cams[currentCam].orthographicSize -= scrollVal * Cams[currentCam].orthographicSize * 0.5f;
                }
                else
                {
                    float dist = Vector3.Distance(Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt, Cams[currentCam].transform.position);
                    float factor;

                    if (dist < 1.0f)
                    {
                        factor = 0.2f;
                    }
                    else
                    {
                        factor = Mathf.Round(dist * 0.5f);
                    }

                    //else if (dist < 10.0f)
                    //{
                    //    factor = 2.0f;
                    //}
                    //else if (dist < 100.0f)
                    //{
                    //    factor = 20.0f;
                    //}
                    //else if (dist < 1000.0f)
                    //{
                    //    factor = 200.0f;
                    //}
                    //else
                    //{
                    //    factor = 1000.0f;
                    //}

                    Cams[currentCam].transform.position += Cams[currentCam].transform.forward * scrollVal * factor;
                }

                Cams[currentCam].gameObject.GetComponent<CameraDataScript>().UpdateTargetPosition();
            }
        }

        if (currentCam == 1 && !GuiHelper.IsInputFieldElementSelected() && isFront())
        {
            float translation = Input.GetAxis("Vertical") * visitorCamSpeed;
            translation *= Time.deltaTime;
            Cams[currentCam].transform.Translate(0, 0, translation);

            float rotation = Input.GetAxis("Horizontal") * visitorCamRotationSpeed;
            rotation *= Time.deltaTime;
            Cams[currentCam].transform.Rotate(0, rotation, 0);

            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.PageUp))
            {
                // go up
                Cams[currentCam].transform.Translate(0, visitorCamSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.PageDown))
            {
                // go down
                Cams[currentCam].transform.Translate(0, -visitorCamSpeed * Time.deltaTime, 0);
            }
        }
    }


    /*
        public void OrthographicCameraProjectionModeBtnOnClick()
        {
            OmniscientCam.orthographic = true;
        }

        public void PerspectiveCameraProjectionModeBtnOnClick()
        {
            OmniscientCam.orthographic = false;
        }
    */


    public void OnDrag(PointerEventData eventData)
    {
        Cursor.lockState = CursorLockMode.Confined;
        //OmniscientCam.gameObject.GetComponent<CameraDataScript>().Target.SetActive(true);

        if (currentCam == 0)
        {
            float dist = Vector3.Distance(Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt, Cams[currentCam].transform.position);
            float factor;

            if (dist < 1.0f)
            {
                factor = 0.05f;
            }
            else
            {
                factor = dist * 0.05f;
            }

            if (eventData.pointerId == -1)
            {
                Vector3 forwardProjection = new Vector3(Cams[currentCam].transform.forward.x, 0.0f, Cams[currentCam].transform.forward.z);
                forwardProjection.Normalize();
                Vector3 vec = Cams[currentCam].transform.right * Input.GetAxis("Mouse X") * factor + forwardProjection * Input.GetAxis("Mouse Y") * factor;
                Cams[currentCam].transform.position -= vec;
                Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt -= vec;
                Cams[currentCam].gameObject.GetComponent<CameraDataScript>().UpdateTargetPosition();
            }

            if (eventData.pointerId == -2)
            {
                Vector3 vec = Cams[currentCam].transform.right * Input.GetAxis("Mouse X") * factor + Cams[currentCam].transform.up * Input.GetAxis("Mouse Y") * factor;
                Cams[currentCam].transform.position -= vec;
                Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt -= vec;
                Cams[currentCam].gameObject.GetComponent<CameraDataScript>().UpdateTargetPosition();
            }

            if (eventData.pointerId == -3)
            {
                Vector3 offset = Cams[currentCam].transform.position - Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt;
                offset = Quaternion.AngleAxis(Input.GetAxis("Mouse X"),
                                            Cams[currentCam].transform.up) * Quaternion.AngleAxis(Input.GetAxis("Mouse Y"),
                                            -1.0f * Cams[currentCam].transform.right) * offset;
                Cams[currentCam].transform.position = Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt + offset;
                Cams[currentCam].transform.LookAt(Cams[currentCam].gameObject.GetComponent<CameraDataScript>().LookAt);
            }
        }
        else if (currentCam == 1)
        {

        }
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        Cursor.lockState = CursorLockMode.None;
        //OmniscientCam.gameObject.GetComponent<CameraDataScript>().Target.SetActive(false);
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if (notesSelectableFrom3DView)
        {
            int noteIndex = GetPointedNoteIndex(eventData.pressPosition, eventData.pressEventCamera);
            if (noteIndex >= 0 && eventData.button == PointerEventData.InputButton.Left)
            {
                NoteDetailPanelRef.SetActive(true);
                NoteDetailPanelRef.GetComponent<NoteDetailPanelScript>().SetData(noteIndex);
            }
        }
    }



    public int GetPointedNoteIndex(Vector2 mousePosition, Camera cam)
    {
        Vector2 localCursor = new Vector2(0, 0);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform, mousePosition, cam, out localCursor))
        {

            Texture tex = GetComponent<RawImage>().texture;
            Rect r = GetComponent<RawImage>().rectTransform.rect;

            //Using the size of the texture and the local cursor, clamp the X,Y coords between 0 and width - height of texture
            float coordX = Mathf.Clamp(0, (((localCursor.x - r.x) * tex.width) / r.width), tex.width);
            float coordY = Mathf.Clamp(0, (((localCursor.y - r.y) * tex.height) / r.height), tex.height);

            //Convert coordX and coordY to % (0.0-1.0) with respect to texture width and height
            float recalcX = coordX / tex.width;
            float recalcY = coordY / tex.height;

            localCursor = new Vector2(recalcX, recalcY);
            Vector2 localToCam = new Vector2(localCursor.x * Cams[currentCam].pixelWidth, localCursor.y * Cams[currentCam].pixelHeight);
            //Debug.Log("position in cam image : " + localToCam);

            Ray ray = Cams[currentCam].ScreenPointToRay(localToCam);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                return ProjectManagerScript.Instance.GetNoteIndexByPosition3D(hit.transform.position);
            }
        }
        return -1;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        hasFocus = true;
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        hasFocus = false;
    }



    public bool isFront()
    {
        foreach(GameObject panel in foregroundPanels)
        {
            if (panel.activeInHierarchy)
                return false;
        }
        return true;
    }
}
