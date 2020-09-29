using UnityEngine;

namespace SparseDesign
{
    public class Filter3
    {
        class FilterStruct
        {
            public Filter x;
            public Filter y;
            public Filter z;
        }

        private FilterStruct m_filters = default;

        public Filter3(float omega, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega, IsAngles: IsAngles);
            m_filters.z = new Filter(omega: omega, IsAngles: IsAngles);
        }
        public Filter3(Vector3 omega, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega.x, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega.y, IsAngles: IsAngles);
            m_filters.z = new Filter(omega: omega.z, IsAngles: IsAngles);
        }

        public Filter3(float omega) : this(omega, IsAngles: false) { }
        public Filter3(Vector3 omega) : this(omega, IsAngles: false) { }

        public Filter3(float omega, float damping, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega, damping: damping, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega, damping: damping, IsAngles: IsAngles);
            m_filters.z = new Filter(omega: omega, damping: damping, IsAngles: IsAngles);
        }
        public Filter3(Vector3 omega, Vector3 damping, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega.x, damping: damping.x, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega.y, damping: damping.y, IsAngles: IsAngles);
            m_filters.z = new Filter(omega: omega.z, damping: damping.z, IsAngles: IsAngles);
        }

        public Filter3(float omega, float damping) : this(omega, damping, IsAngles: false) { }
        public Filter3(Vector3 omega, Vector3 damping) : this(omega, damping, IsAngles: false) { }

        public void UpdateParameters(float omega)
        {
            m_filters.x.UpdateParameters(omega: omega);
            m_filters.y.UpdateParameters(omega: omega);
            m_filters.z.UpdateParameters(omega: omega);
        }

        public void UpdateParameters(Vector3 omega)
        {
            m_filters.x.UpdateParameters(omega: omega.x);
            m_filters.y.UpdateParameters(omega: omega.y);
            m_filters.z.UpdateParameters(omega: omega.z);
        }

        public void UpdateParameters(Vector3 omega, Vector3 damping)
        {
            m_filters.x.UpdateParameters(omega: omega.x, damping: damping.x);
            m_filters.y.UpdateParameters(omega: omega.y, damping: damping.y);
            m_filters.z.UpdateParameters(omega: omega.z, damping: damping.z);
        }

        public Vector3 UpdateFilter(Vector3 U)
        {
            m_filters.x.UpdateFilter(U.x);
            m_filters.y.UpdateFilter(U.y);
            m_filters.z.UpdateFilter(U.z);

            return GetValue();
        }

        public Vector3 GetValue()
        {
            return new Vector3(m_filters.x.GetValue(), m_filters.y.GetValue(), m_filters.z.GetValue());
        }

        public void Reset()
        {
            m_filters.x.Reset();
            m_filters.y.Reset();
            m_filters.z.Reset();
        }
    }
}