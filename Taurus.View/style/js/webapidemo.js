function getToken() {
    $.ajax({
        type: 'GET',
        url: '/api/gettoken',
        data: { un: "13431148888", pwd: "中文密码啦" },
        success: function (data, status, xhr) {
            $("#token").val(data);
        },
        error: function (xhr, type) {
            alert(xhr);
        }
    });
}
function getData(method,withToken, methodType) {
    var data = [{ a: "Aster", b: "999666", list: {a:"chen",b:"yu"} }];//$.param(data.serializeObject("ab"))
    var json = {
        type: methodType,
        contentType:'application/text',
        url: '/api/'+method,
        data: "a=Aster&b=999666&unList=[{a:'chen',b:'yu'},{a:'qiang'}]",    //手动把数据转换拼接,
        success: function (data, status, xhr) {
            $("#apiResult").val(JSON.stringify(data));
        },
        error: function (xhr, type) {
            alert(xhr);
        }
    }
    if (withToken) {
        var token = $("#token").val();
        json.headers = { "token": token };
    }
    $.ajax(json);
}
Array.prototype.serializeObject = function (lName) {
    var o = {};
    $t = this;

    for (var i = 0; i < $t.length; i++) {
        for (var item in $t[i]) {
            o[lName + '[' + i + '].' + item.toString()] = $t[i][item].toString();
        }
    }
    return o;
};