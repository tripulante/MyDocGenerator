using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDocGenerator.source.kernel
{
    class CommentTag
    {
        private string tagName;

        private string text;

        public CommentTag(string nTag, string nText)
        {
            tagName = nTag;
            text = nText;
        }

        public string getTag()
        {
            return tagName;
        }

        public void setTag(string nTag)
        {
            tagName = nTag;
        }

        public string getText()
        {
            return text;
        }
        public void setText(string nText)
        {
            text = nText;
        }

        public override string ToString()
        {
            return "tag: " + tagName + " text: " + text;
        }
    }
}
