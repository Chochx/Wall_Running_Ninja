using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private GameObject bloodSpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var bloodSplashObject = Instantiate(bloodPrefab, bloodSpawnPoint.transform.position, Quaternion.identity);
            bloodSplashObject.transform.SetParent(transform);

            var bloodSplash = bloodSplashObject.GetComponentInChildren<BloodSprayController>();
            bloodSplash.TriggerSplashSpray();
        }
    }
}
