using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDocGenerator.source.generator.interfaces;
using MyDocGenerator.source.generator.implementation;

namespace MyDocGenerator.source.generator.factory
{
    class WordGeneratorFactory : IGeneratorFactory
    {
        public IGenerator createGenerator() {
            return new WordGenerator();
        }
    }
}
