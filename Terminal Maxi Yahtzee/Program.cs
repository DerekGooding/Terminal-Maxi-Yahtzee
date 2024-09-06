using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal_Maxi_Yahtzee;

internal class Player
{
    public string Name { get; set; }
    public Dictionary<string, int?> PlayerCard { get; set; }
    public int AvailableThrows { get; set; }
    public bool BonusCheck { get; set; }

    private static readonly Dictionary<string, string> CategoryShortcuts = new Dictionary<string, string>
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
        foreach (var entry in PlayerCard)
        {
            string scoreText = entry.Value.HasValue ? entry.Value.ToString() : "-";
            Console.WriteLine($"{entry.Key.PadRight(maxKeyLength)}: {scoreText}");
        }
    }

    public void ChooseScoreCategory(int[] diceValues)
    {
        // Check if the player skipped their turn (diceValues is null)
        bool turnSkipped = diceValues == null;

        PrintPlayerCard();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Write category name to input score (score will be set to 0 if no dice were rolled):");
        Console.ResetColor();
        string inputCategory = Console.ReadLine().ToLower().Trim();

        if (CategoryShortcuts.ContainsKey(inputCategory))
        {
            inputCategory = CategoryShortcuts[inputCategory];
        }

        // Check if the category is valid and not already scored
        if (PlayerCard.ContainsKey(inputCategory) && !PlayerCard[inputCategory].HasValue)
        {
            int score = turnSkipped ? 0 : ScoreCalculator.ScoreFunctions[inputCategory](diceValues); // Set score to 0 if no dice were rolled
            PlayerCard[inputCategory] = score;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{inputCategory} set to {score}");
            Console.ResetColor();
            CheckBonusEligibility();
        }
        else
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid category or already scored. Please try again.");
            Console.ResetColor();
            ChooseScoreCategory(diceValues); // Retry if invalid input
        }
    }

    public void DisplayShorthandNotations()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Shorthand Notations:");
        foreach (var entry in CategoryShortcuts)
        {
            Console.WriteLine($"{entry.Key} => {entry.Value}");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    public bool IsScoreboardComplete()
    {
        // Checks that every score has an int value
        return PlayerCard.Values.All(score => score.HasValue);
    }

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

internal class DiceThrower
{
    public int[] DiceValues { get; private set; }
    private Random random;

    public DiceThrower()
    {
        DiceValues = new int[6];
        random = new Random();
        RollAllDice(); // Initialize all dice with random values
    }

    public void RollAllDice()
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            DiceValues[i] = random.Next(1, 7);
        }
    }

    public void RollSpecificDice(bool[] diceToKeep)
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            if (!diceToKeep[i]) // Only reroll dice that are not kept
            {
                DiceValues[i] = random.Next(1, 7);
            }
        }
    }

    public void DisplayDice()
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            Console.WriteLine($"Dice {i + 1}: {DiceValues[i]}");
        }
    }

    public string GetDiceValuesAsString()
    {
        return string.Join(", ", DiceValues.Select((value, index) => $"{value}"));
    }

    public bool[] GetDiceToKeep(DiceThrower diceThrower, int[] currentRoll)
    {
        bool[] diceToKeep = new bool[currentRoll.Length]; // To track which dice to keep (true means keep)

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
            Console.WriteLine("Write the values you wish to keep. Press ENTER to continue");

            string input = Console.ReadLine().Trim();
            Console.Clear();
            // If the input is empty, reroll all dice
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Rerolling all dice...");
                return new bool[currentRoll.Length]; // All dice will be rerolled (none are kept)
            }

            // Validate input: Ensure all characters are digits between 1 and 6
            if (!input.All(c => char.IsDigit(c) && c >= '1' && c <= '6'))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please enter numbers between 1 and 6.");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                Console.ResetColor();
                continue; // Reprompt the player
            }

            // Convert input to an array of integers representing dice values the player wants to keep
            int[] valuesToKeep = input.Select(c => int.Parse(c.ToString())).ToArray();

            // Copy current dice roll and track which dice have been marked as kept
            Dictionary<int, int> diceCount = currentRoll.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());

            bool invalidKeep = false;

            // For each value the player wants to keep, check if it exists in the current roll
            foreach (int value in valuesToKeep)
            {
                if (diceCount.ContainsKey(value) && diceCount[value] > 0)
                {
                    // Find the first available die with this value and mark it to keep
                    for (int i = 0; i < currentRoll.Length; i++)
                    {
                        if (currentRoll[i] == value && !diceToKeep[i])
                        {
                            diceToKeep[i] = true;
                            diceCount[value]--; // Reduce the count of available dice of this value
                            break;
                        }
                    }
                }
                else
                {
                    invalidKeep = true;
                    break;
                }
            }

            if (invalidKeep)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input value does not exist. Please try again.");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                Console.ResetColor();
                continue; // Reprompt the player
            }

            // Return the boolean array indicating which dice to keep
            return diceToKeep;
        }
    }
}

