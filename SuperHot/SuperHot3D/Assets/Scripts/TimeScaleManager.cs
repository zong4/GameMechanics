public class TimeScaleManager
{
    private static TimeScaleManager _instance;

    private float _timeScale = 1f;

    private static TimeScaleManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TimeScaleManager();
            return _instance;
        }
    }

    public float GetTimeScale()
    {
        return Instance._timeScale;
    }

    public static void SetTimeScale(float scale)
    {
        if (scale < 0f)
            throw new System.ArgumentOutOfRangeException(nameof(scale), "Time scale cannot be negative.");

        Instance._timeScale = scale;
        UnityEngine.Time.timeScale = scale;
    }

    public static void ResetTimeScale()
    {
        Instance._timeScale = 1f;
        UnityEngine.Time.timeScale = 1f;
    }
}