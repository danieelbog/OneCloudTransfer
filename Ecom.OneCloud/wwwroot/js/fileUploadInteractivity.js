var getLinkCheckbox = $("#getLinkCheckbox");
var sendEmailCheckbox = $("#sendEmailCheckbox");

var externalDropzone = $("#dropzone-external");
var actionDedector = $(".action-dedector");
var dragTimer;

var textarea = $('textarea');


$(document).ready(function () {

    //COPY THE URL FROM THE INPUT THAT WAS RESPONED
    $('button.copyButton').click(function () {
        $(this).siblings('input.linkToCopy').select();
        document.execCommand("copy");
    });

    //CHECK IF MOBILE AND MAKE CHANGES TO DOM HTML
    var isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    if (isMobile) {
        actionDedector.hide();

        if (/iPhone|iPod|Android/i.test(navigator.userAgent)) {
            $('.card').css({ top: '40%', left: '50%', margin: '-' + ($('.card').height() / 2) + 'px 0 0 -' + ($('.card').width() / 2) + 'px' });
            $(".card").hide();
            $("#mobile-show-upload-card-button").show();
        }
    }

    $("#mobile-show-upload-card-button").on("click", function()
    {
        $("#mobile-show-upload-card-button").hide("fast");
        $(".card").show("slow");
    });

    //CHANGE UPLOAD BUTTON TEXT => GET LINK || TRANSFER
    $("#getLinkCheckbox, #sendEmailCheckbox").click(function (e) {
        $('.collapse').collapse('hide');

        if (getLinkCheckbox.is(':checked')) {
            $("#fake-submit").html('Get link')
        }
        else {
            $("#fake-submit").html('Transfer')
        }
    });

    //SHOW EXTERNAL DROP ZONE AND MAKE ACTION DETECTORS INNACTIVE
    $(".action-dedector, .card").on('dragover', function (e) {
        externalDropzone.show();
        actionDedector.css("z-index", "0");
        window.clearTimeout(dragTimer);
    });

    //HIDE EXTERNAL DROP ZONE AND MAKE ACTION DETECTORS ACTIVE
    externalDropzone.on('dragleave', function (e) {
        dragTimer = window.setTimeout(function () {
            externalDropzone.hide();
            actionDedector.css("z-index", "99");
        }, 25);
    });

    //HIDE EXTERNAL DROP ZONE AND MAKE ACTION DETECTORS ACTIVE
    externalDropzone.on('drop', function (e) {
        dragTimer = window.setTimeout(function () {
            externalDropzone.hide();
            actionDedector.css("z-index", "99");
        }, 25);
    });

    //MAKE TEXTAREA AUTO EXPANDABLE ACCORDING TO INPUT LENGTH
    $("textarea").each(function () {
        this.setAttribute("style", "height:" + (this.scrollHeight) + "px;overflow-y:hidden;");
    }).on("input", function () {
        this.style.height = "auto";
        this.style.height = (this.scrollHeight + 4) + "px";
    });
});

