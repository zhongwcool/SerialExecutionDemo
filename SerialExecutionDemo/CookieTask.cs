using System.Threading;

namespace SerialExecutionDemo
{
    public class CookieTask
    {
        public readonly int Data;
        public readonly Thread Worker;

        public CookieTask(Thread thread, int time)
        {
            Worker = thread;
            Data = time;
        }
    }
}