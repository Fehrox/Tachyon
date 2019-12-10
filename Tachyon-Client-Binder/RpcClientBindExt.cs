using TachyonClientRPC;

namespace TachyonClientBinder
{
    public static class RpcClientBindExt
    {
        public static TService Bind<TService>(this RPCClient client) where TService : class
        {
            return ClientBinder<TService>.Bind(client);
        }
    }
}