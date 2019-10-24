using UnityEngine;
using TDFP.Core;
using TDFP.Colliders;
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
        TDFPTransform fp = go.AddComponent<TDFPTransform>();
        TFPCircleCollider cc = go.AddComponent<TFPCircleCollider>();
        FPRigidbody rb = go.AddComponent<FPRigidbody>();
        cc.radius = (Fix)Random.Range(0.05f, 1.2f);
        rb.inertia = (Fix)Random.Range(0.0f, 1.0f);

        var v3 = Input.mousePosition;
        v3.z = 0;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        fp.Position = new FixVec3((Fix)v3.x, (Fix)v3.y, 0);
        rb.Position = (FixVec2)fp.Position;
        fp.Scale = new FixVec3(cc.radius*((Fix)1.45f), cc.radius*((Fix)1.45f), 1);

        //Sprite
        GameObject cSprite = GameObject.Instantiate(circleSprite);
        cSprite.transform.SetParent(go.transform, false);

        rb.material = TDFPhysics.instance.settings.defaultMaterial;
    }
}
