using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class NewSimpleHitscanBeam : MonoBehaviour
{
    public Transform muzzlePoint;
    //public LineRenderer lineRenderer;
    public float range = 100f;
    public float beamDuration = 0.05f;

    public GameObject impactEffectPrefab;
    public GameObject muzzleFlashPrefab;
    private AudioSource audioSource;
    private AudioClip shotSound;

    [Header("Sound settings")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 5f;           // do tej odległości pełna głośność
    public float maxDistance = 20f;          // dalej ciszej → zero przy Linear

    void Start()
    {
        // ładowanie ImpactEffect.prefab
        string impactEffectPath = "Particle effects/ImpactEffect";
        impactEffectPrefab = Resources.Load<GameObject>(impactEffectPath);
        if(impactEffectPrefab == null) Debug.LogError("Didnt find Resources/"+impactEffectPath);

        // ładowanie Pistol_01_Fire.wav
        string gunShot01Path = "Sounds/Guns/Pistol_01_Fire";
        shotSound = Resources.Load<AudioClip>(gunShot01Path);
        if(shotSound == null) Debug.LogError("Didnt find Resources/"+gunShot01Path);

        // ładowanie MuzzleFlash.prefab
        string muzzleFlashPath = "Particle effects/MuzzleFlash";
        muzzleFlashPrefab = Resources.Load<GameObject>(muzzleFlashPath);
        if(muzzleFlashPrefab == null) Debug.LogError("Didnt find Resources/"+muzzleFlashPath);

        audioSource = muzzlePoint.GetComponent<AudioSource>();
        if(audioSource == null)
        {
            audioSource = muzzlePoint.gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;
        
        // dźwięk strzału
        if(shotSound!=null) audioSource.PlayOneShot(shotSound);

        // efekt muzzle flash
        GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
        Destroy(flash, 0.5f);

        // strzał raycast
        Vector3 start = muzzlePoint.position;
        Vector3 direction = muzzlePoint.forward;
        Vector3 end = start + direction * range;

        if (Physics.Raycast(start, direction, out RaycastHit hit, range))
        {
            end = hit.point;

            // tworzymy efekt trafienia
            GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);
            Destroy(impactGO, 1f);

            RagdollActivator hitTarget1 = hit.collider.GetComponent<RagdollActivator>();
            if (hitTarget1 != null)
            {
                hitTarget1.Hit();
            }

            HitboxTest test = hit.collider.GetComponent<HitboxTest>();
            if (test != null)
            {
                test.Hit();
            }
        }
    }
}
