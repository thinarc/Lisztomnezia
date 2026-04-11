using System;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class UITilt : MonoBehaviour
{
    public RectTransform target;
    public float tilt = 10f;
    public float tiltRot = 10f;

    public bool chto;
    
    public RawImage[] chtoRaw;
    public Vector2 chtoDir;
    public float chtoSpeed;

  public void Start()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }

        r = target.localRotation;
        p = target.localPosition;

        if (chto) tilt *= 4f;
    }

    private Quaternion r;
    private Vector3 p;

    void Update()
    {
        if (chtoRaw.Length == 0) return;
        
        Rect raw = chtoRaw[0].uvRect;
        raw.position = new Vector2(chtoDir.x * chtoSpeed * Time.time, chtoDir.y * chtoSpeed * Time.time);
        chtoRaw[0].uvRect = raw;
        
        Rect raw2 = chtoRaw[1].uvRect;
        raw2.position = new Vector2(chtoDir.x * chtoSpeed * Time.time, chtoDir.y * chtoSpeed * Time.time);
        chtoRaw[1].uvRect = raw2;
        
        Rect raw3 = chtoRaw[2].uvRect;
        raw3.position = new Vector2(chtoDir.x * chtoSpeed * Time.time, chtoDir.y * chtoSpeed * Time.time);
        chtoRaw[2].uvRect = raw3;
        
        Rect raw4 = chtoRaw[3].uvRect;
        raw4.position = new Vector2(chtoDir.x * chtoSpeed * Time.time, chtoDir.y * chtoSpeed * Time.time);
        chtoRaw[3].uvRect = raw4;
        
        // if (target == null) Start();
        
        Vector2 mouse = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 dir = (mouse - center) / Screen.width;

        float rotX =  dir.y * tilt;
        float rotY = -dir.x * tilt;
        
        float rotX2 =  dir.y * tiltRot;
        float rotY2 = -dir.x * tiltRot;

        if (!chto)
        {
            target.localRotation = Quaternion.Euler(rotX2, rotY2, 0f);
            target.localPosition = new Vector3(rotY, -rotX, 0f);
        }
        else
        {
            target.localRotation = r * Quaternion.Euler(rotX2, rotY2, 0f);
            target.localPosition = p + new Vector3(rotY, -rotX, 0f);
        }
    }
}