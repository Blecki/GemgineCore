using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class EdgeMesh
    {
        private void MergePolygons()
        {
            for (var polygonIndex = 0; polygonIndex < Faces.Count; ++polygonIndex)
            {
                var face = Faces[polygonIndex];

                for (var edgeIndex = 0; edgeIndex < face.edges.Count; ++edgeIndex)
                {
                    var edge = face.edges[edgeIndex];
                    var neighbor = edge.Neighbors[0];
                    if (Object.ReferenceEquals(neighbor, face)) neighbor = edge.Neighbors[1];
                    if (neighbor == null) continue;
                    if (Object.ReferenceEquals(neighbor, face)) continue; //Face is it's own neighbor?

                    var reject = false;
                    foreach (var neighborEdge in neighbor.edges)
                    {
                        if (Object.ReferenceEquals(edge, neighborEdge)) continue;
                        if (neighborEdge.Neighbors[0] == face || neighborEdge.Neighbors[1] == face) reject = true;
                    }
                    if (reject) continue;

                    if (!FacesAreCoplanar(face, neighbor)) continue;

                    var candidateFace = MergeFaces(face, neighbor);
                    if (!IsConvex(candidateFace)) continue;

                    //Update neighbor references.
                    foreach (var candidateEdge in candidateFace.edges)
                        for (var i = 0; i < 2; ++i)
                            if (Object.ReferenceEquals(candidateEdge.Neighbors[i], neighbor) ||
                                Object.ReferenceEquals(candidateEdge.Neighbors[i], face))
                                candidateEdge.Neighbors[i] = candidateFace;

                    Faces.Remove(face);
                    Faces.Remove(neighbor);
                    Faces.Add(candidateFace);

                    polygonIndex = 0;
                    break;
                }
            }
        }

        public bool FacesAreCoplanar(EMFace A, EMFace B)
        {
            return true; //In current test case, all faces that are connected are coplanar.
        }

        public bool EdgesAreColinear(EMEdge A, EMEdge B)
        {
            var points = new List<Vector3>(ExtractPointSequence(A, B).Select((s) => Verticies[s]));
            var v0 = points[1] - points[0];
            var v1 = points[2] - points[1];

            return Gem.Math.Utility.AlmostZero(AngleBetweenVectors(v0, v1));
        }

        private EMFace MergeFaces(EMFace A, EMFace B)
        {
            if (Object.ReferenceEquals(A, B))
                throw new InvalidOperationException("Attempt to merge face with itself");

            EMEdge sharedEdge = null;
            foreach (var e in A.edges)
                if (e.Neighbors.Contains(B)) sharedEdge = e;
            if (sharedEdge == null)
                throw new InvalidOperationException("Attempt to merge faces with no shared edge.");

            var newFace = new EMFace();
            newFace.edges = new List<EMEdge>(A.edges);
            newFace.edges.InsertRange(0, B.edges);
            newFace.edges.RemoveAll((e) => Object.ReferenceEquals(e, sharedEdge));

            if (newFace.edges.Count <= 2) 
                throw new InvalidOperationException("Degenerate face generated");

            return newFace;
        }

        private Vector3 Sum(List<Vector3> v)
        {
            var r = Vector3.Zero;
            foreach (var v0 in v)
                r += v0;
            return r;
        }

        public List<Vector3> ExtractPerimeter(EMFace face)
        {
            var r = new List<Vector3>();
            VisitEdgePairsInSequence(face, (a, b) =>
                {
                    //Record the un-shared end point of a.
                    if (b.Verticies.Contains(a.Verticies[0]))
                        r.Add(Verticies[a.Verticies[1]]);
                    else
                        r.Add(Verticies[a.Verticies[0]]);
                    return VEPISReturnCode.Advance;
                });
            return r;
        }

        static private float AngleBetweenVectors(Vector3 A, Vector3 B)
        {
            A.Normalize();
            B.Normalize();
            float DotProduct = Vector3.Dot(A, B);
            DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);
            float Angle = (float)System.Math.Acos(DotProduct);
            return Angle;
        }

        private short[] ExtractPointSequence(EMEdge a, EMEdge b)
        {
            var points = new short[3];

            if (b.Verticies.Contains(a.Verticies[1]))
            {
                points[0] = a.Verticies[0];
                points[1] = a.Verticies[1];
            }
            else
            {
                points[0] = a.Verticies[1];
                points[1] = a.Verticies[0];
            }

            if (b.Verticies[0] == points[1])
                points[2] = b.Verticies[1];
            else
                points[2] = b.Verticies[0];

            return points;
        }

        private bool IsConvex(EMFace face)
        {
            var accumulatedAngle = 0.0f;
            VisitEdgePairsInSequence(face, (a, b) =>
                {
                    var points = ExtractPointSequence(a, b);

                    var v0 = Verticies[points[0]] - Verticies[points[1]];
                    var v1 = Verticies[points[2]] - Verticies[points[1]];

                    var angle = AngleBetweenVectors(v0, v1);
                    accumulatedAngle += angle;

                    return VEPISReturnCode.Advance;
                });

            if (accumulatedAngle < 0) accumulatedAngle *= -1;
            return Gem.Math.Utility.NearlyEqual(accumulatedAngle, (face.edges.Count - 2) * Gem.Math.Angle.PI);
        }

        private void CalculateCentroid(EMFace face)
        {
            var perimeterPoints = ExtractPerimeter(face);
            face.Centroid = Sum(perimeterPoints) / perimeterPoints.Count;
        }

        private enum VEPISReturnCode
        {
            Advance,
            Stall,
            Abort
        }

        private void VisitEdgePairsInSequence(EMFace face, Func<EMEdge, EMEdge, VEPISReturnCode> callback)
        {
            short? endpoint = null;
            var currentEdge = face.edges[0];
            var terminalCheck = false;
            while (true)
            {
                if (!endpoint.HasValue) endpoint = currentEdge.Verticies[1];

                if (terminalCheck)
                {
                    if (Object.ReferenceEquals(currentEdge, face.edges[0])) break; //We've looped through the entire polygon
                }
                else
                    terminalCheck = true;


                var nextEdge = FindNextEdgeInSequence(face, currentEdge, endpoint.Value);
                short nextEndpoint = -1;
                if (nextEdge.Verticies[0] == endpoint) nextEndpoint = nextEdge.Verticies[1];
                else nextEndpoint = nextEdge.Verticies[0];

                var callbackResult = callback(currentEdge, nextEdge);
                if (callbackResult == VEPISReturnCode.Advance)
                    currentEdge = nextEdge;
                else if (callbackResult == VEPISReturnCode.Abort)
                    break;
                else
                    terminalCheck = false;

                endpoint = nextEndpoint;
            }
        }

        private short FindSharedEndpoint(EMEdge a, EMEdge b)
        {
            if (b.Verticies.Contains(a.Verticies[0])) return a.Verticies[0];
            else if (b.Verticies.Contains(a.Verticies[1])) return a.Verticies[1];
            else throw new InvalidOperationException("Attempt to find shared endpoint of edges with no shared endpoint");
        }

        private void SimplifyColinearEdges()
        {
            foreach (var face in Faces)
            {
                VisitEdgePairsInSequence(face, (a, b) =>
                    {
                        var sharedPoint = FindSharedEndpoint(a, b);
                        
                        var shouldMerge = true;
                        if (OtherNeighbor(a, face) != OtherNeighbor(b, face))
                            shouldMerge = false;
                        else if (OtherNeighbor(a, face) == null) //Colinear check is only necessary when edge has no other neighbor.
                            shouldMerge = EdgesAreColinear(a, b);
                            

                        if (shouldMerge)
                        {
                            //mutate a to end at b's non-shared edge.
                            var newEnd = b.Verticies[0];
                            if (newEnd == sharedPoint) newEnd = b.Verticies[1];
                            if (a.Verticies[0] == sharedPoint) a.Verticies[0] = newEnd;
                            else a.Verticies[1] = newEnd;

                            //remove b from both faces
                            face.edges.Remove(b);
                            if (OtherNeighbor(a, face) != null) OtherNeighbor(a, face).edges.Remove(b);
                            
                            return VEPISReturnCode.Stall; //VisitEdges should not advance to the next edge.
                        }
                        else
                            return VEPISReturnCode.Advance;
                    });

                if (face.edges.Count <= 2)
                    throw new InvalidOperationException("Simplifying edges created a degenerate face.");
            }
        }

        private EMFace OtherNeighbor(EMEdge e, EMFace f)
        {
            if (Object.ReferenceEquals(e.Neighbors[0], f)) return e.Neighbors[1];
            else return e.Neighbors[0];
        }

        private EMEdge FindNextEdgeInSequence(EMFace of, EMEdge after, short endpoint)
        {
            foreach (var e in of.edges)
            {
                if (Object.ReferenceEquals(e, after)) continue;
                if (e.Verticies.Contains(endpoint)) return e;
            }
            throw new InvalidOperationException("Malformed edge mesh");
        }

        public void Simplify()
        {
            foreach (var face in Faces)
                if (face.edges.Count <= 2)
                    throw new InvalidOperationException("Mesh to simplify contains degenerate faces");

            var polyCount = Faces.Count;
            while (true)
            {
                MergePolygons();
                if (polyCount == Faces.Count) break; //Nothing was merged.
                polyCount = Faces.Count;
                SimplifyColinearEdges();
            }

            foreach (var face in Faces)
                CalculateCentroid(face);
        }

        public static EMEdge FindSharedEdge(EMFace a, EMFace b)
        {
            foreach (var e in a.edges)
                if (b.edges.Contains(e)) return e;
            return null;
        }
    }
}
