﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDocGenerator.source.parser.interfaces;
using MyDocGenerator.source.parser.implementation;

namespace MyDocGenerator.source.parser.factory
{
    class VBAParserFactory : IParserFactory
    {
        public ICommentParser createParser() {
            return new VBACommentParser();
        }

    }
}
