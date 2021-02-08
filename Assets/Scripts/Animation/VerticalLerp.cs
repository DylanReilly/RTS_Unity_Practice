using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalLerp : MonoBehaviour
{
    [SerializeField] GameObject model = null;
    [SerializeField] float min = -0.2f;
    [SerializeField] float max = 0.2f;
    float start = 0.0f;
    float startY = 0.0f;
    bool hasStarted = false;

    void Update()
    {
        if (hasStarted == false)
        {
            startY = model.transform.position.y;
            hasStarted = true;
        }

        model.transform.position = 
            new Vector3(model.transform.position.x,
            startY + Mathf.Lerp(min, max, start),
            model.transform.position.z);

        start += 0.5f * Time.deltaTime;

        if (start > 1.0f)
        {
            float temp = max;
            max = min;
            min = temp;
            start = 0.0f;
        }

    }
}
