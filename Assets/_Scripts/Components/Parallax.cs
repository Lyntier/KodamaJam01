using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Parallax : MonoBehaviour
{
    [SerializeField] float scale = 1f;
    [SerializeField] float smoothing = 1f;

    Vector3 startPos;
    Transform _cameraTransform;

    Vector3 _camLastFrame;
    
    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNotNull(Camera.main);
        _cameraTransform = Camera.main.transform;
        _camLastFrame = _cameraTransform.position;
        
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float parallaxX = (_camLastFrame.x - _cameraTransform.position.x) * scale;
        Vector3 backgroundTargetPosX = new Vector3(transform.position.x + parallaxX, 
            transform.position.y, 
            transform.position.z);
			
        // Lerp to fade between positions
        transform.position = Vector3.Lerp(transform.position, backgroundTargetPosX, smoothing * Time.deltaTime);

        _camLastFrame = _cameraTransform.position;
    }
}
