using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SparseDesign
{
    namespace ControlledFlight
    {
        [CustomEditor(typeof(MissileSupervisor))]

        public class MissileSupervisorInspector : Editor
        {
            SerializedProperty m_pathObjsProp;

            SerializedProperty m_interceptEventProp;
            SerializedProperty m_launchEventProp;

            SerializedProperty m_motorSettingsProp;

            bool m_showEvents = true;
            bool m_showMissileLimitations = true;
            bool m_showMissileGuidanceProperties = true;
            bool m_showInterceptProperties = true;
            bool m_showLaunchAndThrust = true;

            private MissileSupervisor m_missileSupervisor = default;
            SerializedProperty m_guidSettingsProp;

            private void OnEnable()
            {
                if (m_missileSupervisor == null)
                {
                    m_missileSupervisor = (MissileSupervisor)target;
                }

                m_guidSettingsProp = serializedObject.FindProperty("m_guidanceSettings");
                m_pathObjsProp = m_guidSettingsProp.FindPropertyRelative("m_pathObjs");

                m_motorSettingsProp = serializedObject.FindProperty("m_motorStages");

                m_interceptEventProp = serializedObject.FindProperty("m_interceptEvent");
                m_launchEventProp = serializedObject.FindProperty("m_launchEvent");
            }

            public override void OnInspectorGUI()
            {
                EditorGUIUtility.labelWidth = 160;
                if (m_missileSupervisor == null) m_missileSupervisor = (MissileSupervisor)target;

                MissileGuidance.GuidanceSettings guidanceSettings = m_missileSupervisor.m_guidanceSettings;

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_targetType"),
                    new GUIContent("Target Type", "[m_guidanceSettings.m_targetType]"));

                bool isTarget = guidanceSettings.m_targetType == MissileGuidance.TargetType.TARGET;
                bool isPath = guidanceSettings.m_targetType == MissileGuidance.TargetType.PATH;

                EditorGUI.indentLevel++;

                switch (guidanceSettings.m_targetType)
                {
                    case MissileGuidance.TargetType.TARGET:
                        EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_target"), new GUIContent("Target", "Gameobject to intercept.\nThe object does not need to contain a Rigidbody but it will typically work better, especially at higher target speeds.\n[m_guidanceSettings.m_target]"));
                        break;
                    case MissileGuidance.TargetType.PATH:
                        EditorGUILayout.PropertyField(m_pathObjsProp, new GUIContent("Path Objects", "Objects used as waypoints in the path.\n[m_guidanceSettings.m_pathObjs]"));
                        EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_loopPath"), new GUIContent("Loop path", "Repeat path by continuing from last waypoint to first. [m_guidanceSettings.m_loopPath]"));
                        break;
                }
                EditorGUI.indentLevel--;

#if UNITY_2019_1_OR_NEWER
                m_showMissileGuidanceProperties = EditorGUILayout.BeginFoldoutHeaderGroup(m_showMissileGuidanceProperties, "Guidance properties");
#endif
                if (m_showMissileGuidanceProperties)
                {
                    EditorGUIUtility.labelWidth = 160;

                    if (isTarget) GUILayout.Label(new GUIContent("Algorithm: PROPORTIONALNAVIGATION", "Can be changed in the full version of Controlled Flight."));

                    if (isTarget) EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_N"), new GUIContent("Navigation Constant", "See https://en.wikipedia.org/wiki/Proportional_navigation .\n[m_guidanceSettings.m_N]"));

                    if (isPath)
                    {
                        EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_tTurnIn"), new GUIContent("Time to path",
                            "The approximate time required to turn into the path or line of sight.\nSmall value means fast convergence to the path.\n" +
                            "Too small a value might create instability.\n[m_guidanceSettings.m_tTurnIn]"));
                        guidanceSettings.m_tTurnIn = MathHelp.LimitFloor(guidanceSettings.m_tTurnIn, 0.1f);

                        EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_turnMarginSimple"), new GUIContent("Turn distance factor", "Used to tweak when the turn is to begin. Higher value means to start turn earlier.\n" +
                            "-1: Start turn at waypoint.\n" +
                            "0: Nominal.\n" +
                            "1: Start turn far from waypoint.\n" +
                            "[m_guidanceSettings.m_turnMarginSimple]"));
                    }
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();

                m_showMissileLimitations = EditorGUILayout.BeginFoldoutHeaderGroup(m_showMissileLimitations, "Limitations");
