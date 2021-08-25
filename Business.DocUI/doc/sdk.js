
function GetSdkJavaScript(route, h, c, data, hasParameters) {
    var value = "";
    if (!hasParameters) {
        value += "\n\    { " + route.c + ": \"" + c + "\"";
        if (data.hasOwnProperty(route.t)) {
            value += ", " + route.t + ": \"" + data[route.t] + "\"";
        }
        if (data.hasOwnProperty(route.d)) {
            value += ", " + route.d + ": " + JSON.stringify(data[route.d]);
        }
        value += " }";
    }
    else {
        h += "/" + c;
        var data2 = [];
        for (var i in data) {
            var kv = {};
            kv.k = i;
            kv.v = data[i];
            data2.push(kv);
        }

        value += "\n\    { "
        for (var i = 0; i < data2.length; i++) {
            if ("object" != data2[i].v.t && "array" != data2[i].v.t) {
                value += data2[i].k + ": \"" + data2[i].v.d  + "\"";
            }
            else {
                value += data2[i].k + ": " + JSON.stringify(data2[i].v.d);
            }
            if (i < data2.length - 1) {
                value += ", ";
            }
        }
        value += " }";
    }

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

function GetSdkNet(route, h, c, data, hasParameters) {
    var value = "";
    if (!hasParameters) {
        value = "\n\            KeyValuePair.Create(\"" + route.c + "\", \"" + c + "\")";
        if (data.hasOwnProperty(route.t)) {
            value += ",\n\            KeyValuePair.Create(\"" + route.t + "\", \"" + data[route.t] + "\")";
        }
        if (data.hasOwnProperty(route.d)) {
            value += ",\n\            KeyValuePair.Create(\"" + route.d + "\", " + JSON.stringify(data[route.d]) + ")";
        }
    }
    else {
        h += "/" + c;
        var data2 = [];
        for (var i in data) {
            var kv = {};
            kv.k = i;
            kv.v = data[i];
            data2.push(kv);
        }

        for (var i = 0; i < data2.length; i++) {
            if ("object" != data2[i].v.t && "array" != data2[i].v.t) {
                value += "\n\            KeyValuePair.Create(\"" + data2[i].k + "\", \"" + data2[i].v.d + "\")";
            }
            else {
                value += "\n\            KeyValuePair.Create(\"" + data2[i].k + "\", " + JSON.stringify(data2[i].v.d) + ")";
            }
            if (i < data2.length - 1) {
                value += ",";
            }
        }
    }

    return "\
using System;\n\
using System.Collections.Generic;\n\
using System.Net.Http;\n\
using System.Threading.Tasks;\n\
using Microsoft.Extensions.DependencyInjection;\n\
\n\
static class Program\n\
{\n\
    static readonly ServiceProvider serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();\n\
    static readonly IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();\n\
    static readonly HttpClient httpClient = httpClientFactory.CreateClient();\n\
\n\
    async static Task Main()\n\
    {\n\
        var result = await httpClient.Call("
        +
        "\"" + h + "\","
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
    public async static ValueTask<string> Call(this HttpClient client, string url, params KeyValuePair<string, string>[] data)\n\
    {\n\
		using var content = new FormUrlEncodedContent(data);\n\
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };\n\
        using var response = await client.SendAsync(request);\n\
        response.EnsureSuccessStatusCode();\n\
        return await response.Content.ReadAsStringAsync();\n\
    }\n\
}";
}

function GetCurl(route, h, c, data, data2, encode = false, hasParameters = false) {
    var value = "";

    if (!hasParameters) {
        value = "\n";
        value += "			#Built-in Route   " + h + "?" + route.c + "=&" + route.t + "=&" + route.d + "=";
        value += "\n\n\curl -X GET \"" + h + "?" + route.c + "=" + (encode ? encodeURIComponent(c) : c);

        // Built-in Route

        if (data.hasOwnProperty(route.t)) {
            value += "&" + route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]);
        }
        if (data.hasOwnProperty(route.d)) {
            value += "&" + route.d + "=" + (encode ? encodeURIComponent(data[route.d]) : data[route.d]);
        }

        value += "\"";
        value += "\n";
        value += "\n\curl -X POST -d \"" + route.c + "=" + (encode ? encodeURIComponent(c) : c);

        if (data.hasOwnProperty(route.t)) {
            value += "&" + route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]);
        }
        if (data.hasOwnProperty(route.d)) {
            value += "&" + route.d + "=" + (encode ? encodeURIComponent(data[route.d]) : data[route.d]);
        }
        value += "\" " + h;
        value += "\n";
    }

    value += "\n";
    value += "			#Classical Route   " + h + "/" + (encode ? encodeURIComponent(c) : c);
    value += "\n\n\curl -X GET \"" + h + "/" + (encode ? encodeURIComponent(c) : c);

    // Classical Route

    if (data.hasOwnProperty(route.t) && data2.hasOwnProperty(route.d)) {
        value += "?" + route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]) + "&" + data2.d;
    }
    else if (data.hasOwnProperty(route.t)) {
        value += "?" + route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]);
    } else if (data2.hasOwnProperty(route.d)) {
        value += "?" + data2.d;
    }

    value += "\"";
    value += "\n";
    value += "\n\curl -X POST -d \"";

    if (data.hasOwnProperty(route.t) && data2.hasOwnProperty(route.d)) {
        value += route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]) + "&" + data2.d;
    }
    else if (data.hasOwnProperty(route.t)) {
        value += route.t + "=" + (encode ? encodeURIComponent(data[route.t]) : data[route.t]);
    } else if (data2.hasOwnProperty(route.d)) {
        value += data2.d;
    }

    value += "\" " + h + "/" + (encode ? encodeURIComponent(c) : c);

    value += "\n";

    if (!hasParameters) {
        value += "\n\curl -H \"Content-Type:application/json\" -X POST -d \"";

        // application/json
        if (data.hasOwnProperty(route.d)) {
            value += (encode ? encodeURIComponent(data[route.d]) : data[route.d]);
        }

        value += "\" " + h + "/" + (encode ? encodeURIComponent(c) : c);

        if (data.hasOwnProperty(route.t)) {
            value += "?" + route.t + "=" + (encode ? encodeURIComponent(data2.t) : data2.t);
        }

        value += "\n";
    }

    return value;
}