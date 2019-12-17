using System;
using System.Threading;
using System.Threading.Tasks;
using TachyonClientRPC;

namespace TachyonClientRPC
{
    public static class ClientRpcExt
    {
        public static async Task<T> AskTask<T>(this ClientRpc client, string method, params object[] args) {
            return await ExecAsync<T>(client, method, args);
        }

        private static async Task<T> ExecAsync<T>( ClientRpc client, string method, params object[] args)
        {
            var finished = false;
            T result = default;
            
            client.Ask<T>(
                method,
                (r) => {
                    result = r; 
                    finished = true;
                }, 
                args
            );
                
            while (!finished) {
                await Task.Yield(); 
            }
            
            return result;
        }
    }
}