internal class ScoreCalculator
{
    public static Dictionary<string, Func<int[], int>> ScoreFunctions { get; private set; }

    static ScoreCalculator()
    {
        ScoreFunctions = new Dictionary<string, Func<int[], int>>
    {
        {"ones", dice => dice.Where(d => d == 1).Sum(d => 1)},
        {"twos", dice => dice.Where(d => d == 2).Sum(d => 2)},
        {"threes", dice => dice.Where(d => d == 3).Sum(d => 3)},
        {"fours", dice => dice.Where(d => d == 4).Sum(d => 4)},
        {"fives", dice => dice.Where(d => d == 5).Sum(d => 5)},
        {"sixes", dice => dice.Where(d => d == 6).Sum(d => 6)},
        {"one pair", dice => GetHighestPairScore(dice)},
        {"two pairs", dice => GetTwoPairScore(dice)},
        {"three pairs", dice => GetThreePairsScore(dice)},
        {"3 same", dice => GetOfAKindScore(dice, 3)},
        {"4 same", dice => GetOfAKindScore(dice, 4)},
        {"5 same", dice => GetOfAKindScore(dice, 5)},
        {"small straight", dice => GetSmallStraightScore(dice)},
        {"large straight", dice => GetLargeStraightScore(dice)},
        {"full straight", dice => GetFullStraightScore(dice)},
        {"hut 2+3", dice => GetHut(dice)},
        {"house 3+3", dice => GetHouse(dice)},
        {"tower 2+4", dice => GetTowerScore(dice)},
        {"chance", dice => dice.Sum()},
        {"maxi-yahtzee", dice => GetMaxiYahtzeeScore(dice)}
    };
    }

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
        if (pairs.Count() >= 2)
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
        var groups = dice.GroupBy(d => d)
                         .Where(g => g.Count() == 2)
                         .ToList();

        // Ensure that there are exactly three groups (pairs)
        if (groups.Count == 3)
        {
            return groups.Sum(g => g.Key * 2);
        }

