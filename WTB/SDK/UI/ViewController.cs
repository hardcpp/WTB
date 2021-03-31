﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace WTB.SDK.UI
{
    /// <summary>
    /// IViewController interface
    /// </summary>
    public abstract class IViewController : HMUI.ViewController
    {
        /// <summary>
        /// Show view transition loading
        /// </summary>
        internal abstract void ShowViewTransitionLoading();
    }

    /// <summary>
    /// View controller base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ViewController<T> : IViewController, INotifyPropertyChanged
        where T : ViewController<T>
    {
        /// <summary>
        /// Modal coroutine
        /// </summary>
        private Coroutine m_ModalCoroutine = null;
        /// <summary>
        /// Pending message
        /// </summary>
        private string m_PendingMessage = null;
        /// <summary>
        /// Confirmation modal callback
        /// </summary>
        private Action m_ConfirmationModalCallback = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("SDK_MessageModal")]
        protected GameObject m_SDK_MessageModal = null;
        [UIComponent("SDK_MessageModal_Text")]
        protected TextMeshProUGUI m_SDK_MessageModal_Text = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SDK_ConfirmModal")]
        protected HMUI.ModalView m_SDK_ConfirmModal = null;
        [UIComponent("SDK_ConfirmModal_Text")]
        protected TextMeshProUGUI m_SDK_ConfirmModal_Text = null;
        [UIComponent("SDK_ConfirmModal_Button")]
        protected UnityEngine.UI.Button m_SDK_ConfirmModal_Button = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SDK_LoadingModal")]
        protected HMUI.ModalView m_SDK_LoadingModal = null;
        [UIObject("SDK_LoadingModal_Text")]
        protected GameObject m_SDK_LoadingModalText = null;
        private LoadingControl m_LoadingModal_Spinner = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser params
        /// </summary>
        protected BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static T Instance = null;
        /// <summary>
        /// Can UI be updated
        /// </summary>
        internal static bool CanBeUpdated => Instance != null && Instance && Instance.isInViewControllerHierarchy && Instance.isActiveAndEnabled && Instance.UICreated;
        /// <summary>
        /// Was UI created
        /// </summary>
        internal bool UICreated { get; private set; } = false;
        /// <summary>
        /// Has pending message
        /// </summary>
        internal bool HasPendingMessage => m_PendingMessage != null;
        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// Bind singleton
            Instance = this as T;

            if (p_FirstActivation)
            {
                var l_UICode = GetViewContentDescription();

                /// Add loading modal & message modal code
                if (true)
                {
                    var l_Closure = l_UICode.LastIndexOf('<');
                    var l_NewCode = l_UICode.Substring(0, l_Closure);

                    l_NewCode += "<modal id='SDK_MessageModal' show-event='SDK_ShowMessageModal' hide-event='SDK_CloseMessageModal,CloseAllModals' move-to-center='true' size-delta-y='30' size-delta-x='85'>";
                    l_NewCode += "  <vertical pad='0'>";
                    l_NewCode += "    <text text='' id='SDK_MessageModal_Text' font-size='4' align='Center'/>";
                    l_NewCode += "    <primary-button text='Ok' click-event='SDK_CloseMessageModal'></primary-button>";
                    l_NewCode += "  </vertical>";
                    l_NewCode += "</modal>";

                    l_NewCode += "<modal id='SDK_ConfirmModal' show-event='SDK_ShowConfirmModal' hide-event='SDK_CloseConfirmModal,CloseAllModals' move-to-center='true' size-delta-y='30' size-delta-x='85'>";
                    l_NewCode += "  <vertical pad='0'>";
                    l_NewCode += "    <text id='SDK_ConfirmModal_Text' text=' ' font-size='4' align='Center'/>";
                    l_NewCode += "    <horizontal>";
                    l_NewCode += "      <primary-button text='Yes' id='SDK_ConfirmModal_Button'></primary-button>";
                    l_NewCode += "      <button text='No' click-event='SDK_CloseConfirmModal'></button>";
                    l_NewCode += "    </horizontal>";
                    l_NewCode += "  </vertical>";
                    l_NewCode += "</modal>";

                    l_NewCode += "<modal id='SDK_LoadingModal' show-event='SDK_ShowLoadingModal' hide-event='SDK_CloseLoadingModal,CloseAllModals' move-to-center='true' size-delta-y='30' size-delta-x='48'>";
                    l_NewCode += "    <text id='SDK_LoadingModal_Text' font-size='5.5' align='Center'/>";
                    l_NewCode += "</modal>";
                    l_NewCode += l_UICode.Substring(l_Closure);

                    l_UICode = l_NewCode;
                }

                /// Construct UI
                m_ParserParams = BSMLParser.instance.Parse(l_UICode, gameObject, this as T);

                /// Change state
                UICreated = true;

                /// Setup loading modal
                SDK.UI.ModalView.SetupLoadingControl(m_SDK_LoadingModal, out m_LoadingModal_Spinner);
                SDK.UI.ModalView.SetOpacity(m_SDK_MessageModal, 0.75f);
                SDK.UI.ModalView.SetOpacity(m_SDK_ConfirmModal, 0.75f);
                SDK.UI.ModalView.SetOpacity(m_SDK_LoadingModal, 0.75f);

                /// Call implementation
                OnViewCreation();
            }

            /// Call implementation
            OnViewActivation();
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemDisabling">Is screen system disabling</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            /// Close all remaining modals
            CloseAllModals();

            /// Call implementation
            OnViewDeactivation();
        }
        /// <summary>
        /// On destruction
        /// </summary>
        protected override void OnDestroy()
        {
            /// Call implementation
            OnViewDestruction();

            /// Forward event
            base.OnDestroy();

            /// Unbind singleton
            Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected abstract string GetViewContentDescription();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected virtual void OnViewCreation() { }
        /// <summary>
        /// On view activation
        /// </summary>
        protected virtual void OnViewActivation() { }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected virtual void OnViewDeactivation() { }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected virtual void OnViewDestruction() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set a message to display when this view is activated
        /// </summary>
        /// <param name="p_PendingMessage">Message to display</param>
        internal void SetMessageModal_PendingMessage(string p_PendingMessage)
        {
            /// Set message
            m_PendingMessage = p_PendingMessage;
        }
        /// <summary>
        /// Set loading modal download progress
        /// </summary>
        /// <param name="p_Text"></param>
        /// <param name="p_Progress">Download progress</param>
        protected void SetLoadingModal_DownloadProgress(string p_Text, float p_Progress)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[SDK.UI][ViewController] Set loading modal download progress \"" + p_Text + "\" called before View UI's creation");
                return;
            }

            if (m_LoadingModal_Spinner.gameObject.activeSelf)
                m_LoadingModal_Spinner.ShowDownloadingProgress(p_Text, p_Progress);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the loading modal
        /// </summary>
        protected void ShowLoadingModal(string p_Message = "", bool p_Download = false)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[SDK.UI][ViewController] Show loading modal \"" + p_Message + "\" called before View UI's creation");
                return;
            }

            /// Change modal text
            m_SDK_LoadingModalText.GetComponent<TextMeshProUGUI>().text = p_Download ? "" : p_Message;

            /// Show the modal
            ShowModal("SDK_ShowLoadingModal", () => {
                /// Show animator
                if (!p_Download)
                    m_LoadingModal_Spinner.ShowLoading();
                else
                    m_LoadingModal_Spinner.ShowDownloadingProgress(p_Message, 0);
            });
        }
        /// <summary>
        /// Show view transition loading
        /// </summary>
        internal override sealed void ShowViewTransitionLoading()
        {
            ShowLoadingModal();
        }
        /// <summary>
        /// Show confirmation modal
        /// </summary>
        /// <param name="p_Message">Message</param>
        /// <param name="p_OnConfirm">Callback</param>
        internal void ShowConfirmationModal(string p_Message, Action p_OnConfirm)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[SDK.UI][ViewController.ShowConfirmationModal] Show confirmation modal \"" + p_Message + "\" called before View UI's creation");
                return;
            }

            /// Store callback
            m_ConfirmationModalCallback = p_OnConfirm;

            /// Change modal text
            m_SDK_ConfirmModal_Text.GetComponent<TextMeshProUGUI>().text = p_Message;

            ShowModal("SDK_ShowConfirmModal");
        }
        /// <summary>
        /// Show no message modal
        /// </summary>
        protected void ShowMessageModal()
        {
            if (m_PendingMessage != null)
            {
                m_SDK_MessageModal_Text.text  = m_PendingMessage;
                m_PendingMessage = null;
            }

            HideLoadingModal();

            ShowModal("SDK_ShowMessageModal");
        }
        /// <summary>
        /// Show no message modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        protected void ShowMessageModal(string p_Message)
        {
            SetMessageModal_PendingMessage(p_Message);
            ShowMessageModal();
        }
        /// <summary>
        /// Hide the loading modal
        /// </summary>
        protected void HideLoadingModal()
        {
            CloseModal("SDK_CloseLoadingModal");
            m_LoadingModal_Spinner.Hide();

            /// Should display a pending message
            if (m_PendingMessage != null)
            {
                /// Show the modal
                ShowMessageModal();

                /// Reset back to default
                m_PendingMessage = null;
            }
        }
        /// <summary>
        /// Hide the confirmation modal
        /// </summary>
        protected void HideConfirmationModal() => CloseModal("SDK_CloseConfirmModal");
        /// <summary>
        /// Hide the message modal
        /// </summary>
        protected void HideMessageModal() => CloseModal("SDK_CloseMessageModal");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show modal
        /// </summary>
        /// <param name="p_Event">Modal event</param>
        /// <param name="p_Callback">On emite callback</param>
        protected void ShowModal(string p_Event, Action p_Callback = null)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[SDK.UI][ViewController] Show modal \"" + p_Event + "\" called before View UI's creation");
                return;
            }

            if (m_ModalCoroutine != null)
            {
                StopCoroutine(m_ModalCoroutine);
                m_ModalCoroutine = null;
            }

            if (CanBeUpdated)
                m_ModalCoroutine = StartCoroutine(ShowModalCoroutine(p_Event, p_Callback));
        }
        /// <summary>
        /// Hide modal
        /// </summary>
        /// <param name="p_Event"></param>
        protected void CloseModal(string p_Event)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[SDK.UI][ViewController] Close modal \"" + p_Event + "\" called before View UI's creation");
                return;
            }

            if (m_ModalCoroutine != null)
            {
                StopCoroutine(m_ModalCoroutine);
                m_ModalCoroutine = null;
            }

            m_ParserParams.EmitEvent(p_Event);
        }
        /// <summary>
        /// Close all modals
        /// </summary>
        protected void CloseAllModals()
        {
            if (m_ModalCoroutine != null)
            {
                StopCoroutine(m_ModalCoroutine);
                m_ModalCoroutine = null;
            }

            /// Close all remaining modals
            m_ParserParams.EmitEvent("CloseAllModals");
        }
        /// <summary>
        /// Show modal coroutine
        /// </summary>
        /// <param name="p_Event">Modal event</param>
        /// <param name="p_Callback">On emite callback</param>
        /// <returns></returns>
        private IEnumerator ShowModalCoroutine(string p_Event, Action p_Callback = null)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !isInTransition);

            if (!isInViewControllerHierarchy)
                yield break;

            m_ParserParams.EmitEvent(p_Event);
            p_Callback?.Invoke();
            m_ModalCoroutine = null;

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notify property changed
        /// </summary>
        /// <param name="p_PropertyName">Property name</param>
        protected void NotifyPropertyChanged([CallerMemberName] string p_PropertyName = "")
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p_PropertyName));
            }
            catch (Exception l_Exception)
            {
                Logger.log?.Error($"[SDK][ViewController] Error Invoking PropertyChanged: {l_Exception.Message}");
                Logger.log?.Error(l_Exception);
            }
        }
    }
}
