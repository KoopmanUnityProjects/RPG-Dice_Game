using MHLab.Patch.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MHLab.Patch.Core.Client.IO
{
    public class DownloadSpeedMeter : IDownloadSpeedMeter
    {
        private const int DownloadSpeedHistorySize = 5;

        public virtual string FormattedDownloadSpeed  => FormatUtility.FormatSizeDecimal(_downloadSpeed, 2) + "/s";
        public virtual string FormattedMaxReachedSpeed => FormatUtility.FormatSizeDecimal(_maxReachedDownloadSpeed, 2) + "/s";
        
        public         long   DownloadSpeed           => _downloadSpeed;
        public         long   MaxReachedDownloadSpeed => _maxReachedDownloadSpeed;

        private long     _downloadSpeed;
        private long     _maxReachedDownloadSpeed;
        private long     _accumulator;
        private DateTime _lastTime = DateTime.UtcNow;

        private Queue<long> _accumulatorQueue;

        public DownloadSpeedMeter()
        {
            _accumulatorQueue = new Queue<long>(DownloadSpeedHistorySize);
        }

        public void UpdateDownloadSpeed(long additionalDownloadedSize)
        {
            Interlocked.Add(ref _accumulator, additionalDownloadedSize);
        }

        public void Tick()
        {
            var accumulator = Interlocked.Exchange(ref _accumulator, 0);

            if (_accumulatorQueue.Count >= DownloadSpeedHistorySize)
                _accumulatorQueue.Dequeue();

            _accumulatorQueue.Enqueue((long)(accumulator / (DateTime.UtcNow - _lastTime).TotalSeconds));

            long currentAccumulator = 0;

            foreach (var current in _accumulatorQueue)
            {
                currentAccumulator += current;
            }

            _downloadSpeed = (long)(currentAccumulator / _accumulatorQueue.Count);

            if (_downloadSpeed > _maxReachedDownloadSpeed)
                _maxReachedDownloadSpeed = _downloadSpeed;
            
            _lastTime = DateTime.UtcNow;
        }

        public void Reset()
        {
            _downloadSpeed = 0;
            _lastTime = DateTime.UtcNow;
        }
    }
}
