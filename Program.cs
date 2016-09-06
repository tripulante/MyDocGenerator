using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.parser.implementation;
using MyDocGenerator.source.generator.interfaces;
using MyDocGenerator.source.generator.implementation;
using MyDocGenerator.source.kernel;
using MyDocGenerator.source.helper;
using System.Windows.Forms;

namespace MyDocGenerator
{
    class Program
    {
        public bool folder;
        public bool subfolders;
        public bool config;
        public short language;
        public string source;
        public string destination;
        public string projectName;
        public string output;

        [STAThread]

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Welcome to SqlDocGenerator");
                Program p = new Program();


                if (p.showUserOptions()) {
                        
                    DocumentGenerator generator = new DocumentGenerator();

                    if (p.folder)
                    {
                        if (p.output.Equals("All"))
                        {
                            generator.generateDocumentationFromFolder(p.source, p.destination, p.projectName, p.language, DocumentGenerator.WORD, p.subfolders);
                            generator.generateDocumentationFromFolder(p.source, p.destination, p.projectName, p.language, DocumentGenerator.HTML, p.subfolders);
                        }
                        else
                            generator.generateDocumentationFromFolder(p.source, p.destination, p.projectName, p.language, p.output, p.subfolders);
                    }

                    else if (p.config) {
                        if (p.output.Equals("All"))
                        {
                            generator.generateDocumentationFromConfigFile(p.source, p.destination, p.projectName, DocumentGenerator.SQL_CONFIG, DocumentGenerator.WORD);
                            generator.generateDocumentationFromConfigFile(p.source, p.destination, p.projectName, DocumentGenerator.SQL_CONFIG, DocumentGenerator.HTML);
                        }
                        else
                            generator.generateDocumentationFromConfigFile(p.source, p.destination, p.projectName, DocumentGenerator.SQL_CONFIG, p.output);
                    }
                    else
                    {
                        if (p.output.Equals("All"))
                        {
                            generator.generateDocumentationFromSingleFile(p.source, p.destination, p.projectName, p.language, DocumentGenerator.WORD);
                            generator.generateDocumentationFromSingleFile(p.source, p.destination, p.projectName, p.language, DocumentGenerator.HTML);
                        }
                        else
                            generator.generateDocumentationFromSingleFile(p.source, p.destination, p.projectName, p.language, p.output);
                    }
                        

                    Console.WriteLine("Documentation Generated Successfully!");
                    Console.ReadKey();
                }
                
            }
            catch (Exception e) {
                
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
        }

        public bool showUserOptions(){
            Console.WriteLine("Which language do you want to parse? V/S");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            switch (key.Key) {
                case ConsoleKey.V: language = DocumentGenerator.VBA;
                    break;
                case ConsoleKey.S: language = DocumentGenerator.SQL;
                    break;
                default: break;
            }
            Console.WriteLine("Do you want to generate documentation for a Folder, Single File or Config File? F/S/C");
            key = Console.ReadKey();
            FolderBrowserDialog fdialog = new FolderBrowserDialog();
            folder = true;
            subfolders = false;
            config = false;
            source = null;
            destination = null;
            if (key.Key == ConsoleKey.F)
            {
                Console.WriteLine();
                Console.WriteLine("Do you want to parse subfolders (up to three levels)? Y/N");
                key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key == ConsoleKey.Y)
                    subfolders = true;

                fdialog.Description = "Select source folder";
                if (fdialog.ShowDialog() == DialogResult.OK)
                    source = fdialog.SelectedPath;
                else
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return false;
                }


            }
            else if (key.Key == ConsoleKey.S)
            {
                Console.WriteLine();
                folder = false;
                OpenFileDialog filed = new OpenFileDialog();
                filed.Title = "Open File";
                if(language == DocumentGenerator.SQL)
                    filed.Filter = "SQL Script Files (*.sql) | *.sql";
                else if(language == DocumentGenerator.VBA)
                    filed.Filter = "VBA Files (*.bas) | *.bas";
                filed.Multiselect = false;
                if (filed.ShowDialog() == DialogResult.OK)
                    source = filed.FileName;
                else
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return false;
                }
            }
            else if (key.Key == ConsoleKey.C) {
                Console.WriteLine();
                config = true;
                folder = false;
                OpenFileDialog filed = new OpenFileDialog();
                filed.Title = "Open File";
                filed.Filter = "XML Files (*.xml) | *.xml";
                filed.Multiselect = false;
                if (filed.ShowDialog() == DialogResult.OK)
                    source = filed.FileName;
                else
                {
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return false;
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return false;
            }
            fdialog.Description = "Select destination folder";
            if (fdialog.ShowDialog() == DialogResult.OK)
                destination = fdialog.SelectedPath;
            else
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return false;
            }
            Console.WriteLine("Type the project name: ");
            projectName = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine("Select an output format: Word, HTML, All: W/H/A");
            key = Console.ReadKey();
            Console.WriteLine();
            while (key.Key != ConsoleKey.H && key.Key != ConsoleKey.W && key.Key != ConsoleKey.A)
            {
                Console.WriteLine();
                Console.WriteLine("Select an output format: Word, HTML, All: W/H/A");
                key = Console.ReadKey();
                Console.WriteLine();
            }
            switch (key.Key) {
                case ConsoleKey.W: output = DocumentGenerator.WORD;
                    break;
                case ConsoleKey.H: output = DocumentGenerator.HTML;
                    break;
                default: output = "All";
                    break;
            }
            return true;
        }
    }
}
