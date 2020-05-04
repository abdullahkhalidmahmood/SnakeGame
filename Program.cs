﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Data;

namespace Snake
{
    /// <summary>
    /// Define a structure for the position for every object in the game by row and column
    /// </summary>
    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }


    class Program
    {
        /// <summary>
        /// function for playing background music
        /// </summary>
        public void BackgroundMusic()
        {
            //Create SoundPlayer objbect to control sound playback
            SoundPlayer backgroundMusic = new SoundPlayer();
            
            //Locate the SoundPlayer to the correct sound directory
            backgroundMusic.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeBGM_1.wav";
            
            //Play the background music at the beginning
            backgroundMusic.PlayLooping();
        }

        /// <summary>
        /// function to play sound effect when game over
        /// </summary>
        public void LoseSoundEffect()
        {
            SoundPlayer playerLose = new SoundPlayer();
            playerLose.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeLose_1.wav";
            playerLose.Play(); //Play the die sound effect after player died
        }
        /// <summary>
        /// function to play the sound effect when player win
        /// </summary>
        public void WinSoundEffect()
        {
            SoundPlayer playerWin = new SoundPlayer();
            playerWin.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeWin_1.wav";
            playerWin.Play(); 
        }

        /// <summary>
        /// function to draw food in yellow "@" 
        /// </summary>
        public void DrawFood()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("@");
        }
        
        /// <summary>
        /// function to draw obstacle in cyan "="
        /// </summary>
        public void DrawObstacle()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("=");
        }

        /// <summary>
        /// function to draw snake body in dark grey "*"
        /// </summary>
        public void DrawSnakeBody()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("*");
        }
        
        /// <summary>
        /// array to store directions, elements in game move depends on user direction key input
        /// </summary>
        /// <param name="directions"></param>
        public void Direction(Position[] directions)
        {
            
            directions[0] = new Position(0, 1);
            directions[1] = new Position(0, -1);
            directions[2] = new Position(1, 0);
            directions[3] = new Position(-1, 0);

        }

        /// <summary>
        /// function to place first 5 obstacles randomly when game start
        /// </summary>
        /// <param name="obstacles"></param>
       public void InitialRandomObstacles(List<Position>obstacles)
        {
            //Create obstacles objects and initialise certain random position of obstacles at every game play
            //The randomise obstacles will not exist in the first row at the beginning.
            Random randomNumbersGenerator = new Random();
            obstacles.Add(new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(2, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            
            //Show the obstacle in the windows with marking of "="
            foreach (Position obstacle in obstacles)
            {
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                DrawObstacle();
            }
        }

        /// <summary>
        /// function for reading user direction key input
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="right"></param>
        /// <param name="left"></param>
        /// <param name="down"></param>
        /// <param name="up"></param>
        public void CheckUserInput(ref int direction, byte right, byte left, byte down,byte up)
        {
            
            //User key pressed statement: depends on which direction the user want to go to get food or avoid obstacle
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo userInput = Console.ReadKey();
                if (userInput.Key == ConsoleKey.LeftArrow)
                {
                    if (direction != right) direction = left;
                }
                if (userInput.Key == ConsoleKey.RightArrow)
                {
                    if (direction != left) direction = right;
                }
                if (userInput.Key == ConsoleKey.UpArrow)
                {
                    if (direction != down) direction = up;
                }
                if (userInput.Key == ConsoleKey.DownArrow)
                {
                    if (direction != up) direction = down;
                }
            }
        }
        
        /// <summary>
        /// Check for Game over requirements, if snake eats itself, hit on obstacle or did not eat 3 food within 30 seconds, game over
        /// </summary>
        /// <param name="currentTime"></param>
        /// <param name="snakeElements"></param>
        /// <param name="snakeNewHead"></param>
        /// <param name="negativePoints"></param>
        /// <param name="obstacles"></param>
        /// <returns></returns>
        public int GameOverCheck(int currentTime, Queue<Position> snakeElements, Position snakeNewHead,int negativePoints, List<Position> obstacles)
        {
            if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead) || (Environment.TickCount-currentTime) > 30000)
            {
                LoseSoundEffect(); //this sound effect will be play if game over
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Red;//Text color for game over
               
                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
                userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;
                
                //Display game over text and points
                PrintLinesInCenter("Game Over!", "Your points are:" + userPoints, "Press enter to exit the game!");
                
                SavePointsToFile(userPoints);//saving points to files

                //close only when enter key is pressed
                while (Console.ReadKey().Key != ConsoleKey.Enter) {}
                return 1;
                
            }
            
            return 0;
        }

        /// <summary>
        /// Add winning requirement, snake eat 3 food within 30 seconds to win the game
        /// </summary>
        /// <param name="snakeElements"></param>
        /// <param name="negativePoints"></param>
        /// <returns></returns>
        public int WinningCheck(Queue<Position> snakeElements, int negativePoints)
        {
            // initially snake elements has 4, increment 1 by eating 1 food, so eat 3 food to get 7 snake elements 
            if (snakeElements.Count == 7)
            {
                WinSoundEffect(); //thissound effect plays when game won  
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Green;//Text color for game won

                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
                userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;
                
                //display game won text and user points
                PrintLinesInCenter("You Win!", "Your points are:" + userPoints, "Press enter to exit the game!");
                SavePointsToFile(userPoints);//saving points to files

                //close only when enter key is pressed
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// function to generate new food randomly with conditions 
        /// </summary>
        /// <param name="food"></param>
        /// <param name="snakeElements"></param>
        /// <param name="obstacles"></param>
        public void GenerateFood(ref Position food, Queue<Position> snakeElements, List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();
            do
            {
                food = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), //Food generated within console height
                    randomNumbersGenerator.Next(0, Console.WindowWidth)); //Food generate within console width
            }
            //a loop is created - while the program contains food and the obstacle is not hit 
            //put food on different position which is "@"
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            DrawFood();
        }

        /// <summary>
        /// function to generate new obstacle randomly if conditions
        /// </summary>
        /// <param name="food"></param>
        /// <param name="snakeElements"></param>
        /// <param name="obstacles"></param>
        public void GenerateNewObstacle(ref Position food, Queue<Position> snakeElements, List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();

            Position obstacle = new Position();
            do
            {
                obstacle = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
                    randomNumbersGenerator.Next(1, Console.WindowWidth));
            }
            //if snake or obstacles are already at certain position, new obstacle will not be drawn there
            //new obstacle will not be drawn at the same row & column of food
            while (snakeElements.Contains(obstacle) || obstacles.Contains(obstacle) || (food.row == obstacle.row && food.col == obstacle.col));
            obstacles.Add(obstacle);
            Console.SetCursorPosition(obstacle.col, obstacle.row);
            DrawObstacle();
        }

        /// <summary>
        /// to get the user points and save the value to text file
        /// </summary>
        /// <param name="userPoints"></param>
        public void SavePointsToFile(int userPoints)
        {
            //declare the file path
            String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            try
            {
                //if there is no such text file in the folder, it will be created before saving the points into it
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                    File.WriteAllText(filePath, userPoints.ToString() + Environment.NewLine);
                }
                else
                {
                    //if there are points exist in the text file, new points will be saved in next line
                    File.AppendAllText(filePath, userPoints.ToString() + Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("{0} Exception caught.", exception);
            }
        }

        /// <summary>
        /// Read the user points from text file
        /// </summary>
        public string ReadPointsFromFile()
        {
            //declare file path 
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            //read all the contents in text file and store in an array
            string[] scoreBoard = File.ReadAllLines(filePath);
            //find the highest points from the array
            int max = scoreBoard.Select(int.Parse).Max();
            // convert integer to string 
            string highestPoint = max.ToString();
            return highestPoint;
        }

        /// <summary>
        /// function to print text in center of game screen
        /// </summary>
        /// <param name="lines"></param>
        private static void PrintLinesInCenter(params string[] lines)
        {
            int verticalStart = (Console.WindowHeight - lines.Length) / 2; //  printing the lines
            int verticalPosition = verticalStart;
            foreach (var line in lines)
            {
                // start printing the line text horizontally
                int horizontalStart = (Console.WindowWidth - line.Length) / 2;
                // start position for this line of text
                Console.SetCursorPosition(horizontalStart, verticalPosition);
                // write the text
                Console.Write(line);
                // move to the next line
                ++verticalPosition;
            }
        }

        /// <summary>
        /// To display welcome message and highest score at start of game
        /// </summary>
        public void DisplayStartScreen()
        {
            Console.ForegroundColor = ConsoleColor.Cyan; //text color for text display
            //display welcome message and highest score 
            PrintLinesInCenter("WELCOME TO SNAKE GAME", "\n", "Highest Score", ReadPointsFromFile()); 
            //start screen stay for 3 seconds
            Thread.Sleep(3000);
            //start screen clear before game start
            Console.Clear();
        }

        public void PrintUserPoint(int userPoints, Queue<Position> snakeElements, int negativePoints)
        {
            userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
            userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;
            Console.SetCursorPosition(40, 0);
            Console.WriteLine("                  ");
            Console.SetCursorPosition(40, 0);
            Console.WriteLine("Score: {0}", userPoints);
        }


        /// <summary>
        /// Main starts here
        /// </summary>
        //Define direction by using index number
        //Set the time taken for the food to be dissappear
        //Initialise negative points
        static void Main(string[] args)
        {
            
            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int currentTime = Environment.TickCount;
            int lastFoodTime = 0;
            int foodDissapearTime = 10000; //food dissappears after 10 second 
            int negativePoints = 0;
            int userPoints = 0;
            Position[] directions = new Position[4];

            Console.SetWindowSize(56, 38);
            Program p = new Program();
            //display start screen before background music and game start 
            p.DisplayStartScreen();
            //Play background music
            p.BackgroundMusic();

            // Define direction with characteristic of index of array
            p.Direction(directions);

            // Initialised the obstacles location at the starting of the game
            List<Position> obstacles = new List<Position>();
            p.InitialRandomObstacles(obstacles);

            //Do the initialization for sleepTime (Game's Speed), Snake's direction and food timing
            //Limit the number of rows of text accessible in the console window
            double sleepTime = 100;
            int direction = right;
            Random randomNumbersGenerator = new Random();
            Console.BufferHeight = Console.WindowHeight;
            lastFoodTime = Environment.TickCount;

            //Initialise the snake position in top left corner of the windows
            //Havent draw the snake elements in the windows yet. Will be drawn in the code below
            Queue<Position> snakeElements = new Queue<Position>();
            for (int i = 0; i <= 3; i++) // Length of the snake was reduced to 3 units of *
            {
                snakeElements.Enqueue(new Position(1, i));
            }

            //To position food randomly when the program runs first time
            Position food = new Position();
            p.GenerateFood(ref food,snakeElements,obstacles);
            
            //while the game is running position snake on terminal with shape "*"
            foreach (Position position in snakeElements)
            {
                Console.SetCursorPosition(position.col, position.row);
                p.DrawSnakeBody();
            }

            while (true)
            {
                
                //negative points is initialized as 0 at the beginning of the game. As the player reaches out for food
                //negative points increment depending how far the food is
                negativePoints++;            

                //Check the user input direction
                p.CheckUserInput(ref direction, right, left, down, up);
              
                //When the game starts the snake head is towards the end of his body with face direct to start from right.
                Position snakeHead = snakeElements.Last();
                Position nextDirection = directions[direction];

                //Snake position to go within the terminal window assigned.
                Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                    snakeHead.col + nextDirection.col);

                if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;
                if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;
                if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;

                p.PrintUserPoint(userPoints, snakeElements, negativePoints);

                //Check for GameOver Criteria
                int gameOver=p.GameOverCheck(currentTime, snakeElements, snakeNewHead, negativePoints,obstacles);
                if (gameOver == 1)
                    return;

                //Check for Winning Criteria
                int winning = p.WinningCheck(snakeElements, negativePoints);
                if (winning == 1) return;

                //The way snake head will change as the player changes his direction
                Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                p.DrawSnakeBody();

                //Snake head shape when the user presses the key to change his direction
                snakeElements.Enqueue(snakeNewHead);
                Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (direction == right) Console.Write(">"); //Snake head when going right
                if (direction == left) Console.Write("<");//Snake head when going left
                if (direction == up) Console.Write("^");//Snake head when going up
                if (direction == down) Console.Write("v");//Snake head when going down


                // food will be positioned randomly until they are not at the same row & column as snake head
                if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                {
                    Console.Beep();// Make a sound effect when food was eaten.
                    p.GenerateFood(ref food, snakeElements, obstacles);
                    

                    //when the snake eat the food, the system tickcount will be set as lastFoodTime
                    //new food will be drawn, snake speed will increases
                    lastFoodTime = Environment.TickCount;
                    sleepTime--;

                    //Generate new obstacle
                    p.GenerateNewObstacle(ref food,snakeElements,obstacles);
                    
                }
                else
                {
                    // snake is moving
                    Position last = snakeElements.Dequeue();
                    Console.SetCursorPosition(last.col, last.row);
                    Console.Write(" ");
                }

                //if snake did not eat the food before it disappears, 50 will be added to negative points
                //draw new food after the previous one disappeared
                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    negativePoints = negativePoints + 50;
            
                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");

                    //Generate the new food and record the system tick count
                    p.GenerateFood(ref food, snakeElements, obstacles);
                    lastFoodTime = Environment.TickCount;
                }
                
                //snake moving speed increased 
                sleepTime -= 0.01;

                //pause the execution thread of snake moving speed
                Thread.Sleep((int)sleepTime);
                
            }
            
        }
    }
}