#endif
                if (m_showMissileLimitations)
                {
                    EditorGUIUtility.labelWidth = 160;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_limitAcceleration"), new GUIContent("Limit maneouvre [m/s2]", "Limit the maneouver acceleration [m/s^2].\nRecommendation is this to always be true to avoid strange results, especially close to the target.\n[m_guidanceSettings.m_limitAcceleration]"));
                    if (guidanceSettings.m_limitAcceleration) EditorGUILayout.PropertyField(m_guidSettingsProp.FindPropertyRelative("m_maxAcceleration"), new GUIContent());
                    if (guidanceSettings.m_maxAcceleration < 0f) { guidanceSettings.m_maxAcceleration = 0f; }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_limitFlightTime"), new GUIContent("Limit flight time [Seconds]", "[m_limitFlightTime]"));
                    if (m_missileSupervisor.m_limitFlightTime) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_flightTime"), new GUIContent());
                    if (m_missileSupervisor.m_flightTime < 0f) { m_missileSupervisor.m_flightTime = 0f; }
                    GUILayout.EndHorizontal();
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();

                m_showInterceptProperties = EditorGUILayout.BeginFoldoutHeaderGroup(m_showInterceptProperties, "Intercept properties");
#endif
                if (m_showInterceptProperties)
                {
                    EditorGUIUtility.labelWidth = 170;

                    if (isTarget)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_interceptRange"), new GUIContent("Hit Range [m]", "If the missile passes the target within this range it is counted as a hit.\n[m_interceptRange]"));
                        if (m_missileSupervisor.m_interceptRange < 0f) { m_missileSupervisor.m_interceptRange = 0f; }
                    }

                    if (isTarget)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_stopAtIntercept"), new GUIContent("Stop guidance after intercept", "Stops guidance after intercept (passes target).\nThe missile continues going straight ahead.\n[m_stopAtIntercept]"));
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_destroyAtHit"), new GUIContent("Destroy missile at hit", "Destroys and removes missile if hit target.\n[m_destroyAtHit]"));
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();

                m_showLaunchAndThrust = EditorGUILayout.BeginFoldoutHeaderGroup(m_showLaunchAndThrust, "Launch, speed and motor properties");
#endif
                if (m_showLaunchAndThrust)
                {
                    EditorGUIUtility.labelWidth = 150;

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    if (GUILayout.Button(new GUIContent("Start Launch Sequence", "Track and then launch. Set Tracking time to 0 for immediate launch.\nUses MissileSupervisor.StartLaunchSequence().")) &&
                        EditorApplication.isPlaying &&
                        m_missileSupervisor.gameObject.activeSelf)
                    {
                        m_missileSupervisor.StartLaunchSequence();
                    }

                    if (GUILayout.Button(new GUIContent("Launch", "Launch now. Will launch in the current direction, igonoring tracking (Launch mode).\nUses MissileSupervisor.Launch(forceLaunch: true)")) &&
                        EditorApplication.isPlaying &&
                        m_missileSupervisor.gameObject.activeSelf)
                    {
                        m_missileSupervisor.Launch(forceLaunch: true);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_launchType"), new GUIContent("Launch Mode", "[m_launchType]"));
                    EditorGUI.indentLevel++;
                    if (m_missileSupervisor.m_launchType == MissileSupervisor.LaunchType.CUSTOMDIRECTION) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_launchCustomDir"), new GUIContent("Launch direction", ""));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_autoLaunch"), new GUIContent("Automatic launch", "Launch sequence is started in Start() method.\n[m_autoLaunch]"));
                    EditorGUI.indentLevel++;
                    if (m_missileSupervisor.m_autoLaunch) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_trackingDelay"), new GUIContent("Tracking time [sec]", "Tracking time before launch [seconds].\nWill always be atleast one frame and physics update, even if set to 0.\n[m_trackingDelay]"));
                    if (m_missileSupervisor.m_trackingDelay < 0f) { m_missileSupervisor.m_trackingDelay = 0f; }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_launchSpeed"), new GUIContent("Launch Speed [m/s]", "Speed at launch. This will be added to any speed the missile already have.\n[m_launchSpeed]"));

                    var rb = m_missileSupervisor.GetComponent<Rigidbody>();
                    if (rb && (rb.drag >= float.Epsilon))
                    {
                        EditorGUILayout.TextArea("Note that drag is enabled in the rigidbody (since drag > 0).\n" +
                            "This is not necessarily a problem, but make sure it is what you want.\n" +
                            "The rigidbodys drag will be disabled if drag is enabled below.",
                            new GUIStyle { stretchHeight = true, wordWrap = true });
                    }

                    int ListSize = m_motorSettingsProp.arraySize;
                    ListSize = EditorGUILayout.DelayedIntField(new GUIContent("# Stages", "The stages will be done in order.\nNote that limit flight time needs to be true for the next stage to be used.\n[m_motorStages]"), ListSize);
                    if (ListSize < 1) ListSize = 1;
                    if (ListSize != m_motorSettingsProp.arraySize)
                    {
                        while (ListSize > m_motorSettingsProp.arraySize)
                        {
                            m_motorSettingsProp.InsertArrayElementAtIndex(m_motorSettingsProp.arraySize);
                            serializedObject.ApplyModifiedProperties();//So the next line can be done
                            m_missileSupervisor.m_motorStages[m_missileSupervisor.m_motorStages.Count - 1] = new ThrustControl.MotorSettings();
                        }

                        EditorGUI.BeginChangeCheck();
                        while (ListSize < m_motorSettingsProp.arraySize) m_motorSettingsProp.DeleteArrayElementAtIndex(m_motorSettingsProp.arraySize - 1);
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUI.indentLevel++;

                    GUIStyle style1 = new GUIStyle();
                    style1.fontStyle = FontStyle.BoldAndItalic;
                    EditorGUIUtility.labelWidth = 170;

                    GUILayout.Label(new GUIContent("Algorithm: CONSTANTSPEED", "Keeps the speed constant.\nCan be changed in the full version of Controlled Flight."));

                    for (int i = 0; i < m_motorSettingsProp.arraySize; i++)
                    {
                        var stage = m_missileSupervisor.m_motorStages[i];

                        EditorGUILayout.LabelField(new GUIContent($"Stage {i + 1}", $"m_motorStages[{i}]"), style1);
                        SerializedProperty sp = m_motorSettingsProp.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        ShowRelativeProperty(sp, "m_limitFlightTime", "Limit time [seconds]", $"The time until the next stage.\nNote: if there is no next stage this will not stop the stage.\n[m_motorStages[{i}].m_limitFlightTime]");
                        if (stage.m_limitFlightTime) ShowRelativeProperty(sp, "m_flightTime", "", $"[m_motorStages[{i}].m_flightTime]");
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        var indentLevel = EditorGUI.indentLevel;

                        ShowRelativeProperty(sp, "m_limitMotorAcceleration", "Limit Acceleration [m/s2]", $"If set to 0 it will keep the speed of the missile constant.\nm_motorStages[{i}].m_limitMotorAcceleration]");

                        if (stage.m_limitMotorAcceleration)
                            ShowRelativeProperty(sp, "m_maxAcceleration", "", $"Acceleration [m/s2]\n[m_motorStages[{i}].m_maxAcceleration]");

                        EditorGUI.indentLevel = indentLevel;
                        EditorGUILayout.EndHorizontal();

                        ShowRelativeProperty(sp, "m_speed", "Speed [m/s]", $"[m_motorStages[{i}].m_speed]");
                    }
                    EditorGUI.indentLevel--;
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();

                m_showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(m_showEvents,
                    new GUIContent("Events",
                    $"Intercept Event [{m_interceptEventProp.name}]: Invoked when the missile has intercept the target within hit range.\n\n" +
                    $"Launched event [{m_launchEventProp.name}]: Invoked when the missile is launched.\n\n"));
#endif

                if (m_showEvents)
                {
                    if (isTarget)
                    {
                        EditorGUILayout.PropertyField(m_interceptEventProp, new GUIContent("Intercept event", "Invoked when the missile has intercept the target within hit range"));
                    }

                    EditorGUILayout.PropertyField(m_launchEventProp, new GUIContent("Launched event", "Invoked when the missile is launched"));
                }
#if UNITY_2019_1_OR_NEWER
                EditorGUILayout.EndFoldoutHeaderGroup();
#endif

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_debug"), new GUIContent("Debug Info", "Prints and draw debug info. [m_debug]"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    var activeScene = EditorSceneManager.GetActiveScene();
                    if (!EditorApplication.isPlaying && (activeScene != null)) EditorSceneManager.MarkSceneDirty(activeScene);

                    var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();//Todo: Do not use experimantal features
                    if (prefabStage != null)
                    {
                        EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                    }
                }
            }
            // Show child property of parent serializedProperty
            void ShowRelativeProperty(SerializedProperty serializedProperty, string propertyName, bool customText, string txt, bool toolTip, string toolTipText)
            {
                SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
                if (property != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    if (!customText)
                        EditorGUILayout.PropertyField(property, true);
                    else
                    {
                        var tipStr = toolTip ? toolTipText : "";
                        EditorGUILayout.PropertyField(property, new GUIContent(txt, tipStr), true);
                    }

                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
            }
            void ShowRelativeProperty(SerializedProperty serializedProperty, string propertyName, string txt, string toolTipText) => ShowRelativeProperty(serializedProperty, propertyName, true, txt, true, toolTipText);
        }
    }
}