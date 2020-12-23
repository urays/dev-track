﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Complier.Model.Tokens
{
    class IdentifierToken : Token
    {
        public IdentifierToken(string content, int lineNum)
            :base(content, lineNum)
        { }
    }
}
