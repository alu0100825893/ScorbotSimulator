using UnityEngine;
using System.Text;
using System.Collections;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.EventSystems;

namespace CommandTerminal
{
    public class Terminal : MonoBehaviour
    {
        //Gameobjects vinculado al Controlador, Conexión y el Panel
        public GameObject panel;
        public GameObject Control;
        private Controller Controller;

        //Configuración de la terminal
        int BufferSize = 512;
        Font ConsoleFont;
        string InputCaret = ">";
        string command_text;
        float InputContrast;
        float InputAlpha = 0.5f;

        //Colores de la terminal y sus letras
        [SerializeField] Color BackgroundColor = Color.black;
        [SerializeField] Color ForegroundColor = Color.white;
        [SerializeField] Color BackgroundColor_Border = Color.gray;
        [SerializeField] Color InputColor = Color.cyan;
        [SerializeField] Color Warning = Color.red;
        [SerializeField] Color Success = Color.green;

        //Dibujo de la terminal
        Rect window;
        Rect history;
        TextEditor editor_state;
        Vector2 scroll_position;
        Vector2 scroll_position_history;
        GUIStyle verticalScrollbar;
        GUIStyle window_style;
        GUIStyle label_style;
        GUIStyle input_style;
        Texture2D background_texture;
        Texture2D background_texture_border;
        Texture2D input_background_texture;

        //Introducción de comando o introducción de datos 
        public bool Input { get; set; }
        public bool Input_text { get; set; }
        
        //Función doble click
        float clicked = 0;
        float clicktime = 0;
        float clickdelay = 0.5f;

        //Función tabulador
        List<string> command_possible;
        private int count_tab;
        private bool command_tab;

        //Log y historial
        public CommandLog Buffer { get; private set; }
        public CommandHistory History { get; private set; }

        public void Log(TerminalLogType type, string format, params object[] message)
        {
            Buffer.HandleLog(string.Format(format, message), type);
            scroll_position.y = int.MaxValue;
        }

        public void Log_End(TerminalLogType type, string format, params object[] message)
        {
            Buffer.HandleLog_End(string.Format(format, message), type);
            scroll_position.y = int.MaxValue;
        }

        public void Input_View(bool T)
        {
            Input = T;
        }

        public void Input_Text(bool T)
        {
            Input_text = T;
        }

        void Start() {

            Buffer = new CommandLog(BufferSize);
            History = new CommandHistory();
            Controller = Control.GetComponent<Controller>();

            Input = true;
            Input_text = false;
            count_tab = 0;
            command_tab = true;

            SetupWindow();
            SetupInput();
            SetupLabels();
        }

        void OnGUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            float h = canvas.GetComponent<RectTransform>().rect.height;

            RectTransform rt = (RectTransform)panel.transform;
            float width = rt.rect.width;
            float height = rt.rect.height;
            float x = panel.transform.position.x;
            float y = panel.transform.position.y;
            y = h - y;

            float width_terminal = (width*70) / 100;
            float width_history = (width*30) / 100;

            //Marco
            GUI.DrawTexture(new Rect(x, y, width, height), background_texture_border);

            //Terminal
            GUI.DrawTexture(new Rect(x+5, y+5, width_terminal - 10, height-10), background_texture);
            window = new Rect(x + 5, y + 5, width_terminal - 10, height - 10);
            window = GUILayout.Window(0, window, DrawConsole, "", window_style);

            //Historial
            GUI.DrawTexture(new Rect(x + width_terminal + 2, y+5, width_history - 8, height - 10), background_texture);
            history = new Rect(x + width_terminal + 2, y + 5, width_history - 8, height - 10);
            history = GUILayout.Window(1, history, DrawHistory, "", window_style);

        }
        

