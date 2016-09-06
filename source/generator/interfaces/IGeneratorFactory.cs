using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDocGenerator.source.generator.interfaces
{
    interface IGeneratorFactory
    {
        IGenerator createGenerator();
    }
}
