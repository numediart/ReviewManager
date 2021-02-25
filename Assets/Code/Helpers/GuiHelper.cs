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
using UnityEngine.EventSystems;
using System;
using System.Text.RegularExpressions;

public class GuiHelper
{
    public static bool IsInputFieldElementSelected()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            UnityEngine.UI.InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>();
            if (inputField != null)
            {
                return true;
            }
        }
        return false;
    }

    public static string RemoveEmojiFromString(string input)
    {
        if (!ContainsEmoji(input))
            return input;

        return Regex.Replace(input, @"\p{Cs}", "");
    }

    public static bool ContainsEmoji(string input)
    {
        Match match = Regex.Match(input, @"\p{Cs}");
        Debug.Log("search emoji in " + input + " : " + match.Success);
        return match.Success;
    }
}
