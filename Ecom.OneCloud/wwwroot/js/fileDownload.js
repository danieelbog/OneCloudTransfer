var fakeDownload = $("#fake-download");
var reportSubmited = false;

$(document).ready(function () {

    var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    if (isMobile) {
        if (/iPhone|iPod|Android/i.test(navigator.userAgent)) {
            $('.card').css({ top: '40%', left: '50%', margin: '-' + ($('.card').height() / 2) + 'px 0 0 -' + ($('.card').width() / 2) + 'px' });
            $(".card").hide();
            $("#mobile-show-download-card-button").show();
        }
    }
})

$(".download-item-btn").on("click", async function (e) {

    let fileId = $(this).attr("data-fileId");
    let reportUrl = window.location.origin + `/Report/AddReport/${fileId}`;

    await $.ajax({
        type: 'POST',
        url: reportUrl,
    })
        .done(function (response) {

        })
        .fail(function () {
            reportSubmited = false;
            $("#download-card").hide("fast");
            $("#error-card").hide("fast");
        });
})

$("#mobile-show-download-card-button").on("click", function () {
    $("#mobile-show-download-card-button").hide("fast");
    $(".card").show("slow");
});

fakeDownload.on("click", function () {

    var downloadUrls = [];
    var fileIds = [];

    $('.download-item-btn').each(function (i, obj) {
        let urlDownload = $(this).attr("href");
        let fileId = $(this).attr("data-fileId");

        downloadUrls.push(urlDownload);
        fileIds.push(fileId);
    });

    download(downloadUrls, fileIds);
})

function download(files, fileIds) {

    let reportUrl = window.location.origin + `/Report/AddReports`;

    $.ajax({
        type: 'POST',
        url: reportUrl,
        data: JSON.stringify(fileIds),
        contentType: "application/json"
    })
        .done(function (response) {
        })
        .fail(function () {
            $("#download-card").hide("fast");
            $("#error-card").hide("fast");
        });

    function download_next(i) {
        if (i >= files.length) {
            return;
        }

        var a = document.createElement('a');
        a.href = files[i];
        a.target = '_parent';

        (document.body || document.documentElement).appendChild(a);
        if (a.click) {
            a.click();
        } else {
            $(a).click();
        }

        a.parentNode.removeChild(a);

        setTimeout(function () {
            download_next(i + 1);
        }, 800);
    }
    download_next(0);

}
