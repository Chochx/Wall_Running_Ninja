using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private GameObject bloodSpawnPoint;

    private CameraShake cameraShake;

    public bool gameIsPaused; 

    private Collider2D hitBox;

    private void Start()
    {
        hitBox = GetComponent<Collider2D>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Sword"))
        {
            
            StartCoroutine(FrameFreezeOnHit());
        }
    }

    private IEnumerator FrameFreezeOnHit()
    {
        Time.timeScale = 0;

        cameraShake.TriggerShake();
        var bloodSplashObject = Instantiate(bloodPrefab, bloodSpawnPoint.transform.position, Quaternion.identity);
        bloodSplashObject.transform.SetParent(transform);
        var bloodSplash = bloodSplashObject.GetComponentInChildren<BloodSprayController>();
        bloodSplash.TriggerSplashSpray();
        yield return new WaitForSecondsRealtime(0.025f);
        bloodSplash.TriggerFlowSpray();
        hitBox.enabled = false;
        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(0.01f);

    }
}
