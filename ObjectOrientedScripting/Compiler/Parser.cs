using Compiler.OOS_LanguageObjects;
using Compiler.OOS_LanguageObjects.HelperClasses;
using Compiler.OOS_LanguageObjects.Ex;




using System;



public class Parser {
	public const int _EOF = 0;
	public const int _UINTEGER = 1;
	public const int _UDOUBLE = 2;
	public const int _INTEGER = 3;
	public const int _DOUBLE = 4;
	public const int _FQIDENT = 5;
	public const int _LOCALIDENT = 6;
	public const int _LINETERMINATOR = 7;
	public const int _STRING = 8;
	public const int _COMMA = 9;
	public const int maxT = 69;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;
	OosContainer oosTreeBase;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		oosTreeBase = null;
	}
	
    public void getBaseContainer(out OosContainer blo)
    {
        blo =  oosTreeBase;
    }

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void SCALAR(out BaseLangObject bloOut) {
		var v = new OosValue(); bloOut = (BaseLangObject)v; 
		if (la.kind == 4) {
			Get();
			v.Value = t.val; 
		} else if (la.kind == 3) {
			Get();
			v.Value = t.val; 
		} else if (la.kind == 2) {
			Get();
			v.Value = t.val; 
		} else if (la.kind == 1) {
			Get();
			v.Value = t.val; 
		} else SynErr(70);
	}

	void ARRAY(out BaseLangObject bloOut) {
		var v1 = new OosValue(); bloOut = (BaseLangObject)v1;  BaseLangObject v2; 
		Expect(10);
		v1.append("["); 
		VALUE(out v2);
		v1.append((OosValue)v2); 
		while (la.kind == 9) {
			Get();
			v1.append(","); 
			VALUE(out v2);
			v1.append((OosValue)v2); 
		}
		Expect(11);
		v1.append("]"); 
	}

	void VALUE(out BaseLangObject bloOut) {
		var v = new OosValue(); bloOut = (BaseLangObject)v;  
		if (StartOf(1)) {
			SCALAR(out bloOut);
		} else if (la.kind == 8) {
			Get();
			v.Value = t.val; 
		} else if (la.kind == 10) {
			ARRAY(out bloOut);
		} else if (la.kind == 12 || la.kind == 13) {
			if (la.kind == 12) {
				Get();
				v.Value = t.val; 
			} else {
				Get();
				v.Value = t.val; 
			}
		} else SynErr(71);
	}

	void ENCAPSULATION(out ClassEncapsulation e) {
		e = ClassEncapsulation.PUBLIC; 
		if (la.kind == 14) {
			Get();
			e = ClassEncapsulation.PRIVATE; 
		} else if (la.kind == 15) {
			Get();
			e = ClassEncapsulation.PUBLIC; 
		} else SynErr(72);
	}

	void ASSIGNMENTOPERATORS(out AssignmentOperators ao) {
		ao = AssignmentOperators.Equals; 
		if (la.kind == 16) {
			Get();
			ao = AssignmentOperators.PlusEquals; 
		} else if (la.kind == 17) {
			Get();
			ao = AssignmentOperators.MinusEquals; 
		} else if (la.kind == 18) {
			Get();
			ao = AssignmentOperators.MultipliedEquals; 
		} else if (la.kind == 19) {
			Get();
			ao = AssignmentOperators.DividedEquals; 
		} else if (la.kind == 20) {
			Get();
			ao = AssignmentOperators.Equals; 
		} else SynErr(73);
	}

	void OOS() {
		oosTreeBase = new OosContainer(); BaseLangObject blo; 
		while (la.kind == 21 || la.kind == 24 || la.kind == 31) {
			if (la.kind == 21) {
				NAMESPACE(out blo);
				oosTreeBase.addChild(blo); 
			} else if (la.kind == 24) {
				CLASS(out blo);
				oosTreeBase.addChild(blo); 
			} else if (scanner.FollowedBy("function")) {
				GLOBALFUNCTION(out blo);
				oosTreeBase.addChild(blo); 
			} else {
				GLOBALVARIABLE(out blo);
				oosTreeBase.addChild(blo); 
			}
		}
	}

	void NAMESPACE(out BaseLangObject bloOut) {
		var n = new OosNamespace(); bloOut = (BaseLangObject)n; BaseLangObject blo; 
		Expect(21);
		Expect(5);
		n.Name = t.val; 
		Expect(22);
		while (la.kind == 21 || la.kind == 24 || la.kind == 31) {
			if (la.kind == 21) {
				NAMESPACE(out blo);
				n.addChild(blo); 
			} else if (la.kind == 24) {
				CLASS(out blo);
				n.addChild(blo); 
			} else if (scanner.FollowedBy("function")) {
				GLOBALFUNCTION(out blo);
				n.addChild(blo); 
			} else {
				GLOBALVARIABLE(out blo);
				n.addChild(blo); 
			}
		}
		Expect(23);
		if (la.kind == 7) {
			Get();
			while (la.kind == 7) {
				Get();
			}
		}
	}

	void CLASS(out BaseLangObject bloOut) {
		var c = new OosClass(); bloOut = (BaseLangObject)c; BaseLangObject blo; 
		Expect(24);
		Expect(5);
		c.Name = t.val; 
		if (la.kind == 25) {
			Get();
			Expect(5);
			c.ParentClasses.Add(t.val); 
			while (la.kind == 9) {
				Get();
				Expect(5);
				c.ParentClasses.Add(t.val); 
			}
		}
		Expect(22);
		while (StartOf(2)) {
			if (la.kind == 24) {
				CLASS(out blo);
				c.addChild(blo); blo.setParent(c); 
			} else if (la.kind == 28) {
				CLASSCONSTRUCTOR(out blo);
				c.addChild(blo); blo.setParent(c); ((BaseFunctionObject)blo).Name = "constructor"; 
			} else if (la.kind == 31) {
				if (scanner.FollowedBy("function")) {
					GLOBALFUNCTION(out blo);
					c.addChild(blo); blo.setParent(c); 
				} else {
					GLOBALVARIABLE(out blo);
					c.addChild(blo); blo.setParent(c); 
				}
			} else {
				if (scanner.FollowedByWO("function")) {
					CLASSFUNCTION(out blo);
					c.addChild(blo); blo.setParent(c); 
				} else {
					CLASSVARIABLE(out blo);
					c.addChild(blo); blo.setParent(c); 
				}
			}
		}
		Expect(23);
		if (la.kind == 7) {
			Get();
			while (la.kind == 7) {
				Get();
			}
		}
	}

	void GLOBALFUNCTION(out BaseLangObject bloOut) {
		var gf = new OosGlobalFunction(); bloOut = (BaseLangObject)gf; BaseLangObject blo; ListString argL;
		Expect(31);
		Expect(29);
		Expect(5);
		gf.Name = t.val; 
		ARGLIST(out argL);
		gf.ArgList = argL.getList(); 
		Expect(22);
		while (StartOf(3)) {
			if (StartOf(4)) {
				CODEINSTRUCTION(out blo);
				gf.addChild(blo); 
				Expect(7);
				while (la.kind == 7) {
					Get();
				}
			} else {
				CODEINSTRUCTION_OPTIONALSC(out blo);
				gf.addChild(blo); 
				if (la.kind == 7) {
					Get();
					while (la.kind == 7) {
						Get();
					}
				}
			}
		}
		Expect(23);
		if (la.kind == 7) {
			Get();
			while (la.kind == 7) {
				Get();
			}
		}
	}

	void GLOBALVARIABLE(out BaseLangObject bloOut) {
		var gv = new OosGlobalVariable(); bloOut = (BaseLangObject)gv; BaseLangObject blo; 
		Expect(31);
		Expect(32);
		Expect(5);
		gv.Name = t.val; 
		if (la.kind == 20) {
			Get();
			EXPRESSION(out blo);
			gv.Value = blo; 
		}
		Expect(7);
		while (la.kind == 7) {
			Get();
		}
	}

	void CLASSCONSTRUCTOR(out BaseLangObject bloOut) {
		var cf = new OosClassFunction(); cf.Encapsulation = ClassEncapsulation.PUBLIC; bloOut = (BaseLangObject)cf; BaseLangObject blo; ListString argL;
		Expect(28);
		ARGLIST(out argL);
		cf.ArgList = argL.getList(); 
		Expect(22);
		while (StartOf(3)) {
			if (StartOf(4)) {
				CODEINSTRUCTION(out blo);
				cf.addChild(blo); 
				Expect(7);
				while (la.kind == 7) {
					Get();
				}
			} else {
				CODEINSTRUCTION_OPTIONALSC(out blo);
				cf.addChild(blo); 
				if (la.kind == 7) {
					Get();
					while (la.kind == 7) {
						Get();
					}
				}
			}
		}
		Expect(23);
		if (la.kind == 7) {
			Get();
			while (la.kind == 7) {
				Get();
			}
		}
	}

	void CLASSFUNCTION(out BaseLangObject bloOut) {
		var cf = new OosClassFunction(); bloOut = (BaseLangObject)cf; BaseLangObject blo; ListString argL; ClassEncapsulation e;
		if (la.kind == 14 || la.kind == 15) {
			ENCAPSULATION(out e);
			cf.Encapsulation = e; 
		}
		Expect(29);
		if (la.kind == 30) {
			Get();
			cf.OverrideBase = true; 
		}
		Expect(5);
		cf.Name = t.val; 
		ARGLIST(out argL);
		cf.ArgList = argL.getList(); 
		Expect(22);
		while (StartOf(3)) {
			if (StartOf(4)) {
				CODEINSTRUCTION(out blo);
				cf.addChild(blo); 
				Expect(7);
				while (la.kind == 7) {
					Get();
				}
			} else {
				CODEINSTRUCTION_OPTIONALSC(out blo);
				cf.addChild(blo); 
				if (la.kind == 7) {
					Get();
					while (la.kind == 7) {
						Get();
					}
				}
			}
		}
		Expect(23);
		if (la.kind == 7) {
			Get();
			while (la.kind == 7) {
				Get();
			}
		}
	}

	void CLASSVARIABLE(out BaseLangObject bloOut) {
		var cv = new OosClassVariable(); bloOut = (BaseLangObject)cv; ClassEncapsulation e; BaseLangObject blo; 
		if (la.kind == 14 || la.kind == 15) {
			ENCAPSULATION(out e);
			cv.Encapsulation = e; 
		}
		Expect(32);
		Expect(5);
		cv.Name = t.val; 
		if (la.kind == 20) {
			Get();
			EXPRESSION(out blo);
			cv.Value = blo; 
		}
		Expect(7);
		while (la.kind == 7) {
			Get();
		}
	}

	void ARGLIST(out ListString l) {
		l = new ListString(); 
		Expect(26);
		if (la.kind == 6) {
			Get();
			l.Add(t.val); 
			while (la.kind == 9) {
				Get();
				Expect(6);
				l.Add(t.val); 
			}
		}
		Expect(27);
	}

	void CODEINSTRUCTION(out BaseLangObject bloOut) {
		bloOut = null; 
		if (la.kind == 60) {
			THROWINSTRUCTION(out bloOut);
		} else if (la.kind == 61) {
			RETURNINSTRUCTION(out bloOut);
		} else if (la.kind == 66) {
			BREAKINSTRUCTION(out bloOut);
		} else if (la.kind == 47 || la.kind == 48) {
			TYPEOF(out bloOut);
		} else if (la.kind == 32) {
			LOCALVARIABLE(out bloOut);
		} else if (scanner.FollowedBy("(")) {
			FUNCTIONCALL(out bloOut);
		} else if (scanner.FollowedBy(new string[] { "instanceof", "instanceOf" })) {
			INSTANCEOF(out bloOut);
		} else if (la.kind == 5 || la.kind == 6) {
			ASSIGNMENT(out bloOut);
		} else if (la.kind == 62) {
			ISSET(out bloOut);
		} else if (la.kind == 52) {
			SQFCALL(out bloOut);
		} else SynErr(74);
	}

	void CODEINSTRUCTION_OPTIONALSC(out BaseLangObject bloOut) {
		bloOut = null; 
		switch (la.kind) {
		case 53: {
			FORLOOP(out bloOut);
			break;
		}
		case 54: {
			WHILELOOP(out bloOut);
			break;
		}
		case 67: {
			IFELSE(out bloOut);
			break;
		}
		case 58: {
			TRYCATCH(out bloOut);
			break;
		}
		case 57: {
			SWITCH(out bloOut);
			break;
		}
		case 63: {
			NATIVEMULTI(out bloOut);
			break;
		}
		default: SynErr(75); break;
		}
	}

	void EXPRESSION(out BaseLangObject bloOut) {
		bloOut = null; 
		if (la.kind == 26) {
			Get();
			EXPRESSION_HELPER(out bloOut);
			Expect(27);
		} else if (StartOf(5)) {
			EXPRESSION_HELPER(out bloOut);
		} else SynErr(76);
	}

	void EXPRESSION_OPERATOR(out ExpressionOperator eo) {
		eo = ExpressionOperator.NA; 
		switch (la.kind) {
		case 33: {
			Get();
			eo = ExpressionOperator.And; 
			if (la.kind == 33) {
				Get();
				eo = ExpressionOperator.AndAnd; 
			}
			break;
		}
		case 34: {
			Get();
			eo = ExpressionOperator.Or; 
			if (la.kind == 34) {
				Get();
				eo = ExpressionOperator.OrOr; 
			}
			break;
		}
		case 35: {
			Get();
			eo = ExpressionOperator.Equals; 
			if (la.kind == 20) {
				Get();
				eo = ExpressionOperator.ExplicitEquals; 
			}
			break;
		}
		case 36: {
			Get();
			eo = ExpressionOperator.Plus; 
			break;
		}
		case 37: {
			Get();
			eo = ExpressionOperator.Minus; 
			break;
		}
		case 38: {
			Get();
			eo = ExpressionOperator.Multiplication; 
			break;
		}
		case 39: {
			Get();
			eo = ExpressionOperator.Division; 
			break;
		}
		case 40: {
			Get();
			eo = ExpressionOperator.Larger; 
			break;
		}
		case 41: {
			Get();
			eo = ExpressionOperator.LargerEquals; 
			break;
		}
		case 42: {
			Get();
			eo = ExpressionOperator.Smaller; 
			break;
		}
		case 43: {
			Get();
			eo = ExpressionOperator.SmallerEquals; 
			break;
		}
		default: SynErr(77); break;
		}
	}

	void EXPRESSION_SINGLE(out BaseLangObject bloOut) {
		bloOut = null; 
		switch (la.kind) {
		case 62: {
			ISSET(out bloOut);
			break;
		}
		case 1: case 2: case 3: case 4: case 8: case 10: case 12: case 13: {
			VALUE(out bloOut);
			break;
		}
		case 51: {
			OBJECTCREATION(out bloOut);
			break;
		}
		case 5: case 6: {
			if (scanner.FollowedBy("(")) {
				FUNCTIONCALL(out bloOut);
			} else if (la.kind == 5) {
				Get();
				bloOut = new OosVariable(t.val); 
			} else {
				Get();
				bloOut = new OosVariable(t.val); 
			}
			break;
		}
		case 52: {
			SQFCALL(out bloOut);
			break;
		}
		case 65: {
			NATIVESINGLE(out bloOut);
			break;
		}
		default: SynErr(78); break;
		}
	}

	void ISSET(out BaseLangObject bloOut) {
		var obj = new OosIsSet(); bloOut = (BaseLangObject)obj; BaseLangObject blo; 
		Expect(62);
		Expect(26);
		EXPRESSION(out blo);
		obj.Expression = blo; 
		Expect(27);
	}

	void OBJECTCREATION(out BaseLangObject bloOut) {
		var oc = new OosObjectCreation(); bloOut = (BaseLangObject)oc; ListBaseLangObject cl; 
		Expect(51);
		Expect(5);
		oc.Identifier = new OosVariable(t.val); 
		CALLLIST(out cl);
		oc.Children.AddRange(cl.getList()); 
	}

	void FUNCTIONCALL(out BaseLangObject bloOut) {
		var fc = new OosFunctionCall(); bloOut = (BaseLangObject)fc; ListBaseLangObject cl; 
		if (la.kind == 5) {
			Get();
			fc.Identifier = new OosVariable(t.val); 
		} else if (la.kind == 6) {
			Get();
			fc.Identifier = new OosVariable(t.val); 
		} else SynErr(79);
		CALLLIST(out cl);
		fc.Children.AddRange(cl.getList()); 
	}

	void SQFCALL(out BaseLangObject bloOut) {
		var obj = new OosSqfCall(); bloOut = (BaseLangObject)obj; BaseLangObject blo; 
		Expect(52);
		if (la.kind == 26) {
			Get();
			while (StartOf(6)) {
				EXPRESSION(out blo);
				obj.addChild(blo); 
			}
			Expect(27);
		}
		Expect(5);
		obj.InstructionName = t.val; obj.markEnd(); 
		if (la.kind == 26) {
			Get();
			while (StartOf(6)) {
				EXPRESSION(out blo);
				obj.addChild(blo); 
			}
			Expect(27);
		}
	}

	void NATIVESINGLE(out BaseLangObject bloOut) {
		var obj = new OosNative(); bloOut = (BaseLangObject)obj; 
		Expect(65);
		Expect(26);
		Get();
		obj.nativeCode += t.val; 
		while (StartOf(7)) {
			Get();
			obj.nativeCode += " " + t.val; 
		}
		Expect(27);
	}

	void EXPRESSION_HELPER(out BaseLangObject bloOut) {
		var e = new OosExpression(); bloOut = (BaseLangObject)e; BaseLangObject blo; ExpressionOperator eo; 
		if (la.kind == 44) {
			Get();
			e.Negate = true; 
		}
		EXPRESSION_SINGLE(out blo);
		e.LInstruction = blo; 
		while (StartOf(8)) {
			EXPRESSION_OPERATOR(out eo);
			e.Op = eo; 
			EXPRESSION(out blo);
			e.RInstruction = blo; 
		}
	}

	void LOCALVARIABLE(out BaseLangObject bloOut) {
		var lv = new OosLocalVariable(); bloOut = (BaseLangObject)lv; BaseLangObject blo; 
		Expect(32);
		Expect(6);
		lv.Name = t.val; 
		if (la.kind == 20) {
			Get();
			EXPRESSION(out blo);
			lv.Value = blo; 
		}
	}

	void ASSIGNMENT(out BaseLangObject bloOut) {
		string v1 = ""; string v2 = ""; bloOut = null; BaseLangObject blo; 
		if (la.kind == 5) {
			Get();
			v1 = t.val; 
		} else if (la.kind == 6) {
			Get();
			v1 = t.val; 
		} else SynErr(80);
		if (la.kind == 10) {
			Get();
			Expect(1);
			v2 = t.val; 
			Expect(11);
		}
		if (la.kind == 45 || la.kind == 46) {
			var obj = new OosQuickAssignment(); obj.Variable = new OosVariable(v1); obj.ArrayPosition = v2; bloOut = (BaseLangObject)obj; 
			if (la.kind == 45) {
				Get();
				obj.QuickAssignmentType = QuickAssignmentTypes.PlusPlus; 
			} else {
				Get();
				obj.QuickAssignmentType = QuickAssignmentTypes.MinusMinus; 
			}
		} else if (StartOf(9)) {
			var obj = new OosVariableAssignment(); obj.Variable = new OosVariable(v1); obj.ArrayPosition = v2; bloOut = (BaseLangObject)obj; AssignmentOperators ao; 
			ASSIGNMENTOPERATORS(out ao);
			obj.AssignmentOperator = ao; 
			EXPRESSION(out blo);
			obj.Value = blo; 
		} else SynErr(81);
	}

	void CALLLIST(out ListBaseLangObject l) {
		l = new ListBaseLangObject(); BaseLangObject blo; 
		Expect(26);
		if (StartOf(6)) {
			EXPRESSION(out blo);
			l.Add(blo); 
			while (la.kind == 9) {
				Get();
				EXPRESSION(out blo);
				l.Add(blo); 
			}
		}
		Expect(27);
	}

	void TYPEOF(out BaseLangObject bloOut) {
		var to = new OosTypeOf(); bloOut = (BaseLangObject)to; BaseLangObject blo; 
		if (la.kind == 47) {
			Get();
		} else if (la.kind == 48) {
			Get();
		} else SynErr(82);
		Expect(26);
		EXPRESSION(out blo);
		to.Argument = blo; 
		Expect(27);
	}

	void INSTANCEOF(out BaseLangObject bloOut) {
		var io = new OosInstanceOf(); bloOut = (BaseLangObject)io; BaseLangObject blo; 
		EXPRESSION(out blo);
		io.LArgument = blo; 
		if (la.kind == 49) {
			Get();
		} else if (la.kind == 50) {
			Get();
		} else SynErr(83);
		EXPRESSION(out blo);
		io.RArgument = blo; 
	}

	void FORLOOP(out BaseLangObject bloOut) {
		var fl = new OosForLoop(); bloOut = (BaseLangObject)fl; BaseLangObject blo; 
		Expect(53);
		Expect(26);
		if (la.kind == 5 || la.kind == 6 || la.kind == 32) {
			if (la.kind == 5 || la.kind == 6) {
				ASSIGNMENT(out blo);
				fl.Arg1 = blo; 
			} else {
				LOCALVARIABLE(out blo);
				fl.Arg1 = blo; 
			}
		}
		Expect(7);
		EXPRESSION(out blo);
		fl.Arg2 = blo; 
		Expect(7);
		if (la.kind == 5 || la.kind == 6) {
			ASSIGNMENT(out blo);
			fl.Arg3 = blo; 
		}
		Expect(27);
		CODEBODY(bloOut);
	}

	void WHILELOOP(out BaseLangObject bloOut) {
		var wl = new OosWhileLoop(); bloOut = (BaseLangObject)wl; BaseLangObject blo; 
		Expect(54);
		Expect(26);
		EXPRESSION(out blo);
		wl.Expression = blo; 
		Expect(27);
		CODEBODY(bloOut);
	}

	void IFELSE(out BaseLangObject bloOut) {
		var ie = new OosIfElse(); bloOut = (BaseLangObject)ie; BaseLangObject blo; 
		Expect(67);
		Expect(26);
		EXPRESSION(out blo);
		ie.Expression = blo; 
		Expect(27);
		CODEBODY(bloOut);
		ie.markEnd(); 
		if (la.kind == 68) {
			Get();
			CODEBODY(bloOut);
		}
	}

	void TRYCATCH(out BaseLangObject bloOut) {
		var tc = new OosTryCatch(); bloOut = (BaseLangObject)tc; 
		Expect(58);
		CODEBODY(bloOut);
		Expect(59);
		tc.markEnd(); 
		Expect(26);
		Expect(5);
		tc.CatchVariable = new OosVariable(t.val); 
		Expect(27);
		CODEBODY(bloOut);
	}

	void SWITCH(out BaseLangObject bloOut) {
		var s = new OosSwitch(); bloOut = (BaseLangObject)s; BaseLangObject blo; 
		Expect(57);
		Expect(26);
		EXPRESSION(out blo);
		s.Expression = blo; 
		Expect(27);
		Expect(22);
		while (la.kind == 55 || la.kind == 56) {
			CASE(out blo);
			s.addChild(blo); 
		}
		Expect(23);
	}

	void NATIVEMULTI(out BaseLangObject bloOut) {
		var obj = new OosNative(); bloOut = (BaseLangObject)obj; 
		Expect(63);
		Get();
		obj.nativeCode += t.val; 
		while (StartOf(10)) {
			Get();
			obj.nativeCode += " " + t.val; 
		}
		Expect(64);
	}

	void THROWINSTRUCTION(out BaseLangObject bloOut) {
		var tr = new OosThrow(); bloOut = (BaseLangObject)tr; BaseLangObject blo; 
		Expect(60);
		EXPRESSION(out blo);
		tr.Expression = blo; 
	}

	void RETURNINSTRUCTION(out BaseLangObject bloOut) {
		var tr = new OosReturn(); bloOut = (BaseLangObject)tr; BaseLangObject blo; 
		Expect(61);
		EXPRESSION(out blo);
		tr.Expression = blo; 
	}

	void BREAKINSTRUCTION(out BaseLangObject bloOut) {
		var b = new OosBreak(); bloOut = (BaseLangObject)b; 
		Expect(66);
	}

	void CODEBODY(BaseLangObject bloOut) {
		BaseLangObject blo; 
		if (StartOf(3)) {
			if (StartOf(4)) {
				CODEINSTRUCTION(out blo);
				bloOut.addChild(blo); 
				Expect(7);
				while (la.kind == 7) {
					Get();
				}
			} else {
				CODEINSTRUCTION_OPTIONALSC(out blo);
				bloOut.addChild(blo); 
				if (la.kind == 7) {
					Get();
					while (la.kind == 7) {
						Get();
					}
				}
			}
		} else if (la.kind == 22) {
			Get();
			while (StartOf(3)) {
				if (StartOf(4)) {
					CODEINSTRUCTION(out blo);
					bloOut.addChild(blo); 
					Expect(7);
					while (la.kind == 7) {
						Get();
					}
				} else {
					CODEINSTRUCTION_OPTIONALSC(out blo);
					bloOut.addChild(blo); 
					if (la.kind == 7) {
						Get();
						while (la.kind == 7) {
							Get();
						}
					}
				}
			}
			Expect(23);
		} else SynErr(84);
	}

	void CASE(out BaseLangObject bloOut) {
		var c = new OosCase(); bloOut = (BaseLangObject)c; BaseLangObject blo; 
		if (la.kind == 55) {
			Get();
			VALUE(out blo);
			c.Value = blo; 
		} else if (la.kind == 56) {
			Get();
		} else SynErr(85);
		Expect(25);
		CODEBODY(bloOut);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		OOS();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_x,_T,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_T, _T,_x,_x,_T, _T,_T,_T,_x, _x,_T,_T,_x, _T,_T,_T,_T, _x,_T,_T,_T, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_x,_T,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_T, _T,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _x,_T,_T,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_x,_T,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _T,_x,_T,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_x, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_T,_T,_T, _T,_T,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "UINTEGER expected"; break;
			case 2: s = "UDOUBLE expected"; break;
			case 3: s = "INTEGER expected"; break;
			case 4: s = "DOUBLE expected"; break;
			case 5: s = "FQIDENT expected"; break;
			case 6: s = "LOCALIDENT expected"; break;
			case 7: s = "LINETERMINATOR expected"; break;
			case 8: s = "STRING expected"; break;
			case 9: s = "COMMA expected"; break;
			case 10: s = "\"[\" expected"; break;
			case 11: s = "\"]\" expected"; break;
			case 12: s = "\"true\" expected"; break;
			case 13: s = "\"false\" expected"; break;
			case 14: s = "\"private\" expected"; break;
			case 15: s = "\"public\" expected"; break;
			case 16: s = "\"+=\" expected"; break;
			case 17: s = "\"-=\" expected"; break;
			case 18: s = "\"*=\" expected"; break;
			case 19: s = "\"/=\" expected"; break;
			case 20: s = "\"=\" expected"; break;
			case 21: s = "\"namespace\" expected"; break;
			case 22: s = "\"{\" expected"; break;
			case 23: s = "\"}\" expected"; break;
			case 24: s = "\"class\" expected"; break;
			case 25: s = "\":\" expected"; break;
			case 26: s = "\"(\" expected"; break;
			case 27: s = "\")\" expected"; break;
			case 28: s = "\"constructor\" expected"; break;
			case 29: s = "\"function\" expected"; break;
			case 30: s = "\"override\" expected"; break;
			case 31: s = "\"static\" expected"; break;
			case 32: s = "\"auto\" expected"; break;
			case 33: s = "\"&\" expected"; break;
			case 34: s = "\"|\" expected"; break;
			case 35: s = "\"==\" expected"; break;
			case 36: s = "\"+\" expected"; break;
			case 37: s = "\"-\" expected"; break;
			case 38: s = "\"*\" expected"; break;
			case 39: s = "\"/\" expected"; break;
			case 40: s = "\">\" expected"; break;
			case 41: s = "\">=\" expected"; break;
			case 42: s = "\"<\" expected"; break;
			case 43: s = "\"<=\" expected"; break;
			case 44: s = "\"!\" expected"; break;
			case 45: s = "\"++\" expected"; break;
			case 46: s = "\"--\" expected"; break;
			case 47: s = "\"typeof\" expected"; break;
			case 48: s = "\"typeOf\" expected"; break;
			case 49: s = "\"instanceof\" expected"; break;
			case 50: s = "\"instanceOf\" expected"; break;
			case 51: s = "\"new\" expected"; break;
			case 52: s = "\"SQF\" expected"; break;
			case 53: s = "\"for\" expected"; break;
			case 54: s = "\"while\" expected"; break;
			case 55: s = "\"case\" expected"; break;
			case 56: s = "\"default\" expected"; break;
			case 57: s = "\"switch\" expected"; break;
			case 58: s = "\"try\" expected"; break;
			case 59: s = "\"catch\" expected"; break;
			case 60: s = "\"throw\" expected"; break;
			case 61: s = "\"return\" expected"; break;
			case 62: s = "\"isset\" expected"; break;
			case 63: s = "\"startnative\" expected"; break;
			case 64: s = "\"endnative\" expected"; break;
			case 65: s = "\"native\" expected"; break;
			case 66: s = "\"break\" expected"; break;
			case 67: s = "\"if\" expected"; break;
			case 68: s = "\"else\" expected"; break;
			case 69: s = "??? expected"; break;
			case 70: s = "invalid SCALAR"; break;
			case 71: s = "invalid VALUE"; break;
			case 72: s = "invalid ENCAPSULATION"; break;
			case 73: s = "invalid ASSIGNMENTOPERATORS"; break;
			case 74: s = "invalid CODEINSTRUCTION"; break;
			case 75: s = "invalid CODEINSTRUCTION_OPTIONALSC"; break;
			case 76: s = "invalid EXPRESSION"; break;
			case 77: s = "invalid EXPRESSION_OPERATOR"; break;
			case 78: s = "invalid EXPRESSION_SINGLE"; break;
			case 79: s = "invalid FUNCTIONCALL"; break;
			case 80: s = "invalid ASSIGNMENT"; break;
			case 81: s = "invalid ASSIGNMENT"; break;
			case 82: s = "invalid TYPEOF"; break;
			case 83: s = "invalid INSTANCEOF"; break;
			case 84: s = "invalid CODEBODY"; break;
			case 85: s = "invalid CASE"; break;

			default: s = "error " + n; break;
		}
        Logger.Instance.log(Logger.LogLevel.ERROR, String.Format(errMsgFormat, line, col, s));
		//errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
        Logger.Instance.log(Logger.LogLevel.ERROR, String.Format(errMsgFormat, line, col, s));
		//errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
        Logger.Instance.log(Logger.LogLevel.ERROR, s);
		//errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
        Logger.Instance.log(Logger.LogLevel.WARNING, String.Format(errMsgFormat, line, col, s));
		//errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
        Logger.Instance.log(Logger.LogLevel.WARNING, s);
		//errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
