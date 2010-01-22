/*
 * Created by SharpDevelop.
 * User: Andrew Matthews
 * Date: 13/08/2009
 * Time: 8:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using dnAnalytics.LinearAlgebra;

namespace PetriNetCore
{
	/// <summary>
	/// Description of MarkovModel.
	/// </summary>
	public class MarkovModelRecorder
	{
		int lastSymbol{get;set;}
		public SparseMatrix Occurences {get;set;}
		public MarkovModelRecorder(int symbolRange)
		{
			Occurences = new SparseMatrix(symbolRange);
			lastSymbol = int.MinValue;
		}
		
		public void Register(int symbol)
		{
			if (lastSymbol == int.MinValue) {
				lastSymbol = symbol;
				return;
			}
			Occurences[lastSymbol, symbol]++;
			lastSymbol = symbol;
		}
		
		public static readonly Random r = new Random();
		public IEnumerable<int> Replay()
		{
			return Replay(r.Next(Occurences.Columns));
		}
		
		Dictionary<int, int> GetSuccessors(int symbol)
		{
			var result = new Dictionary<int, int>();
			for (int i = 0; i < Occurences.Columns; i++) {
				if (Occurences[symbol,i] > 0) {
					result[i] = (int)Occurences[symbol,i];
				}
			}
			return result;
		}
		
		public IEnumerable<int> Replay(int symbol)
		{
			var tmpSymbol = symbol;
			while(true)
			{
				var successorSymbols = GetSuccessors(tmpSymbol).OrderByDescending(x => x.Value).ToList();
				if (successorSymbols.Count() == 0) {
					yield break;
				}
				var hits = 0;
				successorSymbols.Foreach(x => hits += x.Value);
				var roll = r.Next(hits);
				var reach = 0;
				foreach(var x in successorSymbols){
					if (reach + x.Value > roll) {
						tmpSymbol = x.Key;
						yield return x.Key;
					}
					else{
						reach += x.Value;
					}
				}
			}
		}
	}
}
