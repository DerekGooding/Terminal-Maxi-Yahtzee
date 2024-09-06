namespace Terminal_Maxi_Yahtzee;

internal class Player
{
    public string Name { get; set; }
    public Dictionary<string, int?> PlayerCard { get; set; }
    public int AvailableThrows { get; set; }
    public bool BonusCheck { get; set; }

    private static readonly Dictionary<string, string> CategoryShortcuts = new()
    {
    { "on", "ones" },
    { "tw", "twos" },
    { "th", "threes" },
    { "fo", "fours" },
    { "fi", "fives" },
    { "si", "sixes" },
    { "op", "one pair" },
    { "tp", "two pairs" },
    { "thp", "three pairs" },
    { "3", "3 same" },
    { "4", "4 same" },
    { "5", "5 same" },
    { "ss", "small straight" },
    { "ls", "large straight" },
    { "fs", "full straight" },
    { "hu", "hut 2+3" },
    { "ho", "house 3+3" },
    { "to", "tower 2+4" },
    { "ch", "chance" },
    { "ma", "maxi-yahtzee" }
};

    public Player(string name)
    {
        Name = name;
        AvailableThrows = 3;
        BonusCheck = false;
        PlayerCard = new Dictionary<string, int?>
    {
        { "ones", null },
        { "twos", null },
        { "threes", null },
        { "fours", null },
        { "fives", null },
        { "sixes", null },
        { "one pair", null },
        { "two pairs", null },
        { "three pairs", null },
        { "3 same", null },
        { "4 same", null },
        { "5 same", null },
        { "small straight", null },
        { "large straight", null },
        { "full straight", null },
        { "hut 2+3", null },
        { "house 3+3", null },
        { "tower 2+4", null },
        { "chance", null },
        { "maxi-yahtzee", null }
    };
    }

    public void PrintPlayerCard()
    {
        int maxKeyLength = PlayerCard.Keys.Max(key => key.Length);
        foreach (KeyValuePair<string, int?> entry in PlayerCard)
        {
            string scoreText = entry.Value.HasValue ? entry.Value.ToString() : "-";
            WriteLine($"{entry.Key.PadRight(maxKeyLength)}: {scoreText}");
        }
    }

    public void ChooseScoreCategory(int[] diceValues)
    {
        // Check if the player skipped their turn (diceValues is null)
        bool turnSkipped = diceValues == null;

        PrintPlayerCard();
        ForegroundColor = ConsoleColor.Green;
        WriteLine("Write category name to input score (score will be set to 0 if no dice were rolled):");
        ResetColor();
        string inputCategory = ReadLine().ToLower().Trim();

        if (CategoryShortcuts.ContainsKey(inputCategory))
        {
            inputCategory = CategoryShortcuts[inputCategory];
        }

        // Check if the category is valid and not already scored
        if (PlayerCard.ContainsKey(inputCategory) && !PlayerCard[inputCategory].HasValue)
        {
            int score = turnSkipped ? 0 : ScoreCalculator.ScoreFunctions[inputCategory](diceValues); // Set score to 0 if no dice were rolled
            PlayerCard[inputCategory] = score;
            ForegroundColor = ConsoleColor.White;
            WriteLine($"{inputCategory} set to {score}");
            ResetColor();
            CheckBonusEligibility();
        }
        else
        {
            Clear();
            ForegroundColor = ConsoleColor.Red;
            WriteLine("Invalid category or already scored. Please try again.");
            ResetColor();
            ChooseScoreCategory(diceValues); // Retry if invalid input
        }
    }

    public void DisplayShorthandNotations()
    {
        ForegroundColor = ConsoleColor.Cyan;
        WriteLine("Shorthand Notations:");
        foreach (KeyValuePair<string, string> entry in CategoryShortcuts)
        {
            WriteLine($"{entry.Key} => {entry.Value}");
        }
        ResetColor();
        WriteLine();
    }

    public bool IsScoreboardComplete => PlayerCard.Values.All(score => score.HasValue);

    public void CheckBonusEligibility()
    {
        int? combinedScore = 0;

        combinedScore += PlayerCard["ones"] ?? 0;
        combinedScore += PlayerCard["twos"] ?? 0;
        combinedScore += PlayerCard["threes"] ?? 0;
        combinedScore += PlayerCard["fours"] ?? 0;
        combinedScore += PlayerCard["fives"] ?? 0;
        combinedScore += PlayerCard["sixes"] ?? 0;

        if (combinedScore >= 84)
        {
            BonusCheck = true;
        }
    }

    public int CalculateTotalScore()
    {
        int totalScore = PlayerCard.Values.Where(v => v.HasValue).Sum(v => v.Value);

        // If the bonus check is true, add 100 points to the total score
        if (BonusCheck)
        {
            totalScore += 100;
        }

        return totalScore;
    }
}