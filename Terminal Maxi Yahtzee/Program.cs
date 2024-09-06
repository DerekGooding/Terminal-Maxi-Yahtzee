using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal_Maxi_Yahtzee;

internal static class Program
{
    private static void Main()
    {
        List<Player> players = [];
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
                    DiceThrower diceThrower = new();
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