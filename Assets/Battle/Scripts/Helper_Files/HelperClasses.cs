using System.Collections;
using System;

// A class to help manage floating point approximations with greater accuracy
public sealed class FPErrorManager
{
  // Returns if two doubles are equal within some threshold
  // The threshold 'eps' can be defined, but is by default 1e8th the value of a and b
  public static bool AreEqual(double a, double b) {
    return AreEqual(a, b, a / 1e8 + b / 1e8);
  }
  public static bool AreEqual(double a, double b, double eps) {
    return Math.Abs(b - a) < eps;
  }
}

// Two-object tuple
// Tuple is unmodifiable once created
// No operations are defined to avoid confusion
public sealed class Tuple<T1, T2>
{
  public T1 First { get; private set; }
  public T2 Second { get; private set; }

  public Tuple(T1 first, T2 second) {
    First = first;
    Second = second;
  }

  public override string ToString() {
    return "(" + First + ", " + Second + ")";
  }

  public override int GetHashCode() {
    return base.GetHashCode();
  }

  public override bool Equals(object obj) {
    if (!(obj is Tuple<T1, T2>))
      return false;
    Tuple<T1, T2> tup = (Tuple<T1, T2>)obj;
    return First.Equals(tup.First) && Second.Equals(tup.Second);
  }
}

// Three-object tuple
// Tuple is unmodifiable once created
// No operations are defined to avoid confusion
public sealed class Tuple<T1, T2, T3>
{
  public T1 First { get; private set; }
  public T2 Second { get; private set; }
  public T3 Third { get; private set; }

  public Tuple(T1 first, T2 second, T3 third) {
    First = first;
    Second = second;
    Third = third;
  }

  public override string ToString() {
    return "(" + First + ", " + Second + ", " + Third + ")";
  }

  public override int GetHashCode() {
    return base.GetHashCode();
  }

  public override bool Equals(object obj) {
    if (!(obj is Tuple<T1, T2, T3>))
      return false;
    Tuple<T1, T2, T3> tup = (Tuple<T1, T2, T3>)obj;
    return First.Equals(tup.First) && Second.Equals(tup.Second) && Third.Equals(tup.Third);
  }
}


// Gives a max priority queue
// That is, the item with the highest priority will be dequeued first
public class PriorityQueue<T> : ICollection, IEnumerable where T : class
{

  // A linked list of the elements of this queue
  private Node head;
  private int count;

  public int Count {
    get {
      return count;
    }
    private set { }
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

  // Removes and returns the element with the highest priority in this queue
  public T Dequeue() {
    T toReturn = head.item;
    head = head.next;
    count--;
    return toReturn;
  }

  // Inserts the given 'item' with the given priority into the queue
  // The boolean update indicates whether this should modify an existing element that equals the given element
  // Returns whether the queue was modified by this operation
  public bool Insert(T item, int priority, bool update) {
    if (head == null) {
      head = new Node(item, priority);
      count++;
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
        updateCheck = updateCheck.next;
      }
    }

    if (prev == null)
      head = new Node(item, priority, current);
    else
      prev.next = new Node(item, priority, current);
    count++;
    return true;
  }

  // Returns whether the given item is contained in this queue
  public bool Contains(T item) {
    Node current = head;
    while (current != null) {
      if (current.item == item)
        return true;
      current = current.next;
    }
    return false;
  }

  // Removes the given item from the queue
  // Returns if the queue was modified by this operation
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
    count--;
    return true;
  }

  public void Clean() {
    head = null;
    count = 0;
  }

  public void CopyTo(Array array, int index) {
    if (array.Length - index < count)
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

  private class Enumerator : IEnumerator
  {
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