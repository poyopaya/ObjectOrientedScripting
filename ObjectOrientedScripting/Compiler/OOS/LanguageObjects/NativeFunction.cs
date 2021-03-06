﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.OOS_LanguageObjects
{
    public class NativeFunction : NativeInstruction, Interfaces.iFunction
    {
        public Ident Name { get { return (Ident)this.children[0]; } set { this.children[0] = value; } }
        public bool IsConstructor { get { return false; } }
        public bool IsVirtual { get { return false; } }
        /// <summary>
        /// Return type of this iFunction
        /// </summary>
        public VarTypeObject ReturnType { get { return this.VTO; } }
        public VarTypeObject ReferencedType { get { return this.ReturnType; } }
        /// <summary>
        /// Returns a Template object which then can deref some unknown class conflicts in
        /// ArgList field
        /// </summary>
        public Template TemplateArguments { get { return this.getFirstOf<Native>().TemplateObject; } }
        /// <summary>
        /// Returns functions encapsulation
        /// </summary>
        public Encapsulation FunctionEncapsulation { get { return this.Parent is Native ? Encapsulation.Public : Encapsulation.Static; } }
        /// <summary>
        /// Returns the Arglist required for this iFunction
        /// </summary>
        public List<VarTypeObject> ArgList
        {
            get
            {
                List<VarTypeObject> retList = new List<VarTypeObject>();
                foreach (var it in this.children)
                {
                    if (it is Variable)
                    {
                        retList.Add(((Variable)it).varType);
                    }
                    else if (it is Ident)
                    {
                        //Do nothing as we got the Name here
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                return retList;
            }
        }
        public bool IsAsync { get { return false; } }

        public NativeFunction(pBaseLangObject parent, int line, int pos, string file) : base(parent, line, pos, file)
        {
            this.addChild(null);
        }
        public override int doFinalize()
        {
            int errCount = 0;
            errCount += this.Name.finalize();
            if (VTO.ident != null)
                errCount += VTO.ident.finalize();
            if (Code.Contains("_this") && !(this.Parent is Native))
            {
                Logger.Instance.log(Logger.LogLevel.ERROR, ErrorStringResolver.resolve(ErrorStringResolver.LinkerErrorCode.LNK0049, this.Line, this.Pos, this.File));
                errCount++;
            }
            return errCount;
        }
        public override string ToString()
        {
            return "nFnc->" + this.Name.FullyQualifiedName;
        }
        public List<Return> ReturnCommands { get { return new List<Return>(); } }
        public bool AlwaysReturns { get { return true; } }


    }
}
