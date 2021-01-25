
using System;
using System.IO;
using UnityEngine;

public class ScreenShotCtrl : MonoBehaviour
{
    private int resolution = 640;
    private float cameraDistance = -10f;
    public GameObject target;
    private RenderTexture renderTexture;
    private Camera renderCamera;
    private Vector4 bounds;

    public string Shoot()
    {
        Debug.Log((object)"Initializing camera and stuff...");
        this.gameObject.AddComponent(typeof(Camera));
        this.renderCamera = this.GetComponent<Camera>();
        this.renderCamera.enabled = true;
        this.renderCamera.cameraType = CameraType.Game;
        this.renderCamera.forceIntoRenderTexture = true;
        this.renderCamera.orthographic = true;
        this.renderCamera.orthographicSize = 5f;
        this.renderCamera.aspect = 1f;
        this.renderCamera.targetDisplay = 2;
        this.renderTexture = new RenderTexture(this.resolution, this.resolution, 24);
        this.renderCamera.targetTexture = this.renderTexture;
        this.bounds = new Vector4();
        Debug.Log((object)"Initialized successfully!");
        Debug.Log((object)"Computing level boundaries...");
        if ((UnityEngine.Object)this.target != (UnityEngine.Object)null)
        {
            Bounds bounds;
            if ((UnityEngine.Object)this.target.GetComponentInChildren<Renderer>() != (UnityEngine.Object)null)
                bounds = this.target.GetComponentInChildren<Renderer>().bounds;
            else if ((UnityEngine.Object)this.target.GetComponentInChildren<Collider2D>() != (UnityEngine.Object)null)
            {
                bounds = this.target.GetComponentInChildren<Collider2D>().bounds;
            }
            else
            {
                Debug.Log((object)"Unfortunately no boundaries could be found :/");
                return string.Empty;
            }
            this.bounds.w = bounds.min.x;
            this.bounds.x = bounds.max.x;
            this.bounds.y = bounds.min.y;
            this.bounds.z = bounds.max.y;
        }
        else
        {
            foreach (GameObject gameObject in (object[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                Bounds bounds = new Bounds();
                if ((UnityEngine.Object)gameObject.GetComponentInChildren<Renderer>() != (UnityEngine.Object)null)
                    bounds = gameObject.GetComponentInChildren<Renderer>().bounds;
                else if ((UnityEngine.Object)gameObject.GetComponentInChildren<Collider2D>() != (UnityEngine.Object)null)
                    bounds = gameObject.GetComponentInChildren<Collider2D>().bounds;
                else
                    continue;
                this.bounds.w = Mathf.Min(this.bounds.w, bounds.min.x);
                this.bounds.x = Mathf.Max(this.bounds.x, bounds.max.x);
                this.bounds.y = Mathf.Min(this.bounds.y, bounds.min.y);
                this.bounds.z = Mathf.Max(this.bounds.z, bounds.max.y);
            }
        }
        Debug.Log((object)("Boundaries computed successfuly! The computed boundaries are " + (object)this.bounds));
        Debug.Log((object)"Computing target image resolution and final setup...");
        Texture2D tex = new Texture2D(Mathf.RoundToInt((float)this.resolution * (float)(((double)this.bounds.x - (double)this.bounds.w) / ((double)this.renderCamera.aspect * (double)this.renderCamera.orthographicSize * 2.0 * (double)this.renderCamera.aspect))), Mathf.RoundToInt((float)this.resolution * (float)(((double)this.bounds.z - (double)this.bounds.y) / ((double)this.renderCamera.aspect * (double)this.renderCamera.orthographicSize * 2.0 / (double)this.renderCamera.aspect))), TextureFormat.RGB24, false);
        RenderTexture.active = this.renderTexture;
        Debug.Log((object)"Success! Everything seems ready to render!");
        float w = this.bounds.w;
        float num1 = 0.0f;
        while ((double)w < (double)this.bounds.x)
        {
            float y = this.bounds.y;
            float num2 = 0.0f;
            while ((double)y < (double)this.bounds.z)
            {
                this.gameObject.transform.position = new Vector3(w + this.renderCamera.aspect * this.renderCamera.orthographicSize, y + this.renderCamera.aspect * this.renderCamera.orthographicSize, this.cameraDistance);
                if ((double)this.gameObject.transform.position.y > 10.0)
                    this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, 9.4f, this.gameObject.transform.position.z);
                Debug.Log((object)("positioned " + (object)this.gameObject.transform.position.x + ":" + (object)this.gameObject.transform.position.y));
                this.renderCamera.Render();
                tex.ReadPixels(new Rect(0.0f, 0.0f, (float)this.resolution, (float)this.resolution), (int)num1 * this.resolution, (int)num2 * this.resolution);
                y += (float)((double)this.renderCamera.aspect * (double)this.renderCamera.orthographicSize * 2.0);
                ++num2;
            }
            w += (float)((double)this.renderCamera.aspect * (double)this.renderCamera.orthographicSize * 2.0);
            ++num1;
        }
        RenderTexture.active = (RenderTexture)null;
        this.renderCamera.targetTexture = (RenderTexture)null;
        byte[] png = tex.EncodeToPNG();
        string.Format("C:\\Temp\\Images\\BNR Screenshot.png", (object[])Array.Empty<object>());
        string path = Path.Combine(Application.persistentDataPath, "BNR Screenshot.png");
        File.WriteAllBytes(path, png);
        return path;
    }
}
