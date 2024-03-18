using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LabelsUI : MonoBehaviour
{
    UIDocument ui_document = null;
    VisualElement container = null;

    Camera scene_camera = null;
    CameraMovement camera_movement = null;

    void Awake()
    {
        ui_document = GetComponent<UIDocument>();
        container = ui_document.rootVisualElement;

        GameObject camera_obj = GameObject.Find("MAIN_CAMERA");
        scene_camera = camera_obj.GetComponent<Camera>();
        camera_movement = camera_obj.GetComponent<CameraMovement>();
    }

    void Update()
    {}
}
