using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommandTerminal
{
    public enum TerminalLogType
    {
        Command = 0,
        Log = 1,
        Warning = 2,
        Success = 3
    }

    public struct LogItem
    {
        public string message;
        public TerminalLogType type;
    }

    public class CommandLog
    {
        List<LogItem> logs = new List<LogItem>();
        int max_items;

        public List<LogItem> Logs {
            get { return logs; }
        }

        public CommandLog(int max_items) {
            this.max_items = max_items;
        }

        public void HandleLog(string message, TerminalLogType type)
        {
            LogItem log = new LogItem()
            {
                message = message,
                type = type
            };

            logs.Add(log);
        }

        public void HandleLog_End(string message, TerminalLogType type)
        {
            string tmp = logs[logs.Count - 1].message;
            logs.RemoveAt(logs.Count - 1);

            LogItem log = new LogItem()
            {
                message = (tmp.TrimEnd() + "> " + message),
                type = type
            };

            logs.Add(log);
        }

        public List<LogItem> Find_Log(string command)
        {
            List<LogItem> command_complete = new List<LogItem>();

            command_complete = Logs.Where(log => (log.message).ToString().StartsWith(command)).ToList();
            return command_complete;
        }

        public void Clear()
        {
            logs.Clear();
        }

        public int Size()
        {
            return logs.Count;
        }
    }
}
