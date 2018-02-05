using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmoothKml
{
    class Program
    {
        private const String kmlFile = "UrbanAreasNL_2017_12.kml.xml";
        private const String outputFile = "UrbanAreasNL_2017_12.kml.optimized.xml";

        private const int SmoothingDistance = 300;
        private const int SmootingPasses = 5;
        private const int minimumPopulation = 800;
        private const double BearingSmoothingDegrees = 3;


        static void Main(string[] args)
        {

            FileInfo startFileInfo = new FileInfo(kmlFile);

            XmlDocument doc = new XmlDocument();
            doc.Load(kmlFile);

            removeSmallAreas(doc);
            smoothOutPolygons(doc);
            addNameElementAreas(doc);
            removeSchemaData(doc);
            removeExtendedData(doc);

            Console.WriteLine("Saving " + outputFile + "...");

            doc.Save(outputFile);



            FileInfo endFileInfo = new FileInfo(outputFile);

            double sizeLeft = ((double)endFileInfo.Length / (double)startFileInfo.Length) * 100.0;

            String origSizeKb = String.Format(CultureInfo.InvariantCulture, "{0:0,000.###} kb", (startFileInfo.Length / 1024.0));
            String endSizeKb = String.Format(CultureInfo.InvariantCulture, "{0:0,000.###} kb", (endFileInfo.Length / 1024.0));

            Console.WriteLine("");

            Console.WriteLine("Done. Output KLM file size is " + Math.Round(sizeLeft) + "% of the original.");
            Console.WriteLine(origSizeKb + "  -->>  " + endSizeKb);
            Console.WriteLine("Press any key...");

            Console.ReadKey();

        }


        private static void removeSmallAreas(XmlDocument doc)
        {
            List<XmlNode> toRemove = new List<XmlNode>();

            XmlNodeList placemarkNodes = doc.GetElementsByTagName("Placemark");

            Console.WriteLine("Iterating " + placemarkNodes.Count + " placemark nodes for a minimum population of " + minimumPopulation);

            foreach (XmlElement schemaDataNode in placemarkNodes)
            {

                XmlNodeList dataElements = schemaDataNode.GetElementsByTagName("SimpleData");
                foreach (XmlElement dataElement in dataElements)
                {
                    XmlAttribute att = dataElement.Attributes["name"];
                    if (att != null)
                    {
                        String attValue = att.Value;

                        if (attValue == "population")
                        {
                            int population = Convert.ToInt32(dataElement.InnerText);
                            if (population < minimumPopulation)
                            {
                                toRemove.Add(schemaDataNode);
                                break;
                            }
                        }
                    }
                }

            }

            foreach (var node in toRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

            Console.WriteLine("Removed " + toRemove.Count + " small placemarks.");

        }


        private static void addNameElementAreas(XmlDocument doc)
        {
            XmlNodeList placemarkNodes = doc.GetElementsByTagName("Placemark");

            Console.WriteLine("Iterating " + placemarkNodes.Count + " placemark nodes to add the name element " + minimumPopulation);

            foreach (XmlElement placemarkNode in placemarkNodes)
            {

                XmlNodeList dataElements = placemarkNode.GetElementsByTagName("SimpleData");
                foreach (XmlElement dataElement in dataElements)
                {
                    XmlAttribute att = dataElement.Attributes["name"];
                    if (att != null)
                    {

                        String attValue = att.Value;
                        if (attValue == "name")
                        {
                            String polygnName = dataElement.InnerText;
                            XmlElement nameElement = doc.CreateElement("name", "http://www.opengis.net/kml/2.2");
                            nameElement.InnerText = polygnName;
                            placemarkNode.InsertAfter(nameElement, null);
                            break;
                        }
                    }
                }

            }


            Console.WriteLine("Replaced all extendedData name attributes with name elements.");

        }


        private static void smoothOutPolygons(XmlDocument doc)
        {
            XmlNodeList coordinateNodes = doc.GetElementsByTagName("coordinates");

            Console.WriteLine("Found " + coordinateNodes.Count + " 'coordinates' elements. Smoothing out...");

            int total = coordinateNodes.Count;
            int index = 0;
            double distanceSizeLeftSum = 0;
            double bearingSizeLeftSum = 0;

            foreach (XmlNode coordinatesNode in coordinateNodes)
            {

                LineSmoother smoother = new LineSmoother(coordinatesNode.InnerText);
                smoother.SmoothingDistance = SmoothingDistance;
                smoother.BearingSmoothingDegrees = BearingSmoothingDegrees;
                distanceSizeLeftSum += smoother.doDistanceSmoothing(SmootingPasses);
                bearingSizeLeftSum += smoother.DoBearingBasedSmoothing(SmootingPasses);
                coordinatesNode.InnerText = smoother.getKmlCoordinateString();

                //Console.WriteLine(index + " / " + total + " Left: " + Math.Round(sizeLeft) + "%");

                index++;
            }

            double distanceCoordinatesLeft = distanceSizeLeftSum / (double)total;
            double bearingCoordinatesLeft = bearingSizeLeftSum / (double)total;


            Console.WriteLine("Coordinate smootinh done.");
            Console.WriteLine("Distance-based smoothing reduced to: " + Math.Round(distanceCoordinatesLeft) + "% coordinates left");
            Console.WriteLine("Bearing-based smoothing reduced to: " + Math.Round(bearingCoordinatesLeft) + "% coordinates left");
        }



        private static void removeExtendedData(XmlDocument doc)
        {

            Console.WriteLine("Removing all extendedData nodes");

            List<XmlNode> toRemove = new List<XmlNode>();

            XmlNodeList placemarks = doc.GetElementsByTagName("Placemark");

            foreach (XmlElement placemark in placemarks)
            {
                XmlNodeList extendedDataNodes = placemark.GetElementsByTagName("ExtendedData");
                foreach (XmlNode extendedDataNode in extendedDataNodes)
                {
                    toRemove.Add(extendedDataNode);
                }
            }


            foreach (var node in toRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

        }

        private static void removeSchemaData(XmlDocument doc)
        {

            Console.WriteLine("Removing schema data");

            List<XmlNode> toRemove = new List<XmlNode>();

            XmlNodeList schemaNodes = doc.GetElementsByTagName("Schema");

            foreach (XmlElement schemaNode in schemaNodes)
            {
                toRemove.Add(schemaNode);
            }


            foreach (var node in toRemove)
            {
                node.ParentNode.RemoveChild(node);
            }

        }
    }
}
