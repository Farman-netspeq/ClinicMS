// Toastr configuration
toastr.options = {
    positionClass: "toast-top-right",
    timeOut: 3000,
    closeButton: true
};

// Called by CustomAjax.js after every AJAX action
// Reads hidden field value → shows toast notification
function ShowMessageBox() {
    var msg = $("#ErrMsgHiddenField").val();
    if (!msg) return;
    var parts = msg.split("|");
    if (parts[0] === "success") toastr.success(parts[1]);
    else if (parts[0] === "error") toastr.error(parts[1]);
    else if (parts[0] === "warning") toastr.warning(parts[1]);
    $("#ErrMsgHiddenField").val("");
}