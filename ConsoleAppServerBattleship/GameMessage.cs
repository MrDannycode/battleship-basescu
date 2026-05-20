using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppServerBattleship
{
    public class GameMessage
    {
        public string Tip { get; set; }      // Message type
        public int X { get; set; }           // Row
        public int Y { get; set; }           // Column
        public string Status { get; set; }   // "Hit", "Miss", "Sunk"
        public int JucatorActiv { get; set; } // 1 or 2
        public int[][] Board { get; set; }   // Board state
    }
}
