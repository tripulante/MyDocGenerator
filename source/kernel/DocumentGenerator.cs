using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using MyDocGenerator.source.generator.interfaces;
using MyDocGenerator.source.generator.implementation;
using MyDocGenerator.source.generator.factory;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.parser.implementation;
using MyDocGenerator.source.parser.factory;
using MyDocGenerator.source.helper;


namespace MyDocGenerator.source.kernel
{
    class DocumentGenerator
    {
        public const string WORD = "Word";
        public const string EXCEL = "Excel";
        public const string HTML = "HTML";

        public const short SQL = 0;

        public const short SQL_CONFIG = 1;

        public const short VBA = 2;

        List<string> validTags;

        IParserFactory fparser;

        ICommentParser parser;

        IGeneratorFactory fgenerator;

        IGenerator generator;
         

        public DocumentGenerator() {
            loadValidTags(@"data\validtags.txt");   
        }

        private void initializeGenerators(short language, string doctype){
            try
            {
                fparser = getParserFactory(language);
                parser = fparser.createParser();
                fgenerator = getGeneratorFactory(doctype);
                generator = fgenerator.createGenerator();
            }
            catch (Exception e) {
                Log instance = Log.getInstance;
                instance.writeToLog("Error Initializing Generators: " + e.Message);
                throw e;
            }
        }

        private void loadValidTags(string tagfile) {
            validTags = new List<string>() { 
                "function",
                "procedure",
                "table",
                "class", 
                "view"
            };
            try
            {
                //Regex delimiters = new Regex(@"\r\n");
                using (StreamReader reader = new StreamReader(tagfile)) {
                    string tag = "";
                    while ((tag = reader.ReadLine()) != null) {
                        //if (!((delimiters.Split(tag)).Length != 1 || String.IsNullOrEmpty(tag) || String.IsNullOrWhiteSpace(tag)))
                        //{
                            if (!validTags.Contains(tag.ToLowerInvariant().Trim()))
                            {
                                validTags.Add(tag.ToLowerInvariant().Trim());
                            }
                        //}
                        //else {
                        //    foreach (string s in delimiters.Split(tag)) {
                        //        Console.WriteLine(s);
                        //    }
                        //}
                        
                    }
                }
                foreach (string s in validTags) {
                    Console.WriteLine(s);
                }
            }
            catch (Exception e) {
                Log instance = Log.getInstance;
                instance.writeToLog("Error Loading Valid Tags: " + e.Message);
                Console.WriteLine(e.Message);
                throw e;
            }

        }

        public void generateDocumentationFromFolder(string fpath, string destination, string projectName, short language, string doctype, bool subfolders) {
            try
            {
                initializeGenerators(language, doctype);

                if (Directory.Exists(fpath))
                {
                    DateTime before = DateTime.Now;
                    List<CommentNode> nodes = new List<CommentNode>();
                    string filter = "*."+language; //createFilterFromLanguage(language);
                    string[] filePaths = Directory.GetFiles(fpath, filter);
                    if (subfolders)
                    {
                        parseFilesFromSubfolders(fpath, 1, filter);
                    }
                    else
                        foreach(string file in filePaths)
                        {
                            parser.parseFile(file);
                            Console.WriteLine(file);
                        }
                    
                    generator.generateDocumentation(destination, parser.getCommentNodes(), projectName, validTags);
                    TimeSpan elapsed = (DateTime.Now - before);
                    Log instance = Log.getInstance;
                    
                    instance.writeToLog("Total Generation Time for Folder " + fpath + ": " + elapsed.ToString() + ". Language: " + language + " Format: " + doctype + " SubFolders: " + subfolders);
                    
                }
                else
                {
                    throw new Exception("Error: Folder path doesn't exist or is unavailable.");
                }
            }
            catch (Exception e) {
                Log instance = Log.getInstance;
                instance.writeToLog(e.Message);
                throw e;
            }
            
        }

        public void parseFilesFromSubfolders(string folder, int level, string filter) {
            if (level >= 3)
                return;
            else
            {
                string[] subfiles = Directory.GetFiles(folder, filter);
                foreach (string file in subfiles)
                {
                    parser.parseFile(file);

                    Console.WriteLine(file);
                }
                string[] dirs = Directory.GetDirectories(folder);
                foreach (string dir in dirs)
                {
                    Console.WriteLine("\tParsing Subfolder: {0}", dir);
                    parseFilesFromSubfolders(dir, level + 1, filter);
                }
            }
        }

        public void generateDocumentationFromSingleFile(string file, string destination, string projectName, short language, string doctype)
        {
            try {
                initializeGenerators(language, doctype);
                DateTime before = DateTime.Now;
                parser.parseFile(file);
                generator.generateDocumentation(destination, parser.getCommentNodes(), projectName, validTags);
                TimeSpan elapsed = (DateTime.Now - before);
                Log instance = Log.getInstance;
                instance.writeToLog("Total Generation Time for Folder " + file + ": " + elapsed.ToString() + ". Language: " + language + " Format: " + doctype);
            }
            catch (Exception e)
            {
                Log instance = Log.getInstance;
                instance.writeToLog(e.Message);
                throw e;
            }
            
        }

        public void generateDocumentationFromConfigFile(string file, string destination, string projectName, short language, string doctype) {
            try
            {
                initializeGenerators(language, doctype);
                DateTime before = DateTime.Now;
                parser.loadConfiguration(file);
                parser.parse();
                generator.generateDocumentation(destination, parser.getCommentNodes(), projectName, validTags);
                TimeSpan elapsed = (DateTime.Now - before);
                Log instance = Log.getInstance;
                instance.writeToLog("Total Generation Time for Folder " + file + ": " + elapsed.ToString() + ". Language: " + language + " Format: " + doctype);
            }
            catch (Exception e)
            {
                Log instance = Log.getInstance;
                instance.writeToLog(e.Message);
                throw e;
            }
        }

        public IGeneratorFactory getGeneratorFactory(string doctype) {
            switch (doctype) {
                case WORD: return new WordGeneratorFactory();
                case HTML: return new HTMLGeneratorFactory();
                case EXCEL: return new ExcelGeneratorFactory();
                default: return new HTMLGeneratorFactory();
            }
            
        }

        public IParserFactory getParserFactory(short doctype)
        {
            switch (doctype) {
                case SQL: return new SQLParserFactory();
                case SQL_CONFIG: return new SQLDBParserFactory();
                case VBA: return new VBAParserFactory();
                default: return new SQLParserFactory();
            }
            
        }

    }
}
