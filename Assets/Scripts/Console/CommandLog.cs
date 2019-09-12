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
                message = (tmp.TrimEnd() + ">" + message),
                type = type
            };

            logs.Add(log);
        }

        public List<string> Find_Log(string command)
        {
            List<string> command_complete = new List<string>();
            var list = Logs.Where(log => (log.message).StartsWith(command));
            foreach (var item in list)
            {
                command_complete.Add(item.message);
            }
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

        public void Remove_Last_Element()
        {
            logs.RemoveAt(logs.Count - 1);
        }
    }
}
