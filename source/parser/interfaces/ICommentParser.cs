using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDocGenerator.source.kernel;

namespace MyDocGenerator.source.parser.interfaces
{
	interface ICommentParser
	{
        void loadConfiguration(string fpath);

        void parseFile(string fpath);

        List<CommentNode> getCommentNodes();

        void parse();
	}
}
