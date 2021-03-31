
using Newtonsoft.Json.Linq;

namespace WTB.Network.Methods
{
    /// <summary>
    /// Base class for a method result
    /// </summary>
    internal abstract class _MethodResult
    {
        internal bool Deserialize(JObject p_Data)
        {
            try
            {
                DeserializeImpl(p_Data);
                return true;
            }
            catch (System.Exception p_Exception)
            {
#if DEBUG
                Logger.log?.Critical("Deserialize error :");
                Logger.log?.Critical(p_Exception);
#endif
            }

            return false;
        }

        protected abstract void DeserializeImpl(JObject p_Data);

    }
}
