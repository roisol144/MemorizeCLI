﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemorizeCLI
{
    internal class GameInterface
    {
        private const string k_QuitGame = "Q";
        private const string k_RestartGame = "R";
        private const int k_DefaultNumOfRows = 4;
        private const int k_DefaultNumOfColumns = 4;
        private const int k_MaxMatrixRows = 6;
        private const int k_MaxMatrixColumns = 6;
        private const int k_MinMatrixRows = 4;
        private const int k_MinMatrixColumns = 4;
        private readonly GameMenu r_GameMenu;
        private GameLogicManager m_GameLogicManager;

        public static int MaxMatrixRows
        {
            get
            {
                return k_MaxMatrixRows;
            }
        }

        public static int MaxMatrixColumns
        {
            get
            {
                return k_MaxMatrixColumns;
            }
        }

        public static int MinMatrixRows
        {
            get
            {
                return k_MinMatrixRows;
            }
        }

        public static int MinMatrixColumns
        {
            get
            {
                return k_MinMatrixColumns;
            }
        }




        public GameInterface()
        {
            r_GameMenu = new GameMenu();
        }
        /* ----------------------------------------------- */

        public void StartGame()
        {
            runMenu();
            runGame();
        }
        /* ----------------------------------------------- */
        public void RestartGame()
        {
            int newNumOfRows = k_DefaultNumOfRows;
            int newNumOfColumns = k_DefaultNumOfColumns;
            m_GameLogicManager.GameDataManager.GameStatus = eGameStatus.CurrentlyRunning;
            Ex02.ConsoleUtils.Screen.Clear();
            r_GameMenu.GetAndValidateMatrixDimensions(out newNumOfRows, out newNumOfColumns);
            m_GameLogicManager.ResetGameLogic(newNumOfRows, newNumOfColumns);
            runGame();
        }
        /* ----------------------------------------------- */

        private void displayWinnerMessage()
        {
            string winnerMessage;
            if (m_GameLogicManager.FirstPlayerScore == m_GameLogicManager.SecondPlayerScore)
            {
                Console.WriteLine("\n No Win Today. It's A Tie...\n");
            }
            else
            {
                string winnerName = m_GameLogicManager.FirstPlayerScore > m_GameLogicManager.SecondPlayerScore
                                  ? m_GameLogicManager.GameDataManager.FirstPlayer.PlayerName
                                  : m_GameLogicManager.GameDataManager.SecondPlayer.PlayerName;
                winnerMessage = string.Format("\n Congratulations {0} You Are The Winner ! \n", winnerName);
                Console.WriteLine(winnerMessage);
            }
        }
        /* ----------------------------------------------- */

        private void runGame()
        {
            string playerInput = "";
            string restartMessage;

            while (m_GameLogicManager.GameDataManager.GameStatus == eGameStatus.CurrentlyRunning)
            {
                displayGameInterface();
                playerInput = getPlayerNextMove();
                updateTurnAndView(playerInput.ToUpper());
            }
            displayWinnerMessage();
            if (playerInput.ToUpper() == k_QuitGame)
            {
              exitGame();
            }
            else //Game finished without an early exit.
            {
                restartMessage = string.Format("Do you Want To Play Again? Press {0}, Otherwise press any key.",
                    k_RestartGame);
                Console.WriteLine(restartMessage);

                playerInput = Console.ReadLine();
                if (playerInput.ToUpper() == k_RestartGame)
                {
                    RestartGame();
                }
                else
                {
                    exitGame();
                }
            }

        }
        /* ----------------------------------------------- */

        private void updateTurnAndView(string i_PlayerInput)
        {
            if (i_PlayerInput != k_QuitGame)
            {
                BoardTile selectedTile = m_GameLogicManager.GameDataManager.GameBoard.GetTile(i_PlayerInput);
                m_GameLogicManager.UpdateTurn(ref selectedTile);
                displayGameInterface();
                if (m_GameLogicManager.IsFirstSelection && !m_GameLogicManager.AreMatchingTiles)
                {
                    Console.ForegroundColor = ConsoleColor.Red; // Change text color to green
                    Console.WriteLine("No Match This Time. Don't give up !");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(2000);
                    m_GameLogicManager.TogglePlayer();
                }
                m_GameLogicManager.AreMatchingTiles = false;
            }
            else
            {
                m_GameLogicManager.GameDataManager.GameStatus = eGameStatus.Over;
            }
        }

        /* ----------------------------------------------- */
        private void exitGame()
        {
            Console.WriteLine("Thanks For Playing. Have A Nice Day!");
        }
        /* ----------------------------------------------- */

        private void runMenu()
        {
            string firstPlayerName, secondPlayerName;
            int columns, rows;

            eGameType gameType =
                r_GameMenu.RunMenuScreen(out firstPlayerName, out secondPlayerName, out rows, out columns);
            Player firstPlayer = new Player(firstPlayerName, ePlayerType.Human);

            ePlayerType secondPlayerType = gameType == eGameType.HumanVComputer ? ePlayerType.Computer :
                 ePlayerType.Human;

            Player secondPlayer = new Player(secondPlayerName, secondPlayerType);
            m_GameLogicManager = new GameLogicManager(firstPlayer, secondPlayer, rows, columns, gameType);
        }

        /* ----------------------------------------------- */

        private void displayGameInterface()
        {
            Ex02.ConsoleUtils.Screen.Clear();
            string currentPlayerToPlayMessage;

            string firstPlayerName = m_GameLogicManager.GameDataManager.FirstPlayer.PlayerName;
            string firstPlayerPoints = m_GameLogicManager.GameDataManager.FirstPlayer.PlayerPoints.ToString();
            string secondPlayerName = m_GameLogicManager.GameDataManager.SecondPlayer.PlayerName;
            string secondPlayerPoints = m_GameLogicManager.GameDataManager.SecondPlayer.PlayerPoints.ToString();

            int nameColumnWidth = Math.Max(firstPlayerName.Length, secondPlayerName.Length) + 2;
            int pointsColumnWidth = Math.Max(firstPlayerPoints.Length, secondPlayerPoints.Length) + 2;
            int totalWidth = nameColumnWidth + pointsColumnWidth + 7;

            string borderLine = new string('=', totalWidth);
            string separatorLine = new string('-', totalWidth);

            string scoreBoard = string.Format(@"
   SCORE BOARD:
  ||{0}||
  ||  {1} | {2}  ||
  ||{3}||
  ||  {4} | {5}  ||
  ||{6}||
",
                borderLine,
                firstPlayerName.PadRight(nameColumnWidth),
                firstPlayerPoints.PadRight(pointsColumnWidth),
                separatorLine,
                secondPlayerName.PadRight(nameColumnWidth),
                secondPlayerPoints.PadRight(pointsColumnWidth),
                borderLine
            );

            Console.WriteLine(scoreBoard);
            currentPlayerToPlayMessage = string.Format("\n {0}'s Turn", m_GameLogicManager.GameDataManager.CurrentPlayer.PlayerName);
            Console.WriteLine(currentPlayerToPlayMessage);
            m_GameLogicManager.GameDataManager.GameBoard.DisplayBoard();
        }


        /* ----------------------------------------------- */

        private string getPlayerNextMove()
        {
            string playerNextMove = "";

            if (m_GameLogicManager.GameDataManager.CurrentPlayer.PlayerType == ePlayerType.Human)
            {
                playerNextMove = getInputFromHumanPlayer();
            }
            else
            {
                playerNextMove = m_GameLogicManager.GetAiNextMove();
                displayComputerMessage();
            }

            return playerNextMove;
        }
        /* ----------------------------------------------- */

        private void displayComputerMessage()
        {

            if (!m_GameLogicManager.ComputerHasMatch)
            {
                Console.Write("The computer is deep in thought");
                for (int i = 0; i < 3; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Write('.');
                }
                Console.WriteLine(); 
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green; 
                Console.Write("Eureka! The computer has found a match!");
                System.Threading.Thread.Sleep(2000);
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        /* ----------------------------------------------- */

        private string getInputFromHumanPlayer()
        {
            string playerNextMove = "";
            string currrentPlayerToPlayMessage;

            bool isNextMoveIsValid = false;

            while (!isNextMoveIsValid)
            {
                currrentPlayerToPlayMessage = string.Format("Now {0} has to choose next Move", m_GameLogicManager.GameDataManager.CurrentPlayer.PlayerName);
                Console.WriteLine(currrentPlayerToPlayMessage);
                playerNextMove = Console.ReadLine();
                isNextMoveIsValid = validateHumanPlayerNextMove(playerNextMove);
            }

            return playerNextMove;
        }
        /* ----------------------------------------------- */

        private bool validateHumanPlayerNextMove(string i_PlayerNextMove)
        {
            bool isNextMoveIsValid = true;

            if (i_PlayerNextMove != null)
            {
                i_PlayerNextMove = i_PlayerNextMove.ToUpper();

                if (i_PlayerNextMove != k_QuitGame)
                {

                    isNextMoveIsValid = validateTileHumanPlayerPicked(i_PlayerNextMove);

                    if (isNextMoveIsValid)
                    {
                        isNextMoveIsValid = validateTileIsNotHidden(i_PlayerNextMove);
                    }
                }

            }

            return isNextMoveIsValid;
        }
        /* ----------------------------------------------- */

        private bool validateTileIsNotHidden(string i_TileHumanPlayerPicked)
        {
            bool isHiddenTile = true;

            if (m_GameLogicManager.GameDataManager.GameBoard.GetTile(i_TileHumanPlayerPicked).IsRevealed)
            {
                Console.WriteLine("Wrong Input. You Picked A Tile That Is Already Revealed!\n");
                isHiddenTile = false;
            }

            return isHiddenTile;
        }
        /* ----------------------------------------------- */

        private bool validateTileHumanPlayerPicked(string i_TileHumanPlayerPicked)
        {
            bool isValidTileChoice;

            if (i_TileHumanPlayerPicked.Length != 2)
            {
                Console.WriteLine("Wrong Input. Input Should Look Like: A2\n");
                isValidTileChoice = false;
            }
            else
            {
                char letterColumns = i_TileHumanPlayerPicked[0];
                char digitRow = i_TileHumanPlayerPicked[1];
                isValidTileChoice = validateColumnLetter(letterColumns) && validateRowDigit(digitRow);
            }

            return isValidTileChoice;
        }
        /* ----------------------------------------------- */

        private bool validateRowDigit(char i_ChosenRow)
        {
            string wrongRowInputMessage;
            bool isValidRowDigit = true;
            char largerstValidDigit = (char)('0' + m_GameLogicManager.GameDataManager.NumOfRows);

            if (i_ChosenRow < '1' || i_ChosenRow > largerstValidDigit)
            {
                wrongRowInputMessage = string.Format("Wrong Input. Enter Row Between {0}-{1}", 1,
                    m_GameLogicManager.GameDataManager.NumOfRows);
                Console.WriteLine(wrongRowInputMessage);
                isValidRowDigit = false;
            }

            return isValidRowDigit;
        }
        /* ----------------------------------------------- */

        private bool validateColumnLetter(char i_ChosenColumn)
        {
            string wrongColumnInputMessage;
            bool isValidLetterColumn = true;
            char maxValidLetter = (char)('A' + m_GameLogicManager.GameDataManager.NumOfColumns - 1);

            if (i_ChosenColumn < 'A' || i_ChosenColumn > maxValidLetter)
            {
                wrongColumnInputMessage = string.Format("Wrong Input. Enter Column Between {0}-{1}", 'A',
                    maxValidLetter);
                Console.WriteLine(wrongColumnInputMessage);
                isValidLetterColumn = false;
            }

            return isValidLetterColumn;
        }
        /* ----------------------------------------------- */
    }

}
