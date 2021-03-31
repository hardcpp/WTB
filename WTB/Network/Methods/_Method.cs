
using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal abstract class _Method
    {
        internal enum AuthType
        {
            None,
            Token
        }

        internal abstract string _MethodName { get; }
        internal abstract AuthType _MethodAuth { get; }
        internal abstract Type _MethodResultType { get; }
        internal abstract bool _MethodShouldRetryOnFailure { get; }

        internal JObject Serialize()
        {
            try
            {
                return SerializeImpl();
            }
            catch (System.Exception p_Exception)
            {
#if DEBUG
                Logger.log?.Critical("Deserialize error :");
                Logger.log?.Critical(p_Exception);
#endif
            }

            return null;
        }
        protected abstract JObject SerializeImpl();
    }
}
