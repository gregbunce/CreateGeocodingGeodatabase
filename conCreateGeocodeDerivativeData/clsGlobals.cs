using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conCreateGeocodeDerivativeData
{
    class clsGlobals
    {
        public static IFeatureClass arcFeatClass_Roads
        {
            get;
            set;
        }

        public static IFeature arcFeat_Roads
        {
            get;
            set;
        }

        public static IWorkspace arcWorkspaceSGID
        {
            get;
            set;
        }

        public static IFeatureWorkspace arcFeatureWorkspaceSGID
        {
            get;
            set;
        }

        public static IFeatureClass arcFeatClass_GeocodeRoads
        {
            get;
            set;
        }

        public static IWorkspace arcWorkspaceGeocodeFGD
        {
            get;
            set;
        }

        public static IFeatureWorkspace arcFeatureWorkspaceGeocodeFGD
        {
            get;
            set;
        }

        public static ITable arcTable_AltNames
        {
            get;
            set;
        }

        public static IWorkspace2 arcWorkspace2GeocodeFGD
        {
            get;
            set;
        }

        public static string strNewGeocodeFeatClassName
        {
            get;
            set;
        }

        public static string strNewGeocodeTableName
        {
            get;
            set;
        }
    }
}
