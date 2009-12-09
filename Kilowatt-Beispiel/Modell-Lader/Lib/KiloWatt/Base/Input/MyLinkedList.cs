using System;
using System.Collections.Generic;
using System.Text;

namespace KiloWatt.Base.Input
{
  /// <summary>
  /// Store a list of some kind of type. Additions and removals at front and back 
  /// are efficient. This class has a small cache of the linked list nodes that are 
  /// kept live even after elements have been removed, to avoid unnecessary garbage 
  /// generation. The LinkedList&lt;T&gt; class in System.Collections.Generic has 
  /// problems with extreme garbage generation. Note that the cache of nodes is 
  /// kept on a per-list-instance basis.
  /// </summary>
  /// <typeparam name="T">Some nullable (class) type.</typeparam>
  public class MyLinkedList<T> where T : class
  {
    public MyLinkedList()
    {
    }

    public void AddFirst(T t)
    {
      Node n = NewNode();
      n.Value = t;
      n.Next = first_;
      if (first_ != null)
        first_.Prev = n;
      first_ = n;
      if (last_ == null)
        last_ = n;
    }

    public void AddLast(T t)
    {
      Node n = NewNode();
      n.Value = t;
      n.Prev = last_;
      if (last_ != null)
        last_.Next = n;
      last_ = n;
      if (first_ == null)
        first_ = n;
    }

    public int Count { get { return count_; } }
    public Node First { get { return first_; } }
    public Node Last { get { return last_; } }

    public void RemoveFirst()
    {
      if (first_ == null) return;
      Node n = first_;
      if (n.Next != null) n.Next.Prev = null;
      first_ = n.Next;
      OldNode(n);
      if (first_ == null)
        last_ = null;
    }

    internal Node NewNode()
    {
      Node n;
      if (freeList_ != null)
      {
        n = freeList_;
        freeList_ = freeList_.Next;
        --freeListSize_;
      }
      else
      {
        n = new Node();
      }
      n.Next = null;
      n.Prev = null;
      ++count_;
      return n;
    }

    internal void OldNode(Node n)
    {
      n.Value = null;
      if (freeListSize_ < Limit)
      {
        n.Next = freeList_;
        freeList_ = n;
        ++freeListSize_;
      }
      --count_;
    }

    public int Limit { get { return limit_; } set { limit_ = value; freeListSize_ = 0; freeList_ = null; } }

    public class Node
    {
      public T Value;
      internal Node Prev;
      internal Node Next;
    }

    int count_;
    int limit_ = 256;
    Node first_;
    Node last_;
    int freeListSize_;
    Node freeList_;
  }
}
