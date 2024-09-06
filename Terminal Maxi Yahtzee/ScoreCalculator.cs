namespace Terminal_Maxi_Yahtzee;

internal static class ScoreCalculator
{
    public static Dictionary<string, Func<int[], int>> ScoreFunctions { get; }

    internal static readonly int[] _oneToFive = [1, 2, 3, 4, 5];
    internal static readonly int[] _twoToSix = [2, 3, 4, 5, 6];

    static ScoreCalculator() => ScoreFunctions = new Dictionary<string, Func<int[], int>>
    {
        {"ones", dice => dice.Where(d => d == 1).Sum(_ => 1)},
        {"twos", dice => dice.Where(d => d == 2).Sum(_ => 2)},
        {"threes", dice => dice.Where(d => d == 3).Sum(_ => 3)},
        {"fours", dice => dice.Where(d => d == 4).Sum(_ => 4)},
        {"fives", dice => dice.Where(d => d == 5).Sum(_ => 5)},
        {"sixes", dice => dice.Where(d => d == 6).Sum(_ => 6)},
        {"one pair", GetHighestPairScore},
        {"two pairs", GetTwoPairScore},
        {"three pairs", GetThreePairsScore},
        {"3 same", dice => GetOfAKindScore(dice, 3)},
        {"4 same", dice => GetOfAKindScore(dice, 4)},
        {"5 same", dice => GetOfAKindScore(dice, 5)},
        {"small straight", GetSmallStraightScore},
        {"large straight", GetLargeStraightScore},
        {"full straight", GetFullStraightScore},
        {"hut 2+3", GetHut},
        {"house 3+3", GetHouse},
        {"tower 2+4", GetTowerScore},
        {"chance", dice => dice.Sum()},
        {"maxi-yahtzee", GetMaxiYahtzeeScore}
    };

    private static int GetHighestPairScore(int[] dice)
    {
        // Group dice by value and filter groups where at least two dice share the same value
        var pairs = dice.GroupBy(d => d)
                        .Where(g => g.Count() >= 2)
                        .Select(g => new { Value = g.Key, Count = g.Count() })
                        .ToList();

        // If pairs exist, find the highest value pair and return twice its value
        if (pairs.Count > 0)
        {
            var highestPair = pairs.OrderByDescending(p => p.Value).First();
            return highestPair.Value * 2;
        }

        return 0;
    }

    private static int GetTwoPairScore(int[] dice)
    {
        // Group the dice by their values
        var pairs = dice.GroupBy(d => d)
                        .Where(g => g.Count() >= 2)  // Select groups that have at least two of the same value
                        .Select(g => new { Value = g.Key, Count = g.Count() / 2 })  // Map to value and count of pairs
                        .ToList();

        // Check if there are at least two distinct pairs
        if (pairs.Count >= 2)
        {
            return pairs.OrderByDescending(p => p.Value)  // Sort pairs by value, high to low
                        .Take(2)  // Select the top two pairs
                        .Sum(p => p.Value * 2);  // Sum twice the value of each pair
        }

        return 0;
    }

    private static int GetThreePairsScore(int[] dice)
    {
        // Group by dice values and filter only groups that have exactly 2 of the same kind
        List<IGrouping<int, int>> groups = dice.GroupBy(d => d)
                         .Where(g => g.Count() == 2)
                         .ToList();

        // Ensure that there are exactly three groups (pairs)
        return groups.Count == 3 ? groups.Sum(g => g.Key * 2) : 0;
    }

    private static int GetOfAKindScore(int[] dice, int count)
        => dice.GroupBy(d => d)
               .Where(g => g.Count() >= count)
               .Select(g => g.Key * count)
               .FirstOrDefault();

    private static int GetSmallStraightScore(int[] dice) => _oneToFive.All(new HashSet<int>(dice).Contains) ? 15 : 0;

    private static int GetLargeStraightScore(int[] dice) => _twoToSix.All(new HashSet<int>(dice).Contains) ? 20 : 0;

    private static int GetFullStraightScore(int[] dice) => new HashSet<int>(dice).Count == 6 ? 21 : 0;

    private static int GetHut(int[] dice)
    {
        List<IGrouping<int, int>> groups = dice.GroupBy(d => d).ToList();

        // Check for the presence of exactly one triplet and one pair
        IGrouping<int, int> hasThreeOfAKind = groups.Find(g => g.Count() == 3);
        IGrouping<int, int> hasPair = groups.Find(g => g.Count() == 2);

        if (hasThreeOfAKind != null && hasPair != null)
        {
            // Score is calculated as the sum of all dice that are part of the full house
            return (hasThreeOfAKind.Key * 3) + (hasPair.Key * 2);
        }

        // If there isn't one triplet and one pair, the score is zero
        return 0;
    }

    private static int GetHouse(int[] dice)
    {
        List<IGrouping<int, int>> groups = dice.GroupBy(d => d).ToList();
        return groups.Count(g => g.Count() >= 3) == 2 ? groups.Sum(g => g.Key * g.Count()) : 0;
    }

    private static int GetTowerScore(int[] dice)
    {
        List<IGrouping<int, int>> groups = dice.GroupBy(d => d).ToList();
        return groups.Any(g => g.Count() == 4) && groups.Any(g => g.Count() == 2) ? groups.Sum(g => g.Key * g.Count()) : 0;
    }

    private static int GetMaxiYahtzeeScore(int[] dice) => dice == null || dice.Length == 0 || dice.All(d => d == 0) || dice.Any(d => d != dice[0]) ? 0 : 100;
}