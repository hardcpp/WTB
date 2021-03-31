﻿using BS_Utils.Utilities;
using IPA.Utilities;
using System.Linq;
using UnityEngine;
using Zenject;

namespace WTB.SDK.UI
{
    /// <summary>
    /// Vertical icon segmented control
    /// </summary>
    internal static class VerticalIconSegmentedControl
    {
        /// <summary>
        /// Create icon segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        internal static HMUI.IconSegmentedControl Create(RectTransform p_Parent, bool p_HideCellBackground)
        {
            PlatformLeaderboardViewController l_PlatformLeaderboardViewController = Resources.FindObjectsOfTypeAll<PlatformLeaderboardViewController>().First();

            HMUI.IconSegmentedControl l_Prefab = l_PlatformLeaderboardViewController.GetField<HMUI.IconSegmentedControl, PlatformLeaderboardViewController>("_scopeSegmentedControl");
            HMUI.IconSegmentedControl l_Control = MonoBehaviour.Instantiate(l_Prefab, p_Parent, false);

            l_Control.name = "BSMLVerticalIconSegmentedControl";
            l_Control.SetField("_container",            l_Prefab.GetField<DiContainer, HMUI.IconSegmentedControl>("_container"));
            l_Control.SetField("_hideCellBackground",   p_HideCellBackground);

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = new Vector2(0.5f, 0.5f);
            l_RectTransform.anchorMax           = new Vector2(0.5f, 0.5f);
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = new Vector2(0.5f, 0.5f);

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            return l_Control;
        }
    }
}
