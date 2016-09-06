using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using MyDocGenerator.source.generator.interfaces;
using MyDocGenerator.source.kernel;
using MyDocGenerator.source.helper;
using Word = Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using System.Reflection;

namespace MyDocGenerator.source.generator.implementation
{
    class WordGenerator : IGenerator
    {
        private Word._Application wordApp;

        private Word.Document wordDoc;

        string project;

        private string folder;

        private Dictionary<string, bool> tagCount;

        public WordGenerator(){
            
            tagCount = new Dictionary<string, bool>();
            wordApp = null;
        }

        public void generateDocumentation(string folderPath, List<CommentNode> comments, string projectName, List<string> validTags)
        {
            object oMissing = System.Reflection.Missing.Value;
            try{
                project = projectName;
                folder = folderPath + @"\word";
                Directory.CreateDirectory(folder);
                wordApp = new Word.Application();
                
                wordApp.Visible = false;
                wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                string filename = folder + "\\" + projectName + "_reference.docx";
                Console.WriteLine(filename);
                try
                {
                    //wordDoc = wordApp.Documents.Add(System.IO.Path.GetFullPath(".") + @"data\template.dotx");
                    wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);    
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);    
                }
                countMainSections(comments, validTags);
                createFrontPage();
                createIndex();

                List<CommentNode> nodes = new List<CommentNode>();
                foreach(string t in validTags)
                    if (tagCount[t]) {
                        foreach (CommentNode node in comments)
                        {
                            Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                            if (lookup[t].Count() > 0)
                                nodes.Add(node);
                        }
                        createReferenceTables(nodes, t, nodes.Count);
                        createDescriptionTables(nodes, validTags, t);
                        nodes.Clear();
                    }
                
                wordDoc.TablesOfContents[1].Update();
                
                //wordDoc.TablesOfContents[1].Range.set_Style(Word.WdBuiltinStyle.wdStyleTOC2);

