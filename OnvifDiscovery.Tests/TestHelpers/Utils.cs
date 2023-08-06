using System.Text;

namespace OnvifDiscovery.Tests.TestHelpers;

public static class Utils
{
    public static byte[] CreateProbeResponse(TestCamera camera)
    {
        var templateResponse = File.ReadAllText("Resources/response.txt");
        var modifiedResponse = string.Format(templateResponse, camera.MessageId, camera.Address,
            camera.Model, camera.Manufacturer, camera.IP);

        return Encoding.ASCII.GetBytes(modifiedResponse);
    }

    public static byte[] CreateProbeResponseWithNullHeader(TestCamera camera)
    {
        var templateResponse = File.ReadAllText("Resources/response_no_header.txt");
        var modifiedResponse = string.Format(templateResponse, camera.Address,
            camera.Model, camera.Manufacturer, camera.IP);

        return Encoding.ASCII.GetBytes(modifiedResponse);
    }
}