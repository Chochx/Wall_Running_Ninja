using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private GameObject bloodSpawnPoint;

    private Animator animator;

    private CameraShake cameraShake;

    public bool gameIsPaused; 

    private Collider2D hitBox;

    private void Start()
    {
        hitBox = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Sword"))
        {
            
            StartCoroutine(FrameFreezeOnHit());
            animator.Play("Hurt"); 
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            animator.Play("Attack");
        }
    }

    private IEnumerator FrameFreezeOnHit()
    {
        Time.timeScale = 0;

        cameraShake.TriggerShake();
        var bloodSplashObject = Instantiate(bloodPrefab, bloodSpawnPoint.transform.position, Quaternion.identity);
        bloodSplashObject.transform.SetParent(bloodSpawnPoint.transform);
        var bloodSplash = bloodSplashObject.GetComponentInChildren<BloodSprayController>();
        bloodSplash.TriggerSplashSpray();

        yield return new WaitForSecondsRealtime(0.05f);

        bloodSplash.TriggerFlowSpray();
        hitBox.enabled = false;
        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(0.01f);

    }
}
