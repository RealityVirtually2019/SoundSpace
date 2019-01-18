using UnityEditor;
using UnityEngine;

namespace UTJ.Alembic
{
    [CustomEditor(typeof(AlembicStreamDescriptor))]
    public class AlembicStreamDescriptorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
#if UNITY_2017_1_OR_NEWER || ENABLE_SCRIPTED_IMPORTERS
            SerializedProperty iterator = this.serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
            }
#else
            base.OnInspectorGUI();
#endif
        }

    }

}
