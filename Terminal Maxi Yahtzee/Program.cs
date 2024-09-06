namespace Terminal_Maxi_Yahtzee;

internal static class Program
{
    private static void Main()
    {
        List<Player> players = [];
        ForegroundColor = ConsoleColor.White;
        Write("Welcome to Terminal Maxi Yahtzee.\n");
        //Thread.Sleep(1000);
        ResetColor();
        int playerCount = 0;

        while (true)
        {
            Write("Please input the number of players: ");
            string input = ReadLine();

            // Try to parse the input and ensure it is a positive integer greater than zero
            if (int.TryParse(input, out playerCount) && playerCount > 0)
            {
                break;  // Valid input, exit the loop
            }
            else
            {
                // Error handling: display a message and reprompt
                ForegroundColor = ConsoleColor.DarkRed;
                WriteLine("Invalid input. Please enter a positive integer greater than 0.");
                ResetColor();
            }
        }

        for (int i = 1; i <= playerCount; i++)
        {
            Clear();
            //Thread.Sleep(1000);
            Write($"Enter name for player {i}: ");
            string name = ReadLine();
            players.Add(new Player(name));
        }
        Clear();
        Write("Game starting...");
        //Thread.Sleep(1000);
        Clear();

        do  // Keep looping until all scoreboards are complete
        {
            foreach (Player player in players)
            {
                if (!player.IsScoreboardComplete)
                {
                    bool decisionMade = false;
                    bool turnSkipped = false; // New flag to track if the turn was skipped

                    // Offer the player the option to skip the turn before rolling any dice
                    Clear();
                    player.PrintPlayerCard();
                    WriteLine($"\n It's your turn {player.Name}.");
                    WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                    while (!decisionMade)
                    {
                        ConsoleKey keyPress = ReadKey(true).Key;

                        if (keyPress == ConsoleKey.S)
                        {
                            Clear();
                            WriteLine($"{player.Name}'s Scoreboard:");
                            player.PrintPlayerCard();
                            WriteLine();

                            WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                        }
                        else if (keyPress == ConsoleKey.H)
                        {
                            Clear();
                            // Display shorthand notations when 'H' is pressed
                            player.DisplayShorthandNotations();
                            WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                        }
                        else if (keyPress == ConsoleKey.E)
                        {
                            Clear();
                            player.AvailableThrows += 3;  // Save all 3 throws for the next turn
                            WriteLine($"{player.Name} skipped their turn. 3 throws saved for later turns");
                            WriteLine("You can set a score of 0 for a category.");
                            player.ChooseScoreCategory(null);  // Pass null to indicate the player skipped the turn
                            decisionMade = true;
                            turnSkipped = true;  // Set flag to true to indicate the turn was skipped
                            //break;  // Exit the loop
                        }
                        else if (keyPress == ConsoleKey.Enter)
                        {
                            Clear();
                            decisionMade = true;
                        }
                    }

                    if (turnSkipped)
                    {
                        continue;  // Skip to the next player if the turn was skipped
                    }

                    WriteLine();

                    // Dice rolling logic will only be executed if the turn wasn't skipped
                    int currentThrows = player.AvailableThrows;
                    DiceThrower diceThrower = new();
                    int throwCount = player.AvailableThrows;

                    bool endTurn = false;

                    for (int i = 0; i < throwCount; i++)
                    {
                        ForegroundColor = ConsoleColor.White;
                        WriteLine($"{diceThrower.GetDiceValuesAsString()}\n");
                        ResetColor();

                        int throwsRemaining = throwCount - i - 1;
                        if (throwsRemaining > 0)
                        {
                            WriteLine($"\u001b[38;2;255;150;0m{throwsRemaining} throws remaining\u001b[0m \n");
                            decisionMade = false;
                            WriteLine("Press 'ENTER' to continue");
                            WriteLine("Press 'S' to view scoreboard");
                            WriteLine("Press 'E' to end turn");
                            WriteLine("Press 'H' to view shorthand notations");
                            while (!decisionMade)
                            {
                                ConsoleKey keyPress = ReadKey(true).Key;

                                if (keyPress == ConsoleKey.S)
                                {
                                    Clear();
                                    // Display the scoreboard and re-prompt the player
                                    WriteLine($"{player.Name}'s Scoreboard:");
                                    player.PrintPlayerCard();
                                    ForegroundColor = ConsoleColor.White;
                                    WriteLine($"\n{diceThrower.GetDiceValuesAsString()}");
                                    ResetColor();
                                    WriteLine();
                                    WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                                }
                                else if (keyPress == ConsoleKey.H)
                                {
                                    Clear();
                                    // Display shorthand notations when 'H' is pressed
                                    player.DisplayShorthandNotations();
                                    WriteLine($"{diceThrower.GetDiceValuesAsString()}");
                                    WriteLine();
                                    WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");
                                }
                                else if (keyPress == ConsoleKey.E)
                                {
                                    Clear();
                                    // End the player's turn and add remaining throws to next turn
                                    player.AvailableThrows = 3 + throwsRemaining;
                                    ForegroundColor = ConsoleColor.Green;
                                    WriteLine($"You ended your turn early. {throwsRemaining} throws carried over to your next turn.\n");
                                    ResetColor();
                                    WriteLine("Press 'ENTER' to throw\n Press 'S' to view scoreboard \n Press 'E' to end turn \n Press 'H' to view shorthand notations");

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
                            HashSet<int> diceToKeep = diceThrower.GetDiceToKeep(diceThrower, currentRoll); // Ask player which dice to keep
                            diceThrower.RollSpecificDice(diceToKeep);
                        }
                        else
                        {
                            WriteLine("No throws remaining.");
                        }
                    }

                    ForegroundColor = ConsoleColor.White;
                    WriteLine("Result:");
                    WriteLine($"\n{diceThrower.GetDiceValuesAsString()}\n");
                    ResetColor();
                    player.ChooseScoreCategory(diceThrower.DiceValues);
                    //Thread.Sleep(1000);

                    WriteLine();
                }
            }
        }
        while (!players.All(p => p.IsScoreboardComplete));
        Clear();
        ForegroundColor = ConsoleColor.DarkYellow;
        WriteLine("Game Over. Final Score: \n");
        ResetColor();
        foreach (Player player in players)
        {
            int totalScore = player.CalculateTotalScore();
            WriteLine($"{player.Name}'s total Score: {totalScore}");
        }

        while (true)
        {
        }
    }
}