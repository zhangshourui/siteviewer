// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    //var visitedUrls = window.localStorage.getItem("visitedUrls");
    //if (visitedUrls) {
    //    visitedUrls = JSON.parse(visitedUrls);
    //}

    $("body").on("click", ".detail-link", function (e) {

        var fileId = $(this).data('fileid');
        // set cookie: fileId: true, exp: 24hr
        document.cookie = `fv-${fileId}=1; max-age=${24 * 60 * 60 * 7}; path=/`;

        $(this).addClass("visited");

        e.preventDefault();

        var url = $(this).attr("href");
        if (url) {
            // Open the link in a new tab
            window.open(url, '_self');
        } else {
            // If no URL is set, just log an error
            console.error("No URL found for detail link with fileId:", fileId);
        }


    })

    //load visited links from cookies
    var cookies = document.cookie.split(';');
    cookies.forEach(function (cookie) {
        var parts = cookie.trim().split('=');
        if (parts[0].startsWith('fv-')) {
            var fileId = parts[0].substring(3);
            $(`.detail-link[data-fileid="${fileId}"]`).addClass("visited");
        }
    });

});