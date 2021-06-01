//#region URL AND SESSION GUID
const originUrl = window.location.origin;
const sessionToken = uuidv4();

let endpointUrl = originUrl + "/OneCloud";
let endpointFunctionUploadChunkAndSendEmail = "UploadChunkAndSendEmail";
let endpointFunctionUploadChunkGetLink = "UploadChunkAndGetDownloadLink"

let verificationUrl = originUrl + "/Verification";
//#endregion

//#region FORMS
let form = $("#uploadForm");
let emailVerificationForm = $("#emailVerificationForm");
//#endregion

//#region FORM INPUTS
let emailTo = $("#floatingEmailTo");
var emailFrom = $("#floatingEmailFrom");
let message = $("#floatingMessage");
let verificationNumber = $("#floatingVerificationEmail");
//#endregion

//#region SUBMIT BUTTONS
let fakeSubmit = $("#fake-submit");
let fakeVerification = $("#fake-verification");
//#endregion

let fileUploader = $("#file-uploader");

$(document).ready(function () {

    //#region DX UPLOADER
    $(function () {
        fileUploader.dxFileUploader({
            name: "myFile",
            uploadMode: "useButtons",
            uploadUrl: endpointUrl,
            chunkSize: 200000,
            dropZone: externalDropzone,
            onValueChanged: onValueChanged,
            onFilesUploaded: onFilesUploaded,
            onUploaded: onUploaded,
            onUploadError: onUploadError,
            onUploadAborted: onUploadAborted,
            multiple: true,
            labelText: "Add your files"
        });
    });

    $(function () {
        $(".dx-button-mode-contained").dxButton({
            icon: "plus",
            text: ""
        });
    });

    //#endregion

    //#region FORM VALIDATIONS
    form.validate({
        errorClass: "is-invalid",
        validClass: "",
        rules: {
            emailTo: {
                required: $("#sendEmailCheckbox").is(':checked'),
                email: true,
            },
            emailFrom: {
                required: $("#sendEmailCheckbox").is(':checked'),
                email: true,
            }
        },
        errorPlacement: function (error, element) {
            element.siblings(".invalid-feedback").text(error.text());
        }
    });

    emailVerificationForm.validate({
        errorClass: "is-invalid",
        validClass: "",
        errorPlacement: function (error, element) {
            element.siblings(".invalid-feedback").text(error.text());
        }
    });
    //#endregion
})

//#region FORM INPUT VALIDATIONS
verificationNumber.on('blur change click keyup paste', function () {
    if (emailVerificationForm.valid()) {
        fakeVerification.removeClass("disabled");
    } else {
        fakeVerification.addClass('disabled');
    }
});

function onValueChanged(e) {
    e.element.find(".dx-fileuploader-upload-button").hide();

    if (e.value.length > 0 && !$("#files-verification-input-text").hasClass("d-none")) {
        $("#files-verification-input").addClass("d-none");
    }
}
//#endregion

//#region UPLOAD OR GET EMAIL VERIFICATION
fakeSubmit.on("click", function () {

    let element = document.getElementById("file-uploader");
    let instance = DevExpress.ui.dxFileUploader.getInstance(element)

    if (instance._files.length == 0 || $("#sendEmailCheckbox").is(':checked') && !form.valid()) {
        if (instance._files.length == 0) {
            $("#files-verification-input").removeClass("d-none");
        }
        return;
    }

    let url = instance.option("uploadUrl");

    if ($("#sendEmailCheckbox").is(':checked')) {

        $("#card-loader").show("fast");
        url = `${endpointUrl}/${endpointFunctionUploadChunkAndSendEmail}/${emailTo.val()}/${emailFrom.val()}/${sessionToken}/${message.val()}`;
        instance.option("uploadUrl", url);

        $.ajax({
            type: 'POST',
            url: `${verificationUrl}/GetEmailVerificationNumber/${sessionToken}/${emailFrom.val()}`,
        })
            .done(function (response) {
                $("#card-loader").hide("slow");
                $("#upload-card").hide("slow");
                $("#email-verification-card").show("slow");
            })
            .fail(function () {
                $("#card-loader").hide("slow");
                $("#upload-card").hide("slow");
                $("#error-card").show("slow");
            });
    }
    else {
        url = `${endpointUrl}/${endpointFunctionUploadChunkGetLink}/${sessionToken}`;

        instance.option("uploadUrl", url);
        instance.upload();
    }
})
//#endregion

