using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDocGenerator.source.parser.interfaces
{
    interface IParserFactory
    {
        ICommentParser createParser();
    }
}
