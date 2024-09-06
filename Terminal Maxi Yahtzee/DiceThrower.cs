namespace Terminal_Maxi_Yahtzee;

internal class DiceThrower
{
    public int[] DiceValues { get; }

    public DiceThrower()
    {
        DiceValues = new int[6];
        RollAllDice(); // Initialize all dice with random values
    }

    public void RollAllDice()
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            DiceValues[i] = Random.Shared.Next(1, 7);
        }
    }

    public void RollSpecificDice(HashSet<int> keepIndices)
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            if (!keepIndices.Contains(i)) // Only reroll dice that are not kept
            {
                DiceValues[i] = Random.Shared.Next(1, 7);
            }
        }
    }

    public void DisplayDice()
    {
        for (int i = 0; i < DiceValues.Length; i++)
        {
            WriteLine($"Dice {i + 1}: {DiceValues[i]}");
        }
    }

    public string GetDiceValuesAsString() => string.Join(", ", DiceValues.Select(value => $"{value}"));

    public HashSet<int> GetDiceToKeep(DiceThrower diceThrower, int[] currentRoll)
    {
        HashSet<int> keepIndices = [];

        while (true)
        {
            Clear();
            WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
            WriteLine("Write the values you wish to keep. Press ENTER to continue");

            string? input = ReadLine()?.Trim();
            Clear();
            // If the input is empty, reroll all dice
            if (string.IsNullOrEmpty(input))
            {
                WriteLine("Rerolling all dice...");
                return keepIndices;
            }

            // Validate input: Ensure all characters are digits between 1 and 6
            if (!input.All(c => char.IsDigit(c) && c >= '1' && c <= '6'))
            {
                Clear();
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Invalid input. Please enter numbers between 1 and 6.");
                ResetColor();
                ForegroundColor = ConsoleColor.White;
                WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                ResetColor();
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
                        if (currentRoll[i] == value && !keepIndices.Contains(i))
                        {
                            keepIndices.Add(i);
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
                ForegroundColor = ConsoleColor.Red;
                WriteLine("Input value does not exist. Please try again.");
                ResetColor();
                ForegroundColor = ConsoleColor.White;
                WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                ResetColor();
                continue; // Reprompt the player
            }

            // Return the boolean array indicating which dice to keep
            return keepIndices;
        }
    }
}