﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrapper;
using Compiler;
using Compiler.SqfConfigObjects;
using Compiler.OOS_LanguageObjects;
using System.IO;

namespace Wrapper
{
    public class Compiler : ICompiler
    {
        string configFileName;
        bool addFunctionsClass;
        List<PPDefine> flagDefines;
        public static readonly string endl = "\r\n";
        public Compiler()
        {
            configFileName = "config.cpp";
            addFunctionsClass = true;
            flagDefines = new List<PPDefine>();
        }
        public void setFlags(string[] strArr)
        {
            foreach (var s in strArr)
            {
                int count = s.IndexOf('=');
                if (count == -1)
                    count = s.Length;
                string switchstring = s.Substring(1, count - 1);
                switch (switchstring.ToUpper())
                {
                    case "CLFN":
                        if (count == -1)
                        {
                            Logger.Instance.log(Logger.LogLevel.ERROR, "Missing output file");
                            Logger.Instance.close();
                            throw new Exception("Missing output file");
                        }
                        configFileName = s.Substring(count + 1);
                        break;
                    case "NFNC":
                        addFunctionsClass = false;
                        break;
                    case "DEFINE":
                        addFunctionsClass = false;
                        flagDefines.Add(new PPDefine('#' + s.Substring(count + 1)));
                        break;
                    default:
                        Logger.Instance.log(Logger.LogLevel.WARNING, "Unknown flag '" + s + "' for compiler version '" + this.getVersion().ToString() + "'");
                        break;
                }
            }
        }
        public Version getVersion()
        {
            return new Version("0.4.0-ALPHA");
        }
        public void CheckSyntax(string filepath)
        {
            Scanner scanner = new Scanner(filepath);
            Parser parser = new Parser(scanner);
            parser.Parse();
        }
        #region Translating
        public void Translate(Project proj)
        {
            //Read compiled file
            Scanner scanner = new Scanner(proj.Buildfolder + "_compile_.obj");
            Parser parser = new Parser(scanner);
            parser.Parse();
            //OosContainer container;
            //parser.getBaseContainer(out container);
            int errCount = parser.errors.count + parser.BaseObject.finalize();
            if (errCount > 0)
            {
                Logger.Instance.log(Logger.LogLevel.ERROR, "Errors found (" + errCount + "), cannot continue with Translating!");
                return;
            }
            SqfConfigFile file = new SqfConfigFile(configFileName);
            iSqfConfig cfgClass = file;
            if (addFunctionsClass)
            {
                cfgClass = new SqfConfigClass("cfgFunctions");
                file.addChild(cfgClass);
            }
            WriteOutTree(proj, parser.BaseObject, proj.OutputFolder, cfgClass, null);
            //Create config.cpp file
            file.writeOut(proj.OutputFolder);
        }
        private void updateTabcount(ref string s, ref int tabCount, int change)
        {
            tabCount += change;
            string tab = new string('\t', tabCount);
        }
        public void WriteOutTree(Project proj, pBaseLangObject container, string path, iSqfConfig configObj, StreamWriter writer, int tabCount = 0)
        {
            string curPath = path;
            string tab = new string('\t', tabCount);
            if (container == null)
            {
                //Just skip null objects and give a warning
                Logger.Instance.log(Logger.LogLevel.WARNING, "Experienced NULL object during WriteOutTree. Output file: " + path);
            }
            #region object Base
            else if (container is Base)
            {
                var obj = (Base)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
            }
            #endregion
            #region object Namespace
            else if (container is Namespace)
            {
                var obj = (Namespace)container;
                var objConfigClass = new SqfConfigClass(obj.FullyQualifiedName.Replace("::", "_"));
                configObj.addChild(objConfigClass);
                path += obj.Name.OriginalValue;
                Logger.Instance.log(Logger.LogLevel.VERBOSE, "Creating directory '" + path + "'");
                Directory.CreateDirectory(path);
                foreach (var it in obj.children)
                {
                    if (it is Namespace)
                        WriteOutTree(proj, it, path, configObj, writer, tabCount);
                    else
                        WriteOutTree(proj, it, path, objConfigClass, writer, tabCount);
                }
            }
            #endregion
            #region object oosClass
            else if (container is oosClass)
            {
                var obj = (oosClass)container;
                var objConfigClass = new SqfConfigClass(obj.Name.OriginalValue);
                configObj.addChild(objConfigClass);
                path += obj.Name.OriginalValue;
                Logger.Instance.log(Logger.LogLevel.VERBOSE, "Creating directory '" + path + "'");
                Directory.CreateDirectory(path);
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, objConfigClass, writer, tabCount);
                }
            }
            #endregion
            #region object Function
            else if (container is Function)
            {
                var obj = (Function)container;
                int index = 0;
                path += obj.Name.OriginalValue + ".sqf";
                writer = new StreamWriter(path);
                var objConfigClass = new SqfConfigClass(obj.Name.OriginalValue);
                configObj.addChild(objConfigClass);
                if (obj.encapsulation == Encapsulation.NA)
                {
                    throw new Exception("Function has Encapsulation.NA on encapsulation field, please report to developer");
                }
                else if (obj.encapsulation == Encapsulation.Static)
                {
                    objConfigClass.addChild(new SqfConfigField("file", path));
                    objConfigClass.addChild(new SqfConfigField("preInit", obj.Name.OriginalValue.StartsWith("preInit", StringComparison.OrdinalIgnoreCase) ? "1" : "0"));
                    objConfigClass.addChild(new SqfConfigField("postInit", obj.Name.OriginalValue.StartsWith("postInit", StringComparison.OrdinalIgnoreCase) ? "1" : "0"));
                    objConfigClass.addChild(new SqfConfigField("preStart", obj.Name.OriginalValue.StartsWith("preStart", StringComparison.OrdinalIgnoreCase) ? "1" : "0"));
                    objConfigClass.addChild(new SqfConfigField("recompile", "0"));
                    objConfigClass.addChild(new SqfConfigField("ext", "\".sqf\""));
                    objConfigClass.addChild(new SqfConfigField("headerType", "0"));
                }
                else
                {
                    objConfigClass.addChild(new SqfConfigField("file", path));
                    objConfigClass.addChild(new SqfConfigField("preInit", "0"));
                    objConfigClass.addChild(new SqfConfigField("postInit", "0"));
                    objConfigClass.addChild(new SqfConfigField("preStart", "0"));
                    objConfigClass.addChild(new SqfConfigField("recompile", "0"));
                    objConfigClass.addChild(new SqfConfigField("ext", "\".sqf\""));
                    objConfigClass.addChild(new SqfConfigField("headerType", "0"));
                }
                writer.WriteLine("params [");
                updateTabcount(ref tab, ref tabCount, 1);
                index = 0;
                foreach (var it in obj.ArgList)
                {
                    if(it is Variable)
                    {
                        if(index > 0)
                            writer.WriteLine(",");
                        var variable = (Variable)it;
                        writer.Write(tab + "\"_" + variable.Name.OriginalValue + "\"");
                        index++;
                    }
                    else
                        throw new Exception("Function has non-Variable object in arglist, please report to developer");
                }
                updateTabcount(ref tab, ref tabCount, -1);
                writer.WriteLine(endl + "];");
                var pVarList = obj.getAllChildrenOf<Variable>(true);
                pVarList.Intersect(obj.ArgList);

                writer.WriteLine("private [");
                updateTabcount(ref tab, ref tabCount, 1);
                index = 0;
                foreach (var it in obj.ArgList)
                {
                    if (it is Variable)
                    {
                        if (index > 0)
                            writer.WriteLine(",");
                        var variable = (Variable)it;
                        writer.Write(tab + "\"_" + variable.Name.OriginalValue + "\"");
                        index++;
                    }
                    else
                        throw new Exception("Function has non-Variable object in arglist, please report to developer");
                }
                updateTabcount(ref tab, ref tabCount, -1);
                writer.WriteLine(endl + "];");
                writer.WriteLine("scopeName \"function\";");
                updateTabcount(ref tab, ref tabCount, 1);
                foreach (var it in obj.CodeInstructions)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                updateTabcount(ref tab, ref tabCount, -1);
            }
            #endregion
            #region object Break
            else if (container is Break)
            {
                var obj = (Break)container;
                updateTabcount(ref tab, ref tabCount, 1);
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                updateTabcount(ref tab, ref tabCount, -1);
                writer.WriteLine(tab + "breakOut \"loop\"");
            }
            #endregion
            #region object Case
            else if (container is Case)
            {
                var obj = (Case)container;
                if (obj.expression == null)
                {
                    writer.WriteLine(tab + "default: {");
                }
                else
                {
                    writer.Write(tab + "case ");
                    WriteOutTree(proj, obj.expression, path, configObj, writer, 0);
                    writer.WriteLine(": {");
                }
                updateTabcount(ref tab, ref tabCount, 1);
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                updateTabcount(ref tab, ref tabCount, -1);
                writer.WriteLine(tab + "};");
            }
            #endregion
            #region object Expression
            else if (container is Expression)
            {
                var obj = (Expression)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object For
            else if (container is For)
            {
                var obj = (For)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object FunctionCall
            else if (container is FunctionCall)
            {
                var obj = (FunctionCall)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object Ident
            else if (container is Ident)
            {
                if (container.Parent is Namespace ||
                    container.Parent is oosClass ||
                    container.Parent is VirtualFunction ||
                    container.Parent is Variable ||
                    container.Parent is Function ||
                    container.Parent is oosInterface ||
                    container.Parent is SqfCall)
                {
                    return;
                }
                var obj = (Ident)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object IfElse
            else if (container is IfElse)
            {
                var obj = (IfElse)container;
                writer.Write(tab + "if (");
                WriteOutTree(proj, obj.expression, path, configObj, writer, 0);
                writer.WriteLine(") then");
                writer.Write(tab + "{");
                updateTabcount(ref tab, ref tabCount, 1);
                foreach (var it in obj.IfInstructions)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                updateTabcount(ref tab, ref tabCount, -1);
                if(obj.HasElse)
                {
                    writer.WriteLine(tab + "}");
                    writer.WriteLine(tab + "else");
                    writer.WriteLine(tab + "{");
                    updateTabcount(ref tab, ref tabCount, 1);
                    foreach (var it in obj.ElseInstructions)
                    {
                        WriteOutTree(proj, it, path, configObj, writer, tabCount);
                    }
                    updateTabcount(ref tab, ref tabCount, -1);
                }
                writer.WriteLine(tab + "};");
            }
            #endregion
            #region object NewArray
            else if (container is NewArray)
            {
                var obj = (NewArray)container;
                var index = 0;
                writer.Write("[");
                foreach (var it in obj.children)
                {
                    if (index > 0)
                        writer.WriteLine(",");
                    WriteOutTree(proj, it, path, configObj, writer, 0);
                    index++;
                }
                writer.Write("];");
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object NewInstance
            else if (container is NewInstance)
            {
                var obj = (NewInstance)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object Return
            else if (container is Return)
            {
                var obj = (Return)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                writer.WriteLine("breakOut \"function\";");
            }
            #endregion
            #region object SqfCall
            else if (container is SqfCall)
            {
                var obj = (SqfCall)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object Switch
            else if (container is Switch)
            {
                var obj = (Switch)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object Throw
            else if (container is Throw)
            {
                var obj = (Throw)container;
                writer.Write(tab + "throw ");
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                writer.WriteLine(";");
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object TryCatch
            else if (container is TryCatch)
            {
                var obj = (TryCatch)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object Value
            else if (container is Value)
            {
                var obj = (Value)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                writer.Write(obj.value);
            }
            #endregion
            #region object Variable
            else if (container is Variable)
            {
                var obj = (Variable)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object VariableAssignment
            else if (container is VariableAssignment)
            {
                var obj = (VariableAssignment)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object While
            else if (container is While)
            {
                var obj = (While)container;
                foreach (var it in obj.children)
                {
                    WriteOutTree(proj, it, path, configObj, writer, tabCount);
                }
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
            #endregion
            #region object oosInterface
            else if (container is oosInterface)
            {
                //Interfaces are just logical structures in OOS, thus nothing gets created here
            }
            #endregion
            #region object VirtualFunction
            else if (container is VirtualFunction)
            {
                //VirtualFunctions are just logical structures in OOS, thus nothing gets created here
            }
            #endregion
            #region object Cast
            else if (container is Cast)
            {
                //Casts are just logical structures in OOS, thus nothing gets created here
            }
            #endregion
            else
            {
                throw new Exception("ShouldNeverEverHappen Exception, developer missed an object for writeOutTree -.-' BLAME HIM!!!!!");
            }
        }
        #endregion
        #region Compiling
        public void Compile(Project proj)
        {
            Logger.Instance.log(Logger.LogLevel.WARNING, "Compile is not supported by this compiler version, thus its just a plain \"CheckSyntax\" ... im sorry :(");
            Scanner scanner = new Scanner(proj.Buildfolder + "_compile_.obj");
            Parser parser = new Parser(scanner);
            parser.Parse();
            //OosContainer container;
            //parser.getBaseContainer(out container);
            int errCount = parser.errors.count;
            if (errCount > 0)
                throw new Exception("Errors found (" + errCount + "), cannot continue with Compile!");
            var filePath = proj.Buildfolder + "_preprocess_.obj";
            var newPath = proj.Buildfolder + "_compile_.obj";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.Copy(filePath, newPath, true);
                    break;
                }
                catch (IOException e)
                {
                    System.Threading.Thread.Sleep(500);
                    if (i == 2)
                        throw e;
                    continue;
                }
            }
        }
        #endregion
        #region PreProcessing
        public void Preprocess(Project proj)
        {
            //Make sure the build directory exists and create it if needed
            if (!Directory.Exists(proj.Buildfolder))
                Directory.CreateDirectory(proj.Buildfolder);
            //Prepare some stuff needed for preprocessing
            StreamWriter writer = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    writer = new StreamWriter(proj.Buildfolder + "_preprocess_.obj", false, Encoding.UTF8, 1024);
                    break;
                }
                catch (IOException e)
                {
                    Logger.Instance.log(Logger.LogLevel.WARNING, e.Message + " Trying again (" + (i + 1) + "/3)");
                    System.Threading.Thread.Sleep(500);
                    if (i == 2)
                        throw e;
                    continue;
                }
            }
            Dictionary<string, PPDefine> defines = new Dictionary<string, PPDefine>();
            foreach(var it in flagDefines)
                defines.Add(it.Name, it);
            List<preprocessFile_IfDefModes> ifdefs = new List<preprocessFile_IfDefModes>();
            //Start actual preprocessing
            preprocessFile(ifdefs, defines, proj, proj.Mainfile, writer);

            //Close the file writer
            writer.Flush();
            writer.Close();
        }
        private enum preprocessFile_IfDefModes
        {
            TRUE = 0,
            FALSE,
            IGNORE
        }
        private bool preprocessFile(List<preprocessFile_IfDefModes> ifdefs, Dictionary<string, PPDefine> defines, Project proj, string filePath, StreamWriter writer)
        {
            //Open given file
            StreamReader reader = new StreamReader(filePath);

            //Prepare some variables needed for the entire processing periode in this function
            string s;
            uint filelinenumber = 0;
            while ((s = reader.ReadLine()) != null)
            {
                filelinenumber++;
                //skip empty lines
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                //Remove left & right whitespaces and tabs from current string
                s = s.Trim();
                if (s[0] != '#')
                {//Current line is no define, thus handle it normally (find & replace)
                    //Make sure we are not inside of an ifdef/ifndef that disallows further processing of following lines
                    int i = ifdefs.Count - 1;
                    if (i >= 0 && ifdefs[i] != preprocessFile_IfDefModes.TRUE)
                        continue;
                    try
                    {
                        //Let every define check if it is inside of current line
                        foreach (PPDefine def in defines.Values)
                            s = def.replace(s);
                    }
                    catch (Exception ex)
                    {
                        //Catch possible exceptions from define parsing
                        Logger.Instance.log(Logger.LogLevel.ERROR, "Experienced some error while parsing existing defines.");
                        Logger.Instance.log(Logger.LogLevel.CONTINUE, ex.Message + ". file: " + filePath + ". linenumber: " + filelinenumber);
                        reader.Close();
                        return false;
                    }
                    writer.WriteLine(s);
                    continue;
                }
                //We DO have a define here
                //get end of the define name
                int spaceIndex = s.IndexOf(' ');
                if (spaceIndex < 0)
                    spaceIndex = s.Length;
                //set some required variables for the switch
                int index = -1;
                int index2 = -1;
                //get text AFTER the define
                string afterDefine = s.Substring(spaceIndex).TrimStart();

                //Check which define was used
                switch (s.Substring(0, spaceIndex))
                {
                    default:
                        throw new Exception("Encountered unknown define '" + s.Substring(0, spaceIndex) + "'");
                    case "#include":
                        //We are supposed to include a new file at this spot so lets do it
                        //Beautify the filepath so we can work with it
                        string newFile = proj.ProjectPath + afterDefine.Trim(new char[] { '"', '\'', ' ' });

                        //make sure we have no self reference here
                        if (newFile.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            //Ohhh no ... some problem in OSI layer 8
                            reader.Close();
                            throw new Exception("Include contains self reference. file: " + filePath + ". linenumber: " + filelinenumber);
                        }
                        //process the file before continuing with this
                        try
                        {
                            if (!preprocessFile(ifdefs, defines, proj, newFile, writer))
                            {
                                //A sub file encountered an error, so stop here to prevent useles waste of ressources
                                reader.Close();
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message + ", from " + filePath);
                        }
                        break;
                    case "#define":
                        //The user wants to define something here
                        while (s.EndsWith("\\"))
                        {
                            afterDefine += reader.ReadLine();
                            filelinenumber++;
                        }
                        //Get the two possible characters index that can be encountered after a define
                        index = afterDefine.IndexOf(' ');
                        index2 = afterDefine.IndexOf('(');
                        //check which one is found first
                        if (index < 0 || (index2 < index && index2 >= 0))
                            index = afterDefine.IndexOf('(');
                        //check that we really got a define with a value here, if not just take the entire length as no value is needed and only value provided
                        if (index < 0)
                            index = afterDefine.Length;
                        if (defines.ContainsKey(afterDefine.Substring(0, index)))
                        {
                            //Redefining something is not allowed, so throw an error here
                            reader.Close();
                            throw new Exception("Redefining a define is not allowed! Use #undefine to undef something. file: " + filePath + ". linenumber: " + filelinenumber);
                        }
                        //FINALLY add the define
                        defines.Add(afterDefine.Substring(0, index), new PPDefine(afterDefine));
                        break;
                    case "#undefine":
                        //just remove straigth
                        defines.Remove(s.Substring(spaceIndex).Trim());
                        break;
                    case "#ifdef":
                        //do required stuff for define ifs
                        if (defines.ContainsKey(afterDefine))
                            ifdefs.Add(ifdefs.Count == 0 || ifdefs[ifdefs.Count - 1] == preprocessFile_IfDefModes.TRUE ? preprocessFile_IfDefModes.TRUE : preprocessFile_IfDefModes.IGNORE);
                        else
                            ifdefs.Add(ifdefs.Count == 0 || ifdefs[ifdefs.Count - 1] == preprocessFile_IfDefModes.TRUE ? preprocessFile_IfDefModes.FALSE : preprocessFile_IfDefModes.IGNORE);
                        break;
                    case "#ifndef":
                        //do required stuff for define ifs
                        if (defines.ContainsKey(afterDefine))
                            ifdefs.Add(ifdefs.Count == 0 || ifdefs[ifdefs.Count - 1] == preprocessFile_IfDefModes.TRUE ? preprocessFile_IfDefModes.FALSE : preprocessFile_IfDefModes.IGNORE);
                        else
                            ifdefs.Add(ifdefs.Count == 0 || ifdefs[ifdefs.Count - 1] == preprocessFile_IfDefModes.TRUE ? preprocessFile_IfDefModes.TRUE : preprocessFile_IfDefModes.IGNORE);
                        break;
                    case "#else":
                        //do required stuff for define ifs
                        index = ifdefs.Count - 1;
                        if (index < 0)
                        {
                            reader.Close();
                            throw new Exception("unexpected #else. file: " + filePath + ". linenumber: " + filelinenumber);
                        }
                        //swap the value of currents if scope to the correct value
                        ifdefs[index] = (ifdefs[index] == preprocessFile_IfDefModes.TRUE ? preprocessFile_IfDefModes.FALSE : (ifdefs[index] == preprocessFile_IfDefModes.FALSE ? preprocessFile_IfDefModes.TRUE : preprocessFile_IfDefModes.IGNORE));
                        break;
                    case "#endif":
                        //do required stuff for define ifs
                        index = ifdefs.Count - 1;
                        if (index < 0)
                        {
                            reader.Close();
                            throw new Exception("unexpected #endif. file: " + filePath + ". linenumber: " + filelinenumber);
                        }
                        //remove current if scope
                        ifdefs.RemoveAt(index);
                        break;
                }
            }
            reader.Close();
            return true;
        }
        #endregion
    }
}
