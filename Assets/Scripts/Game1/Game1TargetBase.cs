using System.Collections;
using UnityEngine;

public abstract class Game1TargetBase : MonoBehaviour
{
    protected Game1Controller controller;

    public float SpawnTime { get; private set; }

    private bool initialized;
    private bool resolved;

    public void Initialize(Game1Controller owner, float lifetime, bool shrink)
    {
        controller = owner;
        SpawnTime = Time.time;
        initialized = true;

        transform.localScale = Vector3.one;

        StartCoroutine(LifetimeRoutine(lifetime, shrink));
    }

    private IEnumerator LifetimeRoutine(float lifetime, bool shrink)
    {
        float elapsed = 0f;

        while (elapsed < lifetime && !resolved)
        {
            elapsed += Time.deltaTime;

            if (shrink)
            {
                float normalized = Mathf.Clamp01(elapsed / lifetime);

                float scale = Mathf.Lerp(1f, 0.08f, normalized);

                transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (!resolved)
        {
            resolved = true;

            OnExpired();

            if (controller != null)
            {
                controller.TargetExpired(this);
            }
        }
    }

    public void ResolveHit()
    {
        if (!initialized || resolved)
        {
            return;
        }

        resolved = true;

        OnHit();

        if (controller != null)
        {
            controller.TargetHit(this);
        }
    }

    public void ResolveWrongKey()
    {
        if (!initialized || resolved)
        {
            return;
        }

        resolved = true;

        OnWrongKey();

        if (controller != null)
        {
            controller.TargetWrongKey(this);
        }
    }

    protected abstract void OnHit();

    protected abstract void OnExpired();

    protected abstract void OnWrongKey();
}