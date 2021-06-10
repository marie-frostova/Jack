using System;
using System.Collections.Generic;

namespace BlackJack
{
    class Program
    {
        static void Main(string[] args)
        {
            //Variable declaration
            Console.Write("Insert a name: ");
            var playerName = Console.ReadLine();
            var play = true;
            while (play == true)
            {
                var game = new Game();
                game.Play(playerName);
                Console.WriteLine("Press ENTER to continue");
                Console.ReadLine();
                Console.Clear();
                //ask for replay
                Console.Write("Do you want to play again? yes/no: ");
                var answer = true;
                var ans = Console.ReadLine();
                //Looping until a correct answer is given
                while (answer)
                {
                    if (ans == "y" || ans == "Y" || ans == "yes" || ans == "YES")
                    {
                        play = true;
                        answer = false;
                    }
                    else if (ans == "n" || ans == "N" || ans == "no" || ans == "NO")
                    {
                        play = false;
                        answer = false;
                    }
                    else
                    {
                        Console.Write("error, unexpected input\nPlease type y or n ");
                        ans = Console.ReadLine();
                    }
                }
                Console.Clear();
            }
        }
    }
}
