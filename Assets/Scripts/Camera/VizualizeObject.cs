using System;
using System.Collections;
using System.Collections.Generic;
using AnimalsManager;
using UnityEngine;
using UnityEngine.AI;

public class VizualizeObject : MonoBehaviour
{
    public static VizualizeObject Instance;
    private Vector3 posLastFame;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            posLastFame = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition - posLastFame;
            delta.y = 0;
            posLastFame = Input.mousePosition;

            var axis = Quaternion.AngleAxis(-90f, Vector3.forward) * delta;
            transform.rotation = Quaternion.AngleAxis(delta.magnitude * .1f, axis) * transform.rotation;
        }
    }

    private void OnEnable()
    {
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;
    }

    private void OnDisable()
    {
        Destroy(transform.GetChild(0).gameObject);
    }

    public void AddElement(Animals animals)
    {
        var anim = Instantiate(animals, transform);
        
        anim.GetComponent<CapsuleCollider>().enabled = false;
        anim.GetComponent<NavMeshAgent>().enabled = false;
        anim.GetComponent<Animals>().enabled = false;
        anim.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.NameToLayer("UI");
    }
}
