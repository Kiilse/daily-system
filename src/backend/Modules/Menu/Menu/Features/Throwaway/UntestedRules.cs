namespace Menu.Features.Throwaway;

// Throwaway (do not merge): untested Features code, to check that the coverage gate fails.
internal static class UntestedRules
{
    public static int Score(int calories, int servings)
    {
        var score = 0;
        if (calories > 800)
        {
            score -= 2;
        }
        else if (calories > 500)
        {
            score -= 1;
        }

        if (servings > 4)
        {
            score += 1;
        }

        return score;
    }
}
