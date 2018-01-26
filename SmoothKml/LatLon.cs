using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoothKml
{
    public class LatLon
    {
        public double Latitude;
        public double Longitude;

        public double LongitudeRad
        {
            get
            {
                return GeoUtils.ToRadians(Longitude);
            }
            set
            {
                Longitude = GeoUtils.ToDegrees(value);
            }
        }

        public double LatitudeRad
        {
            get
            {
                return GeoUtils.ToRadians(Latitude);
            }
            set
            {
                Latitude = GeoUtils.ToDegrees(value);
            }
        }

        public LatLon()
        {
        }

        public LatLon(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }





        public double DistanceTo(LatLon to)
        {
            return GeoUtils.Distance(this, to);
        }

        public double BearingTo(LatLon to)
        {
            return GeoUtils.Bearing(this, to);
        }

        public LatLon Move(double bearing, double distanceM)
        {
            return GeoUtils.Move(this, bearing, distanceM);
        }



    }
}
