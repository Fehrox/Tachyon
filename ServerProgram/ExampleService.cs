using System;
using System.Threading.Tasks;
using Interop;

namespace ServerProgram
{
    public class ExampleService : IExampleService
    {
        public event Action<LogDTO> OnLogWarning;

        public event Action<LogDTO> OnLog;

        private int _i = 0;

        public void Update()
        {
            var log = new LogDTO
            {
                //Message = "Short." };
                Message = "This is some message " + _i++ + "! " +
                          "But it's really really long, because we need to " +
                          "test if the message can wrap it's buffer size " +
                          "and still be correctly reconstructed at the other " +
                          "end of the network. Because although this protocol " +
                          "isn't optimised for large packets, we still want to " +
                          "be able to fully support them. Doing so makes this " +
                          "system much more flexible, and allows the sending of " +
                          "files, like images, video, binary updates, whatever " +
                          "you can imagine, it can send. I know that's a pretty " +
                          "standard feature, but the way other networking systems " +
                          "are built, they support this kind of thing at the expense " +
                          "of performance with smaller data packets. Not Tachyon, " +
                          "tachyon can send and receive those tiny packets faster " +
                          "than the speed of light, and still handle the big ones."
            };
            if (_i % 2 == 0)
                OnLog?.Invoke(log);
            else
                OnLogWarning?.Invoke(new LogDTO {Message = "."});
        }

        public Task<long> Ping(long clientTime)
        {
            return Task.FromResult(DateTime.Now.Ticks - clientTime);
        }

        public void Log(LogDTO log)
        {
            Console.WriteLine(log.Message);
        }
    }
}