using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathTools : ScriptableWizard
{
    [SerializeField] float smoothingFactor = 1f;


    [MenuItem("Tools/Smooth Nodes")]
    static void OpenWindow()
    {
        DisplayWizard<PathTools>("Smooth Nodes", "Smooth");
    }

    void OnWizardCreate()
    {
        FlythroughNode[] nodeList = PopulateNodes();
        for (int i = 0; i < nodeList.Length; i++)
            SmoothNode(nodeList[i]);
    }

    void SmoothNode(FlythroughNode node)
    {
        Transform nodeTransform = node.transform;
        if (nodeTransform.parent == null) return;
        Transform parentTransform = nodeTransform.parent;
        int siblingIndex = nodeTransform.GetSiblingIndex();
        Vector3 previousNodePosition = (siblingIndex > 0) ?
            parentTransform.GetChild(siblingIndex - 1).position :
            nodeTransform.position - nodeTransform.forward;
        Vector3 nextNodePosition = (siblingIndex < parentTransform.childCount - 1) ?
            parentTransform.GetChild(siblingIndex + 1).position :
            nodeTransform.position + nodeTransform.forward;

        SetBezierCurve(node, previousNodePosition, nextNodePosition);
    }

    void SetBezierCurve(FlythroughNode node, Vector3 previousNodePosition, Vector3 nextNodePosition)
    {
        node.transform.rotation = Quaternion.LookRotation(nextNodePosition - previousNodePosition);
        float curveStrength = GetCatmullVectorSize(previousNodePosition, nextNodePosition, smoothingFactor);
        node.entryCurveStrength = curveStrength;
        node.exitCurveStrength = curveStrength;
    }

    float GetCatmullVectorSize(Vector3 p0, Vector3 p1, float smoothingFactor)
    {
        return Vector3.Magnitude(p0 - p1) / (6f * smoothingFactor);
    }

    FlythroughNode[] PopulateNodes()
    {
        List<FlythroughNode> nodes = new List<FlythroughNode>();
        foreach (var obj in Selection.gameObjects)
        {
            FlythroughNode node = obj.GetComponent<FlythroughNode>();
            if (node != null) nodes.Add(node);
        }

        return nodes.ToArray();
    }
}
