using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    class Card
    {
        private readonly Random randCard = new Random(DateTime.Now.Millisecond);
        public List<string> Deck = new List<string>();

        public void CreateDeck()
        {
            List<string> values = new List<string>
            {
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "Jack",
                "Queen",
                "King",
                "Ace"
            };

            List<string> suits = new List<string>
            {
                "Spades",
                "Diamonds",
                "Hearts",
                "Clubs"
            };

            foreach (string value in values)
            {
                foreach (string suit in suits)
                {
                    Deck.Add($"{value} of {suit}");
                }
            }
        }

        public int DealCard(bool secret)
        {
            var value = 0;
            var randomCard = randCard.Next(Deck.Count);
            var card = Deck[randomCard];
            Deck.RemoveAt(randomCard);

            if (!secret)
            {
                Console.Write(" and was dealt the {0}\n", card);
            }

            //Making the numbers exept 10 have their value
            if (card[0] == '2' || card[0] == '3' || card[0] == '4' || card[0] == '5' ||
                card[0] == '6' || card[0] == '7' || card[0] == '8' || card[0] == '9')
            {
                value = int.Parse(card[0].ToString());
            }
            //Making the faces and 10 have a value of 10
            else if (card[0] == '1' || card[0] == 'J' || card[0] == 'Q' || card[0] == 'K')
            {
                value = 10;
            }
            //Making the Aces have a value of 11
            else if (card[0] == 'A')
            {
                value = 11;
            }
            return value;
        }
    }
}
