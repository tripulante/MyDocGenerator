using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

using MyDocGenerator.source.generator.interfaces;
using MyDocGenerator.source.kernel;
using System.Collections;

namespace MyDocGenerator.source.generator.implementation
{
    class ExcelGenerator : IGenerator
    {
        private string project;

        private string folder;

        private int funcCount;

        private int classCount;

        private int procCount;

        private int tableCount;

        private Excel.Application excelApp;

        private Dictionary<string, bool> tagCount;

        public ExcelGenerator() {
            tagCount = new Dictionary<string, bool>();
            excelApp = null;
        }

        public void generateDocumentation(string folderPath, List<CommentNode> comments, string projectName, List<string> validTags)
        {
            try
            {
                folder = folderPath + @"\excel";
                Directory.CreateDirectory(folder);
                project = projectName;
                countMainSections(comments, validTags);

                excelApp = new Excel.Application();
                excelApp.DisplayAlerts = false;
                excelApp.Workbooks.Add();
                string filename = folder + @"\" + project + "_reference.xlsx";
                foreach (string t in validTags)
                    if (tagCount[t])
                        createSheet(comments, validTags, t);

                excelApp.ActiveWorkbook.SaveAs(System.IO.Path.GetFullPath(filename));
                excelApp.ActiveWorkbook.Close();
                excelApp.Quit();

            }
            catch (Exception e) {
                if (excelApp != null) {
                    excelApp.Quit();
                }
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

        private void createSheet(List<CommentNode> comments, List<string> validTags, string tag)
        {
            excelApp.Worksheets.Add();
            excelApp.ActiveSheet.Name = tag;

        }
    }
}
