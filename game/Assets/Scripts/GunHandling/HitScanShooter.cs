using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SimpleHitscanBeam : MonoBehaviour
{
    public Camera fpsCamera;
    public LineRenderer lineRenderer;
    public float range = 100f;
    public float beamDuration = 0.05f;
    public GameObject impactEffectPrefab;

    void Update()
    {
        // ładowanie Resources/ImpactEffect.prefab
        impactEffectPrefab=Resources.Load<GameObject>("ImpactEffect");
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (ExitMenu.Instance != null && ExitMenu.Instance.inputBlocked) return;
        
        Vector3 start = fpsCamera.transform.position;
        Vector3 direction = fpsCamera.transform.forward;
        Vector3 end = start + direction * range;

        if (Physics.Raycast(start, direction, out RaycastHit hit, range))
        {
            end = hit.point;
        }

        //StartCoroutine(ShowBeam(start, end));

        // tworzymy efekt trafienia
        GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);
        Destroy(impactGO, 1f);

        Target hitTarget = hit.collider.GetComponent<Target>();
        if (hitTarget != null)
        {
            hitTarget.Hit();
        }

    }

    private IEnumerator ShowBeam(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(beamDuration);

        lineRenderer.enabled = false;
    }
}
/*  devLog com

tu na razie chyba wszystko git, raycast i particle

*/
