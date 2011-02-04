using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using PetriNetCore;



using System;



public partial class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _ident = 2;
	public const int _pnStart = 3;
	public const int _end = 4;
	public const int _placeFromEnd = 5;
	public const int _placeToEnd = 6;
	public const int _tranFromEnd = 7;
	public const int _tranToEnd = 8;
	public const int _inhibitorMark = 9;
	public const int _arcLine = 10;
	public const int _sep = 11;
	public const int _idListStart = 12;
	public const int _idListEnd = 13;
	public const int _listSep = 14;
	public const int maxT = 18;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public PetriNetCore.CreatePetriNet Builder { get; set; }



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
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

	
	void PetriNetSpec() {
		
		Expect(3);
		Expect(2);
		Builder = CreatePetriNet.Called(t.val);
		
		Expect(15);
		ArcSetSpec();
		while (la.kind == 2 || la.kind == 12) {
			ArcSetSpec();
		}
		Expect(16);
		while (la.kind == 2) {
			MarkingSpec();
		}
	}

	void ArcSetSpec() {
		List<string> src = new List<string>();
		List<string> dst = new List<string>();
		Debug.Assert(Builder != null);
		bool isInArc = true;
		int weight = 1;
		bool isInhibitor = false;
		
		if (la.kind == 2 || la.kind == 12) {
			SrcName(ref src);
			while (la.kind == 5 || la.kind == 7) {
				ArcDetail(ref isInArc, ref weight, ref isInhibitor);
				DstName(ref dst);
				List<string> t = isInArc ? dst : src;
				List<string> p = isInArc ? src : dst;
				                           Builder.GenerateArc(p,t,weight,isInhibitor, isInArc);
				
			}
			Expect(11);
		} else if (la.kind == 2 || la.kind == 12) {
			while (la.kind == 2 || la.kind == 12) {
				SrcName(ref src);
				ArcDetail(ref isInArc, ref weight, ref isInhibitor);
			}
			DstName(ref dst);
			List<string> t = isInArc ? dst : src;
			List<string> p = isInArc ? src : dst;
			                           Builder.GenerateArc(p,t,weight,isInhibitor, isInArc);
			
			Expect(11);
		} else SynErr(19);
	}

	void MarkingSpec() {
		MarkingEntry();
		while (la.kind == 2) {
			MarkingEntry();
		}
	}

	void SrcName(ref List<string> src) {
		StringList(ref src);
	}

	void ArcDetail(ref bool isInArc, ref int weight, ref bool inhib) {
		weight = 1;
		isInArc = true;
		inhib = false;
		
		if (la.kind == 5) {
			InArcDetail(ref inhib, ref weight);
			isInArc = true;
			
		} else if (la.kind == 7) {
			OutArcDetail(ref inhib, ref weight);
			isInArc = false;
			
		} else SynErr(20);
	}

	void DstName(ref List<string> dst) {
		StringList(ref dst);
	}

	void MarkingEntry() {
		string id = "", tokens = "";
		Expect(2);
		id = t.val;
		Expect(17);
		Expect(1);
		tokens = t.val;
		Builder.WithPlace(id).HavingMarking(int.Parse(tokens)).Done();
		
		Expect(11);
	}

	void StringList(ref List<string> src) {
		if (la.kind == 2) {
			Get();
			src.Add(t.val); 
		} else if (la.kind == 12) {
			Get();
			Expect(2);
			src.Add(t.val); 
			while (la.kind == 14) {
				Get();
				Expect(2);
				src.Add(t.val); 
			}
			Expect(13);
		} else SynErr(21);
	}

	void InArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(5);
		while (la.kind == 10) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 10) {
			Get();
		}
		if (la.kind == 9) {
			Get();
			inhib = true; 
		}
		while (la.kind == 10) {
			Get();
		}
		Expect(8);
	}

	void OutArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(7);
		while (la.kind == 10) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 10) {
			Get();
		}
		Expect(6);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PetriNetSpec();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
  public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
  
	public void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "number expected"; break;
			case 2: s = "ident expected"; break;
			case 3: s = "pnStart expected"; break;
			case 4: s = "end expected"; break;
			case 5: s = "placeFromEnd expected"; break;
			case 6: s = "placeToEnd expected"; break;
			case 7: s = "tranFromEnd expected"; break;
			case 8: s = "tranToEnd expected"; break;
			case 9: s = "inhibitorMark expected"; break;
			case 10: s = "arcLine expected"; break;
			case 11: s = "sep expected"; break;
			case 12: s = "idListStart expected"; break;
			case 13: s = "idListEnd expected"; break;
			case 14: s = "listSep expected"; break;
			case 15: s = "\"<\" expected"; break;
			case 16: s = "\">\" expected"; break;
			case 17: s = "\"=\" expected"; break;
			case 18: s = "??? expected"; break;
			case 19: s = "invalid ArcSetSpec"; break;
			case 20: s = "invalid ArcDetail"; break;
			case 21: s = "invalid StringList"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

