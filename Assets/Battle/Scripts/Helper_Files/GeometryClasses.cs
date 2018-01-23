using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

// Defines a point in 2-dimensional space
// Essentially just a wrapper for tuples with some additional helpful geometric operations
// Written by Checkmate
public class Point
{
  private Tuple<double, double> pos;

  public double X { get { return pos.First; } }
  public double Y { get { return pos.Second; } }

  public Point(double x, double y) {
    pos = new Tuple<double, double>(x, y);
  }

  // Operations are defined using standard vector space maths
  public static Point operator +(Point a, Point b) {
    return new Point(a.X + b.X, a.Y + b.Y);
  }
  public static Point operator -(Point a, Point b) {
    return new Point(a.X - b.X, a.Y - b.Y);
  }
  public static Point operator *(Point a, double c) {
    return new Point(a.X * c, a.Y * c);
  }
  public static Point operator *(double c, Point a) {
    return new Point(a.X * c, a.Y * c);
  }
  public static Point operator /(Point a, double c) {
    return new Point(a.X / c, a.Y / c);
  }

  // Returns the 2-dimensional dot product between two points 'a' and 'b'
  public static double Dot(Point a, Point b) {
    return a.X * b.X + a.Y * b.Y;
  }
  // Returns the 2-dimensional cross product between two points 'a' and 'b'
  public static double Cross(Point a, Point b) {
    return a.X * b.Y - a.Y * b.X;
  }
  // Returns the cartesian distance from point 'a' to point 'b'
  public static double Distance(Point a, Point b) {
    double xDist = a.X - b.X;
    double yDist = a.Y - b.Y;
    return Math.Sqrt((xDist * xDist) + (yDist * yDist));
  }
  // Returns the angle of the line segment between points 'a' and 'b' relative to the positive x-axis
  public static double Angle(Point a, Point b) {
    return Math.Atan2(b.Y - a.Y, a.X - b.X);
  }

  // Object-specific definitions of some static functions for convenience;
  public double Dot(Point other) {
    return Dot(this, other);
  }
  public double Cross(Point other) {
    return Cross(this, other);
  }
  public double Distance(Point other) {
    return Distance(this, other);
  }
  public double Angle(Point other) {
    return Angle(this, other);
  }

  // Transposes this point in the given angle direction the given distance
  public Point Transpose(double angle, double distance) {
    return new Point(X + distance * Math.Cos(angle), Y + distance * Math.Sin(angle));
  }

  // Rotates this point a given amount around a given center point
  // Code from: https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
  public Point Rotate(Point center, double angle) {
    double s = Math.Sin(angle);
    double c = Math.Cos(angle);

    double x = c * (X - center.X) - s * (Y - center.Y) + center.X;
    double y = c * (Y - center.Y) + s * (X - center.X) + center.Y;
    return new Point(x, y);
  }

  // Given a point and a list of points
  // Returns the point in the list closest to 'point'
  public static Point ClosestToPoint(Point point, ICollection<Point> points) {
    Point closest = null;
    foreach (Point p in points)
      if (closest == null || Distance(p, point) < Distance(closest, point))
        closest = p;
    return closest;
  }

  public override string ToString() {
    return "P" + pos.ToString();
  }

  public override int GetHashCode() {
    return (int)(X * Y * 100469);
  }

  public override bool Equals(object obj) {
    if (!(obj is Point))
      return false;
    return pos.Equals(((Point)obj).pos);
  }

  // Returns true if the given point is within floating-point error approximation closeness to this point
  public bool SafeEquals(Point other) {
    return FPErrorManager.AreEqual(X, other.X) && FPErrorManager.AreEqual(Y, other.Y);
  }
}

// An interface to indicate a two-dimensional object
// Has a collection of lines, vertices, and a center (average point of the shape)
public interface IShape
{
  Point Center { get; }
  ICollection<Point> Vertices { get; }
  LineSegment[] LineSegments { get; }
}

