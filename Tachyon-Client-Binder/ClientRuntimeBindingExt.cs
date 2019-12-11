using TachyonClientRPC;

namespace TachyonClientBinder
{
    public static class ClientBindExt
    {
        public static TService Bind<TService>(this Client client) where TService : class
        {
            return ClientBinder<TService>.Bind(client);
        }
    }
}