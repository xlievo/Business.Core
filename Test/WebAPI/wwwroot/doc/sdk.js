
function GetSdkJavaScript(h, c, t, d) {
    return "ajax.post(\"" + h + "\",\n\
        { c: \"" + c + "\", t: \"" + t + "\", d: " + d + " },\n\
        function (response, xml) {\n\
            //succcess\n\
            console.log(response);\n\
        }, function (status) {\n\
            //fail\n\
            console.log(status);\n\
        });\n\
\n\
var ajax = {};\n\
ajax.x = function () {\n\
        if (typeof XMLHttpRequest !== 'undefined') {\n\
            return new XMLHttpRequest();\n\
        }\n\
        var versions = [\n\
            \"MSXML2.XmlHttp.6.0\",\n\
            \"MSXML2.XmlHttp.5.0\",\n\
            \"MSXML2.XmlHttp.4.0\",\n\
            \"MSXML2.XmlHttp.3.0\",\n\
            \"MSXML2.XmlHttp.2.0\",\n\
            \"Microsoft.XmlHttp\"\n\
            ];\n\
\n\
        var xhr;\n\
        for (var i = 0; i < versions.length; i++) {\n\
            try {\n\
                xhr = new ActiveXObject(versions[i]);\n\
                break;\n\
            } catch (e) {\n\
            }\n\
        }\n\
        return xhr;\n\
        };\n\
\n\
ajax.send = function (url, callback, method, data, async) {\n\
        if (async === undefined) {\n\
            async = true;\n\
        }\n\
        var x = ajax.x();\n\
        x.open(method, url, async);\n\
        x.onreadystatechange = function () {\n\
            if (x.readyState == 4) {\n\
                callback(x.responseText)\n\
            }\n\
        };\n\
        if (method == 'POST') {\n\
            x.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');\n\
        }\n\
        x.send(data)\n\
        };\n\
\n\
ajax.get = function (url, data, callback, async) {\n\
        var query = [];\n\
        for (var key in data) {\n\
            query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));\n\
        }\n\
        ajax.send(url + (query.length ? '?' + query.join('&') : ''), callback, 'GET', null, async)\n\
        };\n\
        \n\
ajax.post = function (url, data, callback, async) {\n\
        var query = [];\n\
        for (var key in data) {\n\
            query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));\n\
        }\n\
        ajax.send(url, callback, 'POST', query.join('&'), async)\n\
        };";
}

function GetSdkNet(h, c, t, d) {
    return "\
using System;\n\
using System.Collections.Generic;\n\
using System.Net.Http;\n\
using System.Threading.Tasks;\n\
using Microsoft.Extensions.DependencyInjection;\n\
\n\
class Program\n\
{\n\
    static IHttpClientFactory httpClientFactory;\n\
\n\
    async static Task Main(string[] args)\n\
    {\n\
        var serviceProvider = new ServiceCollection()\n\
            .AddHttpClient()\n\
            .BuildServiceProvider();\n\
\n\
        httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();\n\
\n\
        var result = await Call(\"" + c + "\", \"" + t + "\", " + d + ");\n\
\n\
        Console.WriteLine(result);\n\
\n\
        Console.Read();\n\
    }\n\
\n\
    public async static Task<string> Call(string c, string t, string d)\n\
    {\n\
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>()\n\
        {\n\
            {\"c\", c},\n\
            {\"t\", t},\n\
            {\"d\", d}\n\
        });\n\
\n\
        using var request = new HttpRequestMessage(HttpMethod.Post, \"" + h + "\") { Content = content };\n\
        var client = httpClientFactory.CreateClient();\n\
        using var response = await client.SendAsync(request);\n\
        response.EnsureSuccessStatusCode();\n\
        return await response.Content.ReadAsStringAsync();\n\
    }\n\
}";
}