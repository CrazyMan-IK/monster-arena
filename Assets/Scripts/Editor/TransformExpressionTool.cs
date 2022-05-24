using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System.Linq;

// The second argument in the EditorToolAttribute flags this as a Component tool. That means that it will be instantiated
// and destroyed along with the selection. EditorTool.targets will contain the selected objects matching the type.
[EditorTool("Transform Expreesion Tool", typeof(Transform))]
class TransformExpressionTool : EditorTool
{
    private string _expressionX = "{v}/(75/35)";
    private string _expressionY = "{v}";
    private string _expressionZ = "{v}/(75/35)";
    
    private float EvaluateExpression(string expression, float value)
    {
        float result = 0;
        
        if (expression != "")
        {
            ExpressionEvaluator.Evaluate(expression.Replace("{v}", value.ToString()), out result);
        }

        return result;
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView))
            return;

        Handles.BeginGUI();
        using (new GUILayout.HorizontalScope())
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _expressionX = EditorGUILayout.TextField("Expression X", _expressionX);
                _expressionY = EditorGUILayout.TextField("Expression Y", _expressionY);
                _expressionZ = EditorGUILayout.TextField("Expression Z", _expressionZ);

                EditorGUILayout.Space();

                if (GUILayout.Button("Apply"))
                {
                    foreach (var transform in targets.OfType<Transform>())
                    {
                        Undo.RecordObject(transform, "Apply expression");

                        transform.localPosition = new Vector3(
                            EvaluateExpression(_expressionX, transform.localPosition.x),
                            EvaluateExpression(_expressionY, transform.localPosition.y),
                            EvaluateExpression(_expressionZ, transform.localPosition.z));
                    }
                }
            }

            GUILayout.FlexibleSpace();
        }
        Handles.EndGUI();
    }
}