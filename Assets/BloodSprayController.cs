using UnityEngine;
using UnityEngine.VFX;

[System.Serializable] // This makes the class show up in the Inspector
public class BloodSpraySettings
{
    [Header("Spray Configuration")]
    public Vector3 bloodVelocity = new(25,19,0);

    [Header("Droplet Properties")]
    [Tooltip("Minimum droplet size")]
    [Range(0.01f, 10f)]
    public float minDropletSize = 1f;
    [Tooltip("Maximum droplet size")]
    [Range(0.01f, 10f)]
    public float maxDropletSize = 3;
    [Range(10, 1000)]
    public int dropletCount = 33;
    public bool loop = false;

    [Header("Visual Properties")]
    public Color bloodColor = Color.red;
}

public class BloodSprayController : MonoBehaviour
{
    [SerializeField] private VisualEffect vfxGraph;

    [Header("Blood Spray Configurations")]
    [SerializeField] private BloodSpraySettings flowSettings;
    [SerializeField] private BloodSpraySettings splashSettings;

    private void Awake()
    {
        if (vfxGraph == null)
        {
            vfxGraph = GetComponent<VisualEffect>();
            if (vfxGraph == null)
            {
                Debug.LogError("No VisualEffect component found on " + gameObject.name);
            }
        }
    }

    // Trigger specific spray types
    public void TriggerFlowSpray()
    {
        TriggerSpray(flowSettings);
    }

    public void TriggerSplashSpray()
    {
        TriggerSpray(splashSettings);
    }

    // Generic spray method
    public void TriggerSpray(BloodSpraySettings settings)
    {
        ApplySettings(settings);
        vfxGraph.Play();
    }

    private void ApplySettings(BloodSpraySettings settings)
    {
        vfxGraph.SetVector3("BloodVelocity", settings.bloodVelocity);
        vfxGraph.SetBool("Loop", settings.loop);
        vfxGraph.SetVector2("RandomSize", new Vector2(settings.minDropletSize, settings.maxDropletSize));
        vfxGraph.SetFloat("Rate", settings.dropletCount);
        vfxGraph.SetVector4("BloodColor", new Vector4(
            settings.bloodColor.r,
            settings.bloodColor.g,
            settings.bloodColor.b,
            settings.bloodColor.a
        ));
    }
}