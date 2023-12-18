//Author: Dylan Nagel
//File Name: main.cs
//Project Name: NagelDPASS1
//Description: A superior recreation of the the popular guessing game "Wordle"

using System;
using System.IO;

namespace NagelDPASS1
{
    class MainClass
    {
        //create variable to store random number
        static Random rng = new Random();

        //create variables to store game states
        const int MENU = 0;
        const int GAMEPLAY = 1;
        const int INSTRUCTIONS = 2;
        const int ENDGAME = 3;
        const int STATS = 4;

        //create variables to store guess accuracy
        const ConsoleColor UNDEFINED = ConsoleColor.White;
        const ConsoleColor COR_PLACE = ConsoleColor.Green;
        const ConsoleColor WRONG_PLACE = ConsoleColor.Yellow;
        const ConsoleColor WRONG_LETTER = ConsoleColor.DarkBlue;

        //create variables to store the secondary game theme colors
        const ConsoleColor DARK_CYAN = ConsoleColor.DarkCyan;
        const ConsoleColor DARK_GREEN = ConsoleColor.DarkGreen;

        //create variables to store min and max valid letter ascii value
        const int CAP_A_VAL = 'A';
        const int CAP_Z_VAL = 'Z';

        //create variable to store padding size for statistics screen
        const int STAT_PAD = 30;

        //create variables to store locations of coloured characters in the instructions screen
        const int SPEC_CHAR_LOC_1 = 0;
        const int SPEC_CHAR_LOC_2 = 1;
        const int SPEC_CHAR_LOC_3 = 4;

        //create variables for file interaction
        static StreamWriter outFile;
        static StreamReader inFile;

