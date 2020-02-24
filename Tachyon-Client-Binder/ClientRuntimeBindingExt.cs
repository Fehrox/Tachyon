using TachyonClientRPC;

namespace TachyonClientBinder
{
    public static class ClientBindExt
    {
        public static TService Bind<TService>(this ClientRpc clientRpc) where TService : class
        {
            return ClientRuntimeBinder<TService>.Bind(clientRpc);
        }
    }
}