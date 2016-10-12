using System;
using Microsoft.SPOT;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Xml;
using Ws.Services.Soap;
using Ws.Services.WsaAddressing;

namespace DeviceServer.Base
{
    class DeviceServerWcfService : DpwsHostedService
    {
        public DeviceServerWcfService(ProtocolVersion v)
            : base(v)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("h", "http://schemas.example.org/HelloWCF");
            ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b51";
            ServiceTypeName = "HelloWCFDeviceType";
        }

        public DeviceServerWcfService()
            : this(new ProtocolVersion10())
        {
        }
    }

    public class IDeviceServerWcfService : DpwsHostedService
    {

        private IDeviceServerWcfService m_service;

        public IDeviceServerWcfService(IDeviceServerWcfService service, ProtocolVersion version) :
            base(version)
        {
            // Set the service implementation properties
            m_service = service;

            // Set base service properties
            ServiceNamespace = new WsXmlNamespace("ise", "http://localhost/DeviceServerService");
            ServiceID = "urn:uuid:f4c30207-c2cb-493c-8a44-776c1e0ecc7e";
            ServiceTypeName = "IDeviceServerWcfService";

            // Add service types here
            ServiceOperations.Add(new WsServiceOperation("http://localhost/DeviceServerService/IDeviceServerWcfService", "HelloWCF"));

            // Add event sources here
        }

        public IDeviceServerWcfService(IDeviceServerWcfService service) :
            this(service, new ProtocolVersion10())
        {
        }

        public virtual WsMessage HelloWCF(WsMessage request)
        {
            // Build request object
            HelloWCFDataContractSerializer reqDcs;
            reqDcs = new HelloWCFDataContractSerializer("HelloWCF", "http://localhost/DeviceServerService");
            HelloWCF req;
            req = ((HelloWCF)(reqDcs.ReadObject(request.Reader)));

            // Create response object
            // Call service operation to process request and return response.
            HelloWCFResponse resp;
            resp = m_service.HelloWCF(req);

            // Create response header
            WsWsaHeader respHeader = new WsWsaHeader("http://localhost/DeviceServerService/IDeviceServerWcfService/HelloWCFResponse", request.Header.MessageID, m_version.AnonymousUri, null, null, null);
            WsMessage response = new WsMessage(respHeader, resp, WsPrefix.Wsdp);

            // Create response serializer
            HelloWCFResponseDataContractSerializer respDcs;
            respDcs = new HelloWCFResponseDataContractSerializer("HelloWCFResponse", "http://localhost/DeviceServerService");
            response.Serializer = respDcs;
            return response;
        }
    }
}
