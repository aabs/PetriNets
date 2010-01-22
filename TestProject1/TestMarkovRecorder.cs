/*
 * Created by SharpDevelop.
 * User: Andrew Matthews
 * Date: 13/08/2009
 * Time: 8:45 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetriNetCore;

namespace Tests
{
	[TestClass]
	public class TestMarkovRecorder
	{
		[TestMethod]
		public void Test()
		{
			MarkovModelRecorder recorder = new MarkovModelRecorder(10);
			foreach(var x in new[]{7,1,8,2,7,1,8,2,1})
			{
				recorder.Register(x);
			}
			Assert.AreEqual(2, recorder.Occurences[7,1]);
		}
		
		[TestMethod]
		public void Test2()
		{
			MarkovModelRecorder recorder = new MarkovModelRecorder(10);
			foreach(var x in new[]{1,3,6,0,7,2,3,4,8,7,1,0,1,3,0,2,7,0,8,7,6,7,6,5,6,5,7,5,4,3,4,3,4,5,3,2,0
			        		,9,8,7,0,0,8,7,6,9,6,7,5,6,5,4,5,4,4,3,2,4,5,2,2,3,3})
			{
				recorder.Register(x);
			}
			foreach(var y in recorder.Replay(7).Take(100)){
				Debug.WriteLine(y);
			}
		}
		
		[TestMethod]
		public void Test3()
		{
			MarkovModelRecorder recorder = new MarkovModelRecorder(10);
			foreach(var x in new[]{1,2,3,4,5,6,7,8,9,1})
			{
				recorder.Register(x);
			}
			foreach(var y in recorder.Replay(7).Take(100)){
				Debug.WriteLine(y);
			}
		}
	}
}
