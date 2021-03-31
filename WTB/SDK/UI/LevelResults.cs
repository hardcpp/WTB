using System.Linq;
using TMPro;
using UnityEngine;

namespace WTB.SDK.UI
{
    /// <summary>
    /// Level result widget
    /// </summary>
    internal class LevelResults
    {
        /// <summary>
        /// Level result view template
        /// </summary>
        private static UnityEngine.GameObject m_LevelResultsTemplate = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            if (m_LevelResultsTemplate == null)
            {
                m_LevelResultsTemplate = UnityEngine.GameObject.Instantiate(UnityEngine.Resources.FindObjectsOfTypeAll<ResultsViewController>().First(x => x.gameObject.name == "StandardLevelResultsViewController").gameObject);

                /// Clean unused elements
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<ResultsViewController>());
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<HMUI.Touchable>());
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<VRUIControls.VRGraphicRaycaster>());
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<CanvasGroup>());
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<CanvasRenderer>());
                GameObject.Destroy(m_LevelResultsTemplate.GetComponent<Canvas>());
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private GameObject m_GameObject;
        private GameObject m_ClearedAnimation;

        private HMUI.ImageView m_SongCoverImage;
        private TextMeshProUGUI m_SongNameText;
        private TextMeshProUGUI m_AuthorNameText;
        private TextMeshProUGUI m_DifficultyText;
        private HMUI.ImageView m_CharacteristicIcon;

        private GameObject m_LevelCleared;
        private GameObject m_LevelFailed;

        private TextMeshProUGUI m_ScoreText;
        private TextMeshProUGUI m_RankText;
        private TextMeshProUGUI m_GoodCutsText; ///< "0<size=50%> / 0</size>"
        private TextMeshProUGUI m_ComboText;

        private UnityEngine.UI.Button m_Button;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private int m_CutCount;
        private int m_CuttableCount;
        private int m_MaxCombo;
        private int m_Score;
        private float m_Accuracy;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal UnityEngine.Sprite Cover {
            get => m_SongCoverImage.sprite;
            set => m_SongCoverImage.sprite = value;
        }
        internal string Name {
            get => m_SongNameText.text;
            set => m_SongNameText.text = value;
        }
        internal string AuthorNameText {
            get => m_AuthorNameText.text;
            set => m_AuthorNameText.text = value;
        }
        internal string Difficulty {
            get => m_DifficultyText.text;
            set => m_DifficultyText.text = value;
        }
        internal UnityEngine.Sprite Characteristic {
            get => m_CharacteristicIcon.sprite;
            set => m_CharacteristicIcon.sprite = value;
        }
        internal int CutCount {
            get => m_CutCount;
            set {
                m_CutCount          = value;
                m_GoodCutsText.text = $"{m_CutCount}<size=50%> / {m_CuttableCount}</size>";
                m_ComboText.text    = m_CutCount == m_CuttableCount ? "Full Combo" : $"{m_MaxCombo} Max Combo";
            }
        }
        internal int CuttableCount {
            get => m_CuttableCount;
            set {
                m_CuttableCount     = value;
                m_GoodCutsText.text = $"{m_CutCount}<size=50%> / {m_CuttableCount}</size>";
                m_ComboText.text    = m_CutCount == m_CuttableCount ? "Full Combo" : $"{m_MaxCombo} Max Combo";
            }
        }
        internal int MaxCombo {
            get => m_MaxCombo;
            set {
                m_MaxCombo          = value;
                m_ComboText.text    = m_CutCount == m_CuttableCount ? "Full Combo" : $"{m_MaxCombo} Max Combo";
            }
        }
        internal int Score {
            get => m_Score;
            set {
                m_Score = value;
                m_ScoreText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        internal float Accuracy {
            get => m_Accuracy;
            set {
                m_Accuracy = value;
                m_RankText.text = value >= 0f ? ("<line-height=30%><size=60%>" + value.ToString("F2") + "<size=45%>%") : "--";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Parent"></param>
        internal LevelResults(Transform p_Parent)
        {
            /// Close music preview details panel
            m_GameObject = GameObject.Instantiate(m_LevelResultsTemplate, p_Parent);

            var l_BSMLObjects = m_GameObject.GetComponentsInChildren<RectTransform>().Where(x => x.gameObject.name.StartsWith("BSML"));

            foreach (var l_Current in l_BSMLObjects) GameObject.Destroy(l_Current.gameObject);

            /// Find cleared animation
            m_ClearedAnimation = m_GameObject.transform.Find("ClearedAnimation").gameObject;
            m_ClearedAnimation.SetActive(false);

            /// Find container
            var l_Container = m_GameObject.transform.Find("Container").gameObject;
            if (l_Container)
            {
                var l_LevelBarSimple = l_Container.transform.Find("LevelBarSimple").gameObject;
                if (l_LevelBarSimple)
                {
                    m_SongCoverImage    = l_LevelBarSimple.transform.Find("SongArtwork").GetComponent<HMUI.ImageView>();
                    m_SongNameText      = l_LevelBarSimple.transform.Find("SongNameText").GetComponent<TextMeshProUGUI>();
                    m_AuthorNameText    = l_LevelBarSimple.transform.Find("AuthorNameText").GetComponent<TextMeshProUGUI>();

                    var l_BeatmapDataContainer = l_LevelBarSimple.transform.Find("BeatmapDataContainer").gameObject;
                    if (l_BeatmapDataContainer)
                    {
                        m_DifficultyText        = l_BeatmapDataContainer.transform.Find("DifficultyText").GetComponent<TextMeshProUGUI>();
                        m_CharacteristicIcon    = l_BeatmapDataContainer.transform.Find("Icon").GetComponent<HMUI.ImageView>();
                    }
                }

                m_LevelCleared = l_Container.transform.Find("ClearedBanner").gameObject;
                m_LevelCleared.SetActive(true);

                m_LevelFailed = l_Container.transform.Find("FailedBanner").gameObject;
                m_LevelFailed.SetActive(false);

                var l_ClearedInfo = l_Container.transform.Find("ClearedInfo").gameObject;
                if (l_ClearedInfo)
                {
                    var l_Score = l_ClearedInfo.transform.Find("Score").gameObject;
                    if (l_Score)
                    {
                        m_ScoreText = l_Score.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();
                        l_Score.transform.Find("NewHighScoreText").gameObject.SetActive(false);
                    }

                    m_RankText = l_ClearedInfo.transform.Find("RankText").GetComponent<TextMeshProUGUI>();

                    var l_GoodCuts = l_ClearedInfo.transform.Find("GoodCuts").gameObject;
                    if (l_GoodCuts)
                    {
                        m_GoodCutsText  = l_GoodCuts.transform.Find("GoodCutsText").GetComponent<TextMeshProUGUI>();
                        m_ComboText     = l_GoodCuts.transform.Find("ComboText").GetComponent<TextMeshProUGUI>();
                    }
                }

                var l_BottomPanel = l_Container.transform.Find("BottomPanel").gameObject;
                if (l_BottomPanel)
                {
                    for (int l_I = 0; l_I < l_BottomPanel.transform.childCount; ++l_I)
                        GameObject.Destroy(l_BottomPanel.transform.GetChild(l_I).gameObject);

                    l_BottomPanel.AddComponent<UnityEngine.UI.ContentSizeFitter>().horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    l_BottomPanel.AddComponent<UnityEngine.UI.LayoutElement>();

                    m_Button = new BeatSaberMarkupLanguage.Tags.ButtonTag().CreateObject(l_BottomPanel.transform).GetComponent<UnityEngine.UI.Button>();
                }
            }

            SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the game object is active
        /// </summary>
        /// <param name="p_Active"></param>
        internal void SetActive(bool p_Active)
        {
            m_GameObject.SetActive(p_Active);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        internal void SetButtonEnabled(bool p_Value)
        {
            m_Button.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        internal void SetButtonInteractable(bool p_Value)
        {
            m_Button.interactable = p_Value;
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        internal void SetButtonText(string p_Value)
        {
            var l_Text = m_Button.GetComponentInChildren<HMUI.CurvedTextMeshPro>();

            if (!l_Text.richText)
                l_Text.richText = true;
            if (l_Text.text != p_Value)
                l_Text.text = p_Value;
        }
        /// <summary>
        /// Set right button action
        /// </summary>
        /// <param name="p_Value">New value</param>
        internal void SetButtonAction(UnityEngine.Events.UnityAction p_Value)
        {
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(p_Value);
        }

    }
}
