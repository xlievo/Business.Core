
function GetSdkJavaScript(h, c, data) {
	var value = "\n\    { c: \"" + c + "\"";
	if (data.hasOwnProperty("t")) {
		value += ", t: \"" + data.t + "\"";
	}
	if (data.hasOwnProperty("d")) {
		value += ", d: " + JSON.stringify(data.d);
	}
	value += " }";

	return "var ajax = {};\n\
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
ajax.send = function (url, callback, failed, method, data, async, contentType = 'application/x-www-form-urlencoded') {\n\
	if (async === undefined) {\n\
		async = true;\n\
	}\n\
	var x = ajax.x();\n\
	x.open(method, url, async);\n\
	x.onreadystatechange = function () {\n\
		if (x.readyState !== 4) return;\n\
\n\
		if (x.status >= 200 && x.status < 300) {\n\
			callback(x.responseText)\n\
		}\n\
		else {\n\
			failed({\n\
				status: x.status,\n\
				statusText: x.statusText,\n\
				responseText: x.responseText\n\
			})\n\
		}\n\
	};\n\
	if (method == 'POST') {\n\
		if (null !== contentType && '' !== contentType) {\n\
			x.setRequestHeader('Content-type', contentType);\n\
		}\n\
	}\n\
	x.send(data);\n\
};\n\
\n\
ajax.get = function (url, data, callback, failed, async) {\n\
	var query = [];\n\
	for (var key in data) {\n\
		query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));\n\
	}\n\
	ajax.send(url + (query.length ? '?' + query.join('&') : ''), callback, failed, 'GET', null, async)\n\
};\n\
\n\
ajax.post = function (url, data, callback, failed, async) {\n\
	var query = [];\n\
	for (var key in data) {\n\
		query.push(encodeURIComponent(key) + '=' + encodeURIComponent(data[key]));\n\
	}\n\
	ajax.send(url, callback, failed, 'POST', query.join('&'), async)\n\
};\n\
\n\
ajax.postForm = function (url, data, callback, failed, async) {\n\
	ajax.send(url, callback, failed, 'POST', data, async, null)\n\
};\n\
\n\
ajax.post(\"" + h + "\","
		+
		value
		+
		"\n\    , function (response) {\n\
        //succcess\n\
        console.log(response);\n\
    }, function (response) {\n\
        //fail\n\
        console.log(response);\n\
    });";
}

function GetSdkNet(h, c, data) {
	var value = "\n\            KeyValuePair.Create(\"c\", \"" + c + "\")";
	if (data.hasOwnProperty("t")) {
		value += ",\n\            KeyValuePair.Create(\"t\", \"" + data.t + "\")";
	}
	if (data.hasOwnProperty("d")) {
		value += ",\n\            KeyValuePair.Create(\"d\", " + JSON.stringify(data.d) + ")";
	}

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
        var result = await Call("
		+
		value
		+ 
		");\n\
\n\
        Console.WriteLine(result);\n\
\n\
        Console.Read();\n\
    }\n\
\n\
    public async static Task<string> Call(params KeyValuePair<string, string>[] data)\n\
    {\n\
		using var content = new FormUrlEncodedContent(data);\n\
        using var request = new HttpRequestMessage(HttpMethod.Post, \"" + h + "\") { Content = content };\n\
        var client = httpClientFactory.CreateClient();\n\
        using var response = await client.SendAsync(request);\n\
        response.EnsureSuccessStatusCode();\n\
        return await response.Content.ReadAsStringAsync();\n\
    }\n\
}";
}