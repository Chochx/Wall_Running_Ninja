using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBackground : MonoBehaviour
{
    [SerializeField] private float resetPointX;
    private Vector2 originalPosX;
    // Start is called before the first frame update
    void Start()
    {
        originalPosX = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x <= resetPointX)
        {
            transform.position = originalPosX;
        }
    }
}
