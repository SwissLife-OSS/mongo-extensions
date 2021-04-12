namespace MongoDB.Prime.Extensions
{
    public class ProfilingStatus
    {
        public ProfilingStatus(
            ProfileLevel level,
            int slowMs,
            double sampleRate,
            string filter)
        {
            Level = level;
            SlowMs = slowMs;
            SampleRate = sampleRate;
            Filter = filter;
        }

        public ProfileLevel Level { get; }

        public int SlowMs { get; }

        public double SampleRate { get; }

        public string Filter { get; }
    }
}
