using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Bee : MonoBehaviour
{
    [SerializeField] float smoothing;
    [SerializeField] float minRange;
    [SerializeField] float maxRange;
    [SerializeField] float xScaleDistance;
    [SerializeField] float yScaleDistance;


    GameObject _target;

    void Start()
    {
        _target = GameObject.FindWithTag("Player");
        Assert.IsNotNull(_target);
        if (minRange > maxRange)
        {
            Debug.Log("Range is messed up, using default values.");
            minRange = 3f;
            maxRange = 10f;
        }
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, _target.transform.position);
        
        // Creates a half circle above the player that the bee follows.
        float xDelta = Mathf.Sin(Time.time) * xScaleDistance;
        float yDelta = Mathf.Abs(Mathf.Cos(Time.time)) * yScaleDistance;

        Vector2 newPos = (Vector2) _target.transform.position + new Vector2(xDelta, yDelta);


        Vector2 oldPos = transform.position;
        newPos = Vector2.Lerp(oldPos, newPos, smoothing);
        transform.position = newPos;
    }
}