using System;

namespace TachyonServerRPC {
    public class SynchronousReplyProcessor : IReplyProcessor {
        public void ProcessReply(Action reply) => reply.Invoke();
    }
}