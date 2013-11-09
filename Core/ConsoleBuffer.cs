using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class ConsoleBuffer
    {
        private List<Tuple<float, string>> data = new List<Tuple<float, string>>();
        private int maxLength = 128;
        private int maxLines = 64;
        public System.IO.FileStream ostrm;
        public System.IO.StreamWriter writer;

        public ConsoleBuffer(int maxLines, int maxLength)
        {
            this.maxLines = maxLines;
            this.maxLength = maxLength;

            try
            {
                ostrm = new System.IO.FileStream("./log.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                ostrm.Close();
                ostrm = new System.IO.FileStream("./log.txt", System.IO.FileMode.Truncate, System.IO.FileAccess.Write);
                writer = new System.IO.StreamWriter(ostrm);
            }
            catch (Exception e) { addMessage(0, e.Message); }
        }

        private void appendMsg(float time, string msg)
        {
            if (writer != null)
            {
                writer.WriteLine(msg);
                writer.Flush();
            }
            while (data.Count > maxLines) data.RemoveAt(0);
            data.Add(Tuple.Create(time, msg));
        }

        public void addMessage(float time, string msg)
        {
            var msgs = msg.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in msgs)
            {
                var ls = s;
                while (ls.Length > maxLength)
                {
                    appendMsg(time, ls.Substring(0, maxLength));
                    ls = ls.Substring(maxLength);
                }
                appendMsg(time, ls);
            }
        }

        public void CloseLog()
        {
            if (writer != null) writer.Close();
            if (ostrm != null) ostrm.Close();
        }

        public void addMessage(double time, string msg) { addMessage((float)time, msg); }

        public List<String> getLatestMessages(int MaxMessages, float minimumTime)
        {
            var R = new List<String>();
            for (int I = data.Count - 1; I >= 0 && R.Count < MaxMessages; --I)
            {
                if (minimumTime <= data[I].Item1) R.Add(data[I].Item2);
                else break;
            }
            R.Reverse();
            return R;
        }
    }
}
