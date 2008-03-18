/*
 *   Copyright (c) 2008, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
 *
 *   This file is part of Bless.
 *
 *   Bless is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   Bless is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Bless; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.Collections.Generic;
using NUnit.Framework;

using Bless.Util;

namespace BlessTests.Util
{
	[TestFixture()]
	public class IntervalTreeTests
	{
		
		[Test()]
		public void ITInsertTest()
		{
			IntervalTree<Range> t = new IntervalTree<Range>();
			
			t.Insert(new Range(5,55));
			
			IList<Range> result = t.SearchOverlap(new Range(0, 4));
			
			Assert.AreEqual(0, result.Count, "#1");
			
			result = t.SearchOverlap(new Range(4, 13));
			Assert.AreEqual(1, result.Count, "#2");
			Assert.AreEqual(55, result[0].End, "#2.1");
			
			result = t.SearchOverlap(new Range(57, 68));
			Assert.AreEqual(0, result.Count, "#3");
			
			result = t.SearchOverlap(new Range(3,100));
			
			Assert.AreEqual(1, result.Count, "#4");
			Assert.AreEqual(55, result[0].End, "#2.1");
		}
		
		[Test()]
		public void ITSearchTest()
		{
			IntervalTree<Range> t = new IntervalTree<Range>();
			
			for (int i = 0; i < 1000; i += 4) {
				t.Insert(new Range(i, i + 2));
			}
			
			for (int i = 0 ; i < 1000; i += 8) {
				IList<Range> result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(2, result.Count, "#1."+i.ToString());
			}
			
			for (int i = 2 ; i < 1000; i += 4) {
				IList<Range> result = t.SearchOverlap(new Range(i+1, i + 1));
				Assert.AreEqual(0, result.Count, "#2."+i.ToString());
			}
			
			{
				IList<Range> result = t.SearchOverlap(new Range(0, 1050));
				Assert.AreEqual(250, result.Count, "#3");
			}
		}
		
		[Test()]
		public void ITDuplicateTest()
		{
			IntervalTree<Range> t = new IntervalTree<Range>();
			
			for (int i = 0; i < 1000; i += 1) {
				t.Insert(new Range(0, i));
			}
			
			for (int i = 0 ; i <= 1000; i++) {
				IList<Range> result = t.SearchOverlap(new Range(i, i));
				Assert.AreEqual(1000 - i, result.Count, "#1."+i.ToString());
			}
			
		}
		
		[Test()]
		public void ITDeleteTest()
		{
			IntervalTree<Range> t = new IntervalTree<Range>();
			
			for (int i = 0; i < 1000; i += 4) {
				t.Insert(new Range(i, i + 2));
			}
			
			for (int i = 0 ; i < 1000; i += 8) {
				IList<Range> result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(2, result.Count, "#1."+i.ToString());
				
				t.Delete(new Range(i, i+2));
				
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(1, result.Count, "#2."+i.ToString());
				
				t.Delete(new Range(i+2, i+4));
				
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(1, result.Count, "#3."+i.ToString());
				
				t.Delete(new Range(i+4, i+6));
				
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(0, result.Count, "#4."+i.ToString());
				
				t.Delete(new Range(i+6, i+8));
				
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(0, result.Count, "#5."+i.ToString());
			}
			
		}
		
		[Test()]
		public void ITDeleteDuplicateTest()
		{
			IntervalTree<Range> t = new IntervalTree<Range>();
			
			for (int i = 0; i < 1000; i += 4) {
				t.Insert(new Range(i, i + 3));
				t.Insert(new Range(i, i + 3));
				t.Insert(new Range(i, i + 3));
			}
			
			for (int i = 0; i < 1000; i += 8) {
				IList<Range> result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(6, result.Count, "#5."+i.ToString());
			
				t.Delete(new Range(i, i+3));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(5, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i, i+3));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(4, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i, i+3));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(3, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i, i+3));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(3, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i+4, i+7));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(2, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i+4, i+7));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(1, result.Count, "#5."+i.ToString());
				
				t.Delete(new Range(i+4, i+7));
				result = t.SearchOverlap(new Range(i, i + 7));
				Assert.AreEqual(0, result.Count, "#5."+i.ToString());
			}
		}
			
	}
}
