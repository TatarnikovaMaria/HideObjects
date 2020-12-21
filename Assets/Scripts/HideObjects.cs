using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideObjects : MonoBehaviour
{
    public Transform target;
    public LayerMask hideLayer;
    [Range(2, 3)]
    public float volume = 2.24f;

    private float distance = 0f;
    private Camera cam;
    private Vector3[] ClipPoints;

    public void Start()
    {
        StartCoroutine(ShowObjects());
        cam = GetComponent<Camera>();
    }
    void UpdateClipPoints(Quaternion rot, Vector3 pos)
    {
        ClipPoints = new Vector3[5];
        float z = cam.nearClipPlane;
        float x = Mathf.Tan(cam.fieldOfView / volume) * z;
        float y = x / cam.aspect;
        ClipPoints[0] = (rot * new Vector3(-x, y, z)) + pos;
        ClipPoints[1] = (rot * new Vector3(x, y, z)) + pos;
        ClipPoints[2] = (rot * new Vector3(-x, -y, z)) + pos;
        ClipPoints[3] = (rot * new Vector3(x, -y, z)) + pos;
        ClipPoints[4] = pos - transform.forward;
        distance = Vector3.Distance(ClipPoints[4], target.position + offset);
    }

    public Dictionary<Collider, HiddenOb> Hidden = new Dictionary<Collider, HiddenOb>();
    public Vector3 offset = Vector3.up;
    void Update()
    {
        UpdateClipPoints(transform.rotation, transform.position);
        foreach (Vector3 v3 in ClipPoints)
        {
            RaycastHit[] hits;
            Vector3 p = target.position + offset;
            hits = Physics.RaycastAll(p, v3 - p, distance);
            foreach (var hit in hits)
            {
                if (hideLayer == (hideLayer | (1 << hit.transform.gameObject.layer)))
                {
                    HiddenOb current;
                    if (Hidden.TryGetValue(hit.collider, out current))
                    {
                        current.ResetTicks(10);
                    }
                    else
                    {
                        current = new HiddenOb(10, hit.collider.GetComponent<Renderer>());
                        Hidden.Add(hit.collider, current);
                    }
                }
            }
        }
    }

    List<Collider> Remove = new List<Collider>();
    IEnumerator ShowObjects()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.10f);
            foreach (var item in Hidden.Keys)
            {
                if (Hidden[item].AddTick())
                    Remove.Add(item);
            }
            lock (Hidden)
            {
                foreach (var item in Remove)
                {
                    Hidden.Remove(item);
                }
            }
            Remove = new List<Collider>();
        }
    }

    public class HiddenOb
    {
        private int TicksToShow;
        private Renderer renderer;
        private float oldTransparency;

        public HiddenOb(int TicksToShow, Renderer renderer)
        {
            this.TicksToShow = TicksToShow;
            this.renderer = renderer;
            oldTransparency = this.renderer.material.color.a;

            Color newColor = this.renderer.material.color;
            newColor.a = 0.5f;
            renderer.material.SetColor("_BaseColor", newColor);
        }
        public void ResetTicks(int value)
        {
            TicksToShow = value;
        }
        public bool AddTick()
        {
            TicksToShow -= 1;
            if (TicksToShow <= 0)
            {
                Color oldColor = this.renderer.material.color;
                oldColor.a = oldTransparency;
                renderer.material.SetColor("_BaseColor", oldColor);
                return true;
            }
            return false;
        }
    }
}
