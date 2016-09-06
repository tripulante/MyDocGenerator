using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;


using MyDocGenerator.source.kernel;
using MyDocGenerator.source.generator.interfaces;

namespace MyDocGenerator.source.generator.implementation
{
    class HTMLGenerator : IGenerator
    {
        private string project;

        private string folder;

        private Dictionary<string, bool> tagCount;

        private List<string> filelist;

        public HTMLGenerator() {
            
            tagCount = new Dictionary<string, bool>();
            filelist = new List<string>();
        }

        public void generateDocumentation(string folderPath, List<CommentNode> comments, string projectName, List<string> validTags)
        {
            project = projectName;
            folder = folderPath + @"\html";
            Directory.CreateDirectory(folder);
            countMainSections(comments, validTags);
            createMainFile(comments, validTags);
            foreach (string t in validTags) { 
                if(tagCount[t])
                    createFile(comments, validTags, t);
            }
            foreach (CommentNode n in comments) {
                createSourceCodeFile(n, validTags, folder);
            }
            createSourceFileList(folder, validTags);
            createCSSFile();
        }

        private void createSourceCodeFile(CommentNode node, List<string> validTags, string folder)
        {
            if (node.getFile() != null) {
                if (node.filelines.Count > 0) { 
                    //@todo create file
                    string file = node.getFile();
                    if(file.Contains(@"\"))
                        file = file.Substring(file.LastIndexOf(@"\") + 1);
                    string filename = file;
                    Console.WriteLine("Filename: {0}", filename);
                    if(file.Contains("."))
                        file = file.Substring(0, file.LastIndexOf(@".") -1);
                    file = string.Format(@"{1}\{0}.html", file, folder);
                    if(!filelist.Contains(filename))
                        filelist.Add(filename);
                    using (XmlTextWriter writer = new XmlTextWriter(file, Encoding.UTF8))
                    {
                        //create file header
                        writer.Formatting = Formatting.Indented;
                        writer.Indentation = 4;
                        writer.WriteStartElement("html");

                        createHeadTag(writer, project + " API Reference Index");
                        //end of head

                        //start body
                        writer.WriteStartElement("body");
                        //start div content 
                        writer.WriteStartElement("div");
                        writer.WriteAttributeString("id", "wrapper");
                        //start div header
                        createHeader(writer, project + " API Reference", validTags);


                        //start div content
                        writer.WriteStartElement("div");
                        writer.WriteAttributeString("id", "content");

                        writer.WriteElementString("h1", filename);

                        writer.WriteStartElement("div");
                        writer.WriteAttributeString("id", "source");
                        writer.WriteStartElement("code");

                        for (int i = 0; i < node.filelines.Count; i++)
                        {
                            //writer.WriteString(node.filelines[i]);
                            writer.WriteString(node.filelines[i]);
                        }

                        writer.WriteEndElement();
                        writer.WriteEndElement();

                        //end div content
                        writer.WriteEndElement();

                        //end div content 
                        writer.WriteEndElement();
                        //end body 
                        writer.WriteEndElement();


                        //end of html tag (and document)
                        writer.WriteEndElement();
                        writer.Close();
                    }
                }
            }
        }

        private void createSourceFileList(string folder, List<string> validTags) {
            string file = folder + @"\files.html";
            using (XmlTextWriter writer = new XmlTextWriter(file, Encoding.UTF8))
            {
                //create file header
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartElement("html");

                createHeadTag(writer, project + " API Reference Index");
                //end of head

                //start body
                writer.WriteStartElement("body");
                //start div content 
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "wrapper");
                //start div header
                createHeader(writer, project + " API Reference", validTags);


                //start div content
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "content");

                writer.WriteStartElement("h2");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", "files.html");

                writer.WriteString("Source Code File List");

                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("table");
                writer.WriteAttributeString("id", "refTable");
                writer.WriteStartElement("thead");
                writer.WriteStartElement("tr");

                writer.WriteElementString("th", "Name");
                writer.WriteElementString("th", "Description");

                writer.WriteEndElement();
                writer.WriteEndElement();
                int i = 1;
                foreach (string s in filelist)
                {
                    string f = null;
                    if (s.Contains("."))
                        f = s.Substring(0, s.LastIndexOf(@".") - 1);
                    else
                        f = s;
                    f = string.Format(@"{0}.html", f);
                    writer.WriteStartElement("tr");
                    if (i % 2 == 0)
                        writer.WriteAttributeString("class", "alt");
                    writer.WriteStartElement("td");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", f);
                    writer.WriteString(s);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    i++;

                }

                writer.WriteEndElement();
                writer.Flush();


                //end div content
                writer.WriteEndElement();

                //end div content 
                writer.WriteEndElement();
                //end body 
                writer.WriteEndElement();


                //end of html tag (and document)
                writer.WriteEndElement();
                writer.Close();
            }
        }

        private void countMainSections(List<CommentNode> comments, List<string> validTags) {
            

            foreach (string tag in validTags) {
                int tagC = 0;
                bool exists = false;
                foreach (CommentNode node in comments) {
                    Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                    tagC += lookup[tag].Count();
                    if (tagC > 0){
                        exists = true;
                        break;
                    }
                    
                }
                Console.WriteLine("tag {0}, count {1}, exists? {2}", tag, tagC, exists);
                tagCount.Add(tag, exists);

            }
        }

        private void createMainFile(List<CommentNode> comments, List<string> validTags) { 
            using(XmlTextWriter writer = new XmlTextWriter(folder + @"\index.html",Encoding.UTF8)){
                //create file header
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartElement("html");

                createHeadTag(writer, project + " API Reference Index");
                //end of head

                //start body
                writer.WriteStartElement("body");
                //start div content 
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "wrapper");
                //start div header
                createHeader(writer, project + " API Reference", validTags);
                
                //start div content
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "content");
                //create content tables
                foreach (string t in validTags)
                    if (tagCount[t])
                        createReferenceTable(writer, t, comments, validTags, false);
                
                //end div content
                writer.WriteEndElement();

                //end div content 
                writer.WriteEndElement();
                //end body 
                writer.WriteEndElement();

                
                //end of html tag (and document)
                writer.WriteEndElement();
                writer.Close();
            }
            
        }

