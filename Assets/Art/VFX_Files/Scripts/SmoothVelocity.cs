// Smooth towards the target

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class SmoothVelocity : MonoBehaviour
{
    //public Transform target;
    [SerializeField] private AnimationCurve easeIn;
    [SerializeField] private VisualEffect VFX;
    [SerializeField] private ExposedProperty speedProperty = "smoothSpeed" ;
    

    private Vector3 prevPos =  Vector3.zero;

    private void Start()
    {
        prevPos = transform.position;
    }

    void Update()
    {
        
        var delta = transform.position - prevPos;
        var zSpeed = Mathf.Clamp01(Vector3.Dot(Vector3.forward, transform.InverseTransformVector(delta)));
        var smoothSpeed = easeIn.Evaluate(zSpeed);
        prevPos = transform.position;
        
        //Debug.Log($"{zSpeed} / {smoothSpeed}");
        VFX.SetFloat(speedProperty, smoothSpeed);
        

        
        
        

    }
}