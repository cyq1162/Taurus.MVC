/// <reference path="jquery.min.js" />
function run() {
    $("#run").attr('disabled', true);
    $("#runResult").hide();
    $("#resultContent").html("");
    $("#resultHeader").html("");
    //获取头部信息
    var url = $("#url").val();
    if (!url.startsWith("http")) {
        url = location.origin + url;
    }
    var httpType = $("#httpType").val();
    var dataType = $("#dataType").val();
    record("httpType", httpType);
    record("dataType", dataType);
    var data = {};
    var formData;//文件上传
    //收集参数
    var header = {};
    $("form").find("[name]:input").each(function () {
        var name = $(this).attr('name');
        var type = $(this).attr('rtype');
        var value = $(this).val();
        var required = $(this).attr('required');
        if (value == "" && !required) {
            return;
        }
        //保存参数，以便下次还原参数。
        record(name, value);

        if (type == 'header') {
            header[name] = value;
        }
        else if (type == "file") {
            if (isRunAll()) {
                data[name] = "taurus.docdefaultimg";
            }
            else {
                formData = new FormData($("form")[0]);//文件上传
                return;
            }
        }
        else {
            data[name] = value;
        }

    });

    ajax(httpType,dataType, url, data, header, formData);
    //ajax("HEAD", url, data, header);
}
function ajax(type,dataType, url, data, header, formData) {

    var opt = {
        type: type,
        url: url,
        data: formData || data,//文件上传优先
        dataType: dataType,
        beforeSend: function (request) {
            if (header)
                for (var i in header) {
                    request.setRequestHeader(i, header[i]);
                }
        },
        success: function (result, status, xhr) {
            $("#run").attr('disabled', false);
            $("#runResult").show();
            $("#resultHeader").html(formatHeader(xhr.getAllResponseHeaders()));
            var msg = result.toString().replace(/</g, '&lt;').replace(/>/g, '&gt;');
            switch (dataType)
            {
                case "json":
                    msg = JSON.stringify(result);
                    break;
                case "xml":
                    msg = result.firstChild.outerHTML;
                    break;
                default:
                    msg = result.toString();
                    break;
            }
            if (dataType == "xml")
            {
               
            }
            else if (dataType == "text")
            {

            }
                msg=msg.replace(/</g, '&lt;').replace(/>/g, '&gt;');
            $("#resultContent").html(msg);
            if (isRunAll()) {
                parent.setValue(location.href, result.success == undefined ? true : result.success, msg);
            }
        },
        error: function (result) {
            $("#run").attr('disabled', false);
            if (result) {
                $("#runResult").show();
                $("#resultHeader").html(formatHeader(result.getAllResponseHeaders()));
                var msg = result.responseText.replace(/</g, '&lt;').replace(/>/g, '&gt;')
                $("#resultContent").html(msg);
                if (isRunAll()) {
                    parent.setValue(location.href, false, msg);
                }
            }
        }
    };
    if (formData) {
        opt.contentType = false;
        opt.processData = false;
        opt.type = "POST";
    }
    $.ajax(opt);

}
function formatHeader(header) {
    var array = new Array(header.length);
    for (var i = 0; i < header.length; i++) {
        if (header.charCodeAt(i) == 10) {
            array[i] = '<br />';
        }
        else {
            array[i] = header[i];

        }
    }
    return array.join('');
}

//-------------批量运行-------------------
function isRunAll() {
    return location.href.indexOf("&runall=1") > 0
}
function runAll() {
    $("iframe").remove();
    var els = $("#ActionView a");
    if (els && els.length > 0) {
        var i = 0;
        els.each(function () {
            i++;
            $(this).next().attr("id", "i" + i).html("...");
            $(this).parent().next().find("p").html("");
            var url = $(this).attr("href");
            var html = "<iframe src='" + url + "&runall=1&id=i" + i + "' style='display:none'></iframe>";
            $(html).prependTo('body');
        });
    }
}
if (location.href.indexOf("&runall=1") > 0) {
    setTimeout(function () {
        $("#run").click();
    }, 1000)
}
function setValue(url, success, fullMsg) {
    var paras = url.split('=');
    var id = paras[paras.length - 1];
    var html = "<span style='color: " + (success ? "green" : "red") + "'>" + success + "</span>"
    var sp = $("#" + id);
    sp.html(html);
    sp.parent().next().find("p").html(fullMsg);
}

//------------记录、还原 参数值---------
var isRecord = true;
function record(name, value) {
    if (isRecord) {
        var key = location.search;
        localStorage.setItem(key + name, value);
    }
}
function restore() {
    if (isRecord) {
        var key = location.search.split("&runall=1")[0];
        var httpType = localStorage.getItem(key + "httpType");
        var dataType = localStorage.getItem(key + "dataType");
        if (httpType && dataType) {
            $("#httpType").val(httpType);
            $("#dataType").val(dataType);
        }
        $("form").find("[name]:input").each(function () {
            var name = $(this).attr('name');
            var type = $(this).attr('rtype');
            var value = $(this).val();
            if (type != "file" && value == "") {
                $(this).val(localStorage.getItem(key + name));
            }
        }
        );
    }
}
setTimeout(function () { restore();}, 100);


