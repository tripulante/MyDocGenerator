using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.kernel;

namespace MyDocGenerator.source.parser.implementation
{
    class SQLCommentParser : ICommentParser
    {
        protected List<CommentNode> nodes;

        public SQLCommentParser() {
            nodes = new List<CommentNode>();
        }

        public void loadConfiguration(string fpath) { 
        
        }

        public void parseFile(string fpath) {
            bool parsing = false;
            
            using (StreamReader reader = new StreamReader(fpath)){
                string line = "";
                //original pattern on perl : /\/[\*]{2}[^\d\w\*]/ [^\d\w\*]
                //@todo correct beggining regex to avoid extra nodes
                string beggining = @"^/[\*]{2}$";
                //original pattern on perl : /\*[\s]*@[\w]/ or /[\-]*[\s]*@[\w]/
                Regex tagline = new Regex(@"^\-{2}[\s]+|\*+?[\s]+");
                //original pattern on perl : /\*\//
                Regex tag = new Regex(@"(@[\w]*[\s]+)");
                string endcomment = @"\*/";
                CommentNode node = null;
                CommentTag tagInfo = null;
                while ((line = reader.ReadLine()) != null) {
                    if (Regex.IsMatch(line, beggining))
                    {
                        parsing = true;
                        Console.WriteLine("beginning: {0}", line);
                        node = new CommentNode();
                        node.setFile(fpath);
                    }
                    else if (tagline.IsMatch(line))
                    {
                        if (parsing) {
                            
                            if (tag.IsMatch(line))
                            {
                                
                                //MatchCollection matches = tag.Matches(line);
                                //for (int i = 0; i < matches.Count; i++)
                                //    Console.WriteLine("matches: {0}", matches[i].Value);
                                string[] tagvalue = tag.Split(line,2);
                                //Console.WriteLine("tag: {0} value: {1}", tagvalue[1], tagvalue[2]);
                                CommentTag temptag = new CommentTag(tagvalue[1].Substring(1).ToLowerInvariant().Trim(), tagvalue[2]);
                                tagInfo = temptag;
                                node.addTag(tagInfo);
                            }
                            else {
                                string[] addlines = tagline.Split(line, 2);
                                //for (int i = 0; i < addlines.Length; i++)
                                //    Console.WriteLine("tagline: {0}", addlines[i]);
                                tagInfo.setText(tagInfo.getText() + ' ' + addlines[1]);
                                //Console.WriteLine("other line tag: " + line);
                            }
                        }
                        
                    }
                    if (Regex.IsMatch(line, endcomment)) {
                        if (parsing) {
                            parsing = false;
                            if (node != null)
                            {
                                nodes.Add(node);
                                Console.WriteLine("node: {0}", node.ToString());
                            }
                            node = null;
                        }
                    }
                    if (!parsing) { //parsing line from values
                        //syntax SQL parsing
                        //Regex sql = new Regex(@"^[\s]*create|alter|drop[\s]+function|procedure|table[\s]+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                        //if (sql.IsMatch(line)) {
                        //    string[] matches = sql.Split(line);
                        //    for (int i = 0; i < matches.Length; i++)
                        //        Console.WriteLine("matches: {0}", matches[i]);
                        //}
                        
                    }
                    
                }
            }
            
        }

        public List<CommentNode> getCommentNodes() {
            return nodes;
        }

        public void parse() { 
        
        }
    }
}
