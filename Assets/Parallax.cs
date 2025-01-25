
using UnityEngine;

[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
    private LevelManager levelManager;
    private PlayerController playerController;

    float parallaxScrollSpeed;
    [SerializeField] private float speedDivider;

    private void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        playerController = FindFirstObjectByType<PlayerController>();
        parallaxScrollSpeed = levelManager.scrollSpeed * speedDivider;
    }

    private void Update()
    {
        if(playerController.isAlive && playerController.playerHasLanded)
        {
            transform.position -= new Vector3(parallaxScrollSpeed * Time.deltaTime, 0, 0);
        }
    }
}
