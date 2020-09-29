namespace SparseDesign
{
    public class Derivative
    {
        private bool FirstSample;

        private float PrevTime, PrevU;
        private bool IsAngles;
        public float CurrentDerivative { get; private set; }

        public Derivative(bool IsAngles)
        {
            FirstSample = true;

            this.IsAngles = IsAngles;
        }

        public Derivative() : this(IsAngles: false) { }

        public float Update(float U)
        {
            if (FirstSample)
            {
                FirstSample = false;
                CurrentDerivative = 0.0f;
            }
            else
            {
                float dt = UnityEngine.Time.time - PrevTime;
                if (dt > 0)
                {
                    float Diff = U - PrevU;
                    if (IsAngles)
                    {
                        Diff = MathHelp.RadiansToPiToMinusPi(radians: Diff);
                    }
                    CurrentDerivative = Diff / dt;
                }
            }
            PrevU = U;
            PrevTime = UnityEngine.Time.time;
            return CurrentDerivative;
        }
    }
}

