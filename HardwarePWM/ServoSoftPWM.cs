using System;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace HardwarePWM
{
    interface IServo : IDisposable
    {
        bool Invert { get; set; }
        uint Period { get; }
        uint LowRange { get; set; }
        uint HighRange { get; set; }
        DateTime LastChanged { get; }
        void SetDegree(uint angleDegree);
        void SetPosition(uint position);
        void Stop();
    }

    public class ServoSoftPWM : IServo
    {
        readonly OutputCompare oc;
        readonly uint periodUs;     // Normally 20,000us (i.e. 20ms)
        readonly uint lowDegree;    // Low degree of servo; 0 default.
        readonly uint highDegree;   // High degree of servo; 180 default.
        readonly uint[] timings;    // Used for setting OC timings.
        uint lowUs;                 // Small servo (e.g. 690)
        uint highUs;                // Small servo (e.g. 2422)
        bool invert;                // Invert servo direction control.
        uint currentUs = 600;
        DateTime lastChanged;

        public ServoSoftPWM(FEZ_Pin.Digital pin, uint periodUs = 20000, uint lowUs = 1000, uint highUs = 2000, uint lowDegree = 0, uint highDegree = 180)
        {
            this.oc = new OutputCompare((Cpu.Pin)pin, true, 2);
            this.timings = new uint[2];
            this.periodUs = periodUs;
            this.lowUs = lowUs;
            this.highUs = highUs;
            this.lowDegree = lowDegree;
            this.highDegree = highDegree;
            this.currentUs = lowUs;
            this.lastChanged = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets a value to invert the servo direction.
        /// </summary>
        public bool Invert
        {
            get { return this.invert; }
            set { this.invert = value; }
        }

        /// <summary>
        /// Gets the period in microseconds.
        /// </summary>
        public uint Period
        {
            get { return this.periodUs; }
        }

        /// <summary>
        /// Gets or sets the low servo range in microseconds.
        /// </summary>
        public uint LowRange
        {
            get { return this.lowUs; }
            set { this.lowUs = value; }
        }

        /// <summary>
        /// Gets or sets the high servo range in microseconds.
        /// </summary>
        public uint HighRange
        {
            get { return this.highUs; }
            set { this.highUs = value; }
        }

        /// <summary>
        /// Gets datetime of last position change.
        /// </summary>
        public DateTime LastChanged
        {
            get { return this.lastChanged; }
        }

        /// <summary>
        /// Sets the current servo angle in degrees.
        /// </summary>
        /// <param name="degree">Degree angle to set.</param>
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

                uint highTime = ScaleRange(degree, lowDegree, highDegree, lowUs, highUs);
                SetPosition(highTime);
            }
        }

        /// <summary>
        /// Sets the current position of the servo.
        /// </summary>
        /// <param name="positionUs">Position in microseconds</param>
        public void SetPosition(uint positionUs)
        {
            if (positionUs < lowUs || positionUs > highUs)
                throw new ArgumentOutOfRangeException("positionUs");

            if (invert)
                positionUs = (highUs - positionUs) + lowUs;

            // OutputCompare uses microsecond (us) scale.
            timings[0] = positionUs;
            timings[1] = periodUs - positionUs;
            oc.Set(true, timings, 0, 2, true);

            currentUs = positionUs;
//            this.lastChanged = DateTime.Now;
        }

        /// <summary>
        /// Stops holding servo at current position by setting PWM low.
        /// </summary>
        public void Stop()
        {
            oc.Set(false);
        }

        /// <summary>
        /// Dispose servo instance.
        /// </summary>
        public void Dispose()
        {
            Stop();
            oc.Dispose();
        }

        private static uint ScaleRange(uint value, uint oldMin, uint oldMax, uint newMin, uint newMax)
        {
            uint v = ((value - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin; // same good.
            return v;
        }
    }
}