        void DrawConsole(int Window2D) {

            CursorToEnd();

            if (Event.current.Equals(Event.KeyboardEvent("return")) || Event.current.Equals(Event.KeyboardEvent("[enter]")))
            {
                EnterCommand();
                count_tab = -1;
                Event.current.Use();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("up")))
            {
                command_text = History.Previous();
                count_tab = -1;
                Event.current.Use();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("down")))
            {
                command_text = History.Next();
                count_tab = -1;
                Event.current.Use();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("tab")))
            {
                CompleteCommand();
                Event.current.Use();
                command_tab = true;
            }
            else if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {             
                    if (command_tab == true)
                        command_tab = false;
                    else
                        count_tab = -1;
            }

            scroll_position = GUILayout.BeginScrollView(scroll_position, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.BeginVertical();
            DrawLogs();
            if (Input)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(InputCaret, input_style, GUILayout.Width(ConsoleFont.fontSize));
                GUI.SetNextControlName("Text");
                command_text = GUILayout.TextField(command_text, input_style);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        void DrawLogs() {
            int count = 1;
            if (Buffer != null)
            {
                foreach (var log in Buffer.Logs)
                {
                    label_style.normal.textColor = GetLogColor(log.type);
                    if (log.type == TerminalLogType.Command)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(InputCaret, label_style, GUILayout.Width(ConsoleFont.fontSize));
                        GUILayout.Label(log.message, label_style);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (Buffer.Size() == count && Input_text)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(log.message, label_style, GUILayout.ExpandWidth(false));
                            GUILayout.Label(InputCaret, label_style, GUILayout.ExpandWidth(false));
                            command_text = GUILayout.TextField(command_text, label_style);
                            GUILayout.EndHorizontal();
                        }

                        else
                        {
                            GUILayout.Label(log.message, label_style);
                        }
                    }
                    count++;
                }
            }
        }

        void DrawHistory(int Window2D)
        {
            scroll_position_history = GUILayout.BeginScrollView(scroll_position, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
            GUILayout.BeginVertical();

            if (Buffer != null)
            {
                foreach (string log in History.historical())
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(log, input_style))
                    {
                        if (Event.current.button == 0)
                        {
                            clicked++;
                            if (clicked == 1) clicktime = Time.time;

                            if (clicked > 1 && Time.time - clicktime < clickdelay)
                            {
                                clicked = 0;
                                clicktime = 0;
                                command_text = log;
                                GUI.FocusControl("Text");
                            }
                            else if (clicked > 2 || Time.time - clicktime > 1)
                                clicked = 0;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        void EnterCommand() {
            if (Controller.IsOnline())
                Controller.RunCommandOnline(command_text);
            else
                Controller.RunCommandOffline(command_text);
            command_text = "";
        }

        void CompleteCommand()
        {
            string head_text = command_text;
            if (count_tab == -1)
            {
                command_possible = Buffer.Find_Log(head_text);
                string tmp = Controller.ListOfCommand.Find(c => c.name.StartsWith(head_text)).name;
                if (tmp != null)
                    command_possible.Add(tmp);
            }
            count_tab++;

            if (count_tab == command_possible.Count)
                count_tab = 0;

            command_text = command_possible[count_tab];
        }

        void CursorToEnd()
        {
            editor_state = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editor_state.MoveCursorToPosition(new Vector2(5555, 5555));
        }

        void SetupWindow()
        {
            // Set background color
            background_texture = new Texture2D(1,1);
            background_texture.SetPixel(0, 0, BackgroundColor);
            background_texture.Apply();

            background_texture_border = new Texture2D(1, 1);
            background_texture_border.SetPixel(0, 0, BackgroundColor_Border);
            background_texture_border.Apply();

            window_style = new GUIStyle();
            window_style.padding = new RectOffset(10, 10, 10, 10);
            window_style.normal.textColor = ForegroundColor;
            window_style.font = ConsoleFont;
            window_style.border = new RectOffset(70, 70, 70, 70);
        }

        void SetupLabels()
        {
            label_style = new GUIStyle();
            label_style.font = ConsoleFont;
            label_style.padding = new RectOffset(0, 0, 0, 10);
            label_style.wordWrap = true;
            label_style.normal.textColor = InputColor;
        }

        void SetupInput()
        {
            ConsoleFont = Font.CreateDynamicFontFromOSFont("Courier New", 16);
            command_text = "";

            input_style = new GUIStyle();
            input_style.padding = new RectOffset(0, 0, 0, 0);
            input_style.font = ConsoleFont;
            input_style.fixedHeight = ConsoleFont.fontSize * 1.6f;
            input_style.normal.textColor = InputColor;

            var dark_background = new Color();
            dark_background.r = BackgroundColor.r - InputContrast;
            dark_background.g = BackgroundColor.g - InputContrast;
            dark_background.b = BackgroundColor.b - InputContrast;
            dark_background.a = InputAlpha;

            input_background_texture = new Texture2D(1, 1);
            input_background_texture.SetPixel(0, 0, dark_background);
            input_background_texture.Apply();
            input_style.normal.background = input_background_texture;
        }

        Color GetLogColor(TerminalLogType type)
        {
            switch (type)
            {
                case TerminalLogType.Command: return InputColor;
                case TerminalLogType.Log: return Color.yellow;
                case TerminalLogType.Success: return Success;
                case TerminalLogType.Warning: return Warning;
                default: return InputColor;
            }
        }
    }
}
