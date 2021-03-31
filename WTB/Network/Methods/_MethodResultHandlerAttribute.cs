using System;
using System.Collections.Generic;
using System.Reflection;

namespace WTB.Network.Methods
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal class _MethodResultHandlerAttribute : Attribute
    {
        /// <summary>
        /// List of all handlers
        /// </summary>
        static internal Dictionary<Type, _MethodResultHandlerAttribute> s_Handlers = new Dictionary<Type, _MethodResultHandlerAttribute>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method info
        /// </summary>
        internal MethodInfo Method;
        /// <summary>
        /// Target structure type
        /// </summary>
        internal Type ResultType;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init the attribute
        /// </summary>
        /// <param name="p_Method">MethodInfo</param>
        internal bool Init(MethodInfo p_Method)
        {
            Method = p_Method;

            if (p_Method.GetParameters().Length != 1)
            {
                Logger.log?.Error("Network.Methods._MethodResultHandlerAttribute::Init => the method \"" + p_Method.Name + "\" has wrong parameter count");
                return false;
            }

            ParameterInfo l_Parameter = p_Method.GetParameters()[0];
            Type l_StructureType = l_Parameter.ParameterType;

            if (l_StructureType.BaseType != typeof(_MethodResult))
            {
                Logger.log?.Error("Network.Methods._MethodResultHandlerAttribute::Init => PacketStructureHandler the type \"" + l_StructureType.ToString() + "\" is not a PacketStructure");
                return false;
            }

            ResultType = l_StructureType;

            return true;
        }
        /// <summary>
        /// Read the structure and call the handler
        /// </summary>
        /// <param name="p_Session">Session instance</param>
        /// <param name="p_Packet">Packet data</param>
        internal void Call(_MethodResult p_Result)
        {
            Method.Invoke(null, new object[] { p_Result });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init all packet structure handlers
        /// </summary>
        internal static void InitHandlers()
        {
            s_Handlers.Clear();

            foreach (Assembly l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type l_Type in l_Assembly.GetTypes())
                    {
                        foreach (MethodInfo l_Method in l_Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                        {
                            var l_Attributes = l_Method.GetCustomAttributes(typeof(_MethodResultHandlerAttribute));

                            foreach (_MethodResultHandlerAttribute l_Attribute in l_Attributes)
                            {
                                if (!l_Attribute.Init(l_Method))
                                    continue;

                                if (!s_Handlers.ContainsKey(l_Attribute.ResultType))
                                    s_Handlers.Add(l_Attribute.ResultType, l_Attribute);
                                else
                                    s_Handlers[l_Attribute.ResultType] = l_Attribute;
                            }
                        }
                    }
                }
                catch (System.Exception)
                {

                }
            }
        }
    }
}
