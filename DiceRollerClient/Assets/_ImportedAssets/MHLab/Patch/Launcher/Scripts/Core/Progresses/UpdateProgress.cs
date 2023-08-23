using System.Threading;

namespace MHLab.Patch.Core.Client.Progresses
{
    public class UpdateProgress
    {
        public long TotalSteps { get; set; }

        private long _currentSteps;
        public long CurrentSteps
        {
            get => _currentSteps;
            set
            {
                if (value > TotalSteps) _currentSteps = TotalSteps;
                _currentSteps = value;
            }
        }

        public string StepMessage { get; set; }
        
        public void IncrementStep(long increment)
        {
            Interlocked.Add(ref _currentSteps, increment);
        }
    }
}
