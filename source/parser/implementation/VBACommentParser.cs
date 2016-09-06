using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.kernel;
using System.IO;

namespace MyDocGenerator.source.parser.implementation
{
    class VBACommentParser : ICommentParser
    {
        protected List<CommentNode> nodes;

        public VBACommentParser() {
            nodes = new List<CommentNode>();
        }

        public void loadConfiguration(string fpath) { 
        
        }

        public void parseFile(string fpath) {
            
            string[] lines = File.ReadAllLines(fpath);
            bool parsing = false;
            CommentTag tag = null;
            CommentNode n = null;
            foreach (string l in lines) {
                parseLine(l, ref parsing, ref tag, ref n);
            }
            foreach(CommentNode p in nodes){
                p.setFile(fpath);
                p.filelines.AddRange(lines);
            }
            foreach (CommentNode p in nodes) {
                Console.WriteLine(p.getFile());
            }
        }

        public List<CommentNode> getCommentNodes() {
            return nodes;
        }


        public void parse() { 
        
        }

        /// <summary>
        /// Parses a project line.
        /// </summary>
        /// <param name="line">Text line to parse</param>
        /// <param name="hascomment">Indicates if a comment is being parsed. Modified by method</param>
        /// <param name="tagInfo">Tag to parse</param>
        /// <param name="parsed">Current parsing node </param>
        private void parseLine(string line, ref bool hascomment, ref CommentTag tagInfo, ref CommentNode parsed) {
            Regex beggining = new Regex(@"^'[\*]{5,}$");
            //original pattern on perl : /\*[\s]*@[\w]/ or /[\-]*[\s]*@[\w]/
            Regex tagline = new Regex(@"^\'+[\s]+");
            //original pattern on perl : /\*\//
            Regex tag = new Regex(@"([\w]*:[\s]+)");
            Regex endcomment = new Regex(@"^'[\*]{5,}");
            if (beggining.IsMatch(line))
            {
                if (hascomment)
                {
                    hascomment = false;
                    nodes.Add(parsed);
                }
                else {
                    hascomment = true;
                    parsed = new CommentNode();
                }
                
            }
            else if (tagline.IsMatch(line))
            {
                if (hascomment)
                {
                    if (tag.IsMatch(line))
                    {
                        string[] tagvalue = tag.Split(line, 2);
                        CommentTag temptag = new CommentTag(tagvalue[1].Substring(0, tagvalue[1].Length-2).ToLowerInvariant().Trim(), tagvalue[2]);
                        tagInfo = temptag;
                        parsed.getTagList().Add(tagInfo);
                    }
                    else
                    {
                        string[] addlines = tagline.Split(line, 2);
                        if (tagInfo != null)
                            tagInfo.setText(tagInfo.getText() + ' ' + addlines[1]);
                    }
                }
            }
        }
   
    
    }
}
