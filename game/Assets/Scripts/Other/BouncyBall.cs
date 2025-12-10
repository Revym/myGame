using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BouncyBall : MonoBehaviour
{
    [Header("Ustawienia Energii")]
    [Tooltip("Ile procent prędkości tracimy przy każdym odbiciu (0.1 = 10%)")]
    [Range(0f, 1f)]
    public float energyLoss = 0.1f;

    [Tooltip("Minimalna prędkość, poniżej której piłka przestaje się odbijać (zapobiega drganiom na podłodze)")]
    public float minVelocityToBounce = 0.5f;

    private Rigidbody rb;
    private Vector3 lastFrameVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Używamy FixedUpdate, aby śledzić prędkość z fizyczną precyzją
    void FixedUpdate()
    {
        // Zapamiętujemy prędkość z klatki przed kolizją
        lastFrameVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. Obliczamy prędkość uderzenia (magnitude)
        float speed = lastFrameVelocity.magnitude;

        // Jeśli prędkość jest zbyt mała, pozwalamy fizyce Unity zakończyć ruch (tarcie)
        if (speed < minVelocityToBounce) return;

        // 2. Obliczamy kierunek odbicia
        // Bierzemy normalną (kierunek prostopadły) powierzchni, w którą uderzyliśmy
        Vector3 contactNormal = collision.contacts[0].normal;
        
        // Funkcja Reflect oblicza idealne odbicie sprężyste
        Vector3 reflectedDirection = Vector3.Reflect(lastFrameVelocity.normalized, contactNormal);

        // 3. Aplikujemy stratę energii
        // Nowa prędkość to stara prędkość pomniejszona o ustalony procent
        float newSpeed = speed * (1f - energyLoss);

        // Nadpisujemy prędkość Rigidbody
        rb.linearVelocity = reflectedDirection * newSpeed;
    }
}