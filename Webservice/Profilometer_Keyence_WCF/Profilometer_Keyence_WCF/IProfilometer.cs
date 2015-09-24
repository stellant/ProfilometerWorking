using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.Xml;

namespace Profilometer_Keyence_WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IProfilometer" in both code and config file together.
    [ServiceContract]
    public interface IProfilometer
    {
        [OperationContract]
        [WebGet(UriTemplate = "/input?ip={ipAddress}&port={port}&type={type}", ResponseFormat = WebMessageFormat.Xml)]
        XmlElement GetMeasurementValue(string ipAddress, string port, string type);
    }
}
