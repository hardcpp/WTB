﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace WTB.SDK.Config
{
    /// <summary>
    /// INI config file
    /// </summary>
    internal class INIConfig
    {
        /// <summary>
        /// INI file instance
        /// </summary>
        private readonly INIFile m_Instance;
        /// <summary>
        /// Cache
        /// </summary>
        private Dictionary<(string, string), string>    m_StringCache   = new Dictionary<(string, string), string>();
        private Dictionary<(string, string), int>       m_IntCache      = new Dictionary<(string, string), int>();
        private Dictionary<(string, string), float>     m_FloatCache    = new Dictionary<(string, string), float>();
        private Dictionary<(string, string), bool>      m_BoolCache     = new Dictionary<(string, string), bool>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_ConfigName"></param>
        internal INIConfig(string p_ConfigName)
        {
            m_Instance = new INIFile(Path.Combine(Environment.CurrentDirectory, $"UserData/{p_ConfigName}.ini"));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a string from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        internal string GetString(string p_Section, string p_Name, string p_DefaultValue = "", bool p_AutoSave = false)
        {
            if (m_StringCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            string l_Value = m_Instance.GetSetting(p_Section, p_Name);
            if (l_Value != null)
                return l_Value;
            else if (p_AutoSave)
                SetString(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a string in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        internal void SetString(string p_Section, string p_Name, string p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value);

            if (m_StringCache.ContainsKey((p_Section, p_Name)))
                m_StringCache[(p_Section, p_Name)] = p_Value;
            else
                m_StringCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an int from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        internal int GetInt(string p_Section, string p_Name, int p_DefaultValue = 0, bool p_AutoSave = false)
        {
            if (m_IntCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            var l_RawValue = m_Instance.GetSetting(p_Section, p_Name);
            if (l_RawValue != null && int.TryParse(l_RawValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int l_Value))
                return l_Value;
            else if (p_AutoSave)
                SetInt(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets an int in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        internal void SetInt(string p_Section, string p_Name, int p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value.ToString(CultureInfo.InvariantCulture));

            if (m_IntCache.ContainsKey((p_Section, p_Name)))
                m_IntCache[(p_Section, p_Name)] = p_Value;
            else
                m_IntCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a float from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        internal float GetFloat(string p_Section, string p_Name, float p_DefaultValue = 0f, bool p_AutoSave = false)
        {
            if (m_FloatCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            var l_RawValue = m_Instance.GetSetting(p_Section, p_Name);
            if (l_RawValue != null && float.TryParse(l_RawValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float l_Value))
                return l_Value;
            else if (p_AutoSave)
                SetFloat(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a float in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        internal void SetFloat(string p_Section, string p_Name, float p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value.ToString(CultureInfo.InvariantCulture));

            if (m_FloatCache.ContainsKey((p_Section, p_Name)))
                m_FloatCache[(p_Section, p_Name)] = p_Value;
            else
                m_FloatCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a bool from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        internal bool GetBool(string p_Section, string p_Name, bool p_DefaultValue = false, bool p_AutoSave = false)
        {
            if (m_BoolCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            string l_StringVal = GetString(p_Section, p_Name);
            if (l_StringVal != null)
            {
                try
                {
                    if (CultureInfo.CurrentCulture.TextInfo.ToTitleCase(l_StringVal) == "True")
                        return true;
                    else if (CultureInfo.CurrentCulture.TextInfo.ToTitleCase(l_StringVal) == "False")
                        return false;
                    else if (p_AutoSave)
                        SetBool(p_Section, p_Name, p_DefaultValue);
                }
                catch
                {
                    SetBool(p_Section, p_Name, p_DefaultValue);
                }
            }
            else
                SetBool(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a bool in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Mode">What common yes/no alternative should we use.</param>
        /// <param name="p_Value">Value that should be written.</param>
        internal void SetBool(string p_Section, string p_Name, bool p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value ? "True" : "False");

            if (m_BoolCache.ContainsKey((p_Section, p_Name)))
                m_BoolCache[(p_Section, p_Name)] = p_Value;
            else
                m_BoolCache.Add((p_Section, p_Name), p_Value);
        }
    }
}