//TODO: Geometry Helper Classes File
// Defines a line segment in 2-dimensional space, which consists of two points
// Includes a variety of applicable geometric functions
// Note that the start and end points are not interchangeable, so use caution when accessing one or the other
public class LineSegment : IShape
{
  public Point Start { get; private set; }
  public Point End { get; private set; }
  public double Slope {
    get {
      return (End.Y - Start.Y) / (End.X - Start.X);
    }
  }
  // y-intercept
  public double Intercept {
    get {
      return Start.Y - Slope * (Start.X);
    }
  }
  public double Length {
    get {
      return Start.Distance(End);
    }
  }

  // Returns the point in the center of this line segment
  public Point Center {
    get { return (Start + End) / 2; }
  }

  public ICollection<Point> Vertices {
    get { return new List<Point> { Start, End }; }
  }

  public LineSegment[] LineSegments {
    get { return new LineSegment[] { this }; }
  }

  // Returns the angle of this point relative to the positive x-axis
  public double Angle {
    get { return Point.Angle(Start, End); }
  }

  public LineSegment() {
    Start = new Point(0, 0);
    End = new Point(0, 0);
  }
  public LineSegment(double startX, double startY, double endX, double endY) {
    Start = new Point(startX, startY);
    End = new Point(startX, startY);
  }
  public LineSegment(Point start, Point end) {
    Start = start;
    End = end;
  }

  // Transposes this line a given distance in a given direction in radians
  public LineSegment Transpose(double angle, double distance) {
    return new LineSegment(Start.Transpose(angle, distance), End.Transpose(angle, distance));
  }

  // Extends the line segment the given amount
  // An optional center argument indicates the point to extend around (the point that stays constant)
  public LineSegment Extend(double distance) {
    return Extend(distance, Center);
  }
  public LineSegment Extend(double distance, Point center) {
    double startDistance = distance * Start.Distance(center) / Length;
    double endDistance = distance * End.Distance(center) / Length;
    return new LineSegment(Start.Transpose(Angle, startDistance), End.Transpose(Angle, endDistance));
  }

  // Given an angle in radians and optional center of rotation (default is the center of this line segment)
  // Returns the result of rotating this line segment around the given angle
  public LineSegment Rotate(double angle) {
    return Rotate(angle, Center);
  }
  public LineSegment Rotate(double angle, Point center) {
    return new LineSegment(Start.Rotate(center, angle), End.Rotate(center, angle));
  }

  // Given an optional start point and optional length
  //   which default to the center and length of this line, respectively
  // Returns the line perpendicular to this line
  public LineSegment Perpendicular() {
    return Perpendicular(Center);
  }
  public LineSegment Perpendicular(Point start) {
    return Perpendicular(start, Length);
  }
  public LineSegment Perpendicular(double length) {
    return Perpendicular(Center, length);
  }
  public LineSegment Perpendicular(Point start, double length) {
    return new LineSegment(start, start.Transpose(Angle + Math.PI / 2, length));
  }

  // Returns the point that is greater according to a given comparison metric 'f'
  public Point ComparePoints(Func<Point, double> f) {
    if (f(Start) >= f(End))
      return Start;
    return End;
  }
  // Returns whether or not the given point is an endpoint of this line segment
  public bool HasEndPoint(Point point) {
    return Start.Equals(point) || End.Equals(point);
  }
  // Returns a line segment with start and end points in the given order
  // Where the new start point is that given by order(Start, End)
  public LineSegment GetSortedPoints(Func<Point, Point, Point> order) {
    if (order(Start, End).Equals(Start))
      return new LineSegment(Start, End);
    return new LineSegment(End, Start);
  }

  // Returns whether or not this line contains the given point
  public bool Contains(Point point) {
    return FPErrorManager.AreEqual(point.Distance(Start) + point.Distance(End), Length);
  }

