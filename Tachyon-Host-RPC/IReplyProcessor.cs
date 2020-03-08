using System;

namespace TachyonServerRPC {
    public interface IReplyProcessor {
        void ProcessReply(Action reply);
    }
}