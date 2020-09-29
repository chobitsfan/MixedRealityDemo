using UnityEngine;

namespace SparseDesign
{
    public class Filter2
    {
        class FilterStruct
        {
            public Filter x;
            public Filter y;
        }

        private FilterStruct m_filters = default;

        public Filter2(float omega, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega, IsAngles: IsAngles);
        }
        public Filter2(Vector2 omega, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega.x, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega.y, IsAngles: IsAngles);
        }

        public Filter2(float omega) : this(omega, IsAngles: false) { }
        public Filter2(Vector2 omega) : this(omega, IsAngles: false) { }

        public Filter2(float omega, float damping, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega, damping: damping, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega, damping: damping, IsAngles: IsAngles);
        }
        public Filter2(Vector2 omega, Vector2 damping, bool IsAngles)
        {
            m_filters = new FilterStruct();
            m_filters.x = new Filter(omega: omega.x, damping: damping.x, IsAngles: IsAngles);
            m_filters.y = new Filter(omega: omega.y, damping: damping.y, IsAngles: IsAngles);
        }

        public Filter2(float omega, float damping) : this(omega, damping, IsAngles: false) { }
        public Filter2(Vector2 omega, Vector2 damping) : this(omega, damping, IsAngles: false) { }

        public void UpdateParameters(float omega)
        {
            m_filters.x.UpdateParameters(omega: omega);
            m_filters.y.UpdateParameters(omega: omega);
        }

        public void UpdateParameters(Vector2 omega)
        {
            m_filters.x.UpdateParameters(omega: omega.x);
            m_filters.y.UpdateParameters(omega: omega.y);
        }

        public void UpdateParameters(Vector2 omega, Vector2 damping)
        {
            m_filters.x.UpdateParameters(omega: omega.x, damping: damping.x);
            m_filters.y.UpdateParameters(omega: omega.y, damping: damping.y);
        }

        public Vector2 UpdateFilter(Vector2 U)
        {
            m_filters.x.UpdateFilter(U.x);
            m_filters.y.UpdateFilter(U.y);

            return GetValue();
        }

        public Vector2 UpdateFilter(float x, float y)
        {
            return UpdateFilter(new Vector2(x, y));
        }

        public Vector2 GetValue()
        {
            return new Vector2(m_filters.x.GetValue(), m_filters.y.GetValue());
        }

        public void Reset()
        {
            m_filters.x.Reset();
            m_filters.y.Reset();
        }
    }
}