using UnityEditor;
using UnityEngine;

public class SplitPath : ScriptableWizard
{
    FlythroughNode firstNode, secondNode;
    
    [MenuItem("Tools/Split Path")]
    static void OpenWindow()
    {
        DisplayWizard<SplitPath>("Split Path", "Split");
    }

    private void OnWizardUpdate()
    {
        isValid = ValidateNodes();
    }

    void OnWizardCreate()
    {
        Transform firstNodeTransform = firstNode.transform;
        Transform secondNodeTransform = secondNode.transform;
        BezierCurve fullCurve = new BezierCurve(
            firstNodeTransform.position,
            firstNodeTransform.position + firstNodeTransform.forward * firstNode.exitCurveStrength,
            secondNodeTransform.position - secondNodeTransform.forward * secondNode.entryCurveStrength,
            secondNodeTransform.position);
        fullCurve.SplitCurve(out var firstHalf, out var secondHalf);

        firstNode.exitCurveStrength = Vector3.Magnitude(firstHalf.b - firstHalf.a);
        secondNode.entryCurveStrength = Vector3.Magnitude(secondHalf.d - secondHalf.c);

        GameObject newNodeObject = Instantiate(firstNode.gameObject);
        Transform newNodeTransform = newNodeObject.transform;
        newNodeTransform.parent = firstNodeTransform.parent;
        newNodeTransform.SetSiblingIndex(secondNodeTransform.GetSiblingIndex());
        newNodeTransform.position = firstHalf.d;
        newNodeTransform.rotation = Quaternion.LookRotation(firstHalf.d - firstHalf.c);
        newNodeObject.name = "NodeSplit";
        FlythroughNode newNode = newNodeObject.GetComponent<FlythroughNode>();
        newNode.entryCurveStrength = Vector3.Magnitude(firstHalf.d - firstHalf.c);
        newNode.exitCurveStrength = Vector3.Magnitude(secondHalf.b - secondHalf.a);

        EditorUtility.SetDirty(newNodeObject);
        EditorUtility.SetDirty(firstNode);
        EditorUtility.SetDirty(secondNode);
    }

    bool ValidateNodes()
    {
        if (Selection.activeGameObject == null) return false;

        GameObject selectedObject = Selection.activeGameObject;
        firstNode = selectedObject.GetComponent<FlythroughNode>();
        if (firstNode == null) return false;

        Transform selectedTransform = selectedObject.transform;
        Transform parentTransform = selectedTransform.parent;
        if (parentTransform == null) return false;

        if (parentTransform.childCount <= selectedTransform.GetSiblingIndex() + 1) return false;
        Transform nextTransform = parentTransform.GetChild(selectedTransform.GetSiblingIndex() + 1);
        secondNode = nextTransform.GetComponent<FlythroughNode>();
        if (secondNode == null) return false;

        return true;
    }
    
}
