// site.js
toastr.options = {
    positionClass: "toast-top-right",
    timeOut: 3000,
    closeButton: true
};

function ShowMessageBox() {
    var code = $('#ErrMsgHiddenField').attr("data-msg-code");
    var msg = $('#ErrMsgHiddenField').val();
    if (!code || code === "") return;
    if (code === "success") toastr.success(msg);
    else if (code === "error") toastr.error(msg);
    else if (code === "warning") toastr.warning(msg);
    $('#ErrMsgHiddenField').attr("data-msg-code", "");
}

// Auto-trigger on ANY partial load into page
$(document).ready(function () {
    ShowMessageBox();
});

// Also trigger after every AJAX complete
// Catches partial view loads via CustomAjax
$(document).ajaxComplete(function () {
    ShowMessageBox();
});