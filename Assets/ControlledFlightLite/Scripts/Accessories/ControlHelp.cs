namespace SparseDesign
{
    public class ControlHelp
    {
        private float P, I, D;
        //private float CommandLimit;
        private float IState, PrevU, PrevTime;
        private bool IsAngles;
        private bool FirstSample;
        public float UVel { get; private set; }

        public ControlHelp(float P, float I, float D, bool IsAngles)
        {
            this.IsAngles = IsAngles;
            InitParameters();
            SetParameters(P, I, D);
        }

        public ControlHelp(float P, float I, float D) : this(P, I, D, IsAngles: false) { }

        void Start()
        {
            InitParameters();
        }

        public void SetParameters(float P, float I, float D)
        {
            this.P = P;
            this.I = I;
            this.D = D;
        }

        private void InitParameters()
        {
            FirstSample = true;
            IState = 0.0f;
            PrevU = 0.0f;
            UVel = 0.0f;
            PrevTime = UnityEngine.Time.time;
        }

        public float UpdateControlCommand(float U)
        {
            float Command;

            if (IsAngles)
            {
                U = MathHelp.RadiansToPiToMinusPi(radians: U);
            }

            if (FirstSample)
            {
                Command = this.P * U;
                FirstSample = false;
            }
            else
            {
                float dt = UnityEngine.Time.time - PrevTime;
                if (dt > float.Epsilon)
                {

                    //float IstateOld = IState;
                    IState += U * dt;
                    float UVel = (U - PrevU);
                    if (IsAngles)
                    {
                        UVel = MathHelp.RadiansToPiToMinusPi(radians: UVel);
                    }
                    UVel /= dt;
                    Command = this.P * U + this.I * IState + this.D * UVel;
                    //if (Mathf.Abs(Command) > CommandLimit)
                    //{

                    //}
                }
                else
                {
                    Command = 0.0f;
                }

            }
            PrevU = U;
            PrevTime = UnityEngine.Time.time;
            return Command;
        }

        public void Reset()
        {
            InitParameters();
        }
    }
}
