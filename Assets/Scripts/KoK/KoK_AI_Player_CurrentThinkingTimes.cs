public class KoK_AI_Player_CurrentThinkingTimes
{
    public float ExtremelyWorse;
    public float MuchWorse;
    public float Worse;
    public float SlightlyWorse;
    public float Neutral;
    public float SlightlyBetter;
    public float Better;
    public float MuchBetter;
    public float ExtremelyBetter;
}

public static class KoK_AI_Player_ThinkingTimes
{
    public static float[] balanced_ExtremelyWorse = new float[] { 5f, 10f };
    public static float[] balanced_MuchWorse = new float[] { 4f, 9f };
    public static float[] balanced_Worse = new float[] { 3f, 8f };
    public static float[] balanced_SlightlyWorse = new float[] { 3.5f, 7f };
    public static float[] balanced_Neutral = new float[] { 3, 6f };
    public static float[] balanced_SlightlyBetter = new float[] { 2.5f, 5f };
    public static float[] balanced_Better = new float[] { 2.5f, 4f };
    public static float[] balanced_MuchBetter = new float[] { 2.5f, 3f };
    public static float[] balanced_ExtremelyBetter = new float[] { 2, 3f };

    // public static float[] balanced_ExtremelyWorse = new float[] { 5f, 10f };
    // public static float[] balanced_MuchWorse = new float[] { 4f, 9f };
    // public static float[] balanced_Worse = new float[] { 3f, 8f };
    // public static float[] balanced_SlightlyWorse = new float[] { 3.5f, 7f };
    // public static float[] balanced_Neutral = new float[] { 3, 6f };
    // public static float[] balanced_SlightlyBetter = new float[] { 2.5f, 5f };
    // public static float[] balanced_Better = new float[] { 2.5f, 4f };
    // public static float[] balanced_MuchBetter = new float[] { 2.5f, 3f };
    // public static float[] balanced_ExtremelyBetter = new float[] { 2, 3f };
}
