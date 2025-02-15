using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject bloodPrefab;
    [SerializeField] private GameObject bloodSpawnPoint;
    [SerializeField] private GameObject scorePrefab;

    private Animator animator;

    private CameraShake cameraShake;

    public bool gameIsPaused; 

    private Collider2D hitBox;
    private bool isDying; 

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
            isDying = true;
            StartCoroutine(FrameFreezeOnHit());
            animator.Play("Hurt"); 
        }

        if (!isDying && collision.gameObject.CompareTag("Player"))
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

        hitBox.enabled = false;
        bloodSplash.TriggerFlowSpray();
        Time.timeScale = 1;

        SoundManager.PlaySound(SoundType.BLOOD, 0.1f);
        SoundManager.PlaySound(SoundType.SCOREPOINT, 0.3f);
        GameObject scoreText = Instantiate(scorePrefab,transform.position + new Vector3(0,1,0), Quaternion.identity);
        Destroy(scoreText, 3);

        yield return new WaitForSecondsRealtime(0.01f);

    }
}
