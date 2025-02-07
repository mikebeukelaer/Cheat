﻿using System.Collections.Generic;

namespace Cheat
{
    internal static class RecentCommands
    {
        public static void Add(List<string> commands, string command) 
        { 
            commands.Insert(0,command);
            if (commands.Count > 10) 
            {
                commands.RemoveAt(commands.Count - 1);
            }
        }

    }
}
