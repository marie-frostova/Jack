
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoBot.Bots
{
    class GameDurak
    {
        public class Durak
        {
            enum DurakState { Attacking, Defending };

            public enum Suits { Clubs, Diamonds, Hearts, Spades };
            public enum Ranks { Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

            public string[] suitImage = { "♣", "♦", "♥", "♠" };
            public string[] rankName = { "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

            public static Dictionary<Ranks, int> rankValue = new Dictionary<Ranks, int>();
            public static Dictionary<Ranks, string> rankNames = new Dictionary<Ranks, string>();
            public static Dictionary<Suits, string> suitsImages = new Dictionary<Suits, string>();

            DurakState state;

            public List<Card> deck;
            public Table table;

            public Card Trump;

            public Player[] players;

            int currentPlayerIndex;

            public int HowManyActivePlayers;

            public Durak(int playerCount)
            {
                state = DurakState.Attacking;

                Suits[] suits = new Suits[] { Suits.Clubs, Suits.Diamonds, Suits.Hearts, Suits.Spades };
                Ranks[] ranks = new Ranks[] { Ranks.Six, Ranks.Seven, Ranks.Eight, Ranks.Nine, Ranks.Ten, Ranks.Jack, Ranks.Queen, Ranks.King, Ranks.Ace };

                if (suitsImages.Count() == 0)
                {
                    for (var i = 0; i < suits.Length; i++)
                    {
                        suitsImages.Add(suits[i], suitImage[i]);
                    }
                }

                if (rankNames.Count() == 0)
                {
                    for (var i = 0; i < ranks.Length; i++)
                    {
                        rankNames.Add(ranks[i], rankName[i]);
                    }
                }

                if (rankValue.Count() == 0)
                {
                    for (var i = 0; i < ranks.Length; i++)
                    {
                        rankValue.Add(ranks[i], i + 6);
                    }
                }

                table = new Table();

                deck = new List<Card>();
                foreach (var suit in suits)
                {
                    foreach (var rank in ranks)
                    {
                        deck.Add(new Card(suit, rank));
                    }
                }

                Random random = new Random();
                deck = deck.OrderBy(x => random.Next()).ToList();
                Trump = new Card(deck[0].Suit, deck[0].Rank);

                players = new Player[playerCount];
                for (var i = 0; i < playerCount; i++)
                {
                    players[i] = new Player();
                }
                DealCards();
                currentPlayerIndex = 0;
                int HowManyActivePlayers = players.Length;
            }

            public class Card
            {
                public Suits Suit { get; }
                public Ranks Rank { get; }

                public Card(Suits suit, Ranks rank)
                {
                    this.Suit = suit;
                    this.Rank = rank;
                }

                public override string ToString()
                {
                    return "" + Durak.rankNames[Rank] + Durak.suitsImages[Suit];
                }
            }

            public class Player
            {
                public List<Card> Hand { get; set; }

                public bool Active;

                public Player()
                {
                    Hand = new List<Card>();
                    Active = true;
                }

                public void TakeCard(Card card)
                {
                    Hand.Add(card);
                }

                public int HowManyCards()
                {
                    return Hand.Count();
                }
            }

            public class Table
            {
                public List<Tuple<Card, Card>> Layout { get; set; }

                public Table()
                {
                    Layout = new List<Tuple<Card, Card>>();
                }

                public void DiscardPile()
                {
                    Layout.Clear();
                }

                public void AbandonDefense(Player player)
                {
                    foreach (var cardTuple in Layout)
                    {
                        player.Hand.Add(cardTuple.Item1);
                        if (cardTuple.Item2 != null)
                        {
                            player.Hand.Add(cardTuple.Item2);
                        }
                    }
                }
            }

            public Card TakeCard()
            {
                if (deck.Count > 0)
                {
                    var card = deck[deck.Count - 1];
                    deck.RemoveAt(deck.Count - 1);
                    return card;
                }
                return null;
            }

            public void DealCards()
            {
                for (var i = 0; i < players.Length; ++i)
                {
                    var player = players[(i + currentPlayerIndex - 1 + players.Length) % players.Length];
                    while (player.Hand.Count < 6 && deck.Count > 0)
                        player.Hand.Add(TakeCard());
                }
            }

            public bool BeatOrNot(Card card1, Card card2)
            {
                if (card1.Suit == card2.Suit)
                {
                    if (rankValue[card1.Rank] < rankValue[card2.Rank])
                    {
                        return true;
                    }
                    return false;
                }
                if (card1.Suit == Trump.Suit) return false;
                if (card2.Suit == Trump.Suit) return true;
                return false;
            }

            public int CurrentPlayerIndex { get => currentPlayerIndex; }

            public string GetStatus(int playerIndex)
            {
                var res = new StringBuilder();
                res.Append("Current player: " + currentPlayerIndex.ToString() + "\r\n"); // debug only
                res.Append("Trump is : " + rankNames[Trump.Rank] + suitsImages[Trump.Suit] + "\r\n");
                res.Append("Status: " + state.ToString() + "\r\n");
                res.Append("Your opponent has " + players[(playerIndex == players.Length - 1) ? 0 : playerIndex + 1].Hand.Count + " cards" + "\r\n");
                res.Append("Your hand: " + "\r\n");
                if (players[playerIndex].Hand.Count > 0)
                {
                    for (var i = 0; i < players[playerIndex].Hand.Count; i++)
                    {
                        res.Append(i + 1);
                        res.Append(": ");
                        if (players[playerIndex].Hand.Count > 0)
                            res.Append(players[playerIndex].Hand[i].ToString());
                        res.Append("\r\n");
                    }
                }

                res.Append("Table: " + "\r\n");
                for(var i = 0; i <  table.Layout.Count; i++)
                {
                    res.Append(i + 1);
                    res.Append(": ");
                    res.Append(table.Layout[i].Item1.ToString());
                    if (table.Layout[i].Item2 != null)
                        res.Append(table.Layout[i].Item2.ToString());
                    res.Append("\r\n");
                }
                res.Append(InProgress());
                return res.ToString();
            }

            public void Process(string input)
            {
                var player = players[currentPlayerIndex];
                var cardIndex = -1;
                for(var i = 0; i < player.Hand.Count; i++)
                {
                    var card = player.Hand[i].ToString();
                    if (input == card)
                    {
                        cardIndex = i;
                    }
                }

                if (cardIndex == -1 && input != "end turn" && input != "give up")
                    return;

                if (state == DurakState.Attacking)
                {
                    if (input == "end turn")
                    {
                        state = DurakState.Defending;
                        currentPlayerIndex = (currentPlayerIndex == players.Length - 1) ? 0 : currentPlayerIndex + 1;
                    }
                    else
                    {
                        table.Layout.Add(new Tuple<Card, Card>(player.Hand[cardIndex], null));
                        player.Hand[cardIndex] = null;
                        RemoveNullsFromHand(player);
                    }
                }
                else
                {
                    if (input == "give up")
                    {
                        table.Layout.ForEach(item => player.Hand.Add(item.Item1));
                        table.Layout.ForEach(item => player.Hand.Add(item.Item2));
                        RemoveNullsFromHand(player);
                        table.Layout.Clear();

                        DealCards();
                        state = DurakState.Attacking;
                        currentPlayerIndex = (currentPlayerIndex == 0) ? players.Length - 1 : currentPlayerIndex - 1;
                    }
                    else
                    {
                        var index = -1;
                        for (var j = 0; j < table.Layout.Count; j++)
                        {
                            if (table.Layout[j].Item2 == null)
                            {
                                index = j;
                                break;
                            }
                        }

                        table.Layout[index] = new Tuple<Card, Card>(table.Layout[index].Item1, player.Hand[cardIndex]);

                        player.Hand[cardIndex] = null;
                        RemoveNullsFromHand(player);

                        if (index == table.Layout.Count - 1)
                        {
                            table.Layout.Clear();
                            DealCards();
                            state = DurakState.Attacking;
                        }
                    }
                }
            }

            public List<string> GetActions(int userIndex)
            {
                var res = new List<string>();

                var player = players[currentPlayerIndex];
                if (state == DurakState.Attacking)
                {
                    if (table.Layout.Count == 0)
                    {
                        for (var i = 0; i < player.Hand.Count; i++)
                        {
                            res.Add(player.Hand[i].ToString());
                        }
                    }
                    else
                    {
                        var card = table.Layout[0].Item1;
                        for  (var i = 0; i < player.Hand.Count; i++)
                        {
                            if (player.Hand[i].Rank == card.Rank)
                            {
                                res.Add(player.Hand[i].ToString());
                            }
                        }
                        if (res.Count == 0)
                        {
                            state = DurakState.Defending;
                            currentPlayerIndex = (currentPlayerIndex == players.Length - 1) ? 0 : currentPlayerIndex + 1;
                            player = players[currentPlayerIndex];
                        }
                        else
                        {
                            res.Add("end turn");
                        }
                    }
                }
                if (state == DurakState.Defending)
                {
                    var index = -1;
                    for (var j = 0; j < table.Layout.Count; j++)
                    {
                        if (table.Layout[j].Item2 == null)
                        {
                            index = j;
                            break;
                        }
                    }

                    for(var i = 0; i < player.Hand.Count; i++)
                    {
                        if(BeatOrNot(table.Layout[index].Item1, player.Hand[i]))
                        {
                            res.Add(player.Hand[i].ToString());
                        }
                    }
                  
                    res.Add("give up");
                }
                if (currentPlayerIndex != userIndex)
                    res.Clear();
                return res;
            }

            public string InProgress()
            {
                if (players[currentPlayerIndex].Hand.Count == 0 && deck.Count == 0)
                    return "Player # " + currentPlayerIndex.ToString() + " is winner!";
                return "";
            }

            
            public void RemoveNullsFromHand(Player player)
            {
                int pos = 0;
                for (int i = 0; i < player.Hand.Count; ++i)
                {
                    if (player.Hand[i] != null)
                    {
                        player.Hand[pos++] = player.Hand[i];
                    }
                }

                for (int i = player.Hand.Count - 1; i >= pos; --i)
                {
                    player.Hand.RemoveAt(i);
                }
            }
        }
    }
}

