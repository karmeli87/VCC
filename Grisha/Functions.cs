using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCC
{
    public static class Functions
    {
        public static string stringifyQueue(Queue q)
        {
            return String.Join(";", q.ToArray()) + ";";
        }

        public static Queue destringifyQueue(string q)
        {
            
            Queue queue = new Queue();
            if (String.IsNullOrEmpty(q)) {
                return queue;
            }
            foreach (string s in q.Split(';'))
            {
                if(s.Length > 0)
                    queue.Enqueue(s);
            }
            return queue;
        }
        public static string getRelativepath(string file,int goUp)
        {
            string[] dirList = file.Split('\\');
            return String.Join("\\", dirList, dirList.Length - goUp, goUp - 1); 
        }
    }
}