  // Given another line segment 'other'
  // Returns the point at which these line segments intersect
  // OR returns null if no such intersection exists
  // NOTE: if the lines are colinear, this method returns the 'smallest' point at which they intersect
  // Code derived from https://stackoverflow.com/questions/563198/whats-the-most-efficent-way-to-calculate-where-two-line-segments-intersect
  public Point Intersection(LineSegment other) {
    Point p = Start;
    Point r = End - p;
    Point q = other.Start;
    Point s = other.End - q;
    double rs = Point.Cross(r, s);
    Point qpdiff = q - p;
    // parallel
    if (FPErrorManager.AreEqual(rs, 0)) {
      // colinear
      if (FPErrorManager.AreEqual(Point.Cross(qpdiff, r), 0)) {
        Point rnorm = r / Point.Dot(r, r);
        double t0 = Point.Dot(qpdiff, rnorm);
        double t1 = t0 + Point.Dot(s, rnorm);
        double minT = Math.Min(t0, t1);
        double maxT = Math.Max(t0, t1);
        // nonintersecting
        if (maxT < 0 || minT > 1)
          return null;
        // intersect at start
        if (minT < 0)
          return Start;
        // intersect elsewhere on the line
        return Start + minT * End;
      }
      // nonintersecting
      return null;
    }
    // intersecting
    double t = Point.Cross(qpdiff, s / rs);
    double u = Point.Cross(qpdiff, r / rs);
    if (0 <= t && t <= 1 && 0 <= u && u <= 1)
      return p + t * r;
    // not long enough
    return null;
  }

  public override string ToString() {
    return "Line From " + Start + " to " + End;
  }

  public override int GetHashCode() {
    return Start.GetHashCode() ^ End.GetHashCode();
  }

  // Note that two line segments are considered equal even if the start and end points are flipped
  public override bool Equals(object obj) {
    if (!(obj is LineSegment))
      return false;
    LineSegment ls = (LineSegment)obj;
    return (Start.Equals(ls.Start) && End.Equals(ls.End))
      || (Start.Equals(ls.End) && End.Equals(ls.Start));
  }

  // Returns whether or not the given line segment is within floating-point error approximation of being this line segment
  public bool SafeEquals(LineSegment other) {
    return (Start.SafeEquals(other.Start) && End.SafeEquals(other.End))
      || (Start.SafeEquals(other.End) && End.SafeEquals(other.Start));
  }
}

// Defines the structure and necessary functions for an arbitrary regular polygon
// Here, a regular polygon is defined by a collection of line segments which intersect to form vertices
public abstract class RegularPolygon : IShape
{
  // The center of this polygon
  public Point Center { get; private set; }
  // A list of the lines that make up this polygon
  public LineSegment[] LineSegments { get; private set; }
  // The vertex points of this polygon in no particular order
  public ICollection<Point> Vertices {
    get {
      List<Point> toReturn = new List<Point>();
      foreach (LineSegment p in LineSegments) {
        if (!toReturn.Contains(p.Start))
          toReturn.Add(p.Start);
        if (!toReturn.Contains(p.End))
          toReturn.Add(p.End);
      }
      return toReturn;
    }
  }
  // The distance from the center of the regular polygon to the most distant point in that polygon
  // https://en.wikipedia.org/wiki/Regular_polygon#Circumradius
  public double Circumradius {
    get {
      double s = LineSegments[0].Length; // Arbitrary cause regular polygon
      double n = LineSegments.Length;
      return s / (2 * Math.Sin(Math.PI / n));
    }
  }
  // The distance from the center of the polygon to the midpoint of a side
  // https://en.wikipedia.org/wiki/Apothem
  public double Apothem {
    get {
      double s = LineSegments[0].Length; // Arbitrary cause regular polygon
      double n = LineSegments.Length;
      return s / (2 * Math.Tan(Math.PI / n));
    }
  }
  public double Area {
    get {
      double s = LineSegments[0].Length;
      double n = LineSegments.Length;
      return 0.5 * n * s * Apothem;
    }
  }

  // Initialize the structure of a regular polygon at 'distance' from the 'center' point
  protected void _Initialize(int edgeCount, Point center, double distance, double angle) {
    Center = center;
    LineSegments = new LineSegment[edgeCount];
    double theta = 2 * Math.PI / edgeCount;
    angle += theta / 2; // orient the polygon to face 'up'
    Point prev = new Point(distance * Math.Cos(angle) + center.X, distance * Math.Sin(angle) + center.Y);
    for (int i = 0; i < edgeCount; i++) {
      angle += theta;
      Point next = new Point(distance * Math.Cos(angle) + center.X, distance * Math.Sin(angle) + center.Y);
      LineSegments[i] = new LineSegment(prev, next);
      prev = next;
    }
  }

