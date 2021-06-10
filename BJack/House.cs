using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    class House : Player
    {
        public override void Stand()
        {
            CheckIfBust();
            if (Bust)
            {
                Console.WriteLine("The Houses's hand was {0} so it went bust", total);
                total = 0;
            }
            else
            {
                Console.WriteLine("The House stands at {0}", total);
            }
        }
    }
}
