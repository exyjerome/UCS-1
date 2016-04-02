using System;
using System.IO;
using System.Text;
using UCS.Helpers;
using UCS.Logic;

namespace UCS.PacketProcessing
{
    internal class EndOfBattleCommand : Command
    {
        public EndOfBattleCommand(BinaryReader br)
        {
        }
        
        public override void Execute(Level level)
        {
        }
    }
}