  // Returns whether or not the given point is contained in this polygon
  // The optional threshold variable indicates how close to 2*pi the result must be (default 0.01)
  public bool Contains(Point point) {
    return Contains(point, 0.01);
  }
  public bool Contains(Point point, double threshold) {
    double val = 0;
    foreach (Point vertex in Vertices)
      val += Point.Distance(point, vertex);
    return val - (2 * Math.PI) < threshold;
  }

  // Given an angle and optional center of rotation (default center of this polygon)
  // Returns the result of rotating this polygon about the point 'angle' radians 
  public RegularPolygon Rotate(double angle) {
    return Rotate(angle, Center);
  }
  public abstract RegularPolygon Rotate(double angle, Point center);

  // Given a line segment 'line', indicates the first point at which this line intersects each segment of the polygon
  // If no intersections occur, this list is empty
  public List<Point> Intersection(LineSegment line) {
    List<Point> toReturn = new List<Point>();
    foreach (LineSegment l in LineSegments) {
      Point i = l.Intersection(line);
      if (i != null)
        toReturn.Add(i);
    }
    return toReturn;
  }

  public override string ToString() {
    return LineSegments.ToString();
  }

  public override int GetHashCode() {
    return Center.GetHashCode() ^ LineSegments.Count();
  }

  public override bool Equals(object obj) {
    if (!(obj is RegularPolygon))
      return false;
    return LineSegments.SequenceEqual(((RegularPolygon)obj).LineSegments);
  }

  // Returns whether or not this polygon and the 'other' polygon are equal up to floating-point error
  public bool SafeEquals(RegularPolygon other) {
    foreach (LineSegment ls1 in LineSegments) {
      bool contains = false;
      foreach (LineSegment ls2 in other.LineSegments)
        if (ls1.SafeEquals(ls2)) {
          contains = true;
          break;
        }
      if (!contains)
        return false;
    }
    return true;
  }
}

// Defines a regular hexagon based on the regular polygon class
public class RegularHexagon : RegularPolygon
{
  public RegularHexagon() {
    _Initialize(new Point(0, 0), 1, 0);
  }

  public RegularHexagon(Point center) {
    _Initialize(center, 1, 0);
  }

  public RegularHexagon(Point center, double edgeLength) {
    _Initialize(center, edgeLength, 0);
  }

  public RegularHexagon(Point center, double edgeLength, double angle) {
    _Initialize(center, edgeLength, angle);
  }

  private void _Initialize(Point center, double edgeLength, double angle) {
    _Initialize(6, center, edgeLength, angle);
  }

  public override RegularPolygon Rotate(double angle, Point center) {
    return new RegularHexagon(center, LineSegments[0].Length, angle);
  }

  public override bool Equals(object obj) {
    return (obj is RegularHexagon) && base.Equals(obj);
  }

  public override int GetHashCode() {
    return base.GetHashCode();
  }
}

// Defines a square based on the regular polygon class
public class Square : RegularPolygon
{
  public Square() {
    _Initialize(new Point(0, 0), 1, 0);
  }

  public Square(Point center) {
    _Initialize(center, 1, 0);
  }

  public Square(Point center, double edgeLength) {
    _Initialize(center, edgeLength, 0);
  }

  public Square(Point center, double edgeLength, double angle) {
    _Initialize(center, edgeLength, angle);
  }

  private void _Initialize(Point center, double edgeLength, double angle) {
    _Initialize(4, center, edgeLength, angle);
  }

  public override RegularPolygon Rotate(double angle, Point center) {
    return new Square(center, LineSegments[0].Length, angle);
  }

  public override bool Equals(object obj) {
    return (obj is Square) && base.Equals(obj);
  }

  public override int GetHashCode() {
    return base.GetHashCode();
  }
}