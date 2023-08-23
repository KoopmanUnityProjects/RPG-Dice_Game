using MHLab.Patch.Core.Client.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MHLab.Patch.Core.Client.Advanced.IO
{
    public class SmartDownloadMetrics : IDownloadMetrics
    {
        public int RunningThreads
        {
            get
            {
                if (_tasks != null)
                    return _tasks.Count(t => t != null && t.Status == TaskStatus.Running);
                return 1;
            }
        }

        private readonly Task[] _tasks;

        public SmartDownloadMetrics(Task[] tasks)
        {
            _tasks = tasks;
        }
    }
}
