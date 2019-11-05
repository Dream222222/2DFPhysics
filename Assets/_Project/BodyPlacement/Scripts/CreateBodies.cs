using UnityEngine;
using TF.Core;
using TF.Colliders;
using FixedPointy;

public class CreateBodies : MonoBehaviour
{
    public GameObject circleSprite;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreatePolygon();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CreateCircle();
        }
    }

    void CreatePolygon()
    {

    }

    void CreateCircle()
    {
        GameObject go = new GameObject("Circle");
        TFTransform fp = go.AddComponent<TFTransform>();
        TFCircleCollider cc = go.AddComponent<TFCircleCollider>();
        TFRigidbody rb = go.AddComponent<TFRigidbody>();

        var v3 = Input.mousePosition;
        v3.z = 0;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        
        fp.Position = new FixVec3((Fix)v3.x, (Fix)v3.y, 0);
        cc.radius = (Fix)Random.Range(0.05f, 1.2f);

        rb.bodyType = TFBodyType.Dynamic;
        rb.inertia = (Fix)Random.Range(0.0f, 1.0f);
        rb.Position = (FixVec2)fp.Position;
        rb.material = TFPhysics.instance.settings.defaultMaterial;

        fp.LocalScale = new FixVec3(cc.radius*((Fix)1.45f), cc.radius*((Fix)1.45f), 1);

        //Sprite
        GameObject cSprite = GameObject.Instantiate(circleSprite);
        cSprite.transform.SetParent(go.transform, false);
    }
}