        private void createFile(List<CommentNode> comments, List<string> validTags, string tagName){
            string plural = getPluralTag(tagName);

            using (XmlTextWriter writer = new XmlTextWriter(folder + @"\" + plural + ".html", Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                //create file header
                writer.WriteStartElement("html");


                createHeadTag(writer, project + " API Procedures Index");
                //end of head

                //start body
                writer.WriteStartElement("body");
                //start div content 
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "wrapper");

                createHeader(writer, project + " API "+plural+" Index", validTags);

                //start div content
                writer.WriteStartElement("div");
                writer.WriteAttributeString("id", "content");

                
                List<CommentNode> nodes = new List<CommentNode>();
                foreach (CommentNode node in comments)
                {

                    Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                    if (lookup[tagName].Count() > 0)
                    {
                        nodes.Add(node);
                    }
                }
                createReferenceTable(writer, tagName, nodes, validTags, true);
                createDocumentedTable(writer, tagName, nodes, validTags);
                

                //end div content
                writer.WriteEndElement();


                //end body 
                writer.WriteEndElement();

                //end of html tag (and document)
                writer.WriteEndElement();
                writer.Close();
            }
        }

        private void createHeadTag(XmlTextWriter writer, string title) {
            writer.WriteStartElement("head");

            writer.WriteElementString("title", title);
            writer.WriteStartElement("link");
            writer.WriteAttributeString("rel", "stylesheet");
            writer.WriteAttributeString("type", @"text/css");
            writer.WriteAttributeString("href", @"docstyle.css");
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Flush();
        }

        private void createHeader(XmlTextWriter writer, string header, List<string> validTags) {
            //start div header
            writer.WriteStartElement("div");
            writer.WriteAttributeString("id", "header");

            writer.WriteElementString("h1", header);
            
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", "index.html");
            writer.WriteString("Index");
            writer.WriteEndElement();
            foreach (string t in validTags)
                if (tagCount[t]) {

                    string name = getPluralTag(t);
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", name + ".html");
                    
                    writer.WriteString(upperFirst(name));
                    writer.WriteEndElement();
                }
            //create tag for file list
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", "files.html");
            writer.WriteString("Files");
            writer.WriteEndElement();

            //end div header
            writer.WriteEndElement();
            writer.Flush();
        }

