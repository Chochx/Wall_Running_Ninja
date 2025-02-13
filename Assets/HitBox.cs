using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Collider2D hitCollider;
    // Start is called before the first frame update
    void Start()
    {
        hitCollider = GetComponent<Collider2D>();
    }
}