        public static void Main(string[] args)
        {
            //create variables to read and store wordle dictionaries
            string[] wordleAnswers;
            string[] wordleExtras;
            string[] line;

            //create variables to get and store correct word
            int randNum;
            string corWord = "";
            string corWordEdit;

            //create variable to store gamestate
            int gameState = MENU;

            //create variable to store user input
            ConsoleKey userInput;

            //create variable to store indexes in game
            int kBIndex;
            int index;

            //create variables to store user guess information
            char[,] userGuesses = new char[5, 6];
            string userGuess;

            //create variables to store guess accuracy
            ConsoleColor[,] tableAcc = new ConsoleColor[5, 6];
            ConsoleColor[] keyboardAcc = new ConsoleColor[26];

            //create variable to store guess validity
            bool isGuessValid = false;

            //create variables to store if game is playing
            bool isWordleOn = true;
            bool isPlaying = false;
            bool didUserWin = false;

            //create variables to store game loading information
            bool isPrevGameCont = false;
            bool isStatsReset = false;
            bool wasErrorCatched = true;

            //create variables to store special character information for instruction screen
            int[] instructSpecChar = new int[] { SPEC_CHAR_LOC_1, SPEC_CHAR_LOC_2, SPEC_CHAR_LOC_3 };
            ConsoleColor[] instrucColors = new ConsoleColor[] { COR_PLACE, WRONG_PLACE, WRONG_LETTER };

            //create variable to store phrases used for instruction screen
            string[] instructWords = new string[] { "WEARY", "READY", "BLACK" };
            string[] instructMessages = new string[] { "is in the word and in the correct spot\n", "is in the word but in the incorrect spot\n", "is not in the word in any spot\n\n\n" };

            //create variables to store keyboard information
            string[] keyBoardLines = new string[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
            string[] kBRowToppers = new string[] { "┏━━━┳━━━┳━━━┳━━━┳━━━┳━━━┳━━━┳━━━┳━━━┳━━━┓", "┗━┳━┻━┳━┻━┳━┻━┳━┻━┳━┻━┳━┻━┳━┻━┳━┻━┳━┻━┳━┛", "┗━━━╋━━━╋━━━╋━━━╋━━━╋━━━╋━━━╋━━━╋━━━┛", "┗━━━┻━━━┻━━━┻━━━┻━━━┻━━━┻━━━┛" };

            //create variable to store message for user during gameplay
            string userMessage = "";

            //create variables to store user statistics
            int gamesPlayed = 0;
            int gamesWon = 0;
            int winStreak = 0;
            int maxWinStreak = 0;
            int[] winGuessDis = new int[userGuesses.GetLength(1)];

            //create variables to store round information
            int roundWon = -1;
            int roundNum = 0;

            //reset the accuracy of keyboard
            for (int i = 0; i < keyboardAcc.Length; i++)
            {
                //reset accuracy of keyboard at index
                keyboardAcc[i] = UNDEFINED;
            }

            //assign variables with file dictionaries
            wordleAnswers = File.ReadAllText("WordleAnswers.txt").Split('\n');
            wordleExtras = File.ReadAllText("WordleExtras.txt").Split('\n');

            try
            {
                //assign variable with user stat file information
                inFile = File.OpenText("UserStats.txt");

                //read user's previous statistics from file
                gamesPlayed = Convert.ToInt32(inFile.ReadLine());
                gamesWon = Convert.ToInt32(inFile.ReadLine());
                winStreak = Convert.ToInt32(inFile.ReadLine());
                maxWinStreak = Convert.ToInt32(inFile.ReadLine());

                //store win guess distribution in an array that is split at ','
                line = inFile.ReadLine().Split(',');

                //assign number value for each win guess distribution round
                for (int i = 0; i < line.Length; i++)
                {
                    //set win guess distrubution at index with line value at index
                    winGuessDis[i] = Convert.ToInt32(line[i]);
                }

                //set is playing variable to file input
                isPlaying = Convert.ToBoolean(inFile.ReadLine());

                //input round number from file
                roundNum = Convert.ToInt32(inFile.ReadLine());

                //check if round number does not equal zero
                if (isPlaying && roundNum != 0)
                {
                    //set continue from previous game variable to true
                    isPrevGameCont = true;

                    //set gamestate to gameplay
                    gameState = GAMEPLAY;

                    isPlaying = false;

                    //input correct word from file
                    corWord = inFile.ReadLine();

                    //check if the correct word is greater than its maximum length
                    if (corWord.Length > userGuesses.GetLength(0))
                    {
                        //set reset statistics variable to true
                        isStatsReset = true;
                    }

                    //read and store the columns of user guesses and their corrosponding accuracy from file
                    for (int i = 0; i < roundNum; i++)
                    {
                        //store line from file
                        line = inFile.ReadLine().Split(',');

                        //read and store the rows of user guesses and their corrosponding accuracy from file
                        for (int x = 0; x < line.Length; x++)
                        {
                            //set userguesses at index x and index i the read line at index x
                            userGuesses[x, i] = Convert.ToChar(line[x].Split(':')[0]);

                            //check what  accuracy the guess is
                            if (line[x].Split(':')[1].Equals("White"))
                            {
                                //set accuracy to undefined
                                tableAcc[x, i] = UNDEFINED;
                            }
                            else if (line[x].Split(':')[1].Equals("DarkBlue"))
                            {
                                //set accuracy to wrong letter
                                tableAcc[x, i] = WRONG_LETTER;
                                keyboardAcc[userGuesses[x, i] - CAP_A_VAL] = WRONG_LETTER;
                            }
                            else if (line[x].Split(':')[1].Equals("Yellow"))
                            {
                                //set accuracy to wrong place
                                tableAcc[x, i] = WRONG_PLACE;

                                //check if keyboard accuracy at index x, i is not at a higher accuracy
                                if(keyboardAcc[userGuesses[x, i] - CAP_A_VAL] != COR_PLACE)
                                {
                                    //set keyboard accuracy at index x, i to wrong place
                                    keyboardAcc[userGuesses[x, i] - CAP_A_VAL] = WRONG_PLACE;
                                }
                            }
                            else if (line[x].Split(':')[1].Equals("Green"))
                            {
                                //set accuracy to correct place
                                tableAcc[x, i] = COR_PLACE;

                                //set keyboard accuracy at index x, i to correct place
                                keyboardAcc[userGuesses[x, i] - CAP_A_VAL] = COR_PLACE;
                            }
                            else
                            {
                                //set reset statistics variable to true
                                isStatsReset = true;
                            }
                        }
                    }

                    //reset remaining columns of user guesses and their corrosponding accuracy
                    for (int i = roundNum; i < userGuesses.GetLength(1); i++)
                    {
                        //reset remaining rows of user guesses and their corrosponding accuracy
                        for (int x = 0; x < userGuesses.GetLength(0); x++)
                        {
                            //reset userguesses and table accuracy at index x, index i
                            userGuesses[x, i] = ' ';
                            tableAcc[x, i] = UNDEFINED;
                        }
                    }
                }

                //set is error catched to false
                wasErrorCatched = false;
            }
            catch (FileNotFoundException)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("Your previous save file could not be found! If this is your first game, have fun! Otherwise, your past data was lost! Press ", "Your previous save file could not be found! If this is your first game, have fun! Otherwise, your past data was lost! Press any key to continue!", "any key", " to continue!");
                Console.ReadKey();
            }
            catch (FormatException)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("A conversion error occured while trying to continue from previous save. Your past data was lost! Press ", "A conversion error occured while trying to continue from previous save. Your past data was lost! Press any key to continue!", "any key", " to continue!");
                isStatsReset = true;
                Console.ReadKey();
            }
            catch (IndexOutOfRangeException)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("An index out of range exception occured while trying to continue from previous save. Your past data was lost! Press ", "An index out of range exception occured while trying to continue from previous save. Your past data was lost! Press any key to continue!", "any key", " to continue!");
                Console.ReadKey();
            }
            catch (EndOfStreamException)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("An index out of range exception occured while trying to continue from previous save. Your past data was lost! Press ", "An index out of range exception occured while trying to continue from previous save. Your past data was lost! Press any key to continue!", "any key", " to continue!");
                Console.ReadKey();
            }
            catch(NullReferenceException)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("A null reference exception occured while trying to continue from previous save. Your past data was lost! Press ", "A null reference exception occured while trying to continue from previous save. Your past data was lost! Press any key to continue!", "any key", " to continue!");
                Console.ReadKey();
            }
            catch (Exception)
            {
                //set is stats reset to true
                isStatsReset = true;

                //inform user of the error that occured and promt input to continue centered and coloured appropriately
                ChangeColor("An error occured while trying to continue from previous save. Your past data was lost! Press ", "An error occured while trying to continue from previous save. Your past data was lost! Press any key to continue!", "any key", " to continue!");
                Console.ReadKey();
            }

            //check if file is not null
            if (inFile != null)
            {
                //close file
                inFile.Close();
            }

            //check if reset statistics variable is true
            if (isStatsReset)
            {
                //set gamestate to menu
                gameState = MENU;

                //set is playing variable to false
                isPrevGameCont = false;

                //reset user stats
                gamesPlayed = 0;
                gamesWon = 0;
                winStreak = 0;
                maxWinStreak = 0;

                //reset each round of win guess distribution
                for (int i = 0; i < winGuessDis.Length; i++)
                {
                    //set win guess distribution at index to zero
                    winGuessDis[i] = 0;
                }

                //check if an error was not catched
                if (!wasErrorCatched)
                {
                    //inform user of the error that occured and promt input to continue centered and coloured appropriately
                    ChangeColor("Information from previous save was invalid! Your past data was lost! Press ", "Information from previous save was invalid! Your past data was lost! Press any key to continue!", "any key", " to continue!");
                    Console.ReadKey();
                }

                //save user data to file
                SaveData(gamesPlayed, gamesWon, winStreak, maxWinStreak, winGuessDis, isPlaying, corWord, userGuesses, roundNum, tableAcc);
            }

            //play entire wordle program
            while (isWordleOn)
            {
                //clear screen
                Console.Clear();

                //complete wordle actions based on current game state
                switch (gameState)
                {
                    case MENU:
                        //display title of game to user
                        PrintGameTitle();

                        //print user prompt to change gamestate to user centered and coloured appropriately
                        ChangeColor("Enter ", "Enter I for instructions", "P", " to play wordle");
                        ChangeColor("Enter ", "Enter I for instructions", "I", " for instructions");
                        ChangeColor("Enter ", "Enter I for instructions", "S", " for statistics");
                        ChangeColor("Enter ", "Enter I for instructions", "ESCAPE", " to exit game");

                        //change game state depending on user input
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.P:
                                //change gamestate to gameplay
                                gameState = GAMEPLAY;
                                break;

                            case ConsoleKey.I:
                                //change gamestate to instructions
                                gameState = INSTRUCTIONS;
                                break;

                            case ConsoleKey.S:
                                //set gamestate to statistics
                                gameState = STATS;
                                break;

                            case ConsoleKey.Escape:
                                //set is wordle on variable to false
                                isWordleOn = false;
                                break;
                        }
                        break;

                    case GAMEPLAY:
                        //check if not continuing from previous game info
                        if (!isPrevGameCont)
                        {
                            //assign random word for user to guess
                            randNum = rng.Next(0, wordleAnswers.Length);
                            corWord = wordleAnswers[randNum].ToUpper();

                            //reset the rows of user guesses and table accuracy
                            for (int i = 0; i < userGuesses.GetLength(0); i++)
                            {
                                //reset the columns of user guesses and table accuracy
                                for (int x = 0; x < userGuesses.GetLength(1); x++)
                                {
                                    //reset userguesses and table accuracy at i and x
                                    userGuesses[i, x] = ' ';
                                    tableAcc[i, x] = UNDEFINED;
                                }
                            }

                            //reset the accuracy of keyboard
                            for (int i = 0; i < keyboardAcc.Length; i++)
                            {
                                //reset accuracy of keyboard at index
                                keyboardAcc[i] = UNDEFINED;
                            }

                            //set round number to zero
                            roundNum = 0;

                            //set is game playing variable to true
                            isPlaying = true;
                        }
                        else
                        {
                            //inform user that they quit wordle before finishing the game coloured, underlined and centred appropriately
                            Console.ForegroundColor = DARK_CYAN;
                            CenterPrint("You quit wordle before finishing your game!", "You quit wordle before finishing your game!", true);
                            Console.ResetColor();
                            CenterPrint("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", true);

                            //promt user to continue their previous game or back to menu
                            ChangeColor("Press ", "Press M to return to menu (game will be counted as a loss)", "M", " to return to menu (game will be counted as a loss)");
                            ChangeColor("Press ", "Press M to return to menu (game will be counted as a loss)", "C", " to continue from previous game");

                            //perform a task depending on user input
                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.C:
                                    //set game playing variable to true
                                    isPlaying = true;

                                    //set is continuation of previous game to false
                                    isPrevGameCont = false;
                                    break;

                                case ConsoleKey.M:
                                    //set gamestate to meny
                                    gameState = MENU;

                                    //set is boolean variables to false
                                    isPrevGameCont = false;
                                    isPlaying = false;

                                    //edit user stats accordingly
                                    winStreak = 0;
                                    gamesPlayed++;
                                    break;
                            }
                        }

                        //reset user typing in chart index 
                        kBIndex = 0;

                        //save user data to file
                        SaveData(gamesPlayed, gamesWon, winStreak, maxWinStreak, winGuessDis, isPlaying, corWord, userGuesses, roundNum, tableAcc);

                        //play each round of wordle
                        while (isPlaying)
                        {
                            //reset user guess variable
                            userGuess = "";

                            //clear screen and draw wordle chart and keyboard for user
                            Console.Clear();
                            DisplayBoard(userGuesses, roundNum, userMessage, tableAcc);
                            DisplayKeyBoard(keyBoardLines, keyboardAcc, kBRowToppers);

                            //reset the user message variable
                            userMessage = "";

                            //store user input
                            userInput = Console.ReadKey().Key;

                            //perform specified task depending on user input
                            if (userInput == ConsoleKey.Backspace)
                            {
                                //check if there are enough letters to delete
                                if (kBIndex != 0)
                                {
                                    //move index curor back one space
                                    kBIndex--;

                                    //reset letter at deleted position
                                    userGuesses[kBIndex, roundNum] = ' ';
                                }
                                else
                                {
                                    //inform user that they can not delete since there are no letters
                                    userMessage = "There is nothing to delete!";
                                }
                            }
                            else if (userInput == ConsoleKey.Enter)
                            {
                                //add all characters in user guess to one string
                                for (int i = 0; i < userGuesses.GetLength(0); i++)
                                {
                                    //add character of user guess at index to string
                                    userGuess += userGuesses[i, roundNum];
                                }

                                //trim user guess of spaces
                                userGuess = userGuess.Trim();

                                //perform task depending on if guess has enough letters
                                if (userGuess.Length == userGuesses.GetLength(0))
                                {
                                    //perform task depending on if user guessed correctly
                                    if (userGuess.Equals(corWord))
                                    {
                                        //change gamestate to endgame
                                        gameState = ENDGAME;

                                        //set playing variable to false
                                        isPlaying = false;

                                        //set user win variable to true
                                        didUserWin = true;

                                        //add one to each stat
                                        winStreak++;
                                        gamesWon++;
                                        winGuessDis[roundNum]++;
                                        gamesPlayed++;

                                        //check if max win streak is larger then current win streak
                                        if (winStreak > maxWinStreak)
                                        {
                                            //set max win streak to current win streak
                                            maxWinStreak = winStreak;
                                        }

                                        //set the round won to current round number
                                        roundWon = roundNum;

                                        //set user guess validity to true
                                        isGuessValid = true;
                                    }
                                    else
                                    {
                                        //check if user guess is a valid word in wordle answers dictionary
                                        for (int i = 0; i < wordleAnswers.Length; i++)
                                        {
                                            //check if user guess is equal to word at index of wordle answer
                                            if (userGuess.Equals(wordleAnswers[i].ToUpper()))
                                            {
                                                //set guess validity to true
                                                isGuessValid = true;
                                            }
                                        }

                                        //check if the user guess is not valid
                                        if (!isGuessValid)
                                        {
                                            //check if user guess is a valid word in wordle extras dictionary
                                            for (int i = 0; i < wordleExtras.Length; i++)
                                            {
                                                //check if user guess is equal to word at index of wordle extra
                                                if (userGuess.Equals(wordleExtras[i].ToUpper()))
                                                {
                                                    //set guess validity to true
                                                    isGuessValid = true;
                                                }
                                            }
                                        }
                                    }

                                    //check if the user guess is valid
                                    if (isGuessValid)
                                    {
                                        //set new variable to equal the correct word
                                        corWordEdit = corWord;

                                        //find if user guess and correct word have the same letters at the same place
                                        for (int i = 0; i < corWord.Length; i++)
                                        {
                                            //check if correct word at index is equal to user guess at index
                                            if (corWord[i].Equals(userGuess[i]))
                                            {
                                                //set guess accuracies correct placement
                                                keyboardAcc[userGuess[i] - CAP_A_VAL] = COR_PLACE;
                                                tableAcc[i, roundNum] = COR_PLACE;

                                                //replace letter in correct word edit with space
                                                corWordEdit = corWordEdit.Remove(i, 1);
                                                corWordEdit = corWordEdit.Insert(i, " ");
                                            }
                                        }

                                        //find if letters in user guess exist in the correct word
                                        for (int i = 0; i < corWord.Length; i++)
                                        {
                                            //check if correct word contains user guess at index and user guess letter is not already in correct spot
                                            if (tableAcc[i, roundNum] != COR_PLACE && corWordEdit.Contains(Convert.ToString(userGuess[i])))
                                            {
                                                //variable to first location of letter in the correct word
                                                index = corWordEdit.IndexOf(Convert.ToString(userGuess[i]));

                                                //check if the keyboard guess accuracy at index is undefined
                                                if (keyboardAcc[userGuess[i] - CAP_A_VAL] == UNDEFINED)
                                                {
                                                    //set keyboard guess accuracy at index to wrong place
                                                    keyboardAcc[userGuess[i] - CAP_A_VAL] = WRONG_PLACE;
                                                }

                                                //set table accuracy at index and round number to wrong place
                                                tableAcc[i, roundNum] = WRONG_PLACE;

                                                //replace letter in correct word edit with space
                                                corWordEdit = corWordEdit.Remove(index, 1);
                                                corWordEdit = corWordEdit.Insert(index, " ");
                                            }
                                            else
                                            {
                                                //check if keyboard guess accuracy at letter is undefined
                                                if (keyboardAcc[userGuess[i] - CAP_A_VAL] == UNDEFINED)
                                                {
                                                    //set accuracy to wrong letter
                                                    keyboardAcc[userGuess[i] - CAP_A_VAL] = WRONG_LETTER;
                                                }

                                                //check if table guess accuracy at index is undefined
                                                if (tableAcc[i, roundNum] == UNDEFINED)
                                                {
                                                    //set table accuracy at index and round number to wrong letter
                                                    tableAcc[i, roundNum] = WRONG_LETTER;
                                                }
                                            }
                                        }

                                        //set guess validity to false
                                        isGuessValid = false;

                                        //reset typing index
                                        kBIndex = 0;

                                        //add one to round number 
                                        roundNum++;

                                        //check if user guessed the maxmimum amount of guesses
                                        if (roundNum == userGuesses.GetLength(1) && isPlaying)
                                        {
                                            //set boolean variables to false
                                            isPlaying = false;
                                            didUserWin = false;

                                            //set winstreak to zero
                                            winStreak = 0;

                                            //add one to games played
                                            gamesPlayed++;

                                            //set gamestate to endgame
                                            gameState = ENDGAME;
                                        }
                                    }
                                    else
                                    {
                                        //inform user that guess is not a word in the dictionaries
                                        userMessage = "Not in word list!";
                                    }
                                }
                                else
                                {
                                    //inform user that guess does not have enough letters
                                    userMessage = "Not enough letters!";
                                }
                            }
                            else
                            {
                                //check if user input is between A and Z
                                if (Convert.ToChar(userInput) >= CAP_A_VAL && Convert.ToChar(userInput) <= CAP_Z_VAL)
                                {
                                    //check if the guess has too many letters
                                    if (kBIndex < userGuesses.GetLength(0))
                                    {
                                        //add user input to user guesses array
                                        userGuesses[kBIndex, roundNum] = Convert.ToChar(userInput);

                                        //add one index location to user input
                                        kBIndex++;
                                    }
                                    else
                                    {
                                        //inform user that their guess has too many letters
                                        userMessage = "Too many letters!";
                                    }
                                }
                            }

                            //save user statistics data to file
                            SaveData(gamesPlayed, gamesWon, winStreak, maxWinStreak, winGuessDis, isPlaying, corWord, userGuesses, roundNum, tableAcc);
                        }
                        break;

                    case INSTRUCTIONS:
                        //display instructions title
                        DisplayInstruc(instructWords, instructSpecChar, instrucColors, instructMessages);

                        //print user prompt to user centered and coloured appropriately
                        ChangeColor("Enter ", "Enter M to return to menu", "M", " to return to menu");

                        //check if user input is M
                        if (Console.ReadKey().Key == ConsoleKey.M)
                        {
                            //set gamestate to menu
                            gameState = MENU;
                        }
                        break;

                    case ENDGAME:
                        //display finished game board
                        DisplayBoard(userGuesses, roundNum, userMessage, tableAcc);

                        //check if user won game
                        if (didUserWin)
                        {
                            //print winning message to user centered and coloured appropriately
                            CenterPrint("Correct answer!", "Correct answer!", true);
                        }
                        else
                        {
                            //print losing message to user centered and coloured appropriately
                            ChangeColor("Wrong answer! The correct word was ", "Wrong answer! The correct word was " + corWord + "!", corWord, "!");
                        }

                        //add line spacing and print user prompt centered and coloured appropriately
                        Console.WriteLine();
                        ChangeColor("Press ", "Press ENTER to see statistics", "ENTER", " to see statistics");

                        //check if user input is ENTER key
                        if (Console.ReadKey().Key == ConsoleKey.Enter)
                        {
                            //set gamestate to statistics
                            gameState = STATS;
                        }
                        break;

                    case STATS:
                        //display statistics title
                        PrintStatTitle();

                        //print user stats centered and coloured appropriately
                        ChangeColor("Games played:".PadRight(STAT_PAD, ' '), "Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), Convert.ToString(gamesPlayed), "");
                        ChangeColor("Games won:".PadRight(STAT_PAD, ' '), "Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), Convert.ToString(gamesWon), "");
                        ChangeColor("Current win streak:".PadRight(STAT_PAD, ' '), "Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), Convert.ToString(winStreak), "");
                        ChangeColor("Maximum win streak:".PadRight(STAT_PAD, ' '), "Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), Convert.ToString(maxWinStreak), "");

                        //check if user has played more than one game
                        if (gamesPlayed != 0)
                        {
                            //print win percentage centered and coloured appropriately
                            ChangeColor("Win percentage:".PadRight(STAT_PAD, ' '), "Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), Convert.ToString((int)((double)gamesWon / gamesPlayed * 100)) + "%", "");
                        }

                        //print win guess distribution chart
                        PrintWinGuessDis("Games played:".PadRight(STAT_PAD, ' ') + Convert.ToString(gamesPlayed), winGuessDis, roundWon);

                        //print spacing and user prompts to change gamestate centred and coloured appropriately
                        Console.WriteLine("\n\n");
                        ChangeColor("Enter ", "Enter P to play wordle", "R", " to reset stats");
                        ChangeColor("Enter ", "Enter P to play wordle", "M", " to return to menu");
                        ChangeColor("Enter ", "Enter P to play wordle", "P", " to play wordle");

                        //change game information depending on user input
                        switch (Console.ReadKey().Key)
                        {
                            case ConsoleKey.R:
                                //reset user stats
                                gamesPlayed = 0;
                                gamesWon = 0;
                                winStreak = 0;
                                maxWinStreak = 0;

                                //reset the round that user won
                                roundWon = -1;

                                //reset win guess distribution for each round
                                for (int i = 0; i < winGuessDis.Length; i++)
                                {
                                    //set win guess distribution at index to zero
                                    winGuessDis[i] = 0;
                                }

                                //save user data to file
                                SaveData(gamesPlayed, gamesWon, winStreak, maxWinStreak, winGuessDis, isPlaying, corWord, userGuesses, roundNum, tableAcc);
                                break;

                            case ConsoleKey.M:
                                //set gamestate to menu
                                gameState = MENU;

                                //reset round that user won
                                roundWon = -1;
                                break;

                            case ConsoleKey.P:
                                //set gamestate to gameplay
                                gameState = GAMEPLAY;

                                //reset round that user won
                                roundWon = -1;
                                break;
                        }
                        break;
                }
            }

            //clear screen and thank user for playing centred and coloured appropriately
            Console.Clear();
            Console.ForegroundColor = DARK_CYAN;
            CenterPrint("Thanks for playing!", "Thanks for playing!", false);
        }

        //Pre: userGuesses must be a 5 by 5 array of alphabet chars, round num must greater than or equal to zero, userMessage must be a valid string of 5 characters, and table acc must be a 5 by 5 array of dark blue, green, yellow or white
        //Post: None
        //Desc: Displays the wordle board and colours the letters accordingly
        private static void DisplayBoard(char[,] userGuesses, int roundNum, string userMessage, ConsoleColor[,] tableAcc)
        {
            //prints wordle title
            PrintGameTitle();

            //display the top of chart and centered appropriately
            CenterPrint("┏━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┓", "┏━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┓", true);

            //print each row of the chart
            for (int i = 0; i < userGuesses.GetLength(1); i++)
            {
                //print a blank row of chart centered appropriately
                CenterPrint("┃       ┃       ┃       ┃       ┃       ┃", "┃       ┃       ┃       ┃       ┃       ┃", true);

                //print spacing to centre next row
                CenterPrint("", "┃       ┃       ┃       ┃       ┃       ┃", false);

                //print each letter in row of chart with specified colour
                for (int x = 0; x < userGuesses.GetLength(0); x++)
                {
                    //print the start of the row
                    Console.Write("┃   ");

                    //check if i index value does not equal round number
                    if (i != roundNum)
                    {
                        //set foregroudn colour to table accuracy at index x and i
                        Console.ForegroundColor = tableAcc[x, i];
                    }

                    //display user guess letter and spacing and reset colours
                    Console.Write(userGuesses[x, i] + "   ");
                    Console.ResetColor();
                }

                //finish row
                Console.WriteLine("┃");

                //print an empty row centred appropriately
                CenterPrint("┃       ┃       ┃       ┃       ┃       ┃", "┃       ┃       ┃       ┃       ┃       ┃", true);

                //check if i is less than the last row
                if (i < userGuesses.GetLength(1) - 1)
                {
                    //prints line to join next row centred appropriately
                    CenterPrint("┣━━━━━━━╋━━━━━━━╋━━━━━━━╋━━━━━━━╋━━━━━━━┫", "┣━━━━━━━╋━━━━━━━╋━━━━━━━╋━━━━━━━╋━━━━━━━┫", true);
                }
            }

            //ends chart with finished lines centred appropriately
            CenterPrint("┗━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┛\n", "┗━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┛", true);

            //change colour to dark cyan
            Console.ForegroundColor = DARK_CYAN;

            //print message to user centred appropriately and reset colours
            CenterPrint(userMessage, userMessage, true);
            Console.ResetColor();
        }

        //Pre: keyBoardLines must be a valid array of strings with each index containing a row of a canadian english keyboard in order, keyboardAcc must contain white, green, dark blue or yellow for each letter in the alphabet, and kBRowToppers must be a valid string array with each index containing the lene for the top of the row
        //Post: None
        //Desc: Displays the wordle keyboard and colours the letters accordingly
        private static void DisplayKeyBoard(string[] keyBoardLines, ConsoleColor[] keyboardAcc, string[] kBRowToppers)
        {
            //prints spacing
            Console.WriteLine();

            //prints each row of the keyboard coloured appropriately
            for (int i = 0; i < keyBoardLines.Length; i++)
            {
                //prints top of each row centred appropriately
                CenterPrint(kBRowToppers[i], kBRowToppers[i], true);

                //prints spacing for next row to be centred
                CenterPrint("", kBRowToppers[i + 1], false);

                //prints row of keyboard centred coloured appropriately
                for (int x = 0; x < keyBoardLines[i].Length; x++)
                {
                    //prints start of row
                    Console.Write("┃ ");

                    //changes colour to accuracy for letter at index x of key board lines at index i
                    Console.ForegroundColor = keyboardAcc[Convert.ToChar(keyBoardLines[i][x]) - CAP_A_VAL];

                    //display the letter at index and adds spacing and reset colours
                    Console.Write(keyBoardLines[i][x] + " ");
                    Console.ResetColor();
                }

                //finishes each row
                Console.WriteLine("┃");
            }

            //displays bottom of keyboard centred appropriately
            CenterPrint(kBRowToppers[keyBoardLines.Length], kBRowToppers[keyBoardLines.Length], true);
        }

        //Pre: instructWords must be an array containing WEARY, READY and BLACK, instructSpecChar must be a an array containing 0, 1 and 4, instrucColors must be an array containing GREEN, YELLOW and DARK BLUE, and instructMessages must be an array of strings including three valid explanations for each color
        //Post: None
        //Desc: Display the instructions page to user
        private static void DisplayInstruc(string[] instructWords, int[] instructSpecChar, ConsoleColor[] instrucColors, string[] instructMessages)
        {
            //print instructions title
            PrintInstrucTitle();

            //print 3 main rules of wordle centered appropriately
            CenterPrint("Guess the Wordle in 6 tries\n", "Guess the Wordle in 6 tries", true);
            CenterPrint("- Each guess must be a valid 5-letter word", "- The colour of the tiles will change to show how close your guess was to the word", true);
            CenterPrint("- The colour of the tiles will change to show how close your guess was to the word\n\n", "- The colour of the tiles will change to show how close your guess was to the word", true);

            //display examples subheading centered appropriately
            CenterPrint("Examples", "Examples", true);

            //display instructions diagrams
            for (int i = 0; i < instructWords.Length; i++)
            {
                //display start of chart centres appropriately
                CenterPrint("┏━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┓", "┏━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┳━━━━━━━┓", true);
                CenterPrint("┃       ┃       ┃       ┃       ┃       ┃", "┃       ┃       ┃       ┃       ┃       ┃", true);

                //prints spacing to centre next line
                CenterPrint("", "┃       ┃       ┃       ┃       ┃       ┃", false);

                //begin line
                Console.Write("┃   ");

                //print all characters up to special character in row i
                for (int x = 0; x < instructSpecChar[i]; x++)
                {
                    //print character at index i, x and add vertical line
                    Console.Write(instructWords[i][x] + "   ┃   ");
                }

                //change text colour to instructions color at index i
                Console.ForegroundColor = instrucColors[i];

                //write special character and reset colours
                Console.Write(instructWords[i][instructSpecChar[i]] + "   ");
                Console.ResetColor();

                //print remaining charactesr in row i
                for (int x = instructSpecChar[i] + 1; x < instructWords[i].Length; x++)
                {
                    //print character at index i, x and add vertical line
                    Console.Write("┃   " + instructWords[i][x] + "   ");
                }

                //finish row
                Console.WriteLine("┃");

                //print bottom of chart centres appropriately
                CenterPrint("┃       ┃       ┃       ┃       ┃       ┃", "┃       ┃       ┃       ┃       ┃       ┃", true);
                CenterPrint("┗━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┛", "┗━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┛", true);

                //change text color to instructions color at index i
                Console.ForegroundColor = instrucColors[i];

                //print special character centred appropriately and reset colors
                CenterPrint(instructWords[i][instructSpecChar[i]] + " ", "┗━━━━━━━┻━━━━━━┻━━━━━━━┻━━━━━━━┻━━━━━━━┛", false);
                Console.ResetColor();

                //print remaining words in message at index i
                Console.WriteLine(instructMessages[i]);
            }
        }

        //Pre: centSpace is a string with proper length to space appropriately, winGuessDis is an array of intergers containing how many wins the user has for each round, roundWon is an interger containing which round the user won
        //Post: None
        //Desc: print the win guess distribution chart
        private static void PrintWinGuessDis(string centSpace, int[] winGuessDis, int roundWon)
        {
            //create variable to store largest win distribution
            int largestWinDis = 0;

            //add line spacing and print guess distrubtion title to user centered appropriately
            Console.WriteLine();
            CenterPrint("Guess Distribution", centSpace, true);

            //find the largest win guess distrbution
            for (int i = 0; i < winGuessDis.Length; i++)
            {
                //check if win guess at index is greater than previous largest win distribution
                if (winGuessDis[i] > largestWinDis)
                {
                    //set largest win distribution to win guess distrubution at index
                    largestWinDis = winGuessDis[i];
                }
            }

            //print win guess distrubution for each round
            for (int i = 0; i < winGuessDis.Length; i++)
            {
                //print round number centred appropriately
                CenterPrint(i + 1 + ": ", centSpace, false);

                //check if the index is equal to the round the user won
                if (i == roundWon)
                {
                    //set background colour to dark green
                    Console.BackgroundColor = DARK_GREEN;
                }
                else
                {
                    //set background colour to dark cyan
                    Console.BackgroundColor = DARK_CYAN;
                }

                //print space before number
                Console.Write(" ");

                //check if at index the user has not won
                if (winGuessDis[i] == 0)
                {
                    //print the amount of wins at index with extra space to the right
                    Console.Write(winGuessDis[i]);
                }
                else
                {
                    //print the amount of wins at index with bar quantity bar behind it
                    Console.Write(Convert.ToString(winGuessDis[i]).PadLeft(winGuessDis[i] * STAT_PAD / largestWinDis));
                }

                //print space after number and reset colors
                Console.WriteLine(" ");
                Console.ResetColor();
            }
        }

        //Pre: printWord and padWord must be a valid string, isWriteLine must be a valid boolean
        //Post: None
        //Desc: centres text with specified requests
        private static void CenterPrint(string printWord, string padWord, bool isWriteLine)
        {
            //print word in centre of screem
            Console.Write("".PadLeft((Console.WindowWidth - padWord.Length) / 2) + printWord);

            //checks if writeline is true
            if (isWriteLine)
            {
                //prints a break between the lines
                Console.WriteLine();
            }
        }

        //Pre: sentStart, padWord, colorWords and sentRemain must be valid strings
        //Post: None
        //Desc: prints a centered sentence coloured at a specific location in it
        private static void ChangeColor(string sentStart, string padWord, string colorWords, string sentRemain)
        {
            //prints start of word centred
            CenterPrint(sentStart, padWord, false);

            //changes colour to dark cyan
            Console.ForegroundColor = DARK_CYAN;

            //prints the coloured word(s) and resets colour
            Console.Write(colorWords);
            Console.ResetColor();

            //prinst the rest of the sentence
            Console.WriteLine(sentRemain);
        }

        //Pre: None
        //Post: None
        //Desc: Displays the coloured statistics title
        private static void PrintStatTitle()
        {
            //change text colour to dark cyan
            Console.ForegroundColor = DARK_CYAN;

            //display statistics title
            CenterPrint("     _        _   _     _   _          ", "     _        _   _     _   _          ", true);
            CenterPrint(" ___| |_ __ _| |_(_)___| |_(_) ___ ___ ", " ___| |_ __ _| |_(_)___| |_(_) ___ ___ ", true);
            CenterPrint(@"/ __| __/ _` | __| / __| __| |/ __/ __|", @"/ __| __/ _` | __| / __| __| |/ __/ __|", true);
            CenterPrint(@"\__ \ || (_| | |_| \__ \ |_| | (__\__ \", @"\__ \ || (_| | |_| \__ \ |_| | (__\__ \", true);
            CenterPrint(@"|___/\__\__,_|\__|_|___/\__|_|\___|___/", @"|___/\__\__,_|\__|_|___/\__|_|\___|___/", true);

            //reset colour and add line spacing
            Console.ResetColor();
            Console.WriteLine();
        }

        //Pre: None
        //Post: None
        //Desc: Displays the coloured wordle title
        private static void PrintGameTitle()
        {
            //change text colour to dark cyan
            Console.ForegroundColor = DARK_CYAN;

            //display wordle title
            CenterPrint("     __    __              _ _          ", "      __    __              _ _          ", true);
            CenterPrint(@"    / / /\ \ \___  _ __ __| | | ___     ", @"     / / /\ \ \___  _ __ __| | | ___     ", true);
            CenterPrint(@"    \ \/  \/ / _ \| '__/ _` | |/ _ \    ", @"     \ \/  \/ / _ \| '__/ _` | |/ _ \    ", true);
            CenterPrint(@"     \  /\  / (_) | | | (_| | |  __/    ", @"      \  /\  / (_) | | | (_| | |  __/    ", true);
            CenterPrint(@"      \/  \/ \___/|_|  \__,_|_|\___|    ", @"       \/  \/ \___/|_|  \__,_|_|\___|    ", true);

            //reset colorus and add line spacing
            Console.ResetColor();
            Console.WriteLine();
        }

        //Pre: None
        //Post: None
        //Desc: Displays the coloured instructions title 
        private static void PrintInstrucTitle()
        {
            //change text colour to dark cyan
            Console.ForegroundColor = DARK_CYAN;

            //displays instructions title
            CenterPrint(" _           _                   _   _                 ", " _           _                   _   _                 ", true);
            CenterPrint("(_)_ __  ___| |_ _ __ _   _  ___| |_(_) ___  _ __  ___ ", "(_)_ __  ___| |_ _ __ _   _  ___| |_(_) ___  _ __  ___ ", true);
            CenterPrint(@"| | '_ \/ __| __| '__| | | |/ __| __| |/ _ \| '_ \/ __|", @"| | '_ \/ __| __| '__| | | |/ __| __| |/ _ \| '_ \/ __|", true);
            CenterPrint(@"| | | | \__ \ |_| |  | |_| | (__| |_| | (_) | | | \__ \", @"| | | | \__ \ |_| |  | |_| | (__| |_| | (_) | | | \__ \", true);
            CenterPrint(@"|_|_| |_|___/\__|_|   \__,_|\___|\__|_|\___/|_| |_|___/", @"|_|_| |_|___/\__|_|   \__,_|\___|\__|_|\___/|_| |_|___/", true);

            //reset colorus and add line spacing
            Console.ResetColor();
            Console.WriteLine();
        }

        //Pre: gamesPlayed, gamesWon, winStreak, maxWinsStreak and roundNum must have positive interger values, winGuessDis must be an array of intergers representing how many wins the user has at each round, isPlaying must be a valid bool, corWord must contain the correct 5 letter word, userGuesses must be a 5 x 5 array with valid user guesses, tableAcc must contain valid colours to represent accuracy of guesses
        //Post: None
        //Desc: Saves user data to file
        private static void SaveData(int gamesPlayed, int gamesWon, int winStreak, int maxWinStreak, int[] winGuessDis, bool isPlaying, string corWord, char[,] userGuesses, int roundNum, ConsoleColor[,] tableAcc)
        {
            try
            {
                //store txt file in variable
                outFile = File.CreateText("UserStats.txt");

                //write user statistics to file
                outFile.WriteLine(gamesPlayed);
                outFile.WriteLine(gamesWon);
                outFile.WriteLine(winStreak);
                outFile.WriteLine(maxWinStreak);

                //write all but the last win guess distribution values to file
                for (int i = 0; i < winGuessDis.Length - 1; i++)
                {
                    //write win guess distribution at index to file
                    outFile.Write(winGuessDis[i] + ",");
                }

                //write the last win guess distribution value to file
                outFile.WriteLine(winGuessDis[winGuessDis.Length - 1]);

                //write if game is currently playing to file
                outFile.WriteLine(isPlaying);

                //write the round num to file
                outFile.WriteLine(roundNum);

                //check if isPlaying variable is true and round num doesnt equal zero
                if (isPlaying && roundNum != 0)
                {
                    //write the correct word to file
                    outFile.WriteLine(corWord);

                    //write columns of userguesses and table accuracies up to round number to file
                    for (int i = 0; i < roundNum; i++)
                    {
                        //write all rows but the last of userguesses and table accuracies to file
                        for (int x = 0; x < userGuesses.GetLength(0) - 1; x++)
                        {
                            //write user guess letter and table accuracy at index x, i split with a : and ended with a ,
                            outFile.Write(userGuesses[x, i] + ":" + tableAcc[x, i] + ",");
                        }

                        //write the final index of user guess letter and table accuracy at index x, i split with a :
                        outFile.WriteLine(userGuesses[userGuesses.GetLength(0) - 1, i] + ":" + tableAcc[tableAcc.GetLength(0) - 1, i]);
                    }
                }

            }
            catch (Exception)
            {
                //inform user of the error that occured and promt to continue centered and coloured appropriately
                ChangeColor("An error occured and your data could not be saved. Press ", "An error occured and your data could not be saved. Press ENTER to continue!", "ENTER", " to continue!");
                Console.ReadLine();
            }

            //check if outfile is not null
            if (outFile != null)
            {
                //close the file
                outFile.Close();
            }
        }
    }
}