        private void createReferenceTable(XmlTextWriter writer, string tag, List<CommentNode> nodes, List<string> validTags, bool links) {
            string plural = getPluralTag(tag);
            if(validTags.Contains(tag)){
                
                writer.WriteStartElement("h2");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", plural + @".html");
               
                writer.WriteString(upperFirst(tag) + " List");
                
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("table");
                writer.WriteAttributeString("id", "refTable");
                writer.WriteStartElement("thead");
                writer.WriteStartElement("tr");

                writer.WriteElementString("th", "Name");
                writer.WriteElementString("th", "Description");

                writer.WriteEndElement();
                writer.WriteEndElement();
                int i = 1;
                foreach (CommentNode node in nodes)
                {
                    
                    Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                    if (lookup[tag].Count() > 0) {
                        CommentTag t = lookup[tag].First();
                        writer.WriteStartElement("tr");
                        if (i % 2 == 0)
                            writer.WriteAttributeString("class", "alt");
                        if (!links)
                            writer.WriteElementString("td", t.getText());
                        else {
                            writer.WriteStartElement("td");
                            writer.WriteStartElement("a");
                            writer.WriteAttributeString("href", "#"+t.getText());
                            writer.WriteString(t.getText());
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        if (lookup["description"].Count() > 0)
                            t = lookup["description"].First();
                        else if (lookup["desc"].Count() > 0)
                            t = lookup["desc"].First();
                        else if (lookup["brief"].Count() > 0)
                            t = lookup["brief"].First();
                        writer.WriteElementString("td", t.getText());
                        
                        writer.WriteEndElement();
                        i++;
                    }
                    
                }

                writer.WriteEndElement();
                writer.Flush();
            }
            
        }

        private void createDocumentedTable(XmlTextWriter writer, string tag, List<CommentNode> nodes, List<string> validTags)
        {
            if (validTags.Contains(tag))
            {
                writer.WriteStartElement("h2");
                writer.WriteString(upperFirst(tag) + " Descriptions");
                writer.WriteEndElement();


                foreach (CommentNode node in nodes)
                {
                    List<CommentTag> nodetags = node.getTagList();
                    Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)nodetags.ToLookup(p => p.getTag());
                    if (lookup[tag].Count() > 0)
                    {
                        writer.WriteStartElement("table");
                        writer.WriteAttributeString("id", "tableDoc");
                        CommentTag t = lookup[tag].First();
                        writer.WriteStartElement("thead");
                        writer.WriteStartElement("tr");
                        writer.WriteStartElement("th");
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("name", t.getText());
                        writer.WriteString(t.getText());
                        //end head a
                        writer.WriteEndElement();
                        //end head tr
                        writer.WriteEndElement();
                        //end th
                        writer.WriteEndElement();
                        //end head
                        writer.WriteEndElement();
                        int i = 1;
                        foreach (CommentTag tg in nodetags) {
                            if (!tg.getTag().Equals(tag)) {
                                writer.WriteStartElement("tr");
                                if (i % 2 == 0) {
                                    writer.WriteAttributeString("class", "alt");
                                }
                                writer.WriteElementString("td", tg.getTag());
                                writer.WriteElementString("td", tg.getText());
                                //end row
                                writer.WriteEndElement();
                                i++;
                            }
                        }

                        writer.WriteStartElement("tr");
                        if (i % 2 == 0)
                        {
                            writer.WriteAttributeString("class", "alt");
                        }
                        if (node.getFile() != null) {
                            writer.WriteElementString("td", "file");
                            writer.WriteStartElement("td");
                            writer.WriteStartElement("a");
                            string file = node.getFile().Substring(node.getFile().LastIndexOf(@"\") + 1);
                            string f = null;
                            if (file.Contains("."))
                                f = file.Substring(0, file.LastIndexOf(".") - 1);
                            else
                                f = file;
                            f = string.Format("{0}.html", f);
                            writer.WriteAttributeString("href", f);
                            writer.WriteString(file);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        
                        
                        //end table
                        writer.WriteEndElement();

                        writer.WriteElementString("p", "");
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", "#");
                        writer.WriteString("Go To Top");
                        writer.WriteEndElement();
                    }
                }
                

                
                
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        private void createCSSFile() {
            File.Copy(@"data\docstyle.css", folder + @"\docstyle.css", true);
        }

        private string upperFirst(string a) {
            if (String.IsNullOrEmpty(a) || String.IsNullOrWhiteSpace(a))
                return a;
            char[] up = a.ToCharArray();
            up[0] = Char.ToUpper(up[0]);
            return new string(up);
        }

        private string getPluralTag(string tagName) {
            char last = tagName[tagName.Length - 1];
            string plural = tagName;
            if (last == 'x')
                plural += "es";
            else if(last != 's')
                plural += "s";
            return plural;
        }
    }
}
