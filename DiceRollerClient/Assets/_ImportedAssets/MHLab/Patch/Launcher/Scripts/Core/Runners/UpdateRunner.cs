using System;
using System.Collections.Generic;

namespace MHLab.Patch.Core.Client.Runners
{
    public class UpdateRunner : IUpdateRunner
    {
        public event EventHandler<IUpdater> StartedStep;
        public event EventHandler<IUpdater> PerformedStep;

        private readonly List<IUpdater> _steps;

        public UpdateRunner()
        {
            _steps = new List<IUpdater>();
        }

        public void Update()
        {
            foreach (var step in _steps)
            {
                OnStartedStep(step);
                step.Update();
                OnPerformedStep(step);
            }
        }

        public void RegisterStep<T>(T step) where T : IUpdater
        {
            _steps.Add(step);
        }

        public long GetProgressAmount()
        {
            var accumulator = 0L;

            foreach (var step in _steps)
            {
                accumulator += step.ProgressRangeAmount();
            }

            return accumulator;
        }

        private void OnStartedStep(IUpdater updater)
        {
            var handler = StartedStep;
            handler?.Invoke(this, updater);
        }

        private void OnPerformedStep(IUpdater updater)
        {
            var handler = PerformedStep;
            handler?.Invoke(this, updater);
        }
    }
}