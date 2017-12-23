using System.Collections;
using System;

public sealed class Tuple<T1, T2>
{
  public T1 First { get; private set; }
  public T2 Second { get; private set; }

  public Tuple(T1 first, T2 second) {
    First = first;
    Second = second;
  }
}

public class PriorityQueue<T> : ICollection, IEnumerable where T : class {

  // A linked list of the elements of this queue
  private Node head;

  public int Count {
    get {
      return Count;
    }
    private set {}
  }

  public object SyncRoot {
    get {
      throw new NotImplementedException();
    }
  }

  public bool IsSynchronized {
    get {
      return false;
    }
  }

  public PriorityQueue() {
    head = null;
  }

  public T Pop() {
    T toReturn = head.item;
    head = head.next;
    return toReturn;
  }

  public bool Insert(T item, int priority, bool update) {
    if (head == null) {
      head = new Node(item, priority);
      Count++;
      return true;
    }
    Node prev = null;
    Node current = head;
    while (current != null && priority < current.priority) {
      if (update && current.item == item) 
        return false; //note that current.priority >= priority
      prev = current;
      current = current.next;

    }
    if (update) {
      Node updateCheck = current;
      while (updateCheck != null) {
        if (updateCheck.item == item) {
          if (updateCheck.priority < priority) {
            updateCheck.priority = priority;
            return true;
          }
          return false;
        }
      }
    }

    if (prev == null)
      head = new Node(item, priority, current);
    else
      prev.next = new Node(item, priority, current);
    Count++;
    return true;
  }

  public bool Contains(T item) {
    Node current = head;
    while (current != null) {
      if (current.item == item)
        return true;
      current = current.next;
    }
    return false;
  }

  public bool Remove(T item) {
    if (head == null)
      return false;
    Node prev = null;
    Node current = head;
    while (current.item != item) {
      prev = current;
      current = current.next;
      if (current == null)
        return false;
    }
    if (prev == null)
      head = current.next;
    else
      prev.next = current.next;
    Count--;
    return true;
  }

  public void Clean() {
    head = null;
    Count = 0;
  }

  public void CopyTo(Array array, int index) {
    if (array.Length - index < Count)
      throw new ArgumentException();
    Node current = head;
    while (current != null) {
      array.SetValue(current.item, index);
      index++;
    }
  }

  public IEnumerator GetEnumerator() {
    return new Enumerator(head);
  }

  private class Enumerator : IEnumerator {
    private Node start;
    private Node current;

    public Enumerator(Node start) {
      this.start = start;
      current = start;
    }

    public object Current {
      get {
        return current;
      }
    }

    public bool MoveNext() {
      if (current == null)
        return false;
      current = current.next;
      return current == null;
    }

    public void Reset() {
      current = start;
    }
  }

  private class Node
  {
    public T item;
    public int priority;

    public Node next;

    public Node(T item, int priority) {
      this.item = item;
      this.priority = priority;
      next = null;
    }

    public Node(T item, int priority, Node next) {
      this.item = item;
      this.next = next;
      this.priority = priority;
    }
  }
}
