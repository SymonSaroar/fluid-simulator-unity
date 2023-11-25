using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Interpolate : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 boundSize = new Vector2(18f, 9.5f);
    Vector2 _halfBoundsSize;
    public GameObject coloredDot;
    public GameObject particle;
    public float spacing = 0.1f;
    public float gravity = 0f;
    public float dampingFactor = 1f;
    public float particleSize = 1f;
    public int numParticles;
    public float smoothingRadius = 1.0f;
    public float targetDensity;
    public float pressureMultiplier;
    
    const float Mass = 1;
    private Vector2[] positions;
    private Vector2[] velocities;
    private float[] particleProperties;
    private float[] densities;
    private bool setup = true;

    private void Awake()
    {
        CreateParticle(99);
        foreach (var pos in positions)
        {
            Instantiate(particle, pos, Quaternion.identity);
        }
    }

    void Start()
    {
        _halfBoundsSize = boundSize / 2f;
        print(_halfBoundsSize);
        
        CreateParticle(99);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var pos in positions)
        {
            Instantiate(particle, pos, Quaternion.identity);
        }
        
        SimulationStep(Time.deltaTime);
    }
    void SimulationStep(float dt)
    {
        for (int i = 0; i < numParticles; i++) { 
            velocities[i] += Vector2.down * (gravity * dt);
            densities[i] = CalculateDensity(positions[i]);
        }
        // Apply Pressure Forces
        for (int i = 0; i < numParticles; i++)
        {
            Vector2 pressureForce = CalculatePressureForce(i);
            Vector2 pressureAcceleration = pressureForce / densities[i];
            velocities[i] += pressureAcceleration * dt;
        }
        for (int i = 0; i < numParticles; i++)
        {
            positions[i] += velocities[i] * dt;
            ResolveCollisions(ref positions[i], ref velocities[i]);
        }

    }
    void ResolveCollisions(ref Vector2 position, ref Vector2 velocity)
    {

        if (Mathf.Abs(position.x) > _halfBoundsSize.x)
        {
            position.x = _halfBoundsSize.x * Mathf.Sign(position.x);
            velocity.x *= -1 * dampingFactor;
        }
        if (Mathf.Abs(position.y) > _halfBoundsSize.y)
        {
            position.y = _halfBoundsSize.y * Mathf.Sign(position.y);
            velocity.y *= -1 * dampingFactor;
        }
    }
    float ExampleFunction(Vector2 pos)
    {
        return Mathf.Cos(pos.y + 1 - Mathf.Sin(pos.x));
    }
    float SmoothingKernel(float radius, float dst)
    {
        if(dst >= radius) return 0;
        float volume = Mathf.PI * Mathf.Pow(radius, 5) / 10;
        float val = radius - dst;
        return val * val * val / volume;
    }
    float SmoothingKernelDerivative(float radius, float dst)
    {
        if (dst >= radius) return 0;
        float val = (radius - dst) * (radius - dst);
        float coeff = -30 / (Mathf.PI * Mathf.Pow(radius, 5)); 
        return val * coeff;
    }
    float SmoothingKernelSpike(float radius, float dst)
    {
        if (dst >= radius) return 0;
        float volume = Mathf.PI * Mathf.Pow(radius, 7) / 21;
        float val = radius - dst;
        return val * val * val * val * val / volume;
    }

    float SmoothingKernelSpikeDerivative(float radius, float dst)
    {
        if (dst >= radius) return 0;
        float val = Mathf.Pow(radius - dst, 4);
        float coeff = -105 / (Mathf.PI * Mathf.Pow(radius, 7));
        return val * coeff;
    }
    
    void CreateParticle(int seed)
    {
        Random.InitState(seed);
        float oldSize = particle.GetComponent<SpriteRenderer>().bounds.size.x;
        particle.transform.localScale = Vector3.one * (particleSize / (oldSize == 0? 0.04f: oldSize));
        positions = new Vector2[numParticles];
        particleProperties = new float[numParticles];
        densities = new float[numParticles];
        velocities = new Vector2[numParticles];

        for (int i = 0; i < numParticles; i++)
        {
            float x = (Random.value - 0.5f) * boundSize.x;
            float y = (Random.value - 0.5f) * boundSize.y;
            positions[i] = new Vector2(x, y);
            particleProperties[i] = ExampleFunction(positions[i]);
            densities[i] = CalculateDensity(positions[i]);
        }
    }
    float CalculateDensity(Vector2 point)
    {
        float density = 0;
        const float mass = 1;

        foreach (Vector2 position in positions)
        {
            float dst = (position - point).magnitude;
            if (dst > smoothingRadius) continue;
            float influence = SmoothingKernelSpike(smoothingRadius, dst);
            density += mass * influence;
        }
        return density;
    }


    Vector2 CalculatePressureForce(int id)
    {
        Vector2 propertyPressure = Vector2.zero;
        for (int i = 0; i < numParticles; i++)
        {
            float dst = (positions[i] - positions[id]).magnitude;
            Vector2 dir = (dst == 0)? GetRandomDir() : (positions[i] - positions[id]) / dst;
            if (Mathf.Abs(dst) > smoothingRadius) continue;
            float slope = SmoothingKernelSpikeDerivative(smoothingRadius, dst);
            float sharedPressure = CalculateSharedPressureForce(densities[i], densities[id]);
            propertyPressure += -dir * (Mass * slope * sharedPressure) / densities[i];
        }

        return propertyPressure;
    }

    float ConvertDensityToPressure(float density)
    {
        float deltaDensity = density - targetDensity;
        float pressure = -deltaDensity * pressureMultiplier;
        return pressure;
    }
    float CalculateSharedPressureForce(float density1, float density2)
    {
        return (ConvertDensityToPressure(density1) + ConvertDensityToPressure(density2)) / 2f;
    }
    Vector2 GetRandomDir()
    {
        return Random.insideUnitCircle.normalized;
    }
}
