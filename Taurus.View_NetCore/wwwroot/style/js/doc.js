function run() {
    //获取头部信息
    var url = $("#url").val();
    if (!url.startsWith("http")) {
        url = location.origin + url;
    }
    var httpType = $("#httpType").val();
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
        if (type == 'header') {
            header[name] = value;
        }
        else if (type == "file") {
            formData = new FormData($("form")[0]);//文件上传
            return;
        }
        else {
            data[name] = value;
        }

    });

    ajax(httpType, url, data, header, formData);
    //ajax("HEAD", url, data, header);
}
function ajax(type, url, data, header, formData) {

    var opt = {
        type: type,
        url: url,
        data: formData || data,//文件上传优先
        dataType: 'json',
        beforeSend: function (request) {
            if (header)
                for (var i in header) {
                    request.setRequestHeader(i, header[i]);
                }
        },
        success: function (result, status, xhr) {
            $("#runResult").show();
            $("#resultHeader").html(formatHeader(xhr.getAllResponseHeaders()));
            $("#resultContent").html(JSON.stringify(result).replace('<', '&lt;').replace('>', '&gt;'))
        },
        error: function (result) {
            if (result.statusText == "OK") {
                $("#runResult").show();
                $("#resultHeader").html(formatHeader(result.getAllResponseHeaders()));
                $("#resultContent").html(result.responseText.replace('<', '&lt;').replace('>', '&gt;'));
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