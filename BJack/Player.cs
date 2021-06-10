using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    class Player
    {
        public string Name { get; set; }
        public List<int> hand = new List<int>();
        public bool Bust = false;
        public bool isStanding = false;
        public int total;

        public Player(string name = "")
        {
            Name = name;
            total = 0;
        }

        public int GetTotal()
        {
            total = 0;
            foreach (int card in hand)
            {
                total += card;
            }
            return total;
        }

        public bool HasBlackjack()
        {
            return (total == 21) ? true : false;
        }

        public virtual void Stand()
        {
            CheckIfBust();
            if (!Bust)
            {
                Console.WriteLine("{0} stands at {1}", Name, total);
            }
            isStanding = true;
        }

        public virtual void ViewHand()
        {
            Console.WriteLine("{0}'s hand is {1}", Name, total);
        }

        public bool CheckIfBust()
        {
            GetTotal();
            return Bust = (total > 21) ? true : false;
        }

        public void ChangeAces()
        {
            while (Bust && hand.Contains(11))
            {
                hand[hand.FindIndex(index => index.Equals(11))] = 1;
                CheckIfBust();
            }
        }
    }
}
