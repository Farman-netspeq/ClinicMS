$(function () {
    //**************** JS to show loading progress during ajax call *********************//
    $(document).bind("ajaxStart.box-body", function () {
        $("#ajaxLoading").css("display", "block");
        $("#ajaxLoading").css("top", "50%");
        $("#ajaxLoading").css("left", "50%");
        $("#ajaxLoading").css("position", "fixed");
        $("#ajax-backdrop").css("display", "block");
    });
    $(document).bind("ajaxStop.box-body", function () {
        $("#ajaxLoading").css("display", "none");
        $("#ajax-backdrop").css("display", "none");
    })
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        if (jqxhr && jqxhr.status === 401) {
            alert('UnAuthorized Access or Session Expired. Reloading Page. Please try Again');
            //window.location.href = "/account/logoff";
            window.location.reload();

        }
        else {
            alert('Session Expired: You were idle too long. Reloading Page. Please try Again');
            window.location.reload();
        }
    });
    //$('.box-body').ajaxStart(function () {
    //    $("#ajaxLoading").css("display", "block");
    //    $("#ajaxLoading").css("top", $(window).height() / 1.8);
    //    $("#ajaxLoading").css("left", $(window).width() / 2);
    //    $("#ajaxLoading").css("position", "fixed");
    //    $("#ajax-backdrop").css("display", "block");
    //});
    //$('.box-body').ajaxStop(function () {
    //    $("#ajaxLoading").css("display", "none");
    //    $("#ajax-backdrop").css("display", "none");
    //});
    //************************************************************************************//

    //*************************  Highlight Active Menu  ***********************************//
    jQuery(function () {
        var url = window.location.pathname,
            urlRegExp = new RegExp(url.replace(/\/$/, '') + "$");
        var IsActivated = false;
        $("#stacked-menu ul li").each(function () {
            $(this).removeClass("has-active");
            try {
                if ($(this).children().attr("href").toLowerCase() == url.toLowerCase()) {
                    $(this).addClass("has-active");
                    $(this).closest('.menu-second-level').addClass("has-active");
                    IsActivated = true;
                }
            } catch (e) {

            }
        })
        if (IsActivated == false) {
            $("#stacked-menu ul li").each(function () {
                $(this).removeClass("active");
                try {

                    if ($(this).children().attr("href").toLowerCase() == $("#ActiveURL").data("value").toLowerCase()) {
                        $(this).addClass("has-active");
                        $(this).closest('.menu-second-level').addClass("has-active");
                        IsActivated = true;
                    }
                } catch (e) {

                }

            })
        }
    });
    //************************************************************************************//

    ShowMessageBox();
    //************************************************************************************//

    //******************************** JS for Grid paging *********************************//
    var getPage = function () {
        var $a = $(this);

        if ($a.attr("href").trim() == undefined || $a.attr("href").trim() == "") {
            return;
        }
        var options = {
            url: $a.attr("href")
            , data: $("#searchFrom").serialize()
            , type: "get"
        }

        $.ajax(options).done(function (data) {
            var $target = $($a.parents("div.ns-grid-pager").attr("data-sks-target"));
            $target.replaceWith(data);
        });
        return false;
    };
    $("body").on("click", "a.ns-page-link", getPage);
    $("body").on("change", "#searchFrom select, #searchFrom input[type='date']", function () {
        $(this).closest("#searchFrom").find("a.ns-page-link").first().trigger("click");
    });
    var getPageForDDL = function () {
        var TargetURL = $(this).parent().attr("data-sks-actionlink");
        if (TargetURL.indexOf("?") > -1) {
            TargetURL = TargetURL + "&PageSize=" + $('.page-size').val() + "&PageNo=" + $(this).val()
        }
        else {
            TargetURL = TargetURL + "?PageSize=" + $('.page-size').val() + "&PageNo=" + $(this).val()
        }
        //TargetURL = TargetURL + "?PageSize=" + $('.page-size').val()
        var options = {
            url: TargetURL
            , data: $("#searchFrom").serialize()
            , type: "get"
        }
        var target = $(this).parent().attr("data-sks-target");
        $.ajax(options).done(function (data) {
            $(target).replaceWith(data);
        });
    };
    $("body").on("change", ".page-number", getPageForDDL);

    var getPageSizeForDDL = function () {
        var TargetURL = $(this).parent().attr("data-sks-actionlink");
        //TargetURL = TargetURL + "?PageSize=" + $(this).val()
        if (TargetURL.indexOf("?") > -1) {
            TargetURL = TargetURL + "&PageSize=" + $('.page-size').val()
        }
        else {
            TargetURL = TargetURL + "?PageSize=" + $('.page-size').val()
        }
        var options = {
            url: TargetURL
            , data: $("#searchFrom").serialize()
            , type: "get"
        }
        var target = $(this).parent().attr("data-sks-target");
        $.ajax(options).done(function (data) {
            $(target).replaceWith(data);
        });
    };
    $("body").on("change", ".page-size", getPageSizeForDDL);

    //************************************************************************************//

    //************** Dropdown Changed event for Cascading Dropdown list ******************//

    var getDropDownList = function () {

        var targeturl = $(this).attr("dataotflink")
        var elem = $(this);
        if (elem.val() != '' && elem.val() != null) {
            targeturl = targeturl + "/" + elem.val();
            var target = $(this).attr("dataotftarget");
            var options = {
                url: targeturl
                , data: $("form").serialize()
                , type: "get"
                , dataType: 'json'
            }

            $.ajax(options).done(function (data) {
                $(target).empty();
                $(target).append('<option value="">--- Select ---</option>');
                $.each(data, function (i, item) {
                    $(target).append('<option value="' + item.Value + '">' + item.Text + '</option>');
                    // here we are adding option for employee dropdown
                });
                $("#select2-" + $(target).attr('id') + "-container").replaceWith('<span id="select2-' + $(target).attr('id') + '-container" class="select2-selection__rendered" title="--- Select ---">--- Select ---</span>')
            });

        }
        else {
            var target = $(elem.attr("dataotftarget"));
            target.empty();
            $("#select2-" + $(target).attr('id') + "-container").replaceWith('<span id="select2-' + $(target).attr('id') + '-container" class="select2-selection__rendered" title="--- Select ---">--- Select ---</span>')
            $(elem.attr("dataotftarget")).append('<option value="">--- Select ---</option>');
        }
        return false;
    };
    $("body").on("change", ".dropdown-change", getDropDownList);

    //************************************************************************************//

    //************** Dropdown Changed event for Cascading Html ******************//

    var getPartial = function () {

        var targeturl = $(this).attr("dataotflink")
        targeturl = targeturl + "&EmpCode=" + $(this).val();
        var target = $(this).attr("dataotftarget");
        var options = {
            url: targeturl
            , data: $("form").serialize()
            , type: "get"
        }
        $.ajax(options).done(function (data) {
            $(target).replaceWith(data);
        });
        return false;
    };
    $("body").on("change", ".partial-change", getPartial);

    //************************************************************************************//

    $('body').on("click", ".delete", ShowWarningMessageBox);
    var ajaxFormSubmit = function () {
        var $form = $(this);
        var options = {
            url: $form.attr("action")
            , type: $form.attr("method")
            , data: $form.serialize()
        }

        var target = $($form.attr("data-sks-target"));
        $.ajax(options).done(function (data) {
            $(target).replaceWith(data);
        }).fail(function (err) {
            alert(err);
        });
        return false;
    };
    $("form[data-sks-ajax='true']").submit(ajaxFormSubmit);
    var ajaxCustomFormSubmit = function () {

        var $form = $(this);
        var options = {
            url: $form.attr("action")
            , type: $form.attr("method")
            , data: $form.serialize()
        }

        $.ajax(options).done(function (data) {
            if (data.success) {
                $('#ErrMsgHiddenField').attr('data-msg-code', 'success');
                $('#ErrMsgHiddenField').val('Scheme info updated Successfully...');
            }
            else {
                $('#ErrMsgHiddenField').attr('data-msg-code', 'error');
                $('#ErrMsgHiddenField').val(data.errmsg);
            }
            ShowMessageBox();
        }).fail(function (err) {
            $('#ErrMsgHiddenField').attr('data-msg-code', 'error');
            $('#ErrMsgHiddenField').val(err);
            ShowMessageBox();
        });

        return false;
    };
    $("form[data-update-ajax='true']").submit(ajaxCustomFormSubmit);

});
// ************************* This is to update <textarea> by ckediotr content on ajax submit  ********************************//
function ajaxCKEditorUpdate() {

    $(".editor").each(function () {
        var name = $(this).attr('id');
        CKEDITOR.instances[name].updateElement();
    })
}
// *********************************************************************************************//
function RemoveErrorDiv() {
    $("#ErrMsg").remove();
}
// ************************* This is to show success/error message ********************************//

