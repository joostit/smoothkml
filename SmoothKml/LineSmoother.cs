using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoothKml
{
    /// <summary>
    /// Process the window and indicate of the center coordinate should be removed
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns>true if the center coordinate (p2) should be removed</returns>
    public delegate bool processWindowDelegate(LatLon p1, LatLon p2, LatLon p3);

    public class LineSmoother
    {

        private List<LatLon> points = new List<LatLon>();
        private const int smoothWindowSize = 3;
        private const int minimumPolyPointCount = 10;

        public double SmoothingDistance { get; set; } = 100;
        public double BearingSmoothingDegrees { get; set; } = 5;

        public LineSmoother(String inputString)
        {
            points = getCoordinates(inputString);
        }

        private List<LatLon> getCoordinates(String input)
        {
            List<LatLon> retVal = new List<LatLon>();
            string[] coordinates = input.Split(' ');
            foreach (string coord in coordinates)
            {
                string[] parts = coord.Split(',');
                LatLon newPoint = new LatLon();
                newPoint.Longitude = Convert.ToDouble(parts[0], CultureInfo.InvariantCulture);
                newPoint.Latitude = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                retVal.Add(newPoint);
            }

            return retVal;
        }

        internal String getKmlCoordinateString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (LatLon pt in points)
            {
                sb.Append(" ");
                sb.Append(Math.Round(pt.Longitude, 14).ToString(CultureInfo.InvariantCulture));
                sb.Append(",");
                sb.Append(Math.Round(pt.Latitude, 14).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Smooths the coordinate list
        /// </summary>
        /// <param name="passes"The number of smooting passes/>
        /// <returns>The size reduction in percent</returns>
        public double doDistanceSmoothing(int passes)
        {
            double sizeLeft;
            double smoothingStartSize = points.Count;

            for (int pass = 0; pass < passes; pass++)
            {
                double passStartSize = points.Count;
                doWindowedProcessing(ProcessDistanceSmoothingWindow);
                double passEndSize = points.Count;
                double passSizeLeft = (passEndSize / passStartSize) * 100.0;
                //Console.WriteLine("   DistanceSmoothing Pass#" + pass + " Left: " + Math.Round(passSizeLeft, 0) + "%");
            }

            double smoothingEndSize = points.Count;

            sizeLeft = (smoothingEndSize / smoothingStartSize) * 100.0;
            return sizeLeft;
        }


        /// <summary>
        /// Smooths the coordinate list
        /// </summary>
        /// <param name="passes"The number of smooting passes/>
        /// <returns>The size reduction in percent</returns>
        public double DoBearingBasedSmoothing(int passes)
        {
            double sizeLeft;
            double smoothingStartSize = points.Count;

            for (int pass = 0; pass < passes; pass++)
            {
                double passStartSize = points.Count;
                doWindowedProcessing(ProcessBearingSmoothingWindow);
                double passEndSize = points.Count;
                double passSizeLeft = (passEndSize / passStartSize) * 100.0;
                //Console.WriteLine("   BearingSmoothing Pass#" + pass + " Left: " + Math.Round(passSizeLeft, 0) + "%");
            }

            double smoothingEndSize = points.Count;

            sizeLeft = (smoothingEndSize / smoothingStartSize) * 100.0;
            if (Double.IsNaN(sizeLeft))
            {

            }
            return sizeLeft;
        }

        private bool ProcessBearingSmoothingWindow(LatLon p1, LatLon p2, LatLon p3)
        {
            bool doRemoveP2 = false;

            double p1ToP3 = p1.BearingTo(p3);
            double p1ToP2 = p1.BearingTo(p2);
            double difference = GetBearingDifference(p1ToP2, p1ToP3);

            doRemoveP2 = (difference < BearingSmoothingDegrees);

            return doRemoveP2;
        }

        private bool ProcessDistanceSmoothingWindow(LatLon p1, LatLon p2, LatLon p3)
        {
            bool doRemoveP2 = false;

            // Check if the outer points are close to each other
            double outerDistance = p1.DistanceTo(p3);
            if (outerDistance < SmoothingDistance)
            {
                // Then check if the center point is also near the other two
                double distanceP1 = p1.DistanceTo(p2);
                double distanceP3 = p3.DistanceTo(p2);

                if ((distanceP1 < SmoothingDistance) && (distanceP3 < SmoothingDistance))
                {
                    doRemoveP2 = true;
                }
            }

            return doRemoveP2;
        }


        private void doWindowedProcessing(processWindowDelegate processor)
        {

            int index = 0;
            while (index < points.Count)
            {
                if (points.Count < minimumPolyPointCount)
                {
                    break;
                }

                int p1index = index;
                int p2index = (index + 1) % points.Count;
                int p3index = (index + 2) % points.Count;

                LatLon p1 = points[p1index];
                LatLon p2 = points[p2index];
                LatLon p3 = points[p3index];

                Boolean doRemoveP2 = processor(p1, p2, p3);

                if (doRemoveP2)
                {
                    points.RemoveAt(p2index);
                }
                else
                {
                    // Only increase the index if no point was removed
                    index++;
                }

            }
        }

        private static double GetBearingDifference(double bearing1, double bearing2)
        {

            double difference = bearing1 - bearing2;
            while (difference < -180) difference += 360;
            while (difference > 180) difference -= 360;
            return Math.Abs(difference);
        }

    }
}
