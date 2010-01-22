/*
 * Created by SharpDevelop.
 * User: Andrew Matthews
 * Date: 12/08/2009
 * Time: 9:38 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dnAnalytics.LinearAlgebra;

namespace Tests
{
	[TestClass]
	public class Test2
	{
		[TestMethod]
		public void Test()
		{
			SparseMatrix sm = new SparseMatrix(200);
			SparseVector sv = new SparseVector(200);
			sv[100] = 268358;
			Assert.IsNotNull(sm);
			Assert.AreEqual(0, sm[100,100]);
			sm[100,100] = 127268;
			Assert.AreEqual(127268, sm[100,100]);
			SparseVector x = sm*sv;
			System.Diagnostics.Debug.WriteLine(x[100]);
		}
	}
}