//#region EMAIL VERIFICATION
fakeVerification.on("click", function () {

    let url = `${verificationUrl}/VerifyEmail/${sessionToken}/${emailFrom.val()}/${verificationNumber.val()}`;

    $.ajax({
        type: 'POST',
        url: url,
    })
        .done(function (response) {

            $("#email-verification-card").hide("slow");
            $("#upload-card").show("slow");
            fakeSubmit.addClass("disabled");
            $(".additionalSettingsToggle").addClass("d-none");

            let element = document.getElementById("file-uploader");
            let instance = DevExpress.ui.dxFileUploader.getInstance(element)

            instance.upload();
        })
        .fail(function () {
            console.log($("#verification-response-text"))

            $("#verification-response-text").text("Verification failed, please try again");
            $("#verification-response-text").removeClass("text-muted");
            $("#verification-response-text").addClass("text-danger");

            setTimeout(function () {
                $("#verification-response-text").text("Enter the recieved code and press Verify");
                $("#verification-response-text").removeClass("text-danger");
                $("#verification-response-text").addClass("text-muted");
            }, 4000)
        });
})

$("#resend-email-verification").on("click", function (e) {
    e.preventDefault();

    if ($("#resend-email-verification").attr("href") === undefined)
        return;

    $("#floatingVerificationEmail").val("");

    let url = `${verificationUrl}/GetEmailVerificationNumber/${sessionToken}/${emailFrom.val()}`;

    $.ajax({
        type: 'POST',
        url: url,
    })
        .done(function (response) {

            $("#resend-email-verification").addClass("disabled");
            $("#resend-email-text").text("Email Sent");
            $("#resend-email-text").addClass("text-success");

            setTimeout(function () {

                $("#resend-email-verification").removeClass("disabled");
                $("#resend-email-text").text("Resend Email");
                $("#resend-email-text").removeClass("text-success");

            }, 3000)
        })
        .fail(function () {

            $("#resend-email-verification").removeAttr("href");
            $("#resend-email-text").text("Error sending Email");
            $("#resend-email-text").addClass("text-danger");

            setTimeout(function () {

                $("#resend-email-verification").attr("href", "");
                $("#resend-email-text").text("Resend Email");
                $("#resend-email-text").removeClass("text-danger");

            }, 4000)
        });
})

$("#back-to-upload").on("click", function () {

    $("#floatingVerificationEmail").val("");
    $("#email-verification-card").hide("slow");
    $("#upload-card").show("slow");
})

//#endregion

//#region FILES UPLOADED
function onUploaded(e) {
    if (e.request.status != 200) {

        let element = document.getElementById("file-uploader");
        let instance = DevExpress.ui.dxFileUploader.getInstance(element)

        instance.reset();

        $("#upload-card").hide("slow");
        $("#error-card").show("slow");
    }
}

function onFilesUploaded(e) {
    var url = `${endpointUrl}/GetDownloadLink/${sessionToken}`
    $.ajax({
        type: 'GET',
        url: url,
    })
        .done(function (response) {
            $("#upload-card").hide("slow");
            if (response.uploadType == "GetLink") {
                $("#getLink-uploaded-card").show("slow");
                $(".linkToCopy").val(response.downloadUrl)
            }
            else {
                $("#sendEmail-uploaded-card").show("slow");
            }
        })
        .fail(function () {
            $("#upload-card").hide("slow");
            $("#error-card").show("slow");
        });
}
//#endregion

//#region ERRORS
function onUploadError(e) {
    $("#upload-card").hide("slow");
    $("#error-card").show("slow");
}

function onUploadAborted(e) {
    window.location.reload();
}

//#endregion

//#region GENERAL
function uuidv4() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        var r = (Math.random() * 16) | 0,
            v = c == "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

//#endregion



