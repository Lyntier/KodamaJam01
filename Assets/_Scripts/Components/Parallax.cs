using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

public class Parallax : MonoBehaviour
{
    Transform[] _backgrounds;
    float[] _backgroundParallaxAmount;
    [SerializeField] float smoothing = 1f;
    [SerializeField] float scale = 1f;

    Transform _cameraTransform;
    Vector3 _previousCameraPos;

    void Start()
    {
        _backgrounds = GetComponentsInChildren<Transform>();
        _backgroundParallaxAmount = new float[_backgrounds.Length];

        Assert.IsNotNull(Camera.main);
        _cameraTransform = Camera.main.transform;
        _previousCameraPos = _cameraTransform.position;
        
        // Backgrounds should be layered from 1 to n
        // where n = amount of backgrounds.
        // We can use that for proper parallaxing
        // (item furthest away gets scaled most).
        for (int i = 0; i < _backgrounds.Length; i++)
        {
            float targetParallax = _backgrounds[i].position.z;
            
            // Scale the parallax so that targets at 1 stay at 1
            // (move same distance as camera).
            targetParallax -= 1;
            targetParallax *= scale;
            targetParallax += 1;
            
            _backgroundParallaxAmount[i] = targetParallax;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < _backgrounds.Length; i++)
        {
            float parallax = (_previousCameraPos.x - _cameraTransform.position.x) * _backgroundParallaxAmount[i];

            float bgTargetX = _backgrounds[i].position.x + parallax;
            Vector3 backgroundTargetPos = new Vector3(bgTargetX, _backgrounds[i].position.y, _backgrounds[i].position.z);

            _backgrounds[i].position = Vector3.Lerp(_backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        _previousCameraPos = _cameraTransform.position;
    }
}
