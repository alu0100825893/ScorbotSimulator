using System.Collections.Generic;

namespace CommandTerminal
{
    public class CommandHistory
    {
        List<string> history = new List<string>();
        int position;

        public void Push(string command_string) {
            if (command_string == "") {
                return;
            }

            history.Add(command_string);
            position = history.Count;
        }

        public string Next() {
            position++;

            if (position >= history.Count) {
                position = history.Count;
                return "";
            }

            return history[position];
        }

        public string Previous() {
            if (history.Count == 0) {
                return "";
            }

            position--;

            if (position < 0) {
                position = 0;
            }

            return history[position];
        }

        public List<string> Find_Log(string command)
        {
            List<string> command_complete = new List<string>();
            var list = history.FindAll(c => c.ToLower().StartsWith(command.ToLower()));
            foreach (var item in list)
            {
                command_complete.Add(item);
            }
            return command_complete;
        }

        public void Clear() {
            history.Clear();
            position = 0;
        }

        public List<string> historical()
        {
            return history;
        }
    }
}
