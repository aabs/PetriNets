using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using PetriNetCore;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _ident = 2;
	public const int maxT = 17;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public CreatePetriNet Builder {get;set;}

    private static CreatePetriNet GenerateArc(CreatePetriNet builder,
                                                List<string> p,
                                                List<string> t,
                                                int weight,
                                                bool isInhibitor,
                                                bool isIntoTransition)
    {
        Contract.Requires(!p.Any(x => string.IsNullOrWhiteSpace(x)));
        Contract.Requires(!t.Any(x => string.IsNullOrWhiteSpace(x)));
		Contract.Requires(p.Intersect(t).Count() == 0);
		Contract.Requires(weight >= 1);
        var places = p.ToArray();
        var transitions = t.ToArray();
        builder.WithPlaces(places).WithTransitions(transitions);
		foreach(var tran in t){
			var connectionBuilder = builder.With(tran).Weight(weight);
			if (isInhibitor)
			{
				connectionBuilder.AsInhibitor();
			}
			if (isIntoTransition)
			{
				connectionBuilder.FedBy(places);
			}
			else
			{
				connectionBuilder.Feeding(places);
			}
        connectionBuilder.Done();
		}
		return builder;
    }



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
		Expect(4);
		Expect(2);
		Builder = CreatePetriNet.Called(t.val);
		
		Expect(5);
		Expect(6);
		ArcSetSpec();
		while (la.kind == 2 || la.kind == 8) {
			ArcSetSpec();
		}
		Expect(7);
	}

	void ArcSetSpec() {
		List<string> src = new List<string>();
		List<string> dst = new List<string>();
		Debug.Assert(Builder != null);
		bool isInArc = true;
		int weight = 1;
		bool isInhibitor = false;
		
		SrcName(ref src);
		ArcDetail(ref isInArc, ref weight, ref isInhibitor);
		DstName(ref dst);
		List<string> t = isInArc ? dst : src;
		List<string> p = isInArc ? src : dst;
		
											GenerateArc(Builder,p,t,weight,isInhibitor, isInArc);
											
		Expect(5);
	}

	void SrcName(ref List<string> src) {
		StringList(ref src);
	}

	void ArcDetail(ref bool isInArc, ref int weight, ref bool inhib) {
		weight = 1;
		isInArc = true;
		inhib = false;
		
		if (la.kind == 11) {
			InArcDetail(ref inhib, ref weight);
			isInArc = true;
			
		} else if (la.kind == 15) {
			OutArcDetail(ref inhib, ref weight);
			isInArc = false;
			
		} else SynErr(18);
	}

	void DstName(ref List<string> dst) {
		StringList(ref dst);
	}

	void StringList(ref List<string> src) {
		if (la.kind == 2) {
			Get();
			src.Add(t.val); 
		} else if (la.kind == 8) {
			Get();
			Expect(2);
			src.Add(t.val); 
			while (la.kind == 9) {
				Get();
				Expect(2);
				src.Add(t.val); 
			}
			Expect(10);
		} else SynErr(19);
	}

	void InArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(11);
		while (la.kind == 12) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 12) {
			Get();
		}
		if (la.kind == 13) {
			Get();
			inhib = true; 
		}
		while (la.kind == 12) {
			Get();
		}
		Expect(14);
	}

	void OutArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(15);
		while (la.kind == 12) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 12) {
			Get();
		}
		Expect(16);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PetriNetSpec();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

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
			case 3: s = "\"PetriNet\" expected"; break;
			case 4: s = "\":\" expected"; break;
			case 5: s = "\";\" expected"; break;
			case 6: s = "\"Graph\" expected"; break;
			case 7: s = "\"End\" expected"; break;
			case 8: s = "\"{\" expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\"}\" expected"; break;
			case 11: s = "\")\" expected"; break;
			case 12: s = "\"-\" expected"; break;
			case 13: s = "\"o\" expected"; break;
			case 14: s = "\"[\" expected"; break;
			case 15: s = "\"]\" expected"; break;
			case 16: s = "\"(\" expected"; break;
			case 17: s = "??? expected"; break;
			case 18: s = "invalid ArcDetail"; break;
			case 19: s = "invalid StringList"; break;

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

