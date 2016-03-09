using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class InvertColliderBox : MonoBehaviour {

    public bool isTrigger = false;
    public float size = 0.5f;

    private Collider2D myCollider;

	public void InvertCollider() {
        myCollider = GetComponent<Collider2D>();
        myCollider.enabled = true;
        myCollider.isTrigger = isTrigger;
        Bounds b = myCollider.bounds;
        Vector2 offset = myCollider.offset;
        myCollider.enabled = false;

        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach(Transform t in children)
        {
            int index = t.name.LastIndexOf('_');
            if (index >= 0 && t.name.Substring(0, index) == "bounds")
                DestroyImmediate(t.gameObject);
        }
        CreateBound("N", new Vector2(0, b.size.y/2 + size/2) + offset,
            new Vector2(b.size.x, size));
        CreateBound("S", new Vector2(0, -b.size.y / 2 - size / 2) + offset,
            new Vector2(b.size.x, size));

        CreateBound("E", new Vector2(b.size.x / 2 + size / 2, 0) + offset,
            new Vector2(size, b.size.y + 2 * size));
        CreateBound("W", new Vector2(-b.size.x / 2 - size / 2, 0) + offset,
            new Vector2(size, b.size.y + 2 * size));

    }

    GameObject CreateBound(string mod, Vector2 pos, Vector2 bounds) {
        GameObject n = new GameObject();
        n.layer = this.gameObject.layer;
        n.name = "bounds_" + mod;
        n.transform.parent = this.transform;
        n.transform.localPosition = pos;
        BoxCollider2D b = n.AddComponent<BoxCollider2D>();
        b.size = bounds;
        return n;
    }
}
