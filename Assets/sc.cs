using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class sc : MonoBehaviour
{
    public GameObject circle;
    public float gravity;
    [Range(0f, 2f)]
    public float dampingFactor = 1;
    public Vector2[] positions;
    public Vector2[] velocities;
    private GameObject obj;
    public float particleSize = 0.1f;
    private Vector2 boundsSize = new Vector2(18f, 9.5f);
    Vector2 halfBoundsSize;
    public int numParticles = 125;
    public float particleSpacing = 0.15f;
    public float smoothingRadius = 1;
    // Start is called before the first frame update
    void Start()
    {
        halfBoundsSize = boundsSize / 2 - Vector2.one * particleSize;
        positions = new Vector2[numParticles];
        velocities = new Vector2[numParticles];

        int particlesPerRow = (int)Mathf.Sqrt(numParticles);
        int particlesPerColumn = (numParticles - 1) / (particlesPerRow + 1);
        float spacing = particleSize * 2 + particleSpacing;

        for (int i = 0; i < numParticles; i++)
        {
            // float x = (i % particlesPerRow - particlesPerRow / 2f + 0.5f) * spacing;
            // float y = (i / particlesPerRow - particlesPerColumn / 2f + 0.5f) * spacing;
            positions[i] = new Vector2(Random.Range(-halfBoundsSize.x, halfBoundsSize.x), Random.Range(-halfBoundsSize.y, halfBoundsSize.y));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        for (int i = 0; i < positions.Length; i++)
        {
            velocities[i] += Vector2.down * gravity * Time.deltaTime;   
            positions[i] += velocities[i] * Time.deltaTime;
            ResolveCollisions(ref positions[i], ref velocities[i]);


            DrawCircle(positions[i]);
        }

    }

    void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
    {
        
        if (Mathf.Abs(position.x) > halfBoundsSize.x)
        {
            position.x = halfBoundsSize.x * Mathf.Sign(position.x);
            velocity.x *= -1 * dampingFactor;
        }
        if (Mathf.Abs(position.y) > halfBoundsSize.y)
        {
            position.y = halfBoundsSize.y * Mathf.Sign(position.y);
            velocity.y *= -1 * dampingFactor;
        }
    }

    void DrawCircle(Vector3 position)
    {
        obj = Instantiate(circle, position, Quaternion.identity);
        obj.GetComponent<NewBehaviourScript>().updateSize(particleSize);
    }
    void DrawCircle(Vector3 position, Quaternion rotation)
    {
        obj = Instantiate(circle, position, rotation);
    }
    void DrawCircle(Vector3 position, Quaternion rotation, Vector3 ground)
    {
        obj = Instantiate(circle, position, rotation);
    }

    float SmoothingKernel(float radius, float dst)
    {
        float volume = Mathf.PI * Mathf.Pow(radius, 5) / 10;
        float val = Mathf.Max(0, radius - dst);
        return val * val * val / volume;
    }

    float CalculateDensity(Vector2 point)
    {
        float density = 0;
        const float mass = 1;

        foreach (Vector2 position in positions)
        {
            float dst = (position - point).magnitude;
            float influence = SmoothingKernel(smoothingRadius, dst);
            density += mass * influence;
        }
        return density;
    }
}

