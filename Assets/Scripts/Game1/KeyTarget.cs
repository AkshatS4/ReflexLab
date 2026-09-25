using UnityEngine;

public class KeyTarget : Game1TargetBase
{
    [SerializeField] private KeyTargetLetter letter;

    public KeyTargetLetter Letter => letter;

    public void SetLetter(KeyTargetLetter newLetter)
    {
        letter = newLetter;
    }

    protected override void OnHit()
    {
    }

    protected override void OnExpired()
    {
    }

    protected override void OnWrongKey()
    {
    }
}