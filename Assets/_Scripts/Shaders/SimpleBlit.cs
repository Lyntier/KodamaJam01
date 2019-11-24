using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleBlit : MonoBehaviour
{
    [SerializeField] Material transitionMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if(transitionMaterial != null) Graphics.Blit(src, dst, transitionMaterial);
    }
}
