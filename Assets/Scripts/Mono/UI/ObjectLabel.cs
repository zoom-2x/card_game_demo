using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLabel : MonoBehaviour
{
    Camera scene_camera = null;
    GameObject screen_ui = null;
    RectTransform region_label = null; 
    Vector3 label_offset = new Vector3(0, 0, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        screen_ui = GameObject.Find("screen_ui");
        scene_camera = GameObject.Find("MAIN_CAMERA").GetComponent<Camera>();
        region_label = screen_ui.transform.Find("region_label").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 sp = scene_camera.WorldToScreenPoint(transform.position + label_offset);
        region_label.transform.position = sp;
        // Debug.Log(region_label.transform.position);
    }
}
