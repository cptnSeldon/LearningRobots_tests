using System;
using System.Runtime.InteropServices;

namespace P3_LearningRobots
{
    /// <summary>
    /// Frame per second management class
    /// Source: IntelRealSense SDK - DisplayHands sample
    /// </summary>
    class FPSTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long data);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long data);

        private long freq, last;
        private int fps;

        public FPSTimer()
        {
            QueryPerformanceFrequency(out freq);
            fps = 0;
            QueryPerformanceCounter(out last);
        }

        public void Tick()
        {
            QueryPerformanceCounter(out long now);
            fps++;
            if (now - last > freq) // update every second
            {
                last = now;
                Console.WriteLine("FPS=" + fps);
                fps = 0;
            }
        }
    }
}