// Function to call custom Ajax form submit
function CustomAjaxFormSubmit(sender, url) {
    if (url == "#") { return false; }

    var $form = $('a[href="' + decodeURI(url) + '"]').closest('form')
    if ($form.attr("data-sks-ajax") == 'true') {
        var options = {
            url: decodeURI(url)
            , type: $form.attr("method")
            , data: $form.serialize()
        }
        var target = $($form.attr("data-sks-target"));
        $.ajax(options).done(function (data) {
            RemoveErrorDiv();
            $(target).empty().append(data);
            ShowMessageBox();
            RemoveErrorDiv();
        });
        return false;
    }
    else {
        return true;
    }
};

function ShowMessageBox() {

    if ($('#ErrMsgHiddenField').attr("data-msg-code") != undefined && $('#ErrMsgHiddenField').attr("data-msg-code") != "") {
        toastr.options.timeOut = 10000;
        toastr.options.extendedTimeOut = 1000; //1000;
        toastr.options.positionClass = 'toast-top-right';
        toastr.options.fadeOut = 250;
        toastr.options.fadeIn = 250;
        if ($('#ErrMsgHiddenField').attr("data-msg-code") == "success") {
            toastr.success($('#ErrMsgHiddenField').val());

        }
        else if ($('#ErrMsgHiddenField').attr("data-msg-code") == "info") {
            toastr.info($('#ErrMsgHiddenField').val());

        }
        else if ($('#ErrMsgHiddenField').attr("data-msg-code") == "warning") {
            toastr.warning($('#ErrMsgHiddenField').val());

        }
        else {
            toastr.error($('#ErrMsgHiddenField').val());
            toastr.options.extendedTimeOut = 5000;


        }

    }
}

