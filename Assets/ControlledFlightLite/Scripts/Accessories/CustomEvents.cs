using System;
using UnityEngine;
using UnityEngine.Events;

namespace SparseDesign
{
    [Serializable] public class NormalEvent : UnityEvent { }
    [Serializable] public class FloatEvent : UnityEvent<float> { }
    [Serializable] public class BoolEvent : UnityEvent<bool> { }
    [Serializable] public class IntEvent : UnityEvent<int> { }
    [Serializable] public class Vector2Event : UnityEvent<Vector2> { }
    [Serializable] public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable] public class Collision2DEvent : UnityEvent<Collision2D> { }
    [Serializable] public class GameObjectEvent : UnityEvent<GameObject> { }
    [Serializable] public class GameObjectBoolEvent : UnityEvent<GameObject, bool> { }

    public class CustomEvents : MonoBehaviour { }
}