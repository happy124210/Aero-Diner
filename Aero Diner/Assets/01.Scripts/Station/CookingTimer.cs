using UnityEngine;

public class CookingTimer
{
    public float Duration { get; }
    public float Remaining { get; private set; }
    public bool IsRunning => Remaining > 0f;

    public CookingTimer(float duration)
    {
        Duration = duration;
        Remaining = 0f;
    }

    public void Start()
    {
        Remaining = Duration;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning) return;
        Remaining = Mathf.Max(Remaining - deltaTime, 0f);
    }
}