//// This is to show warning message before delete operation
var ShowWarningMessageBox = function (e) {
    if ($(this).text() != "Cancel") {
        // Set the sender infromation in hidden field and its closest form
        $("#eventSender").val(($(this).attr('href')) + '|' + $($(this).closest('form')));

        $('#myErroModalLabel').text('Information');
        //$('#myErroMsgModalNoButton').val("Cancel");
        $('#Msg').text('Are you sure you want to delete the record..?');

        $('#myErroMsgModalYesButton').removeClass('d-none');
        $('#myErroMsgModal').modal('show');
        e.preventDefault();
    }
}




var CustomDeleteMessageBox = function (e) {
    if ($(this).text() != "Cancel") {
        // Set the sender infromation in hidden field and its closest form
        $("#eventSender").val(($(this).attr('href')) + '|' + $($(this).closest('form')));

        $('#myErroModalLabel').text('Information');
        $('#myErroMsgModalNoButton').val("Cancel");

        $('#Msg').replaceWith('<h4 id="Msg"><div><span class="" style="">You will be redirected to the Payment Page..</span></div><div>Are you sure you want to Proceed.?</div></h4>');

        $('#myErroMsgModalYesButton').removeClass('hidden');
        $('#myErroMsgModal').modal('show');
        e.preventDefault();
    }
}



var ShowWarningUnallocateMessageBox = function (e) {
    if ($(this).text() != "Cancel") {
        // Set the sender infromation in hidden field and its closest form
        $("#eventSender").val(($(this).attr('href')) + '|' + $($(this).closest('form')));

        $('#myErroModalLabel').text('Information');
        $('#myErroMsgModalNoButton').val("Cancel");
        $('#Msg').text('Are you sure you want to UnAllocate this Employee from the task..?');

        $('#myErroMsgModalYesButton').removeClass('hidden');
        $('#myErroMsgModal').modal('show');
        e.preventDefault();
    }
}


