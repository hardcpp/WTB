using BS_Utils.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Match view controller
    /// </summary>
    public class Match_Network : MonoBehaviour
    {
        private static float s_PULL_RATE = 1.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Instance
        /// </summary>
        private static Match_Network m_Instance = null;
        /// <summary>
        /// Is pulling from server
        /// </summary>
        private bool m_IsPulling = false;
        /// <summary>
        /// Last message time
        /// </summary>
        private float m_LastMessageTime = 0;
        /// <summary>
        /// Pulling coroutine
        /// </summary>
        private Coroutine m_PullingCoroutine = null;
        /// <summary>
        /// Pause on start coroutine
        /// </summary>
        private Coroutine m_PauseOnStartCoroutine = null;
        /// <summary>
        /// Countdown coroutine
        /// </summary>
        private Coroutine m_CountdownCoroutine = null;
        /// <summary>
        /// On menu scene actions
        /// </summary>
        private readonly ConcurrentQueue<Action> m_OnMenuSceneActions = new ConcurrentQueue<Action>();
        /// <summary>
        /// Pause menu manager instance
        /// </summary>
        private PauseMenuManager m_PauseMenuManager;
        /// <summary>
        /// Pause controller instance
        /// </summary>
        private PauseController m_PauseController;
        /// <summary>
        /// Audio time sync controller
        /// </summary>
        private AudioTimeSyncController m_AudioTimeSyncController;
        /// <summary>
        /// Is a triggered pause
        /// </summary>
        private bool m_IsTriggeredPause = false;
        /// <summary>
        /// Pause text instance
        /// </summary>
        private GameObject m_PauseText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// RPC Id
        /// </summary>
        internal int RPCId { get; private set; } = 0;
        /// <summary>
        /// RPC state
        /// </summary>
        internal string RPCState { get; private set; } = "";
        /// <summary>
        /// RPC Data
        /// </summary>
        internal JObject RPCData { get; private set; } = null;
        /// <summary>
        /// Map start time
        /// </summary>
        internal DateTime? MapStartTime = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        public void Awake()
        {
            /// Keep object alive
            DontDestroyOnLoad(this);

            /// Bind singleton
            m_Instance = this;

            /// Start polling
            m_IsPulling = true;

            /// Start coroutine
            m_PullingCoroutine = StartCoroutine(PullCoroutine(-1f));

            /// Bind events
            BSEvents.gameSceneActive += BSEvents_gameSceneActive;
            BSEvents.menuSceneActive += BSEvents_menuSceneActive;
        }
        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        public void OnDestroy()
        {
            /// Unbind events
            BSEvents.menuSceneActive -= BSEvents_menuSceneActive;
            BSEvents.gameSceneActive -= BSEvents_gameSceneActive;

            /// Stop pulling
            m_IsPulling = false;

            /// Stop pulling coroutine
            if (m_PullingCoroutine != null)
            {
                StopCoroutine(m_PullingCoroutine);
                m_PullingCoroutine = null;
            }

            /// Stop pause on start coroutine
            if (m_PauseOnStartCoroutine != null)
            {
                StopCoroutine(m_PauseOnStartCoroutine);
                m_PauseOnStartCoroutine = null;
            }

            /// Stop countdown coroutine
            if (m_CountdownCoroutine != null)
            {
                StopCoroutine(m_CountdownCoroutine);
                m_CountdownCoroutine = null;
            }

            /// Unbind singleton
            m_Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Execute pending menu actions
        /// </summary>
        internal void ExecutePendingMenuActions()
        {
            /// @todo
            while (m_Instance.m_OnMenuSceneActions.TryDequeue(out var l_Action))
                l_Action();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private IEnumerator PullCoroutine(float p_WaitTime)
        {
            ///Logger.log?.Debug("New pull " + p_WaitTime);

            /// Wait pull rate
            if (p_WaitTime > 0f)
                yield return new WaitForSecondsRealtime(p_WaitTime);

            /// Ensure all valid
            if (m_Instance == null || !m_Instance || !m_IsPulling)
            {
                m_PullingCoroutine = null;
                yield break;
            }

            ///Logger.log?.Debug("pull ");

            /// Send the query
            Network.ServerConnection.SendQuery(new Network.Methods.MatchPull()
            {
                MatchID = Match.Instance.MatchID,
                RPCId   = RPCId
            });

            m_PullingCoroutine = null;
            yield return null;
        }
        /// <summary>
        /// Pause on start coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator PauseOnStart()
        {
            /// Wait one frame
            yield return new WaitForEndOfFrame();
            /// Wait until song started
            yield return new WaitUntil(() => m_AudioTimeSyncController.songTime > 0);

            /// Disable intro skip & Song chart visualizer
            DisableIntroSkip();
            DisableSongChartVisualizer();

            /// Remove restart button
            var l_InitData = m_PauseMenuManager.GetField<PauseMenuManager.InitData>("_initData");
            if (l_InitData != null)
                l_InitData.SetField("showRestartButton", false);

            /// Pause on song start
            m_IsTriggeredPause = true;
            m_PauseController.didPauseEvent += PauseController_didPauseEvent;
            m_PauseController.Pause();

            /// Disable future pauses
            m_PauseController.canPauseEvent += PauseController_canPauseEvent;

            /// Start countdown
            m_CountdownCoroutine = StartCoroutine(ResumeCountdown());
        }
        /// <summary>
        /// Resume countdown coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResumeCountdown()
        {
            var l_Diff = (int)(MapStartTime.Value - DateTime.Now).TotalMilliseconds;

            while (l_Diff > 0)
            {
                if (m_PauseText != null && m_PauseText)
                    m_PauseText.GetComponent<TextMeshProUGUI>().text = "<color=red>Map starting in <color=yellow><b>" + (Math.Max(1000, l_Diff) / 1000) + "</b>";

                /// Wait for remaining time, maximum 1 second
                yield return new WaitForSecondsRealtime((float)(Math.Min(l_Diff, 1000.0) / 1000.0));

                l_Diff = (int)(MapStartTime.Value - DateTime.Now).TotalMilliseconds;
            }

            /// Resume map
            m_PauseMenuManager?.ContinueButtonPressed();

            /// Destroy the text
            if (m_PauseText != null && m_PauseText)
            {
                Destroy(m_PauseText);
                m_PauseText = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene active callback
        /// </summary>
        private void BSEvents_gameSceneActive()
        {
            /// Find components instances
            m_PauseMenuManager              = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
            m_PauseController               = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();
            m_AudioTimeSyncController       = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();

            /// Ensure we got all
            if (m_PauseMenuManager == null || m_PauseController == null || m_AudioTimeSyncController == null)
                return;

            /// Start coroutine
            m_PauseOnStartCoroutine = StartCoroutine(PauseOnStart());

            /// Disable intro skip & Song chart visualizer
            DisableIntroSkip();
            DisableSongChartVisualizer();
        }
        /// <summary>
        /// On menu scene active
        /// </summary>
        private void BSEvents_menuSceneActive()
        {
            /// Unbind event
            m_PauseController.didPauseEvent -= PauseController_didPauseEvent;
            /// Enable future pauses
            m_PauseController.canPauseEvent -= PauseController_canPauseEvent;
        }
        /// <summary>
        /// When a pause is started
        /// </summary>
        private void PauseController_didPauseEvent()
        {
            /// Hide menu
            if (!m_IsTriggeredPause)
            {
                m_PauseMenuManager.enabled = false;
                return;
            }

            /// Restore trigger state
            m_IsTriggeredPause = false;

            /// Bind event
            m_PauseController.didResumeEvent += PauseController_didResumeEvent;

            /// Disable buttons
            m_PauseMenuManager.GetField<Button>("_backButton")?.gameObject?.SetActive(false);
            m_PauseMenuManager.GetField<Button>("_restartButton")?.gameObject?.SetActive(false);
            m_PauseMenuManager.GetField<Button>("_continueButton")?.gameObject?.SetActive(false);

            /// Create text
            var l_Parent = m_PauseMenuManager.GetField<Button>("_backButton")?.gameObject?.transform?.parent;
            if (l_Parent != null && l_Parent && (m_PauseText == null || !m_PauseText))
            {
                BeatSaberMarkupLanguage.Tags.TextTag l_Tag = new BeatSaberMarkupLanguage.Tags.TextTag();
                m_PauseText = l_Tag.CreateObject(l_Parent);
                m_PauseText.transform.localPosition = new Vector3(38f, -45f, 0f);
                m_PauseText.GetComponent<TextMeshProUGUI>().fontSize    = 6f;
                m_PauseText.GetComponent<TextMeshProUGUI>().text        = "<color=red>Map starting in <color=yellow><b>?</b>";
            }
        }
        /// <summary>
        /// When a pause is ended
        /// </summary>
        private void PauseController_didResumeEvent()
        {
            /// Unbind event
            m_PauseController.didResumeEvent -= PauseController_didResumeEvent;

            /// Enable buttons
            m_PauseMenuManager.GetField<Button>("_backButton")?.gameObject?.SetActive(true);
            m_PauseMenuManager.GetField<Button>("_restartButton")?.gameObject?.SetActive(true);
            m_PauseMenuManager.GetField<Button>("_continueButton")?.gameObject?.SetActive(true);
        }
        /// <summary>
        /// Can pause callback
        /// </summary>
        /// <param name="p_Action">Action</param>
        private void PauseController_canPauseEvent(Action<bool> p_Action)
        {
            p_Action?.Invoke(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable intro skip
        /// </summary>
        private void DisableIntroSkip()
        {
            var l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "IntroSkip Behavior");
            if (l_Object != null && l_Object)
                l_Object.SetActive(false);

            l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "IntroSkip Prompt");
            if (l_Object != null && l_Object)
                l_Object.SetActive(false);
        }
        /// <summary>
        /// Disable song chart visualizer
        /// </summary>
        private void DisableSongChartVisualizer()
        {
            var l_Object = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "BeatSaberPlus_SongChartVisualizer");
            if (l_Object != null && l_Object)
                l_Object.SetActive(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the match pull received
        /// </summary>
        /// <param name="p_Result">Result</param>
        [Network.Methods._MethodResultHandler]
        static private void OnMatchPull(Network.Methods.MatchPull_Result p_Result)
        {
            /// Ensure all valid
            if (m_Instance == null || !m_Instance || !m_Instance.m_IsPulling)
                return;

            /// Should back to tournament select
            if (p_Result.BackToTournamentSelect)
            {
                /// Disable pulling
                m_Instance.m_IsPulling = false;

                if (m_Instance.m_PullingCoroutine != null)
                {
                    m_Instance.StopCoroutine(m_Instance.m_PullingCoroutine);
                    m_Instance.m_PullingCoroutine = null;
                }

                if (Match.CanBeUpdated)
                {
                    /// Go back to tournament select with a message
                    ViewFlowCoordinator.Instance().tournamentSelect.SetMessageModal_PendingMessage(p_Result.BackMessage);
                    ViewFlowCoordinator.Instance().SwitchToTournamentSelect();
                }
                else
                {
                    /// Clear other actions
                    while (m_Instance.m_OnMenuSceneActions.TryDequeue(out var _))
                        ;

                    /// Enqueue action
                    m_Instance.m_OnMenuSceneActions.Enqueue(() =>
                    {
                        /// Go back to tournament select with a message
                        ViewFlowCoordinator.Instance().tournamentSelect.SetMessageModal_PendingMessage(p_Result.BackMessage);
                        ViewFlowCoordinator.Instance().SwitchToTournamentSelect();
                    });
                }

                return;
            }

            /// Next pull
            m_Instance.m_PullingCoroutine = m_Instance.StartCoroutine(m_Instance.PullCoroutine(s_PULL_RATE - (Time.unscaledTime - m_Instance.m_LastMessageTime)));
            m_Instance.m_LastMessageTime  = Time.unscaledTime;

            /// Update RPC status
            if (m_Instance.RPCId != p_Result.RPCId)
            {
                m_Instance.RPCId      = p_Result.RPCId;
                m_Instance.RPCState   = p_Result.RPCState;
                m_Instance.RPCData    = p_Result.RPCData;
            }
        }
    }
}
