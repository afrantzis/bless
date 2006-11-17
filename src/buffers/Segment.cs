// created on 6/4/2004 at 3:37 PM
/*
 *   Copyright (c) 2004, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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

namespace Bless.Buffers {

///<summary>Represents a portion of a buffer</summary>
public class Segment {

	IBuffer m_buffer;

	long m_start;
	long m_end;

	/// <summary>Create a segment of a buffer</summary>
	public Segment(IBuffer buffer, long start, long end)
	{
		m_buffer=buffer;
		m_start=start;
		m_end=end;
	}

	///<summary>Does this segment contain an offset</summary>
	public bool Contains(long offset, long mapping) 
	{
		if (offset>=mapping && offset<= mapping+m_end-m_start)
			return true;
		else
			return false;
	}

	///<summary>Split a segment into two</summary>
	public Segment SplitAt(long pos) 
	{
		if (pos > m_end-m_start || pos==0)
			return null;
 
		Segment s=new Segment(m_buffer, m_start+pos, m_end);	
		this.m_end=m_start+pos-1;
		
		return s;
	}
	
	public override string ToString() 
	{
		return string.Format("({0}->{1})", m_start, m_end);
	}
	
	public long Size {
		get {return m_end-m_start+1;}	
	}

	public IBuffer Buffer {
		get { return m_buffer;}
	}
	
	
	public long Start {
		get { return m_start;}
		set { m_start=value; }
	}
	
	public long End {
		get { return m_end;}
		set { m_end=value; }
	}
	
}

} // end namespace
