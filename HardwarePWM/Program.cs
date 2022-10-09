using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace HardwarePWM
{
    public class Program
    {
        const uint PERIOD = 20 * 1000 * 1000; // 20ms is 20000000ms(nanoseconds)
        const uint LowUs = 675;   // min range of servo as microseconds.
        const uint HighUs = 2250; // max range of servo as microseconds.

        ///// <summary>
        ///// Set servo position.
        ///// </summary>
        ///// <remarks>
        ///// The standard range for 180degree servos is 1 to 2 milliseconds high in a 20ms period, with
        ///// neutral in the middle at 1.5ms. Exact range varies between servos. Find exact range with
        ///// a servo tester or PWM range testing.
        ///// </remarks>
        ///// <param name="servo">PWM pin of servo control.</param>
        ///// <param name="highUs">High time in microseconds (e.g. 1500us is 1.5ms or neutral)</param>
        //public static void SetSimpleServo(PWM servo, uint highUs)
        //{
        //    Debug.Print("Set servo nanoseconds: " + highUs);
        //    servo.SetPulse(PERIOD, highUs * 1000);
        //}

        //public static void SetSimpleServoByDegree(PWM servo, uint degree, bool invert)
        //{
        //    if (degree < 0 || degree > 180)
        //        throw new ArgumentOutOfRangeException("angleDegree");

        //    if (invert)
        //        degree = (byte)(180 - degree);

        //    // Scale degree (0-180) to servo position. Mult 1K to get to nanoseconds.
        //    uint pos = ScaleRange(degree, 0, 180, LowUs, HighUs) * 1000;
        //    servo.SetPulse(PERIOD, pos);

        //    Debug.Print("Degree:" + degree + " Position ms:" + pos / (double)1000000);
        //}

        //private static uint ScaleRange(uint oldValue, uint oldMin, uint oldMax, uint newMin, uint newMax)
        //{
        //    return ((oldValue - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
        //}



        public static void Main()
        {
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.Di10, 20000, LowUs, HighUs, 0, 180);      // PWM 1    OK
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.Di9, 20000, LowUs, HighUs, 0, 180);       // PWM 2    OK
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.Di8, 20000, LowUs, HighUs, 0, 180);       // PWM 3    OK
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.MOD, 20000, LowUs, HighUs, 0, 180);       // PWM 4    OK
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.Di5, 20000, LowUs, HighUs, 0, 180);       // PWM 5    OK
            //ServoHardPWM pwm = new ServoHardPWM(FEZ_Pin.PWM.Di6, 20000, LowUs, HighUs, 0, 180);       // PWM 6    OK

            ServoSoftPWM pwm = new ServoSoftPWM(FEZ_Pin.Digital.Di30, 20000, LowUs, HighUs, 0, 180);

            // Turn off board LED
            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            InputPort button;
            button = new InputPort((Cpu.Pin)FEZ_Pin.Digital.LDR, false, Port.ResistorMode.PullUp);

            // Calibration.
            uint degree = 50;
            //pwm.SetDegree(degree);
            pwm.SetPosition(LowUs);

            bool firstRun = true;

            while (true)
            {
                Thread.Sleep(100);

                // Toggle LED state
                ledState = !ledState;
                led.Write(ledState);

                bool ldrState;
                if ((ldrState = button.Read()) != true || firstRun)
                {
                    firstRun = false;

                    if (degree == 50)
                        degree = 130;
                    else
                        degree = 50;

                    pwm.SetDegree(degree);

                    Debug.Print("Current command angle->" + degree);

                    Thread.Sleep(500);
                }
            }
        }
    }
}
