using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDocGenerator.source.kernel
{
    class CommentNode
    {
        private string file;

        private List<CommentTag> tags;

        private List<string> fullfile;

        public CommentNode(){
            file = "";
            tags = new List<CommentTag>();
            fullfile = new List<string>();
        }

        public List<string> filelines {
            get { return fullfile; }
            set { fullfile = value; }
        }

        public void setFile(string nFile) {
            file = nFile;
        }

        public string getFile() {
            return file;
        }

        public void addTag(CommentTag nTag) {
            tags.Add(nTag);
        }

        public List<CommentTag> getTagList() {
            return tags;
        }

        public override string ToString()
        {
            string complete = "file: " + file + " ";
            foreach(CommentTag tag in tags)
            {
                complete += " " + tag.ToString() + Environment.NewLine;
            }
            return complete;
        }
    }
}
