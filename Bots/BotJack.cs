using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using static EchoBot.Bots.GameDurak;
using System.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    enum UserState
    { 
        NewUser,
        PickedGameUser,
        GotIdUser
    }

    enum GameSort
    {
        DurakGame
    }

    public class BotJack : ActivityHandler
    {
        static Dictionary<string, Durak> games = new Dictionary<string, Durak>(); // our_game_id -> game
        static Dictionary<string, Tuple<UserState, GameSort, string, int>> users
            = new Dictionary<string, Tuple<UserState, GameSort, string, int>>(); // conversation_id -> (state, game_sort, our_game_id, user_num)
        static Dictionary<string, List<ConversationReference>> conversations
            = new Dictionary<string, List<ConversationReference>>(); // our_game_id -> List of user_conversation_references

        private static Random random = new Random();

        static string CreateRandomId()
        {
            int length = 5;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string res;
            do
            {
                res = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            } while (games.ContainsKey(res));
            return res;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome!", "Hello and welcome!"), cancellationToken);
                    await SendSuggestedActionsAsync(turnContext, cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string id = turnContext.Activity.Conversation.Id;

            if (!users.ContainsKey(id))
            {
                string message;
                switch (turnContext.Activity.Text)
                {
                    case "Durak":
                        {
                            users[id] = Tuple.Create(UserState.PickedGameUser, GameSort.DurakGame, "", 0);
                            var reply = MessageFactory.Text("Choose start new game or enter existing game ID to join");
                            reply.SuggestedActions = new SuggestedActions()
                            {
                                Actions = new List<CardAction>() {
                                new CardAction() { Title = "New game", Type = ActionTypes.ImBack, Value = "New game" }
                            }
                            };
                            await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                            return;
                        }

                    default:
                        {
                            message = "Please, select another game. This game is under development.";

                            var reply = MessageFactory.Text(message);
                            await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                            break;
                        }
                }
            }
            else
            {
                var user = users[id];
                if (user.Item1 == UserState.PickedGameUser)
                {
                    if (turnContext.Activity.Text == "New game")
                    {
                        var gameId = CreateRandomId();
                        users[id] = Tuple.Create(UserState.GotIdUser, GameSort.DurakGame, gameId, 0);

                        conversations[gameId] = new List<ConversationReference>();
                        conversations[gameId].Add(turnContext.Activity.GetConversationReference());
                        games[gameId] = new Durak(2);

                        var reply = MessageFactory.Text("Your game id is " + gameId + ". Share it with your friend");
                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = new List<CardAction>() {
                                new CardAction() { Title = "Ok", Type = ActionTypes.ImBack, Value = "Ok" }
                            }
                        };
                        await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                        return;
                    }
                    else
                    {
                        var gameId = turnContext.Activity.Text;
                        if (games.ContainsKey(gameId))
                        {
                            users[id] = Tuple.Create(UserState.GotIdUser, GameSort.DurakGame, gameId, 1);
                            conversations[gameId].Add(turnContext.Activity.GetConversationReference());
                            // move next
                        }
                        else
                        {
                            var reply = MessageFactory.Text("No such game");
                            await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                            return;
                        }
                    }
                }

                user = users[id];
                if (user.Item1 == UserState.GotIdUser)
                {
                    var gameId = users[id].Item3;
                    if (games.ContainsKey(gameId))
                    {
                        var game = games[gameId];

                        if (turnContext.Activity.Text != "Ok")
                            game.Process(turnContext.Activity.Text);

                        var userIndex = user.Item4;
                        var actionsStrings = game.GetActions(userIndex);

                        if (actionsStrings.Count == 0)
                        {
                            // somehow notify our partner

                            for (var i = 0; i < 2; ++i)
                            {
                                var conversationReference = conversations[gameId][i];

                                var appId = "90a0a630-a0b5-4fc1-9171-abfa300c390d";
                                //var _adapter = Microsoft.BotBuilderSamples.Controllers.BotController._adapter;
                                await /*((BotAdapter)_adapter)*/
                                    turnContext.Adapter.ContinueConversationAsync(appId, conversationReference, BotCallback, default(CancellationToken));
                            }
                        }

                        var message = game.GetStatus(user.Item4);
                        var reply = MessageFactory.Text(message);
                        var actions = new List<CardAction>();

                        foreach (var actionsString in actionsStrings)
                            actions.Add(new CardAction() { Title = actionsString, Type = ActionTypes.ImBack, Value = actionsString });

                        reply.SuggestedActions = new SuggestedActions()
                        {
                            Actions = actions,
                        };
                        await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        var reply = MessageFactory.Text("Something went wrong");
                        await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("your turn");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>() {
                    new CardAction() { Title = "Ok", Type = ActionTypes.ImBack, Value = "Ok" }
                }
            };
            await turnContext.SendActivityAsync(reply);
        }

        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Please, choose game to play: ");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Durak", Type = ActionTypes.ImBack, Value = "Durak"},
                    new CardAction() { Title = "Black Jack", Type = ActionTypes.ImBack, Value = "Black Jack"},
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}