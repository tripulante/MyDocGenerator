using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using MyDocGenerator.source.kernel;


namespace MyDocGenerator.source.generator.interfaces
{
    interface IGenerator
    {
        void generateDocumentation(string folderPath, List<CommentNode> comments, string projectName, List<string> validTags);
    }
}
