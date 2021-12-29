using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace TileMap
{
    public class Vertex
    {
        public float x;
        public float y;
        public float z;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string filename = @"C:\Users\nateh\OneDrive\Documents\kml\trim2_renumbered.kml";
            string outfilename = @"C:\Users\nateh\OneDrive\Documents\kml\trim2_merge1.kml";
            using StreamReader stream = File.OpenText(filename);
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            XmlNode root = doc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("kml", "http://www.opengis.net/kml/2.2");
            string xPathString = "//kml:Placemark[kml:Polygon/kml:outerBoundaryIs/kml:LinearRing]";

            //XmlNode? node;
            //node = root.SelectSingleNode(xPathString, nsmgr);
            //int count = 0;

            //while (node != null && node.NodeType == XmlNodeType.Element && node.Name == "Placemark")
            //{
            //    node.Attributes["id"].Value = $"poly{count}";
            //    XmlNode rect = node.SelectSingleNode("kml:Polygon/kml:outerBoundaryIs/kml:LinearRing", nsmgr);
            //    rect.Attributes["id"].Value = $"{count}";
            //    node = node.NextSibling;
            //    count++;
            //}

            int count = 0;
            XmlNode? rect1 = root.SelectSingleNode(xPathString, nsmgr);
            XmlNode? rect2 = rect1.NextSibling;

            while (rect1 != null && rect2 != null)
            {
                List<Vertex> rect1_points = new List<Vertex>(4);
                List<Vertex> rect2_points = new List<Vertex>(4);

                XmlNode rect1_coords = rect1.SelectSingleNode(".//kml:coordinates", nsmgr);
                XmlNode rect2_coords = rect2.SelectSingleNode(".//kml:coordinates", nsmgr);

                Regex regex = new Regex(@"(([-]?[0-9]*\.[0-9]*),([-]?[0-9]*\.[0-9]*),([-]?[0-9]*\.[0-9]*))", RegexOptions.Singleline);
                MatchCollection matches_rect1 = regex.Matches(rect1_coords.InnerText);
                MatchCollection matches_rect2 = regex.Matches(rect2_coords.InnerText);

                for (int i = 0; i < 4; i++)
                {
                    rect1_points.Add(new Vertex
                    {
                        x = float.Parse(matches_rect1[i].Groups[2].Value),
                        y = float.Parse(matches_rect1[i].Groups[3].Value),
                        z = float.Parse(matches_rect1[i].Groups[4].Value)
                    });

                    rect2_points.Add(new Vertex
                    {
                        x = float.Parse(matches_rect2[i].Groups[2].Value),
                        y = float.Parse(matches_rect2[i].Groups[3].Value),
                        z = float.Parse(matches_rect2[i].Groups[4].Value)
                    });
                }

                XmlNode? cachedNextNode = null;

                if (rect1_points[1].x == rect2_points[0].x && rect1_points[1].y == rect2_points[0].y
                    && rect1_points[2].x == rect2_points[3].x && rect1_points[2].y == rect2_points[3].y)
                {
                    cachedNextNode = rect2.NextSibling;
                    rect2.ParentNode.RemoveChild(rect2);
                    rect1.Attributes["id"].Value = $"poly{count}";
                    XmlNode rect = rect1.SelectSingleNode("kml:Polygon/kml:outerBoundaryIs/kml:LinearRing", nsmgr);
                    rect.Attributes["id"].Value = $"{count}";
                    count++;
                }

                rect1 = cachedNextNode;
                if (rect1 != null)
                    rect2 = rect1.NextSibling;
            }            

            XmlTextWriter writer = new XmlTextWriter(outfilename, System.Text.Encoding.ASCII)
            {
                Formatting = Formatting.Indented
            };
            doc.WriteTo(writer);
            writer.Flush();

            _ = Console.ReadKey();
        }
    }
}
