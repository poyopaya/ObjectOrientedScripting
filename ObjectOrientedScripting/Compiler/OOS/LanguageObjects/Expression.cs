﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.OOS_LanguageObjects
{
    public class Expression : pBaseLangObject, Interfaces.iHasType
    {
        public pBaseLangObject lExpression { get { return this.children[0]; } set { this.children[0] = value; } }
        public pBaseLangObject rExpression { get { return this.children[1]; } set { this.children[1] = value; } }
        public VarTypeObject ReferencedType
        {
            get
            {
                VarTypeObject lType = null;
                VarTypeObject rType = null;
                VarTypeObject oType = null;
                if (lExpression is Expression)
                {
                    lType = ((Expression)lExpression).ReferencedType;
                }
                else if (lExpression is Variable)
                {
                    lType = ((Variable)lExpression).varType;
                }
                else if (lExpression is Ident)
                {
                    if (((Ident)lExpression).LastIdent.ReferencedObject is Interfaces.iHasType)
                    {
                        lType = ((Interfaces.iHasType)((Ident)lExpression).LastIdent.ReferencedObject).ReferencedType;
                        if (!lType.IsObject && lType.IsArray && !((Ident)lExpression).LastIdent.ReferencedType.IsArray)
                        {
                            lType = ((Ident)lExpression).LastIdent.ReferencedType;
                        }
                    }
                    else
                    {
                        lType = ((Ident)lExpression).LastIdent.ReferencedType;
                    }
                    
                }
                else if (lExpression is Cast)
                {
                    lType = ((Cast)lExpression).ReferencedType;
                }
                else if (lExpression is NewInstance)
                {
                    lType = ((NewInstance)lExpression).ReferencedType;
                }
                else if (lExpression is SqfCall)
                {
                    lType = ((SqfCall)lExpression).ReferencedType;
                }
                else if (lExpression is Value)
                {
                    lType = new VarTypeObject(((Value)lExpression).varType);
                }
                else if (lExpression is InstanceOf)
                {
                    lType = ((InstanceOf)lExpression).ReferencedType;
                }
                else
                {
                    throw new NotImplementedException();
                }
                if (rExpression != null)
                {
                    if (rExpression is Expression)
                    {
                        rType = ((Expression)rExpression).ReferencedType;
                    }
                    else if (rExpression is Variable)
                    {
                        rType = ((Variable)rExpression).varType;
                    }
                    else if (rExpression is Ident)
                    {
                        rType = ((Ident)rExpression).LastIdent.ReferencedType;
                    }
                    else if (rExpression is Cast)
                    {
                        rType = ((Cast)rExpression).ReferencedType;
                    }
                    else if (rExpression is NewInstance)
                    {
                        rType = ((NewInstance)rExpression).ReferencedType;
                    }
                    else if (rExpression is SqfCall)
                    {
                        rType = ((SqfCall)rExpression).ReferencedType;
                    }
                    else if (rExpression is Value)
                    {
                        rType = new VarTypeObject(((Value)rExpression).varType);
                    }
                    else if (rExpression is InstanceOf)
                    {
                        rType = ((InstanceOf)rExpression).ReferencedType;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                switch (expOperator)
                {
                    case "&": case "&&": case "|": case "||":
                        if (lType.varType != VarType.Bool || rType.varType != VarType.Bool)
                            throw new Ex.LinkerException(ErrorStringResolver.LinkerErrorCode.LNK0014, line, pos);
                        oType = new VarTypeObject(VarType.Bool);
                        break;
                    case "==": case "===":
                        oType = new VarTypeObject(VarType.Bool);
                        break;
                    case "+": case "-": case "*": case "/":
                        if(lType.varType == VarType.String)
                        {
                            if (lType.varType != VarType.String || rType.varType != VarType.String)
                                throw new Ex.LinkerException(ErrorStringResolver.LinkerErrorCode.LNK0014, line, pos);
                            oType = new VarTypeObject(VarType.String);
                        }
                        else
                        {
                            if (lType.varType != VarType.Scalar || rType.varType != VarType.Scalar)
                                throw new Ex.LinkerException(ErrorStringResolver.LinkerErrorCode.LNK0014, line, pos);
                            oType = new VarTypeObject(VarType.Scalar);
                        }
                        break;
                    case ">": case ">=": case "<": case "<=":
                        if (lType.varType != VarType.Scalar || rType.varType != VarType.Scalar)
                            throw new Ex.LinkerException(ErrorStringResolver.LinkerErrorCode.LNK0014, line, pos);
                        oType = new VarTypeObject(VarType.Bool);
                        break;
                    case "":
                        return lType;
                    default:
                        throw new NotImplementedException();
                }
                return oType;
            }
        }
        public string expOperator;
        public bool negate;

        private int line;
        public int Line { get { return this.line; } }
        private int pos;
        public int Pos { get { return this.pos; } }

        public Expression(pBaseLangObject parent, int line, int pos) : base(parent)
        {
            this.children.Add(null);
            this.children.Add(null);
            negate = false;
            expOperator = "";
            this.line = line;
            this.pos = pos;
        }
        public override int doFinalize() { return 0; }
        public override void writeOut(System.IO.StreamWriter sw, SqfConfigObjects.SqfConfigFile cfg)
        {
            if (this.negate)
                sw.Write('!');
            sw.Write('(');
            this.lExpression.writeOut(sw, cfg);
            if(this.rExpression != null)
            {
                sw.Write(')' + this.expOperator + '(');
                this.rExpression.writeOut(sw, cfg);
            }
            sw.Write(')');
        }
    }
}
