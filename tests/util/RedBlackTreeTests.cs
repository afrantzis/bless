// Created 12:17 PM 16/3/2008
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
using NUnit.Framework;
using Bless.Util;
using System.Collections.Generic;

namespace BlessTests.Util
{
	
	
[TestFixture()]
public class RedBlackTreeTests
{
	
	[Test()]
	public void RBInsertTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int, string>();
		
		rb.Insert(5, "5");
		IList<string> result = rb.Search(5);
		Assert.AreEqual("5", result[0], "#1");
		
		result = rb.Search(8);
		Assert.AreEqual(0, result.Count, "#1");
		
		rb.Insert(100, "100");
		result = rb.Search(100);
		Assert.AreEqual("100", result[0], "#3");
		
		result = rb.Search(5);
		Assert.AreEqual("5", result[0], "#4");
	}
	
	[Test()]
	public void RBSearchTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int, string>();
		
		for (int i = 0; i < 10000; i += 2) {
			rb.Insert(i, i.ToString());
		}
		
		for (int i = 0; i < 10000; i++) {
			IList<string> result = rb.Search(i);
			if ( i % 2 == 0)
				Assert.AreEqual(i.ToString(), result[0], i.ToString());
			else
				Assert.AreEqual(0, result.Count, i.ToString());
		}
		

	}
	
	[Test()]
	public void RBDuplicateTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int, string>();
		
		for (int i = 0; i < 1000; i++) {
			rb.Insert(0, i.ToString());
		}
		
		IList<string> result = rb.Search(0);
		
		Assert.AreEqual(1000, result.Count, "#1");
		
	}
	
	[Test()]
	public void RBDeleteTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int, string>();
		
		rb.Insert(0, "0");
		IList<string> result = rb.Search(0);
		Assert.AreEqual(1, result.Count, "#1");
		Assert.AreEqual("0", result[0], "#1.1");
		
		rb.Delete(0);
		
		result = rb.Search(0);
		Assert.AreEqual(0, result.Count, "#2");
		
		rb.Delete(0);
	}
	
	[Test()]
	public void RBDelete1Test()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int, string>();
		
		for (int i = 0; i < 10; i++) {
			rb.Insert(i, i.ToString());
		}
		
		for (int i = 0; i < 10; i++) {
			IList<string> result = rb.Search(i);
			Assert.AreEqual(1, result.Count, "#1.1:" + i.ToString());
			Assert.AreEqual(i.ToString(), result[0], "#1.2:" + i.ToString());
		}
		
		
		for (int i = 0; i < 10; i+=2) {
			rb.Delete(i);
		}
		
		for (int i = 0; i < 10; i++) {
			IList<string> result = rb.Search(i);
			if (i % 2 == 0)
				Assert.AreEqual(0, result.Count, "#2.1:" + i.ToString());
			else {
				Assert.AreEqual(1, result.Count, "#2.2:" + i.ToString());
				Assert.AreEqual(i.ToString(), result[0], "#2.3:" + i.ToString());
			}
		}
		
	}
	
	private void RBAssertPresent(RedBlackTree<int, string> rb, int s, int e)
	{
		for (int i = s; i <= e; i++) {
			IList<string> result = rb.Search(i);
			Assert.AreEqual(1, result.Count, string.Format("#{0}->{1}@{2}", s, e, i));
			Assert.AreEqual(i.ToString(), result[0], string.Format("#{0}->{1}@{2}", s, e, i));
		}
	}
	
	private void RBAssertNotPresent(RedBlackTree<int, string> rb, int s, int e)
	{
		for (int i = s; i <= e; i++) {
			IList<string> result = rb.Search(i);
			Assert.AreEqual(0, result.Count, string.Format("#!{0}->{1}@{2}", s, e, i));
		}
	}
	
	[Test()]
	public void RBDelete2Test()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int,string>();
		
		for (int i = 19; i >= 0; i--) {
			rb.Insert(i, i.ToString());
		}
				
		for (int i = 0; i < 10; i++) {
			rb.Delete(9 - i);
			
			RBAssertPresent(rb, 0, 9 - i - 1);
			RBAssertPresent(rb, 10 + i, 19);
			RBAssertNotPresent(rb, 9 - i, 10 + i - 1);
			
			rb.Delete(10 + i);
			
			RBAssertPresent(rb, 0, 9 - i - 1);
			RBAssertPresent(rb, 10 + i + 1, 19);
			RBAssertNotPresent(rb, 9 - i, 10 + i );
		}
	}
	
	[Test()]
	public void RBDelete3Test()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int,string>();
		
		for (int i = 99; i >= 0; i--) {
			rb.Insert(i, i.ToString());
		}
		
		for (int i = 0; i < 50; i++) {
			rb.Delete(49 - i);
			
			RBAssertPresent(rb, 0, 49 - i - 1);
			RBAssertPresent(rb, 50 + i, 99);
			RBAssertNotPresent(rb, 49 - i, 50 + i - 1);
			
			rb.Delete(50 + i);
			
			RBAssertPresent(rb, 0, 49 - i - 1);
			RBAssertPresent(rb, 50 + i + 1, 99);
			RBAssertNotPresent(rb, 49 - i, 50 + i );
		}
	}
	
	[Test()]
	public void RBDeleteNotPresentTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int,string>();
		
		for (int i = 0; i < 5; i++) {
			rb.Insert(i, i.ToString());
		}
		
		for (int i = 0; i < 5; i++) {
			rb.Delete(i);
			rb.Delete(i);
		}
		
	}
	
	[Test()]
	public void RBDeleteRandomTest()
	{
		RedBlackTree<int, string> rb = new RedBlackTree<int,string>();
		
		for (int i = 99; i >= 0; i--) {
			rb.Insert(i, i.ToString());
		}
		Random r = new Random();
		
		for (int i = 0; i < 1000; i++) {
			int j = r.Next(0, 99);
			rb.Delete(j);
		}
	}
}

} // end namespace

