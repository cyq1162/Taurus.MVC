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
function getData(withToken, methodType) {
    var json = {
        type: methodType,
       // contentType:'application/json',
        url: '/api/getdata',
        data: { un: "Aster" },
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