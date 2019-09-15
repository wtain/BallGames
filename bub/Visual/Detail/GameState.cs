using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace bub.Visual.Detail
{
    public class GameState : DependencyObject
    {
        public Field Field { get; set; }
        public Selection Selection { get; set; }
        public int Score { get; set; }
        public int TurnCount { get; set; }
        public int SelectionPoints { get; set; }
        public int SelectionCount { get; set; }
    }
}
