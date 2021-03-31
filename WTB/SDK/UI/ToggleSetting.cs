using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using System;
using System.Linq;
using TMPro;

using BSMLToggleSetting     = BeatSaberMarkupLanguage.Components.Settings.ToggleSetting;

namespace WTB.SDK.UI
{
    /// <summary>
    /// Toggle setting
    /// </summary>
    internal static class ToggleSetting
    {
        /// <summary>
        /// Setup a toggle setting
        /// </summary>
        /// <param name="p_Setting">Setting to setûp</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_Value">New value</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        internal static void Setup(BSMLToggleSetting p_Setting, BSMLAction p_Action, bool p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            p_Setting.Value = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            if (p_RemoveLabel)
            {
                UnityEngine.GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                UnityEngine.RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as UnityEngine.RectTransform;
                l_RectTransform.anchorMin = UnityEngine.Vector2.zero;
                l_RectTransform.anchorMax = UnityEngine.Vector2.one;
                l_RectTransform.sizeDelta = UnityEngine.Vector2.one;

                p_Setting.gameObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);
        }
    }
}
