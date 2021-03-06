﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.OOS_LanguageObjects
{
    public class AssignContainer : pBaseLangObject, Interfaces.iName, Interfaces.iHasType
    {

        public Ident Name { get { return ((Ident)this.children[0]); } set { this.children[0] = value; } }
        public VariableAssignment assign { get { return ((VariableAssignment)this.children[1]); } set { this.children[1] = value; } }
        public string FullyQualifiedName { get { return ((Ident)this.children[0]).FullyQualifiedName; } }
        public VarTypeObject ReferencedType { get { return ((Ident)this.children[0]).ReferencedType; } }
        public pBaseLangObject ReferencedObject { get { return ((Ident)this.children[0]).ReferencedObject; } }
        public AssignContainer(pBaseLangObject parent) : base(parent)
        {
            this.addChild(null);
            this.addChild(null);
        }
        public override int doFinalize() { return 0; }
        public override void writeOut(System.IO.StreamWriter sw, SqfConfigObjects.SqfConfigFile cfg)
        {
            string tab = new string('\t', this.getAllParentsOf<Interfaces.iCodeBlock>().Count);
            sw.Write(tab);
            if (((Variable)this.Name.LastIdent.ReferencedObject).encapsulation == Encapsulation.NA || ((Variable)this.Name.LastIdent.ReferencedObject).encapsulation == Encapsulation.Static)
            {
                this.Name.writeOut(sw, cfg);
                sw.Write(" = ");
                this.assign.writeOut(sw, cfg);
            }
            else
            {
                this.Name.writeOut(sw, cfg);
                sw.Write(" set [" + ((Variable)this.Name.LastIdent.ReferencedObject).SqfVariableName + ", ");
                this.assign.writeOut(sw, cfg);
                sw.Write("]");
            }
        }
    }
}
