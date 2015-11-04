﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.OOS_LanguageObjects
{
    public class While : pBaseLangObject, Interfaces.iCodeBlock
    {
        public pBaseLangObject expression { get { return this.children[0]; } set { this.children[0] = value; } }
        public List<pBaseLangObject> CodeInstructions { get { return this.children.GetRange(1, this.children.Count - 1); } }

        public While(pBaseLangObject parent) : base(parent)
        {
            this.children.Add(null);
        }
        public override int doFinalize() { return 0; }
        public List<Return> ReturnCommands
        {
            get { return this.getAllChildrenOf<Return>(); }
        }
        public bool AlwaysReturns
        {
            get
            {
                if (this.ReturnCommands.Count > 0)
                    return true;
                var codeBlocks = this.getAllChildrenOf<Interfaces.iCodeBlock>();
                foreach (var it in codeBlocks)
                {
                    if (it.AlwaysReturns)
                        return true;
                }
                return false;
            }
        }

        public override void writeOut(System.IO.StreamWriter sw, SqfConfigObjects.SqfConfigFile cfg)
        {
            string tab = new string('\t', this.getAllParentsOf<Interfaces.iCodeBlock>().Count);
            sw.Write(tab + "while {");
            this.expression.writeOut(sw, cfg);
            sw.WriteLine("} do");
            sw.WriteLine(tab + "{");
            var varList = this.getAllChildrenOf<Variable>();
            if (varList.Count > 0)
            {
                if (varList.Count == 1)
                    sw.Write("private ");
                else
                    sw.Write("private [");

                for (int i = 0; i < varList.Count; i++)
                {
                    var it = varList[i];
                    if (i != 0)
                    {
                        sw.Write(", ");
                    }
                    if (it is Variable)
                    {
                        sw.Write('"' + ((Variable)it).SqfVariableName + '"');
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (varList.Count > 1)
                    sw.Write("]");
                sw.WriteLine(";");
            }
            foreach(var it in this.CodeInstructions)
            {
                it.writeOut(sw, cfg);
                sw.WriteLine(";");
            }
            sw.Write(tab + "}");
        }
    }
}
