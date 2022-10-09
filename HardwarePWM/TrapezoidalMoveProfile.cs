using System;
using Microsoft.SPOT;
using System.Threading;

namespace HardwarePWM {
    class TrapezoidalMoveProfile {

        private enum State {
            IDLE,
            ACCELERATING,
            AT_MAX_SPEED,
            DECELERATING
        }

        private double pwm_current;
        private double pwm_target;
        private double seconds;

        //public ServoHardPWM pwm { get; set; }
        public IServo pwm { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pwm_current"></param>
        /// <param name="pwm_target"></param>
        /// <param name="seconds"></param>
        public TrapezoidalMoveProfile(double pwm_current, double pwm_target, double seconds) {
            this.pwm_current = pwm_current;
            this.pwm_target = pwm_target;
            this.seconds = seconds;
        }

        public void Begin() {
            // Calculate required velocity given as v = s/t, where s is distance, and t is time.
            double distance = pwm_target - pwm_current;
            double steps = seconds * 1000 / 10;
            double max_velocity = 1.5 * (distance / steps);   // PWM width in microseconds (us) per step

            int direction = 1;
            if (distance < 0) {
                direction = -1;
            }

            // Calculate acceleration and deceleration ramp rates.
            double acceleration = 4.5 * (distance / System.Math.Pow(steps, 2));

            Debug.Print("direction -> " + direction);
            Debug.Print("distance -> " + distance);
            Debug.Print("steps -> " + steps);
            Debug.Print("max_velocity -> " + max_velocity);
            Debug.Print("acceleration -> " + acceleration);

            Debug.Print("--Ramping--");

            double step = 1;
            double deceleration_step = 0;
            double speed = 0;
            State state = State.ACCELERATING;

            DateTime begin = DateTime.Now;
            while (step < steps) {
//                Debug.Print("  '--> PWM -> " + pwm_current + " -- Speed -> " + speed + " -- Acceleration -> " + acceleration);

                switch (state) {
                    case State.ACCELERATING:
                        speed = speed + acceleration;
                        pwm_current += speed;

                        if (speed > max_velocity && direction == 1 || speed < max_velocity && direction == -1) {
                            // Calculate deceleration step and change state.
                            deceleration_step = steps - step;
                            state = State.AT_MAX_SPEED;
                        }
//                        Debug.Print("ACCELERATING");
                        break;
                    case State.AT_MAX_SPEED:
                        pwm_current += speed;
                        if (step == deceleration_step) {
                            state = State.DECELERATING;
                        }
//                        Debug.Print("AT_MAX_SPEED");
                        break;
                    case State.DECELERATING:
                        speed = speed - acceleration;
                        pwm_current += speed;
                        if (step == steps)
                            state = State.IDLE;
//                        Debug.Print("DECELERATING");
                        break;
                    default:
                        break;
                }

                pwm.SetPosition((uint)pwm_current);

                step++;

                Thread.Sleep(10);
            }
            DateTime end = DateTime.Now;

            Debug.Print("--Time elapsed -> " + end.Subtract(begin).Milliseconds + " ms --");

            Debug.Print("--Done--");
        }

    }
}
