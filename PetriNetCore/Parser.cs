using System.Diagnostics;
using PetriNetCore;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _ident = 2;
	public const int maxT = 9;

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
                                                string p,
                                                string t,
                                                int weight,
                                                bool isInhibitor,
                                                bool isIntoTransition)
    {
        builder.WithPlaces(p).WithTransitions(t);
        var connectionBuilder = builder.With(t).Weight(weight);
        if (isInhibitor)
        {
            connectionBuilder.AsInhibitor();
        }
        if (isIntoTransition)
        {
            connectionBuilder.FedBy(p);
        }
        else
        {
            connectionBuilder.Feeding(p);
        }
        return connectionBuilder.Done();
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

	
	void PnArcLang() {
		string src = "", dst = "";
		Debug.Assert(Builder != null);
		bool isInArc = true;
		int weight = 1;
		bool isInhibitor = false;
		
		SrcName(ref src);
		ArcDetail(ref isInArc, ref weight, ref isInhibitor);
		DstName(ref dst);
		string t = isInArc ? dst.ToString() : src.ToString();
		string p = isInArc ? src.ToString() : dst.ToString();
		
													GenerateArc(Builder,p,t,weight,isInhibitor, isInArc);
													
	}

	void SrcName(ref string src) {
		Expect(2);
		src = t.val;
	}

	void ArcDetail(ref bool isInArc, ref int weight, ref bool inhib) {
		weight = 1;
		isInArc = true;
		inhib = false;
		
		if (la.kind == 3) {
			InArcDetail(ref inhib, ref weight);
			isInArc = true;
			
		} else if (la.kind == 7) {
			OutArcDetail(ref inhib, ref weight);
			isInArc = false;
			
		} else SynErr(10);
	}

	void DstName(ref string dst) {
		Expect(2);
		dst = t.val;
	}

	void InArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(3);
		while (la.kind == 4) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 4) {
			Get();
		}
		if (la.kind == 5) {
			Get();
			inhib = true; 
		}
		Expect(6);
	}

	void OutArcDetail(ref bool inhib, ref int weight) {
		inhib = false;
		Expect(7);
		while (la.kind == 4) {
			Get();
		}
		if (la.kind == 1) {
			Get();
			int.TryParse(t.val, out weight);
			
		}
		while (la.kind == 4) {
			Get();
		}
		Expect(8);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		PnArcLang();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x}

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
			case 3: s = "\")\" expected"; break;
			case 4: s = "\"-\" expected"; break;
			case 5: s = "\"o\" expected"; break;
			case 6: s = "\"[\" expected"; break;
			case 7: s = "\"]\" expected"; break;
			case 8: s = "\"(\" expected"; break;
			case 9: s = "??? expected"; break;
			case 10: s = "invalid ArcDetail"; break;

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

