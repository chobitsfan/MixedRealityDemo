using UnityEngine;

namespace SparseDesign
{
    public class Filter
    {
        private float omega, damping;
        private bool FirstSample;
        private int order;

        //private float Uzm1, Uzm2, Yzm1, Yzm2
        private float PrevTime, PrevPrevTime, PrevY, a0, a1, ym1, ym2, xm1, xm2;
        private bool IsAngles;

        public Filter(float omega, bool IsAngles)
        {
            order = 1;

            this.IsAngles = IsAngles;

            //this.omega = omega;

            UpdateParameters(omega);

            InitParameters();
        }

        public Filter(float omega) : this(omega, IsAngles: false)
        {
        }
        public Filter(float omega, float damping, bool IsAngles)
        {
            order = 2;

            this.IsAngles = IsAngles;

            UpdateParameters(omega, damping);

            //this.omega = omega;
            //this.damping = damping;

            //a0 = 1 / Mathf.Pow(this.omega, 2f);
            //a1 = 2 * this.damping / this.omega;

            InitParameters();
        }

        public void UpdateParameters(float omega)
        {
            UpdateParameters(omega, this.damping);
        }

        public void UpdateParameters(float omega, float damping)
        {
            this.omega = omega;
            this.damping = damping;

            a0 = 1 / (this.omega * this.omega);// Mathf.Pow(this.omega, 2f);
                                               //a1 = 2 * this.damping / this.omega;
            a1 = 2f * this.damping * this.omega * a0;
        }

        public Filter(float omega, float damping) : this(omega, damping, IsAngles: false)
        {
        }

        private void InitParameters()
        {
            FirstSample = true;
            PrevTime = UnityEngine.Time.time;
        }

        public float UpdateFilter(float U)
        {
            return UpdateFilter(U: U, limitSpeed: false, speedLimit: 0f);
        }

        public float UpdateFilter(float U, float speedLimit)
        {
            return UpdateFilter(U: U, limitSpeed: true, speedLimit: speedLimit);
        }

        public float UpdateFilter(float U, bool limitSpeed, float speedLimit)
        {
            float Y = PrevY;

            if (FirstSample)
            {
                FirstSample = false;
                PrevY = U;

                ym1 = ym2 = xm1 = xm2 = U;
                PrevTime = UnityEngine.Time.time - 1f;
                PrevPrevTime = PrevTime - 2f;
            }

            float dt = UnityEngine.Time.time - PrevTime;

            if (order == 1)
            {
                float alpha = (omega * dt) / (1 + omega * dt);
                float Diff = U - PrevY;
                if (IsAngles)
                {
                    Diff = MathHelp.RadiansToPiToMinusPi(radians: Diff);
                }
                Y = PrevY + alpha * Diff;
            }
            else if (order == 2)
            {
                float dtOld = PrevTime - PrevPrevTime;
                float xm1old = xm1;
                float ym1old = ym1;
                if (dt < Mathf.Epsilon || dtOld < Mathf.Epsilon)
                {
                    return Y;
                }

                if (dt <= dtOld)
                {
                    float dtOldinv = 1 / dtOld;
                    xm2 = xm1 - dt * (xm1 - xm2) * dtOldinv;
                    ym2 = ym1 - dt * (ym1 - ym2) * dtOldinv;
                }
                else
                {
                    xm1 = U - dtOld * (U - xm1) / dt;
                    ym1 = ym2 + (dt + dtOld) / 2 * (ym1 - ym2) / dtOld;
                    dt = dtOld;
                }
                float w = omega;
                float K = w / Mathf.Tan(w * dt / 2);//Prewarp (https://en.wikipedia.org/wiki/Bilinear_transform)

                float num = a0 * (K * K) + a1 * K + 1;

                Y = U +
                    (2 * 1) * xm1 +
                    1 * xm2 + -(2 * 1 - 2 * a0 * (K * K)) * ym1 +
                   -(a0 * (K * K) - a1 * K + 1) * ym2;//Bilinear transformation
                Y = Y / num;

                ym2 = ym1old;
                ym1 = Y;
                xm2 = xm1old;
                xm1 = U;

            }

            if (limitSpeed && Mathf.Abs(Y - PrevY) > dt * speedLimit)
            {
                Y = PrevY + Mathf.Sign(Y - PrevY) * speedLimit * dt;
            }

            PrevY = Y;
            PrevPrevTime = PrevTime;
            PrevTime = UnityEngine.Time.time;

            return Y;
        }

        public float GetValue()
        {
            return PrevY;
        }

        public void Reset()
        {
            InitParameters();
        }
    }
}