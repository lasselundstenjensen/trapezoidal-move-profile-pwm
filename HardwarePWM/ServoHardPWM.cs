using System;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using System.Threading;

namespace HardwarePWM
{
    public class ServoHardPWM : IServo
    {
        readonly PWM pwm;
        readonly uint periodUs;     // Number of us in period.
        readonly uint periodNs;     // Number of ns in period.
        readonly uint lowDegree;    // Low degree of servo; 0 default.
        readonly uint highDegree;   // High degree of servo; 180 default.
        uint lowUs = 600;           // microsecond (us) range. Find your specific values with servo range testing.
        uint highUs = 2400;         // microsecond (us) range. Find your specific values with servo range testing.
        bool invert;                // Invert/reverse rotation.
        uint currentUs = 600;
        DateTime lastChanged;

        public ServoHardPWM(FEZ_Pin.PWM pin, uint periodUs = 20000, uint lowUs = 1000, uint highUs = 2000, uint lowDegree = 0, uint highDegree = 180)
        {
            this.periodUs = periodUs;
            this.periodNs = this.periodUs * 1000;
            this.lowUs = lowUs;
            this.highUs = highUs;
            this.lowDegree = lowDegree;
            this.highDegree = highDegree;
            this.pwm = new PWM((PWM.Pin)pin);
            pwm.Set(false);
            this.currentUs = lowUs;
            this.lastChanged = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets value to invert direction.
        /// </summary>
        public bool Invert
        {
            get { return invert; }
            set { this.invert = value; }
        }

        /// <summary>
        /// Gets number of microseconds (us) in the period.
        /// </summary>
        public uint Period
        {
            get { return periodUs / 1000; }
        }

        /// <summary>
        /// Gets or sets number of microseconds (us) in low servo range.
        /// </summary>
        public uint LowRange
        {
            get { return this.lowUs; }
            set { this.lowUs = value; }
        }

        /// <summary>
        /// Gets or sets number of microseconds (us) in high servo range.
        /// </summary>
        public uint HighRange
        {
            get { return this.highUs; }
            set { this.highUs = value; }
        }

        /// <summary>
        /// Gets datetime position was last changed.
        /// </summary>
        public DateTime LastChanged
        {
            get { return this.lastChanged; }
        }

        /// <summary>
        /// Sets current servo angle degree.
        /// </summary>
        /// <param name="degree">Degree to set. (i.e. 0-180)</param>
        public void SetDegree(uint degree)
        {
            bool usingSoftRamp = true;
            if (usingSoftRamp) {
                uint pwm_current = currentUs;
                uint pwm_target = ScaleRange(degree, lowDegree, highDegree, lowUs, highUs);

                Microsoft.SPOT.Debug.Print("degree -> " + degree + "\npwm_current -> " + pwm_current + "\npwm_target -> " + pwm_target);

                TrapezoidalMoveProfile thread = new TrapezoidalMoveProfile(pwm_current, pwm_target, 2);
                thread.pwm = this;
                Thread threadRunner = new Thread(new ThreadStart(thread.Begin));
                threadRunner.Start();

            } else {
                if (degree < lowDegree || degree > highDegree)
                    throw new ArgumentOutOfRangeException("angleDegree");

                uint posUs = ScaleRange(degree, lowDegree, highDegree, lowUs, highUs);
                SetPosition(posUs);
            }
        }

        /// <summary>
        /// Sets current servo position.
        /// </summary>
        /// <param name="positionUs">High time in microseconds.</param>
        public void SetPosition(uint positionUs)
        {
            if (invert)
                positionUs = (highUs - positionUs) + lowUs;

            uint posNs = positionUs * 1000; // convert us to ns as SetPulse uses ns scale.
            pwm.SetPulse(periodNs, posNs);

            currentUs = positionUs;
//            this.lastChanged = DateTime.Now;
        }

        /// <summary>
        /// Stops holding servo at current position by setting PWM low.
        /// </summary>
        public void Stop()
        {
            pwm.Set(false);
        }

        /// <summary>
        /// Dispose servo instance.
        /// </summary>
        public void Dispose()
        {
            // Try to stop servo clean on a pulse low.
            Stop();
            pwm.Dispose();
        }

        private static uint ScaleRange(uint oldValue, uint oldMin, uint oldMax, uint newMin, uint newMax)
        {
            return ((oldValue - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
        }
    }
}