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
            case Difficulty.Hell: return 2.25f;
            default: return 1f;
        }
    }

    public float GetStaminaMultiplier()
    {
        switch (current)
        {
            case Difficulty.Hard: return 1.5f;
            case Difficulty.Hell: return 2.25f;
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