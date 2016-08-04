+function ($) {
    $(function () {
        var url = window.location.href;
        $("#navbar>ul>li a").each(function () {
            var href = $(this).attr('href');
            if (url.indexOf(href) != -1) {
                $(this).parent().addClass('active');
            }
        });
    });    
}(jQuery);