using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    class Game
    {
        byte status = (byte)Status.gameOn;
        House house = new House();
        Player player1 = new Player("");
        Card deck = new Card();

        //player hits as many times as he wants when < 21
        public void PlayerHit()
        {
            player1.CheckIfBust();
            if (!player1.Bust)
            {
                Console.Write("{0} Hits", player1.Name);
                player1.hand.Add(deck.DealCard(false));
                player1.GetTotal();
            }
        }

        //Initial cards
        public void InitialHits()
        {
            PlayerHit();
            PlayerHit();

            house.CheckIfBust();
            if (!house.Bust)
            {
                Console.Write("The House Hits");
                house.hand.Add(deck.DealCard(false));
                Console.WriteLine("The House gets a secret card");
                house.hand.Add(deck.DealCard(true));
                house.CheckIfBust();
                house.ChangeAces();
            }
        }

        //House hits untill it reaches 16
        public int HouseHit()
        {
            while (house.total < 16)
            {
                Console.Write("The House Hits");
                house.hand.Add(deck.DealCard(false));
                house.CheckIfBust();
                house.ChangeAces();
            }
            house.Stand();
            return house.total;
        }

        public void Play(string pName)
        {
            player1.Name = pName;
            player1.CheckIfBust();
            deck.CreateDeck();

            //Initial cards
            InitialHits();

            //Player wins automatically if he has a "blackjack"
            if (player1.HasBlackjack() == true)
            {
                Console.WriteLine($"{player1.Name} has a Blackjack!");
                status = (byte)Status.blackjack;
            }
            else
            {
                //loops as long as the player hasn't gone bust
                while (!player1.Bust && !player1.isStanding)
                {
                    //Player gives input
                    Console.WriteLine($"It is {player1.Name}'s turn:");
                    Console.WriteLine("Press H to hit   Press S to stand" +
                        "\nPress V to view your hand");

                    string input = Console.ReadLine();
                    //input check
                    if (input == "h" || input == "H")
                    {
                        PlayerHit();
                    }
                    else if (input == "s" || input == "S")
                    {
                        player1.Stand();
                        break;
                    }
                    else if (input == "v" || input == "V")
                    {
                        player1.ViewHand();
                    }
                    else
                    {
                        Console.Write("Unknown command\nPlease type a valid command ");
                        input = Console.ReadLine();
                    }
                    player1.CheckIfBust();
                    player1.ChangeAces();
                }
                if (player1.Bust)
                {
                    Console.WriteLine("{0}'s hand is {1} so he goes bust"
                        , player1.Name, player1.total);
                }
                HouseHit();
                CompareHands();
            }
            AnounceWinner();
        }

        public void AnounceWinner()
        {
            switch (status)
            {
                case 1:
                    Console.WriteLine($"{player1.Name} wins!");
                    break;
                case 2:
                    Console.WriteLine("The House wins!");
                    break;
                case 3:
                    Console.WriteLine("Is is a draw");
                    break;
                case 4:
                    Console.WriteLine($"{player1.Name} wins!");
                    break;
            }
        }

        void CompareHands()
        {
            if (player1.total > 21)
                player1.total = 0;
            if (house.total > 21)
                house.total = 0;

            if (player1.total > house.total)
            {
                status = (byte)Status.playerWin;
            }
            else if (house.total > player1.total)
            {
                status = (byte)Status.houseWin;
            }
            else
            {
                status = (byte)Status.draw;
            }
            if (player1.Bust && house.Bust)
            {
                status = (byte)Status.draw;
            }
        }
    }
}
