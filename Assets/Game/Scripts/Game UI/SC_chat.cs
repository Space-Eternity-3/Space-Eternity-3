using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SC_chat : MonoBehaviour
{
    [TextArea(15, 20)]
    public string chat;
    public Text txc;
    public int max_line_length;

    public int max_lines;
    public int scroll = -1;
    public bool typing = false;
    public bool hide_when_inventory_open;
    public int msg_visibility_time;

    public List<string> SendHistory = new List<string>();
    public int history_pointer = 0;

    public RectTransform ChatOver; Vector3 FieldOverDefPos = new Vector3(0f,0f,0f);
    public RectTransform ChatOutput;
    public RectTransform ChatInput; Vector3 FieldInputDefPos = new Vector3(0f,0f,0f);
    public InputField ChatInputer;

    public int[] line_timers = new int[18];

    public SC_control SC_control;

    bool first_message_added = false;
    public void AddMessage(string message)
    {
        int lines_occupied = 1;

        StringBuilder sb = new StringBuilder();
        string[] msg_words = message.Split(' ');
        int i, lngt = msg_words.Length, line_length = 0;
        for(i=0;i<lngt;i++)
        {
            int characters_left = max_line_length - line_length;
            if(line_length==0)
            {
                if(msg_words[i].Length <= characters_left)
                {
                    //Good word starting
                    sb.Append(msg_words[i]);
                    line_length += msg_words[i].Length;
                }
                else
                {
                    //Too long word starting
                    int j, jngt = msg_words[i].Length;
                    for(j=0;j<jngt;j++)
                    {
                        sb.Append(msg_words[i][j]);
                        line_length++;
                        if((j+1)%max_line_length==0) {
                            sb.Append("\n");
                            lines_occupied++;
                            line_length = 0;
                        }
                    }
                }
            }
            else
            {
                if(msg_words[i].Length + 1 <= characters_left)
                {
                    //Good word inside
                    sb.Append(" "+msg_words[i]);
                    line_length += msg_words[i].Length + 1;
                }
                else
                {
                    //Too long word inside
                    sb.Append("\n");
                    lines_occupied++;
                    line_length = 0; i--;
                }
            }
        }

        if(!first_message_added) first_message_added = true;
        else chat += "\n";

        chat += sb.ToString();

        if(lines_occupied > 18) lines_occupied = 18;
        for(i=17;i>=lines_occupied;i--)
            line_timers[i] = line_timers[i-lines_occupied];
        for(i=lines_occupied-1;i>=0;i--)
            line_timers[i] = msg_visibility_time;
    }
    string ProceedMessage(string raw_msg)
    {
        StringBuilder sb = new StringBuilder(raw_msg);
        sb.Replace("\n", "");
        sb.Replace("\t", " ");
        return sb.ToString().Trim();
    }
    public void SentChatMsg()
    {
        string msg = ProceedMessage(ChatInputer.text);
        if(msg=="") return;
        if(SendHistory.Count==0 || SendHistory[SendHistory.Count-1]!=msg) SendHistory.Add(msg);
        if((int)SC_control.Communtron4.position.y==100)
        {
            SC_control.SendMTP("/ChatMessage "+SC_control.connectionID+" "+(new StringBuilder(msg)).Replace(" ","\t").ToString());
        }
        else
        {
            AddMessage("<Player> " + msg);
        }
    }
    void TextUpdateFromHistory(int his)
    {
        if(his==SendHistory.Count) ChatInputer.text = "";
        else ChatInputer.text = SendHistory[his];
    }
    void Update()
    {
        //F2 hide all chat
        if(Input.GetKeyDown(KeyCode.F2))
            for(int j=0;j<line_timers.Length;j++)
                line_timers[j] = 0;

        //Chat input activator
		if(!SC_control.pause)
		{
			if(!typing && (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown("/")) && !SC_control.SC_inv_mover.active && SC_control.Communtron1.position.z==0f)
            {
                history_pointer = SendHistory.Count;
                TextUpdateFromHistory(history_pointer);
                ChatInputer.Select();
				typing=true;
            }
			
			else if(typing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || SC_control.Communtron1.position.z!=0f || !ChatInputer.isFocused))
			{
                EventSystem.current.SetSelectedGameObject(null);
				typing = false;
				SC_control.blockEscapeThisFrame = true;
			}

            if(typing)
            {
                int old_history_pointer = history_pointer;
                bool arrowed = false;
                if(Input.GetKeyDown(KeyCode.UpArrow)) { history_pointer--; arrowed = true; }
                if(Input.GetKeyDown(KeyCode.DownArrow)) { history_pointer++; arrowed = true; }
                if(history_pointer < 0) history_pointer = 0;
                if(history_pointer > SendHistory.Count) history_pointer = SendHistory.Count;
                if(old_history_pointer != history_pointer) TextUpdateFromHistory(history_pointer);
                if(arrowed) ChatInputer.MoveTextEnd(false);
            }
		}

        if(hide_when_inventory_open)
            if(!SC_control.SC_inv_mover.active) ChatOver.localPosition = FieldOverDefPos;
            else ChatOver.localPosition = new Vector3(10000f,0f,0f);

        //Chat converter
        string newText = "";
        string[] newLines = chat.Split('\n');
        int i, lngt = newLines.Length, min = 0;

        max_lines = getMaxLinesVisible();

        int max_lines_local, destin;
        if(typing)
        {
            destin = lngt;
            ChatInput.localPosition = FieldInputDefPos;
        }
        else
        {
            destin = max_lines;
            ChatInput.localPosition = new Vector3(10000f,0f,0f);
        }

        if(destin > 18) max_lines_local = 18;
        else max_lines_local = destin;

        if(max_lines_local==0 || chat=="") ChatOutput.GetComponent<Image>().enabled = false;
        else
        {
            ChatOutput.GetComponent<Image>().enabled = true;
            ChatOutput.sizeDelta = new Vector2(ChatOutput.sizeDelta.x,14.1579f*max_lines_local+18.2421f-6f);
        }

        if (scroll == -1) scroll = lngt;
        if(typing)
        {
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0) scroll++;
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0) scroll--;
        }
        if (scroll <= max_lines_local) scroll = max_lines_local;
        if (scroll >= lngt || !typing) scroll = -1;

        if (scroll != -1) lngt = scroll;
        if (lngt > max_lines_local) min = lngt - max_lines_local;
        for(i=lngt-1;i>=min;i--)
        {
            newText = newLines[i] + newText;
            if(i!=min) newText = "\n" + newText;
        }
        txc.text = newText;
    }
    public int getMaxLinesVisible()
    {
        for(int i=0;i<18;i++)
            if(line_timers[i]==0) return i;
        return 18;
    }
    void FixedUpdate()
    {
        int i;
        for(i=0;i<18;i++)
            if(line_timers[i]>0) line_timers[i]--;
    }
    void Start()
    {
        FieldInputDefPos = ChatInput.localPosition;
        FieldOverDefPos = ChatOver.localPosition;
    }
}
