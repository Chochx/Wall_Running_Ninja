using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeath : MonoBehaviour
{
    public GameObject bloodPrefab;

    private void OnDestroy()
    {
        Instantiate(bloodPrefab, transform.position, Quaternion.identity);
    }
}