                wordDoc.SaveAs(System.IO.Path.GetFullPath(filename));
                wordDoc.Close(ref oMissing, ref oMissing, ref oMissing);
                Marshal.FinalReleaseComObject(wordDoc);
                wordDoc = null;
                wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                Marshal.FinalReleaseComObject(wordApp);
                wordApp = null;
            }
            catch(Exception e){
                if (wordDoc != null) {
                    wordDoc.Close(false);
                    Marshal.FinalReleaseComObject(wordDoc);
                }
                if (wordApp != null) {
                    wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                    Marshal.FinalReleaseComObject(wordApp);
                }
                wordDoc = null;
                wordApp = null;
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        private void countMainSections(List<CommentNode> comments, List<string> validTags)
        {

            foreach (string tag in validTags)
            {
                int tagC = 0;
                bool exists = false;
                foreach (CommentNode node in comments)
                {
                    Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                    tagC += lookup[tag].Count();
                    if (tagC > 0)
                    {
                        exists = true;
                        break;
                    }

                }
                Console.WriteLine("tag {0}, count {1}, exists? {2}", tag, tagC, exists);
                tagCount.Add(tag, exists);

            }

        }

        private void createFrontPage() {
            //setting document properties   
            object oMissing = System.Reflection.Missing.Value;

            wordDoc.BuiltInDocumentProperties(Word.WdBuiltInProperty.wdPropertyTitle).Value = project + " API reference";
            Word.Paragraph oPara1;
            oPara1 = wordDoc.Content.Paragraphs.Add(ref oMissing);
            //oPara1.Range.Text = "Heading 1";
            //oPara1.Range.Font.Bold = 1;
            //oPara1.Format.SpaceAfter = 24;    //24 pt spacing after paragraph.
            oPara1.Range.InsertParagraphAfter();
        }

        private void createIndex() {

            object oEndOfDoc = "\\endofdoc";
            object oMissing = System.Reflection.Missing.Value;
            Word.TableOfContents contents = wordDoc.TablesOfContents.Add(wordDoc.Bookmarks[oEndOfDoc].Range);

            

            //wordDoc.Range().InsertBreak();
            contents.IncludePageNumbers = true;
            contents.Range.set_Style(Word.WdBuiltinStyle.wdStyleTOC2);
            wordDoc.Paragraphs.Add();
            //wordDoc.Content.InsertBreak(oMissing);
            //object oItem = "Automatic Table 2";
            //object oTrue = true;
            //((Word.Template)wordDoc.get_AttachedTemplate()).BuildingBlockEntries.Item(oItem).Insert(wordDoc.Bookmarks[oEndOfDoc].Range, oTrue);
            //Word.Template temp = ((Word.Template)wordDoc.get_AttachedTemplate());
            //temp.BuildingBlockTypes.Item(Word.WdBuildingBlockTypes.wdTypeTableOfContents);
            
            //wordDoc.Paragraphs.Add();
        }

        private void createReferenceTables(List<CommentNode> nodes, string tagName, int count) {
            object oMissing = System.Reflection.Missing.Value;
            object endOfDoc = "\\endofdoc";
            Word.Paragraph sec = wordDoc.Content.Paragraphs.Add(wordDoc.Bookmarks[endOfDoc].Range);
            
            sec.Range.Text = upperFirst(tagName) + " Reference Table";
            sec.Range.set_Style(Word.WdBuiltinStyle.wdStyleHeading1);
            sec.Range.InsertParagraphAfter();

            Word.Table functions = wordDoc.Tables.Add(wordDoc.Bookmarks[endOfDoc].Range, count + 1, 2, ref oMissing, ref oMissing);
            functions.Cell(1, 1).Range.Text = "Name";
            functions.Cell(1, 2).Range.Text = "Description";
            functions.set_Style("Medium List 1");

            int i = 2;
            foreach(CommentNode node in nodes){
                Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                foreach (CommentTag t in lookup[tagName]) {
                    
                    functions.Cell(i, 1).Range.Text = t.getText();
                    if (lookup["description"].Count() > 0)
                        functions.Cell(i, 2).Range.Text = lookup["description"].First().getText();
                    else if (lookup["desc"].Count() > 0)
                        functions.Cell(i, 2).Range.Text = lookup["desc"].First().getText();
                    else if (lookup["brief"].Count() > 0)
                        functions.Cell(i, 2).Range.Text = lookup["brief"].First().getText();
                }
                i++;   
            }
            functions.Rows[1].Range.Font.Bold = 1;
            functions.Rows[1].Range.Font.Italic = 1;
            wordDoc.Paragraphs.Add();
            
            
            

        }

        private void createDescriptionTables(List<CommentNode> comments, List<string> validTags, string tagName)
        {
            //@todo correct
            object oMissing = System.Reflection.Missing.Value;
            object endOfDoc = "\\endofdoc";

            Word.Paragraph sec = wordDoc.Content.Paragraphs.Add(wordDoc.Bookmarks[endOfDoc].Range);
            
            sec.Range.Text = upperFirst(tagName) + " Description Tables";
            sec.Range.set_Style(Word.WdBuiltinStyle.wdStyleHeading1);
            sec.Range.InsertParagraphAfter();

            foreach (CommentNode node in comments)
            {
                Word.Table comment = wordDoc.Tables.Add(wordApp.ActiveDocument.Bookmarks["\\endofdoc"].Range, 2, 2);

                List<CommentTag> tags = node.getTagList();
                Lookup<string, CommentTag> lookup = (Lookup<string, CommentTag>)node.getTagList().ToLookup(p => p.getTag());
                CommentTag tag = null;
                if (lookup.Contains(tagName)) {
                    tag = lookup[tagName].First();
                    comment.Range.Text = tag.getText();
                    comment.Cell(2, 1).Range.Text = "File";
                    comment.Cell(2, 2).Range.Text = node.getFile().Substring(node.getFile().LastIndexOf(@"\")+1);
                    foreach (CommentTag t in tags)
                    {
                        Word.Row row = comment.Rows.Add();
                        row.Cells[1].Range.Text = t.getTag();
                        row.Cells[2].Range.Text = t.getText();
                    }
                    object style = "Medium List 1 - Accent 5";
                    comment.set_Style(ref style);
                    comment.Columns.DistributeWidth();
                    wordDoc.Paragraphs.Add();

                }
                
                
            }
        }

        private string upperFirst(string a)
        {
            if (String.IsNullOrEmpty(a) || String.IsNullOrWhiteSpace(a))
                return a;
            char[] up = a.ToCharArray();
            up[0] = Char.ToUpper(up[0]);
            return new string(up);
        }
    }
}
