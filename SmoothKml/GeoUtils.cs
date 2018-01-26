using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoothKml
{
   

    /**
     * Created by Joost on 16-1-2018.
     */

    public static class GeoUtils
    {


        /**
         * Calculate distance between two shape in latitude and longitude.
         * Uses Haversine method as its base.
         * <p>
         * lat1, lon1 Start point lat2, lon2 End point
         *
         * @returns Distance in Meters
         */
        public static double Distance(LatLon pos1, LatLon pos2)
        {
            return Distance(pos1, 0, pos2, 0);
        }

        /**
         * Calculate distance between two shape in latitude and longitude taking
         * into account height difference. If you are not interested in height
         * difference pass 0.0. Uses Haversine method as its base.
         * <p>
         * lat1, lon1 Start point lat2, lon2 End point el1 Start altitude in meters
         * el2 End altitude in meters
         *
         * @returns Distance in Meters
         */
        public static double Distance(LatLon pos1, double el1, LatLon pos2, double el2)
        {
            const int R = 6371;

            double lat1 = pos1.Latitude;
            double lon1 = pos1.Longitude;
            double lat2 = pos2.Latitude;
            double lon2 = pos2.Longitude;

            double latDistance = ToRadians(lat2 - lat1);
            double lonDistance = ToRadians(lon2 - lon1);
            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = R * c * 1000; // convert to meters

            double height = el1 - el2;

            distance = Math.Pow(distance, 2) + Math.Pow(height, 2);

            return Math.Sqrt(distance);
        }

        /// <summary>
        /// Returns the bearing in degrees
        /// </summary>
        /// <param name="ptFrom"></param>
        /// <param name="ptTo"></param>
        /// <returns></returns>
        public static double Bearing(LatLon from, LatLon to, double? relativeTo = null)
        {
            double lat1 = from.LatitudeRad;
            double lon1 = from.LongitudeRad;
            double lat2 = to.LatitudeRad;
            double lon2 = to.LongitudeRad;

            double dLon = lon2 - lon1;
            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double bearing = ToDegrees(Math.Atan2(y, x));  // This is the heading relative to 0, from -180 to 180

            if (bearing < 0) bearing += 360;

            if (relativeTo != null)
            {
                double relVal = relativeTo.Value;

                if (relVal > 180) relVal -= 360;

                bearing -= relVal;
                if (bearing > 180)
                {
                    bearing -= 360;
                }
            }

            return bearing;
        }

        public static LatLon Move(LatLon start, double bearing, double distanceM)
        {
            const double R = 6371000;
            double lat1 = ToRadians(start.Latitude);
            double lng1 = ToRadians(start.Longitude);
            double brng = ToRadians(bearing);

            
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(distanceM / R) +
                    Math.Cos(lat1) * Math.Sin(distanceM / R) * Math.Cos(brng));
            double lng2 = lng1 + Math.Atan2(Math.Sin(brng) * Math.Sin(distanceM / R) * Math.Cos(lat1),
                    Math.Cos(distanceM / R) - Math.Sin(lat1) * Math.Sin(lat2));

            LatLon result = new LatLon(ToDegrees(lat2), ToDegrees(lng2));

            return result;
        }


        public static double ToRadians(double angleDeg)
        {
            return (Math.PI / 180) * angleDeg;
        }

        public static double ToDegrees(double angleRad)
        {
            return angleRad / (Math.PI / 180);
        }

    }

}
