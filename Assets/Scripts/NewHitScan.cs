using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class NewSimpleHitscanBeam : MonoBehaviour
{
    //public Camera fpsCamera;
    public Transform muzzlePoint;
    //public LineRenderer lineRenderer;
    public float range = 100f;
    public float beamDuration = 0.05f;

    public GameObject impactEffectPrefab;
    private AudioSource audioSource;
    private AudioClip shotSound;

    [Header("Sound settings")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 5f;           // do tej odległości pełna głośność
    public float maxDistance = 20f;          // dalej ciszej → zero przy Linear

    void Start()
    {
        // ładowanie Resources/ImpactEffect.prefab
        impactEffectPrefab=Resources.Load<GameObject>("ImpactEffect");
        if(impactEffectPrefab == null) Debug.LogError("Didnt find Resources/ImpactEffect");

        // ładowanie Resources/Sounds/Guns/Pistol_01_Fire
        shotSound = Resources.Load<AudioClip>("Sounds/Guns/Pistol_01_Fire");
        if(shotSound == null) Debug.LogError("Didnt find Resources/Sounds/Guns/Pistol_01_Fire");

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


        Vector3 start = muzzlePoint.position;
        Vector3 direction = muzzlePoint.forward;
        Vector3 end = start + direction * range;

        if (Physics.Raycast(start, direction, out RaycastHit hit, range))
        {
            end = hit.point;

            // tworzymy efekt trafienia
            GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);
            Destroy(impactGO, 1f);

            Target hitTarget = hit.collider.GetComponent<Target>();
            if (hitTarget != null)
            {
                hitTarget.Hit();
            }
        }
    }
    /*
    private IEnumerator ShowBeam(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(beamDuration);

        lineRenderer.enabled = false;
    }
    */
}
/*  devLog com

tu na razie chyba wszystko git, raycast i particle

*/
