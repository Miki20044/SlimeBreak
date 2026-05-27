using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager instance;

    public enum Difficulty { Normal, Hard, Hell }
    public Difficulty current = Difficulty.Normal;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetProjectileMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 1.5f;
            case Difficulty.Hell: return 2f;
            default: return 1f;
        }
    }

    public float GetStaminaMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 1.25f;
            case Difficulty.Hell: return 3f;
            default: return 1f;
        }
    }

    public float GetBossHPMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 1.5f;
            case Difficulty.Hell: return 3f;
            default: return 1f;
        }
    }

    public float GetPlayerSpeedMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 0.8f;
            case Difficulty.Hell: return 0.75f;
            default: return 1f;
        }
    }

    public float GetReactionTimeMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hell: return 0.6f;
            default: return 1f;
        }
    }

    public float GetDashLengthMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 0.8f;
            case Difficulty.Hell: return 0.6f;
            default: return 1f;
        }
    }

    public float GetDashCooldownMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 1.25f;
            case Difficulty.Hell: return 1.5f;
            default: return 1f;
        }
    }

    public int GetQTEFailLimit()
    {
        switch (current)
        {
            case Difficulty.Hard: return 3;
            case Difficulty.Hell: return 2;
            default: return 4;
        }
    }
}