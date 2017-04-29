function getToken() {
    $.ajax({
        type: 'GET',
        url: '/api/gettoken',
        data: { un: "Aster", pwd: "123456" },
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
        url: '/api/getdata',
        data: { un: "Aster" },
        success: function (data, status, xhr) {
            $("#apiResult").val(data);
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