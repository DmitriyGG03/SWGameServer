using SharedLibrary.Models;

namespace SharedLibrary.Responses
{
    public class GameEndedResponse
    {
        public Hero? GameWinner { get; set; }
        public int CountOfTurns { get; set; }
        public int CountOfBattles { get; set; }
    }
}