// Close message box
function CloseMyModal() {
    $('#myErroMsgModalYesButton').addClass('d-none');
    $('#ErrMsgHiddenField').val("");
    $('#myErroMsgModal').modal('toggle');
}
// close message box and procceed for intended action.
function OkMyModal() {

    $('#ErrMsgHiddenField').val("");
    $('#myErroMsgModal').modal('toggle');
    // Retrieve the sender infromation from hidden field and pass it to the function
    CustomAjaxFormSubmit($("#eventSender").val().split('|')[1], $("#eventSender").val().split('|')[0]);
}
var RemoveForwardMessageBox = function (e) {
    if ($(this).text() != "Cancel") {
        // Set the sender infromation in hidden field and its closest form
        $("#removeeventSender").val(($(this).attr('href')) + '|' + $($(this).closest('form')));

        $('#myRemoveForwardModalLabel').text('Confirmation');
        //$('#myErroMsgModalNoButton').val("Cancel");
        $('#removeMsg').text('Are you sure you want to remove the forwarded record..?');

        $('#myRemoveForwardModalYesButton').removeClass('hidden');
        $('#myRemoveForwardModal').modal('show');
        e.preventDefault();
    }
}
// Close message box
function CloseRemoveForwardModal() {
    $('#myRemoveForwardModalYesButton').addClass('hidden');
    $('#ErrMsgHiddenField').val("");
    $("#myRemoveForwardModal").modal('hide');
}
// close message box and procceed for intended action.
function OkRemoveForwardModal() {
    $("#myRemoveForwardModal").modal('hide');
    $('#ErrMsgHiddenField').val("");
    // Retrieve the sender infromation from hidden field and pass it to the function
    RemoveForwardedFormSubmit($("#removeeventSender").val().split('|')[1], $("#removeeventSender").val().split('|')[0]);
}
function RemoveForwardedFormSubmit(sender, url) {
    if (url == "#") { return false; }

    var $form = $('a[href="' + decodeURI(url) + '"]').closest('form')
    if ($form.attr("data-unforward-ajax") == 'true') {
        var options = {
            url: decodeURI(url)
            , type: $form.attr("method")
            , data: $form.serialize()
        }
        var target = $($form.attr("data-sks-target"));
        $.ajax(options).done(function (data) {
            if (data.success) {
                $form.closest(".forwardrow").remove();
                if ($('.forwardrow').length == 0) {
                    $('#hold-link').show();
                    $('#approve-link').attr('disabled', true);
                }
            }
            else {
                $('#ErrMsgHiddenField').val(data.errmsg);
                ShowMessageBox();
            }
        });
        return false;
    }
    else {
        return true;
    }
};
$(".container-fluid").on("click", ".removeformard", RemoveForwardMessageBox);
//**************************** Model Ajax *********************************
$(function () {
    $.ajaxSetup({ cache: false });
    $(".container-fluid").on("click", "a[data-modal]", function (e) {
        // hide dropdown if any (this is used wehen invoking modal from link in bootstrap dropdown )
        //$(e.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
        
        $('#AddEditModalContent').load(this.href, function () {
            $('#AddEditModal').modal('show');
            bindForm(this);
           
        });
        return false;
    });
});
$(function () {
    $.ajaxSetup({ cache: false });

    $(".container-fluid").on("click", "button[data-modal]", function (e) {
        // hide dropdown if any (this is used wehen invoking modal from link in bootstrap dropdown )
        //$(e.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');
       
        $('#AddEditModalContent').load(this.href, function () {
            $('#AddEditModal').modal('show');
            bindForm(this);
           
           
        });
        return false;
    });
});


function bindForm(dialog) {

    var $form = $('form', dialog);
    if ($.validator && $.validator.unobtrusive) {
        $form.removeData("validator").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse($form);
    }

    $('form', dialog).submit(function () {
        if (!$form.valid()) {
            return false;
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (result) {
                if (typeof result === 'object' && result !== null) {
                    // JSON response
                    if (result.success) {
                        $('#AddEditModal').modal('hide');
                        $('#replaceTarget').load(result.url);
                    } else {
                        toastr.error(result.message || 'Save failed.');
                    }
                } else {
                    // HTML response (re-rendered partial with validation errors)
                    $('#AddEditModalContent').html(result);
                    bindForm(dialog);
                }
            }
        });
        return false;
    });
}

