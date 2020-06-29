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

﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITabNavigator : MonoBehaviour
{
    public List<Selectable> selectables = new List<Selectable>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Navigate backward when holding shift, else navigate forward.
            this.HandleHotkeySelect(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), true);
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            EventSystem.current.SetSelectedGameObject(null, null);
        }
    }

    private void HandleHotkeySelect(bool isNavigateBackward, bool isWrapAround)
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject != null && selectedObject.activeInHierarchy) // Ensure a selection exists and is not an inactive object.
        {
            Selectable currentSelection = selectedObject.GetComponent<Selectable>();
            if (currentSelection != null)
            {
                Selectable nextSelection = this.FindNextSelectable(
                    selectables.IndexOf(currentSelection), isNavigateBackward, isWrapAround);
                if (nextSelection != null)
                {
                    nextSelection.Select();
                }
            }
            else
            {
                this.SelectFirstSelectable();
            }
        }
        else
        {
            this.SelectFirstSelectable();
        }
    }

    private void SelectFirstSelectable()
    {
        if (selectables != null && selectables.Count > 0)
        {
            Selectable firstSelectable = selectables[0];
            firstSelectable.Select();
        }
    }

    /// <summary>
    /// Looks at ordered selectable list to find the selectable we are trying to navigate to and returns it.
    /// </summary>
    private Selectable FindNextSelectable(int currentSelectableIndex, bool isNavigateBackward, bool isWrapAround)
    {
        Selectable nextSelection = null;

        int totalSelectables = selectables.Count;
        if (totalSelectables > 1)
        {
            if (isNavigateBackward)
            {
                if (currentSelectableIndex == 0)
                {
                    nextSelection = (isWrapAround) ? selectables[totalSelectables - 1] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex - 1];
                }
            }
            else // Navigate forward.
            {
                if (currentSelectableIndex == (totalSelectables - 1))
                {
                    nextSelection = (isWrapAround) ? selectables[0] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex + 1];
                }
            }
        }
        // ignore not interactable inputs
        if(nextSelection != null  && nextSelection.interactable == false)
        {
            int nextSelectableindex = isNavigateBackward? currentSelectableIndex - 1: currentSelectableIndex + 1;
            if(isWrapAround)
            {
                if (nextSelectableindex >= totalSelectables) nextSelectableindex = 0;
                if (nextSelectableindex < 0) nextSelectableindex = totalSelectables - 1;
            }
            else
            {
                if (nextSelectableindex >= totalSelectables) nextSelectableindex = totalSelectables - 1;
                if (nextSelectableindex < 0) nextSelectableindex = 0;
            }
            return FindNextSelectable(nextSelectableindex, isNavigateBackward, isWrapAround);
        }
        return nextSelection;
    }

}