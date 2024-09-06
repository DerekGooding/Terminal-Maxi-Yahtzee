using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal_Maxi_Yahtzee;

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
