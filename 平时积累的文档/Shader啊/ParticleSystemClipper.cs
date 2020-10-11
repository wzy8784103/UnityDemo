using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemClipper : MonoBehaviour {

    // Use this for initialization
    private UIRoot root;
    private UIPanel panel;
    private Renderer renderer;
    private Material material;
	void Start () {
        root = GetComponentInParent<UIRoot>();
        panel = GetComponentInParent<UIPanel>();
        renderer = GetComponent<Renderer>();
        if (renderer != null) material = renderer.material;
	}
	
	// Update is called once per frame
	void Update () {

	}

    void OnWillRenderObject()
    {
        if(renderer == null) return;
		if(panel == null) return;
        Clip();
    }

    private void Clip()
    {
        Vector4 clipArea = CalcClipArea();
     //   Material mat = renderer.material;
        material.SetVector("_Area", clipArea);
    }

    private Vector4 CalcClipArea()
    {
        var clipRegion = panel.finalClipRegion;
        Vector4 nguiArea = new Vector4()
        {
            x = clipRegion.x - clipRegion.z / 2,
            y = clipRegion.y - clipRegion.w / 2,
            z = clipRegion.x + clipRegion.z / 2,
            w = clipRegion.y + clipRegion.w / 2
        };
        float h = 2;
#if ART_PRO
        float temp = h / 720f;
#else
        float temp = h / root.manualHeight;
#endif
        //      Vector3 pos = panel.transform.position - root.transform.position;
        Vector3 pos = panel.transform.position;

        return new Vector4()
        {
       /*     x = (pos.x + nguiArea.x) * temp,
            y = (pos.y + nguiArea.y) * temp,
            z = (pos.x + nguiArea.z) * temp,
            w = (pos.y + nguiArea.w) * temp */
            x = pos.x + nguiArea.x * temp,
            y = pos.y + nguiArea.y * temp,
            z = pos.x + nguiArea.z * temp,
            w = pos.y + nguiArea.w * temp
        };
    }

    void OnDestroy()
    {

    }

}