        return 0;
    }

    private static int GetOfAKindScore(int[] dice, int count)
    {
        return dice.GroupBy(d => d)
                   .Where(g => g.Count() >= count)
                   .Select(g => g.Key * count)
                   .FirstOrDefault();
    }

    private static int GetSmallStraightScore(int[] dice)
    {
        var straight = new HashSet<int>(dice);
        if (new int[] { 1, 2, 3, 4, 5 }.All(straight.Contains))
            return 15;
        return 0;
    }

    private static int GetLargeStraightScore(int[] dice)
    {
        var straight = new HashSet<int>(dice);
        if (new int[] { 2, 3, 4, 5, 6 }.All(straight.Contains))
            return 20;
        return 0;
    }

    private static int GetFullStraightScore(int[] dice)
    {
        return new HashSet<int>(dice).Count == 6 ? 21 : 0;
    }

    private static int GetHut(int[] dice)
    {
        var groups = dice.GroupBy(d => d).ToList();

        // Check for the presence of exactly one triplet and one pair
        var hasThreeOfAKind = groups.FirstOrDefault(g => g.Count() == 3);
        var hasPair = groups.FirstOrDefault(g => g.Count() == 2);

        if (hasThreeOfAKind != null && hasPair != null)
        {
            // Score is calculated as the sum of all dice that are part of the full house
            return hasThreeOfAKind.Key * 3 + hasPair.Key * 2;
        }

        // If there isn't one triplet and one pair, the score is zero
        return 0;
    }

    private static int GetHouse(int[] dice)
    {
        var groups = dice.GroupBy(d => d).ToList();
        return groups.Count(g => g.Count() >= 3) == 2 ? groups.Sum(g => g.Key * g.Count()) : 0;
    }

    private static int GetTowerScore(int[] dice)
    {
        var groups = dice.GroupBy(d => d).ToList();
        if (groups.Any(g => g.Count() == 4) && groups.Any(g => g.Count() == 2))
            return groups.Sum(g => g.Key * g.Count());
        return 0;
    }

    private static int GetMaxiYahtzeeScore(int[] dice)
    {
        if (dice == null || dice.Length == 0 || dice.All(d => d == 0))
        {
            return 0;  // No valid roll, return 0
        }
        return dice.All(d => d == dice[0]) ? 100 : 0;
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        List<Player> players = new List<Player>();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Welcome to Terminal Maxi Yahtzee.\n");
        //Thread.Sleep(1000);
        Console.ResetColor();
        int playerCount = 0;

        while (true)
        {
            Console.Write("Please input the number of players: ");
            string input = Console.ReadLine();

            // Try to parse the input and ensure it is a positive integer greater than zero
            if (int.TryParse(input, out playerCount) && playerCount > 0)
            {
                break;  // Valid input, exit the loop
            }
            else
            {
                // Error handling: display a message and reprompt
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Invalid input. Please enter a positive integer greater than 0.");
                Console.ResetColor();
            }
        }

        for (int i = 1; i <= playerCount; i++)
        {
            Console.Clear();
            //Thread.Sleep(1000);
            Console.Write($"Enter name for player {i}: ");
            string name = Console.ReadLine();
            players.Add(new Player(name));
        }
        Console.Clear();
        Console.Write("Game starting...");
        //Thread.Sleep(1000);
        Console.Clear();

        while (true)  // Keep looping until all scoreboards are complete
        {
            foreach (Player player in players)
            {
                if (!player.IsScoreboardComplete())
                {
                    bool decisionMade = false;
                    bool turnSkipped = false; // New flag to track if the turn was skipped

                    // Offer the player the option to skip the turn before rolling any dice
                    Console.Clear();
                    player.PrintPlayerCard();
                    Console.WriteLine($"\n It's your turn {player.Name}.");
                    Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                    while (!decisionMade)
                    {
                        var keyPress = Console.ReadKey(true).Key;

                        if (keyPress == ConsoleKey.S)
                        {
                            Console.Clear();
                            Console.WriteLine($"{player.Name}'s Scoreboard:");
                            player.PrintPlayerCard();
                            Console.WriteLine();

                            Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                        }
                        else if (keyPress == ConsoleKey.H)
                        {
                            Console.Clear();
                            // Display shorthand notations when 'H' is pressed
                            player.DisplayShorthandNotations();
                            Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                        }
                        else if (keyPress == ConsoleKey.E)
                        {
                            Console.Clear();
                            player.AvailableThrows += 3;  // Save all 3 throws for the next turn
                            Console.WriteLine($"{player.Name} skipped their turn. 3 throws saved for later turns");
                            Console.WriteLine("You can set a score of 0 for a category.");
                            player.ChooseScoreCategory(null);  // Pass null to indicate the player skipped the turn
                            decisionMade = true;
                            turnSkipped = true;  // Set flag to true to indicate the turn was skipped
                            //break;  // Exit the loop
                        }
                        else if (keyPress == ConsoleKey.Enter)
                        {
                            Console.Clear();
                            decisionMade = true;
                        }
                    }

                    if (turnSkipped)
                    {
                        continue;  // Skip to the next player if the turn was skipped
                    }

                    Console.WriteLine();

                    // Dice rolling logic will only be executed if the turn wasn't skipped
                    int currentThrows = player.AvailableThrows;
                    DiceThrower diceThrower = new DiceThrower();
                    int throwCount = player.AvailableThrows;

                    bool endTurn = false;

                    for (int i = 0; i < throwCount; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{diceThrower.GetDiceValuesAsString()}\n");
                        Console.ResetColor();

                        int throwsRemaining = throwCount - i - 1;
                        if (throwsRemaining > 0)
                        {
                            Console.WriteLine($"\u001b[38;2;255;150;0m{throwsRemaining} throws remaining\u001b[0m \n");
                            decisionMade = false;
                            Console.WriteLine("Press 'ENTER' to continue");
                            Console.WriteLine("Press 'S' to view scoreboard");
                            Console.WriteLine("Press 'E' to end turn");
                            Console.WriteLine("Press 'H' to view shorthand notations");
                            while (!decisionMade)
                            {
                                var keyPress = Console.ReadKey(true).Key;

                                if (keyPress == ConsoleKey.S)
                                {
                                    Console.Clear();
                                    // Display the scoreboard and re-prompt the player
                                    Console.WriteLine($"{player.Name}'s Scoreboard:");
                                    player.PrintPlayerCard();
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine($"\n{diceThrower.GetDiceValuesAsString()}");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                    Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                                }
                                else if (keyPress == ConsoleKey.H)
                                {
                                    Console.Clear();
                                    // Display shorthand notations when 'H' is pressed
                                    player.DisplayShorthandNotations();
                                    Console.WriteLine($"{diceThrower.GetDiceValuesAsString()}");
                                    Console.WriteLine();
                                    Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                                }
                                else if (keyPress == ConsoleKey.E)
                                {
                                    Console.Clear();
                                    // End the player's turn and add remaining throws to next turn
                                    player.AvailableThrows = 3 + throwsRemaining;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"You ended your turn early. {throwsRemaining} throws carried over to your next turn.\n");
                                    Console.ResetColor();
                                    Console.WriteLine($"Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");

                                    decisionMade = true;
                                    endTurn = true;  // Set flag to true to indicate turn end
                                    break;  // Exit the loop
                                }
                                else if (keyPress == ConsoleKey.Enter)
                                {
                                    decisionMade = true;
                                }
                                // set statement to handle incorrect keypresses so that nothing happens
                            }
                            if (endTurn)
                            {
                                break;
                            }

                            int[] currentRoll = diceThrower.DiceValues;       // Get the current roll
                            bool[] diceToKeep = diceThrower.GetDiceToKeep(diceThrower, currentRoll); // Ask player which dice to keep
                            diceThrower.RollSpecificDice(diceToKeep);
                        }
                        else
                        {
                            Console.WriteLine("No throws remaining.");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Result:");
                    Console.WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                    Console.ResetColor();
                    player.ChooseScoreCategory(diceThrower.DiceValues);
                    //Thread.Sleep(1000);

                    Console.WriteLine();
                }
            }

            // Check if all players are complete
            if (players.All(p => p.IsScoreboardComplete()))
            {
                break;
            }
        }
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Game Over. Final Score: \n");
        Console.ResetColor();
        foreach (Player player in players)
        {
            int totalScore = player.CalculateTotalScore();
            Console.WriteLine($"{player.Name}'s total Score: {totalScore}");
        }

        while (true)
        {
        }
    }
}